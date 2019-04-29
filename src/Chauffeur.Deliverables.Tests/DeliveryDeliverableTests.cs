using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Chauffeur.Host;
using NPoco;
using NSubstitute;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Xunit;

namespace Chauffeur.Deliverables.Tests
{
    public class DeliveryDeliverableTests
    {
        [Fact]
        public async Task NoExistingDatabase_WillCreateTable()
        {
            var db = Substitute.For<IUmbracoDatabase>();
            var sqlSyntax = Substitute.For<ISqlSyntaxProvider>();
            var sqlContext = Substitute.For<ISqlContext>();
            db.SqlContext.Returns(sqlContext);
            sqlContext.SqlSyntax.Returns(sqlSyntax);

            var logger = Substitute.For<ILogger>();

            var settings = Substitute.For<IChauffeurSettings>();

            var scopeProvider = Substitute.For<IScopeProvider>();
            var scope = Substitute.For<IScope>();
            scope.Database.Returns(db);
            scopeProvider.CreateScope().Returns(scope);

            var deliverable = new DeliveryDeliverable(null, new MockTextWriter(), settings, null, null, scopeProvider, logger);

            await deliverable.Run(null, new string[0]);

            sqlSyntax.Received().DoesTableExist(Arg.Is((IDatabase)db), Arg.Any<string>());
        }

        [Fact]
        public async Task NoDeliveriesFound_DoesntRequireHost()
        {
            var settings = Substitute.For<IChauffeurSettings>();
            string s;
            settings.TryGetChauffeurDirectory(out s).Returns(x =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var db = Substitute.For<IUmbracoDatabase>();
            var sqlSyntax = Substitute.For<ISqlSyntaxProvider>();
            var sqlContext = Substitute.For<ISqlContext>();
            db.SqlContext.Returns(sqlContext);
            sqlContext.SqlSyntax.Returns(sqlSyntax);

            var logger = Substitute.For<ILogger>();

            var writer = new MockTextWriter();

            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\foo\bar.txt", new MockFileData("This is not a deliverable")}
            });

            var scopeProvider = Substitute.For<IScopeProvider>();
            var scope = Substitute.For<IScope>();
            scope.Database.Returns(db);
            scopeProvider.CreateScope().Returns(scope);

            var deliverable = new DeliveryDeliverable(null, writer, settings, fs, null, scopeProvider, logger);

            await deliverable.Run(null, new string[0]);

            Assert.Single(writer.Messages);
        }

        [Fact]
        public async Task FoundDeliveryNotPreviouslyRun_WillBeGivenToTheHost()
        {
            var settings = Substitute.For<IChauffeurSettings>();
            string s;
            settings.TryGetChauffeurDirectory(out s).Returns(x =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var db = Substitute.For<IUmbracoDatabase>();
            db.FirstAsync<ChauffeurDeliveryTable>(Arg.Any<string>(), Arg.Any<object>())
                .Returns(Task.FromResult(new ChauffeurDeliveryTable()));

            var provider = Substitute.For<ISqlSyntaxProvider>();
            provider.DoesTableExist(Arg.Any<IDatabase>(), Arg.Any<string>()).Returns(true);

            var sctx = Substitute.For<ISqlContext>();
            sctx.SqlSyntax.Returns(provider);
            db.SqlContext.SqlSyntax.Returns(provider);

            var logger = Substitute.For<ILogger>();

            var writer = new MockTextWriter();

            var deliverableScript = "foo";
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\foo\bar.delivery", new MockFileData(deliverableScript)}
            });

            var host = Substitute.For<IChauffeurHost>();
            host.RunWithArgs(Arg.Any<string[]>()).Returns(Task.FromResult(DeliverableResponse.Continue));

            var scopeProvider = Substitute.For<IScopeProvider>();
            var scope = Substitute.For<IScope>();
            scope.Database.Returns(db);
            scopeProvider.CreateScope().Returns(scope);

