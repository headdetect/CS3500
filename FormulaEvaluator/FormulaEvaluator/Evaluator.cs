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
            var tokens = Regex.Split(expression, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)")
                .Select(element => Regex.Replace(element, "\\s+", ""))
                .Where(element => element != "");

            var OperationStack = new Stack<OperationToken>();
            var NumberStack = new Stack<int>();

            foreach (var token in tokens)
            {
                int? possibleNumber = getNumberOrNothing(token);
                var operation = new OperationToken
                {
                    Operation = token
                };
                if (!operation.IsValid && !possibleNumber.HasValue)
                {
                    // Is a variable //
                    possibleNumber = variableLookup(token);
                }

                if (possibleNumber.HasValue)
                {
                    // Is number //

                    if (OperationStack.Count >= 1)
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
                        // If there's already a pending operation. //
                        if (OperationStack.Count >= 1)
                        {
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

                        OperationStack.Push(operation);
                    }
                    else if (operation.IsMultiplication || operation.IsDivision || operation.IsOpenBrace)
                        // Nothing complex, just push operation to stack //
                        OperationStack.Push(operation);
                    else if (operation.IsClosingBrace)
                    {
                        if (OperationStack.Count >= 1)
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
                        if (OperationStack.Count == 0 || !OperationStack.Pop().IsOpenBrace) throw new ArgumentException("Expecting '('. Invalid syntax.");
                        
                        // Operation size could have changed, we need to check again //
                        if (OperationStack.Count >= 1)
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

                return OperationStack.Pop().Apply(a, b);
            }
            else
            {
                return NumberStack.Pop();
            }
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
        internal struct OperationToken
        {

            /// <summary>
            /// The operation belonging to this struct
            /// </summary>
            public string Operation {get; internal set;}

            /// <summary>
            /// Returns true if the current operation is multiplication
            /// </summary>
            public bool IsMultiplication
            {
                get
                {
                    return Operation == "*";
                }
            }

            /// <summary>
            /// Returns true if the current operation is division
            /// </summary>
            public bool IsDivision
            {
                get
                {
                    return Operation == "/" || Operation == "÷";
                }
            }

            /// <summary>
            /// Returns true if the current operation is addition
            /// </summary>
            public bool IsAddition
            {
                get
                {
                    return Operation == "+";
                }
            }


            /// <summary>
            /// Returns true if the current operation is addition
            /// </summary>
            public bool IsSubtraction
            {
                get
                {
                    return Operation == "-";
                }
            }

            /// <summary>
            /// If the operation is a opening parenthesis '('
            /// </summary>
            public bool IsOpenBrace
            {
                get
                {
                    return Operation == "(";
                }
            }

            /// <summary>
            /// If the operation is a closing parenthesis ')'
            /// </summary>
            public bool IsClosingBrace
            {
                get
                {
                    return Operation == ")";
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
            /// Apply the operation to the specified integers.
            /// </summary>
            /// <param name="a">The fist integer</param>
            /// <param name="b">The second integer</param>
            /// <returns>The result of the application</returns>
            public int Apply(int a, int b)
            {
                if (IsMultiplication)
                    return a * b;
                if (IsAddition)
                    return a + b;
                if (IsSubtraction)
                    return a - b;
                if (IsDivision)
                    return a / b;
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
