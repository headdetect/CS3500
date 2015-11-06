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
        public const int WIDTH = 100;
        public const int HEIGHT = 75;

        public World()
        {
            Cubes = new Dictionary<int, Cube>();
        }

        public World(Dictionary<int, Cube> cubes )
        {
            this.Cubes = cubes;
        }
    }
}
