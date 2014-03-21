using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Chauffeur
{
    public abstract class Deliverable
    {
        protected Deliverable(TextReader reader, TextWriter writer)
        {
            In = reader;
            Out = writer;
        }

        protected TextReader In { get; private set; }
        protected TextWriter Out { get; private set; }

        public abstract string Name { get; }
        public abstract IEnumerable<string> Aliases { get; }

        public virtual async Task<DeliverableResponse> Run(string[] args)
        {
            return await Task.FromResult(DeliverableResponse.Continue);
        }
    }

    public enum DeliverableResponse
    {
        Shutdown,
        Continue
    }
}
