using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using SS;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;


namespace UITests
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class CodedUiTest
    {
        public CodedUiTest()
        {
        }

        private static Spreadsheet GetSpreadsheet(UITestControl window)
        {
            return window.GetProperty("Spreadsheet") as Spreadsheet;
        }

        [TestMethod]
        public void MakeSureTextboxEmtpy()
        {
            this.UIMap.MakeSureTextboxEmpty();
            Assert.IsTrue(string.IsNullOrWhiteSpace(UIMap.MakeSureTextboxEmptyParams.UICellContentTextBoxEditText));
            var spreadsheet = GetSpreadsheet(UIMap.UISpreadsheetuntitledWindow);

            Assert.AreEqual(spreadsheet.GetCellContents("A1"), 2); // Make sure it puts 2 //
        }

        [TestMethod]
        public void MakeSureTextboxEmptyOnClear()
        {
            this.UIMap.MakeSureTextboxEmpty();
            Assert.IsTrue(string.IsNullOrWhiteSpace(UIMap.MakeSureTextboxEmptyParams.UICellContentTextBoxEditText));

        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //}

        #endregion

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;

        public UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;
    }
}
