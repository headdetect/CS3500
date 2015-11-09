using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network_Controller
{
    public class NetworkManager
    {
        private static NetworkManager _instanceNetworkManager; 

        public Socket Socket => Client?.Client;

        public TcpClient Client;

        private NetworkManager()
        {
            Client = new TcpClient();
        }

        public static NetworkManager Create()
        {
            return _instanceNetworkManager ?? (_instanceNetworkManager = new NetworkManager());
        }

        public static NetworkManager Get()
        {
            if (_instanceNetworkManager == null) throw new NullReferenceException("Create() must be called first.");

            return _instanceNetworkManager;
        }

        /// <summary>
        /// Connect to the specified server.
        /// </summary>
        /// <param name="address">Address to connect to</param>
        /// <param name="port">The port to bind on</param>
        public static async void ConnectAsync(string address, int port = 11000)
        {
            var manager = Get();

            if (manager.Client.Connected)
                manager.Client.Close();

            await manager.Client.ConnectAsync(address, port);
        }

        /// <summary>
        /// Connect to the specified server. <br />
        /// Use <see cref="ConnectAsync"/>.
        /// </summary>
        /// <param name="address">Address to connect to</param>
        /// <param name="port">The port to bind on</param>
        [Obsolete("Use ConnectAsync()")]
        public static void Connect(string address, int port = 11000)
        {
            var manager = Get();

            if (manager.Client.Connected)
                manager.Client.Close();

            manager.Client.Connect(address, port);
        }

        public static void Disconnect()
        {
            var manager = Get();

            manager.Client?.Close();
        }
    }
}
