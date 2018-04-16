using Microsoft.AspNet.Identity.Owin;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Chauffeur.DependencyBuilders
{
    class UserManagerBuilder : IBuildDependencies
    {
        public void Build(IContainer container)
        {
            container.Register(() =>
            {
                var userManager = BackOfficeUserManager.Create(
                    new IdentityFactoryOptions<BackOfficeUserManager>
                    {
                        Provider = new IdentityFactoryProvider<BackOfficeUserManager>()
                    },
                    container.Resolve<IUserService>(),
                    container.Resolve<IEntityService>(),
                    container.Resolve<IExternalLoginService>(),
                    MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider(),
                    UmbracoConfig.For.UmbracoSettings().Content
                );

                return userManager;
            })
            .As<BackOfficeUserManager<BackOfficeIdentityUser>>();
        }
    }
}
