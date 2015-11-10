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

        /// <summary>
        /// Gets the connected client socket.
        /// </summary>
        /// <value>
        /// The socket.
        /// </value>
        public Socket Socket => Client?.Client;

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public TcpClient Client { get; }

        private NetworkManager()
        {
            Client = new TcpClient();
        }

        /// <summary>
        /// Creates a network instance.
        /// </summary>
        /// <returns>the initialized instance</returns>
        public static NetworkManager Create()
        {
            return _instanceNetworkManager ?? (_instanceNetworkManager = new NetworkManager());
        }

        /// <summary>
        /// Gets the initialized instance.
        /// </summary>
        /// <returns>The initialized instance</returns>
        public static NetworkManager Get()
        {
            if (_instanceNetworkManager == null) throw new NullReferenceException("Create() must be called first.");

            return _instanceNetworkManager;
        }

        /// <summary>
        /// Connect to the specified server. <br />
        /// </summary>
        /// <param name="address">Address to connect to</param>
        /// <param name="port">The port to bind on</param>
        public static void Connect(string address, int port = 11000)
        {
            var manager = Get();

            if (manager.Client.Connected)
                manager.Client.Close();

            manager.Client.Connect(address, port);
        }

        /// <summary>
        /// Disconnects the client from the connected server.
        /// </summary>
        public static void Disconnect()
        {
            var manager = Get();

            manager.Client?.Close();
        }
    }
}
