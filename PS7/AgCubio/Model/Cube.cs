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
        public long uid { get; set; }
        public Point coord { get; set; }
        public Color color { get; set; }
        public string name { get; set; }
        public double mass { get; set; }
        public bool isFood { get; set; }

        public int Width => Math.Max(1, (int) (mass / 9));
        public int Top => coord.Y;
        public int Left => coord.X;
        public int Right => coord.X + Width;
        public int Bottom => coord.Y + Width;
    }
}