using System.Linq;
using System.IO.Abstractions;
namespace Chauffeur
{
    static class IFileSystemExtensions
    {
        public static string Combine(this PathBase pathBase, params string[] paths)
        {
            return paths.Aggregate((path, current) => pathBase.Combine(path, current));
        }
    }
}
