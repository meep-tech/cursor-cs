﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.Collections {

    /// <inheritdoc cref="ICursor{T}"/>
    public partial class Cursor<T>
        : ICursor<T>
        where T : notnull {

        #region Private Fields

        private readonly IEnumerator<T> _source;

        private List<T> _buffer = [];

        #endregion

        /// <summary>
        /// The remaining source of the cursor; as an enumerable.
        /// </summary>
        protected IEnumerable<T> Rest
            => _source.AsEnumerable();

        /// <inheritdoc />
        public int Index { get; private set; }
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
        public bool HasReachedEnd { get; private set; }
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
            internal set => _buffer = value is List<T> list
                ? list
                : [.. value];
        }

        /// <inheritdoc />
        public int Buffer
            => _buffer.Count - Index;

        /// <summary>
        /// Clone constructor.
        /// </summary>
        protected virtual Func<Cursor<T>> CloneConstructor
            => () => new Cursor<T>(Rest);

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
        public T Read()
            => Move()
                ? Current
                : default!;

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
        public bool Read(params Predicate<T>[] predicate)
            => predicate.Any(p => p(Current))
            && Move(1);

        /// <inheritdoc />
        public bool Read([NotNullWhen(true)] out T? match, params Predicate<T>[] predicate) {
            if(predicate.Any(option => option(Current))) {
                return Read(out match);
            }
            else {
                match = default;
                return false;
            }
        }

        /// <inheritdoc />
        public bool Read([NotNull] Predicate<T> predicate, [NotNullWhen(true)] out T? match) {
            if(predicate(Current)) {
                return Read(out match);
            }
            else {
                match = default;
                return false;
            }
        }

        /// <inheritdoc />
        public bool Read([NotNullWhen(true)] out T? match, params T[] matches) {
            if(matches.Contains(Current)) {
                return Read(out match);
            }
            else {
                match = default;
                return false;
            }
        }

        /// <inheritdoc />
        public bool Read([NotNullWhen(true)] out T? match, [NotNull] Predicate<T> predicate) {
            if(predicate(Current)) {
                return Read(out match);
            }
            else {
                match = default;
                return false;
            }
        }

        /// <inheritdoc />
        public bool ReadWhile([NotNullWhen(true)] out IEnumerable<T>? match, params Predicate<T>[] predicate) {
            List<T> matches = [];
            while(predicate.Any(option => option(Current))) {
                matches.Add(Current);
                Move(1);
            }

            match = matches.Count > 0
                ? matches
                : default;

            return match is not null;
        }

        /// <inheritdoc />
        public bool ReadWhile([NotNull] Predicate<T> predicate, [NotNullWhen(true)] out IEnumerable<T>? match)
            => ReadWhile(out match, predicate);

        /// <inheritdoc />
        public bool ReadWhile([NotNullWhen(true)] out IEnumerable<T>? match, params T[] options) {
            List<T> found = [];
            while(options.Contains(Current)) {
                found.Add(Current);
                Move(1);
            }

            match = found.Count > 0
                ? found
                : default;

            return match is not null;
        }

        /// <inheritdoc />
        public bool ReadWhile([NotNull] T[] options, [NotNullWhen(true)] out IEnumerable<T>? match)
            => ReadWhile(out match, options);

        /// <inheritdoc />
        public bool ReadWhile([NotNull] T value, [NotNullWhen(true)] out IEnumerable<T>? match)
            => ReadWhile(out match, value);

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
        public bool ReadNext(params Predicate<T>[] predicate)
            => predicate.Any(p => p(Next ?? default!))
            && Move(1);

        /// <inheritdoc />
        public bool ReadNextWhile([NotNullWhen(true)] out IEnumerable<T>? match, params Predicate<T>[] predicate) {
            List<T> matches = new();
            while(predicate.Any(option => option(Next ?? default!))) {
                matches.Add(Next!);
                Move(1);
            }

            match = matches.Count > 0
                ? matches
                : default;

            return match is not null;
        }

        /// <inheritdoc />
        public bool ReadNextWhile([NotNull] Predicate<T> predicate, [NotNullWhen(true)] out IEnumerable<T>? match)
            => ReadNextWhile(out match, predicate);

        /// <inheritdoc />
        public bool ReadNextWhile([NotNullWhen(true)] out IEnumerable<T>? match, params T[] options) {
            List<T> found = new();
            while(options.Contains(Next)) {
                found.Add(Next!);
                Move(1);
            }

            match = found.Count > 0
                ? found
                : default;

            return match is not null;
        }

        /// <inheritdoc />
        public bool Skip(int count = 1)
            => Move(count);

        /// <inheritdoc />
        public bool Skip(params T[] options) {
            bool moved = false;
            if(options.Contains(Current)) {
                Move(1);
                moved = true;
            }

            return moved;
        }

        /// <inheritdoc />
        public bool Skip(params Predicate<T>[] predicate) {
            bool moved = false;
            if(predicate.Any(p => p(Current))) {
                Move(1);
                moved = true;
            }

            return moved;
        }

        /// <inheritdoc />
        public bool SkipWhile(params Predicate<T>[] predicate) {
            bool moved = false;
            while(predicate.Any(p => p(Current))) {
                Move(1);
                moved = true;
            }

            return moved;
        }

        /// <inheritdoc />
        public bool SkipWhile(params T[] options) {
            bool moved = false;
            while(options.Contains(Current)) {
                Move(1);
                moved = true;
            }

            return moved;
        }

        /// <inheritdoc />
        public bool SkipUntil(params Predicate<T>[] predicate) {
            bool moved = false;
            while(predicate.All(p => !p(Current))) {
                Move(1);
                moved = true;
            }

            return moved;
        }

        /// <inheritdoc />
        public bool SkipUntil(params T[] options) {
            bool moved = false;
            while(!options.Contains(Current)) {
                Move(1);
                moved = true;
            }

            return moved;
        }

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
        public virtual bool MoveTo(Cursor.ILocation? position, int withOffset = 0) {
            int index
                = (position?.Index ?? 0)
                + withOffset;

            if(index < 0) {
                return false;
            }
            else if(Memory.Count <= index) {
                return Move(index - Index);
            }
            else {
                Index = position?.Index ?? 0;

                return withOffset == 0
                    || Move(withOffset);
            }
        }

        /// <inheritdoc />
        public bool MoveTo(int index, int withOffset = 0)
            => MoveTo(new Cursor.Location(index), withOffset);

        /// <inheritdoc />
        public bool Move(Cursor.ILocation? to, int offset = 0)
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

            while(Move()) {
                _buffer.Add(Current!);
                Index++;
                yield return Next!;
            }
        }

        /// <inheritdoc cref="IReadOnlyCursor{T}.Clone"/>
        public virtual TCursor Clone<TCursor>()
            where TCursor : Cursor<T> {
            TCursor cursor = (TCursor)CloneConstructor();

            cursor.Index = Index;
            cursor.HasReachedEnd = HasReachedEnd;
            cursor.Memory = _buffer;

            return cursor;
        }

        #region Explicit Implementations

        int IReadOnlyCollection<T>.Count
            => Index;

        Cursor.ILocation IReadOnlyCursor<T>.Position
            => Position;

        T IReadOnlyList<T>.this[int index]
            => this[index] ?? throw new IndexOutOfRangeException();

        Cursor<T> IReadOnlyCursor<T>.Clone()
            => Clone<Cursor<T>>();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        #endregion

    }
}
