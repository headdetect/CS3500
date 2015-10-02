using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

            spreadsheet.SetCellContents("_kanye_west", 2.0);
            spreadsheet.SetCellContents("_kanye_north", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("_kanye_west + _kanye_north"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("_kanye_west + _kanye_north"));

            Assert.AreEqual(47.0d, formula.Evaluate(resolve));
        }  

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestMethod8()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("+quack", 2.0);
        }  

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestMethod9()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("_+quack", 2.0);
        }  


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestMethod10()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", (string)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestMethod11()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", (Formula)null);
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
            var resolve = _variableResolver(spreadsheet);
            var random = new Random();

            for (var x = 'A'; x <= 'Z'; x++)
            {
                double sum = 0;
                var expression = "";

                const int count = 30;
                for (var y = 1; y < count; y++)
                {
                    var cell = x.ToString() + y;
                    var randomNum = random.Next(1, 200);

                    spreadsheet.SetCellContents(cell, randomNum);
                    sum += randomNum;

                    // Adds the cell with a + //
                    expression += (y == count - 1) ? cell : cell + "+";
                }
                
                var cellName = x + count.ToString();

                spreadsheet.SetCellContents(cellName, new Formula(expression));

                var formula = spreadsheet.GetCellContents(cellName) as Formula;

                Assert.IsNotNull(formula);

                Assert.AreEqual(sum, formula.Evaluate(resolve));
            }
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidNameException))]
        public void TestMethod14()
        {
            var spreadsheet = new SS.Spreadsheet();
            spreadsheet.SetCellContents(null, 2.0);
        }

        [TestMethod]
        public void TestMethod15()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("C1", new Formula("A1"));

            var resolve = _variableResolver(spreadsheet);

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(formula, new Formula("A1"));

            Assert.AreEqual(2d, formula.Evaluate(resolve));
        }  

        [TestMethod]
        public void TestMethod16()
        {
            var spreadsheet = new SS.Spreadsheet();

            var random = new Random();

            var listOfNonEmpty = new List<string>();

            foreach (var cell in GenerateCells(26, 26))
            {
                var shouldBeEmpty = random.Next() % 2 == 0;

                spreadsheet.SetCellContents(cell, shouldBeEmpty ? string.Empty : "Not empty bro");

                if (!shouldBeEmpty)
                    listOfNonEmpty.Add(cell);
            }

            var notEmpties = spreadsheet.GetNamesOfAllNonemptyCells().ToList();

            foreach (var element in notEmpties)
            {
                Assert.IsTrue(listOfNonEmpty.Contains(element));
            }
            
        }  
        [TestMethod]
        public void TestMethod17()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var result = spreadsheet.SetCellContents("A1", 3.0); // Should update them other cells //
            
            Assert.IsTrue(result.Contains("A1") && result.Contains("C1"));
        }

        [TestMethod]
        public void TestMethod18()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var result = spreadsheet.SetCellContents("C1", 3.0);

            Assert.IsTrue(!result.Contains("B1") && !result.Contains("A1"));
        }


        [TestMethod]
        public void TestMethod19()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));
            spreadsheet.SetCellContents("D1", new Formula("A1 + B1"));
            spreadsheet.SetCellContents("E1", new Formula("A1 + B1"));

            var result = spreadsheet.SetCellContents("A1", 3.0);

            Assert.IsTrue(result.Contains("C1") && result.Contains("D1") && result.Contains("E1"));
        }  
        [TestMethod]
        public void TestMethod20()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));
            spreadsheet.SetCellContents("D1", new Formula("C1 + B1"));
            spreadsheet.SetCellContents("E1", new Formula("D1 + B1"));

            var result = spreadsheet.SetCellContents("A1", 3.0);

            Assert.IsTrue(result.Contains("C1") && result.Contains("D1") && result.Contains("E1") && result.Contains("A1"));
        }  

        private IEnumerable<string> GenerateCells(int width, int height)
        {
            for (var x = 'A'; x < 'A' + width; x++)
            {
                for (var y = 1; y <= height; y++)
                {
                    yield return x.ToString() + y;
                }
            }
        }
    }
}
