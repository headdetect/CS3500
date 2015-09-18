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
            var tokens = Regex.Split(expression, "(?<=[-+*/(),])(?=.)|(?<=.)(?=[-+*/(),])")
                .Select(element => Regex.Replace(element, "\\s+", ""))
                .Where(element => element != "")
                .ToArray();

            var OperationStack = new Stack<OperationToken>();
            var NumberStack = new Stack<double>();

            // Will either be a 1 or -1
            int? sign = null;

            for (int i = 0; i < tokens.Length; i++ )
            {
                var token = tokens[i];
                double? possibleNumber = getNumberOrNothing(token);
                var operation = new OperationToken
                {
                    Operation = token[0]
                };
                if (!operation.IsValid && !possibleNumber.HasValue)
                {
                    // Is a variable //
                    possibleNumber = variableLookup(token);
                }

                // Handle negatives and positives //
                if (sign.HasValue)
                {
                    possibleNumber = possibleNumber * sign;
                    sign = null;
                }


                if (possibleNumber.HasValue)
                {
                    // Is number //

                    if (!OperationStack.IsEmpty())
                    {
                        var topOperation = OperationStack.Peek();
                        if (topOperation.IsDivision || topOperation.IsMultiplication)
                        {
                            if (NumberStack.Count < 1) throw new ArgumentException("Invalid Syntax");

                            OperationStack.Pop(); // Passed our test, remove from stack //

                            var number = NumberStack.Pop();

                            NumberStack.Push(topOperation.Apply(number, possibleNumber.Value));

                            continue;
                        }

                    }

                    NumberStack.Push(possibleNumber.Value);
                }
                else
                {
                    if (operation.IsAddition || operation.IsSubtraction)
                    {
                        
                        var previousIndex = i - 1;
                        if (previousIndex < 0)
                        {
                            // The sign provided was the first sign, it must be a + or - signifying that its positive or negative //
                            sign = operation.IsSubtraction ? -1 : 1;
                            continue;
                        }
                        else
                        {
                            var previousOperation = new OperationToken() {
                                Operation = tokens[previousIndex][0]
                            };

                            // If the previous token was an operation then this token must mean positive or negative //
                            // Also make sure its not the last operation either //
                            if (previousOperation.IsOperation && i + 1 < tokens.Length)
                            {
                                sign = operation.IsSubtraction ? -1 : 1;
                                continue;
                            }
                            else if (!OperationStack.IsEmpty())
                            {
                                // There's a pending operation, let's do that instead //

                                var topOperation = OperationStack.Peek();
                                if (topOperation.IsAddition || topOperation.IsSubtraction)
                                {
                                    if (NumberStack.Count < 2) throw new ArgumentException("Invalid Syntax");

                                    OperationStack.Pop();

                                    var number1 = NumberStack.Pop();
                                    var number2 = NumberStack.Pop();

                                    NumberStack.Push(topOperation.Apply(number2, number1));
                                }
                            }
                        }
                        

                        OperationStack.Push(operation);
                    }
                    else if (operation.IsMultiplication || operation.IsDivision || operation.IsOpenBrace)
                        // Nothing complex, just push operation to stack //
                        OperationStack.Push(operation);
                    else if (operation.IsClosingBrace)
                    {
                        if (!OperationStack.IsEmpty())
                        {
                            // If there's already a pending operation. //
                            var topOperation = OperationStack.Peek();
                            if (topOperation.IsAddition || topOperation.IsSubtraction)
                            {
                                if (NumberStack.Count < 2) throw new ArgumentException("Invalid Syntax");

                                OperationStack.Pop();

                                var number1 = NumberStack.Pop();
                                var number2 = NumberStack.Pop();

                                NumberStack.Push(topOperation.Apply(number2, number1));

                            }
                        }

                        // Check for ending parenthesis //
                        if (OperationStack.IsEmpty() || !OperationStack.Pop().IsOpenBrace) throw new ArgumentException("Expecting '('. Invalid syntax.");

                        // Operation size could have changed, we need to check again //
                        if (!OperationStack.IsEmpty())
                        {
                            var topOperation = OperationStack.Peek();
                            if (topOperation.IsDivision || topOperation.IsMultiplication)
                            {
                                if (NumberStack.Count < 1) throw new ArgumentException("Invalid Syntax");

                                OperationStack.Pop(); // Passed our test, remove from stack //

                                var b = NumberStack.Pop();

                                if (!possibleNumber.HasValue)
                                    possibleNumber = NumberStack.Pop();

                                NumberStack.Push(topOperation.Apply(possibleNumber.Value, b));
                            }
                        }

                    }
                    else
                    {
                        throw new ArgumentException("Invalid Syntax. Cannot handle given operation.");
                    }
                }
            }

            if (OperationStack.Count >= 2)
                throw new ArgumentException("Invalid Syntax. Cannot handle given operation.");

            if (OperationStack.Count == 1)
            {
                var b = NumberStack.Pop();
                var a = NumberStack.Pop();

                return (int)OperationStack.Pop().Apply(a, b);
            }
            else
            {
                return (int)NumberStack.Pop();
            }
        }

        /// <summary>
        /// Gets an integer number, or returns null
        /// </summary>
        /// <param name="test">The string to parse</param>
        /// <returns>The value if parsable, null otherwise.</returns>
        private static double? getNumberOrNothing(string test)
        {
            double result;
            if (double.TryParse(test, out result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// A tiny structure for handling operations. Not needed, but helps keep things clean and scalable. With variables, it's &lt; 16 bytes.
        /// </summary>
        internal struct OperationToken
        {

            /// <summary>
            /// The operation belonging to this struct
            /// </summary>
            public char Operation {get; internal set;}

            /// <summary>
            /// Returns true if the current operation is multiplication
            /// </summary>
            public bool IsMultiplication
            {
                get
                {
                    return Operation == '*';
                }
            }

            /// <summary>
            /// Returns true if the current operation is division
            /// </summary>
            public bool IsDivision
            {
                get
                {
                    return Operation == '/' || Operation == '÷';
                }
            }

            /// <summary>
            /// Returns true if the current operation is addition
            /// </summary>
            public bool IsAddition
            {
                get
                {
                    return Operation == '+';
                }
            }


            /// <summary>
            /// Returns true if the current operation is addition
            /// </summary>
            public bool IsSubtraction
            {
                get
                {
                    return Operation == '-';
                }
            }

            /// <summary>
            /// If the operation is a opening parenthesis '('
            /// </summary>
            public bool IsOpenBrace
            {
                get
                {
                    return Operation == '(';
                }
            }

            /// <summary>
            /// If the operation is a closing parenthesis ')'
            /// </summary>
            public bool IsClosingBrace
            {
                get
                {
                    return Operation == ')';
                }
            }

            /// <summary>
            /// Returns true if provided operation/token can be handled as an operation
            /// </summary>
            public bool IsValid
            {
                get
                {
                    return IsAddition || IsSubtraction || IsMultiplication || IsDivision || IsOpenBrace || IsClosingBrace;
                }
            }

            /// <summary>
            /// Returns true if provided operation/token can be handled as an operation
            /// </summary>
            public bool IsOperation
            {
                get
                {
                    return IsAddition || IsSubtraction || IsMultiplication || IsDivision;
                }
            }

            /// <summary>
            /// Apply the operation to the specified integers.
            /// </summary>
            /// <param name="a">The fist integer</param>
            /// <param name="b">The second integer</param>
            /// <returns>The result of the application</returns>
            public double Apply(double a, double b)
            {
                if (IsMultiplication)
                    return a * b;
                if (IsAddition)
                    return a + b;
                if (IsSubtraction)
                    return a - b;
                if (IsDivision)
                {
                    if ((int)b == 0) throw new DivideByZeroException();
                    return a / b;
                }
                if (IsOpenBrace || IsClosingBrace)
                    throw new ArgumentException("Brace is not an acceptable operation");
                throw new ArgumentException("No acceptable operation found");
            }

            /// <summary>
            /// Returns string representation of this structure.
            /// </summary>
            /// <returns>String representation of this structure.</returns>
            public override string ToString()
            {
                return string.Format("\"{0}\"", Operation);
            }
        }


    }
}
