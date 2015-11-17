using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Network_Controller
{
    public class NetworkManager
    {
        public static event Action<string[]> PacketListener;
        public static event Action<Exception> ServerException;

        private static NetworkManager _instanceNetworkManager;

        /// <summary>
        /// Gets the connected client socket.
        /// </summary>
        /// <value>
        /// The socket.
        /// </value>
        private Socket Socket => Client?.Client;

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>t
        private TcpClient Client { get; }

        /// <summary>
        /// Gets if the client is connected.
        /// </summary>
        public static bool Connected => _instanceNetworkManager?.Client?.Connected ?? false;

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
        /// <param name="address">Address to connect to</param>
        /// <param name="port">The port to bind on</param>
        public static void Connect(string address, int port = 11000)
        {
            _instanceNetworkManager = new NetworkManager();

            var client = _instanceNetworkManager.Client;

            if (client.Connected)
                client.Close();


            client.Connect(address, port);
        }

        public static void Start()
        {
            var client = _instanceNetworkManager.Client;

            if (!client.Connected)
                throw new IOException("You must connect the client before starting it.");

            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += BeginRead;
            bgWorker.RunWorkerAsync();
        }

        private static void BeginRead(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            try
            {
                // If we're not connected, stop reading //
                if (!_instanceNetworkManager.Socket.Connected) return;

                var streamReader = _instanceNetworkManager.Client.GetStream();
                var chunky = string.Empty;

                while (streamReader.CanRead)
                {
                    var bytes = new byte[short.MaxValue * 2];

                    var length = streamReader.Read(bytes, 0, short.MaxValue * 2);

                    var packet = Encoding.UTF8.GetString(bytes).Substring(0, length);

                    chunky += packet;

                    var chunks = chunky.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    var last = chunks.Last();

                    if (!last.EndsWith("}"))
                    {
                        // If it's not a valid json, we assuming its not finished //
                        chunky = last;
                        chunks = chunks.Take(chunks.Length - 1).ToArray();
                    }
                    else
                    {
                        chunky = string.Empty;
                    }

                    try
                    {
                        PacketListener?.Invoke(chunks);
                    }
                    catch (Exception e)
                    {
                        // Ignore 
                    }
                }
            }
            catch (Exception e)
            {
                Disconnect();

                try
                {
                    ServerException?.Invoke(e);
                }
                catch
                {
                    // Ignore 
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

        /// <summary>
        /// Sends a command to the server
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <param name="x">The x coord</param>
        /// <param name="y">The y coord</param>
        public static void SendCommand(string command, int x, int y)
        {
            var manager = Get();

            if (!manager.Client.Connected) return;

            var stream = manager.Client.GetStream();
            var commandQuery = $"({command}, {x}, {y})";

            var commandBytes = Encoding.UTF8.GetBytes(commandQuery);
            stream.BeginWrite(commandBytes, 0, commandBytes.Length, null, null); // We don't care if it didn't finish //
        }

        /// <summary>
        /// Sends a "QUIT" command to the server as well as disconnects from the socket
        /// </summary>
        public static void Quit()
        {
            try
            {
                var manager = Get();

                if (!manager.Client.Connected) return;

                var stream = manager.Client.GetStream();

                var commandBytes = Encoding.UTF8.GetBytes("QUIT");
                stream.Write(commandBytes, 0, commandBytes.Length);

                Disconnect();
            }
            catch
            {
                // Ignore
            }
        }

        /// <summary>
        /// Sends the name to the server
        /// </summary>
        /// <param name="name">The name to send</param>
        /// <returns>The json response from the server</returns>
        public static string SendName(string name)
        {
            var manager = Get();

            var nameBytes = Encoding.UTF8.GetBytes(name);
            var stream = manager.Client.GetStream();

            stream.Write(nameBytes, 0, nameBytes.Length); // We don't care if it didn't finish //

            var bytes = new byte[1024];
            stream.Read(bytes, 0, bytes.Length);

            return Encoding.UTF8.GetString(bytes).Trim('\0');
        }
    }
}
