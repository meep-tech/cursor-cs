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
        public int Line { get; private set; }
            = 0;

        /// <inheritdoc />
        public int Column { get; private set; }
            = 0;

        /// <inheritdoc />
        public bool IsStartOfLine
            => Column == 0;

        /// <inheritdoc />
        public string Text {
            get {
                if(_full is null) {
                    if(HasReachedEnd) {
                        return _full = new([.. Memory]);
                    }
                    else {
                        StringBuilder full = new(Memory.Join());
                        int ahead = 1;
                        while(!HasReachedEnd) {
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
        protected override Func<Cursor<char>> CloneConstructor
            => () => new TextCursor(Rest);

        /// <inheritdoc />
        public override TCursor Clone<TCursor>() {
            TextCursor cursor = base.Clone<TextCursor>();
            cursor.Line = Line;
            cursor.Column = Column;

            return (TCursor)(object)cursor;
        }

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

        /// <inheritdoc cref="Cursor{T}.MoveTo(Cursor.ILocation, int)"/>
        public bool MoveTo(ILocation? position, int withOffset = 0) {
            if(position is null) {
                return MoveTo(0, withOffset);
            }

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
                if(base.MoveTo(position)) {
                    Column = position.Column;
                    Line = position.Line;

                    return withOffset == 0
                        || Move(withOffset);
                }
                else {
                    return false;
                }
            }
        }

        /// <inheritdoc cref="Cursor{T}.MoveTo(Cursor.ILocation, int)"/>
        public override bool MoveTo(Cursor.ILocation? position, int withOffset = 0)
            => position is ILocation location
                ? MoveTo(location, withOffset)
                : base.MoveTo(position, withOffset);

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

        /// <summary>
        /// Skips to the next non-whitespace character.
        /// </summary> 
        public bool SkipWhiteSpace(bool skipNulls = true)
            => skipNulls
                ? Skip(char.IsWhiteSpace)
                : Skip(CharExtensions.IsWhiteSpaceOrNull);

        /// <inheritdoc cref="Cursor{T}.Clone"/>
        public TextCursor Clone()
            => Clone<TextCursor>();

        #region Explicit Interface Implementations

        ILocation IReadOnlyTextCursor<char>.Position
            => Position;

        #endregion
    }
}
