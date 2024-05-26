namespace Meep.Tech.Text {

    /// <summary>
    /// Helper methods for reading text via cursors.
    /// </summary>
    public static class TextReaderCharEnumerationExtensions {

        /// <summary>
        /// Get an enumerable of characters from a text reader.
        /// </summary>
        public static IEnumerable<char> AsEnumerable(this TextReader reader) {
            while(true) {
                int next = reader.Read();
                if(next == -1) {
                    yield break;
                }

                yield return (char)next;
            }
        }

        /// <summary>
        /// Get an enumerator of chars for a text reader.
        /// </summary>
        public static IEnumerator<char> GetEnumerator(this TextReader reader)
            => reader.AsEnumerable().GetEnumerator();
    }
}
