using Chauffeur.Services.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace Chauffeur.Deliverables
{


    [DeliverableName("package-list")]
    [DeliverableAlias("pkg-list")]
    [DeliverableAlias("pl")]
    public class PackageListDeliverable : Deliverable
    {
        private readonly ICreatedPackageService createdPackagesService;

        public PackageListDeliverable(
            TextReader reader, 
            TextWriter writer,
            ICreatedPackageService createdPackagesService
            ) : base(reader, writer)
        {
            this.createdPackagesService = createdPackagesService;
        }

        public async override Task<DeliverableResponse> Run(string command, string[] args)
        {
            var packages = createdPackagesService.GetAllCreatedPackages();
            bool foundPackages = false;

            foreach (var package in packages)
            {
                await Out.WriteLineAsync($"{package.Data.Id}) {package.Data.Name} [{package.Data.PackageGuid}]");
                foundPackages = true;
            }

            if (foundPackages == false)
            {
                await Out.WriteLineAsync("No Created Packages Found");
            }

            return DeliverableResponse.Continue;
        }
    }
}