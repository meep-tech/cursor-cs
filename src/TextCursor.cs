using System.Diagnostics.CodeAnalysis;

using Meep.Tech.Text;

namespace Meep.Tech.Data {

    /// <summary>
    /// A cursor designed for reading text organized into lines.
    /// </summary>
    /// <param name="source">The enumerable to read though.</param>
    public class TextCursor(IEnumerable<char> source)
        : Cursor<char>(source) {

        /// <summary>
        /// The current line number of the cursor.
        /// </summary>
        public int Line { get; internal set; }
            = 0;

        /// <summary>
        /// The column index of the cursor within the current line.
        /// </summary>
        /// <value></value>
        public int Column { get; internal set; }
            = 0;

        /// <summary>
        /// If the cursor is at the start of a line.
        /// </summary>
        public bool IsStartOfLine
            => Column == 0;

        /// <summary>
        /// TODO: Split into FullText and ReadText
        /// </summary>
        public string Text
            => new([.. Memory]);

        /// <summary>
        /// Creates a new TextCursor for a reader.
        /// </summary>
        public TextCursor(TextReader reader)
            : this(reader.AsEnumerable()) { }

        /// <inheritdoc />
        public override bool Move(int offset) {
            int oldPosition = Position;

            if(!base.Move(offset)) {
                return false;
            }
            else if(offset == 0) {
                return true;
            }

            if(offset > 0) {
                for(int i = oldPosition; i < Position; i++) {
                    if(Memory[i] == '\n') {
                        Line++;
                        Column = 0;
                    }
                    else {
                        Column++;
                    }
                }
            }
            else {
                for(int i = oldPosition; i > Position; i--) {
                    if(Memory[i] == '\n') {
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

        /// <summary>
        /// Reads a whole string from the cursor if the source at the current head matches the given string.
        /// </summary>
        /// <remarks>
        /// <seealso cref="Cursor{T}.Read(T)"/>
        /// </remarks>
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

        /// <summary>
        /// Reads a whole string from the cursor if the source after the next char matches the given string.
        /// </summary>
        /// <remarks>
        /// <seealso cref="Cursor{T}.ReadNext(T)"/>
        /// </remarks>
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
    }
}
