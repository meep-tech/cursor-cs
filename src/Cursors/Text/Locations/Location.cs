namespace Meep.Tech.Collections {

  public partial class TextCursor {

    /// <summary>
    /// A location in the text.
    /// </summary>
    public readonly record struct Location(int Index, int Line, int Column)
        : ILocation;
  }
}
