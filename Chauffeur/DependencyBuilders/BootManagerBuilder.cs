using Chauffeur.Services;
using Chauffeur.Services.Interfaces;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;

namespace Chauffeur.DependencyBuilders
{
    class BootManagerBuilder : IBuildDependencies
    {
        public void Build(IContainer container)
        {
            var application = new ChauffeurUmbracoApplication();
            application.Start();

            var context = ApplicationContext.Current;

            container.Register(() => context);
            container.Register(() => context.DatabaseContext);
            container.Register(() => context.DatabaseContext.Database).As<Database>();
            container.Register(() => context.DatabaseContext.SqlSyntax);
            container.Register(() => context.ProfilingLogger.Logger);
            container.Register<DatabaseSchemaHelper>();

            var services = context.Services;
            container.Register(() => services.ContentService);
            container.Register(() => services.ContentTypeService);
            container.Register(() => services.DataTypeService);
            container.Register(() => services.EntityService);
            container.Register(() => services.ExternalLoginService);
            container.Register(() => services.FileService);
            container.Register(() => services.MediaService);
            container.Register(() => services.MacroService);
            container.Register(() => services.MemberGroupService);
            container.Register(() => services.MemberService);
            container.Register(() => services.MemberTypeService);
            container.Register(() => services.MigrationEntryService);
            container.Register(() => new OverridingPackagingService(services.PackagingService, services.MacroService, services.DataTypeService, services.ContentTypeService))
                .As<IPackagingService>();
            container.Register(() => new MigrationRunnerService(services.MigrationEntryService, context.ProfilingLogger.Logger, context.DatabaseContext.Database))
                .As<IMigrationRunnerService>();
            container.Register(() => new XmlDocumentService()).As<IXmlDocumentService>();
            container.Register(() => new CreatedPackageService()).As<ICreatedPackageService>();
            container.Register(() => services.UserService);
        }

        class ChauffeurBootManager : CoreBootManager
        {
            public ChauffeurBootManager(UmbracoApplicationBase application)
                : base(application)
            {
            }
        }

        class ChauffeurUmbracoApplication : UmbracoApplicationBase
        {
            protected override IBootManager GetBootManager()
            {
                return new ChauffeurBootManager(this);
            }

            public void Start()
            {
                GetBootManager()
                    .Initialize();
            }
        }
    }
}
