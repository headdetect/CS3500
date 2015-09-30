using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Evaluator class will take a string and evaluate it mathmatically.
    /// </summary>
    internal static class Evaluator
    {
        
        /// <summary>
        /// Evaluate a string based expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate</param>
        /// <param name="variableLookup">A delegate that should return the value of the variable specified.</param>
        /// <returns>An evaluated expression</returns>
        public static double Evaluate(string expression, Func<string, double> variableLookup)
        {
            // Get all tokens then remap and strip all whitespace //
            var tokens = Regex.Split(expression, "(?<=[-+*/(),])(?=.)|(?<=.)(?=[-+*/(),])")
                .Select(element => Regex.Replace(element, "\\s+", ""))
                .Where(string.IsNullOrWhiteSpace);

            return Evaluate(tokens, variableLookup);
        }


        /// <summary>
        /// Evaluate a string based expression.
        /// </summary>
        /// <param name="tokenEnumeration">The expression to evaluate</param>
        /// <param name="variableLookup">A delegate that should return the value of the variable specified.</param>
        /// <returns>An evaluated expression</returns>
        public static double Evaluate(IEnumerable<string> tokenEnumeration, Func<string, double> variableLookup)
        {
            try {
                var tokens = tokenEnumeration.ToArray();

                var operationStack = new Stack<OperationToken>();
                var numberStack = new Stack<double>();

                // Will either be a 1 or -1
                int? sign = null;

                for (var i = 0; i < tokens.Length; i++)
                {
                    var token = tokens[i];
                    var possibleNumber = GetNumberOrNothing(token);
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

                        if (!operationStack.IsEmpty())
                        {
                            var topOperation = operationStack.Peek();
                            if (topOperation.IsDivision || topOperation.IsMultiplication)
                            {
                                if (numberStack.Count < 1) throw new ArgumentException("Invalid Syntax");

                                operationStack.Pop(); // Passed our test, remove from stack //

                                var number = numberStack.Pop();

                                numberStack.Push(topOperation.Apply(number, possibleNumber.Value));

                                continue;
                            }

                        }

                        numberStack.Push(possibleNumber.Value);
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
                            var previousOperation = new OperationToken()
                            {
                                Operation = tokens[previousIndex][0]
                            };

                            // If the previous token was an operation then this token must mean positive or negative //
                            // Also make sure its not the last operation either //
                            if (previousOperation.IsOperation && i + 1 < tokens.Length)
                            {
                                sign = operation.IsSubtraction ? -1 : 1;
                                continue;
                            }
                            if (!operationStack.IsEmpty())
                            {
                                // There's a pending operation, let's do that instead //

                                var topOperation = operationStack.Peek();
                                if (topOperation.IsAddition || topOperation.IsSubtraction)
                                {
                                    if (numberStack.Count < 2) throw new ArgumentException("Invalid Syntax");

                                    operationStack.Pop();

                                    var number1 = numberStack.Pop();
                                    var number2 = numberStack.Pop();

                                    numberStack.Push(topOperation.Apply(number2, number1));
                                }
                            }


                            operationStack.Push(operation);
                        }
                        else if (operation.IsMultiplication || operation.IsDivision || operation.IsOpenBrace)
                            // Nothing complex, just push operation to stack //
                            operationStack.Push(operation);
                        else if (operation.IsClosingBrace)
                        {
                            if (!operationStack.IsEmpty())
                            {
                                // If there's already a pending operation. //
                                var topOperation = operationStack.Peek();
                                if (topOperation.IsAddition || topOperation.IsSubtraction)
                                {
                                    if (numberStack.Count < 2) throw new ArgumentException("Invalid Syntax");

                                    operationStack.Pop();

                                    var number1 = numberStack.Pop();
                                    var number2 = numberStack.Pop();

                                    numberStack.Push(topOperation.Apply(number2, number1));

                                }
                            }

                            // Check for ending parenthesis //
                            if (operationStack.IsEmpty() || !operationStack.Pop().IsOpenBrace) throw new ArgumentException("Expecting '('. Invalid syntax.");

                            // Operation size could have changed, we need to check again //
                            if (!operationStack.IsEmpty())
                            {
                                var topOperation = operationStack.Peek();
                                if (topOperation.IsDivision || topOperation.IsMultiplication)
                                {
                                    if (numberStack.Count < 1) throw new ArgumentException("Invalid Syntax");

                                    operationStack.Pop(); // Passed our test, remove from stack //

                                    var b = numberStack.Pop();
                                    
                                    possibleNumber = numberStack.Pop();

                                    numberStack.Push(topOperation.Apply(possibleNumber.Value, b));
                                }
                            }

                        }
                        else
                        {
                            throw new ArgumentException("Invalid Syntax. Cannot handle given operation.");
                        }
                    }
                }

                if (operationStack.Count >= 2)
                    throw new ArgumentException("Invalid Syntax. Cannot handle given operation.");

                if (operationStack.Count == 1)
                {
                    var b = numberStack.Pop();
                    var a = numberStack.Pop();

                    return operationStack.Pop().Apply(a, b);
                }

                return numberStack.Pop();
            }
            catch (Exception e)
            {
                if (e is DivideByZeroException)
                    throw e; // Re-throw

                throw new ArgumentException(e.Message);
            }
        }

        /// <summary>
        /// Gets an integer number, or returns null
        /// </summary>
        /// <param name="test">The string to parse</param>
        /// <returns>The value if parsable, null otherwise.</returns>
        private static double? GetNumberOrNothing(string test)
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
            /// Creates a new structure for handling tokens
            /// </summary>
            /// <param name="token">The token of this operation</param>
            internal OperationToken(char token)
            {
                Operation = token;
            }

            /// <summary>
            /// The operation belonging to this struct
            /// </summary>
            public char Operation {get; internal set;}

            /// <summary>
            /// Returns true if the current operation is multiplication
            /// </summary>
            public bool IsMultiplication => Operation == '*';

            /// <summary>
            /// Returns true if the current operation is division
            /// </summary>
            public bool IsDivision => Operation == '/' || Operation == '÷';

            /// <summary>
            /// Returns true if the current operation is addition
            /// </summary>
            public bool IsAddition => Operation == '+';


            /// <summary>
            /// Returns true if the current operation is addition
            /// </summary>
            public bool IsSubtraction => Operation == '-';

            /// <summary>
            /// If the operation is a opening parenthesis '('
            /// </summary>
            public bool IsOpenBrace => Operation == '(';

            /// <summary>
            /// If the operation is a closing parenthesis ')'
            /// </summary>
            public bool IsClosingBrace => Operation == ')';

            /// <summary>
            /// Returns true if provided operation/token can be handled as an operation
            /// </summary>
            public bool IsValid => IsAddition || IsSubtraction || IsMultiplication || IsDivision || IsOpenBrace || IsClosingBrace;

            /// <summary>
            /// Returns true if provided operation/token can be handled as an operation
            /// </summary>
            public bool IsOperation => IsAddition || IsSubtraction || IsMultiplication || IsDivision;

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
                    if (b == 0) throw new DivideByZeroException("Division by zero is a nono");
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
                return Operation.ToString();
            }
        }


    }
}
