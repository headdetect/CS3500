using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using SpreadsheetUtilities;

namespace SS
{
    /**
        Specified Invariant:
        
        The "Value" is to be computed using your own methods.
        The result should be pushed into the Value property. 
        There are other ways of going about
        this, such as, just evaluating the content in the getter
        of Value. That method would require additional computation
        when it is not needed.

    **/

    /// <summary>
    /// This will represent a spreadsheet 'cell'. 
    /// It contains a value and content. 
    /// The content is the physical input given by a user,
    /// whereas the value is the content evalueated.
    /// </summary>
    public class Cell
    {
        private static readonly HashSet<Cell> EmptySet = new HashSet<Cell>(); 

        /// <summary>
        /// Gets or sets the content of the cell.
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// Gets the evaluated value based off of <see cref="Content"/>
        /// </summary>
        public object Value { get; internal set; }

        /// <summary>
        /// Gets and sets the name of this cell.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets and sets the dependents of this cell.
        /// Say that A1 contains the forumla:
        /// B1 + 2 - C3
        /// This should return {B1, C3}
        /// </summary>
        public DependencyGraph Dependents { get; set; }

        /// <summary>
        /// Creates an empty cell
        /// </summary>
        /// <param name="name">The name of this cell</param>
        public Cell(string name) : this(name, EmptySet, string.Empty, string.Empty)
        {
            
        }

        /// <summary>
        /// Creates a cell object using the specified contents and values.
        /// </summary>
        /// <param name="name">The name of this cell</param>
        /// <param name="content">The content of the cell</param>
        /// <param name="value">The value of the cell</param>
        public Cell(string name, object content, object value) : this(name, EmptySet, content, value)
        {
            
        }

        /// <summary>
        /// Creates a cell object using the specified dependents, contents, and values.
        /// </summary>
        /// <param name="name">The name of this cell</param>
        /// <param name="dependents">The cells this cell depends on</param>
        /// <param name="content">The content of the cell</param>
        /// <param name="value">The value of the cell</param>
        public Cell(string name, IEnumerable<Cell> dependents, object content, object value)
        {
            Name = name;
            Dependents = new DependencyGraph();
            foreach(var depend in dependents)
                Dependents.AddDependency(name, depend.Name); // This cell depends on all those other cells //
            Content = content;
            Value = value;
        }
        
    }
}
