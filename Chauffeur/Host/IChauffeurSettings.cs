using System;
using System.Configuration;
namespace Chauffeur.Host
{
    public interface IChauffeurSettings
    {
        bool TryGetChauffeurDirectory(out string exportDirectory);
        ConnectionStringSettings ConnectionString { get; }
    }
}
