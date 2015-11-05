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
using RemoteLib.Net;
using SpreadsheetGUI.Networking.Packets;
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

            RemoteClient.ClientLeft += RemoteClient_ClientLeft;

            Packet.PacketRecieved += Packet_PacketRecieved;
        }

        

        private void Packet_PacketRecieved(object sender, Packet.PacketEventArgs e)
        {
            if (e.Packet is PacketCellUpdate)
            {
                var cellUpdate = (PacketCellUpdate)e.Packet;
                InvokeCellUpdate(SpreadsheetPanelHelpers.GetCoordFromCellName(cellUpdate.Cell), cellUpdate.Content);
            }

            if (e.Packet is PacketSelectionChanged)
            {
                var selectionChanged = (PacketSelectionChanged)e.Packet;
                DoForegroundWork(() =>
                {
                    var coords = SpreadsheetPanelHelpers.GetCoordFromCellName(selectionChanged.Cell);
                    spreadsheetPanel.SetOtherSelection(coords.Column, coords.Row);
                });
            }

            if (e.Packet is PacketSpreadsheetReady)
            {
                DoForegroundWork(() =>
                {
                    foreach (var cell in Spreadsheet.GetNamesOfAllNonemptyCells())
                    {
                        var coord = SpreadsheetPanelHelpers.GetCoordFromCellName(cell);
                        var cellContent = Spreadsheet.GetCellContents(cell);

                        if (cellContent is Formula)
                            cellContent = $"={cellContent}";

                        if (cellContent is string || cellContent is double)
                            cellContent = cellContent.ToString();

                        SendCellUpdate(coord, cellContent.ToString());
                    }
                });
            }
        }

        private void RemoteClient_ClientLeft(object sender, ClientConnectionEventArgs e)
        {
            DoForegroundWork(() =>
            {
                spreadsheetPanel.SetOtherSelection(-1, -1); // Hide it from the spreadsheet //
            });
        }
        
        private void spreadsheetPanel_SelectionChanged(SS.SpreadsheetPanel sender)
        {
            var selected = sender.GetSelection();

            if (_previouSpreadsheetCoord != SpreadsheetCoord.Invalid && _previouSpreadsheetCoord != selected)
            {
                string txt = spreadsheetPanel.GetValue(_previouSpreadsheetCoord);
                InvokeCellUpdate(_previouSpreadsheetCoord, txt);
                SendCellUpdate(_previouSpreadsheetCoord, txt);
            }

            var cellName = selected.CellName;
            var cellContent = Spreadsheet.GetCellContents(cellName);
            var cellValue = Spreadsheet.GetCellValue(cellName).ToString().Truncate(15);

            selCellLabel.Text = $"Cell: {cellName}";
            cellContentLabel.Text = $"Cell Value: {cellValue}";

            if (cellContent is Formula)
                cellContentTextBox.Text = $"={cellContent}";

            if (cellContent is string || cellContent is double)
                cellContentTextBox.Text = cellContent.ToString();

            _previouSpreadsheetCoord = selected;

            cellContentTextBox.Focus(); // Keep focus here //

            DoBackgroundWork(args =>
            {
                // Do networking stuff //
                var packet = new PacketSelectionChanged(cellName);

                Program.Client?.PacketWriter.EnqueuePacket(packet); // Will send to server if client //
                Program.Server?.Clients.ForEach(client => client.PacketWriter.EnqueuePacket(packet)); // Will send to all clients (should just be 1) if there's clients //
            });
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: Ask user if they want to save if Changes == true

            FileName = string.Empty;
            LoadFile();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            var result = openFileDialog.ShowDialog(this);

            if (result != DialogResult.OK && result != DialogResult.Yes) return;

            //TODO: Ask user if they want to save if changed == true.
            SaveIfChanged(sender, e);

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

                spreadsheetPanel.Clear(); // Clear all cells. //

                foreach (var cell in Spreadsheet.GetNamesOfAllNonemptyCells())
                {
                    var value = Spreadsheet.GetCellValue(cell);
                    var point = SpreadsheetPanelHelpers.GetCoordFromCellName(cell);

                    DoForegroundWork(() => spreadsheetPanel.SetValue(point.Column, point.Row, value.ToString()));
                }

                // Set selected cell to A1
                spreadsheetPanel.SetSelection(0, 0);
                DoForegroundWork(() =>
                {
                    selCellLabel.Text = @"Cell: A1";
                    string cellContents = spreadsheetPanel.GetValue(0, 0);
                    cellContentTextBox.Text = cellContents;

                    Invalidate();
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

            //TODO: Handle SpreadsheetReadWriteExceptions
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

            //TODO: Handle SpreadsheetReadWriteExceptions
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
                SendCellUpdate(selectedCoord, cellContentTextBox.Text);
            }
            catch { }
        }

        private void SendCellUpdate(SpreadsheetCoord selectedCoord, string text)
        {
            // Do networking stuff //
            var packet = new PacketCellUpdate(selectedCoord.CellName, text);

            Program.Client?.PacketWriter.EnqueuePacket(packet); // Will send to server if client //
            Program.Server?.Clients.ForEach(client => client.PacketWriter.EnqueuePacket(packet)); // Will send to all clients (should just be 1) if there's clients //
        }

        private void InvokeCellUpdate(SpreadsheetCoord coord, string text)
        {
            DoBackgroundWork(arg =>
            {
                var cell = coord.CellName;

                // Spreadsheet isn't threadsafe by design //
                lock (_spreadsheetLock)
                {
                    try
                    {
                        Spreadsheet.SetContentsOfCell(cell, text);
                        var value = Spreadsheet.GetCellValue(cell);

                        // Check for invalid formulas
                        if (value is FormulaError)
                        {
                            Spreadsheet.SetContentsOfCell(cell, "Invalid formula.");
                            DoForegroundWork(() => 
                            {
                                spreadsheetPanel.SetSelection(coord.Column, coord.Row);
                                selCellLabel.Text = "Cell: " + coord.CellName;
                                cellContentTextBox.Text = "Invalid formula!";
                                spreadsheetPanel.SetValue(coord.Column, coord.Row, "Invalid formula!");
                                MessageBox.Show("Invalid formula!");
                            });
                        }

                        DoForegroundWork(() => spreadsheetPanel.SetValue(coord.Column, coord.Row, value.ToString()));
                    }
                    catch (CircularException)
                    {
                        DoForegroundWork(() =>
                        {
                            spreadsheetPanel.SetSelection(coord.Column, coord.Row);
                            selCellLabel.Text = "Cell: " + coord.CellName;
                            Spreadsheet.SetContentsOfCell(cell, "=0"); //no clear way of removing this cell
                            spreadsheetPanel.SetValue(coord.Column, coord.Row, "=0");
                            MessageBox.Show("You cannot have circular dependencies!");
                        });
                    }
                    catch (FormulaFormatException)
                    {
                        DoForegroundWork(() =>
                        {
                            spreadsheetPanel.SetSelection(coord.Column, coord.Row);
                            selCellLabel.Text = "Cell: " + coord.CellName;
                            cellContentTextBox.Text = "Invalid formula!";
                            spreadsheetPanel.SetValue(coord.Column, coord.Row, "Invalid formula!");
                            MessageBox.Show("Invalid formula!"); // both messageboxes keep popping up twice
                        });
                    }
                }
                
            });
        }

        private void cellContentTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var arrows = new[] { Keys.Left, Keys.Right, Keys.Up, Keys.Down };
            if (arrows.Contains(e.KeyCode))
            {
                // We can use arrow keys if (and only if) the textbox is empty.
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
                {
                    if (string.IsNullOrWhiteSpace(cellContentTextBox.Text))
                    {
                        HandleKeyPress(e.KeyCode);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                }
                else
                {
                    HandleKeyPress(e.KeyCode);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }


            }

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

                var direction = e.Shift ? -1 : 1; // Shift tab means go backwards, just tab means go forward //

                var row = oldCoord.Row;
                var col = (oldCoord.Column + direction) % 26;

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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // The textbox shouldn't be messed with. It's got its own handler //
            if (!cellContentTextBox.Focused)
            {
                var arrows = new[] { Keys.Left, Keys.Right, Keys.Up, Keys.Down };
                if (arrows.Contains(keyData))
                {
                    HandleKeyPress(keyData);
                    return true; // Return true because we handled the event //
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void HandleKeyPress(Keys keyData)
        {
            var oldCoord = spreadsheetPanel.GetSelection();

            var col = oldCoord.Column;
            var row = oldCoord.Row;

            //Do modifiers //
            if (keyData == Keys.Left)
                col--;
            if (keyData == Keys.Right)
                col++;

            if (keyData == Keys.Up)
                row--;
            if (keyData == Keys.Down)
                row++;

            spreadsheetPanel.SetSelection(col, row);
            spreadsheetPanel_SelectionChanged(spreadsheetPanel); // Not sure why this isn't called //
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //(new About()).ShowDialog();
            MessageBox.Show(
                "This spreadsheet application was created by Brayden Lopez and Eric Miramontes " +
                "in November, 2015"
                , "About", MessageBoxButtons.OK);
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //(new Help()).ShowDialog();
            MessageBox.Show(
                "This spreadsheet application lets users enter information into a grid of cells.  \n" +
                "You can use the mouse, arrow keys, or the tab key to navigate between cells.  \n" +
                "To enter in words or numbers into cells, type it into the textbox at the top of " +
                "the grid.  \n" + 
                "To enter a formula, type \"=\" and then your formula, and then either " +
                "hit enter or click the checkmarked box. To cancel, click the button with the \"x\".  \n" +
                "You may use cells that have numeric values in your formulas.  \n" + 
                "In the file menu, you can create a new spreadsheet, open an existing one, or save " + 
                "your current spreadsheet to your file system.  "
                , "Help", MessageBoxButtons.OK);
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var wasNull = Program.Client == null;

            (new Connect()).ShowDialog();

            if (Program.Client != null && wasNull)
            {
                // This means we have connected to a host, we need to clear our board //
                FileName = string.Empty;
                LoadFile();


                // Do networking stuff //
                var packet = new PacketSpreadsheetReady();

                Program.Client?.PacketWriter.EnqueuePacket(packet); // Will send to server if client //
                Program.Server?.Clients.ForEach(client => client.PacketWriter.EnqueuePacket(packet)); // Will send to all clients (should just be 1) if there's clients //
            }


        }

        private void Workbench_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveIfChanged(sender, e);

            Program.Client?.Disconnect();
            Program.StopNetworkTransactions();
        }

        private void SaveIfChanged(object sender, EventArgs e)
        {
            if (Changed)
            {
                DialogResult result = MessageBox.Show("Do you want to save your spreadsheet?",
                    "You have made changes to " + FileName, MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(sender, e);
                }
            }
        }

    }
}
