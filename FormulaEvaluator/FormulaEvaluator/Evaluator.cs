using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FormulaEvaluator
{
    /// <summary>
    /// Evaluator class will take a string and evaluate it mathmatically.
    /// </summary>
    public static class Evaluator
    {
        /// <summary>
        /// Function delegate whose purpose it taking in a string based variable and producing an integer value.
        /// </summary>
        /// <param name="variable">The variable to look up.</param>
        /// <returns>The integer associated with the supplied variable.</returns>
        public delegate int LookupEvaluator(string variable);
        
        /// <summary>
        /// Evaluate a string based expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate</param>
        /// <param name="variableLookup">A delegate that should return the value of the variable specified.</param>
        /// <returns>An evaluated expression</returns>
        public static int Evaluate(string expression, LookupEvaluator variableLookup)
        {
            // Get all tokens then remap and strip all whitespace //
            var tokens = Regex.Split(expression, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)").Select(element => {
                return Regex.Replace(element, "\\s+", "");
            });

            var OperationStack = new Stack<OperationToken>();
            var Numbers = new Stack<int>();

            int result = 0;

            foreach (var token in tokens)
            {
                int? possibleNumber = getNumberOrNothing(token);
                if (possibleNumber.HasValue)
                {
                    // Is number //
                }
                else
                {
                    // Is Operation //
                }
            }

            return result;
        }


        private static int? getNumberOrNothing(string test)
        {
            int result;
            if (int.TryParse(test, out result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// A tiny structure for handling operations. Not needed, but helps keep things clean and scalable.
        /// </summary>
        private static struct OperationToken
        {
            /// <summary>
            /// The operation belonging to this struct
            /// </summary>
            public string Operation {get; set;}

            /// <summary>
            /// Apply the operation to the specified integers.
            /// </summary>
            /// <param name="a">The fist integer</param>
            /// <param name="b">The second integer</param>
            /// <returns>The result of the application</returns>
            public int Apply(int a, int b)
            {
                return 0;
            }
        }


    }
}
