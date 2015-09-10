using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FormulaEvaluator;

namespace PoopyWolframAlpha
{
    class Program
    {
        private const int
            A = 5,
            B = 10,
            C = -3,
            D = 1,
            E = 0,
            F = 100;

        static Dictionary<string, int> DefaultVariableLookup = new Dictionary<string, int>() {
            {"A", A},
            {"B", B},
            {"C", C},
            {"D", D},
            {"E", E},
            {"F", F}
        };

        static void Main(string[] args)
        {
            WriteLine("Press Enter to begin tests");
            Console.ReadLine();

            // These tests all assume C# arithmic parser is correct. //

            // Simple operations //

            Test("3", 3);
            Test("2 + 4", 2 + 4);
            Test("5 * 3", 5 * 3);
            Test("12 / 6", 12 / 6);
            Test("4 - 2", 4 - 2);
            Test("2 - 4", 2 - 4);
            Test("6 / 1", 6 / 1);

            // Simple operations with variables // 
            Test("A", A);
            Test("2 + A", 2 + A);
            Test("5 * B", 5 * B);
            Test("12 / C", 12 / C);
            Test("4 - D", 4 - D);
            Test("E - 2", E - 2);
            Test("600 / F", 600 / F);

            // Long operations with variables // 
            Test("2 + A + A + 3 + 3", 2 + A + A + 3 + 3);
            Test("5 * B * A * C * 2", 5 * B * A * C * 2);
            Test("12 / C / 4", 12 / C / 4);
            Test("E - 2 - 5 - 9 - A", E - 2 - 5 - 9 - A);
            Test("600 / F / 2", 600 / F / 2);
            
            // Complex operations with variables & parens // 
            Test("2 + A - A + 3 - 3", 2 + A - A + 3 - 3);
            Test("5 * B / A * C / 2", 5 * B / A * C / 2);
            Test("E + 2 - 5 / 9 * A", (int)(E + 2 - 5 / (double)9 * A));
            Test("(A + B) - D * (F - C)", (A + B) - D * (F - C));
            Test("(A - 4) + D / (F + C)", (A - 4) + D / (F + C));
            Test("(A * 3) * (9 * 3) + D - E * (F - C) + (4 - 3) + A", (A * 3) * (9 * 3) + D - E * (F - C) + (4 - 3) + A);

            // Gimme them errors //
            Test("2 / 0", shouldFail: true); // Divide by 0 //
            Test("G + 3", shouldFail: true); // Variable not found //
            Test("2^2", shouldFail: true); // Not a valid operation //
            Test("1 + ", shouldFail: true); // Not a valid syntax //
            Test("1 + )", shouldFail: true); // Not a valid syntax //
            Test("(3 + 4 + 5", shouldFail: true); // Not a valid syntax //
            Test("C / E", shouldFail: true); // Divide by 0 //

            WriteLine("To denote a variable do <expression>;A=2;B=33;C=-2 etc...");

            while (true)
            {
                WriteLine("Enter your expression (type Quit to exit):");
                var expression = Console.ReadLine();

                if (expression.ToLower() == "quit")
                    return;

                Test(expression);
            }
        }

        static void Test(string expression, double? shouldBe = null, bool shouldFail = false)
        {
            Dictionary<string, int> lookupDictionary = null;

            try
            {
                var tokens = expression.Split(';');
                if (tokens.Length > 1)
                {
                    // Has variables //
                    expression = tokens[0];
                    lookupDictionary = new Dictionary<string, int>(tokens.Length - 1);

                    for (int i = 1; i < tokens.Length; i++)
                    {
                        var token = tokens[i];
                        var key = token.Split('=')[0];
                        var value = int.Parse(token.Split('=')[1]);
                        lookupDictionary.Add(key, value);
                    }
                }

                lookupDictionary = lookupDictionary ?? DefaultVariableLookup;

                var result = Evaluator.Evaluate(expression, varb => lookupDictionary[varb]);

                Write("(");
                if (shouldBe.HasValue)
                    if (shouldBe.Value == result)
                        Write("Pass", ConsoleColor.Green);
                    else
                        Write("Fail", ConsoleColor.Red);
                else Write("???", ConsoleColor.Red);
                Write(") " + expression + " = " + result);
                if (shouldBe.HasValue && shouldBe.Value != result)
                    Write(" (expecting: " + shouldBe.Value + ")");
                Write("\n");

            }
            catch (Exception e)
            {
                Write("(");
                if (shouldFail) Write("Pass", ConsoleColor.Green);
                else Write("Fail", ConsoleColor.Red);
                WriteLine(") " + expression + " = " + e.Message);
            }
        }

        private static void Write(string write, ConsoleColor color = ConsoleColor.White)
        {
            var pre = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(write);
            Console.ForegroundColor = pre;
        }

        private static void WriteLine(string write, ConsoleColor color = ConsoleColor.White)
        {
            Write(write + "\n", color);
        }
    }
}
