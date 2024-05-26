using Meep.Tech.Text;

namespace Meep.Tech.Data {

    /// <summary>
    /// A cursor designed for reading text organized into lines.
    /// </summary>
    /// <param name="source">The enumerable to read though.</param>
    public class TextCursor(IEnumerable<char> source)
        : Cursor<char>(source) {

        public int Line { get; internal set; }
            = 0;

        public int Column { get; internal set; }
            = 0;

        /// <summary>
        /// TODO: Split into FullText and ReadText
        /// </summary>
        public string Text
            => new([.. Memory]);

        public TextCursor(TextReader reader)
            : this(reader.AsEnumerable()) { }

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
                    if(_buffer[i] == '\n') {
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
                    if(_buffer[i] == '\n') {
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
    }
}
