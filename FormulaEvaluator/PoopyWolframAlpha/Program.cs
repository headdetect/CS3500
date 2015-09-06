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
        static void Main(string[] args)
        {
            Console.WriteLine("To denote a variable do <expression>;A=2;B=33;C=-2 etc...");
            Console.WriteLine("Enter your expression:");
            var expression = Console.ReadLine();

            var tokens = expression.Split(';');
            if (tokens.Length > 1)
            {
                // Has variables //
                
            }

            var result = Evaluator.Evaluate("1+1", (varb) =>
            {
                return 0;
            });

            Console.WriteLine(result);
            Console.Read();
        }
    }
}
