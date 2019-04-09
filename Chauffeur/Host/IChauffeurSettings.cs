using System.Configuration;

namespace Chauffeur.Host
{
    public interface IChauffeurSettings
    {
        bool TryGetChauffeurDirectory(out string exportDirectory);
        bool TryGetSiteRootDirectory(out string siteRootDirectory);
        bool TryGetUmbracoDirectory(out string umbracoDirectory);
        ConnectionStringSettings ConnectionString { get; }
        string UmbracoVersion { get; }
        string ChauffeurVersion { get; }
    }
}
