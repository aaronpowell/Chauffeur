using System.IO;
using System.Threading.Tasks;
namespace Chauffeur
{
    static class TextWriterExtensions
    {
        public static async Task WriteLineFormattedAsync(this TextWriter writer, string format, params object[] arguments)
        {
            await writer.WriteLineAsync(string.Format(format, arguments));
        }
    }
}
