using SS;

namespace SpreadsheetGUI
{
    /// <summary>
    /// A bunch of helper functions created to make the spreadsheet panel more C#-esque
    /// </summary>
    public static class SpreadsheetPanelHelpers
    {
        
        /// <summary>
        /// Change the selected cell with the specified content
        /// </summary>
        /// <param name="panel">The specified spreadhseet panel.</param>
        /// <param name="value">The value to set the cell to</param>
        /// <returns>True if the selected column and row are in range</returns>
        public static bool SetSelectedValue(this SpreadsheetPanel panel, string value)
        {
            var selected = panel.GetSelection();
            return panel.SetValue(selected.Column, selected.Row, value);
        }


        /// <summary>
        /// A more, C# friendly way of obtaining row and column locations.
        /// </summary>
        /// <param name="panel">The specified spreadhseet panel.</param>
        /// <param name="col">The column you want to get it from</param>
        /// <param name="row">The row you want to get it from</param>
        /// <returns>The value at that location</returns>
        public static string GetValue(this SpreadsheetPanel panel, int col, int row)
        {
            string contents;
            var result = panel.GetValue(col, row, out contents);
            return result ? contents : null;
        }

        /// <summary>
        /// A more, C# friendly way of obtaining row and column locations.
        /// </summary>
        /// <param name="panel">The specified spreadhseet panel.</param>
        /// <param name="coord">The location you want to get it from</param>
        /// <returns>The value at that location</returns>
        public static string GetValue(this SpreadsheetPanel panel, SpreadsheetCoord coord)
        {
            return panel.GetValue(coord.Column, coord.Row);
        }


        /// <summary>
        /// A more, C# friendly way of obtaining row and column locations.
        /// </summary>
        /// <param name="panel">The specified spreadsheet panel</param>
        /// <returns>a SpreadsheetCoord of a pair of integers (row, column) </returns>
        public static SpreadsheetCoord GetSelection(this SpreadsheetPanel panel)
        {
            int row;
            int col;

            panel.GetSelection(out col, out row);

            return new SpreadsheetCoord(row, col);
        }

        public static SpreadsheetCoord GetCoordFromCellName(string cell)
        {
            if (cell.Length < 2)
                return SpreadsheetCoord.Invalid;

            int col = cell[0] - 'A'; // Convert the first char to int index
            int row = int.Parse(cell.Substring(1)) - 1; // Get the rest of the chars //

            return new SpreadsheetCoord(row, col);
        }

        public static string GetCellNameFromCoord(SpreadsheetCoord coord)
        {
            var col = (char)('A' + coord.Column); // Convert column to letter //
            var row = coord.Row + 1; // Just get the row //

            return col + row.ToString();
        }
    }

    /// <summary>
    /// Turns the row/column pair into a readable struct.
    /// </summary>
    public struct SpreadsheetCoord
    {
        /// <summary>
        /// Invalid spreadsheet coord. Basically a null for this specific struct.
        /// </summary>
        public static readonly SpreadsheetCoord Invalid = new SpreadsheetCoord(-1, -1);

        /// <summary>
        /// Gets or sets the row value.
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Gets or sets the column value.
        /// </summary>
        public int Column { get; }
        
        /// <summary>
        /// Creates a new spreadsheet coord.
        /// </summary>
        /// <param name="row">The row of the spreadsheet.</param>
        /// <param name="column">The column of the spreadsheet.</param>
        public SpreadsheetCoord(int row, int column)
        {
            Row = row;
            Column = column;
        }

        /// <summary>
        /// The cell name/identifier of this coord
        /// </summary>
        public string CellName => SpreadsheetPanelHelpers.GetCellNameFromCoord(this);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return Column.GetHashCode() ^ Row.GetHashCode();
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
        /// </returns>
        /// <param name="obj">The object to compare with the current instance. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (!(obj is SpreadsheetCoord))
                return false;

        
            SpreadsheetCoord a = (SpreadsheetCoord)obj;
            return Column == a.Column && Row == a.Row;
        }

        public static bool operator ==(SpreadsheetCoord f1, SpreadsheetCoord f2)
        {
            // Can't be null, so we won't check for null //
            return f1.Equals(f2);
        }

        public static bool operator !=(SpreadsheetCoord a, SpreadsheetCoord b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return CellName + " (" + Column + "," + Row + ")";
        }
    }
}
