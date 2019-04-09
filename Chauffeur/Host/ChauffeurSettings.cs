using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Umbraco.Core.IO;
using IFileSystem = System.IO.Abstractions.IFileSystem;

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
            exportDirectory = string.Empty;

            var rootFolder = (string)AppDomain.CurrentDomain.GetData("DataDirectory");

            exportDirectory = fileSystem.Path.Combine(rootFolder, "Chauffeur");

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

        public bool TryGetSiteRootDirectory(out string siteRootDirectory)
        {
            siteRootDirectory = fileSystem.Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, "..");

            return fileSystem.Directory.Exists(siteRootDirectory);
        }

        public bool TryGetUmbracoDirectory(out string umbracoDirectory)
        {
            umbracoDirectory = string.Empty;

            string rootFolder;
            if (!TryGetSiteRootDirectory(out rootFolder))
                return false;

            umbracoDirectory = fileSystem.Path.Combine(rootFolder, SystemDirectories.Umbraco.Replace("~/", string.Empty));

            return fileSystem.Directory.Exists(umbracoDirectory);
        }

        public ConnectionStringSettings ConnectionString =>
            ConfigurationManager.ConnectionStrings["umbracoDbDSN"];

        public string UmbracoVersion =>
            ConfigurationManager.AppSettings["umbracoConfigurationStatus"];

        public string ChauffeurVersion =>
            FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ToString();
    }
}
