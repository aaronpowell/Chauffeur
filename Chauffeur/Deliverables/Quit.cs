using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Chauffeur.Deliverables
{
    [DeliverableName("quit")]
    public sealed class Quit : Deliverable
    {
        public Quit(TextReader reader, TextWriter writer)
            : base(reader, writer)
        {
        }

        public override IEnumerable<string> Aliases
        {
            get { return new[] { "q" }; }
        }

        public override async Task<DeliverableResponse> Run(string[] args)
        {
            await Out.WriteLineAsync("Good bye!");
            return await Task.FromResult(DeliverableResponse.Shutdown);
        }
    }
}
