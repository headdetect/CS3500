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
        /// <summary>
        /// Gets or sets my cube.
        /// </summary>
        /// <value>
        /// My cube.
        /// </value>
        public Cube MyCube { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether developer stats is enabled or disabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if developer stats enabled; otherwise, <c>false</c>.
        /// </value>
        public bool DeveloperStats { get; set; }

        private int _numberOfPacketsReceived, _numberOfPacketsSent;
        private readonly World _world;
        private readonly Stopwatch _watch;
        private int _frameCount;
        private float _fps;
        private int numberOfCubesOnTeam;

        /// <summary>
        /// Gets the cursor position relative to the window
        /// </summary>
        public int CursorPositionX { get; private set; }
        public int CursorPositionY { get; private set; }

        public GameWindow()
        {
            InitializeComponent();
            _world = new World();
            _watch = new Stopwatch();
            
        }
        

        private void GameWindow_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;

            Width = World.Width + 200; // 200 for the stats //
            Height = World.Height;

            (new ConnectForm()).ShowDialog(this);
            
            CheckConnected();

            NetworkManager.PacketListener += NetworkManager_PacketListener;

            var t = new Timer { Interval = 1 };
            t.Tick += T_Tick;
            t.Start();

            _watch.Start();
        }
        
        private void T_Tick(object sender, EventArgs e)
        {

            _fps = (float) Math.Round(_frameCount / (float) _watch.Elapsed.Seconds, 2);

            Invalidate();

            NetworkManager.SendCommand("move", CursorPositionX, CursorPositionY);
            _numberOfPacketsSent++;
        }

        private void GameWindow_MouseMove(object sender, MouseEventArgs e)
        {
            var relative = PointToClient(Cursor.Position);
            CursorPositionX = Math.Max(0, relative.X);
            CursorPositionY = Math.Max(0, relative.Y);
        }
        
        
        private void NetworkManager_PacketListener(string[] chunks)
        {
            foreach (var bCube in chunks.Select(Cube.FromJson))
            {
                _numberOfPacketsReceived++;


                if (MyCube == null || numberOfCubesOnTeam == 0)
                    MyCube = bCube;

                if (bCube.TeamId == MyCube.TeamId) numberOfCubesOnTeam++;

                lock (_world)
                {
                    if (bCube.IsFood) _world.UpdateFoodCube(bCube); // If a food cube packet is sent again, it means remove it //
                    else _world.UpdatePlayerCube(bCube);
                }

            }

            if (numberOfCubesOnTeam == 0)
                MyCubeDied();

            numberOfCubesOnTeam = 0;
        }

        /// <summary>
        /// Checks if player's cube's mass went down to 0.  If it has, then the cube has died
        /// and the player will be asked if they wish to play again.
        /// </summary>
        private void MyCubeDied()
        {
            if (MyCube.Mass == 0d)
            {
                var result = MessageBox.Show(@"You have died! Do you want to play again?",
                    @"You died!", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);

                if (result == DialogResult.OK)
                {
                    (new ConnectForm()).ShowDialog(this);
                    CheckConnected();
                }

                if (result == DialogResult.Cancel)
                    Close();
            }
        }

        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.Space)
            {
                NetworkManager.SendCommand("split", CursorPositionX, CursorPositionY);
                _numberOfPacketsSent++;
            }

            if (e.KeyCode == Keys.G)
            {
                DeveloperStats = !DeveloperStats;
            }

            if (e.KeyCode == Keys.Up)
            {
                CursorPositionY--;
            }

            if (e.KeyCode == Keys.Down)
            {
                CursorPositionY++;
            }

            if (e.KeyCode == Keys.Left)
            {
                CursorPositionX--;
            }

            if (e.KeyCode == Keys.Right)
            {
                CursorPositionX++;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.Clear(BackColor);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            
            lock (_world)
            {
                foreach (var cube in _world.Players.Where(cube => cube != null))
                {
                    DrawPlayerCube(g, cube);
                }

                foreach (var cube in _world.Food.Where(cube => cube != null))
                {
                    DrawFoodCube(g, cube);
                }
            }

            DrawStats(g);
            
            _frameCount++;

            base.OnPaint(e);
        }

        

        #region Drawing

        /// <summary>
        /// Draws the specified cube
        /// </summary>
        /// <param name="g">Graphics to draw the cube on</param>
        /// <param name="cube">The cube to draw</param>
        private void DrawPlayerCube(Graphics g, Cube cube)
        {
            Brush b = new SolidBrush(cube.Color);

            g.FillRectangle(b, cube.Left, cube.Top, cube.Width, cube.Height);

            var nameSize = g.MeasureString(cube.Name, Font);

            if (DeveloperStats)
            {
                g.DrawLine(Pens.Red, cube.Left, cube.Y, cube.Right, cube.Y);
                g.DrawLine(Pens.Red, cube.X, cube.Top, cube.X, cube.Bottom);
            }

            // If it can fit in the cube //
            if (nameSize.Width < cube.Width)
                g.DrawString(cube.Name, Font, Brushes.Yellow, cube.X - nameSize.Width / 2, cube.Y - nameSize.Height / 2);
        }

        /// <summary>
        /// Draws the specified cube
        /// </summary>
        /// <param name="g">Graphics to draw the cube on</param>
        /// <param name="cube">The cube to draw</param>
        private void DrawFoodCube(Graphics g, Cube cube)
        {
            if (cube.Mass == 0)
                return;

            Brush b = new SolidBrush(cube.Color);
            g.FillRectangle(b, cube.X, cube.Y, cube.Width * 10, cube.Height * 10);

            if (DeveloperStats)
            {
                var text = _world.GetFoodCubeIndex(cube.Uid).ToString();
                var nameSize = g.MeasureString(text, Font);
                g.DrawString(text, Font, Brushes.Black,
                    cube.X - nameSize.Width/2, cube.Y - nameSize.Height/2);
            }
        }
        

        private void DrawStats(Graphics g)
        {
            var left = World.Width + 15; // 15px for padding //

            // Calculate FPS //

            g.DrawString($"FPS: {_fps}", Font, Brushes.Black, left, 15);

            if (MyCube != null)
            {
                g.DrawString($"Mass: {MyCube.Mass}", Font, Brushes.Black, left, 40);
                g.DrawString($"Size: {MyCube.Width}", Font, Brushes.Black, left, 60);
            }

            g.DrawString($"Foods: {_world.Food.Count}", Font, Brushes.Black, left, 85);
            g.DrawString($"Players: {_world.Players.Count}", Font, Brushes.Black, left, 105);
            

            if (DeveloperStats)
            {
                g.DrawString($"Packets Sent: {_numberOfPacketsSent}", Font, Brushes.Black, left, 135);
                g.DrawString($"Packets Got: {_numberOfPacketsReceived}", Font, Brushes.Black, left, 155);
            }
        }

        #endregion

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

    }
}
