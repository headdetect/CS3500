using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Xml;
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
        /// The version of this spreadsheet implementation.
        /// </summary>
        public const string CurrentVersion = "v0.0.1 (Duck Face)";

        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed { get; protected set; }
        
        /// <summary>
        /// All the cells for this spreadsheet.
        /// </summary>
        private readonly Dictionary<string, Cell> _cells;

        /// <summary>
        /// Gets and sets the dependents of this cell.
        /// Say that A1 contains the forumla:
        /// B1 + 2 - C3
        /// This should return {B1, C3}
        /// </summary>
        private readonly DependencyGraph _depenencyManager;

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
        /// Creates a new empty spreadsheet with a proper validation and normalization funciton.
        /// </summary>
        public Spreadsheet() : this(IsValidName, s => s.ToUpperInvariant())
        {
            
        }

        /// <summary>
        /// Creates a spreadsheet that contains cells where you can put in a 
        /// string, number, or a forumla.
        /// </summary>
        /// <param name="isValid">The function used to determine if a variable (usually cell name) is valid</param>
        /// <param name="normalize">The function used to normalize variable names</param>
        /// <param name="version">The version of this spreadsheet implementation</param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version = CurrentVersion) : base(isValid, normalize, version)
        {
            _cells = new Dictionary<string, Cell>();
            _depenencyManager = new DependencyGraph();
        }

        /// <summary>
        /// Creates a spreadsheet from a specified file.
        /// </summary>
        /// <param name="filename">The file </param>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="version"></param>
        public Spreadsheet(string filename, Func<string, bool> isValid, Func<string, string> normalize, string version = CurrentVersion) : this(isValid, normalize, version)
        {
            ReadSpreadsheet(version, filename);
        }

        /// <summary>
        /// Reads the specified spreadsheet file and fills the spreadsheet object out.
        /// Created to prevent inheritence issues which allows us to circumvent the "sealed" issue.
        /// So we don't have to have the "GetSavedVersion", "SetContentsOfCell" or the class be marked as sealed.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="filename"></param>
        private void ReadSpreadsheet(string version, string filename)
        {
            var savedVersion = GetSavedVersion(filename);

            if (version != savedVersion) throw new SpreadsheetReadWriteException("Version mismatch");

            try
            {
                using (var reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType != XmlNodeType.Element) continue;

                        if (reader.Name != "cell") continue;
                    
                        var name = string.Empty;
                        var content = string.Empty;

                        reader.Read();

                        if (reader.Name == "name")
                        {
                            name = reader.ReadInnerXml();
                            content = reader.ReadInnerXml();
                        }

                        if (reader.Name == "contents")
                        {
                            content = reader.ReadInnerXml();
                            name = reader.ReadInnerXml();
                        }

                        SetContentsOfCell(name, content);
                    }
                }
            }
            catch (Exception e)
            {
                throw new SpreadsheetReadWriteException(e.Message);
            }
        }



        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            if (string.IsNullOrWhiteSpace(name) || !IsValidName(name))
                throw new InvalidNameException();

            if (content == null)
                throw new ArgumentNullException(nameof(content), "content is null");

            name = Normalize(name);

            Changed = true;

            var getDouble = content.TryGetDouble();
            if (getDouble != null)
            {
                // Is a double //
                return SetCellContents(name, getDouble.Value);
            }

            if (content.StartsWith("=", StringComparison.CurrentCultureIgnoreCase))
            {
                // Is formula, parse as such //
                return SetCellContents(name, new Formula(content.Substring(1), Normalize, IsValid)); // Skip first character //
            }

            // Is just a plain string //
            return SetCellContents(name, content);
        }

        

        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override string GetSavedVersion(string filename)
        {
            try
            {
                using (var reader = XmlReader.Create(filename))
                {
                    reader.ReadToFollowing("spreadsheet");
                    reader.MoveToAttribute("version");

                    return reader.Value;
                }
            }
            catch (Exception ioe)
            {
                throw new SpreadsheetReadWriteException(ioe.Message);
            } 
            // Just rethrow every other type of exception //
            
        }

        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>
        /// cell name goes here
        /// </name>
        /// <contents>
        /// cell contents goes here
        /// </contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(string filename)
        {
            try
            {
                var settings = new XmlWriterSettings
                {
                    //Indent = true
                };

                using (var writer = XmlWriter.Create(filename, settings))
                {

                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("version", Version); 

                    writer.WriteStartElement("cells");

                    foreach (var cell in _cells.Select(cellPair => cellPair.Value))
                    {
                        var formula = cell.Content as Formula;

                        writer.WriteStartElement("cell");

                        writer.WriteElementString("name", cell.Name);
                        writer.WriteElementString("contents", formula != null ? "=" + formula.Expression : cell.Content.ToString());

                        writer.WriteEndElement();
                    }
                    
                    writer.WriteEndDocument();
                }
                Changed = false;
            }
            catch (Exception ioe)
            {
                throw new SpreadsheetReadWriteException(ioe.Message);
            }
            // Rethrow every other exception //
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || !IsValidName(name))
                throw new InvalidNameException();

            name = Normalize(name);

            return !_cells.ContainsKey(name) ? string.Empty : _cells[name].Value;
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || !_cells.ContainsKey(name))
                throw new InvalidNameException();

            name = Normalize(name);

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
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {
            if (string.IsNullOrWhiteSpace(name) || !IsValidName(name))
                throw new InvalidNameException();

            if (formula == null)
                throw new ArgumentNullException(nameof(formula), "Formula is null");

            if (!_cells.ContainsKey(name))
                _cells.Add(name, new Cell(name));

            var referencedCellsEnumeration = formula.GetVariables();
            var referencedCells = referencedCellsEnumeration as string[] ?? referencedCellsEnumeration.ToArray();

            var cell = _cells[name];

            cell.Content = formula;
            cell.Value = formula.Evaluate(_resolveVariables);
            _depenencyManager.ReplaceDependents(name, referencedCells);

            foreach (var dep in referencedCells.Where(dep => _cells.ContainsKey(dep)))
            {
                _depenencyManager.AddDependency(name, dep);
            }

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
        protected override ISet<string> SetCellContents(string name, string text)
        {
            if (string.IsNullOrWhiteSpace(name) || !IsValidName(name))
                throw new InvalidNameException();

            if (text == null)
                throw new ArgumentNullException(nameof(text), "text is null");

            if (!_cells.ContainsKey(name))
                _cells.Add(name, new Cell(name));

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
        protected override ISet<string> SetCellContents(string name, double number)
        {
            if (string.IsNullOrWhiteSpace(name) || !IsValidName(name))
                throw new InvalidNameException();

            if (!_cells.ContainsKey(name))
                _cells.Add(name, new Cell(name));

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
            if (!IsValidName(name)) throw new InvalidNameException();

            if (!_cells.ContainsKey(name))
                _cells.Add(name, new Cell(name));
            
            var cell = _cells[name];

            return _depenencyManager.Dependees.ContainsKey(cell.Name) ?
                _depenencyManager.Dependees[cell.Name] :
                new List<string>(0);
        }

        /// <summary>
        /// Checks to see if the specified string is a valid variable name
        /// </summary>
        /// <param name="name">The string to check</param>
        /// <returns>True if valid; false otherwise</returns>
        public static bool IsValidName(string name)
        {
            return Regex.IsMatch(name, @"^[a-zA-Z]{1,}\d{1,}");
        }
    }
}
