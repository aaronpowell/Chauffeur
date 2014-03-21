using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Chauffeur.Deliverables
{
    public class Help : Deliverable
    {
        public Help(TextReader reader, TextWriter writer)
            : base(reader, writer)
        {
        }

        public override string Name
        {
            get { return "help"; }
        }

        public override IEnumerable<string> Aliases
        {
            get { return new[] { "h" }; }
        }

        public virtual async Task<DeliverableResponse> Run(string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
