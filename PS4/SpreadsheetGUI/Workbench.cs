using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
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
        public string ActiveFile { get; }

        /// <summary>
        /// Gets the associated spreadsheet
        /// </summary>
        public Spreadsheet Spreadsheet { get; private set; }

        private readonly Func<string, string> _normalizer = s => s.ToUpper();

        public Workbench(string fileName = "")
        {
            InitializeComponent();
            ActiveFile = fileName;
        }

        private void Workbench_Load(object sender, EventArgs e)
        {
            DoBackgroundWork(handler =>
            {
                Spreadsheet = !string.IsNullOrWhiteSpace(ActiveFile)
                    ? new Spreadsheet(ActiveFile, s => Regex.IsMatch(s, @"[A-Z][0-9]{1,2}"), _normalizer)
                    : new Spreadsheet(s => Regex.IsMatch(s, @"[A-Z][0-9]{1,2}"), _normalizer);

                foreach (var cell in Spreadsheet.GetNamesOfAllNonemptyCells())
                {
                    var value = Spreadsheet.GetCellValue(cell);
                    var point = GetPointFromCellName(cell);
                    
                    DoForegroundWork(() => spreadsheetPanel.SetValue(point.X, point.Y, value.ToString()));

                    // TODO: Check for formulas
                }
            });

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

            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                new Workbench(openFileDialog.FileName).Show();
            }
        }

        private void DoBackgroundWork(Action<DoWorkEventArgs> work)
        {
            var b = new BackgroundWorker();
            b.DoWork += (sender, e) => work(e);
            b.RunWorkerAsync();
        }

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
                    work();
                }
            }
            catch
            {
            }
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
    }
}
