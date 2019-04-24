using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using VersOne.Epub.Readers;

namespace VersOne.Epub {
    public static class EpubReader {

        /// <summary>
        /// Opens the book synchronously without reading its whole content. Holds the handle to the EPUB file.
        /// </summary>
        /// <param name="filePath">path to the EPUB file</param>
        /// <returns></returns>
        public static EpubBookRef OpenBook(string filePath) {
            return OpenBook(GetZipArchive(filePath), filePath);
        }

        /// <summary>
        /// Opens the book synchronously without reading its whole content.
        /// </summary>
        /// <param name="stream">seekable stream containing the EPUB file</param>
        /// <returns></returns>
        public static EpubBookRef OpenBook(Stream stream) {
            return OpenBook(GetZipArchive(stream), null);
        }

        /// <summary>
        /// Opens the book synchronously and reads all of its content into the memory. Does not hold the handle to the EPUB file.
        /// </summary>
        /// <param name="filePath">path to the EPUB file</param>
        /// <returns></returns>
        public static EpubBook ReadBook(string filePath) {
            return ReadBook(OpenBook(filePath));
        }

        /// <summary>
        /// Opens the book synchronously and reads all of its content into the memory.
        /// </summary>
        /// <param name="stream">seekable stream containing the EPUB file</param>
        /// <returns></returns>
        public static EpubBook ReadBook(Stream stream) {
            return ReadBook(OpenBook(stream));
        }

        private static EpubBookRef OpenBook(ZipArchive zipArchive, string filePath) {
            EpubBookRef result = null;
            try {
                result = new EpubBookRef(zipArchive);
                result.FilePath = filePath;
                result.Schema = SchemaReader.ReadSchema(zipArchive);
                result.Title = result.Schema.Package.Metadata.Titles.FirstOrDefault() ?? String.Empty;
                result.AuthorList = result.Schema.Package.Metadata.Creators.Select(creator => creator.Creator).ToList();
                result.Author = String.Join(", ", result.AuthorList);
                result.Content = ContentReader.ParseContentMap(result);
                return result;
            } catch {
                result?.Dispose();
                throw;
            }
        }

        private static EpubBook ReadBook(EpubBookRef epubBookRef) {
            EpubBook result = new EpubBook();
            
            using (epubBookRef) {
                result.FilePath = epubBookRef.FilePath;
                result.Schema = epubBookRef.Schema;
                result.Title = epubBookRef.Title;
                result.AuthorList = epubBookRef.AuthorList;
                result.Author = epubBookRef.Author;
                result.Content = ReadContent(epubBookRef.Content);
                result.CoverImage = epubBookRef.ReadCover();
                List<EpubTextContentFileRef> htmlContentFileRefs = epubBookRef.GetReadingOrder();
                result.ReadingOrder = ReadReadingOrder(result, htmlContentFileRefs);
                List<EpubNavigationItemRef> navigationItemRefs = epubBookRef.GetNavigation();
                result.Navigation = ReadNavigation(result, navigationItemRefs);
            }

            return result;
        }

        private static ZipArchive GetZipArchive(string filePath) {
            return ZipFile.OpenRead(filePath);
        }

        private static ZipArchive GetZipArchive(Stream stream) {
            return new ZipArchive(stream, ZipArchiveMode.Read);
        }

        private static EpubContent ReadContent(EpubContentRef contentRef) {
            EpubContent result = new EpubContent();
            result.Html = ReadTextContentFiles(contentRef.Html);
            result.Css = ReadTextContentFiles(contentRef.Css);
            result.Images = ReadByteContentFiles(contentRef.Images);
            result.Fonts = ReadByteContentFiles(contentRef.Fonts);
            result.AllFiles = new Dictionary<string, EpubContentFile>();
            foreach (KeyValuePair<string, EpubTextContentFile> textContentFile in result.Html.Concat(result.Css)) {
                result.AllFiles.Add(textContentFile.Key, textContentFile.Value);
            }

            foreach (KeyValuePair<string, EpubByteContentFile> byteContentFile in result.Images.Concat(result.Fonts)) {
                result.AllFiles.Add(byteContentFile.Key, byteContentFile.Value);
            }

            foreach (KeyValuePair<string, EpubContentFileRef> contentFileRef in contentRef.AllFiles) {
                if (!result.AllFiles.ContainsKey(contentFileRef.Key)) {
                    result.AllFiles.Add(contentFileRef.Key, ReadByteContentFile(contentFileRef.Value));
                }
            }

            return result;
        }

        private static Dictionary<string, EpubTextContentFile> ReadTextContentFiles(Dictionary<string, EpubTextContentFileRef> textContentFileRefs) {
            Dictionary<string, EpubTextContentFile> result = new Dictionary<string, EpubTextContentFile>();
            foreach (KeyValuePair<string, EpubTextContentFileRef> textContentFileRef in textContentFileRefs) {
                EpubTextContentFile textContentFile = new EpubTextContentFile {
                    FileName = textContentFileRef.Value.FileName,
                    ContentType = textContentFileRef.Value.ContentType,
                    ContentMimeType = textContentFileRef.Value.ContentMimeType
                };
                textContentFile.Content = textContentFileRef.Value.ReadContentAsText();
                result.Add(textContentFileRef.Key, textContentFile);
            }

            return result;
        }

        private static Dictionary<string, EpubByteContentFile> ReadByteContentFiles(Dictionary<string, EpubByteContentFileRef> byteContentFileRefs) {
            Dictionary<string, EpubByteContentFile> result = new Dictionary<string, EpubByteContentFile>();
            foreach (KeyValuePair<string, EpubByteContentFileRef> byteContentFileRef in byteContentFileRefs) {
                result.Add(byteContentFileRef.Key, ReadByteContentFile(byteContentFileRef.Value));
            }

            return result;
        }

        private static EpubByteContentFile ReadByteContentFile(EpubContentFileRef contentFileRef) {
            EpubByteContentFile result = new EpubByteContentFile {
                FileName = contentFileRef.FileName,
                ContentType = contentFileRef.ContentType,
                ContentMimeType = contentFileRef.ContentMimeType
            };
            result.Content = contentFileRef.ReadContentAsBytes();
            return result;
        }

        private static List<EpubTextContentFile> ReadReadingOrder(EpubBook epubBook, List<EpubTextContentFileRef> htmlContentFileRefs) {
            return htmlContentFileRefs.Select(htmlContentFileRef => epubBook.Content.Html[htmlContentFileRef.FileName]).ToList();
        }

        private static List<EpubNavigationItem> ReadNavigation(EpubBook epubBook, List<EpubNavigationItemRef> navigationItemRefs) {
            List<EpubNavigationItem> result = new List<EpubNavigationItem>();
            foreach (EpubNavigationItemRef navigationItemRef in navigationItemRefs) {
                EpubNavigationItem navigationItem = new EpubNavigationItem(navigationItemRef.Type) {
                    Title = navigationItemRef.Title,
                    Link = navigationItemRef.Link,
                };
                if (navigationItemRef.HtmlContentFileRef != null) {
                    navigationItem.HtmlContentFile = epubBook.Content.Html[navigationItemRef.HtmlContentFileRef.FileName];
                }

                navigationItem.NestedItems = ReadNavigation(epubBook, navigationItemRef.NestedItems);
                result.Add(navigationItem);
            }

            return result;
        }

    }
}