using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Spreadsheet;
using SpreadsheetUtilities;

namespace SS
{
    /// <summary>
    /// The spreadsheet class. Contains a row and column of cells that
    /// can take in both functions and plain text.
    /// These cells can relate to eachother by referencing them via 
    /// a dependency map.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        /// <summary>
        /// All the cells for this spreadsheet.
        /// </summary>
        private readonly Dictionary<string, Cell> _cells;

        /// <summary>
        /// The resolver is temporary, just to be accessed outside the class.
        /// Made it this way because we can't make any other public methods >.>
        /// I gettin real sneaky.
        /// </summary>
        private Func<string, double> _resolver;

        private double _resolveVariables(string variable)
        {
            if (!_cells.ContainsKey(variable)) return double.NaN;

            var cell = _cells[variable].Value;
            var formula = cell as Formula;
            if (formula != null)
                return (double) formula.Evaluate(_resolveVariables);

            var value = cell as double?;
            return value ?? double.NaN;
        }

        /// <summary>
        /// Creates an empty spreadsheet. A1 - Z50
        /// </summary>
        public Spreadsheet() : this(new Dictionary<string, Cell>())
        {
            // Iterate A - Z //
            for (var x = 'A'; x <= 'Z'; x++)
            {
                // Iterate 1 - 50 //
                for (var y = 1; y <= 50; y++)
                {
                    var cellName = x + y.ToString();

                    _cells.Add(cellName, new Cell(cellName));
                }
            }
            
        }

        /// <summary>
        /// Creates a spreadsheet from the specified cells.
        /// </summary>
        /// <param name="cells">Cells to put into the spreadsheet</param>
        public Spreadsheet(Dictionary<string, Cell> cells)
        {
            if (cells == null) throw new ArgumentNullException(nameof(cells), "Cells cannot be null");
            _cells = cells;

            _resolver = _resolveVariables;
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            if (!_cells.ContainsKey(name))
                throw new InvalidNameException();

            return _cells[name].Content;
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return _cells
                .Where(pair => !pair.Value.Content.IsEmpty())
                .Select(pair => pair.Key);
        }

        /// <summary>
        /// If the formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.  (No change is made to the spreadsheet.)
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetCellContents(string name, Formula formula)
        {
            if (!_cells.ContainsKey(name))
                throw new InvalidNameException();

            if (formula == null)
                throw new ArgumentNullException(nameof(formula), "Formula is null");
            
            _cells[name].Content = formula;
            _cells[name].Value = formula.Evaluate(_resolveVariables);

            return new HashSet<string>(GetCellsToRecalculate(name));
        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetCellContents(string name, string text)
        {
            if (!_cells.ContainsKey(name)) return new HashSet<string>();

            _cells[name].Content = text;
            _cells[name].Value = text;

            return new HashSet<string>(GetCellsToRecalculate(name));
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetCellContents(string name, double number)
        {
            if (!_cells.ContainsKey(name)) return new HashSet<string>();

            _cells[name].Content = number;
            _cells[name].Value = number;

            return new HashSet<string>(GetCellsToRecalculate(name));
        }

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (!_cells.ContainsKey(name)) throw new InvalidNameException();
            
            var cell = _cells[name];

            return cell.Dependents.GetDependents(cell.Name);
        }
    }
}
