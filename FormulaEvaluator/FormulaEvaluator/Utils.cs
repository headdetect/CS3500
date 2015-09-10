using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulaEvaluator
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
