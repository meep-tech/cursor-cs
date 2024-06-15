using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.Data {

    /// <summary>
    /// A Lazy reader with multi-peek and rewind capabilities.
    /// <para>Cursors use a 'Head' to keep track of the current position while reading through the source, and a 'Memory' buffer to store previously read values.</para>
    /// </summary>
    public interface ICursor<T>
        : IReadOnlyCursor<T>
        where T : notnull {

        /// <summary>
        /// Moves the cursor head to the next element in the source, and returns the previous one.
        /// Used for more traditional reading.
        /// </summary>
        /// <param name="prev">The previous element the cursor read over in this call. Null if there are no elements left to read.</param>
        /// <returns>True if the cursor was able to move; False if the cursor has reached the end of the source.</returns>
        bool Read([NotNullWhen(true)] out T? prev);

        /// <summary>
        /// Pushes the head past the next element in the source if it matches any of the given elements.
        /// </summary>
        /// <param name="matches">The elements to match against the current head of the cursor.</param>
        /// <returns>True if the head was moved past a matching element; False otherwise.</returns>
        bool Read(params T[] matches);

        /// <summary>
        /// <inheritdoc cref="Read(T[])"/>
        /// </summary>
        /// <param name="prev">The previous element the cursor read over in this call. Null if there are no elements left to read.</param>
        /// <param name="matches">The elements to match against the current head of the cursor.</param>
        /// <returns>True if the cursor was able to move; False if the cursor has reached the end of the source.</returns>
        bool Read([NotNullWhen(true)] out T? prev, params T[] matches);

        /// <summary>
        /// Pushes the head past the next element in the source if it matches the given element.
        /// </summary>
        /// <returns>True if the head was moved past the matching element; False otherwise.</returns>
        bool Read([NotNull] T match);

        /// <summary>
        /// Try to move the cursor head forward by one if the next element matches the given value(s).
        /// </summary>
        /// <returns>True if the head was moved past the matching element; False otherwise.</returns>
        bool ReadNext(params T[] matches);

        /// <summary>
        /// <inheritdoc cref="ReadNext(T[])"/>
        /// </summary>
        /// <param name="match">The value to match against the next element of the cursor.</param>
        /// <param name="prev">The element the cursor read over in this call. Null if there are no elements left to read.</param>
        /// <returns>True if the head was moved past the matching element; False otherwise.</returns>
        public bool ReadNext([NotNull] T match, [NotNullWhen(true)] out T? prev);

        /// <summary>
        /// <inheritdoc cref="ReadNext(T[])" path="/summary"/>
        /// </summary>
        /// <param name="prev">The element the cursor read over in this call. Null if there are no elements left to read.</param>
        /// <param name="matches">The values to match against the current head of the cursor.</param>
        public bool ReadNext([NotNullWhen(true)] out T? prev, params T[] matches);

        /// <summary>
        /// Try to move the cursor head forward by one if the next element matches the given value.
        /// </summary>
        /// <returns>True if the head was moved past the matching element; False otherwise.</returns>
        bool ReadNext([NotNull] T match);

        /// <summary>
        /// Move the cursor head forward by a specified number of elements.
        /// </summary>
        void Skip(int count = 1);

        /// <summary>
        /// Move the cursor head back by a specified number of elements.
        /// </summary>
        void Rewind(int offset = 1);

        /// <summary>
        /// Move the cursor head in either direction by a specified number of elements.
        /// </summary>
        /// <param name="offset">A positive or negative number of elements to move the cursor head forward or back.</param>
        bool Move(int offset);
    }
}