using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Chauffeur.DependencyBuilders
{
    class ServicesBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<ContentService, IContentService>();
            container.Register<ContentTypeService, IContentTypeService>();
            container.Register<DataTypeService, IDataTypeService>();
            container.Register<MediaService, IMediaService>();
            container.Register<FileService, IFileService>();

            container.Register<IPackagingService>(() => new PackagingService(
                    container.Resolve<IContentService>(),
                    container.Resolve<IContentTypeService>(),
                    container.Resolve<IMediaService>(),
                    null,
                    container.Resolve<IDataTypeService>(),
                    container.Resolve<IFileService>(),
                    null,
                    container.Resolve<RepositoryFactory>(),
                    container.Resolve<IDatabaseUnitOfWorkProvider>()
                )
            );

            var type = typeof(LegacyPropertyEditorIdToAliasConverter);
            var method = type.GetMethod("CreateMappingsForCoreEditors", BindingFlags.Static | BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(null, null);
        }
    }
}
