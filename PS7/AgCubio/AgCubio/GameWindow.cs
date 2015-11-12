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

namespace AgCubio
{
    public partial class GameWindow : Form
    {
        private readonly World _world;

        private NetworkManager _networkManager;

        public int myTeamId { get; set; }

        public GameWindow()
        {
            InitializeComponent();
            _world = new World();
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
                cube.Width * 10, cube.Height * 10);

            g.DrawString(cube.Name, Font, b, cube.X, cube.Y);
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
            (new ConnectForm()).ShowDialog(this);

            _networkManager = NetworkManager.Get();
            CheckConnected();

            _networkManager.PacketListener = (stringy) =>
            {
                Cube bCube = Cube.FromJson(stringy);

                Debug.WriteLine(stringy);

                lock (_world)
                    _world.UpdateCube(bCube);
            };

            DoBackgroundWork(args => InvokeDrawer());
        }

        private void CheckConnected()
        {
            if (_networkManager.Client.Connected) return;

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
                _world.SplitMyCubes(0); 
            }

            if (e.KeyCode == Keys.W)
            {
                _world.EjectMassFromMyCubes(0); 
            }
        }

        private void GameWindow_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

       
        private void InvokeDrawer()
        {
            if (Disposing) return;

            Thread.Sleep(50);
            
            DoForegroundWork(Refresh);

            InvokeDrawer();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            var cubes = _world.Cubes.ToArray();

            foreach (KeyValuePair<int, Cube> t in cubes)
            {
                var cube = t.Value;

                DrawCube(g, cube);
            }

            base.OnPaint(e);
        }
    }
}
