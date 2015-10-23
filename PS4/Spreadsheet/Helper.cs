using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetUtilities;

namespace SS
{
    /// <summary>
    /// A set of utility functions.
    /// </summary>
    public static class Helper
    {

        /// <summary>
        /// Will check to see if an object is concidered "Empty"
        /// </summary>
        /// <param name="obj">The object to check for</param>
        /// <returns>True if empty; false otherwise</returns>
        public static bool IsEmpty(this object obj)
        {
            var formula = obj as Formula;
            if (formula != null)
            {
                return string.IsNullOrEmpty(formula.Expression);
            }

            var s = obj as string;
            return s != null && string.IsNullOrEmpty(s);
        }



        /// <summary>
        /// Tries to get a double from the specified string, if null, not a valid double.
        /// </summary>
        /// <param name="str">The string to parse</param>
        /// <returns>A double from the specified string, if null, not a valid double.</returns>
        public static double? TryGetDouble(this string str)
        {
            double tryGet;
            if (double.TryParse(str, out tryGet))
                return tryGet;
            return null;
        }

    }
}
