using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SpreadsheetUtilities;

namespace ForumlaTest
{
    [TestClass]
    public class FormulaTester
    {
        private const double
            A = 5,
            B = 10,
            C = -3,
            D = 1,
            E = 0,
            F = 100;

        static Dictionary<string, double> DefaultVariableLookup = new Dictionary<string, double>() {
            {"A", A},
            {"B", B},
            {"C", C},
            {"D", D},
            {"E", E},
            {"F", F}
        };

        private static readonly Func<string, double> EmptyFunction = (str) =>
        {
            return 0;
        };

        private static readonly Func<string, double> DefaultVariableFunction = (str) =>
        {
            return DefaultVariableLookup[str];
        };

        #region Basic Tests

        [TestMethod]
        public void Test1()
        {
            Formula f = new Formula("1 + 1");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(2d, result);
        }

        [TestMethod]
        public void Test2()
        {
            Formula f = new Formula("1 + -1");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(0d, result);
        }

        [TestMethod]
        public void Test3()
        {
            Formula f = new Formula("1.5 * 2");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(3d, result);
        }

        [TestMethod]
        public void Test4()
        {
            Formula f = new Formula("1 / 2");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(.5d, result);
        }

        [TestMethod]
        public void Test5()
        {
            Formula f = new Formula("1      +                                                       1");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(2d, result);
        }

        [TestMethod]
        public void Test6()
        {
            Formula f = new Formula("3+-1");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(2d, result);
        }

        [TestMethod]
        public void Test7()
        {
            Formula f = new Formula("2");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(2d, result);
        }

        [TestMethod]
        public void Test8()
        {
            Formula f = new Formula("2.0");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(2d, result);
        }

        [TestMethod]
        public void Test9()
        {
            Formula f = new Formula("2/.1");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(2d/.1, result);
        }

        [TestMethod]
        public void Test10()
        {
            Formula f = new Formula("1 + 1");
            Formula b = new Formula("1.0000000    +     1.0");
            
            Assert.AreEqual(f, b);
        }

        [TestMethod]
        public void Test11()
        {
            Formula f = new Formula("2");
            Formula b = new Formula("2.0000");

            Assert.AreEqual(f, b);
        }

        [TestMethod]
        public void Test12()
        {
            Formula f = new Formula("2 + 5 / 2 + 4 - 4 - 3 - 2");
            Formula b = new Formula("2 + 5 / 2 + 4 - 4 - 3 - 2");

            Assert.AreEqual(f, b);
        }

        [TestMethod]
        public void Test13()
        {
            Formula f = new Formula("3 + 5");
            Formula b = new Formula("5 + 3");

            Assert.AreNotEqual(f, b);
        }

        #endregion

        #region Basic Tests with variables
        [TestMethod]
        public void Test14()
        {
            Formula f = new Formula("1 + D");

            var result = f.Evaluate(DefaultVariableFunction);

            Assert.AreEqual(1d + D, result);
        }

        [TestMethod]
        public void Test15()
        {
            Formula f = new Formula("A + -1");

            var result = f.Evaluate(DefaultVariableFunction);

            Assert.AreEqual(A - 1d, result);
        }

        [TestMethod]
        public void Test16()
        {
            Formula f = new Formula("1.5 * 2 + C");

            var result = f.Evaluate(DefaultVariableFunction);

            Assert.AreEqual(3d + C, result);
        }

        [TestMethod]
        public void Test17()
        {
            Formula f = new Formula("1 / 2 - B");

            var result = f.Evaluate(DefaultVariableFunction);

            Assert.AreEqual(.5d - B, result);
        }

        [TestMethod]
        public void Test18()
        {
            Formula f = new Formula("B      +                                                       B");

            var result = f.Evaluate(DefaultVariableFunction);

            Assert.AreEqual(B + B, result);
        }

        [TestMethod]
        public void Test19()
        {
            Formula f = new Formula("3+-C");

            var result = f.Evaluate(DefaultVariableFunction);

            Assert.AreEqual(3 - C, result);
        }

        [TestMethod]
        public void Test20()
        {
            Formula f = new Formula("F");

            var result = f.Evaluate(DefaultVariableFunction);

            Assert.AreEqual(F, result);
        }

