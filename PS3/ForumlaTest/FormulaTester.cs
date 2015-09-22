using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SpreadsheetUtilities;

namespace ForumlaTest
{
    [TestClass]
    public class FormulaTester
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

        private static readonly Func<string, double> EmptyFunction = (str) =>
        {
            return 0;
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
            Formula f = new Formula("1 + 1");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(2d, result);
        }

        [TestMethod]
        public void Test15()
        {
            Formula f = new Formula("1 + -1");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(0d, result);
        }

        [TestMethod]
        public void Test16()
        {
            Formula f = new Formula("1.5 * 2");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(3d, result);
        }

        [TestMethod]
        public void Test17()
        {
            Formula f = new Formula("1 / 2");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(.5d, result);
        }

        [TestMethod]
        public void Test18()
        {
            Formula f = new Formula("1      +                                                       1");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(2d, result);
        }

        [TestMethod]
        public void Test19()
        {
            Formula f = new Formula("3+-1");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(2d, result);
        }

        [TestMethod]
        public void Test20()
        {
            Formula f = new Formula("2");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(2d, result);
        }

        [TestMethod]
        public void Test21()
        {
            Formula f = new Formula("2.0");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(2d, result);
        }

        [TestMethod]
        public void Test22()
        {
            Formula f = new Formula("2/.1");

            var result = f.Evaluate(EmptyFunction);

            Assert.AreEqual(2d / .1, result);
        }

        [TestMethod]
        public void Test23()
        {
            Formula f = new Formula("1 + 1");
            Formula b = new Formula("1.0000000    +     1.0");

            Assert.AreEqual(f, b);
        }

        [TestMethod]
        public void Test24()
        {
            Formula f = new Formula("2");
            Formula b = new Formula("2.0000");

            Assert.AreEqual(f, b);
        }

        [TestMethod]
        public void Test25()
        {
            Formula f = new Formula("2 + 5 / 2 + 4 - 4 - 3 - 2");
            Formula b = new Formula("2 + 5 / 2 + 4 - 4 - 3 - 2");

            Assert.AreEqual(f, b);
        }

        [TestMethod]
        public void Test26()
        {
            Formula f = new Formula("3 + 5");
            Formula b = new Formula("5 + 3");

            Assert.AreNotEqual(f, b);
        }
        #endregion

        #region Validation and Normalization tests
        #endregion

        #region Gimme them syntax errors
        #endregion

    }
}
