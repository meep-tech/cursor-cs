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

        #region Read [Next] [While]

        /// <summary>
        /// Reads the next element in the source, and moves the cursor head to the next element, returning the previous (read) element.
        /// </summary>
        T Read();

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
        /// <param name="options">The elements to match against the current head of the cursor.</param>
        /// <returns>True if the head was moved past a matching element; False otherwise.</returns>
        bool Read(params T[] options);

        /// <summary>
        /// <inheritdoc cref="Read(T[])"/>
        /// </summary>
        /// <param name="prev">The previous element the cursor read over in this call. Null if there are no elements left to read.</param>
        /// <param name="options">The elements to match against the current head of the cursor.</param>
        /// <returns>True if the cursor was able to move; False if the cursor has reached the end of the source.</returns>
        bool Read([NotNullWhen(true)] out T? prev, params T[] options);

        /// <summary>
        /// Pushes the head past the next element in the source if it matches the given element.
        /// </summary>
        /// <param name="match">The element to match against the current head of the cursor.</param>
        /// <param name="predicate">The predicate to match against the current head of the cursor.</param>
        /// <returns>True if the head was moved past the matching element; False otherwise.</returns>
        bool Read([NotNullWhen(true)] out T? match, params Predicate<T>[] predicate);

        /// <inheritdoc cref="Read(out T, Predicate{T}[])"/>
        bool Read([NotNull] Predicate<T> predicate, [NotNullWhen(true)] out T? match);

        /// <summary>
        /// Pushes the head past the next element in the source if it matches the given element.
        /// </summary>
        /// <returns>True if the head was moved past the matching element; False otherwise.</returns>
        bool Read([NotNull] T match);

        /// <summary>
        /// Pushes the head past the next element in the source if it matches the given predicate.
        /// </summary>
        /// <param name="predicate">The predicate to match against the current head of the cursor.</param>
        /// <returns>True if the head was moved past the matching element; False otherwise.</returns>
        bool Read(params Predicate<T>[] predicate);

        /// <summary>
        /// Reads all elements from the source that match the given options until one does not.
        ///    (The cursor's current value will be left as the first non-matching element.) 
        /// </summary>
        /// <param name="options">The elements to match against the current head of the cursor.</param>
        /// <param name="matches">The elements the cursor read over in this call. Null if there are no elements left to read.</param>
        bool ReadWhile([NotNullWhen(true)] out IEnumerable<T>? matches, params T[] options);

        /// <inheritdoc cref="ReadWhile(out IEnumerable{T}, T[])"/>
        bool ReadWhile([NotNull] T[] options, [NotNullWhen(true)] out IEnumerable<T>? matches);

        /// <inheritdoc cref="ReadWhile(out IEnumerable{T}, T[])"/>
        bool ReadWhile([NotNull] T value, [NotNullWhen(true)] out IEnumerable<T>? matches);

        /// <summary>
        /// Reads all elements from the source that match the given predicate until one does not. 
        ///     (The cursor's current value will be left as the first non-matching element.)
        /// </summary>
        /// <param name="predicate">The predicate to match against the current head of the cursor.</param>
        /// <param name="match">The elements the cursor read over in this call. Null if there are no elements left to read.</param> 
        bool ReadWhile([NotNull] Predicate<T> predicate, [NotNullWhen(true)] out IEnumerable<T>? match);

        /// <inheritdoc cref="ReadWhile(Predicate{T}, out IEnumerable{T})"/>
        bool ReadWhile([NotNullWhen(true)] out IEnumerable<T>? match, params Predicate<T>[] predicate);

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
        /// <param name="options">The values to match against the current head of the cursor.</param>
        public bool ReadNext([NotNullWhen(true)] out T? prev, params T[] options);

        /// <summary>
        /// Try to move the cursor head forward by one if the next element matches the given value.
        /// </summary>
        /// <returns>True if the head was moved past the matching element; False otherwise.</returns>
        bool ReadNext([NotNull] T match);

        /// <summary>
        /// Try to move the cursor head forward by one if the next element matches the given predicate.
        /// </summary>
        /// <param name="predicate">The predicate to match against the next element of the cursor.</param>
        /// <returns>True if the head was moved past the matching element; False otherwise.</returns>
        bool ReadNext(params Predicate<T>[] predicate);

        /// <summary>
        /// Try to move the cursor head forward until the next element does not match any of the given values.
        /// </summary>
        /// <param name="match">The element the cursor read over in this call. Null if there are no elements left to read.</param>
        /// <param name="options">The elements to match against the current head of the cursor.</param>
        bool ReadNextWhile([NotNullWhen(true)] out IEnumerable<T>? match, params T[] options);

        /// <summary>
        /// Try to move the cursor head forward until the next element does not match the given value.
        /// </summary>
        /// <param name="predicate">The predicate to match against the next element of the cursor.</param>
        /// <param name="matches">The element the cursor read over in this call. Null if there are no elements left to read.</param>  
        /// <returns>True if the head was moved past the matching element; False otherwise.</returns>
        bool ReadNextWhile([NotNullWhen(true)] out IEnumerable<T>? matches, params Predicate<T>[] predicate);

        /// <inheritdoc cref="ReadNextWhile(Predicate{T}, out IEnumerable{T})"/>
        bool ReadNextWhile([NotNull] Predicate<T> predicate, [NotNullWhen(true)] out IEnumerable<T>? matches);

        #endregion

        #region Move [Next], Skip[While|Until], and Rewind

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
        bool Skip(int count = 1);

        /// <summary>
        /// Skip the current value and move the cursor head forward by one if the next element matches the given value(s).
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        bool Skip(params T[] options);

        /// <summary>
        /// Skip the current value and move the cursor head forward by one if the next element matches the given predicate(s).
        /// </summary>
        bool Skip(params Predicate<T>[] predicate);

        /// <summary>
        /// Move the cursor head forward until the next element does not match any of the given values.
        /// </summary>
        bool SkipWhile(params Predicate<T>[] predicate);

        /// <summary>
        /// Move the cursor head forward until the next element does not match any of the given values.
        /// </summary>
        bool SkipWhile(params T[] options);

        /// <summary>
        /// Move the cursor head forward until the next element does not match any of the given values.
        /// </summary>
        bool SkipUntil(params Predicate<T>[] predicate);

        /// <summary>
        /// Move the cursor head forward until the next element does not match any of the given values.
        /// </summary>
        bool SkipUntil(params T[] options);

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