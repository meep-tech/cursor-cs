using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.Collections {

    /// <summary>
    /// A Read-Only interface for a Cursor.
    /// <para>Provides a way to peek at and view the source around the current cursor head without being able to move it.</para>
    /// </summary>
    public interface IReadOnlyCursor<T>
        : IReadOnlyList<T>,
            ICloneable
        where T : notnull {

        /// <summary>
        /// The current index of the cursor 'Head' within the source.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// The current location of the cursor within the source.
        /// </summary>
        Cursor.ILocation Position { get; }

        /// <summary>
        /// If the cursor has ever been moved from the start of the source.
        /// </summary>
        bool HasMoved { get; }

        /// <summary>
        /// If the cursor head is at the start of the source.
        /// </summary>
        bool IsAtStart { get; }

        /// <summary>
        /// Checks if the cursor head is at the end of the source.
        /// </summary>
        bool IsAtEnd { get; }

        /// <summary>
        /// Checks if the cursor has reached the end of the source.
        /// </summary>
        bool HasReachedEnd { get; }

        /// <summary>
        /// The previous element in the cursor.
        /// </summary>
        T? Previous { get; }

        /// <summary>
        /// The element at the current cursor position.
        /// </summary>
        T Current { get; }

        /// <summary>
        /// Peek at the element after the current cursor head within the source.
        /// </summary>
        T? Next { get; }

        /// <summary>
        /// The buffer of characters the cursor has read so far.
        /// </summary>
        IReadOnlyList<T> Memory { get; }

        /// <summary>
        /// The number of pre-read elements in the memory buffer (compared to the current cursor position).
        /// </summary>
        int Buffer { get; }

        /// <summary>
        /// Checks if this cursor was provided an empty source.
        /// </summary>
        bool SourceIsEmpty { get; }

        /// <summary>
        /// Used to peek around the current head of the cursor.
        /// </summary>
        /// <returns>The element at the specified index relative to the current position. 0 = current, -1 = previous, 1 = next, etc.</returns>
        new T? this[int index] { get; }

        /// <summary>
        /// Used to peek at an element further ahead in the source without moving the current head position of the cursor.
        /// </summary>
        /// <param name="offset">The number of elements to peek ahead (or behind).</param>
        /// <returns>The element X positions ahead of the current position, or null if there isn't one</returns>
        T? Peek(int offset);

        /// <summary><inheritdoc cref="Peek(int)" path="/summary"/></summary>
        /// <param name="offset"><inheritdoc cref="Peek(int)" path="/param[@name='offset']"/></param>
        /// <param name="peek">The element X positions ahead of the current position, or null if there isn't one</param>
        /// <returns>True if the peek was successful and didn't encounter the end of the source; False otherwise.</returns>
        bool Peek(int offset, [NotNullWhen(true)] out T? peek);

        /// <summary>
        /// Get the previously-read element at the specified offset behind the current cursor head position.
        /// </summary>
        /// <param name="offset">The number of elements behind the current position to peek at.</param>
        /// <returns>The element at the specified offset behind the current position, or null if there isn't one.</returns>
        T? Back(int offset);

        /// <summary>
        /// Returns an enumerator to iterate through the remaining elements in the source; starting from the current head position (by default).
        /// </summary>
        /// <param name="withCurrent">If the current element should be included in the enumeration. (default: true)</param>
        /// <param name="withPrevious">If the previous elements should be included in the enumeration. (default: false)</param>

        IEnumerator<T> GetEnumerator(bool withCurrent = true, bool withPrevious = false);

        /// <summary>
        /// Splits the cursor into a new cursor at the current head position. 
        /// </summary>
        new Cursor<T> Clone();

        #region Default Implementations

        T IReadOnlyList<T>.this[int index]
            => this[index] ?? throw new IndexOutOfRangeException();

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        object ICloneable.Clone()
            => Clone();

        #endregion


    }
}