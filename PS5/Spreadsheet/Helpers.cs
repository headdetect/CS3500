using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetUtilities;

namespace Spreadsheet
{
    /// <summary>
    /// Utility class.
    /// </summary>
    public static class Helpers
    {

        /// <summary>
        /// Checks to see if an object is concidered empty
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <returns>True if empty; false otherwise</returns>
        public static bool IsEmpty(this object obj)
        {
            var s = obj as string;
            if (s != null)
                return string.IsNullOrWhiteSpace(s);

            var formula = obj as Formula;
            if (formula != null)
                return string.IsNullOrWhiteSpace(formula.Expression);

            return false;
        }
    }
}
