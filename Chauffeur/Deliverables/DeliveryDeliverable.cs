using System.IO;
namespace Chauffeur.Deliverables
{
    [DeliverableName("delivery")]
    [DeliverableAlias("d")]
    public sealed class DeliveryDeliverable : Deliverable
    {
        public DeliveryDeliverable(TextReader reader, TextWriter writer)
            : base(reader, writer)
        {
        }
    }
}
