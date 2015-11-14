using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Model
{
    /// <summary>
    /// Cube instance.
    /// </summary>
    public class Cube
    {
        public static float Stuff = 1;

        /// <summary>
        /// Gets or sets the parent uid.
        /// </summary>
        /// <value>
        /// The parent uid.
        /// </value>
        [JsonProperty("team_id")]
        public int TeamId { get; set; }

        /// <summary>
        /// Gets or sets the uid.
        /// </summary>
        /// <value>
        /// The uid.
        /// </value>
        [JsonProperty("uid")]
        public int Uid { get; set; }

        /// <summary>
        /// Gets or sets the x coord.
        /// </summary>
        /// <value>
        /// The x coord.
        /// </value>
        [JsonProperty("loc_x")]
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the y coord.
        /// </summary>
        /// <value>
        /// The y coord.
        /// </value>
        [JsonProperty("loc_y")]
        public float Y { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public Color Color
        {
            get { return Color.FromArgb(_color); }
            set { _color = value.ToArgb(); }
        }

        /// <summary>
        /// Integer value of a color
        /// </summary>
        [JsonProperty("argb_color")]
        private int _color;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the mass.
        /// </summary>
        /// <value>
        /// The mass.
        /// </value>
        [JsonProperty("Mass")]
        public double Mass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this cube is food.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this cube is food; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("food")]
        public bool IsFood { get; set; }


        /// <summary>
        /// Gets the width of the cube.
        /// </summary>
        public float Width => (int) Math.Pow(Mass, 0.65);

        /// <summary>
        /// Gets the height of the cube.
        /// </summary>
        public float Height => Width;


        /// <summary>
        /// Gets the top of the cube.
        /// </summary>
        public float Top => Y - (Height / 2f);

        /// <summary>
        /// Gets the left of the cube. Based from the center
        /// </summary>
        public float Left => X - (Width / 2f);

        /// <summary>
        /// Gets the right of the cube. Based from the center
        /// </summary>
        public float Right => X + (Width / 2f);

        /// <summary>
        /// Gets the bottom of the cube. Based from the center
        /// </summary>
        public float Bottom => Y + (Height / 2f);

        /// <summary>
        /// Gets a cube from json
        /// </summary>
        /// <param name="json">the json</param>
        /// <returns>The cube</returns>
        public static Cube FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Cube>(json);
        }
    }
}