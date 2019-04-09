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
using Umbraco.Core.Services;
using Task = System.Threading.Tasks.Task;

namespace Chauffeur.Deliverables
{
    [DeliverableName("package")]
    [DeliverableAlias("p")]
    [DeliverableAlias("pkg")]
    public sealed class PackageDeliverable : Deliverable
    {
        private static string[] knownPackageElements = new[] {
            "info",
            "DataTypes",
            "Templates",
            "DocumentTypes",
            "Macros",
            "DocumentType",
            "files"
        };

        private readonly IFileSystem fileSystem;
        private readonly IChauffeurSettings settings;
        private readonly IPackagingService packagingService;
        private readonly IContentTypeService contentTypeService;
        private readonly IDataTypeService dataTypeService;

        string ElementValue(XElement e, string name)
        {
            return (string)e.Element(name);
        }

        public PackageDeliverable(
            TextReader reader,
            TextWriter writer,
            IFileSystem fileSystem,
            IChauffeurSettings settings,
            IPackagingService packagingService,
            IContentTypeService contentTypeService,
            IDataTypeService dataTypeService)
            : base(reader, writer)
        {
            this.fileSystem = fileSystem;
            this.settings = settings;
            this.packagingService = packagingService;
            this.contentTypeService = contentTypeService;
            this.dataTypeService = dataTypeService;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            var packages = args.Where(a => !a.StartsWith("-f:"));

            if (!packages.Any())
            {
                await Out.WriteLineAsync("No packages were provided, use `help package` to see usage");
                return DeliverableResponse.Continue;
            }

            string chauffeurFolder;

            var overridePath = args.FirstOrDefault(a => a.StartsWith("-f:"));
            if (overridePath != null)
            {
                chauffeurFolder = overridePath.Replace("-f:", string.Empty);
            }
            else
            {
                if (!settings.TryGetChauffeurDirectory(out chauffeurFolder))
                    return DeliverableResponse.Continue;
            }

            var tasks = packages.Select(pkg => Unpack(pkg, chauffeurFolder));
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
                var root = xml.Root;

                var info = root.Element("info");

                if (info != null)
                    await PrintInfo(info);

                var element = root.Element("DataTypes");
                if (element != null)
                    await UnpackDataTypes(element.Elements("DataType"));

                element = root.Element("Templates");
                if (element != null)
                    await UnpackTemplates(element.Elements("Template"));

                element = root.Element("Macros");
                if (element != null)
                    await UnpackMacros(element.Elements("macro"));

                element = root.Element("DocumentTypes");
                if (element != null)
                    await UnpackDocumentTypes(element);
                else if (root.Name == "DocumentType")
                    await UnpackDocumentTypes(root);

                element = root.Element("files");
                if (element != null)
                    await UnpackFiles(chauffeurFolder, element);

                var unknownElements = root.Elements()
                    .Select(x => x.Name)
                    .Where(n => !knownPackageElements.Contains(n.LocalName));

                if (unknownElements.Any())
                {
                    await Out.WriteLineAsync("The following parts of the package weren't imported as their import isn't supported yet. Want it supported? Add an issue on GitHub or send a PR to include it!");
                    foreach (var item in unknownElements)
                        await Out.WriteLineAsync($"- {item}");
                }
            }
        }

        private async Task UnpackFiles(string packageFolder, XElement element)
        {
            var files = element.Elements();
            if (!settings.TryGetSiteRootDirectory(out string siteRootDirectory))
                return;

            foreach (var file in files)
            {
                var metadata = new
                {
                    PackageFilename = ElementValue(file, "guid"),
                    OriginalPath = ElementValue(file, "orgPath"),
                    OriginalName = ElementValue(file, "orgName")
                };

                await Out.WriteLineAsync($"Copying {metadata.OriginalName} from package");

                var destinationPath = fileSystem.Path.Combine(
                        siteRootDirectory,
                        // they use `/` to denote web root, but that'll break when just using fs.Copy, so normalise to just empty
                        metadata.OriginalPath.StartsWith("/") ?
                            metadata.OriginalPath.TrimStart(new[] { '/' }) :
                            metadata.OriginalPath.Replace("~/", string.Empty)
                    );

                if (!fileSystem.Directory.Exists(destinationPath))
                    fileSystem.Directory.CreateDirectory(destinationPath);

                var destFileName = fileSystem.Path.Combine(destinationPath, metadata.OriginalName);
                try
                {
                    fileSystem.File.Copy(
                                fileSystem.Path.Combine(packageFolder, metadata.PackageFilename),
                                destFileName,
                                true
                            );
                }
                catch (IOException)
                {
                    await Out.WriteLineAsync($"Failed to copy a file to {destFileName} as it's locked by another process. You may need to do it manually");
                }

                if (fileSystem.Path.GetExtension(destFileName) == ".dll")
                {
                    await Out.WriteLineAsync($"Found a dll in the package named {metadata.OriginalName} and we'll try and load it into the current AppDomain");
                    var assembly = Assembly.LoadFile(destFileName);
                    AppDomain.CurrentDomain.Load(assembly.FullName);
                }
            }
        }

        private async Task PrintInfo(XElement info)
        {
            var pkg = info.Element("package");

            if (pkg == null)
                return;

            var name = ElementValue(pkg, "name");
            var version = ElementValue(pkg, "version");

            await Out.WriteLineFormattedAsync("Installing package {0} v{1}", name, version);
        }

        private async Task UnpackDataTypes(IEnumerable<XElement> elements)
        {
            foreach (var element in elements)
            {
                var name = (string)element.Attribute("Name");
                await Out.WriteLineFormattedAsync("Importing DataType '{0}'", name);
                packagingService.ImportDataTypeDefinitions(new XElement("DataTypes", element));

                var preValues = element.Element("PreValues");

                if (preValues != null)
                {
                    var pv = preValues.Elements("PreValue");

                    var dataType = dataTypeService.GetDataTypeDefinitionById(Guid.Parse(element.Attribute("Definition").Value));

                    dataTypeService.SavePreValues(
                        dataType,
                        pv.Select(xml => new
                        {
                            Alias = xml.Attribute("Alias").Value,
                            xml.Attribute("Value").Value
                        })
                        .ToDictionary(x => x.Alias, x => new PreValue(x.Value))
                    );
                }
            }
        }

        private async Task UnpackTemplates(IEnumerable<XElement> elements)
        {
            foreach (var element in elements)
            {
                var name = ElementValue(element, "Name");
                await Out.WriteLineFormattedAsync("Importing Template '{0}'", name);
                packagingService.ImportTemplates(element);
            }
        }

        private async Task UnpackMacros(IEnumerable<XElement> elements)
        {
            foreach (var element in elements)
            {
                var name = ElementValue(element, "name");
                await Out.WriteLineFormattedAsync("Importing Macro '{0}'", name);
                packagingService.ImportMacros(element);
            }
        }

        private async Task<IEnumerable<IContentType>> UnpackDocumentTypes(XElement elements)
        {
            await Out.WriteLineAsync("Importing content types");
            return packagingService.ImportContentTypes(elements);
        }
    }
}
