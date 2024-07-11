using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.Collections {
    /// <summary>
    /// A cursor designed for reading text organized into lines who's position in the source can be moved.
    /// </summary>
    public interface ITextCursor<T>
        : IReadOnlyTextCursor<T>,
            ICursor<T>
        where T : notnull {

        /// <summary>
        /// Reads a whole string from the cursor if the source at the current head matches the given string.
        /// </summary>
        /// <remarks>
        /// <seealso cref="Cursor{T}.Read(T)"/>
        /// </remarks>
        bool Read([NotNull] string match);

        /// <summary>
        /// Reads a whole string from the cursor if the source after the next char matches the given string.
        /// </summary>
        /// <remarks>
        /// <seealso cref="Cursor{T}.ReadNext(T)"/>
        /// </remarks>
        bool ReadNext([NotNull] string match);
    }
}
