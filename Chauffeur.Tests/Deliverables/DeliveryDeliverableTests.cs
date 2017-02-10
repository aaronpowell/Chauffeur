using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using Chauffeur.Deliverables;
using Chauffeur.Host;
using NSubstitute;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;
using Xunit;

namespace Chauffeur.Tests.Deliverables
{
    public class DeliveryDeliverableTests
    {
        [Fact]
        public async Task NoExistingDatabase_WillCreateTable()
        {
            var provider = Substitute.For<ISqlSyntaxProvider>();
            provider.Format(Arg.Any<ICollection<ForeignKeyDefinition>>()).Returns(new List<string>());
            provider.Format(Arg.Any<ICollection<IndexDefinition>>()).Returns(new List<string>());
            provider.DoesTableExist(Arg.Any<Database>(), Arg.Any<string>()).Returns(false);

            SqlSyntaxContext.SqlSyntaxProvider = provider;

            var conn = Substitute.For<IDbConnection>();
            var db = new Database(conn);

            var settings = Substitute.For<IChauffeurSettings>();

            var deliverable = new DeliveryDeliverable(null, new MockTextWriter(), db, settings, null, null);

            await deliverable.Run(null, new string[0]);

            provider.Received().DoesTableExist(Arg.Is((Database)db), Arg.Any<string>());
        }

        [Fact]
        public async Task NoDeliveriesFound_DoesntRequireHost()
        {
            var provider = Substitute.For<ISqlSyntaxProvider>();
            provider.DoesTableExist(Arg.Any<Database>(), Arg.Any<string>()).Returns(true);

            SqlSyntaxContext.SqlSyntaxProvider = provider;

            var settings = Substitute.For<IChauffeurSettings>();
            string s;
            settings.TryGetChauffeurDirectory(out s).Returns(x =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var conn = Substitute.For<IDbConnection>();
            var db = new Database(conn);

            var writer = new MockTextWriter();

            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\foo\bar.txt", new MockFileData("This is not a deliverable")}
            });

            var deliverable = new DeliveryDeliverable(null, writer, db, settings, fs, null);

            await deliverable.Run(null, new string[0]);

            Assert.Equal(writer.Messages.Count(), 1);
        }

        [Fact]
        public async Task FoundDeliveryNotPreviouslyRun_WillBeGivenToTheHost()
        {
            var provider = Substitute.For<ISqlSyntaxProvider>();
            provider.DoesTableExist(Arg.Any<Database>(), Arg.Any<string>()).Returns(true);

            SqlSyntaxContext.SqlSyntaxProvider = provider;

            var settings = Substitute.For<IChauffeurSettings>();
            string s;
            settings.TryGetChauffeurDirectory(out s).Returns(x =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var cmd = Substitute.For<IDbCommand>();
            cmd.ExecuteScalar().Returns(1);
            var conn = Substitute.For<IDbConnection>();
            conn.CreateCommand().Returns(cmd);
            var db = new Database(conn);

            var writer = new MockTextWriter();

            var deliverableScript = "foo";
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\foo\bar.delivery", new MockFileData(deliverableScript)}
            });

            var host = Substitute.For<IChauffeurHost>();
            host.Run(Arg.Any<string[]>()).Returns(Task.FromResult(DeliverableResponse.Continue));

            var deliverable = new DeliveryDeliverable(null, writer, db, settings, fs, host);

            await deliverable.Run(null, new string[0]);

