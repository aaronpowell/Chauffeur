using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Chauffeur.Deliverables
{
    [DeliverableName("unknown")]
    public sealed class Unknown : Deliverable
    {
        public Unknown(TextReader reader, TextWriter writer)
            : base(reader, writer)
        {
        }

        public override IEnumerable<string> Aliases
        {
            get { return Enumerable.Empty<string>(); }
        }

        public override async Task<DeliverableResponse> Run(string[] args)
        {
            await Out.WriteLineAsync(string.Format("Unknown command '{0}' entered, check `help` for available commands", string.Join(" ", args)));
            return await base.Run(args);
        }
    }
}
