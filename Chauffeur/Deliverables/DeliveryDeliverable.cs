using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Chauffeur.Deliverables
{
    [DeliverableName("delivery")]
    [DeliverableAlias("d")]
    public sealed class DeliveryDeliverable : Deliverable
    {
        public DeliveryDeliverable(TextReader reader, TextWriter writer)
            : base(reader, writer)
        {
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            if (!args.Any())
            {
                await Out.WriteLineAsync("No deliveries were provided, use `help delivery` to see usage");
                return DeliverableResponse.Continue;
            }

            await Out.WriteLineAsync(@"¯\_(ツ)_/¯");
            return DeliverableResponse.Continue;
        }
    }
}
