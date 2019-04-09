using Chauffeur.Deliverables;
using Chauffeur.Host;
using NSubstitute;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Xunit;

namespace Chauffeur.Tests.Deliverables
{
    public class DataTypeDeliverableTests
    {
        [Fact]
        public async System.Threading.Tasks.Task CanExportAllDataTypeDefinitions()
        {
            var writer = new MockTextWriter();

            var defs = new List<IDataTypeDefinition>();
            var d = Substitute.For<IDataTypeDefinition>();
            d.Id = 1;
            d.Name = "One";
            defs.Add(d);

            d = Substitute.For<IDataTypeDefinition>();
            d.Id = 2;
            d.Name = "Two";
            defs.Add(d);

            var dataTypeService = Substitute.For<IDataTypeService>();
            dataTypeService.GetAllDataTypeDefinitions(Arg.Any<int>()).Returns(defs);

            var packagingService = new PackagingService(null, null, null, null, null, dataTypeService, null, null, null, null, null, null);

            var fs = new MockFileSystem();

            var settings = Substitute.For<IChauffeurSettings>();
            string dir;
            settings.TryGetChauffeurDirectory(out dir).Returns(x =>
            {
                x[0] = "c:\\chauffeur";
                return true;
            });
            var deliverable = new DataTypeDeliverable(null, writer, dataTypeService, packagingService, fs, settings);

            await deliverable.Run("data-type", new[] { "export" });

            Assert.Single(fs.AllFiles);
        }
    }
}
