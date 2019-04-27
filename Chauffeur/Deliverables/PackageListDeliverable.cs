using Chauffeur;
using Chauffeur.Services;
using System.IO;
using System.Threading.Tasks;
using umbraco.cms.businesslogic.packager;

namespace Chauffeur.Deliverables
{
    

    [DeliverableName("package-list")]
    [DeliverableAlias("pkg-list")]
    [DeliverableAlias("pl")]
    public class PackageListDeliverable : Deliverable
    {
        private readonly ICreatedPackageWrapper createdPackagesWrapper;

        public PackageListDeliverable(
            TextReader reader, 
            TextWriter writer,
            ICreatedPackageWrapper createdPackagesWrapper
            ) : base(reader, writer)
        {
            this.createdPackagesWrapper = createdPackagesWrapper;
        }

        public async override Task<DeliverableResponse> Run(string command, string[] args)
        {
            var packages = createdPackagesWrapper.GetAllCreatedPackages();
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