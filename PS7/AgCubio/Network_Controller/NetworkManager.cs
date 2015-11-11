using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Network_Controller
{
    public class NetworkManager
    {
        public Action<string> PacketListener;  

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
        /// <param name="name">Whacha name bruh?</param>
        /// <param name="address">Address to connect to</param>
        /// <param name="port">The port to bind on</param>
        public static void Connect(string name, string address, int port = 11000)
        {
            _instanceNetworkManager = new NetworkManager();

            var client = _instanceNetworkManager.Client;

            if (client.Connected)
                client.Close();


            client.Connect(address, port);

            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += BeginRead;
            bgWorker.RunWorkerAsync();

            var nameBytes = Encoding.UTF8.GetBytes(name);
            client.GetStream().Write(nameBytes, 0, nameBytes.Length);
        }

        private static void BeginRead(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            // If we're not connected, stop reading //
            if (!_instanceNetworkManager.Socket.Connected) return;

            var streamReader = _instanceNetworkManager.Client.GetStream();
            var bytes = new List<byte>();
            while (streamReader.CanRead)
            {
                var byteo = (byte) streamReader.ReadByte();
                bytes.Add(byteo);

                if (byteo == '\n')
                {
                    // Do something else //
                    var result = Encoding.UTF8.GetString(bytes.ToArray());
                    bytes.Clear();

                    _instanceNetworkManager.PacketListener?.Invoke(result);
                }

                
            }
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
