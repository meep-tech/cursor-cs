using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.Collections {

    /// <inheritdoc cref="ICursor{T}"/>
    public partial class Cursor<T>
        : ICursor<T>
        where T : notnull {

        #region Private Fields

        internal readonly IEnumerator<T> _source;

        internal readonly List<T> _buffer = [];

        #endregion

        /// <inheritdoc />
        public int Index { get; internal set; }
            = 0;

        /// <inheritdoc cref="IReadOnlyCursor{T}.Position"/>
        public Cursor.Location Position
            => new(Index);

        /// <inheritdoc />
        public bool IsAtStart
            => Index < 1;

        /// <inheritdoc />
        public bool IsAtEnd
            => HasReachedEnd
            && Index == _buffer.Count;

        /// <inheritdoc />
        public bool HasMoved
            => _buffer.Count > 1;

        /// <inheritdoc />
        public bool HasReachedEnd { get; internal set; }
            = false;

        /// <inheritdoc />
        public T? Previous
            => Peek(-1);

        /// <inheritdoc />
        public T Current
            => _buffer[Index];

        /// <inheritdoc />
        public T? Next
            => Peek(1);

        /// <inheritdoc />
        public bool SourceIsEmpty
            => Index == -1;

        /// <inheritdoc />
        public IReadOnlyList<T> Memory {
            get => _buffer;
            internal init => _buffer.AddRange(value);
        }

        /// <inheritdoc />
        public int Buffer
            => _buffer.Count - Index;

        /// <summary>
        /// Clones this cursor to create a new cursor with the same source and state.
        /// </summary>
        protected virtual Func<Cursor<T>> Cloner
            => () => new(_source.AsEnumerable()) {
                Index = Index,
                HasReachedEnd = HasReachedEnd,
                Memory = _buffer
            };

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
                Index = -1;
            }
        }

        /// <inheritdoc />
        public virtual T? Peek(int offset) {
            int peekIndex = Index + offset;
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
        public bool ReadNext([NotNull] T match, [NotNullWhen(true)] out T? prev) {
            if(match.Equals(Next)) {
                return Read(out prev);
            }
            else {
                prev = default;
                return false;
            }
        }

        /// <inheritdoc />
        public bool ReadNext([NotNullWhen(true)] out T? prev, params T[] matches) {
            if(matches.Contains(Next)) {
                return Read(out prev);
            }
            else {
                prev = default;
                return false;
            }
        }

        /// <inheritdoc />
        public bool ReadNext(params T[] matches)
            => matches.Length == 0
                ? Move(1)
                : matches.Contains(Next)
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

            int nextIndex = Index + offset;
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

            Index = nextIndex;
            return true;
        }

        /// <inheritdoc />
        public bool MoveNext()
            => Move(1);

        /// <inheritdoc />
        public void Reset(Cursor.ILocation? toLocation = null)
            => Move(to: toLocation);

        /// <inheritdoc />
        public virtual bool MoveTo(Cursor.ILocation position, int withOffset = 0) {
            int index
                = position.Index
                + withOffset;

            if(index < 0) {
                return false;
            }
            else if(Memory.Count <= index) {
                return Move(index - Index);
            }
            else {
                Index = position.Index;

                return true;
            }
        }

        /// <inheritdoc />
        public bool MoveTo(int index, int withOffset = 0)
            => MoveTo(new Cursor.Location(index), withOffset);

        /// <inheritdoc />
        public bool Move(Cursor.ILocation to, int offset = 0)
            => MoveTo(to, offset);

        /// <inheritdoc />
        public bool Move(int offset = 0, int to = 0)
            => MoveTo(to, offset);

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator(bool withCurrent = true, bool withPrevious = false) {
            if(withPrevious) {
                foreach(T? item in _buffer[..Index]) {
                    yield return item;
                }
            }

            if(withCurrent) {
                yield return Current!;
            }

            while(Read()) {
                _buffer.Add(Current!);
                Index++;
                yield return Next!;
            }
        }

        /// <inheritdoc cref="IReadOnlyCursor{T}.Clone"/>
        public TCursor Clone<TCursor>()
            where TCursor : Cursor<T>
            => (TCursor)Cloner();

        #region Explicit Implementations

        int IReadOnlyCollection<T>.Count
            => Index;

        Cursor.ILocation IReadOnlyCursor<T>.Position
            => Position;

        T IReadOnlyList<T>.this[int index]
            => this[index] ?? throw new IndexOutOfRangeException();

        Cursor<T> IReadOnlyCursor<T>.Clone()
            => Cloner();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        #endregion

    }
}