            var deliverable = new DeliveryDeliverable(null, writer, settings, fs, host, scopeProvider, logger);

            await deliverable.Run(null, new string[0]);

            host.Received(1).RunWithArgs(Arg.Any<string[]>()).IgnoreAwaitForNSubstituteAssertion();
        }

        [Fact]
        public async Task FoundDeliveryPreviouslyRun_WillBeSkipped()
        {
            var settings = Substitute.For<IChauffeurSettings>();
            string s;
            settings.TryGetChauffeurDirectory(out s).Returns(x =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var writer = new MockTextWriter();

            var deliverableScript = "foo";
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\foo\bar.delivery", new MockFileData(deliverableScript)}
            });

            var db = Substitute.For<IUmbracoDatabase>();
            db.FirstAsync<ChauffeurDeliveryTable>(Arg.Any<string>(), Arg.Any<object>())
                .Returns(Task.FromResult(new ChauffeurDeliveryTable { Name = "bar.delivery" }));
            var logger = Substitute.For<ILogger>();

            var host = Substitute.For<IChauffeurHost>();
            host.RunWithArgs(Arg.Any<string[]>()).Returns(Task.FromResult(DeliverableResponse.Continue));

            var scopeProvider = Substitute.For<IScopeProvider>();
            var scope = Substitute.For<IScope>();
            scope.Database.Returns(db);
            scopeProvider.CreateScope().Returns(scope);

            var deliverable = new DeliveryDeliverable(null, writer, settings, fs, host, scopeProvider, logger);

            await deliverable.Run(null, new string[0]);

            host.Received(0).RunWithArgs(Arg.Any<string[]>()).IgnoreAwaitForNSubstituteAssertion();
        }

        [Fact]
        public async Task DatabaseError_WillStillAttemptFirstDeliverableThenCreateTableAgain()
        {
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
            host.RunWithArgs(Arg.Any<string[]>()).Returns(Task.FromResult(DeliverableResponse.Continue));

            var db = Substitute.For<IUmbracoDatabase>();
            var sqlSyntax = Substitute.For<ISqlSyntaxProvider>();
            var sqlContext = Substitute.For<ISqlContext>();
            db.SqlContext.Returns(sqlContext);
            sqlContext.SqlSyntax.Returns(sqlSyntax);

            var ex = Substitute.For<DbException>();
            sqlSyntax.DoesTableExist(Arg.Is(db), Arg.Any<string>())
                .Returns(_ => { throw ex; }, _ => true);

            sqlSyntax.Format(Arg.Any<ICollection<ForeignKeyDefinition>>()).Returns(new List<string>());
            sqlSyntax.Format(Arg.Any<ICollection<IndexDefinition>>()).Returns(new List<string>());

            var logger = Substitute.For<ILogger>();

            var scopeProvider = Substitute.For<IScopeProvider>();
            var scope = Substitute.For<IScope>();
            scope.Database.Returns(db);
            scopeProvider.CreateScope().Returns(scope);

            var deliverable = new DeliveryDeliverable(null, writer, settings, fs, host, scopeProvider, logger);

            var result = await deliverable.Run(null, new string[0]);

            Assert.Equal(DeliverableResponse.Continue, result);
            sqlSyntax.Received(1).Format(Arg.Any<TableDefinition>());
        }

        [Fact]
        public async Task WhenRunWithParameters_ParametersSubsitutedToDeliverable()
        {
            var settings = Substitute.For<IChauffeurSettings>();
            string s;
            settings.TryGetChauffeurDirectory(out s).Returns(x =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var writer = new MockTextWriter();

            var deliverableScript = "foo $bar$";
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\foo\bar.delivery", new MockFileData(deliverableScript)}
            });

            var host = Substitute.For<IChauffeurHost>();
            host.RunWithArgs(Arg.Any<string[]>()).Returns(Task.FromResult(DeliverableResponse.Continue));

            var db = Substitute.For<IUmbracoDatabase>();
            var sqlSyntax = Substitute.For<ISqlSyntaxProvider>();
            var sqlContext = Substitute.For<ISqlContext>();
            db.SqlContext.Returns(sqlContext);
            sqlContext.SqlSyntax.Returns(sqlSyntax);

            sqlSyntax.DoesTableExist(Arg.Any<IDatabase>(), Arg.Any<string>()).Returns(true);

            var logger = Substitute.For<ILogger>();

            var scopeProvider = Substitute.For<IScopeProvider>();
            var scope = Substitute.For<IScope>();
            scope.Database.Returns(db);
            scopeProvider.CreateScope().Returns(scope);

            var deliverable = new DeliveryDeliverable(null, writer, settings, fs, host, scopeProvider, logger);

            await deliverable.Run(null, new[] { "-p:bar=baz" });

            host.Received(1)
                .RunWithArgs(Arg.Is<string[]>(x => x[0] == "foo baz"))
                .IgnoreAwaitForNSubstituteAssertion();
        }

        [Fact]
        public async Task WhenRunWithMultipleParameters_ParametersSubsitutedToDeliverable()
        {
            var settings = Substitute.For<IChauffeurSettings>();
            string s;
            settings.TryGetChauffeurDirectory(out s).Returns(x =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var writer = new MockTextWriter();

            var deliverableScript = "foo $bar$ $pwd$";
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\foo\bar.delivery", new MockFileData(deliverableScript)}
            });

            var host = Substitute.For<IChauffeurHost>();
            host.RunWithArgs(Arg.Any<string[]>()).Returns(Task.FromResult(DeliverableResponse.Continue));

            var db = Substitute.For<IUmbracoDatabase>();
            var sqlSyntax = Substitute.For<ISqlSyntaxProvider>();
            var sqlContext = Substitute.For<ISqlContext>();
            db.SqlContext.Returns(sqlContext);
            sqlContext.SqlSyntax.Returns(sqlSyntax);

            sqlSyntax.DoesTableExist(Arg.Any<IDatabase>(), Arg.Any<string>()).Returns(true);

            var logger = Substitute.For<ILogger>();

            var scopeProvider = Substitute.For<IScopeProvider>();
            var scope = Substitute.For<IScope>();
            scope.Database.Returns(db);
            scopeProvider.CreateScope().Returns(scope);

            var deliverable = new DeliveryDeliverable(null, writer, settings, fs, host, scopeProvider, logger);

            await deliverable.Run(null, new[] { "-p:bar=baz", "-p:pwd=pwd" });

            host.Received(1)
                .RunWithArgs(Arg.Is<string[]>(x => x[0] == "foo baz pwd"))
                .IgnoreAwaitForNSubstituteAssertion();
        }

        [Fact]
        public async Task WhenRunWithMissingParameters_ErrorsAndOutputsMissingParameters()
        {
            var settings = Substitute.For<IChauffeurSettings>();
            settings.TryGetChauffeurDirectory(out var s).Returns(x =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var writer = new MockTextWriter();

            var deliverableScript = "foo $test$ $bar$ $baz$";
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\foo\bar.delivery", new MockFileData(deliverableScript)}
            });

            var host = Substitute.For<IChauffeurHost>();
            host.RunWithArgs(Arg.Any<string[]>()).Returns(Task.FromResult(DeliverableResponse.Continue));

            var db = Substitute.For<IUmbracoDatabase>();
            var sqlSyntax = Substitute.For<ISqlSyntaxProvider>();
            var sqlContext = Substitute.For<ISqlContext>();
            db.SqlContext.Returns(sqlContext);
            sqlContext.SqlSyntax.Returns(sqlSyntax);

            sqlSyntax.DoesTableExist(Arg.Any<IDatabase>(), Arg.Any<string>()).Returns(true);

            var logger = Substitute.For<ILogger>();

            var scopeProvider = Substitute.For<IScopeProvider>();
            var scope = Substitute.For<IScope>();
            scope.Database.Returns(db);
            scopeProvider.CreateScope().Returns(scope);

            var deliverable = new DeliveryDeliverable(null, writer, settings, fs, host, scopeProvider, logger);

            await deliverable.Run(null, Array.Empty<string>());

            Assert.Equal(new[]
            {
                "The following parameters have not been specified:",
                " - bar",
                " - baz",
                " - test"
            }, writer.Messages);
        }

        [Fact]
        public async Task CommentsInDelivery_WillNotBeGivenToTheHost()
        {
            var settings = Substitute.For<IChauffeurSettings>();
            settings.TryGetChauffeurDirectory(out string s).Returns(x =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var writer = new MockTextWriter();

            var deliverableScript = "## this is a comment\r\nfoo";
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\foo\bar.delivery", new MockFileData(deliverableScript)}
            });

            var host = Substitute.For<IChauffeurHost>();
            host.RunWithArgs(Arg.Any<string[]>()).Returns(Task.FromResult(DeliverableResponse.Continue));

            var db = Substitute.For<IUmbracoDatabase>();
            var sqlSyntax = Substitute.For<ISqlSyntaxProvider>();
            var sqlContext = Substitute.For<ISqlContext>();
            db.SqlContext.Returns(sqlContext);
            sqlContext.SqlSyntax.Returns(sqlSyntax);

            sqlSyntax.DoesTableExist(Arg.Any<IDatabase>(), Arg.Any<string>()).Returns(true);

            var logger = Substitute.For<ILogger>();

            var scopeProvider = Substitute.For<IScopeProvider>();
            var scope = Substitute.For<IScope>();
            scope.Database.Returns(db);
            scopeProvider.CreateScope().Returns(scope);
            var deliverable = new DeliveryDeliverable(null, writer, settings, fs, host, scopeProvider, logger);

            await deliverable.Run(null, new string[0]);

            host.Received(1).RunWithArgs(Arg.Any<string[]>()).IgnoreAwaitForNSubstituteAssertion();
        }

        [Fact]
        public async Task StopDeliverableName_WillOnlyRunUpToNamedDeliverable()
        {
            var settings = Substitute.For<IChauffeurSettings>();
            string s;
            settings.TryGetChauffeurDirectory(out s).Returns(x =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var writer = new MockTextWriter();

            var deliverableScript = "foo";
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\foo\001.delivery", new MockFileData(deliverableScript)},
                {@"c:\foo\002.delivery", new MockFileData(deliverableScript)}
            });

            var host = Substitute.For<IChauffeurHost>();
            host.RunWithArgs(Arg.Any<string[]>()).Returns(Task.FromResult(DeliverableResponse.Continue));

            var db = Substitute.For<IUmbracoDatabase>();
            var sqlSyntax = Substitute.For<ISqlSyntaxProvider>();
            var sqlContext = Substitute.For<ISqlContext>();
            db.SqlContext.Returns(sqlContext);
            sqlContext.SqlSyntax.Returns(sqlSyntax);

            sqlSyntax.DoesTableExist(Arg.Any<IDatabase>(), Arg.Any<string>()).Returns(true);

            var logger = Substitute.For<ILogger>();

            var scopeProvider = Substitute.For<IScopeProvider>();
            var scope = Substitute.For<IScope>();
            scope.Database.Returns(db);
            scopeProvider.CreateScope().Returns(scope);

            var deliverable = new DeliveryDeliverable(null, writer, settings, fs, host, scopeProvider, logger);

            await deliverable.Run(null, new[] { "-s:002.delivery" });

            host.Received(1).RunWithArgs(Arg.Any<string[]>()).IgnoreAwaitForNSubstituteAssertion();
        }

    }
}
