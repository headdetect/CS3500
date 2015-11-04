using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RemoteLib.Net;
using RemoteLib.Net.TCP;
using SpreadsheetGUI.Networking.Packets;

namespace SpreadsheetGUI
{
    public static class Program
    {
        public static TcpRemoteServer Server;
        public static TcpRemoteClient Client;
        public static TcpRemoteClient ServerClient;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Packet.RegisterPacket(typeof(PacketCellUpdate));
            Packet.RegisterPacket(typeof(PacketSelectionChanged));
            Packet.RegisterPacket(typeof(PacketSpreadsheetReady));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Workbench());
        }

        public static void BeginNetworkTransactions()
        {
            if (Server == null) throw new NullReferenceException("Server is null");

            RemoteClient.ClientJoined += TcpRemoteClient_ClientJoined;
        }

        private static void TcpRemoteClient_ClientJoined(object sender, RemoteLib.Net.ClientConnectionEventArgs e)
        {
            ServerClient?.Disconnect(); // Kick the connected user if connected //

            ServerClient = e.RemoteClient as TcpRemoteClient;

            Debug.WriteLine($"Client Joined ({ServerClient?.TcpClient.Client.RemoteEndPoint})");
        }

        internal static void StopNetworkTransactions()
        {
            RemoteClient.ClientJoined -= TcpRemoteClient_ClientJoined;

            ServerClient?.Disconnect();
            Server?.Stop();
        }
    }
}
