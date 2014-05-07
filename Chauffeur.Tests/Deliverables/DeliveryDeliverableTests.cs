using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chauffeur.Deliverables;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Chauffeur.Tests.Deliverables
{
    [TestFixture]
    public class DeliveryDeliverableTests
    {
        [Test]
        public async Task NoExistingDatabase_WillCreateTable()
        {
            SqlSyntaxContext.SqlSyntaxProvider = Substitute.For<ISqlSyntaxProvider>();
            SqlSyntaxContext.SqlSyntaxProvider.Format(Arg.Any<ICollection<ForeignKeyDefinition>>()).Returns(new List<string>());
            SqlSyntaxContext.SqlSyntaxProvider.Format(Arg.Any<ICollection<IndexDefinition>>()).Returns(new List<string>());

            var conn = Substitute.For<IDbConnection>();
            var db = new UmbracoDatabase(conn);

            var deliverable = new DeliveryDeliverable(null, new MockTextWriter(), db);

            await deliverable.Run(null, null);

            Assert.Pass("No errors raised when creating the database");
        }
    }
}
