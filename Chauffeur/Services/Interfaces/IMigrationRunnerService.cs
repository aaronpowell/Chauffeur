using Semver;

namespace Chauffeur.Services.Interfaces
{
    public interface IMigrationRunnerService
    {
        bool Execute(SemVersion from, SemVersion to);
    }
}
