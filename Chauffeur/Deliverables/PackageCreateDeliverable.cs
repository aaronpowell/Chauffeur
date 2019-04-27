using Chauffeur;
using Chauffeur.Services.Interfaces;
using System.IO;
using System.Threading.Tasks;
using umbraco.cms.businesslogic.packager;
using Umbraco.Core.IO;

namespace Chauffeur.Deliverables
{
    [DeliverableName("package-create")]
    [DeliverableAlias("pkg-create")]
    [DeliverableAlias("pc")]
    public class PackageCreateDeliverable : Deliverable
    {
        private readonly ICreatedPackageService createdPackageService;

        public PackageCreateDeliverable(TextReader reader, 
            TextWriter writer,
            ICreatedPackageService createdPackagesWrapper
            ) : base(reader , writer)
        {
            this.createdPackageService = createdPackagesWrapper;
        }

        public async override Task<DeliverableResponse> Run(string command, string[] args)
        {
            var createdPackage = createdPackageService.GetById(int.Parse(args[0]));
            var pack = createdPackage.Data;
            createdPackage.Publish();

            var fileName = SystemDirectories.Media + "/created-packages/" + (pack.Name + "_" + pack.Version).Replace(' ', '_') + "." + Settings.PackageFileExtension;

            await Out.WriteLineAsync($"{createdPackage.Data.Id})  {createdPackage.Data.Name} created");
            await Out.WriteLineAsync($"{fileName}");
            return DeliverableResponse.Continue;
        }

    }
}