using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            this.UIMap.FillSpreadsheet();
            this.UIMap.SimpleSaveSpreadsheet();

            this.UIMap.AssertFileSaved();

        }

        [TestMethod]
        public void DoesSaveAsFile()
        {
            this.UIMap.FillSpreadsheet();
            this.UIMap.SaveSpreadsheet();
        }

        [TestMethod]
        public void DoesStartCollaboration()
        {
            this.UIMap.HostCollaboration();
            this.UIMap.OpenCollaborationToolBox();
            this.UIMap.AssertHostingCollaborationStarted();
        }


        [TestMethod]
        public void DoesJoinCollaboration()
        {
            this.UIMap.JoinNullCollaboration();
            this.UIMap.AssertCantJoinCollaboration();
        }



        [TestMethod]
        public void OpensAboutDialog()
        {
            this.UIMap.OpenAboutDialog();
            this.UIMap.AssertAboutDialogOpen();
        }

        [TestMethod]
        public void OpensHelpDialog()
        {
            this.UIMap.OpenHelpDialog();
            this.UIMap.AssertHelpDialogOpen();
        }

        [TestMethod]
        public void OpensHelpDialogFromF1()
        {
            this.UIMap.OpenHelpDialogF1();
            this.UIMap.AssertHelpDialogOpen();
        }

        [TestMethod]
        public void PressEscClearsForm()
        {
            this.UIMap.FillTextbox();
            this.UIMap.PressEscOnTextbox();
            this.UIMap.MoveToA1();
            this.UIMap.AssertCellValueEmpty();
            this.UIMap.AssertTextboxEmpty();
        }

        [TestMethod]
        public void PressEnterOnTextbox()
        {
            this.UIMap.FillTextbox();
            this.UIMap.PressEnterInTextbox();
            this.UIMap.MoveToA1();
            this.UIMap.AssertCellValueHello();
        }

        [TestMethod]
        public void PressEx()
        {
            this.UIMap.FillTextbox();
            this.UIMap.ClickXButton();
            this.UIMap.MoveToA1();
            this.UIMap.AssertCellValueEmpty();
            this.UIMap.AssertTextboxEmpty();
        }

        [TestMethod]
        public void PressCheck()
        {
            this.UIMap.FillTextbox();
            this.UIMap.ClickCheckmark();
            this.UIMap.MoveToA1();
            this.UIMap.AssertCellValueHello();
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        private Process proc;

        [TestInitialize]
        public void PreTest()
        {
            proc = Process.Start(Path.GetFullPath("../../../SpreadsheetGUI/bin/Debug/SpreadsheetGUI.exe"));
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            proc?.Close();
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
