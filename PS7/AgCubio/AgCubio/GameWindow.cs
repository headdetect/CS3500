using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Sockets;
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
        /// Gets or sets the level of developer stats to display.
        /// </summary>
        /// <value>
        /// 0 - disabled
        /// 1 - borders, grids
        /// 2 - borders, grids, networking
        /// 3 - borders, grids, networking, index info
        /// </value>
        public int DeveloperStats { get; set; }

        /// <summary>
        /// Gets a value indicating whether we want to keep playing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if keep playing; otherwise, <c>false</c>.
        /// </value>
        internal bool KeepPlaying { get; private set; }

        private int _numberOfPacketsReceived, _numberOfPacketsSent;
        private readonly World _world;
        private readonly Stopwatch _watch, _totalWatch;
        private int _frameCount;
        private float _fps;
        private Cube _myCube;
        private readonly List<Cube> _teamCubes;
        private readonly Timer _timer;

        public GameWindow()
        {
            InitializeComponent();
            _world = new World();
            _watch = new Stopwatch();
            _totalWatch = new Stopwatch();
            _teamCubes = new List<Cube>();
            _timer = new Timer { Interval = 1 };
        }


        private void GameWindow_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;

            Width = World.Width + 200; // 200 for the stats //
            Height = World.Height;

            NetworkManager.Quit();

            var connectForm = new ConnectForm();
            connectForm.ShowDialog(this);

            CheckConnected();

            if (Disposing || IsDisposed) return;

            // Remove any pre-existing listeners //
            NetworkManager.PacketListener -= NetworkManager_PacketListener;
            NetworkManager.ServerException -= NetworkManager_ServerException;

            NetworkManager.PacketListener += NetworkManager_PacketListener;
            NetworkManager.ServerException += NetworkManager_ServerException;

            _myCube = Cube.FromJson(connectForm.MyCubeJson);
            
            _timer.Tick += T_Tick;
            _timer.Start();

            _watch.Start();
            _totalWatch.Start();
        }

        private void NetworkManager_ServerException(Exception obj)
        {
            if (IsDisposed || Disposing) return; // Just ignore //

            MessageBox.Show(@"Disconnected from server");

            KeepPlaying = true;
            DoForegroundWork(Close);
        }

        private TimeSpan _prevTime = TimeSpan.Zero;

        private void T_Tick(object sender, EventArgs e)
        {
            if ((_watch.Elapsed - _prevTime).TotalSeconds >= 1d)
            {
                _fps = (float)Math.Round(_frameCount / (float)_watch.Elapsed.Seconds, 2);
                _frameCount = 0; // Reset frame count, so its not, but relative to this current second. //
                _watch.Restart();
                _prevTime = _watch.Elapsed;
            }

            Invalidate();

            NetworkManager.SendCommand("move", Cursor.Position.X, Cursor.Position.Y);
            _numberOfPacketsSent++;
        }

        private void NetworkManager_PacketListener(string[] chunks)
        {
            foreach (var bCube in chunks.Select(Cube.FromJson))
            {
                _numberOfPacketsReceived++;

                lock (_world)
                {
                    if (bCube.IsFood) _world.UpdateFoodCube(bCube); // If a food cube packet is sent again, it means remove it //
                    else _world.UpdatePlayerCube(bCube);
                }

                if (_myCube == null) continue;

                if (_myCube.Uid == bCube.Uid)
                {
                    _myCube = bCube;
                }

                lock (_teamCubes)
                {
                    if (_myCube.TeamId != bCube.TeamId || bCube.TeamId == 0) continue;

                    var index = _teamCubes.FindIndex(cube => cube.Uid == bCube.Uid);

                    if (!bCube.IsDead)
                    {
                        if (index == -1)
                        {
                            _teamCubes.Add(bCube);
                        }
                        else
                        {
                            _teamCubes[index] = bCube;
                        }
                    }
                    else
                    {
                        if (index != -1)
                        {
                            _teamCubes.RemoveAll(cube => cube.Uid == bCube.Uid);
                        }
                    }
                }
            }

            if (_myCube?.IsDead ?? false)
                MyCubeDied();
        }

        /// <summary>
        /// Checks if player's cube's mass went down to 0.  If it has, then the cube has died
        /// and the player will be asked if they wish to play again.
        /// </summary>
        private void MyCubeDied()
        {
            if (_myCube?.Mass != 0d) return;

            _myCube = null;
            _timer.Stop();

            DoForegroundWork(() =>
            {
                var result = MessageBox.Show($"You have died\nYou lasted: {_totalWatch.Elapsed.TotalSeconds} seconds.\n  Do you want to play again?",
                    @"You died!", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                {
                    KeepPlaying = true;
                }

                NetworkManager.Quit();
                Close();

            });
        }

        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Space)
            {
                NetworkManager.SendCommand("split", Cursor.Position.X, Cursor.Position.Y);
                _numberOfPacketsSent++;
            }

            if (e.KeyCode == Keys.G)
            {
                DeveloperStats = (DeveloperStats + 1) % 4;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.Clear(BackColor);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            Transform(g);

            lock (_world)
            {
                foreach (var cube in _world.Players)
                {
                    DrawPlayerCube(g, cube.Value);
                }

                foreach (var cube in _world.Food)
                {
                    DrawFoodCube(g, cube.Value);
                }
            }

            _frameCount++;

            if (DeveloperStats >= 1)
                g.DrawRectangle(Pens.Black, 0, 0, World.Width, World.Height); // World Boundries //

            DrawStats(g);

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

            if (DeveloperStats >= 1)
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

            if (DeveloperStats >= 3)
            {
                var text = cube.Uid.ToString();
                var nameSize = g.MeasureString(text, Font);
                g.DrawString(text, Font, Brushes.Black,
                    cube.X - nameSize.Width / 2, cube.Y - nameSize.Height / 2);
            }
        }


        /// <summary>
        /// Draws the stats.
        /// </summary>
        /// <param name="g">The graphics.</param>
        private void DrawStats(Graphics g)
        {
            // Reset transformation //
            g.Transform = new Matrix();

            var left = World.Width + 15; // 15px for padding //

            g.FillRectangle(new SolidBrush(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF)), World.Width, 0, 200, 210);

            // Calculate FPS //

            g.DrawString($"FPS: {_fps}", Font, Brushes.Black, left, 15);

            if (_myCube != null)
            {
                g.DrawString($"Mass: {_myCube.Mass}", Font, Brushes.Black, left, 40);
                g.DrawString($"Size: {_myCube.Width}", Font, Brushes.Black, left, 60);
            }

            g.DrawString($"Foods: {_world.Food.Count}", Font, Brushes.Black, left, 85);
            g.DrawString($"Players: {_world.Players.Count}", Font, Brushes.Black, left, 105);

            if (DeveloperStats > 0)
            {
                g.DrawString($"Developer Mode: {DeveloperStats}", Font, Brushes.Black, left, 135);
            }

            if (DeveloperStats >= 2)
            {
                g.DrawString($"Packets Sent: {_numberOfPacketsSent}", Font, Brushes.Black, left, 160);
                g.DrawString($"Packets Got: {_numberOfPacketsReceived}", Font, Brushes.Black, left, 180);
            }
        }

        /// <summary>
        /// Transforms the specified graphics.
        /// Will zoom in and orientate based on the Player Cube
        /// </summary>
        /// <param name="g">The graphics.</param>
        private void Transform(Graphics g)
        {
            if (_myCube == null) return;

            float leftMost = _myCube.Left;
            float rightMost = _myCube.Right;
            float topMost = _myCube.Top;
            float bottomMost = _myCube.Bottom;

            lock (_teamCubes)
            {
                foreach (var cube in _teamCubes)
                {
                    leftMost = Math.Min(leftMost, cube.Left);
                    topMost = Math.Min(topMost, cube.Top);

                    rightMost = Math.Max(rightMost, cube.Right);
                    bottomMost = Math.Max(bottomMost, cube.Bottom);
                }
            }

            var viewWidth = rightMost - leftMost;
            var viewHeight = bottomMost - topMost;

            var scale = 1.5f;

            var theMatrixReloaded = new Matrix();

            var dx = MathUtils.Clamp(-(leftMost - (World.Width / 2f) + viewWidth), -World.Width + (World.Width * scale) / 2, 0);
            var dy = MathUtils.Clamp(-(topMost - (World.Height / 2f) + viewHeight), -World.Height + (World.Height * scale) / 2, 0);


            theMatrixReloaded.Scale(scale, scale);
            theMatrixReloaded.Translate(dx, dy);

            g.Transform = theMatrixReloaded;

            if (DeveloperStats >= 1)
                g.DrawRectangle(Pens.Orange, leftMost, topMost, viewWidth, viewHeight);

        }

        #endregion

        private void GameWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (NetworkManager.Connected)
                    NetworkManager.Quit();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Checks the connection.
        /// </summary>
        private void CheckConnected()
        {
            if (NetworkManager.Connected) return;

            NetworkManager.Quit();
            DoForegroundWork(Close);
        }


        /// <summary>
        /// The foreground lock
        /// </summary>
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

    }

}
