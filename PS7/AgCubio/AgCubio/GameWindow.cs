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
using Network_Controller;

namespace AgCubio
{
    public partial class GameWindow : Form
    {
        private readonly World _world;

        private readonly NetworkManager _networkManager;

        public GameWindow()
        {
            InitializeComponent();
            _world = new World();
            _world.AddCube(new Cube
            {
                Color = Color.Aqua,
                Coord = new Point(34, 67),
                Mass = 90,
                IsFood = false,
                Name = "hi",
                Uid = 0
            });

            _networkManager = NetworkManager.Create();
        }

        private void GameWindow_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            
            foreach (KeyValuePair<int, Cube> cube in _world.Cubes)
            {
                DrawCube(g, cube.Value);
            }
        }

        private void GameWindow_MouseMove(object sender, MouseEventArgs e)
        {

        }


        #region Drawing

        /// <summary>
        /// Draws the specified cube
        /// </summary>
        /// <param name="g">Graphics to draw the cube on</param>
        /// <param name="cube">The cube to draw</param>
        private void DrawCube(Graphics g, Cube cube)
        {
            Brush b = new SolidBrush(cube.Color);
            g.FillRectangle(b, cube.Left, cube.Top,
                cube.Width, cube.Width);
        }

        #endregion
    }
}
