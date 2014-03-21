using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Chauffeur.Deliverables
{
    [DeliverableName("quit")]
    [DeliverableAlias("q")]
    public sealed class QuitDeliverable : Deliverable, IProvideDirections
    {
        public QuitDeliverable(TextReader reader, TextWriter writer)
            : base(reader, writer)
        {
        }

        public override async Task<DeliverableResponse> Run(string[] args)
        {
            await Out.WriteLineAsync("Good bye!");
            return await Task.FromResult(DeliverableResponse.Shutdown);
        }

        public async Task Directions()
        {
            await Out.WriteLineAsync("quit");
            await Out.WriteLineAsync("\talias: q");
            await Out.WriteLineAsync("\tTo exit the Chauffeur type `quit` or `q`.");
        }
    }
}
