using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Chauffeur.Host;
using Umbraco.Core;

namespace Chauffeur.Deliverables
{
    [DeliverableName("help")]
    [DeliverableAlias("h")]
    [DeliverableAlias("?")]
    public class HelpDeliverable : Deliverable, IProvideDirections
    {
        private readonly IChauffeurHost host;

        public HelpDeliverable(TextReader reader, TextWriter writer, IChauffeurHost host)
            : base(reader, writer)
        {
            this.host = host;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            if (args.Any())
                await Print(args[0]);
            else
                await PrintAll();

            return DeliverableResponse.Continue;
        }

        private async Task Print(string command)
        {
            var deliverable = UmbracoHost.Current.Container.ResolveDeliverableByName(command) as IProvideDirections;
            if (deliverable != null)
            {
                await deliverable.Directions();
                return;
            }

            await Out.WriteLineFormattedAsync(
                "The command '{0}' doesn't implement help, you best contact the author",
                command
            );

        }

        private async Task PrintAll()
        {
            var deliverables = UmbracoHost.Current.Container.ResolveAllDeliverables();
            await Out.WriteLineAsync("The following deliverables are loaded. Use `help <deliverable>` for detailed help");

            foreach (var deliverable in deliverables)
            {
                var type = deliverable.GetType();
                var name = type.GetCustomAttribute<DeliverableNameAttribute>(false).Name;
                var aliases = type.GetCustomAttributes<DeliverableAliasAttribute>(false).Select(a => a.Alias);

                await Out.WriteLineFormattedAsync(
                    "{0}{1}",
                    name,
                    aliases.Any() ? string.Format(" (aliases: {0})", string.Join(", ", aliases)) : string.Empty
                );
            }
        }

        public async Task Directions()
        {
            await Out.WriteLineAsync("help");
            await Out.WriteLineAsync("\taliases: h, ?");
            await Out.WriteLineAsync("\tUse `help` to display system help");
            await Out.WriteLineAsync("\tUse `help <Deliverable>` to display help for a deliverable");
        }
    }
}
