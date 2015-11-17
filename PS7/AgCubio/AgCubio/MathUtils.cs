using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgCubio
{
    /// <summary>
    /// Collective mathmatical utilities
    /// </summary>
    public static class MathUtils
    {

        /// <summary>
        /// Clamp the specified value in a min and max range
        /// </summary>
        /// <param name="value">The value to clamp</param>
        /// <param name="min">Min the value can be</param>
        /// <param name="max">Max the value can be</param>
        /// <returns>The value in the specified range</returns>
        public static float Clamp(float value, float min, float max)
        {
            return value < min ?
                min :
                value > max ?
                    max :
                    value;
        }

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
    }
}
