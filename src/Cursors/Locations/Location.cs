namespace Meep.Tech.Collections {

  public static partial class Cursor {

    /// <summary>
    /// A location within the cursor.
    /// </summary>
    public readonly record struct Location(int Index)
        : ILocation;
  }
}
