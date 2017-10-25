using Chauffeur.Host;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Umbraco.Core.Services;

namespace Chauffeur.Deliverables
{
    [DeliverableName("scaffold")]
    public class ScaffoldDeliverable : Deliverable, IProvideDirections
    {
        private readonly IChauffeurSettings settings;
        private readonly IFileSystem fileSystem;

        public ScaffoldDeliverable(
            TextReader reader,
            TextWriter writer,
            IChauffeurSettings settings,
            IFileSystem fileSystem) : base(reader, writer)
        {
            this.settings = settings;
            this.fileSystem = fileSystem;
        }

        public async Task Directions()
        {
            await Out.WriteLineAsync("scaffold");
            await Out.WriteLineAsync("\tAllows you to setup a new Chauffeur usage within your project. It will create a base delivery and offer to setup the umbraco installation for you.");
        }

        public async override Task<DeliverableResponse> Run(string command, string[] args)
        {
            var (name, includeInstall) = await GetScaffoldSettings();
            using (var deliveryFileStream = CreateDeliveryFile(name))
            {
                if (includeInstall == "Y")
                {
                    await deliveryFileStream.WriteLineAsync("install y");
                    await deliveryFileStream.WriteLineAsync("user change-password admin $adminpwd$");
                }
            }

            return DeliverableResponse.Continue;
        }

        private StreamWriter CreateDeliveryFile(string name)
        {
            settings.TryGetChauffeurDirectory(out string dir);
            var file = fileSystem.FileInfo.FromFileName(
                fileSystem.Path.Combine(dir, $"{name}.delivery")
            );
            return file.CreateText();
        }

        private async Task<(string name, string includeInstall)> GetScaffoldSettings()
        {
            await Out.WriteLineAsync("Time to setup Chauffeur!");
            await Out.WriteAsync("What do you want the name to be (001-Setup)? ");
            var name = await In.ReadLineWithDefaultAsync("001-Setup");

            await Out.WriteAsync("Include an install step (Y/n)? ");
            var includeInstall = await In.ReadLineWithDefaultAsync("Y");

            return (name, includeInstall);
        }
    }
}
