using Semver;

namespace Chauffeur.Services
{
	public interface IMigrationRunnerService
	{
		bool Execute(SemVersion from, SemVersion to);
	}
}
