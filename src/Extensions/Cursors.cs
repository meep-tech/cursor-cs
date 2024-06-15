namespace Meep.Tech.Data {

    /// <summary>
    /// Utility methods for Cursors.
    /// </summary>
    public static class CursorExtensions {

        /// <summary>
        /// Move the cursor head to the next element in the source, and return the new current element.
        /// </summary>
        public static T? Next<T>(this ICursor<T> cursor)
            where T : notnull
            => cursor.Move(1)
                ? cursor.Current
                : default;

        /// <summary>
        /// Move the cursor head to the previous element in the source, and return the new current element.
        /// </summary>
        public static T? Previous<T>(this ICursor<T> cursor)
            where T : notnull
            => cursor.Move(-1)
                ? cursor.Current
                : default;
    }
}