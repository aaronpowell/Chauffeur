using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core;

namespace Chauffeur.Deliverables
{
    [DeliverableName("help")]
    [DeliverableAlias("h")]
    public class Help : Deliverable
    {
        public Help(TextReader reader, TextWriter writer)
            : base(reader, writer)
        {
        }

        public override async Task<DeliverableResponse> Run(string[] args)
        {
            var ipd = typeof(IProvideDirections);
            var deliverables = TypeFinder
                .FindClassesOfType<Deliverable>()
                .Where(t => ipd.IsAssignableFrom(t));

            foreach (var deliverable in deliverables)
            {
                var instance = (IProvideDirections)Activator.CreateInstance(deliverable, new object[] { In, Out });
                await instance.Directions();
            }

            return DeliverableResponse.Continue;
        }
    }
}
