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

        public virtual async Task<DeliverableResponse> Run(string command, string[] args)
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

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class DeliverableAliasAttribute : Attribute
    {
        readonly string alias;

        public DeliverableAliasAttribute(string alias)
        {
            this.alias = alias;
        }

        public string Alias
        {
            get { return alias; }
        }
    }

    public enum DeliverableResponse
    {
        Shutdown,
        Continue,
        FinsihedWithError
    }
}
