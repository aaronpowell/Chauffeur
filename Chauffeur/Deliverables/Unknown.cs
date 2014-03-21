using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace Chauffeur.Deliverables
{
    public sealed class Unknown : Deliverable
    {
        public Unknown(TextReader reader, TextWriter writer)
            : base(reader, writer)
        {
        }

        public override string Name
        {
            get { return "unknown"; }
        }

        public override IEnumerable<string> Aliases
        {
            get { return Enumerable.Empty<string>(); }
        }

        public override async Task<DeliverableResponse> Run(string[] args)
        {
            await Out.WriteLineAsync("Unknown command entered, check `help` for available commands");
            return await base.Run(args);
        }
    }
}
