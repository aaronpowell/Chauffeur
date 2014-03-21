using System;
using System.Threading.Tasks;
using Chauffeur.Host;
namespace Chauffeur
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new UmbracoHost(Console.In, Console.Out);

            var result = host.Run();

            Task.WaitAll(result);
        }
    }
}
