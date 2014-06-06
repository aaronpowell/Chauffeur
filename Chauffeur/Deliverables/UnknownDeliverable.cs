using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Chauffeur.Deliverables
{
    [DeliverableName("unknown")]
    public sealed class UnknownDeliverable : Deliverable, IProvideDirections
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

        public async Task Directions()
        {
            await Out.WriteLineAsync("Seriously, you're asking for help on the unknown command? Good luck with that");
        }
    }
}
