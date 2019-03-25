using System.Threading.Tasks;

namespace Chauffeur.Deliverables.Tests
{
    public static class NSubstituteHelper
    {
        public static void IgnoreAwaitForNSubstituteAssertion(this Task task)
        {
        }
    }

}
