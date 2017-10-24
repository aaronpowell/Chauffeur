using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Chauffeur.Host;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using Task = System.Threading.Tasks.Task;

namespace Chauffeur.Deliverables
{
    [DeliverableName("content-type")]
    [DeliverableAlias("ct")]
    public sealed class ContentTypeDeliverable : Deliverable, IProvideDirections
    {
        private readonly IContentTypeService contentTypeService;
        private readonly Database database;
        private readonly IPackagingService packagingService;
        private readonly IFileSystem fileSystem;
        private readonly IChauffeurSettings settings;

        public ContentTypeDeliverable(
            TextReader reader,
            TextWriter writer,
            IContentTypeService contentTypeService,
            Database database,
            IPackagingService packagingService,
            IFileSystem fileSystem,
            IChauffeurSettings settings
            )
            : base(reader, writer)
        {
            this.contentTypeService = contentTypeService;
            this.database = database;
            this.packagingService = packagingService;
            this.fileSystem = fileSystem;
            this.settings = settings;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            var operation = args[0];

            switch (operation.ToLower())
            {
                case "get-all":
                    await GetAll();
                    break;

                case "get":
                    await Get(args.Skip(1).ToArray());
                    break;

                case "export":
                    await Export(args.Skip(1).ToArray());
                    break;

                case "import":
                    await Import(args.Skip(1).ToArray());
                    break;

                case "remove":
                    await Remove(args.Skip(1).ToArray());
                    break;

                default:
                    await Out.WriteLineFormattedAsync("The operation `{0}` is not supported", operation);
                    break;
            }

            return await base.Run(command, args);
        }

        private async Task Remove(string[] aliases)
        {
            var contentType = await Get(aliases, false);
            if (contentType == null)
                return;

            contentTypeService.Delete(contentType);
            await Out.WriteLineAsync($"Removed the content type '{aliases[0]}'");
        }

        private async Task Import(string[] args)
        {
            if (!args.Any())
            {
                await Out.WriteLineAsync("No import target defined");
                return;
            }

            var deliveryName = args[0].Trim();

            string directory;
            if (!settings.TryGetChauffeurDirectory(out directory))
                return;

            var file = fileSystem.Path.Combine(directory, deliveryName + ".xml");
            if (!fileSystem.File.Exists(file))
            {
                await Out.WriteLineFormattedAsync("Unable to located the import script '{0}'", deliveryName);
                return;
            }

            var xml = XDocument.Load(file);

            packagingService.ImportContentTypes(xml.Elements().First());

            await Out.WriteLineFormattedAsync("Content Type has been imported");
        }

        private async Task Export(string[] args)
        {
            var contentType = await Get(args, false);

            if (contentType == null)
                return;

            string exportDirectory;
            if (!settings.TryGetChauffeurDirectory(out exportDirectory))
                return;

            var xml = new XDocument();

            xml.Add(packagingService.Export(contentType, false));

            var fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "-" + contentType.Alias + ".xml";
            xml.Save(fileSystem.Path.Combine(exportDirectory, fileName));
            await Out.WriteLineFormattedAsync("Content Type has been exported with file name '{0}'", fileName);
        }

        private async Task<IContentType> Get(string[] args, bool dump = true)
        {
            if (!args.Any())
            {
                await Out.WriteLineAsync("Please provide the numerical id or alias if the doc type to get");
                return null;
            }

            IContentType contentType = null;
            var foundWith = "id";
            int id;
            if (int.TryParse(args[0], out id))
                contentType = contentTypeService.GetContentType(id);
            else
            {
                contentType = contentTypeService.GetContentType(args[0]);
                foundWith = "alias";
            }

            if (contentType == null)
            {
                await Out.WriteLineFormattedAsync("No content type found with {0} of '{1}'", foundWith, args[0]);
                return null;
            }

            // I'm sorry about the following code. In Umbraco 7.0.0 PropertyType.PropertyGroupId is:
            //  a) Not set when you do GetContentType
            //  b) An internal property (!!)
            // So to ensure that the object is proprely constructed I have to use reflection and SQL
            // and set it up myself!
            var propertyTypes = contentType.PropertyTypes.ToArray();
            foreach (var propertyType in propertyTypes)
            {
                var pi = propertyType.GetType().GetProperty("PropertyGroupId", BindingFlags.Instance | BindingFlags.NonPublic);
                var sql = new Sql()
                    .Select("PropertyTypeGroupId")
                    .From("CmsPropertyType")
                    .Where("Id = @0", new[] { propertyType.Id });

                var groupId = database.Fetch<int?>(sql).FirstOrDefault();
                if (groupId != null && groupId.HasValue)
                    pi.SetValue(propertyType, new Lazy<int>(() => groupId.Value));
                else
                    pi.SetValue(propertyType, new Lazy<int>(() => -1));
            }

            if (dump)
                await PrintContentType(contentType);
            return contentType;
        }

        private async Task PrintContentType(IContentType contentType)
        {
            await Out.WriteLineAsync("\tId\tAlias\tName\tParent Id");
            await Out.WriteLineFormattedAsync(
                  "\t{0}\t{1}\t{2}\t{3}",
                contentType.Id,
                contentType.Alias,
                contentType.Name,
                contentType.ParentId
            );

            await Out.WriteLineAsync("\tProperty Types");
            await Out.WriteLineAsync("\tId\tName\tAlias\tMandatory\tProperty Editor Alias");
            foreach (var propertyType in contentType.PropertyTypes)
            {
                await Out.WriteLineFormattedAsync(
                    "\t{0}\t{1}\t{2}\t{3}\t{4}",
                    propertyType.Id,
                    propertyType.Name,
                    propertyType.Alias,
                    propertyType.Mandatory,
                    propertyType.PropertyEditorAlias
                );
            }
        }

        private async Task GetAll()
        {
            var types = contentTypeService.GetAllContentTypes();

            if (!types.Any())
            {
                await Out.WriteLineAsync("No content types found.");
                return;
            }

            await Out.WriteLineAsync("\tId\tAlias\tName");
            foreach (var type in types)
                await Out.WriteLineFormattedAsync("\t{0}\t{1}\t{2}", type.Id, type.Alias, type.Name);
        }

        public async Task Directions()
        {
            await Out.WriteLineAsync("content-type");
            await Out.WriteLineAsync("\tPerform operations against Umbraco Content Types");
            await Out.WriteLineAsync("Available Operations:");

            await Out.WriteLineAsync("\tget-all");
            await Out.WriteLineAsync("\t\tDisplay a list of all Content Types");

            await Out.WriteLineAsync("\tget <id or alias>");
            await Out.WriteLineAsync("\t\tDisplays the properties of the specified Content Type");

            await Out.WriteLineAsync("\texport <id or alias>");
            await Out.WriteLineAsync("\t\tExports the specified Content Type to the Chauffeur folder");

            await Out.WriteLineAsync("\timport <filename>");
            await Out.WriteLineAsync("\t\tImport a Content Type from the specified file in the Chauffeur folder");
        }
    }
}
