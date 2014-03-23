using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Chauffeur.Host
{
    public sealed class ChauffeurSettings
    {
        private readonly TextWriter writer;

        internal ChauffeurSettings(TextWriter writer)
        {
            this.writer = writer;
        }

        public bool TryGetChauffeurDirectory(out string exportDirectory)
        {
            exportDirectory = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, "..", "App_Data", "Chauffeur");

            if (!Directory.Exists(exportDirectory))
            {
                try
                {
                    Directory.CreateDirectory(exportDirectory);
                }
                catch (UnauthorizedAccessException)
                {
                    writer.WriteLine("Chauffer directory 'App_Data\\Chauffeur' cannot be created, check directory permissions");
                    return false;
                }
            }
            return true;
        }
    }
}
