using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Chauffeur.Host;
using Umbraco.Core.Services;

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
        public PackageDeliverable(
            TextReader reader,
            TextWriter writer,
            IFileSystem fileSystem,
            IChauffeurSettings settings,
            IPackagingService packagingService)
            : base(reader, writer)
        {
            this.fileSystem = fileSystem;
            this.settings = settings;
            this.packagingService = packagingService;
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

                var documentTypesElement = xml.Root.Element("DocumentTypes");

                if (documentTypesElement != null)
                    await UnpackDocumentTypes(documentTypesElement.Elements("DocumentType"));
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

        private async Task UnpackDocumentTypes(IEnumerable<XElement> elements)
        {
            foreach (var element in elements)
            {
                var name = (string)element.Element("Info").Element("Name");
                await Out.WriteLineFormattedAsync("Importing DocumentType '{0}'", name);
                packagingService.ImportContentTypes(element);
            }
        }
    }
}
