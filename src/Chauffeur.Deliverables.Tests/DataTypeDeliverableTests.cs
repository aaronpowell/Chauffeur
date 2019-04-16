using NSubstitute;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Xunit;

namespace Chauffeur.Deliverables.Tests
{
    public class DataTypeDeliverableTests
    {
        [Fact]
        public async System.Threading.Tasks.Task CanExportAllDataTypeDefinitions()
        {
            var writer = new MockTextWriter();

            var defs = new List<IDataType>();
            var d = Substitute.For<IDataType>();
            d.Id = 1;
            d.Name = "One";
            defs.Add(d);

            d = Substitute.For<IDataType>();
            d.Id = 2;
            d.Name = "Two";
            defs.Add(d);

            var dataTypeService = Substitute.For<IDataTypeService>();
            dataTypeService.GetAll(Arg.Any<int>()).Returns(defs);

            var fs = new MockFileSystem();

            var settings = Substitute.For<IChauffeurSettings>();
            settings.TryGetChauffeurDirectory(out string dir).Returns(x =>
             {
                 x[0] = "c:\\chauffeur";
                 return true;
             });
            fs.AddDirectory("c:\\chauffeur");

            var serializer = Substitute.For<IEntityXmlSerializer>();
            serializer.Serialize(Arg.Any<IEnumerable<IDataType>>())
                .Returns(new System.Xml.Linq.XElement("Foo"));

            var deliverable = new DataTypeDeliverable(null, writer, dataTypeService, fs, settings, serializer);

            await deliverable.Run("data-type", new[] { "export" });

            Assert.Single(fs.AllFiles);
        }
    }
}