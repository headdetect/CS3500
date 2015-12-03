using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Network_Controller;

namespace AgCubio
{
    public partial class ConnectForm : Form
    {
        /// <summary>
        /// Gets or sets the name of the game cube.
        /// </summary>
        /// <value>
        /// The name of the game cube.
        /// </value>
        public string MyCubeJson { get; set; }

        public ConnectForm()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Please fill out the address field");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please fill out the name field");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPort.Text) && !Regex.IsMatch(txtPort.Text, @"^\d$"))
            {
                MessageBox.Show("Please enter a valid port");
                return;
            }

            var address = txtAddress.Text;
            var port = int.Parse(txtPort.Text);
            var name = txtName.Text;

            lblStatus.Text = @"Connecting...";
            txtAddress.Enabled = false;
            txtPort.Enabled = false;

            DoBackgroundWork(args =>
            {
                try
                {
                    ClientNetworkManager.Connect(address, port);

                    MessageBox.Show(@"Successfully connected!", @"Success", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    MyCubeJson = ClientNetworkManager.SendName(name);

                    ClientNetworkManager.Start();

                    DoForegroundWork(Close);
                }
                catch (Exception ex)
                {
                    var result = MessageBox.Show($"Error connecting to server\n{ex.Message}",
                        @"Error connecting to server",
                        MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                    DoForegroundWork(() => lblStatus.Text = @"Error connecting to server");

                    if (result == DialogResult.Retry)
                        DoForegroundWork(() => btnConnect_Click(sender, e));
                }
                finally
                {
                    DoForegroundWork(() =>
                    {
                        txtAddress.Enabled = true;
                        txtPort.Enabled = true;
                    });
                }
            });

        }

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

        private void txtbox_keyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnConnect_Click(sender, new EventArgs());
        }
    }
}
