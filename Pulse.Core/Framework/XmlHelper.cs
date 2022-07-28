using System.IO;
using System.Xml;

namespace Pulse.Core
{
    public static class XmlHelper
    {
        public static XmlElement CreateDocument(string rootName)
        {
            XmlDocument doc = new();
            
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", null, null);
            doc.AppendChild(dec);

            XmlElement root = doc.CreateElement(rootName);
            doc.AppendChild(root);

            return root;
        }

        public static XmlElement LoadDocument(string xmlPath)
        {
            XmlDocument doc = new();
            doc.Load(xmlPath);

            return doc.GetDocumentElement();
        }

        public static XmlElement TryLoadDocument(string xmlPath)
        {
            if (!File.Exists(xmlPath))
                return null;

            XmlDocument doc = new();
            doc.Load(xmlPath);

            return doc.GetDocumentElement();
        }
    }
}