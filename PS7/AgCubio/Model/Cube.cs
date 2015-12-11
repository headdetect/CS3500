using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Model
{
    /// <summary>
    /// Cube instance.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Cube : ICloneable
    {
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
        /// Gets or sets the target x for smooth interpolation.
        /// </summary>
        /// <value>
        /// The target x.
        /// </value>
        public float TargetX { get; set; }

        /// <summary>
        ///Gets or sets the target y for smooth interpolation.
        /// </summary>
        /// <value>
        /// The target y.
        /// </value>
        public float TargetY { get; set; }

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
        
        public RectangleF AsRectangle => new RectangleF(Left, Top, Width, Height);

        /// <summary>
        /// Gets or sets a value indicating whether this instance is dead.
        /// </summary>
        /// <value>
        /// If is dead <c>true</c> if this instance is dead; otherwise, <c>false</c>.
        /// </value>
        public bool IsDead => Math.Abs(Mass) <= 0;

        /// <summary>
        /// A list of the cubes a player has eaten.
        /// </summary>
        public List<string> CubesEaten = new List<string>();
        
        /// <summary>
        /// Gets or sets the highest mass achieved by a player.
        /// </summary>
        /// <value>
        /// Highest mass achieved by the player.
        /// </value>
        public double HighestMassAchieved { get; set; }

        /// <summary>
        /// Gets or sets the highest rank achieved by a player.
        /// </summary>
        /// <value>
        /// Highest rank achieved by a player.
        /// </value>
        public int HighestRankAchieved { get; set; }

        /// <summary>
        /// Gets or sets the time the player joined the game.
        /// </summary>
        /// <value>
        /// Time player joined the game.
        /// </value>
        public DateTime TimeJoined { get; set; }

        /// <summary>
        /// Gets or sets the amount of time the player was alive.
        /// </summary>
        /// <value>
        /// Amount of time the player was alive.
        /// </value>
        public TimeSpan TimeAlive { get; set; }

        /// <summary>
        /// Gets or sets the number of cubes the player has eaten.
        /// </summary>
        /// <value>
        /// Number of cubes eaten by player.
        /// </value>
        public int NumberOfCubesEaten { get; set; }

        /// <summary>
        /// Gets or sets the time of player's death.
        /// </summary>
        /// <value>
        /// Time of player's death.
        /// </value>
        public DateTime TimeOfDeath { get; set; }

        /// <summary>
        /// Gets or sets the name of the cube that ate this one.
        /// </summary>
        public string EatenBy { get; set; }

        /// <summary>
        /// Gets a cube from json
        /// </summary>
        /// <param name="json">the json</param>
        /// <returns>The cube</returns>
        public static Cube FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Cube>(json);
        }

        /// <summary>
        /// Converts and object into string json
        /// </summary>
        /// <returns>a string json representation of this object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Determines whether the specified cube is on same team.
        /// </summary>
        /// <param name="b">The cube.</param>
        /// <returns></returns>
        public bool IsOnTeam(Cube b)
        {
            return TeamId != 0 && b.TeamId == TeamId;
        }
    }
}