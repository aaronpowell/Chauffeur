using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chauffeur.Services.Interfaces
{
    public interface IXmlDocumentWrapper
    {
        XmlDocument LoadDocument(string filePath);
        void SaveDocument(XmlDocument document, string filePath);
    }
}
