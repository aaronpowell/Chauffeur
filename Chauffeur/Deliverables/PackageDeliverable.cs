using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Chauffeur.Host;

namespace Chauffeur.Deliverables
{
    [DeliverableName("package")]
    [DeliverableAlias("p")]
    [DeliverableAlias("pkg")]
    public sealed class PackageDeliverable : Deliverable
    {
        private readonly IFileSystem fileSystem;
        private readonly IChauffeurSettings settings;
        public PackageDeliverable(
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

            await Out.WriteLineAsync(@"¯\_(ツ)_/¯");
        }
    }
}
