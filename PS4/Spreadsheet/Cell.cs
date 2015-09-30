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

        /// <summary>
        /// Gets or sets the content of the cell.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets the evaluated value based off of <see cref="Content"/>
        /// </summary>
        public string Value { get; internal set; }

        /// <summary>
        /// Creates an empty cell
        /// </summary>
        public Cell() : this(string.Empty, string.Empty)
        {
            
        }

        /// <summary>
        /// Creates a cell object using the specified contents and values.
        /// </summary>
        /// <param name="content">The content of the cell</param>
        /// <param name="value">The value of the cell</param>
        public Cell(string content, string value)
        {
            Content = content;
            Value = value;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Content;
        }
    }
}
