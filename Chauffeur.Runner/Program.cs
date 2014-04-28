using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Chauffeur.Host;

namespace Chauffeur.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ConfigurationManager.ConnectionStrings["umbracoDbDSN"] == null)
            {
                var path = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;

                var webConfigPath = Path.Combine(path, "..", "web.config");

                var domain = AppDomain.CreateDomain(
                    "umbraco-domain",
                    AppDomain.CurrentDomain.Evidence,
                    new AppDomainSetup
                    {
                        ConfigurationFile = webConfigPath
                    });

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        domain.Load(assembly.FullName);
                    }
                    catch (FileNotFoundException)
                    {
                        //Console.WriteLine("Failed to load {0}", assembly.FullName);
                    }
                }

                domain.SetData("DataDirectory", Path.Combine(path, "..", "App_Data"));

                var thisAssembly = new FileInfo(Assembly.GetExecutingAssembly().Location);
                domain.ExecuteAssembly(thisAssembly.Name, args);
            }
            else
            {
                var host = new UmbracoHost(Console.In, Console.Out);
                UmbracoHost.Current = host;
                if (args.Any())
                    host.Run(args);
                else
                    host.Run();
            }
        }
    }
}
