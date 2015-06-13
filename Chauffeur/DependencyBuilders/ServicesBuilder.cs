using System.Reflection;
using System.Web.Security;
using Chauffeur.Services;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Publishing;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Chauffeur.DependencyBuilders
{
    class ServicesBuilder : IBuildDependencies
    {
        public void Build(IContainer container)
        {
            container.Register<ContentService, IContentService>();
            container.Register<ContentTypeService, IContentTypeService>();
            container.Register<DataTypeService, IDataTypeService>();
            container.Register<MediaService, IMediaService>();
            container.Register<FileService, IFileService>();
            container.Register<MacroService, IMacroService>();
            container.Register<MemberGroupService, IMemberGroupService>();
            container.Register<MemberService, IMembershipMemberService<IMember>>().As<IMemberService>();
            container.Register<UserService, IMembershipMemberService<IUser>>().As<IUserService>();

            container.Register<PublishingStrategy, IPublishingStrategy>();

            container.Register<PackagingService>();

            container.Register<OverridingPackagingService, IPackagingService>();

            var type = typeof(LegacyPropertyEditorIdToAliasConverter);
            var method = type.GetMethod("CreateMappingsForCoreEditors", BindingFlags.Static | BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(null, null);

            container.Register(() => Membership.Providers["UsersMembershipProvider"] as UmbracoMembershipProviderBase);
        }
    }
}
