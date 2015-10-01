using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace SpreadsheetTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var spreadsheet = new SS.Spreadsheet();

            spreadsheet.SetCellContents("A1", 2.0);
            spreadsheet.SetCellContents("B1", 45.0);
            spreadsheet.SetCellContents("C1", new Formula("A1 + B1"));

            var formula = spreadsheet.GetCellContents("C1") as Formula;

            Assert.IsNotNull(formula);
            Assert.AreEqual(47.0d, formula.GetVariables());
        }
    }
}
