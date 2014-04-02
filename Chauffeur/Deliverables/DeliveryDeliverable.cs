using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Chauffeur.Host;

namespace Chauffeur.Deliverables
{
    [DeliverableName("delivery")]
    [DeliverableAlias("d")]
    public sealed class DeliveryDeliverable : Deliverable
    {
        private readonly IFileSystem fileSystem;
        private readonly IChauffeurSettings settings;
        public DeliveryDeliverable(
            TextReader reader,
            TextWriter writer,
            IFileSystem fileSystem,
            IChauffeurSettings settings)
            : base(reader, writer)
        {
            this.fileSystem = fileSystem;
            this.settings = settings;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            if (!args.Any())
            {
                await Out.WriteLineAsync("No deliveries were provided, use `help delivery` to see usage");
                return DeliverableResponse.Continue;
            }

            string chauffeurFolder;
            if (!settings.TryGetChauffeurDirectory(out chauffeurFolder))
                return DeliverableResponse.Continue;

            var tasks = args.Select(arg => Deliver(arg, chauffeurFolder));
            await Task.WhenAll(tasks);

            return DeliverableResponse.Continue;
        }

        private async Task Deliver(string name, string chauffeurFolder)
        {
            var fileLocation = fileSystem.Path.Combine(chauffeurFolder, name + ".xml");
            if (!fileSystem.File.Exists(fileLocation))
            {
                await Out.WriteLineFormattedAsync("The delivery '{0}' is not found in the Chauffeur folder", name);
                return;
            }

            await Out.WriteLineAsync(@"¯\_(ツ)_/¯");
        }
    }
}