            host.Received(1).Run(Arg.Any<string[]>()).IgnoreAwaitForNSubstituteAssertion();
        }

        [Fact]
        public async Task FoundDeliveryPreviouslyRun_WillBeSkipped()
        {
            var provider = Substitute.For<ISqlSyntaxProvider>();
            provider.DoesTableExist(Arg.Any<Database>(), Arg.Any<string>()).Returns(true);

            SqlSyntaxContext.SqlSyntaxProvider = provider;

            var settings = Substitute.For<IChauffeurSettings>();
            string s;
            settings.TryGetChauffeurDirectory(out s).Returns(x =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var reader = Substitute.For<IDataReader>();
            reader.Read().Returns(true, false);
            reader.GetBoolean(Arg.Any<int>()).Returns(true);
            reader.GetInt32(Arg.Any<int>()).Returns(1);
            reader.GetString(Arg.Any<int>()).Returns(string.Empty);
            reader.GetDateTime(Arg.Any<int>()).Returns(DateTime.Now);
            reader.GetValue(Arg.Any<int>()).Returns(DateTime.Now);
            reader.FieldCount.Returns(5); //the number of properties on the table
            reader.GetName(Arg.Any<int>()).Returns("Id", "Name", "ExecutionDate", "SignedFor", "Hash");
            reader.GetFieldType(Arg.Any<int>()).Returns(typeof(int), typeof(string), typeof(DateTime), typeof(bool), typeof(string));
            var cmd = Substitute.For<IDbCommand>();
            cmd.ExecuteReader().Returns(reader);
            cmd.ExecuteScalar().Returns(1);
            var conn = Substitute.For<IDbConnection>();
            conn.CreateCommand().Returns(cmd);
            var db = new Database(conn);

            var writer = new MockTextWriter();

            var deliverableScript = "foo";
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\foo\bar.delivery", new MockFileData(deliverableScript)}
            });

            var host = Substitute.For<IChauffeurHost>();
            host.Run(Arg.Any<string[]>()).Returns(Task.FromResult(DeliverableResponse.Continue));

            var deliverable = new DeliveryDeliverable(null, writer, db, settings, fs, host);

            await deliverable.Run(null, new string[0]);

            host.Received(0).Run(Arg.Any<string[]>()).IgnoreAwaitForNSubstituteAssertion();
        }

        [Fact]
        public async Task DatabaseError_WillStillAttemptFirstDeliverableThenCreateTableAgain()
        {
            var ex =Substitute.For<DbException>();

            var provider = Substitute.For<ISqlSyntaxProvider>();
            provider.Format(Arg.Any<ICollection<ForeignKeyDefinition>>()).Returns(new List<string>());
            provider.Format(Arg.Any<ICollection<IndexDefinition>>()).Returns(new List<string>());
            provider.DoesTableExist(Arg.Any<Database>(), Arg.Any<string>()).Returns(_ => { throw ex; }, _ => true);

            SqlSyntaxContext.SqlSyntaxProvider = provider;

            var conn = Substitute.For<IDbConnection>();
            var cmd = Substitute.For<IDbCommand>();
            cmd.ExecuteScalar().Returns(1);
            conn.CreateCommand().Returns(cmd);
            var db = new Database(conn);

            var settings = Substitute.For<IChauffeurSettings>();
            string s;
            settings.TryGetChauffeurDirectory(out s).Returns(x =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var writer = new MockTextWriter();

            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\foo\bar.delivery", new MockFileData("install")}
            });

            var host = Substitute.For<IChauffeurHost>();
            host.Run(Arg.Any<string[]>()).Returns(Task.FromResult(DeliverableResponse.Continue));

            var deliverable = new DeliveryDeliverable(null, writer, db, settings, fs, host);

            await deliverable.Run(null, new string[0]);

            cmd.Received().CommandText = Arg.Any<string>();
        }

        [Fact]
        public async Task WhenRunWithParameters_ParametersSubsitutedToDeliverable()
        {
            var provider = Substitute.For<ISqlSyntaxProvider>();
            provider.DoesTableExist(Arg.Any<Database>(), Arg.Any<string>()).Returns(true);

            SqlSyntaxContext.SqlSyntaxProvider = provider;

            var settings = Substitute.For<IChauffeurSettings>();
            string s;
            settings.TryGetChauffeurDirectory(out s).Returns(x =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var cmd = Substitute.For<IDbCommand>();
            cmd.ExecuteScalar().Returns(1);
            var conn = Substitute.For<IDbConnection>();
            conn.CreateCommand().Returns(cmd);
            var db = new Database(conn);

            var writer = new MockTextWriter();

            var deliverableScript = "foo $bar$";
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\foo\bar.delivery", new MockFileData(deliverableScript)}
            });

            var host = Substitute.For<IChauffeurHost>();
            host.Run(Arg.Any<string[]>()).Returns(Task.FromResult(DeliverableResponse.Continue));

            var deliverable = new DeliveryDeliverable(null, writer, db, settings, fs, host);

            await deliverable.Run(null, new[] { "-p:bar=baz" });

            host.Received(1)
                .Run(Arg.Is<string[]>(x => x[0] == "foo baz"))
                .IgnoreAwaitForNSubstituteAssertion();
        }
    }
}
