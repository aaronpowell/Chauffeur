using System;
namespace Chauffeur.Host
{
    public interface IChauffeurSettings
    {
        bool TryGetChauffeurDirectory(out string exportDirectory);
    }
}
