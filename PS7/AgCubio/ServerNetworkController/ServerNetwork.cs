using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerNetworkController
{
    public class ServerNetwork
    {
        /// <summary>
        /// Event called when a client joins the server
        /// </summary>
        public static event Action<int, TcpClient> ClientJoined;


        /// <summary>
        /// Event called with a client leaves the server
        /// </summary>
        public static event Action<int> ClientLeft;

        public Dictionary<int, TcpClient> Clients { get; }

        public TcpListener TcpListener { get; }

        public bool Listening { get; private set; }

        public ServerNetwork()
        {
            Clients = new Dictionary<int, TcpClient>();
            TcpListener = new TcpListener(IPAddress.Any, 11000); // TODO: Change to use Consts
        }

        /// <summary>
        /// Starts listening for clients on the server. 
        /// Is a blocking proccess
        /// </summary>
        public void Listen()
        {
            TcpListener.Start();
            Listening = true;

            while (Listening)
            {
                var client = TcpListener.AcceptTcpClient();
                ClientJoined?.Invoke(client);
            }
        }

        /// <summary>
        /// Stop accepting new clients and closes all the connections
        /// </summary>
        public void Stop()
        {
            Listening = false;

            foreach (var client in Clients)
            {
                ClientLeft?.Invoke(client.Key);
                client.Value.Close();
            }

            TcpListener.Stop();
        }


        public void SendStringGlobal(string str)
        {
            foreach(var client in Clients)
                SendString(client.Key, str);
        }

        public void SendString(int uid, string str)
        {
            if (!Clients.ContainsKey(uid)) return;

            var stream = Clients[uid].GetStream();

            var bytes = Encoding.UTF8.GetBytes(str);

            stream.Write(bytes, 0, bytes.Length);
        }

    }
}
