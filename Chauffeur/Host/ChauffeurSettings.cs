using System;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Threading.Tasks;

namespace Chauffeur.Host
{
    class ChauffeurSettings : IChauffeurSettings
    {
        private readonly TextWriter writer;
        private readonly IFileSystem fileSystem;

        public ChauffeurSettings(TextWriter writer, IFileSystem fileSystem)
        {
            this.writer = writer;
            this.fileSystem = fileSystem;
        }

        public bool TryGetChauffeurDirectory(out string exportDirectory)
        {
            exportDirectory = fileSystem.Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, "..", "App_Data", "Chauffeur");

            if (!fileSystem.Directory.Exists(exportDirectory))
            {
                try
                {
                    fileSystem.Directory.CreateDirectory(exportDirectory);
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
