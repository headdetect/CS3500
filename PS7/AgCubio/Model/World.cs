using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class World
    {
        public Dictionary<int, Cube> Cubes { get; set; }

        public const int Width = 100;

        public const int Height = 75;

        public World()
        {
            Cubes = new Dictionary<int, Cube>();
        }

        public World(Dictionary<int, Cube> cubes)
        {
            Cubes = cubes;
        }

        public void AddCube(Cube b)
        {
            Cubes?.Add(b.Uid, b);
        }


    }
}
