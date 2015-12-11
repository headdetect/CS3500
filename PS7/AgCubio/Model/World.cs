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
        internal Dictionary<int, Cube> Food { get; set; }

        /// <summary>
        /// Gets or sets the players cubes.
        /// </summary>
        /// <value>
        /// The cubes.
        /// </value>
        internal Dictionary<int, Cube> Players { get; set; }

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
        /// Adds a player cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void AddPlayerCube(Cube b)
        {
            lock (this)
            {
                Players.Add(b.Uid, b);
            }
        }

        /// <summary>
        /// Adds a player cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void AddFoodCube(Cube b)
        {
            lock (this)
            {
                Food.Add(b.Uid, b);
            }
        }

        /// <summary>
        /// Adds a player cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void RemovePlayerCube(Cube b)
        {
            lock (this)
            {
                Players.Remove(b.Uid);
            }
        }

        /// <summary>
        /// Adds a player cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void RemoveFoodCube(Cube b)
        {
            lock (this)
            {
                Food.Remove(b.Uid);
            }
        }


        /// <summary>
        /// Adds a player cube to the world.
        /// </summary>
        /// <param name="uid">The cube's uid.</param>
        public void RemovePlayerCube(int uid)
        {
            lock (this)
            {
                Players.Remove(uid);
            }
        }

        /// <summary>
        /// Adds a player cube to the world.
        /// </summary>
        /// <param name="uid">The cube's uid.</param>
        public void RemoveFoodCube(int uid)
        {
            lock (this)
            {
                Food.Remove(uid);
            }
        }


        /// <summary>
        /// If the player the cube exists based on the uid.
        /// </summary>
        /// <param name="uid">The uid.</param>
        /// <returns></returns>
        public bool PlayerCubeExists(int uid)
        {
            return Players.ContainsKey(uid);
        }

        /// <summary>
        /// If the food the cube exists based on the uid.
        /// </summary>
        /// <param name="uid">The uid.</param>
        /// <returns></returns>
        public bool FoodCubeExists(int uid)
        {
            return Food.ContainsKey(uid);
        }

        /// <summary>
        /// Gets the number of players
        /// </summary>
        public int PlayersCount => Players.Count;

        /// <summary>
        /// Gets the number of foods
        /// </summary>
        public int FoodCount => Food.Count;

        /// <summary>
        /// Gets the player cubes.
        /// </summary>
        /// <returns>The player cubes</returns>
        public IEnumerable<Cube> GetPlayerCubes()
        {
            lock (this)
            {
                return Players.Values.ToArray();
            }
        }

        /// <summary>
        /// Gets the player cubes.
        /// </summary>
        /// <returns>The player cubes</returns>
        public IEnumerable<Cube> GetFoodCubes()
        {
            lock (this)
            {
                return Food.Values.ToArray();
            }
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
                    AddFoodCube(b);

                return;
            }

            lock (this)
            {
                Food.Remove(b.Uid); // A food cube is never updated, only removed //
            }
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
                    AddPlayerCube(b);
                return;
            }

            lock (this)
            {
                if (b.Mass == 0)
                    Players.Remove(b.Uid);
                else
                    Players[b.Uid] = b;
            }
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
