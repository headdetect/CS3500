using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RemoteLib.Net.TCP;

namespace SpreadsheetGUI
{
    public partial class Connect : Form
    {
        private bool clientConnected, serverStarted;

        public Connect()
        {
            InitializeComponent();
        }

        private void Connect_Load(object sender, EventArgs e)
        {
            if (Program.Client != null && Program.Client.TcpClient.Connected)
            {
                button2.Text = @"Disconnect";
                clientConnected = true;
            }

            if (Program.Server != null && Program.Server.Running)
            {
                button1.Text = @"Stop Collaboration";
                serverStarted = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!clientConnected)
            {
                // Not connected, lets connect. //
                var address = txtAddress.Text;

                IPAddress ipAddress;
                if (IPAddress.TryParse(address, out ipAddress))
                {
                    DoBackgroundWork(args =>
                    {
                        try
                        {
                            var client = new TcpClient();
                            client.Connect(ipAddress, 45903);

                            Program.Client = new TcpRemoteClient(client);
                            Program.Client.StartClient();

                            DoForegroundWork(() =>
                            {
                                button2.Text = @"Disconnect";
                                clientConnected = true;

                                MessageBox.Show($"Successfully connected to {address}", @"Success!",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                                Close(); // Close dialog //
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error trying to connect to {address}\n\n{ex.Message}");
                        }
                    });
                }
                else
                {
                    MessageBox.Show($"{address} is not a valid IP address");
                }
            }
            else
            {
                // We're already connected. Let's disconnect //
                DoBackgroundWork(args =>
                {
                    Program.Client?.Disconnect();
                    Program.Client = null;

                    DoForegroundWork(() =>
                    {
                        button2.Text = @"Connect";
                        clientConnected = true;
                    });
                });
            }
        }


        private void DoBackgroundWork(Action<DoWorkEventArgs> work)
        {
            var b = new BackgroundWorker();
            b.DoWork += (sender, e) => work(e);
            b.RunWorkerAsync();
        }

        private static readonly object ForegroundLock = new object();

        private void button1_Click(object sender, EventArgs e)
        {
            if (!serverStarted)
            {
                try
                {
                    // start the server if not running //
                    DoBackgroundWork(args =>
                    {
                        // Bind to every address. Port 45903 //
                        Program.Server = new TcpRemoteServer("0.0.0.0");
                        Program.Server.Start();
                        Program.BeginNetworkTransactions();

                        DoForegroundWork(() =>
                        {
                            button1.Text = @"Stop Collaboration";
                            serverStarted = true;

                            MessageBox.Show($"Successfully started collaboration", @"Success!",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                            Close(); // Close dialog //
                        });
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error trying to host collaboration\n\n{ex.Message}");
                }
            }
            else
            {
                // Stop the server if running //
                Program.StopNetworkTransactions();

                DoForegroundWork(() =>
                {
                    button1.Text = @"Host Collaboration";
                    serverStarted = false;
                });
            }
        }

        private void txtAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button2_Click(sender, e);
        }

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
