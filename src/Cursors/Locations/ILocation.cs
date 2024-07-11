namespace Meep.Tech.Collections {

  /// <summary>
  /// Static namespace for Cursor related types and Constants.
  /// </summary>
  public static partial class Cursor {

    /// <summary>
    /// A specific location within the cursor.
    /// </summary>
    public interface ILocation {

      /// <summary>
      /// The index of this location within the cursor.
      /// </summary>
      int Index { get; }
    }
  }
}
