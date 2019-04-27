using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chauffeur.Services
{
    public class XmlDocumentWrapper : IXmlDocumentWrapper
    {
        public XmlDocument LoadDocument(string filePath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);

            return xmlDocument;
        }

        public void SaveDocument(XmlDocument document, string filePath)
        {
            document.Save(filePath);
        }
    }
}
