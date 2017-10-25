using Chauffeur.Host;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Core.Services;

namespace Chauffeur.Deliverables
{
    [DeliverableName("scaffold")]
    public class ScaffoldDeliverable : Deliverable, IProvideDirections
    {
        private readonly IChauffeurSettings settings;
        private readonly IFileSystem fileSystem;
        private readonly IContentTypeService contentTypeService;
        private readonly IDataTypeService dataTypeService;
        private readonly IPackagingService packagingService;

        public ScaffoldDeliverable(
            TextReader reader,
            TextWriter writer,
            IChauffeurSettings settings,
            IFileSystem fileSystem,
            IContentTypeService contentTypeService,
            IDataTypeService dataTypeService,
            IPackagingService packagingService) : base(reader, writer)
        {
            this.settings = settings;
            this.fileSystem = fileSystem;
            this.contentTypeService = contentTypeService;
            this.dataTypeService = dataTypeService;
            this.packagingService = packagingService;
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
                if (includeInstall.ToLowerInvariant() == "y")
                {
                    await deliveryFileStream.WriteLineAsync("install y");
                    await deliveryFileStream.WriteLineAsync("user change-password admin $adminpwd$");
                }

                await Out.WriteAsync("Do you want to package your current instance (Y/n)? ");
                var package = await In.ReadLineWithDefaultAsync("Y");
                if (package.ToLowerInvariant() == "y")
                    await CreatePackage(deliveryFileStream, name);
            }

            return DeliverableResponse.Continue;
        }

        private async Task CreatePackage(StreamWriter deliveryFileStream, string name)
        {
            var contentTypes = contentTypeService.GetAllContentTypes();
            var dataTypes = dataTypeService.GetAllDataTypeDefinitions();

            var packageXml = new XDocument();
            packageXml.Add(new XElement(
                "umbPackage",
                new XElement(
                    "DocumentTypes",
                    contentTypes.Select(ct => packagingService.Export(ct, false))
                    )
                ),
                new XElement(
                    "DataTypes",
                    dataTypes.Select(dt => packagingService.Export(dt, false))
                )
            );

            settings.TryGetChauffeurDirectory(out string dir);
            packageXml.Save(fileSystem.Path.Combine(dir, $"{name}.xml"));
            await deliveryFileStream.WriteLineAsync($"package {name}");
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
