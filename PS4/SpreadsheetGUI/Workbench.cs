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
using SpreadsheetUtilities;
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
        private SpreadsheetCoord _previouSpreadsheetCoord = SpreadsheetCoord.Invalid;
        private readonly object _spreadsheetLock = new object();

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
            cellContentTextBox.Focus();

            var selected = sender.GetSelection();

            if (_previouSpreadsheetCoord != SpreadsheetCoord.Invalid && _previouSpreadsheetCoord != selected)
            {
                string txt = spreadsheetPanel.GetValue(_previouSpreadsheetCoord);
                
                if (!string.IsNullOrWhiteSpace(txt)) // Ignore if empty //
                    InvokeCellUpdate(_previouSpreadsheetCoord, txt);
            }

            var cellName = selected.CellName;

            selCellLabel.Text = $"Cell: {cellName}";

            var cellContent = Spreadsheet.GetCellContents(cellName);
            
            if (cellContent is Formula) 
                cellContentTextBox.Text = $"={cellContent}";

            if (cellContent is string || cellContent is double)
                cellContentTextBox.Text = cellContent.ToString();

            _previouSpreadsheetCoord = selected;
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
                    var point = SpreadsheetPanelHelpers.GetCoordFromCellName(cell);

                    DoForegroundWork(() => spreadsheetPanel.SetValue(point.Column, point.Row, value.ToString()));

                    // TODO: Check for formulas
                }

                // Set selected cell to A1
                spreadsheetPanel.SetSelection(0, 0);
                DoForegroundWork(() =>
                {
                    selCellLabel.Text = @"Cell: A1";
                    string cellContents = spreadsheetPanel.GetValue(0, 0);
                    cellContentTextBox.Text = cellContents;
                });

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
                    BeginInvoke(work);
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
                Text = IsUntitled ? 
                    @"Spreadsheet - untitled" : 
                    $"Spreadsheet - {Path.GetFileName(FileName)}";

                if (Changed) Text += @"*";
            });
        }

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

        private void updateButton_Click(object sender, EventArgs e)
        {
            //TODO formulas

            try
            {
                var selectedCoord = spreadsheetPanel.GetSelection();
                InvokeCellUpdate(selectedCoord, cellContentTextBox.Text);
            }
            catch { }
        }

        private void InvokeCellUpdate(SpreadsheetCoord coord, string text)
        {
            DoBackgroundWork(arg =>
            {
                var cell = coord.CellName;

                // Spreadsheet isn't threadsafe by design //
                lock (_spreadsheetLock)
                {
                    Spreadsheet.SetContentsOfCell(cell, text);
                    var value = Spreadsheet.GetCellValue(cell);
                    
                    DoForegroundWork(() => spreadsheetPanel.SetValue(coord.Column, coord.Row, value.ToString()));
                }
            });
        }

        private void cellContentTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                updateButton_Click(sender, e);

                // Move the selected cell down a row //
                var oldCoord = spreadsheetPanel.GetSelection();

                var row = (oldCoord.Row + 1) % 99;
                var col = oldCoord.Column;

                if (row == 0) col++; // If the row got over 99, go to the next column //

                spreadsheetPanel.SetSelection(col, row);
                spreadsheetPanel_SelectionChanged(spreadsheetPanel); // Not sure why this isn't called //

                e.Handled = true; // Stop the ding >:( //
                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.Escape)
            {
                cancelButton_Click(sender, e);
                e.Handled = true; // Stop the ding >:( //
                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.Tab)
            {
                updateButton_Click(sender, e);

                // Move the selected cell one to the right //
                var oldCoord = spreadsheetPanel.GetSelection();

                var row = oldCoord.Row;
                var col = (oldCoord.Column + 1) % 26;

                if (col == 0) row++; // If the col got over 26, go to the next row //

                spreadsheetPanel.SetSelection(col, row);
                spreadsheetPanel_SelectionChanged(spreadsheetPanel); // Not sure why this isn't called //

                e.Handled = true; // Stop the ding >:( //
                e.SuppressKeyPress = true;
            }

        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            cellContentTextBox.Clear();
        }

        private void cellContentTextBox_TextChanged(object sender, EventArgs e)
        {
            spreadsheetPanel.SetSelectedValue(cellContentTextBox.Text);
        }
    }
}
