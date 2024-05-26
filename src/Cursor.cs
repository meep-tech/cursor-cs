using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.Data {

    /// <summary>
    /// A Lazy reader with multi-peek and rewind capabilities.
    /// </summary>
    public class Cursor<T>
        : IReadOnlyList<T?>
        where T : notnull {

        internal readonly IEnumerator<T> _source;
        internal readonly List<T> _buffer = [];

        /// <summary>
        /// The current index position of the cursor.
        /// </summary>
        public int Position { get; internal set; }
            = 0;

        public bool IsAtStart 
            => Position < 1;

        public bool IsAtEnd { get; private set; }
            = false;

        public T? Previous
            => Peek(-1);

        /// <summary>
        /// The current element in the cursor.
        /// </summary>
        public T Current
            => _buffer[Position];

        public T? Next
            => Peek(1);

        public bool HasEmptySource
            => Position == -1;

        public IReadOnlyList<T> Memory
            => _buffer;

        public int Buffer
            => _buffer.Count - Position;

        public T? this[int index]
            => index < 0
                ? default
                : Peek(index - Position);

        public Cursor(IEnumerable<T> source) {
            _source = source.GetEnumerator();
            if(_source.MoveNext()) {
                _buffer.Add(_source.Current);
            } else {
                Position = -1;
            }
        }

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

        public bool Peek(int offset, [NotNullWhen(true)] out T? peek) {
            peek = Peek(offset);
            return peek is not null;
        }

        public T? Back(int offset)
            => Peek(-offset);

        public bool Read([NotNullWhen(true)] out T? prev) {
            if(Move(1)) {
                prev = Previous!;
                return true;
            } else {
                prev = default;
                return false;
            }
        }

        public bool Read([NotNull] string match) {
            if(match.Length == 0) {
                return Move(1);
            }

            int matchIndex = 0;
            while(matchIndex < match.Length) {
                if(Peek(matchIndex, out T? peeked)) {
                    if(peeked!.Equals(match[matchIndex])) {
                        matchIndex++;
                    }
                    else {
                        return false;
                    }
                }
                else {
                    return false;
                }
            }

            return Move(match.Length);
        }

        public bool ReadNext([NotNull] string match) {
            if(match.Length == 0) {
                return Move(1);
            }

            int matchIndex = 0;
            while(matchIndex < match.Length) {
                if(Peek(matchIndex + 1, out T? peeked)) {
                    if(peeked!.Equals(match[matchIndex])) {
                        matchIndex++;
                    } else {
                        return false;
                    }
                } else {
                    return false;
                }
            }

            return Move(match.Length);
        }

        public bool Read(params T[] matches) 
            => matches.Length == 0 
                ? Move(1) 
                : matches.Contains(Current) 
                    && Move(1);

        public bool Read(IEnumerable<T> matches, [NotNullWhen(true)] out T? prev) {
            if(matches.Contains(Current)) {
                return Read(out prev);
            } else {
                prev = default;
                return false;
            }
        }

        public void Skip(int count = 1)
            => Move(count);

        public void Rewind(int offset = 1)
            => Move(-offset);

        public virtual bool Move(int offset) {
            if (offset == 0) {
                return true;
            }

            int nextIndex = Position + offset;
            if(nextIndex < 0) {
                return false;
            }

            while(_buffer.Count <= nextIndex) {
                if(!_source.MoveNext()) {
                    IsAtEnd = true;
                    return false;
                }

                _buffer.Add(_source.Current);
            }

            Position = nextIndex;
            return true;
        }

        public IEnumerator<T> GetEnumerator() {
            foreach(T? item in _buffer) {
                yield return item;
            }

            while (Move(1)) {
                _buffer.Add(Current!);
                Position++;
                yield return Current!;
            }
        }

        int IReadOnlyCollection<T?>.Count
            => Position;

        IEnumerator IEnumerable.GetEnumerator() 
            => GetEnumerator();
    }
}
