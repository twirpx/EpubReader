using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VersOne.Epub.Internal;
using VersOne.Epub.Schema;

namespace VersOne.Epub.Readers {
    internal static class BookCoverReader {

        public static async Task<byte[]> ReadBookCoverAsync(EpubBookRef bookRef) {
            List<EpubMetadataMeta> metaItems = bookRef.Schema.Package.Metadata.MetaItems;
            if (metaItems == null || !metaItems.Any()) {
                return null;
            }

            EpubMetadataMeta coverMetaItem = metaItems.FirstOrDefault(metaItem => metaItem.Name.CompareOrdinalIgnoreCase("cover"));
            if (coverMetaItem == null) {
                return null;
            }

            if (String.IsNullOrEmpty(coverMetaItem.Content)) {
                return null;
            }

            EpubManifestItem coverManifestItem = bookRef.Schema.Package.Manifest.FirstOrDefault(manifestItem => manifestItem.Id.CompareOrdinalIgnoreCase(coverMetaItem.Content));
            if (coverManifestItem == null) {
                return null;
            }

            if (bookRef.Content == null) {
                return null;
            }
            
            if (!bookRef.Content.Images.TryGetValue(coverManifestItem.Href, out EpubByteContentFileRef coverImageContentFileRef)) {
                return null;
            }

            byte[] coverImageContent = await coverImageContentFileRef.ReadContentAsBytesAsync().ConfigureAwait(false);
            return coverImageContent;
        }

    }
}