using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Chauffeur.Services;
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
            container.Register<MacroService, IMacroService>();

            container.Register<PackagingService>();

            container.Register<OverridingPackagingService, IPackagingService>();

            var type = typeof(LegacyPropertyEditorIdToAliasConverter);
            var method = type.GetMethod("CreateMappingsForCoreEditors", BindingFlags.Static | BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(null, null);
        }
    }
}
