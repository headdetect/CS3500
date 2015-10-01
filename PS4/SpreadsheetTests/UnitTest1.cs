using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using SS;

namespace SpreadsheetTests
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// Gets the internal variable resolver for getting cells.
        /// Because the only thing we're allowed to make public is the
        /// already defined functions and the Constructor. So, we had
        /// to get tricky and make it a private Func so we can access it 
        /// using the Private Object method.
        /// </summary>
        /// <param name="spreadsheet">The spreadsheet that we are getting the function from</param>
        /// <returns>The function that resolves variables</returns>
        private Func<string, double> _variableResolver(SS.Spreadsheet spreadsheet)
        {
            var privateObject = new PrivateObject(spreadsheet);
            return privateObject.GetFieldOrProperty("_resolver") as Func<string, double>;
        }
            
        [TestMethod]
        public void TestMethod1()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod2()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));
            spreadsheet.SetCellContents("D1", new Formula("C1 + A1"));

            var resolve = _variableResolver(spreadsheet);

            var d1 = spreadsheet.GetCellContents("D1") as Formula;

            Assert.IsNotNull(d1);
            Assert.AreEqual(d1, new Formula("C1 + A1"));

            Assert.AreEqual(49.0d, d1.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod3()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 1);
            spreadsheet.SetCellContents("A2", 2);
            spreadsheet.SetCellContents("A3", new Formula("A1 + A2"));
            spreadsheet.SetCellContents("B1", 3);
            spreadsheet.SetCellContents("B2", 4);
            spreadsheet.SetCellContents("B3", new Formula("B1 + B2"));
            spreadsheet.SetCellContents("C3", new Formula("A3 + B3"));
            spreadsheet.SetCellContents("D3", new Formula("C3 / 2"));

            var resolve = _variableResolver(spreadsheet);

            var a3 = spreadsheet.GetCellContents("A3") as Formula;
            var b3 = spreadsheet.GetCellContents("B3") as Formula;
            var c3 = spreadsheet.GetCellContents("C3") as Formula;
            var d3 = spreadsheet.GetCellContents("D3") as Formula;

            Assert.IsNotNull(a3);
            Assert.IsNotNull(b3);
            Assert.IsNotNull(c3);
            Assert.IsNotNull(d3);

            Assert.AreEqual(3d, a3.Evaluate(resolve));
            Assert.AreEqual(7d, b3.Evaluate(resolve));
            Assert.AreEqual(10d, c3.Evaluate(resolve));
            Assert.AreEqual(5d, d3.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod4()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", "Title");
            spreadsheet.SetCellContents("A2", "Tesla #1");
            spreadsheet.SetCellContents("A3", "Tesla #2");
            spreadsheet.SetCellContents("A4", "Tesla #3");
            spreadsheet.SetCellContents("A5", "Total:");

            spreadsheet.SetCellContents("B1", "Sales");
            spreadsheet.SetCellContents("B2", 75000);
            spreadsheet.SetCellContents("B3", 85000);
            spreadsheet.SetCellContents("B4", 90000);
            spreadsheet.SetCellContents("B5", new Formula("B2+B3+B4"));

            var resolve = _variableResolver(spreadsheet);

            var a1 = spreadsheet.GetCellContents("A1") as string;
            var b2 = spreadsheet.GetCellContents("B2") as double?;
            var b5 = spreadsheet.GetCellContents("B5") as Formula;

            Assert.IsNotNull(a1);
            Assert.IsNotNull(b2);
            Assert.IsNotNull(b5);

            Assert.AreEqual("Title", a1);
            Assert.AreEqual(75000, b2);
            Assert.AreEqual(75000 + 85000 + 90000d, b5.Evaluate(resolve));
        }  

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestMethod5()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", new Formula("B1"));
            spreadsheet.SetCellContents("B1", new Formula("A1"));
            
        }  

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestMethod6()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", new Formula("B1"));
            spreadsheet.SetCellContents("B1", new Formula("C1"));
            spreadsheet.SetCellContents("C1", new Formula("D1"));
            spreadsheet.SetCellContents("D1", new Formula("E1"));
            spreadsheet.SetCellContents("E1", new Formula("A1"));
        }  
        [TestMethod]
        public void TestMethod7()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod8()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod9()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod10()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod11()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod12()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod13()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod14()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod15()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod16()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod17()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod18()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod19()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod20()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  
        [TestMethod]
        public void TestMethod21()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1+B1"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }
    }
}
