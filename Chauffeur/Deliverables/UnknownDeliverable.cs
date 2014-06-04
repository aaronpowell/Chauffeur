using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Chauffeur.Deliverables
{
    [DeliverableName("unknown")]
    public sealed class UnknownDeliverable : Deliverable
    {
        public UnknownDeliverable(TextReader reader, TextWriter writer)
            : base(reader, writer)
        {
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            await Out.WriteLineFormattedAsync("Unknown command '{0}' entered, check `help` for available commands", string.Join(" ", new[] { command }.Concat(args)));
            return await base.Run(command, args);
        }
    }
}
