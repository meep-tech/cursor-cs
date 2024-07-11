using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.Collections {

    /// <summary>
    /// A Lazy reader with multi-peek and rewind capabilities.
    /// <para>
    ///     - Cursors use a 'Head' to keep track of the current position 
    ///     while reading through the source, and a 'Memory' buffer to
    ///     store previously read values. 
    /// </para>
    /// <para>
    ///     - Unless no text is provided; a cursor will always begin 
    ///     pre-initalized at  position 0, with  the first element poised 
    ///     to be read. If the source is empty, the cursor will be 
    ///     positioned at -1 instead to indicate that the cursor could
    ///     not initialize itself to the first element of the input.
    /// </para>
    /// </summary>
    public interface ICursor<T>
        : IReadOnlyCursor<T>
        where T : notnull {

        #region Read [Next]

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

        #endregion

        #region Move [Next], Skip, and Rewind

        /// <summary>
        /// Move the cursor head in either direction by a specified number of elements.
        /// </summary>
        /// <param name="offset">A positive or negative number of elements to move the cursor head forward or back.</param>
        /// <returns>True if the operation was successful; False if the cursor could not move the desired amount (0 always should result in true).</returns>
        bool Move(int offset);

        /// <summary>
        /// Move the cursor head forward by one to the next element in the source.
        /// </summary>
        /// <returns>True if the cursor was able to move; False if the cursor has reached the end of the source.</returns>
        bool MoveNext();

        /// <summary>
        /// Move the cursor head forward by a specified number of elements.
        /// <para>- Alias for <see cref="Move(int)"/> with a positive offset.</para>
        /// </summary>
        void Skip(int count = 1);

        /// <summary>
        /// Move the cursor head back by a specified number of elements.
        /// <para>- Alias for <see cref="Move(int)"/> with the offset applied negatively.</para>
        /// </summary>
        void Rewind(int offset = 1);

        #endregion

        #region Move(to) and Reset

        /// <summary>
        /// Reset the cursor to the beginning of the source, or to a specified location.
        /// </summary>
        void Reset(Cursor.ILocation? toLocation = null);

        /// <summary>
        /// <inheritdoc cref="Move(Cursor.ILocation, int)"/>
        /// </summary>
        /// <param name="index">The index to move the cursor head to.</param>
        /// <param name="withOffset"><inheritdoc cref="MoveTo(Cursor.ILocation, int)" path="/param[@name='withOffset']"/></param>
        bool MoveTo(int index, int withOffset = 0);

        /// <summary>
        /// <inheritdoc cref="Move(Cursor.ILocation, int)"/>
        /// </summary>
        /// <param name="position">The location to move the cursor head to.</param>
        /// <param name="withOffset">A positive or negative number of elements to move the cursor head forward or back.</param>
        bool MoveTo(Cursor.ILocation position, int withOffset = 0);

        /// <summary>
        /// Move the cursor head to a specified location in the source, with an optional offset.
        /// </summary>
        /// <param name="to">The location to move the cursor head to.</param>
        /// <param name="offset">A positive or negative number of elements to move the cursor head forward or back.</param>
        /// <returns>True if the operation was successful; False if the cursor could not move the desired amount (0 always should result in true).</returns>
        bool Move(Cursor.ILocation to, int offset = 0);

        /// <inheritdoc cref="Move(Cursor.ILocation, int)"/>
        bool Move(int offset = 0, int to = 0);

        #endregion
    }
}