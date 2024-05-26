namespace Meep.Tech.Data {
    public static class CursorExtensions {
        public static T? Next<T>(this Cursor<T> cursor)
            where T : notnull
            => cursor.Move(1)
                ? cursor.Current
                : default;

        public static T? Previous<T>(this Cursor<T> cursor)
            where T : notnull
            => cursor.Move(-1)
                ? cursor.Current
                : default;
    }
}