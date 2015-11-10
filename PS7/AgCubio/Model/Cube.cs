using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// Cube instance.
    /// </summary>
    public class Cube
    {
        /// <summary>
        /// Gets or sets the uid.
        /// </summary>
        /// <value>
        /// The uid.
        /// </value>
        public int Uid { get; set; }

        /// <summary>
        /// Gets or sets the coord.
        /// </summary>
        /// <value>
        /// The coord.
        /// </value>
        public Point Coord { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the mass.
        /// </summary>
        /// <value>
        /// The mass.
        /// </value>
        public double Mass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this cube is food.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this cube is food; otherwise, <c>false</c>.
        /// </value>
        public bool IsFood { get; set; }


        /// <summary>
        /// Gets the width of the cube.
        /// </summary>
        public int Width => Math.Max(1, (int)(Mass / 9));

        /// <summary>
        /// Gets the height of the cube.
        /// </summary>
        public int Height => Width;


        /// <summary>
        /// Gets the top of the cube.
        /// </summary>
        public int Top => Coord.Y;

        /// <summary>
        /// Gets the left of the cube.
        /// </summary>
        public int Left => Coord.X;

        /// <summary>
        /// Gets the right of the cube.
        /// </summary>
        public int Right => Coord.X + Width;

        /// <summary>
        /// Gets the bottom of the cube.
        /// </summary>
        public int Bottom => Coord.Y + Width;

    }
}