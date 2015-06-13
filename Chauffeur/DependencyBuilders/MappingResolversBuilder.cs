using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Persistence.Mappers;

namespace Chauffeur.DependencyBuilders
{
    class MappingResolversBuilder : IBuildDependencies
    {
        public void Build(IContainer container)
        {
            SetupMappingResolver();
        }

        private static void SetupMappingResolver()
        {
            var umbracoCore = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.Contains("Umbraco.Core"));

            var mappingResolver = umbracoCore.GetTypes().First(t => t.FullName == "Umbraco.Core.Persistence.Mappers.MappingResolver");

            var ctor = mappingResolver.GetConstructor(new[] { typeof(Func<IEnumerable<Type>>) });

            var currentProperty = mappingResolver.GetProperty("Current", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            var assignedMappingTypes = TypeFinder.FindClassesOfType<BaseMapper>();

            Func<IEnumerable<Type>> fn = () => assignedMappingTypes;

            var memberResolverInstance = ctor.Invoke(new object[] { fn });

            currentProperty.SetValue(null, memberResolverInstance);
        }
    }
}
