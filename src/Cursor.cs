using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.Data {

    /// <summary>
    /// A Lazy reader with multi-peek and rewind capabilities.
    /// <para>Cursors use a 'Head' to keep track of the current position while reading through the source, and a 'Memory' buffer to store previously read values.</para>
    /// </summary>
    public class Cursor<T>
        : IReadOnlyList<T?>
        where T : notnull {
        #region Private Fields
        private readonly IEnumerator<T> _source;
        private readonly List<T> _buffer = [];
        #endregion

        /// <summary>
        /// The current index of the cursor 'Head' within the source.
        /// </summary>
        public int Position { get; internal set; }
            = 0;

        /// <summary>
        /// If the cursor head is at the start of the source.
        /// </summary>
        public bool IsAtStart
            => Position < 1;

        /// <summary>
        /// Checks if the cursor head is at the end of the source.
        /// </summary>
        public bool IsAtEnd
            => HasReachedEnd
            && Position == _buffer.Count;

        /// <summary>
        /// If the cursor has ever been moved from the start of the source.
        /// </summary>
        public bool HasMoved
            => _buffer.Count > 1;

        /// <summary>
        /// Checks if the cursor has reached the end of the source.
        /// </summary>
        public bool HasReachedEnd { get; private set; }
            = false;

        /// <summary>
        /// The previous element in the cursor.
        /// </summary>
        public T? Previous
            => Peek(-1);

        /// <summary>
        /// The element at the current cursor position.
        /// </summary>
        public T Current
            => _buffer[Position];

        /// <summary>
        /// Peek at the next element in the cursor.
        /// </summary>
        /// <returns></returns>
        public T? Next
            => Peek(1);

        /// <summary>
        /// Checks if this cursor was provided an empty source.
        /// </summary>
        public bool HasEmptySource
            => Position == -1;

        /// <summary>
        /// The buffer of characters the cursor has read so far.
        /// </summary>
        public IReadOnlyList<T> Memory
            => _buffer;

        /// <summary>
        /// The number of pre-read elements in the memory buffer (compared to the current cursor position).
        /// </summary>
        public int Buffer
            => _buffer.Count - Position;

        /// <summary>
        /// Used to peek around the current head of the cursor.
        /// </summary>
        /// <returns>The element at the specified index relative to the current position. 0 = current, -1 = previous, 1 = next, etc.</returns>
        public T? this[int index]
            => index == 0
                ? Current
                : index < 0
                    ? Back(-index)
                    : Peek(index);

        /// <summary>
        /// Creates a new cursor for a source.
        /// </summary>
        /// <param name="source">The source to read through using this new cursor.</param>
        public Cursor(IEnumerable<T> source) {
            _source = source.GetEnumerator();
            if(_source.MoveNext()) {
                _buffer.Add(_source.Current);
            }
            else {
                Position = -1;
            }
        }

        /// <summary>
        /// Used to peek at an element further ahead in the source without moving the current head position of the cursor.
        /// </summary>
        /// <param name="offset">The number of elements to peek ahead (or behind).</param>
        /// <returns>The element X positions ahead of the current position, or null if there isn't one</returns>
        public T? Peek(int offset) {
            int peekIndex = Position + offset;
            if(peekIndex < 0) {
                return default;
            }

            while(_buffer.Count <= peekIndex) {
                if(!_source.MoveNext()) {
                    return default;
                }

                _buffer.Add(_source.Current);
            }

            return _buffer[peekIndex];
        }

        /// <summary><inheritdoc cref="Peek(int)" path="/summary"/></summary>
        /// <param name="offset"><inheritdoc cref="Peek(int)" path="/param[@name='offset']"/></param>
        /// <param name="peek">The element X positions ahead of the current position, or null if there isn't one</param>
        /// <returns>True if the peek was successful and didn't encounter the end of the source; False otherwise.</returns>
        public bool Peek(int offset, [NotNullWhen(true)] out T? peek)
            => (peek = Peek(offset)) is not null;

        /// <summary>
        /// Get the previously-read element at the specified offset behind the current cursor head position.
        /// </summary>
        /// <param name="offset">The number of elements behind the current position to peek at.</param>
        /// <returns>The element at the specified offset behind the current position, or null if there isn't one.</returns>
        public T? Back(int offset)
            => Peek(-offset);

        /// <summary>
        /// Moves the cursor head to the next element in the source, and returns the previous one.
        /// Used for more traditional reading.
        /// </summary>
        /// <param name="prev">The previous element the cursor read over in this call. Null if there are no elements left to read.</param>
        /// <returns>True if the cursor was able to move; False if the cursor has reached the end of the source.</returns>
        public bool Read([NotNullWhen(true)] out T? prev) {
            if(Move(1)) {
                prev = Previous!;
                return true;
            }
            else {
                prev = default;
                return false;
            }
        }

        /// <summary>
        /// Pushes the head past the next element in the source if it matches the given element.
        /// </summary>
        /// <returns>True if the head was moved past the matching element; False otherwise.</returns>
        public bool Read([NotNull] T match)
            => match.Equals(Current)
            && Move(1);

        /// <summary>
        /// Reads a whole string from the cursor if the next value passed the current head matches.
        /// </summary>
        public bool ReadNext([NotNull] T match)
            => match.Equals(Next)
            && Move(1);

        /// <summary>
        /// Pushes the head past the next element in the source if it matches any of the given elements.
        /// </summary>
        /// <param name="matches">The elements to match against the current head of the cursor.</param>
        /// <returns>True if the head was moved past a matching element; False otherwise.</returns>
        public bool Read(params T[] matches)
            => matches.Length == 0
                ? Move(1)
                : matches.Contains(Current)
                    && Move(1);

        /// <summary>
        /// <inheritdoc cref="Read(T[])"/>
        /// </summary>
        /// <param name="prev">The previous element the cursor read over in this call. Null if there are no elements left to read.</param>
        /// <param name="matches">The elements to match against the current head of the cursor.</param>
        /// <returns>True if the cursor was able to move; False if the cursor has reached the end of the source.</returns>
        public bool Read([NotNullWhen(true)] out T? prev, params T[] matches) {
            if(matches.Contains(Current)) {
                return Read(out prev);
            }
            else {
                prev = default;
                return false;
            }
        }

        /// <summary>
        /// Move the cursor head forward by a specified number of elements.
        /// </summary>
        public void Skip(int count = 1)
            => Move(count);

        /// <summary>
        /// Move the cursor head back by a specified number of elements.
        /// </summary>
        public void Rewind(int offset = 1)
            => Move(-offset);

        /// <summary>
        /// Move the cursor head in either direction by a specified number of elements.
        /// </summary>
        /// <param name="offset">A positive or negative number of elements to move the cursor head forward or back.</param>
        public virtual bool Move(int offset) {
            if(offset == 0) {
                return true;
            }

            int nextIndex = Position + offset;
            if(nextIndex < 0) {
                return false;
            }

            while(_buffer.Count <= nextIndex) {
                if(!_source.MoveNext()) {
                    HasReachedEnd = true;
                    return false;
                }

                _buffer.Add(_source.Current);
            }

            Position = nextIndex;
            return true;
        }

        /// <summary>
        /// Returns an enumerator to iterate through the remaining elements in the source; starting from the current head position.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
            => GetEnumerator(true, false);

        /// <summary>
        /// Returns an enumerator to iterate through the remaining elements in the source; starting from the current head position (by default).
        /// </summary>
        /// <param name="withCurrent">If the current element should be included in the enumeration. (default: true)</param>
        /// <param name="withPrevious">If the previous elements should be included in the enumeration. (default: false)</param>
        public IEnumerator<T> GetEnumerator(bool withCurrent = true, bool withPrevious = false) {
            if(withPrevious) {
                foreach(T? item in _buffer[..Position]) {
                    yield return item;
                }
            }

            if(withCurrent) {
                yield return Current!;
            }

            while(Read()) {
                _buffer.Add(Current!);
                Position++;
                yield return Next!;
            }
        }

        #region Explicit Implementations

        int IReadOnlyCollection<T?>.Count
            => Position;

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        #endregion
    }
}
