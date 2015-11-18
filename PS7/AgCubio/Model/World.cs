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
        public List<Cube> Food { get; set; }

        /// <summary>
        /// Gets or sets the players cubes.
        /// </summary>
        /// <value>
        /// The cubes.
        /// </value>
        public List<Cube> Players { get; set; }

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
            Players = new List<Cube>();
            Food = new List<Cube>();
        }
        
        /// <summary>
        /// Adds a cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void AddCube(Cube b)
        {
            (b.IsFood ? Food : Players).Add(b);
        }

        /// <summary>
        /// Adds a cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void UpdateFoodCube(Cube b)
        {
            var findCube = GetFoodCubeIndex(b.Uid);

            if (findCube == -1)
            {
                if (b.Mass != 0)
                    AddCube(b);

                return;
            }
            
            Food.RemoveAt(findCube); // A food cube is never updated, only removed //
        }

        /// <summary>
        /// Adds a cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void UpdatePlayerCube(Cube b)
        {
            var findCube = GetPlayerCubeIndex(b.Uid);

            if (findCube == -1)
            {
                if (b.Mass != 0)
                    AddCube(b);
                return;
            }

            if (b.Mass == 0)
                Players.RemoveAt(findCube);
            else
                Players[findCube] = b;
        }

        /// <summary>
        /// Finds a cube with that UID
        /// </summary>
        /// <param name="uid">the cube to find</param>
        /// <returns>A cube with that UID if exists; otherwise, returns null.</returns>
        public int GetFoodCubeIndex(int uid)
        {
            return Food.FindIndex(cube => cube.Uid == uid);
        }

        /// <summary>
        /// Finds a cube with that UID
        /// </summary>
        /// <param name="uid">the cube to find</param>
        /// <returns>A cube with that UID if exists; otherwise, returns null.</returns>
        public int GetPlayerCubeIndex(int uid)
        {
            return Players.FindIndex(cube => cube.Uid == uid);
        }

        /// <summary>
        /// Finds a cube with that UID
        /// </summary>
        /// <param name="uid">the cube to find</param>
        /// <returns>A cube with that UID if exists; otherwise, returns null.</returns>
        public Cube GetFoodCube(int uid)
        {
            var index = GetFoodCubeIndex(uid);
            return index == -1 ? null : Food[index];
        }

        /// <summary>
        /// Finds a cube with that UID
        /// </summary>
        /// <param name="uid">the cube to find</param>
        /// <returns>A cube with that UID if exists; otherwise, returns null.</returns>
        public Cube GetPlayerCube(int uid)
        {
            var index = GetPlayerCubeIndex(uid);
            return index == -1 ? null : Players[index];
        }
    }
}
