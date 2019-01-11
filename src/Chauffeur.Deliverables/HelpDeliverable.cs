using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Chauffeur.Deliverables
{
    [DeliverableName("help")]
    [DeliverableAlias("h")]
    [DeliverableAlias("?")]
    public class HelpDeliverable : Deliverable, IProvideDirections
    {
        private readonly IFactory container;

        public HelpDeliverable(TextReader reader, TextWriter writer, IFactory container)
            : base(reader, writer)
        {
            this.container = container;
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
            if (container.GetInstance<Deliverable>() is IProvideDirections deliverable)
            {
                await deliverable.Directions();
                return;
            }

            await Out.WriteLineAsync(
                $"The deliverable '{command}' doesn't implement help, you best contact the author"
            );
        }

        private async Task PrintAll()
        {
            await Out.WriteLineAsync("The following deliverables are loaded. Use `help <deliverable>` for detailed help");

            var toWrite = new List<string>();
            var deliverables = container.GetAllInstances<Deliverable>().Distinct(new DeliverableComparer());

            foreach (var deliverable in deliverables)
            {
                var type = deliverable.GetType();
                var name = type.GetCustomAttribute<DeliverableNameAttribute>(false).Name;
                var aliases = type.GetCustomAttributes<DeliverableAliasAttribute>(false).Select(a => a.Alias);

                toWrite.Add(
                    string.Format(
                        "{0}{1}",
                        name,
                        aliases.Any() ? string.Format(" (aliases: {0})", string.Join(", ", aliases)) : string.Empty
                    )
                );
            }

            await Task.WhenAll(toWrite.OrderBy(s => s).Select(Out.WriteLineAsync).ToArray());
        }

        public async Task Directions()
        {
            await Out.WriteLineAsync("help");
            await Out.WriteLineAsync("\taliases: h, ?");
            await Out.WriteLineAsync("\tUse `help` to display system help");
            await Out.WriteLineAsync("\tUse `help <Deliverable>` to display help for a deliverable");
        }

        class DeliverableComparer : IEqualityComparer<Deliverable>
        {
            public bool Equals(Deliverable x, Deliverable y)
            {
                return x.GetType() == y.GetType();
            }

            public int GetHashCode(Deliverable obj)
            {
                return obj.GetType().GetHashCode();
            }
        }
    }
}
