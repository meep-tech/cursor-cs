using System.Diagnostics.CodeAnalysis;
using System.Text;

using Meep.Tech.Text;

namespace Meep.Tech.Collections {

    /// <summary>
    /// A cursor designed for reading text organized into lines.
    /// </summary>
    /// <param name="source">The enumerable to read though.</param>
    public partial class TextCursor(IEnumerable<char> source)
        : Cursor<char>(source),
            ITextCursor<char> {

        #region Private Fields

        private string? _full;

        #endregion

        /// <inheritdoc />
        public new Location Position
            => new(Index, Line, Column);

        /// <inheritdoc />
        public int Line { get; internal set; }
            = 0;

        /// <inheritdoc />
        public int Column { get; internal set; }
            = 0;

        /// <inheritdoc />
        public bool IsStartOfLine
            => Column == 0;

        /// <inheritdoc />
        public string Text {
            get {
                if(_full is null) {
                    if(IsAtEnd) {
                        return _full = new([.. Memory]);
                    }
                    else {
                        StringBuilder full = new(Memory.Join());
                        int ahead = 1;
                        while(!IsAtEnd) {
                            full.Append(Peek(ahead++));
                        }

                        return _full = full.ToString();
                    }
                }
                else {
                    return _full;
                }
            }
        }

        #region Protected Overrides

        /// <inheritdoc />
        protected override Func<Cursor<char>> Cloner
            => () => new TextCursor(_source.AsEnumerable()) {
                Index = Index,
                Line = Line,
                Column = Column,
                HasReachedEnd = HasReachedEnd,
                Memory = _buffer
            };

        #endregion

        /// <summary>
        /// Creates a new TextCursor for a reader.
        /// </summary>
        public TextCursor(TextReader reader)
            : this(reader.AsEnumerable()) { }

        /// <inheritdoc />
        public override bool Move(int offset) {
            int oldPosition = Index;

            if(!base.Move(offset)) {
                return false;
            }
            else if(offset is 0) {
                return true;
            }

            if(offset > 0) {
                for(int i = oldPosition; i < Index; i++) {
                    if(Memory[i] is '\n') {
                        Line++;
                        Column = 0;
                    }
                    else {
                        Column++;
                    }
                }
            }
            else {
                for(int i = oldPosition; i > Index; i--) {
                    if(Memory[i] is '\n') {
                        Line--;
                        Column = 0;
                    }
                    else {
                        Column--;
                    }
                }
            }

            return true;
        }

        /// <inheritdoc />
        public bool Read([NotNull] string match) {
            if(match.Length == 0) {
                return Move(1);
            }

            int matchIndex = 0;
            while(matchIndex < match.Length) {
                if(Peek(matchIndex, out char peeked)) {
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

        /// <inheritdoc cref="Cursor{T}.Read(T)"/>
        public bool ReadNext([NotNull] string match) {
            if(match.Length == 0) {
                return Move(1);
            }

            int matchIndex = 0;
            while(matchIndex < match.Length) {
                if(Peek(matchIndex + 1, out char peeked)) {
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

        /// <inheritdoc cref="Cursor{T}.Clone"/>
        public TextCursor Clone()
            => Clone<TextCursor>();

        #region Explicit Interface Implementations

        ILocation IReadOnlyTextCursor<char>.Position
            => Position;

        #endregion
    }
}
