using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Chauffeur.Host;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Chauffeur.Deliverables
{
    [DeliverableName("document-type-diff")]
    [DeliverableAlias("diff-dt")]
    public class DocumentTypeDiffDeliverable : Deliverable
    {
        private readonly IContentTypeService contentTypeService;
        private readonly IChauffeurSettings settings;
        private readonly IFileSystem fileSystem;
        public DocumentTypeDiffDeliverable(
            TextReader reader,
            TextWriter writer,
            IContentTypeService contentTypeService,
            IChauffeurSettings settings,
            IFileSystem fileSystem)
            : base(reader, writer)
        {
            this.contentTypeService = contentTypeService;
            this.settings = settings;
            this.fileSystem = fileSystem;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            return await DiffDocumentType(args[0], args.Any(x => x == "--save"));
        }

        private async Task<DeliverableResponse> DiffDocumentType(string alias, bool save)
        {
            int id;
            IContentType contentType;
            if (int.TryParse(alias, out id))
                contentType = contentTypeService.GetContentType(id);
            else
                contentType = contentTypeService.GetContentType(alias);

            if (contentType == null)
            {
                await Out.WriteLineFormattedAsync("Unable to find a Document Type with the id or alias of '{0}'", alias);
                return DeliverableResponse.Continue;
            }

            string dir;
            if (!settings.TryGetChauffeurDirectory(out dir))
            {
                await Out.WriteLineAsync("Error accessing the Chauffeur folder, check the permissions and relaunch");
                return DeliverableResponse.Continue;
            }

            var folder = fileSystem.DirectoryInfo.FromDirectoryName(dir);
            var packages = folder.GetFiles("*.xml");

            var packagesAsXml = packages.Select(p => XDocument.Load(p.FullName));

            var documentTypes = packagesAsXml.SelectMany(xml => xml.Descendants("DocumentType"));

            var matches = documentTypes
                .Where(x => x.Descendants("Info").Any())
                .Select(x => new
                {
                    Info = x.Descendants("Info").First(),
                    Xml = x
                })
                .Where(x => x.Info.Descendants("Alias").First().Value == contentType.Alias)
                .Select(x => x.Xml)
                .ToArray();

            if (!matches.Any())
            {
                await Out.WriteLineFormattedAsync("The alias '{0}' does not match a packaged Document Type.");
            }
            else
            {
                var exportedProperties = matches.SelectMany(x => x.Descendants("GenericProperty"))
                    .Distinct()
                    .ToArray();

                var currentProperties = contentType.PropertyTypes.ToArray();

                var propertiesNotExported = currentProperties.Where(p => !exportedProperties.Any(x => (string)x.Element("Alias") == p.Alias));

                if (!propertiesNotExported.Any())
                {
                    await Out.WriteLineAsync("All properties of the Document Type are already exported as packages");
                    return DeliverableResponse.Continue;
                }

                await Out.WriteLineAsync("The following properties don't have an export that Chauffeur knows about:");
                foreach (var p in propertiesNotExported)
                    await Out.WriteLineFormattedAsync("{0},{1}", p.Name, p.Alias);
            }

            return DeliverableResponse.Continue;
        }
    }
}
