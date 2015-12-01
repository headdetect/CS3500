using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class World
    {
        /// <summary>
        /// Gets or sets the food cubes.
        /// </summary>
        /// <value>
        /// The cubes.
        /// </value>
        public Dictionary<int, Cube> Food { get; set; }

        /// <summary>
        /// Gets or sets the players cubes.
        /// </summary>
        /// <value>
        /// The cubes.
        /// </value>
        public Dictionary<int, Cube> Players { get; set; }

        /// <summary>
        /// The width of this world
        /// </summary>
        public const int Width = 1000;

        /// <summary>
        /// The height world
        /// </summary>
        public const int Height = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="World"/> class.
        /// </summary>
        public World()
        {
            Players = new Dictionary<int, Cube>();
            Food = new Dictionary<int, Cube>();
        }
        
        /// <summary>
        /// Adds a cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void AddCube(Cube b)
        {
            (b.IsFood ? Food : Players).Add(b.Uid, b);
        }

        /// <summary>
        /// Adds a cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void UpdateFoodCube(Cube b)
        {
            if (!Food.ContainsKey(b.Uid))
            {
                if (b.Mass != 0)
                    AddCube(b);

                return;
            }
            
            Food.Remove(b.Uid); // A food cube is never updated, only removed //
        }

        /// <summary>
        /// Adds a cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void UpdatePlayerCube(Cube b)
        {
            if (!Players.ContainsKey(b.Uid))
            {
                if (b.Mass != 0)
                    AddCube(b);
                return;
            }

            if (b.Mass == 0)
                Players.Remove(b.Uid);
            else
                Players[b.Uid] = b;
        }

        /// <summary>
        /// Finds a cube with that UID
        /// </summary>
        /// <param name="uid">the cube to find</param>
        /// <returns>A cube with that UID if exists; otherwise, returns null.</returns>
        public Cube GetFoodCube(int uid)
        {
            return !Food.ContainsKey(uid) ? null : Food[uid];
        }

        /// <summary>
        /// Finds a cube with that UID
        /// </summary>
        /// <param name="uid">the cube to find</param>
        /// <returns>A cube with that UID if exists; otherwise, returns null.</returns>
        public Cube GetPlayerCube(int uid)
        {
            return !Players.ContainsKey(uid) ? null : Players[uid];
        }
    }
}
