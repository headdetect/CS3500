using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using SS;

namespace SpreadsheetGUI
{
    public partial class Workbench : Form
    {
        /// <summary>
        /// Gets the active file for the spreadsheet gui.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets the associated spreadsheet
        /// </summary>
        public Spreadsheet Spreadsheet { get; private set; }

        /// <summary>
        /// Gets whether the current spreadsheet is untitled/unsaved
        /// </summary>
        public bool IsUntitled => string.IsNullOrEmpty(FileName);

        /// <summary>
        /// Gets whether the spreadsheet has been changed
        /// </summary>
        public bool Changed => Spreadsheet != null && Spreadsheet.Changed;

        private readonly Func<string, string> _normalizer = s => s.ToUpper();

        public Workbench(string fileName = "")
        {
            InitializeComponent();
            FileName = fileName;
        }

        private void Workbench_Load(object sender, EventArgs e)
        {
            LoadFile();
        }

        private void spreadsheetPanel_SelectionChanged(SS.SpreadsheetPanel sender)
        {
            int row = -1;
            int col = -1;
            sender.GetSelection(out col, out row);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var bench = new Workbench();
            bench.Show();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            var result = openFileDialog.ShowDialog(this);

            if (result != DialogResult.OK && result != DialogResult.Yes) return;

            FileName = openFileDialog.FileName;
            LoadFile();
        }


        private void LoadFile()
        {
            DoBackgroundWork(handler =>
            {
                try
                {
                    Spreadsheet = !string.IsNullOrWhiteSpace(FileName)
                        ? new Spreadsheet(FileName, s => Regex.IsMatch(s, @"[A-Z][0-9]{1,2}"), _normalizer)
                        : new Spreadsheet(s => Regex.IsMatch(s, @"[A-Z][0-9]{1,2}"), _normalizer);
                }
                catch (SpreadsheetReadWriteException)
                {
                    DoForegroundWork(() =>
                    {
                        MessageBox.Show(@"There was an error reading the file", @"Error reading file",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                    });

                    return;
                }

                foreach (var cell in Spreadsheet.GetNamesOfAllNonemptyCells())
                {
                    var value = Spreadsheet.GetCellValue(cell);
                    var point = GetPointFromCellName(cell);

                    DoForegroundWork(() => spreadsheetPanel.SetValue(point.X, point.Y, value.ToString()));

                    // TODO: Check for formulas
                }

                SetTitle();
            });
        }

        private void DoBackgroundWork(Action<DoWorkEventArgs> work)
        {
            var b = new BackgroundWorker();
            b.DoWork += (sender, e) => work(e);
            b.RunWorkerAsync();
        }

        private static readonly object ForegroundLock = new object();

        private void DoForegroundWork(Action work)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(work);
                }
                else
                {
                    lock (ForegroundLock)
                    {
                        work();
                    }
                }
            }
            catch
            {
            }

        }


        /// <summary>
        /// Sets the title of the window based on the given properties.
        /// Can be ran on any thread.
        /// </summary>
        private void SetTitle()
        {
            DoForegroundWork(() =>
            {
                if (IsUntitled)
                    Text = @"Spreadsheet - untitled";
                else
                    Text = @"Spreadsheet - " + Path.GetFileName(FileName);

                if (Changed) Text += @"*";
            });
        }

        #region Helpers

        private static IEnumerable<Tuple<int, int, string>> IterateCells(int height = 99)
        {
            for (var x = 'A'; x < 'Z'; x++)
            {
                for (var y = 1; y <= height; y++)
                {
                    yield return new Tuple<int, int, string>(x - 'A', y, x.ToString() + y);
                }
            }
        }

        private static readonly Point NonPoint = new Point(-1, -1);

        private static Point GetPointFromCellName(string cell)
        {
            if (cell.Length < 2)
                return NonPoint;

            int col = cell[0] - 'A'; // Convert the first char to int index
            int row = int.Parse(cell.Substring(1)) - 1; // Get the rest of the chars //

            return new Point(col, row);
        }

        #endregion

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsUntitled)
            {
                saveAsToolStripMenuItem_Click(sender, e);
                return;
            }

            Spreadsheet?.Save(FileName);
            SetTitle();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Title = @"Save the spreadsheet",
                Filter = @"Spreadsheet|.sprd"
            };

            saveDialog.ShowDialog();

            var fileName = saveDialog.FileName;
            if (string.IsNullOrWhiteSpace(fileName)) return;

            Spreadsheet?.Save(fileName);
            FileName = fileName;
            SetTitle();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        
    }
}
