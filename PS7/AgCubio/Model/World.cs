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
        public List<Cube> Cubes { get; set; }

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
            Cubes = new List<Cube>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="World"/> class.
        /// </summary>
        /// <param name="cubes">The cubes of this world.</param>
        public World(List<Cube> cubes )
        {
            Cubes = cubes;
        }

        /// <summary>
        /// Adds a cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void AddCube(Cube b)
        {
            Cubes?.Add(b);
        }

        /// <summary>
        /// Adds a cube to the world.
        /// </summary>
        /// <param name="b">The cube.</param>
        public void UpdateCube(Cube b)
        {
            var findCube = GetCubeIndex(b.Uid);

            if (findCube == -1)
            {
                AddCube(b);
                return;
            }

            if (b.Mass == 0)
                Cubes.RemoveAt(findCube);
            else
                Cubes[findCube] = b;
        }

        /// <summary>
        /// Adds a live cube with half the mass of the original to the world.
        /// </summary>
        /// <param name="cube"></param>
        public void SplitMyCubes(int teamId)
        {
            
        }

        /// <summary>
        /// Ejects a food cube with one tenth the mass of the original
        /// </summary>
        /// <param name="cube"></param>
        public void EjectMassFromMyCubes(int teamId)
        {
            Cube cube = Cubes[teamId]; // Original cube (equal uid and team uid)

            Cube newCube = new Cube();

            newCube.Mass = cube.Mass * .1;
            cube.Mass *= .9;

            newCube.IsFood = true;
            newCube.Color = cube.Color;
            UpdateCube(newCube);
        }

        /// <summary>
        /// Finds a cube with that UID
        /// </summary>
        /// <param name="uid">the cube to find</param>
        /// <returns>A cube with that UID if exists; otherwise, returns null.</returns>
        public int GetCubeIndex(int uid)
        {
            return Cubes?.FindIndex(cube => cube.Uid == uid) ?? -1;
        }
        
        /// <summary>
        /// Finds a cube with that UID
        /// </summary>
        /// <param name="uid">the cube to find</param>
        /// <returns>A cube with that UID if exists; otherwise, returns null.</returns>
        public Cube GetCube(int uid)
        {
            var index = GetCubeIndex(uid);
            return index == -1 ? null : Cubes[index];
        }
    }
}
