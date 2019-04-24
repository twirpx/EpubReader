using System.IO.Compression;
using System.Threading.Tasks;
using VersOne.Epub.Internal;
using VersOne.Epub.Schema;

namespace VersOne.Epub.Readers {
    internal static class SchemaReader {

        public static EpubSchema ReadSchema(ZipArchive epubArchive) {
            EpubSchema result = new EpubSchema();
            string rootFilePath = RootFilePathReader.GetRootFilePath(epubArchive);
            string contentDirectoryPath = ZipPathUtils.GetDirectoryPath(rootFilePath);
            result.ContentDirectoryPath = contentDirectoryPath;
            EpubPackage package = PackageReader.ReadPackage(epubArchive, rootFilePath);
            result.Package = package;
            result.Epub2Ncx = Epub2NcxReader.ReadEpub2Ncx(epubArchive, contentDirectoryPath, package);
            result.Epub3NavDocument = Epub3NavDocumentReader.ReadEpub3NavDocument(epubArchive, contentDirectoryPath, package);
            return result;
        }

    }
}