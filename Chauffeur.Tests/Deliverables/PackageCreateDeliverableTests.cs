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
    public class PackageCreateDeliverableTests
    {
        [Fact]
        public async Task CanCreatePackage()
        {
            var publishCount = 0;

            var reader = Substitute.ForPartsOf<TextReader>();
            var writer = Substitute.ForPartsOf<TextWriter>();
            var packagerService = Substitute.For<ICreatedPackageService>();
            var createdPackage = Substitute.For<ICreatedPackageService>();
            var packageCreate = new PackageCreateDeliverable(reader, writer, packagerService);
            
            createdPackage.When(x => x.Publish()).Do(x => publishCount++);
            createdPackage.Data = new PackageInstance() { Id = 1, Name = "Test1", PackageGuid = Guid.Empty.ToString() };
            packagerService.GetById(Arg.Any<int>()).Returns( createdPackage );

            await packageCreate.Run(null, new string[] { "1" });

            writer.Received(2).WriteLineAsync(Arg.Any<string>()).IgnoreAwaitForNSubstituteAssertion();
            Assert.Equal(1, publishCount);
        }
    }
}
