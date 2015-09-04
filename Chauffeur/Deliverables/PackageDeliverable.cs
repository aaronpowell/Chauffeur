using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Chauffeur.Host;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Task = System.Threading.Tasks.Task;

namespace Chauffeur.Deliverables
{
    [DeliverableName("package")]
    [DeliverableAlias("p")]
    [DeliverableAlias("pkg")]
    public sealed class PackageDeliverable : Deliverable
    {
        private readonly IFileSystem fileSystem;
        private readonly IChauffeurSettings settings;
        private readonly IPackagingService packagingService;
        private readonly IContentTypeService contentTypeService;

        public PackageDeliverable(
            TextReader reader,
            TextWriter writer,
            IFileSystem fileSystem,
            IChauffeurSettings settings,
            IPackagingService packagingService,
            IContentTypeService contentTypeService)
            : base(reader, writer)
        {
            this.fileSystem = fileSystem;
            this.settings = settings;
            this.packagingService = packagingService;
            this.contentTypeService = contentTypeService;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            if (!args.Any())
            {
                await Out.WriteLineAsync("No packages were provided, use `help package` to see usage");
                return DeliverableResponse.Continue;
            }

            string chauffeurFolder;
            if (!settings.TryGetChauffeurDirectory(out chauffeurFolder))
                return DeliverableResponse.Continue;

            var tasks = args.Select(arg => Unpack(arg, chauffeurFolder));
            await Task.WhenAll(tasks);

            return DeliverableResponse.Continue;
        }

        private async Task Unpack(string name, string chauffeurFolder)
        {
            var fileLocation = fileSystem.Path.Combine(chauffeurFolder, name + ".xml");
            if (!fileSystem.File.Exists(fileLocation))
            {
                await Out.WriteLineFormattedAsync("The package '{0}' is not found in the Chauffeur folder", name);
                return;
            }

            using (var stream = fileSystem.File.OpenRead(fileLocation))
            {
                var xml = XDocument.Load(stream);

                var info = xml.Root.Element("info");

                if (info != null)
                    await PrintInfo(info);

                var element = xml.Root.Element("DataTypes");
                if (element != null)
                    await UnpackDataTypes(element.Elements("DataType"));

                element = xml.Root.Element("Templates");
                if (element != null)
                    await UnpackTemplates(element.Elements("Template"));

                element = xml.Root.Element("Macros");
                if (element != null)
                    await UnpackMacros(element.Elements("macro"));

                element = xml.Root.Element("DocumentTypes");
                if (element != null)
                {
                    var docTypes = element.Elements("DocumentType");
                    var importedDocTypes = await UnpackDocumentTypes(docTypes);
                    await UpdateDocumentTypesStructure(docTypes, importedDocTypes);
                }
                else if (xml.Root.Name == "DocumentType")
                {
                    var importedDocTypes = await UnpackDocumentTypes(new[] { xml.Root });
                    await UpdateDocumentTypesStructure(new[] { xml.Root }, importedDocTypes);
                }
            }
        }

        private async Task UpdateDocumentTypesStructure(IEnumerable<XElement> docTypes, IEnumerable<IContentType> importedDocumentTypes)
        {
            var allDocumentTypes = contentTypeService.GetAllContentTypes();

            foreach (var docType in docTypes)
            {
                var allowedChildren = docType.Element("Structure").Elements("DocumentType");
                if (!allowedChildren.Any())
                    continue;

                var current = importedDocumentTypes.First(x => x.Alias == docType.Element("Info").Element("Alias").Value);
                var currentAllowed = current.AllowedContentTypes.ToList();
                foreach (var allowedChild in allowedChildren)
                {
                    var dt = allDocumentTypes.FirstOrDefault(x => x.Alias == (string)allowedChild);
                    if (dt != null && !currentAllowed.Any(x => x.Alias == dt.Alias))
                    {
                        await Out.WriteLineFormattedAsync("Adding '{0}' as a child of '{1}'", dt.Alias, current.Alias);
                        currentAllowed.Add(new ContentTypeSort(new Lazy<int>(() => dt.Id), currentAllowed.Count + 1, (string)allowedChild));
                    }
                }
                current.AllowedContentTypes = currentAllowed;
                contentTypeService.Save(current);
            }
        }

        private async Task PrintInfo(XElement info)
        {
            var pkg = info.Element("package");

            if (pkg != null)
                return;

            var name = (string)pkg.Element("name");
            var version = (string)pkg.Element("version");

            await Out.WriteLineFormattedAsync("Installing package {0} v{1}", name, version);
        }

        private async Task UnpackDataTypes(IEnumerable<XElement> elements)
        {
            foreach (var element in elements)
            {
                var name = (string)element.Attribute("Name");
                await Out.WriteLineFormattedAsync("Importing DataType '{0}'", name);
                packagingService.ImportDataTypeDefinitions(new XElement("DataTypes", element));
            }
        }

        private async Task UnpackTemplates(IEnumerable<XElement> elements)
        {
            foreach (var element in elements)
            {
                var name = (string)element.Element("Name");
                await Out.WriteLineFormattedAsync("Importing Template '{0}'", name);
                packagingService.ImportTemplates(element);
            }
        }

        private async Task UnpackMacros(IEnumerable<XElement> elements)
        {
            foreach (var element in elements)
            {
                var name = (string)element.Element("name");
                await Out.WriteLineFormattedAsync("Importing Macro '{0}'", name);
                packagingService.ImportMacros(element);
            }
        }

        private async Task<IEnumerable<IContentType>> UnpackDocumentTypes(IEnumerable<XElement> elements)
        {
            var docTypes = new List<IContentType>();
            foreach (var element in elements)
            {
                var name = (string)element.Element("Info").Element("Name");
                await Out.WriteLineFormattedAsync("Importing DocumentType '{0}'", name);
                docTypes.AddRange(packagingService.ImportContentTypes(element));
            }

            return docTypes;
        }
    }
}
