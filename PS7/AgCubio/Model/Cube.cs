using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Cube
    {
        public int Uid { get; set; }
        public Point Coord { get; set; }
        public Color Color { get; set; }
        public string Name { get; set; }
        public double Mass { get; set; }
        public bool IsFood { get; set; }

        public int Width => Math.Max(1, (int) (Mass / 9));
        public int Height => Width;

        public int Top => Coord.Y;
        public int Left => Coord.X;
        public int Right => Coord.X + Width;
        public int Bottom => Coord.Y + Width;
    }
}