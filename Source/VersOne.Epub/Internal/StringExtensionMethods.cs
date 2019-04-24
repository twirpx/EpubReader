using System;

namespace VersOne.Epub.Internal {
    internal static class StringExtensionMethods {

        public static bool CompareOrdinalIgnoreCase(this string source, string value) {
            return String.Compare(source, value, StringComparison.OrdinalIgnoreCase) == 0;
        }

    }
}