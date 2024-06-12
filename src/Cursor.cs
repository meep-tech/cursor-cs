using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.Data {

    /// <summary>
    /// A Lazy reader with multi-peek and rewind capabilities.
    /// <para>Cursors use a 'Head' to keep track of the current position while reading through the source, and a 'Memory' buffer to store previously read values.</para>
    /// </summary>
    public class Cursor<T>
        : ICursor<T>
        where T : notnull {

        #region Private Fields

        private readonly IEnumerator<T> _source;

        private readonly List<T> _buffer = [];

        #endregion

        /// <inheritdoc />
        public int Position { get; internal set; }
            = 0;

        /// <inheritdoc />
        public bool IsAtStart
            => Position < 1;

        /// <inheritdoc />
        public bool IsAtEnd
            => HasReachedEnd
            && Position == _buffer.Count;

        /// <inheritdoc />
        public bool HasMoved
            => _buffer.Count > 1;

        /// <inheritdoc />
        public bool HasReachedEnd { get; private set; }
            = false;

        /// <inheritdoc />
        public T? Previous
            => Peek(-1);

        /// <inheritdoc />
        public T Current
            => _buffer[Position];

        /// <inheritdoc />
        public T? Next
            => Peek(1);

        /// <inheritdoc />
        public bool SourceIsEmpty
            => Position == -1;

        /// <inheritdoc />
        public IReadOnlyList<T> Memory {
            get => _buffer;
            private init => _buffer.AddRange(value);
        }

        /// <inheritdoc />
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
        public Cursor(IEnumerable<T> source)
            : this(source.GetEnumerator()) { }

        /// <inheritdoc />
        public Cursor(IEnumerator<T> source) {
            _source = source;
            if(_source.MoveNext()) {
                _buffer.Add(_source.Current);
            }
            else {
                Position = -1;
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool Peek(int offset, [NotNullWhen(true)] out T? peek)
            => (peek = Peek(offset)) is not null;

        /// <inheritdoc />
        public T? Back(int offset)
            => Peek(-offset);

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool Read([NotNull] T match)
            => match.Equals(Current)
            && Move(1);

        /// <inheritdoc />
        public bool Read(params T[] matches)
            => matches.Length == 0
                ? Move(1)
                : matches.Contains(Current)
                    && Move(1);

        /// <inheritdoc />
        public bool Read([NotNullWhen(true)] out T? prev, params T[] matches) {
            if(matches.Contains(Current)) {
                return Read(out prev);
            }
            else {
                prev = default;
                return false;
            }
        }

        /// <inheritdoc />
        public bool ReadNext([NotNull] T match)
            => match.Equals(Next)
            && Move(1);

        /// <inheritdoc />
        public void Skip(int count = 1)
            => Move(count);

        /// <inheritdoc />
        public void Rewind(int offset = 1)
            => Move(-offset);

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual Cursor<T> Clone()
            => new(_source) {
                Position = Position,
                HasReachedEnd = HasReachedEnd,
                Memory = _buffer
            };

        #region Explicit Implementations

        int IReadOnlyCollection<T>.Count
            => Position;

        T IReadOnlyList<T>.this[int index]
            => this[index] ?? throw new IndexOutOfRangeException();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        #endregion

    }
}
