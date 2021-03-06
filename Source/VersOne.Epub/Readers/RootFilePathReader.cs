﻿using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using VersOne.Epub.Internal;

namespace VersOne.Epub.Readers {
    internal static class RootFilePathReader {

        public static string GetRootFilePath(ZipArchive epubArchive) {
            const string EPUB_CONTAINER_FILE_PATH = "META-INF/container.xml";
            ZipArchiveEntry containerFileEntry = epubArchive.GetEntry(EPUB_CONTAINER_FILE_PATH);
            if (containerFileEntry == null) {
                throw new Exception($"EPUB parsing error: {EPUB_CONTAINER_FILE_PATH} file not found in archive.");
            }

            XDocument containerDocument;
            using (Stream containerStream = containerFileEntry.Open()) {
                containerDocument = XmlUtils.LoadDocument(containerStream);
            }

            XNamespace cnsNamespace = "urn:oasis:names:tc:opendocument:xmlns:container";
            XAttribute fullPathAttribute = containerDocument.Element(cnsNamespace + "container")?.Element(cnsNamespace + "rootfiles")?.Element(cnsNamespace + "rootfile")?.Attribute("full-path");
            if (fullPathAttribute == null) {
                throw new Exception("EPUB parsing error: root file path not found in the EPUB container.");
            }

            return fullPathAttribute.Value;
        }

    }
}