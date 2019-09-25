//using Chauffeur.Host;
//using NPoco;
//using System;
//using System.Collections.Generic;
//using System.Data.Common;
//using System.IO;
//using System.IO.Abstractions;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using Umbraco.Core.Logging;
//using Umbraco.Core.Migrations;
//using Umbraco.Core.Migrations.Expressions.Create;
//using Umbraco.Core.Migrations.Install;
//using Umbraco.Core.Persistence;
//using Umbraco.Core.Persistence.DatabaseAnnotations;
//using Umbraco.Core.Persistence.SqlSyntax;
//using Umbraco.Core.Scoping;

//namespace Chauffeur.Deliverables
//{
//    [DeliverableName("delivery")]
//    [DeliverableAlias("d")]
//    public sealed class DeliveryDeliverable : Deliverable
//    {
//        static readonly Regex tokenRegex = new Regex(@"\$(\w+)\$", RegexOptions.Compiled);
//        static readonly Func<IDictionary<string, string>, string, string> replaceTokens =
//            (@params, input) => tokenRegex.Replace(input, match => @params[match.Groups[1].Value]);

//        private readonly IChauffeurSettings settings;
//        private readonly IFileSystem fileSystem;
//        private readonly IChauffeurHost host;
//        private readonly IScopeProvider scopeProvider;
//        private readonly ILogger logger;
//        public const string TableName = "Chauffeur_Delivery";

//        public DeliveryDeliverable(
//            TextReader reader,
//            TextWriter writer,
//            IChauffeurSettings settings,
//            IFileSystem fileSystem,
//            IChauffeurHost host,
//            IScopeProvider scopeProvider,
//            ILogger logger
//        )
//            : base(reader, writer)
//        {
//            this.settings = settings;
//            this.fileSystem = fileSystem;
//            this.host = host;
//            this.scopeProvider = scopeProvider;
//            this.logger = logger;
//        }

//        public override async Task<DeliverableResponse> Run(string command, string[] args)
//        {
//            var dbNotReady = false;
//            using (var scope = scopeProvider.CreateScope())
//            {

//                try
//                {
//                    var dbSchemaHelper = new DatabaseSchemaCreator(scope.Database, logger);
//                    if (!dbSchemaHelper.TableExists(TableName))
//                    {
//                        if (!await SetupDatabase(scope))
//                            return DeliverableResponse.FinishedWithError;
//                    }
//                }
//                catch (DbException)
//                {
//                    Out.WriteLine("There was an error checking for the database Chauffeur Delivery tracking table, most likely your connection string is invalid or your database doesn't exist.");
//                    Out.WriteLine("Chauffeur will attempt to run the first delivery, expecting it to call `install`.");
//                    dbNotReady = true;
//                }

//                if (!settings.TryGetChauffeurDirectory(out string chauffeurDirectory))
//                {
//                    await Out.WriteLineAsync("Error accessing the Chauffeur directory. Check your file system permissions");
//                    return DeliverableResponse.Continue;
//                }

//                var allDeliveries = fileSystem.Directory
//                    .GetFiles(chauffeurDirectory, "*.delivery", SearchOption.TopDirectoryOnly)
//                    .ToArray();

//                if (!allDeliveries.Any())
//                {
//                    await Out.WriteLineAsync("No deliveries found.");
//                    return DeliverableResponse.Continue;
//                }

//                var @params = ParseParameterTokens(args, chauffeurDirectory);

//                var stopAtDeliverableParam = args.FirstOrDefault(arg => arg.StartsWith("-s:"));

//                if (stopAtDeliverableParam != null)
//                {
//                    var stopDeliverableName = stopAtDeliverableParam.Replace("-s:", string.Empty);
//                    allDeliveries = allDeliveries.TakeWhile(name => fileSystem.Path.GetFileName(name) != stopDeliverableName).ToArray();
//                }

//                if (dbNotReady)
//                {
//                    try
//                    {
//                        var delivery = allDeliveries.First();
//                        var file = fileSystem.FileInfo.FromFileName(delivery);

//                        var tracking = await Deliver(file, @params);

//                        if (!tracking.SignedFor)
//                            return DeliverableResponse.FinishedWithError;

//                        if (!await SetupDatabase(scope))
//                            return DeliverableResponse.FinishedWithError;

//                        scope.Database.Save(tracking);

//                        allDeliveries = allDeliveries.Skip(1).ToArray();
//                    }
//                    catch (DbException)
//                    {
//                        Out.WriteLine("Ok, I tried. Chauffeur had a database error, either a missing connection string or the DB couldn't be setup.");
//                        return DeliverableResponse.FinishedWithError;
//                    }
//                }

//                await ProcessDeliveries(scope, allDeliveries, @params);

//                return DeliverableResponse.Continue;
//            }
//        }

//        private Dictionary<string, string> ParseParameterTokens(string[] args, string chauffeurDirectory)
//        {
//            var @params = args.Where(arg => arg.StartsWith("-p:"))
//                            .Select(arg => arg.Replace("-p:", string.Empty))
//                            .Select(arg => arg.Split('='))
//                            .ToDictionary(arg => arg[0], arg => arg[1]);

//            @params.Add("ChauffeurPath", chauffeurDirectory);

//            if (settings.TryGetSiteRootDirectory(out string siteRootDirectory))
//                @params.Add("WebsiteRootPath", siteRootDirectory);

