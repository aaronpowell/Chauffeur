using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Chauffeur.Deliverables.Tests
{
    class MockTextWriter : TextWriter
    {
        private readonly List<string> messages;
        public MockTextWriter()
        {
            messages = new List<string>();
        }

        public IEnumerable<string> Messages { get { return messages; } }

        public override System.Text.Encoding Encoding { get { return System.Text.Encoding.Default; } }

        public override async Task WriteLineAsync(string value)
        {
            messages.Add(value);
            await Task.FromResult(value);
        }
    }
}
