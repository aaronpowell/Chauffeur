using Chauffeur.Host;

namespace Chauffeur.DependencyBuilders
{
    class ChauffeurSettingBuilder : IBuildDependencies
    {
        public void Build(IContainer container)
        {
            container.Register<ChauffeurSettings, IChauffeurSettings>();
        }
    }
}
