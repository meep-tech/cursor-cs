namespace Meep.Tech.Collections {
  /// <summary>
  /// A cursor designed for reading text and documents that are organized into lines. 
  /// <para> This provices a read-only interface that can peak but cannot move the cursor's current position.</para>
  /// </summary>
  public interface IReadOnlyTextCursor<T>
      : IReadOnlyCursor<T>
      where T : notnull {

    /// <summary>
    /// The current location of the cursor within the source.
    /// </summary>
    new TextCursor.ILocation Position { get; }

    /// <summary>
    /// The current line number of the cursor.
    /// </summary>
    int Line { get; }

    /// <summary>
    /// The column index of the cursor within the current line.
    /// </summary>
    int Column { get; }

    /// <summary>
    /// If the cursor is at the start of a line.
    /// </summary>
    bool IsStartOfLine { get; }

    /// <summary>
    /// Used to get the full text of the cursor's source.
    /// </summary>
    string Text { get; }
  }
}
