using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetGUI
{
    /// <summary>
    /// Functions that aren't included in the default C#.net Framework.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Clamp the specified value in a min and max range
        /// </summary>
        /// <param name="value">The value to clamp</param>
        /// <param name="min">Min the value can be</param>
        /// <param name="max">Max the value can be</param>
        /// <returns>The value in the specified range</returns>
        public static int Clamp(int value, int min, int max)
        {
            return value < min ? 
                min : 
                value > max ? 
                    max : 
                    value;
        }

        /// <summary>
        /// Truncate a string to the specified length
        /// </summary>
        /// <param name="stringy">The string to cut off</param>
        /// <param name="length">The length to cut it off at</param>
        /// <param name="appendedText">The end of the text</param>
        /// <returns>The truncated string</returns>
        public static string Truncate(this string stringy, int length, string appendedText = "...")
        {
            var mLength = Clamp(length, 0, stringy.Length);
            return stringy.Substring(0, mLength) + (stringy.Length > length ? appendedText : string.Empty);
        }
    }
}
