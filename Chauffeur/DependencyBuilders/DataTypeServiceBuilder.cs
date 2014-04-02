using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
namespace Chauffeur.DependencyBuilders
{
    class DataTypeServiceBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<DataTypeService, IDataTypeService>();
        }
    }
}
