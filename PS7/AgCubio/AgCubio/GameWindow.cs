using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Model;

namespace AgCubio
{
    public partial class GameWindow : Form
    {
        private World world;
        public GameWindow()
        {
            InitializeComponent();
            world = new World();
            world.Cubes.Add(0, new Cube()
            {
                color = Color.Aqua, coord = new Point(34, 67), mass = 90,
                isFood = false, name = "hi", uid = 0
            });
        }

        private void GameWindow_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            
            foreach (KeyValuePair<int, Cube> cube in world.Cubes)
            {
                Brush b = new SolidBrush(cube.Value.color);
                g.FillRectangle(b, cube.Value.Left, cube.Value.Top, 
                    cube.Value.Width, cube.Value.Width);
            }
        }

        private void GameWindow_MouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}
