using System.Threading.Tasks;

namespace Chauffeur.Host
{
    public interface IChauffeurHost
    {
        Task<DeliverableResponse> Run();
        Task<DeliverableResponse> Run(string[] args);
    }
}
