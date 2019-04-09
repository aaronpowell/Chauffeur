using System.IO;
using System.Threading.Tasks;

namespace Chauffeur
{
    public static class TextReaderExtensions
    {
        public static async Task<string> ReadLineWithDefaultAsync(this TextReader reader, string @default)
        {
            var input = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(input))
                return @default;

            return input;
        }
    }
}
