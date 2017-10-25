using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
namespace Chauffeur
{
    static class TextWriterExtensions
    {
        public static async Task WriteLineFormattedAsync(this TextWriter writer, string format, params object[] arguments)
        {
            await writer.WriteLineAsync(string.Format(format, arguments));
        }

        public static Task WriteTableAsync<T>(this TextWriter writer, T row) =>
            writer.WriteTableAsync(new[] { row }.AsEnumerable(), new Dictionary<string, string>());

        public static Task WriteTableAsync<T>(this TextWriter writer, T row, IDictionary<string, string> columnMappings) =>
            writer.WriteTableAsync(new[] { row }.AsEnumerable(), columnMappings);

        public static Task WriteTableAsync<T>(this TextWriter writer, IEnumerable<T> rows) =>
            writer.WriteTableAsync(rows, new Dictionary<string, string>());

        public static async Task WriteTableAsync<T>(this TextWriter writer, IEnumerable<T> rows, IDictionary<string, string> columnMappings)
        {
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var headerRow = properties.Select(p => columnMappings.ContainsKey(p.Name) ? columnMappings[p.Name] : p.Name).ToArray();
            var dataRows = rows.Select(row =>
            {
                var rowProperties = new List<string>();
                foreach (var property in properties)
                    rowProperties.Add(property.GetValue(row).ToString());

                return rowProperties.ToArray();
            }).ToArray();

            var totalRows = new[] { headerRow }.Concat(dataRows);

            var lengths = new List<int>();
            for (int i = 0; i < headerRow.Length; i++)
            {
                var dataItems = totalRows.Select(x => x[i]);
                lengths.Add(dataItems.Select(x => x.Length).Max());
            }

            foreach (var dataRow in totalRows)
            {
                await writer.WriteLineAsync(
                    string.Join(" | ", dataRow.Select((row, index) => row.PadRight(lengths[index], ' ')))
                );
            }
        }
    }
}
