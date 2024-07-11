namespace Meep.Tech.Collections {

  public partial class TextCursor {

    /// <summary>
    /// A cursor location within a text source.
    /// </summary>
    public interface ILocation
      : Cursor.ILocation {

      /// <summary>
      /// The line number of this location within the cursor.
      /// </summary>
      int Line { get; }

      /// <summary>
      /// The column number of this location within the cursor.
      /// </summary>
      int Column { get; }
    }
  }
}
