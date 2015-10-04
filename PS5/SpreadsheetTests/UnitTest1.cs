using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using SS;
using Spreadsheet = SS.Spreadsheet;

namespace SpreadsheetTests
{
    [TestClass]
    public class UnitTest1
    {
        private readonly Func<string, bool> _isValid = SS.Spreadsheet.IsValidName;
        private readonly Func<string, string> _normalize = str => str.ToUpper(CultureInfo.CurrentCulture);
            
        [TestMethod]
        public void TestMethod1()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "2.0");
            spreadsheet.SetContentsOfCell("B1", "45.0");
            spreadsheet.SetContentsOfCell("C1", "=A1 + B1");
            
            var result = spreadsheet.GetCellValue("C1");

            Assert.AreEqual(47.0d, result);
        }  

        [TestMethod]
        public void TestMethod2()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "2.0");
            spreadsheet.SetContentsOfCell("B1", "45.0");
            spreadsheet.SetContentsOfCell("C1","=A1 + B1");
            spreadsheet.SetContentsOfCell("D1", "=C1 + A1");
            
            var d1 = spreadsheet.GetCellValue("D1");

            Assert.IsNotNull(d1);
            Assert.AreEqual(d1, 2 + 45 + 2d);
        }  
        [TestMethod]
        public void TestMethod3()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "1");
            spreadsheet.SetContentsOfCell("A2", "2");
            spreadsheet.SetContentsOfCell("A3", "=A1 + A2");
            spreadsheet.SetContentsOfCell("B1", "3");
            spreadsheet.SetContentsOfCell("B2", "4");
            spreadsheet.SetContentsOfCell("B3", "=B1 + B2");
            spreadsheet.SetContentsOfCell("C3", "=A3 + B3");
            spreadsheet.SetContentsOfCell("D3", "=C3 / 2");
            
            var a3 = spreadsheet.GetCellValue("A3");
            var b3 = spreadsheet.GetCellValue("B3");
            var c3 = spreadsheet.GetCellValue("C3");
            var d3 = spreadsheet.GetCellValue("D3");

            Assert.AreEqual(3d, a3);
            Assert.AreEqual(7d, b3);
            Assert.AreEqual(10d, c3);
            Assert.AreEqual(5d, d3);
        }  
        [TestMethod]
        public void TestMethod4()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "Title");
            spreadsheet.SetContentsOfCell("A2", "Tesla #1");
            spreadsheet.SetContentsOfCell("A3", "Tesla #2");
            spreadsheet.SetContentsOfCell("A4", "Tesla #3");
            spreadsheet.SetContentsOfCell("A5", "Total:");

            spreadsheet.SetContentsOfCell("B1", "Sales");
            spreadsheet.SetContentsOfCell("B2", "75000");
            spreadsheet.SetContentsOfCell("B3", "85000");
            spreadsheet.SetContentsOfCell("B4", "90000");
            spreadsheet.SetContentsOfCell("B5", "=B2+B3+B4");

            var a1 = spreadsheet.GetCellValue("A1");
            var b2 = spreadsheet.GetCellValue("B2");
            var b5 = spreadsheet.GetCellValue("B5");

            Assert.IsNotNull(a1);
            Assert.IsNotNull(b2);
            Assert.IsNotNull(b5);

            Assert.AreEqual("Title", a1);
            Assert.AreEqual(75000d, b2);
            Assert.AreEqual(75000 + 85000 + 90000d, b5);
        }  

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestMethod5()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "=B1");
            spreadsheet.SetContentsOfCell("B1", "=A1");
        }  

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestMethod6()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "=B1");
            spreadsheet.SetContentsOfCell("B1", "=C1");
            spreadsheet.SetContentsOfCell("C1", "=D1");
            spreadsheet.SetContentsOfCell("D1", "=E1");
            spreadsheet.SetContentsOfCell("E1", "=A1");
        }  
        

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestMethod8()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("+quack", "2.0");
        }  

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestMethod9()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("_+quack", "2.0");
        }  


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestMethod10()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", null);
        }
        
        [TestMethod]
        public void TestMethod13()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);
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

                    spreadsheet.SetContentsOfCell(cell, randomNum.ToString());
                    sum += randomNum;

                    // Adds the cell with a + //
                    expression += (y == count - 1) ? cell : cell + "+";
                }
                
                var cellName = x + count.ToString();

                spreadsheet.SetContentsOfCell(cellName, "=" + expression);

                var formula = spreadsheet.GetCellValue(cellName);

                Assert.IsNotNull(formula);

                Assert.AreEqual(sum, formula);
            }
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidNameException))]
        public void TestMethod14()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);
            spreadsheet.SetContentsOfCell(null, "2.0");
        }

        [TestMethod]
        public void TestMethod15()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "2.0");
            spreadsheet.SetContentsOfCell("C1", "=A1");
            
            var formula = spreadsheet.GetCellValue("C1");

            Assert.IsNotNull(formula);

            Assert.AreEqual(2d, formula);
        }  

        [TestMethod]
        public void TestMethod16()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            var random = new Random();

            var listOfNonEmpty = new List<string>();

            foreach (var cell in GenerateCells(26))
            {
                var shouldBeEmpty = random.Next() % 2 == 0;

                spreadsheet.SetContentsOfCell(cell, shouldBeEmpty ? string.Empty : "Not empty bro");

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
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "2.0");
            spreadsheet.SetContentsOfCell("B1", "45.0");
            spreadsheet.SetContentsOfCell("C1", "=A1 + B1");

            var result = spreadsheet.SetContentsOfCell("A1", "3.0"); // Should update them other cells //
            
            Assert.IsTrue(result.Contains("A1") && result.Contains("C1"));
        }

        [TestMethod]
        public void TestMethod18()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "2.0");
            spreadsheet.SetContentsOfCell("B1", "45.0");
            spreadsheet.SetContentsOfCell("C1", "=A1 + B1");

            var result = spreadsheet.SetContentsOfCell("C1", "3.0");

            Assert.IsTrue(!result.Contains("B1") && !result.Contains("A1"));
        }


        [TestMethod]
        public void TestMethod19()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "2.0");
            spreadsheet.SetContentsOfCell("B1", "45.0");
            spreadsheet.SetContentsOfCell("C1", "=A1 + B1");
            spreadsheet.SetContentsOfCell("D1", "=A1 + B1");
            spreadsheet.SetContentsOfCell("E1", "=A1 + B1");

            var result = spreadsheet.SetContentsOfCell("A1", "3.0");

            Assert.IsTrue(result.Contains("C1") && result.Contains("D1") && result.Contains("E1"));
        }  
        [TestMethod]
        public void TestMethod20()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "2.0");
            spreadsheet.SetContentsOfCell("B1", "45.0");
            spreadsheet.SetContentsOfCell("C1", "=A1 + B1");
            spreadsheet.SetContentsOfCell("D1", "=C1 + B1");
            spreadsheet.SetContentsOfCell("E1", "=D1 + B1");

            var result = spreadsheet.SetContentsOfCell("A1", "3.0");

            Assert.IsTrue(result.Contains("C1") && result.Contains("D1") && result.Contains("E1") && result.Contains("A1"));
        }

        [TestMethod]
        public void TestMethod21()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "2.0");
            spreadsheet.SetContentsOfCell("B1", "45.0");
            spreadsheet.SetContentsOfCell("C1", "=A1 + B1");
            spreadsheet.SetContentsOfCell("D1", "=C1 + B1");
            spreadsheet.SetContentsOfCell("E1", "=D1 + B1");
            
            spreadsheet.Save("Test21.cellular");

            Assert.IsTrue(File.Exists("Test21.cellular"));
        }

        [TestMethod]
        public void TestMethod22()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "2.0");
            spreadsheet.SetContentsOfCell("B1", "45.0");
            spreadsheet.SetContentsOfCell("C1", "=A1 + B1");
            spreadsheet.SetContentsOfCell("D1", "=C1 + B1");
            spreadsheet.SetContentsOfCell("E1", "=D1 + B1");

            spreadsheet.Save("Test22.cellular");

            var version = spreadsheet.GetSavedVersion("Test22.cellular");

            Assert.AreEqual(version, SS.Spreadsheet.CurrentVersion);
        }

        [TestMethod]
        public void TestMethod23()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize, "v0.0.0a (#NoFilter)");

            spreadsheet.SetContentsOfCell("A1", "2.0");
            spreadsheet.SetContentsOfCell("B1", "45.0");
            spreadsheet.SetContentsOfCell("C1", "=A1 + B1");
            spreadsheet.SetContentsOfCell("D1", "=C1 + B1");
            spreadsheet.SetContentsOfCell("E1", "=D1 + B1");

            spreadsheet.Save("Test23.cellular");

            var version = spreadsheet.GetSavedVersion("Test23.cellular");

            Assert.AreEqual(version, "v0.0.0a (#NoFilter)");
        }

        [TestMethod]
        public void TestMethod24()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "2.0");
            spreadsheet.SetContentsOfCell("B1", "45.0");
            spreadsheet.SetContentsOfCell("C1", "=A1 + B1");

            spreadsheet.Save("Test24.cellular");
            var contents = Regex.Replace(File.ReadAllText("Test24.cellular"), @"\s", string.Empty);
            var shouldBe =
                Regex.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<spreadsheet version=\"v0.0.1 (Duck Face)\">\r\n  <cells>\r\n    <cell>\r\n      <name>A1</name>\r\n      <contents>2</contents>\r\n    </cell>\r\n    <cell>\r\n      <name>B1</name>\r\n      <contents>45</contents>\r\n    </cell>\r\n    <cell>\r\n      <name>C1</name>\r\n      <contents>=A1+B1</contents>\r\n    </cell>\r\n  </cells>\r\n</spreadsheet>", @"\s", string.Empty);

            Assert.AreEqual(contents, shouldBe);
        }

        [TestMethod]
        public void TestMethod25()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);

            spreadsheet.SetContentsOfCell("A1", "2.0");

            Assert.IsTrue(spreadsheet.Changed);

            spreadsheet.Save("Test25.cellular");

            Assert.IsFalse(spreadsheet.Changed);

            spreadsheet.SetContentsOfCell("A1", "3.0");

            Assert.IsTrue(spreadsheet.Changed);

            spreadsheet.Save("Test25.cellular");

            Assert.IsFalse(spreadsheet.Changed);
        }

        [TestMethod]
        public void TestMethod26()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);
            var random = new Random();

            foreach (var cell in GenerateCells(100))
            {
                spreadsheet.SetContentsOfCell(cell, (1000 * random.NextDouble()).ToString(CultureInfo.InvariantCulture));
            }

            spreadsheet.Save("Test26.cellular");

            var spreadsheet2 = new SS.Spreadsheet("Test26.cellular", _isValid, _normalize);

            var insideSpreadsheet = new PrivateObject(spreadsheet);
            var cells = insideSpreadsheet.GetFieldOrProperty("_cells") as Dictionary<string, Cell>;

            Assert.IsNotNull(cells);

            foreach (var cell in cells)
            {
                var value1 = spreadsheet.GetCellValue(cell.Key);
                var value2 = spreadsheet2.GetCellValue(cell.Key);
                var content1 = spreadsheet.GetCellContents(cell.Key);
                var content2 = spreadsheet2.GetCellContents(cell.Key);

                Assert.AreEqual(value1, value2);
                Assert.AreEqual(content1, content2);
            }
            
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestMethod27()
        {
            // Spreadsheet that contains a circular exception //
            const string circles = "<?xml version=\"1.0\" encoding=\"utf-8\"?><spreadsheet version=\"v0.0.1 (Duck Face)\"><cells><cell><name>A1</name><contents>=B1</contents></cell><cell><name>B1</name><contents>=A1</contents></cell></cells></spreadsheet>"; 

            File.WriteAllText("Test27.cellular", circles);

            var spreadsheet = new SS.Spreadsheet("Test27.cellular", _isValid, _normalize);
        }


        [TestMethod]
        public void TestMethod28()
        {
            var spreadsheet = new SS.Spreadsheet(_isValid, _normalize);
            var random = new Random();

            foreach (var cell in GenerateCells(10000))
            {
                spreadsheet.SetContentsOfCell(cell, (1000 * random.NextDouble()).ToString(CultureInfo.InvariantCulture));
            }

            spreadsheet.Save("Test26.cellular");
        }

        private IEnumerable<string> GenerateCells(int height)
        {
            for (var x = 'A'; x < 'Z'; x++)
            {
                for (var y = 1; y <= height; y++)
                {
                    yield return x.ToString() + y;
                }
            }
        }
    }
}
