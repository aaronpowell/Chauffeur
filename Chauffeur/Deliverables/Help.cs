using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Chauffeur.Deliverables
{
    [DeliverableName("help")]
    public class Help : Deliverable
    {
        public Help(TextReader reader, TextWriter writer)
            : base(reader, writer)
        {
        }

        public override IEnumerable<string> Aliases
        {
            get { return new[] { "h" }; }
        }

        public override async Task<DeliverableResponse> Run(string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
