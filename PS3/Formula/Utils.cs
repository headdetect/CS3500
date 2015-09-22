using System.Collections;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Extensions and utility functions.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Returns true if the listable is empty
        /// </summary>
        /// <param name="collection">The list to check</param>
        /// <returns>True if empty; false otherwise</returns>
        public static bool IsEmpty(this ICollection collection)
        {
            return collection.Count <= 0;
        }
    }
}
