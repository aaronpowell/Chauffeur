using Chauffeur.Deliverables;
using Chauffeur.Services.Interfaces;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using umbraco.cms.businesslogic.packager;
using Xunit;

namespace Chauffeur.Tests.Deliverables
{
    public class PackageListDeliverableTests
    {
        [Fact]
        public async Task CanListPackages()
        {
            var reader = Substitute.ForPartsOf<TextReader>();
            var writer = Substitute.ForPartsOf<TextWriter>();
            var createdPackager = Substitute.For<ICreatedPackageService>();
            var packagelist = new PackageListDeliverable(reader, writer, createdPackager);

            IList<CreatedPackage> testdata = new List<CreatedPackage>()
            {
                new CreatedPackage(){ Data =  new PackageInstance() { Id = 1, Name = "Test1", PackageGuid = Guid.Empty.ToString() } },
                new CreatedPackage(){ Data =  new PackageInstance() { Id = 2, Name = "Test2", PackageGuid = Guid.Empty.ToString() } },
                new CreatedPackage(){ Data =  new PackageInstance() { Id = 3, Name = "Test3", PackageGuid = Guid.Empty.ToString() } },
                new CreatedPackage(){ Data =  new PackageInstance() { Id = 4, Name = "Test4", PackageGuid = Guid.Empty.ToString() } },
            };
            createdPackager.GetAllCreatedPackages().Returns(testdata);


            await packagelist.Run(null, new string[0]);

            writer.Received(4).WriteLineAsync(Arg.Any<string>()).IgnoreAwaitForNSubstituteAssertion();
        }

        [Fact]
        public async Task CanReportNoPackages()
        {
            var reader = Substitute.ForPartsOf<TextReader>();
            var writer = Substitute.ForPartsOf<TextWriter>();
            var createdPackager = Substitute.For<ICreatedPackageService>();
            var packagelist = new PackageListDeliverable(reader, writer, createdPackager);

            IList<CreatedPackage> testdata = new List<CreatedPackage>();
            createdPackager.GetAllCreatedPackages().Returns(testdata);


            await packagelist.Run(null, new string[0]);

            writer.Received(1).WriteLineAsync("No Created Packages Found").IgnoreAwaitForNSubstituteAssertion();
        }
    }
}
