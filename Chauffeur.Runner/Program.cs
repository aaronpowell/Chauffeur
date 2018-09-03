using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
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

                var webConfigPath = Path.Combine(path, "..", "Web.config");

                var domain = AppDomain.CreateDomain(
                    "umbraco-domain",
                    AppDomain.CurrentDomain.Evidence,
                    new AppDomainSetup
                    {
                        ConfigurationFile = webConfigPath
                    });

                domain.AssemblyResolve += AssemblyResolveHacks;

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
                domain.SetData(".appDomain", "From Domain");
                domain.SetData(".appId", "From Domain");
                Thread.GetDomain().SetData(".appDomain", "From Thread");
                Thread.GetDomain().SetData(".appId", "From Thread");

                var thisAssembly = new FileInfo(Assembly.GetExecutingAssembly().Location);
                domain.ExecuteAssembly(thisAssembly.FullName, args);
            }
            else
            {
                using (var host = new UmbracoHost(Console.In, Console.Out))
                {
                    UmbracoHost.Current = host;
                    if (args.Any())
                        host.Run(args).Wait();
                    else
                        host.Run().Wait();
                }
            }
        }

        // Relicating the functionality of https://github.com/umbraco/Umbraco-CMS/blob/72cc5ce88bc92d5f487a83225e40f3f2a457c933/src/Umbraco.Core/BindingRedirects.cs
        // This should address https://github.com/aaronpowell/Chauffeur/issues/67
        private static readonly Regex Log4NetAssemblyPattern = new Regex("log4net, Version=([\\d\\.]+?), Culture=neutral, PublicKeyToken=\\w+$", RegexOptions.Compiled);
        private const string Log4NetReplacement = "log4net, Version=2.0.8.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a";

        private static Assembly AssemblyResolveHacks(object sender, ResolveEventArgs args)
        {
            //log4net:
            if (Log4NetAssemblyPattern.IsMatch(args.Name) && args.Name != Log4NetReplacement)
                return Assembly.Load(Log4NetAssemblyPattern.Replace(args.Name, Log4NetReplacement));

            //AutoMapper:
            // ensure the assembly is indeed AutoMapper and that the PublicKeyToken is null before trying to Load again
            // do NOT just replace this with 'return Assembly', as it will cause an infinite loop -> stackoverflow
            if (args.Name.StartsWith("AutoMapper") && args.Name.EndsWith("PublicKeyToken=null"))
                return Assembly.Load(args.Name.Replace(", PublicKeyToken=null", ", PublicKeyToken=be96cd2c38ef1005"));

            return null;
        }
    }
}
