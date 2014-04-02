using System.IO;
using System.IO.Abstractions;
using Chauffeur.Host;
namespace Chauffeur.DependencyBuilders
{
    class ChauffeurSettingBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<ChauffeurSettings, IChauffeurSettings>();
        }
    }
}
