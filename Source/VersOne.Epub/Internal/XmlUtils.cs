using System.IO;
using System.Xml.Linq;

namespace VersOne.Epub.Internal {
    internal static class XmlUtils {

        public static XDocument LoadDocument(Stream stream) {
            using (MemoryStream memoryStream = new MemoryStream()) {
                stream.CopyTo(memoryStream);
                
                memoryStream.Position = 0;

                return XDocument.Load(memoryStream);
            }
        }

    }
}