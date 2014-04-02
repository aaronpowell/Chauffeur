using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Chauffeur.Deliverables;
using Chauffeur.DependencyBuilders;
using Umbraco.Core;

namespace Chauffeur.Host
{
    public sealed class UmbracoHost
    {
        private readonly TextReader reader;
        private readonly TextWriter writer;

        private IEnumerable<Type> deliverableTypes;

        public static UmbracoHost Current { get; set; }
        internal ShittyIoC Container { get; private set; }

        public UmbracoHost(TextReader reader, TextWriter writer)
        {
            this.reader = reader;
            this.writer = writer;

            Container = new ShittyIoC();
            Container.Register<TextReader>(() => reader);
            Container.Register<TextWriter>(() => writer);
            Container.RegisterFrom<RepositoryFactoryBuilder>();
            Container.RegisterFrom<MediaServiceBuilder>();
            Container.RegisterFrom<ContentTypeServiceBuilder>();
            Container.RegisterFrom<ContentServiceBuilder>();
            Container.RegisterFrom<SqlSyntaxProviderBuilder>();
            Container.RegisterFrom<DatabaseUnitOfWorkProviderBuilder>();
            Container.RegisterFrom<DataTypeServiceBuilder>();
            Container.RegisterFrom<PackagingServiceBuilder>();
            Container.RegisterFrom<DatabaseBuilder>();
            Container.RegisterFrom<ChauffeurSettingBuilder>();
            Container.RegisterFrom<FileSystemBuilder>();

            Container.RegisterFrom<MappingResolversBuilder>();
            Container.RegisterFrom<ApplicationContextBuilder>();
        }

        public async Task Run()
        {
            await writer.WriteLineAsync("Welcome to Chauffeur, your Umbraco console.");
            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            await writer.WriteLineFormattedAsync("You're running Chauffeur v{0} against Umbraco '{1}'", fvi.FileVersion, ConfigurationManager.AppSettings["umbracoConfigurationStatus"]);
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("Type `help` to list the commands and `help <command>` for help for a specific command.");
            await writer.WriteLineAsync();

            var result = DeliverableResponse.Continue;

            while (result == DeliverableResponse.Continue)
            {
                var command = await Prompt();

                result = await Process(command);
            }
        }

        public async Task Run(string[] args)
        {
            await Process(string.Join(" ", args));
        }

        private async Task<DeliverableResponse> Process(string command)
        {
            if (string.IsNullOrEmpty(command))
                return DeliverableResponse.Continue;

            var args = command.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            var what = args[0].ToLower();
            args = args.Skip(1).ToArray();

            try
            {
                var deliverable = Container.ResolveDeliverableByName(what);
                return await deliverable.Run(what, args);
            }
            catch (Exception ex)
            {
                writer.WriteLine("Error running the current deliverable: " + ex.Message);
                return DeliverableResponse.Continue;
            }
        }

        private async Task<string> Prompt()
        {
            await writer.WriteAsync("umbraco> ");
            return await reader.ReadLineAsync();
        }
    }
}
