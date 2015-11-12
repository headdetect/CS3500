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
        /// Gets or sets the cubes.
        /// </summary>
        /// <value>
        /// The cubes.
        /// </value>
        public Dictionary<int, Cube> Cubes { get; set; }

        /// <summary>
        /// The width of this world
        /// </summary>
        public const int Width = 100;

        /// <summary>
        /// The height world
        /// </summary>
        public const int Height = 75;

        /// <summary>
        /// Initializes a new instance of the <see cref="World"/> class.
        /// </summary>
        public World()
        {
            Cubes = new Dictionary<int, Cube>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="World"/> class.
        /// </summary>
        /// <param name="cubes">The cubes of this world.</param>
        public World(Dictionary<int, Cube> cubes)
        {
            Cubes = cubes;
        }

        /// <summary>
        /// Adds a cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void AddCube(Cube b)
        {
            Cubes?.Add(b.Uid, b);
        }

        /// <summary>
        /// Adds a cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void UpdateCube(Cube b)
        {
            if (!Cubes.ContainsKey(b.Uid))
                AddCube(b);
            Cubes[b.Uid] = b;
        }

        /// <summary>
        /// Adds a live cube with half the mass of the original to the world.
        /// </summary>
        /// <param name="cube"></param>
        public void SplitMyCubes(int parentuid)
        {
            foreach (Cube cube in Cubes.Values) //Iterate through cubes
            {
                if (cube.ParentUid == parentuid) // If cube belongs to player
                {
                    Cube newCube = new Cube();
                    cube.Mass /= 2; // Halve the cube's mass

                    newCube.Mass = cube.Mass; //Create a new, similar cube
                    newCube.IsFood = false;
                    newCube.ParentUid = cube.ParentUid;
                    newCube.Color = cube.Color;
                    UpdateCube(newCube);
                }
            }
        }

        /// <summary>
        /// Ejects a food cube with one tenth the mass of the original
        /// </summary>
        /// <param name="cube"></param>
        public void EjectMassFromMyCubes(int parentuid)
        {
            Cube cube = Cubes[parentuid]; // Original cube (equal uid and parent uid)

            Cube newCube = new Cube();

            newCube.Mass = cube.Mass * .1;
            cube.Mass *= .9;

            newCube.IsFood = true;
            newCube.Color = cube.Color;
            UpdateCube(newCube);
        }
    }
}