        [TestMethod]
        public void Test21()
        {
            Formula f = new Formula("C * 1.0");

            var result = f.Evaluate(DefaultVariableFunction);

            Assert.AreEqual(C, result);
        }

        [TestMethod]
        public void Test22()
        {
            Formula f = new Formula("E/.1");

            var result = f.Evaluate(DefaultVariableFunction);

            Assert.AreEqual(E / .1d, result);
        }

        [TestMethod]
        public void Test23()
        {
            Formula f = new Formula("1 + A");
            Formula b = new Formula("1.0000000    +     A");

            Assert.AreEqual(f, b);
        }

        [TestMethod]
        public void Test24()
        {
            Formula f = new Formula("B * 1");
            Formula b = new Formula("B * 1.000000000");

            Assert.AreEqual(f, b);
        }

        [TestMethod]
        public void Test25()
        {
            Formula f = new Formula("2 + 5 / C + 4 - 4 - D - 2");
            Formula b = new Formula("2 + 5 / C + 4 - 4 - D - 2");

            Assert.AreEqual(f, b);
        }

        [TestMethod]
        public void Test26()
        {
            Formula f = new Formula("D + C");
            Formula b = new Formula("C + D");

            Assert.AreNotEqual(f, b);
        }
        #endregion

        #region Validation and Normalization tests

        private static readonly Func<string, string> Normalize = (str) =>
        {
            return str.ToUpper();
        };

        private static readonly Func<string, bool> IsValid = (str) =>
        {
            return DefaultVariableLookup.ContainsKey(str);
        };

        [TestMethod]
        public void Test27()
        {
            Formula f = new Formula("a + b", Normalize, IsValid);
            Formula b = new Formula("A + B", Normalize, IsValid);

            Assert.AreEqual(f, b);
        }



        [TestMethod]
        public void Test28()
        {
            Formula f = new Formula("A + b", Normalize, IsValid);
            Formula b = new Formula("a + B", Normalize, IsValid);

            Assert.AreEqual(f, b);
        }

        [TestMethod]
        public void Test29()
        {
            Formula f = new Formula("A", Normalize, IsValid);
            Formula b = new Formula("a", Normalize, IsValid);

            Assert.AreEqual(f, b);
        }


        [TestMethod]
        public void Test30()
        {
            Formula f = new Formula("a", Normalize, IsValid);
            Formula b = new Formula("a", Normalize, IsValid);

            Assert.AreEqual(f, b);
        }

        #endregion

        #region Gimme them syntax errors
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test31()
        {
            Formula f = new Formula("a1", Normalize, IsValid);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test32()
        {
            Formula f = new Formula("A + C / 3 - 2 + A1", Normalize, IsValid);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test33()
        {
            Formula f = new Formula("A + C / 3 - 2 + A       1", Normalize, IsValid);
        }

        [TestMethod]
        public void Test34()
        {
            Formula f = new Formula("A + C /", Normalize, IsValid);
            Assert.IsInstanceOfType(f.Evaluate(DefaultVariableFunction), typeof(FormulaError));
        }

        [TestMethod]
        public void Test35()
        {
            Formula f = new Formula("A / 0", Normalize, IsValid);
            Assert.IsInstanceOfType(f.Evaluate(DefaultVariableFunction), typeof(FormulaError));
        }

        [TestMethod]
        public void Test36()
        {
            Formula f = new Formula("A - * - / - + 3", Normalize, IsValid);
            Assert.IsInstanceOfType(f.Evaluate(DefaultVariableFunction), typeof(FormulaError));
        }

        [TestMethod]
        public void Test37()
        {
            Formula f = new Formula("A-", Normalize, IsValid);
            Assert.IsInstanceOfType(f.Evaluate(DefaultVariableFunction), typeof(FormulaError));
        }

        [TestMethod]
        public void Test38()
        {
            Formula f = new Formula("", Normalize, IsValid);
            Assert.IsInstanceOfType(f.Evaluate(DefaultVariableFunction), typeof(FormulaError));
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Test39()
        {
            Formula f = new Formula("G", Normalize, IsValid);
        }

        [TestMethod]
        public void Test40()
        {
            Formula f = new Formula("3 ^ 2", Normalize, IsValid);
            Assert.IsInstanceOfType(f.Evaluate(DefaultVariableFunction), typeof(FormulaError));
        }
        #endregion

    }
}
