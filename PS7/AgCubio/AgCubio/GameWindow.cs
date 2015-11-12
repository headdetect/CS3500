using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Model;
using Network_Controller;
using Timer = System.Windows.Forms.Timer;

namespace AgCubio
{
    public partial class GameWindow : Form
    {
        private readonly World _world;
        
        public Cube MyCube { get; set; }

        public GameWindow()
        {
            InitializeComponent();
            _world = new World();
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
                cube.Width, cube.Height);

            g.DrawString(cube.Name, Font, Brushes.Aqua, cube.X, cube.Y);
        }

        #endregion


        #region Utils

        /// <summary>
        /// Used to do operations on the underlying business logic thread.
        /// </summary>
        /// <param name="work"></param>
        private void DoBackgroundWork(Action<DoWorkEventArgs> work)
        {
            var b = new BackgroundWorker();
            b.DoWork += (sender, e) => work(e);
            b.RunWorkerAsync();
        }

        private static readonly object ForegroundLock = new object();

        /// <summary>
        /// Used to do operations on the GUI thread.
        /// </summary>
        /// <param name="work"></param>
        private void DoForegroundWork(Action work)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(work);
                }
                else
                {
                    lock (ForegroundLock)
                    {
                        work();
                    }
                }
            }
            catch
            {
            }

        }

        #endregion

        private void GameWindow_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;

            (new ConnectForm()).ShowDialog(this);
            
            CheckConnected();

            NetworkManager.PacketListener += NetworkManager_PacketListener;

            var t = new Timer { Interval = 60 };
            t.Tick += T_Tick;
            t.Start();
        }

        private void T_Tick(object sender, EventArgs e)
        {
            lblCursorCoords.Text = $"({Cursor.Position.X}, {Cursor.Position.Y})";

            NetworkManager.SendCommand("move", Cursor.Position.X, Cursor.Position.Y);
            Invalidate();
        }

        private void NetworkManager_PacketListener(string stringy)
        {
            var bCube = Cube.FromJson(stringy);

            if (MyCube == null)
                MyCube = bCube;
            
            _world.UpdateCube(bCube);
         }

        private void CheckConnected()
        {
            if (NetworkManager.Connected) return;

            var result = MessageBox.Show(@"You must connect to a server to play AgCubio", @"Must connect to server",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Error);

            if (result == DialogResult.OK)
            {
                (new ConnectForm()).ShowDialog(this);
                CheckConnected();
            }

            if (result == DialogResult.Cancel)
                Close();
        }
        
        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            //TODO: handle teamid
            if (e.KeyCode == Keys.Space)
            {
                NetworkManager.SendCommand("split", Cursor.Position.X, Cursor.Position.Y);
            }

            if (e.KeyCode == Keys.W)
            {
                _world.EjectMassFromMyCubes(0); 
            }
        }

        private void GameWindow_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }
        

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            var cubes = _world.Cubes.ToArray();

            foreach (var cube in cubes)
            {
                DrawCube(g, cube);
            }

            base.OnPaint(e);
        }
    }
}
