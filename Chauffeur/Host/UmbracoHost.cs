using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Chauffeur.Deliverables;
using Umbraco.Core;

namespace Chauffeur.Host
{
    public sealed class UmbracoHost
    {
        private readonly TextReader reader;
        private readonly TextWriter writer;

        private IEnumerable<Type> deliverableTypes;

        public static UmbracoHost Current { get; set; }
        public ChauffeurSettings Settings { get; private set; }

        public UmbracoHost(TextReader reader, TextWriter writer)
        {
            this.reader = reader;
            this.writer = writer;

            Settings = new ChauffeurSettings(writer);
        }

        public async Task Run()
        {
            await writer.WriteLineAsync("Welcome to Chauffeur, your Umbraco console.");
            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            await writer.WriteLineFormattedAsync("You're running Chauffeur v{0} against Umbraco '{1}'", fvi.FileVersion, ConfigurationManager.AppSettings["umbracoConfigurationStatus"]);
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("Type `help` to list the commands and `help <command>` for help for a specific command.");
            await writer.WriteLineAsync();

            deliverableTypes = TypeFinder.FindClassesOfType<Deliverable>();

            var result = DeliverableResponse.Continue;

            while (result == DeliverableResponse.Continue)
            {
                var command = await Prompt();

                var deliverable = Process(command);
                if (deliverable != null)
                    result = await Execute(deliverable);
            }
        }

        private async Task<DeliverableResponse> Execute(ProcessedDeliverable deliverable)
        {
            var toRun = (Deliverable)Activator.CreateInstance(deliverable.DeliverableType, new object[] { reader, writer });

            try
            {
                return await toRun.Run(deliverable.Args);
            }
            catch (Exception ex)
            {
                writer.WriteLine("Error running the current deliverable: " + ex.Message);
                return DeliverableResponse.Continue;
            }
        }

        private ProcessedDeliverable Process(string command)
        {
            if (string.IsNullOrEmpty(command))
                return null;

            var args = command.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            var what = args[0].ToLower();

            var deliverableType = deliverableTypes
                .FirstOrDefault(d => string.Compare(d.GetCustomAttribute<DeliverableNameAttribute>(false).Name, what, StringComparison.InvariantCultureIgnoreCase) == 0);

            if (deliverableType == null)
            {
                deliverableType = deliverableTypes
                    .FirstOrDefault(d => d.GetCustomAttributes<DeliverableAliasAttribute>(false).Any(a => string.Compare(a.Alias, what, StringComparison.InvariantCultureIgnoreCase) == 0));

                if (deliverableType == null)
                    deliverableType = typeof(UnknownDeliverable);
                else
                    args = args.Skip(1).ToArray();
            }
            else
                args = args.Skip(1).ToArray();

            return new ProcessedDeliverable
                        {
                            DeliverableType = deliverableType,
                            Args = args
                        };
        }

        private async Task<string> Prompt()
        {
            await writer.WriteAsync("umbraco> ");
            return await reader.ReadLineAsync();
        }

        private class ProcessedDeliverable
        {
            public Type DeliverableType { get; set; }
            public string[] Args { get; set; }
        }
    }
}
