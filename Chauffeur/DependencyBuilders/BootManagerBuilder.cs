using Chauffeur.Services;
using Umbraco.Core;
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

            container.Register(() => context.DatabaseContext);

            var services = context.Services;
            container.Register(() => services.ContentService);
            container.Register(() => services.ContentTypeService);
            container.Register(() => services.DataTypeService);
            container.Register(() => services.FileService);
            container.Register(() => services.MediaService);
            container.Register(() => services.MacroService);
            container.Register(() => services.MemberGroupService);
            container.Register(() => services.MemberService);
            container.Register(() => services.MemberTypeService);
            container.Register(() => new OverridingPackagingService(services.PackagingService, services.MacroService)).As<IPackagingService>();
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