//            if (settings.TryGetUmbracoDirectory(out string umbracoDirectory))
//                @params.Add("UmbracoPath", umbracoDirectory);

//            @params.Add("UmbracoVersion", settings.UmbracoVersion);

//            return @params;
//        }

//        private async Task ProcessDeliveries(IScope scope, string[] allDeliveries, IDictionary<string, string> @params)
//        {
//            foreach (var delivery in allDeliveries)
//            {
//                var file = fileSystem.FileInfo.FromFileName(delivery);

//                var entry = await scope.Database.FirstAsync<ChauffeurDeliveryTable>("WHERE [Name] = @Name", new { file.Name });

//                if (entry != null && entry.SignedFor)
//                {
//                    await Out.WriteLineAsync($"'{file.Name}' is already signed for, skipping it.");
//                    continue;
//                }

//                var tracking = await Deliver(file, @params);
//                scope.Database.Save(tracking);
//                if (!tracking.SignedFor)
//                    break;
//            }
//        }

//        private async Task<ChauffeurDeliveryTable> Deliver(IFileInfo file, IDictionary<string, string> @params)
//        {
//            var instructions = fileSystem.File
//                .ReadAllLines(file.FullName)
//                .Where(x => !string.IsNullOrEmpty(x))
//                .Where(x => !x.StartsWith("##"));

//            var tracking = new ChauffeurDeliveryTable
//            {
//                Name = file.Name,
//                ExecutionDate = DateTime.Now,
//                Hash = HashDelivery(file),
//                SignedFor = true
//            };

//            if (!await AreAllParametersSpecified(instructions, @params))
//            {
//                tracking.SignedFor = false;
//                return tracking;
//            }

//            foreach (var instruction in instructions)
//            {
//                var result = await host.RunWithArgs(new[] { replaceTokens(@params, instruction) });

//                if (result != DeliverableResponse.Continue)
//                {
//                    tracking.SignedFor = false;
//                    break;
//                }
//            }
//            return tracking;
//        }

//        private async Task<bool> SetupDatabase(IScope scope)
//        {
//            await Out.WriteLineAsync("Chauffeur Delivery does not have its database setup. Setting up now.");

//            try
//            {
//                var mctx = new ChauffeurMockMigrationContext(scope.Database, logger);
//                var builder = new CreateBuilder(mctx);

//                builder.Table<ChauffeurDeliveryTable>().Do();
//            }
//            catch (Exception ex)
//            {
//                Out.WriteLine("Error creating the Chauffeur Delivery tracking table.");
//                Out.WriteLine(ex.ToString());
//                return false;
//            }

//            await Out.WriteLineAsync("Successfully created database table.");
//            return true;
//        }

//        private async Task<bool> AreAllParametersSpecified(
//            IEnumerable<string> instructions,
//            IDictionary<string, string> @params)
//        {
//            var parameters = instructions
//                .SelectMany(x => GetInstructionParametersNames(x))
//                .Distinct()
//                .OrderBy(x => x)
//                .ToList();

//            var missingParameters = parameters.Except(@params.Keys).OrderBy(x => x).ToList();
//            if (missingParameters.Count != 0)
//            {
//                await Out.WriteLineAsync($"The following parameters have not been specified:");
//                foreach (var missingParameter in missingParameters)
//                {
//                    await Out.WriteLineAsync($" - {missingParameter}");
//                }

//                return false;
//            }

//            return true;

//            IEnumerable<string> GetInstructionParametersNames(string instruction)
//            {
//                return tokenRegex
//                    .Matches(instruction)
//                    .Cast<Match>()
//                    .Select(x => x.Groups[1].Value);
//            }
//        }

//        private static string HashDelivery(IFileInfo file)
//        {
//            using (var fs = file.OpenRead())
//            using (var bs = new BufferedStream(fs))
//            {
//                using (var sha1 = new SHA1Managed())
//                {
//                    var hash = sha1.ComputeHash(bs);
//                    var formatted = new StringBuilder(2 * hash.Length);
//                    foreach (var b in hash)
//                    {
//                        formatted.AppendFormat("{0:X2}", b);
//                    }
//                    return formatted.ToString();
//                }
//            }
//        }
//    }

//    [TableName(DeliveryDeliverable.TableName)]
//    [PrimaryKey("Id")]
//    public class ChauffeurDeliveryTable
//    {
//        [Column("Id")]
//        [PrimaryKeyColumn(Name = "PK_id", IdentitySeed = 1)]
//        public int Id { get; set; }

//        [Column("Name")]
//        public string Name { get; set; }

//        [Column("ExecutionDate")]
//        public DateTime ExecutionDate { get; set; }

//        [Column("SignedFor")]
//        public bool SignedFor { get; set; }

//        [Column("Hash")]
//        public string Hash { get; set; }
//    }

//    class ChauffeurMockMigrationContext : IMigrationContext
//    {
//        public ChauffeurMockMigrationContext(IUmbracoDatabase database, ILogger logger)
//        {
//            Database = database ?? throw new ArgumentNullException(nameof(database));
//            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        public ILogger Logger { get; }

//        public IUmbracoDatabase Database { get; }

//        public ISqlContext SqlContext => Database.SqlContext;

//        public int Index { get; set; }

//        public bool BuildingExpression { get; set; }

//        public List<Type> PostMigrations { get; } = new List<Type>();

//        public void AddPostMigration<TMigration>()
//            where TMigration : IMigration
//        {
//            PostMigrations.Add(typeof(TMigration));
//        }
//    }
//}
