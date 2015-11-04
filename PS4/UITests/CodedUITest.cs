using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using SpreadsheetGUI;
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


        [TestMethod]
        public void TextboxEmtpy()
        {
            this.UIMap.FillSpreadsheet();
            this.UIMap.AssertIsEmpty();
        }

        [TestMethod]
        public void MatchesFormula()
        {
            this.UIMap.FillSpreadsheet();
            this.UIMap.MoveToC3();
            this.UIMap.AssertFormulaMatches();
        }

        [TestMethod]
        public void MatchesCellValue()
        {
            this.UIMap.FillSpreadsheet();
            this.UIMap.MoveToC3();
            this.UIMap.AssertCellValueMatches();

        }

        [TestMethod]
        public void MatchesCellName()
        {
            this.UIMap.FillSpreadsheet();
            this.UIMap.MoveToC3();
            this.UIMap.AssertCellNameMatches();

        }

        [TestMethod]
        public void ShowsChanges()
        {
            this.UIMap.FillSpreadsheet();
            this.UIMap.AssertShowsChanges();
        }

        [TestMethod]
        public void DoesNewFile()
        {
            this.UIMap.FillSpreadsheet();
            this.UIMap.SaveSpreadsheet();
            this.UIMap.ClearSpreadsheet();
            this.UIMap.AssertIsNewFile();
        }

        [TestMethod]
        public void DoesSaveFile()
        {
            this.UIMap.FillSpreadsheet();
            this.UIMap.SaveSpreadsheet();
            this.UIMap.AssertIsSaved();

        }

        [TestMethod]
        public void DoesSaveAsFile()
        {
            this.UIMap.FillSpreadsheet();
            this.UIMap.SaveSpreadsheet();
        }


        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:
        
        [TestInitialize]
        public void PreTest()
        {
            //TODO: Open executable here //
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            //TODO: Close executable here //
        }

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
