using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;
using VersOne.Epub.Readers;

namespace VersOne.Epub {
    public class EpubBookRef : IDisposable {

        private bool isDisposed;

        public EpubBookRef(ZipArchive epubArchive) {
            EpubArchive = epubArchive;
            isDisposed = false;
        }

        ~EpubBookRef() {
            Dispose(false);
        }

        public string FilePath { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public List<string> AuthorList { get; set; }

        public EpubSchema Schema { get; set; }

        public EpubContentRef Content { get; set; }

        internal ZipArchive EpubArchive { get; private set; }

        public byte[] ReadCover() {
            return BookCoverReader.ReadBookCover(this);
        }

        public List<EpubTextContentFileRef> GetReadingOrder() {
            return SpineReader.GetReadingOrder(this);
        }

        public List<EpubNavigationItemRef> GetNavigation() {
            return NavigationReader.GetNavigationItems(this);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!isDisposed) {
                if (disposing) {
                    EpubArchive?.Dispose();
                }

                isDisposed = true;
            }
        }

    }
}