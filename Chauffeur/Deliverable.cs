using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Chauffeur
{
    public abstract class Deliverable
    {
        protected Deliverable(TextReader reader, TextWriter writer)
        {
            In = reader;
            Out = writer;
        }

        protected TextReader In { get; private set; }
        protected TextWriter Out { get; private set; }

        public abstract IEnumerable<string> Aliases { get; }

        public virtual async Task<DeliverableResponse> Run(string[] args)
        {
            return await Task.FromResult(DeliverableResponse.Continue);
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DeliverableNameAttribute : System.Attribute
    {
        readonly string name;
        public DeliverableNameAttribute(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return name; }
        }
    }

    public enum DeliverableResponse
    {
        Shutdown,
        Continue
    }
}
