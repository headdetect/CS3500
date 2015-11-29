using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerNetworkController
{
    /// <summary>
    /// Network class for the server implementation of AgCubio.
    /// </summary>
    public class ServerNetwork
    {
        /// <summary>
        /// Event called when a client joins the server
        /// </summary>
        public static event Action<int, TcpClient> ClientJoined;

        
        /// <summary>
        /// Event called when a string packet is received
        /// </summary>
        public static event Action<int, string> PacketReceived;


        /// <summary>
        /// Event called with a client leaves the server
        /// </summary>
        public static event Action<int> ClientLeft;


        /// <summary>
        /// A dicitonary of all the clients connected to the server. Using the UID as the key.
        /// </summary>
        public Dictionary<int, TcpClient> Clients { get; }


        /// <summary>
        /// The TcpListener of the server. 
        /// </summary>
        public TcpListener TcpListener { get; }

        /// <summary>
        /// Gets if the server is currently accepting connection. 
        /// To set to false, close the connection by using <see cref="Stop()"/>
        /// </summary>
        public bool Listening { get; private set; }
        
        /// <summary>
        /// Creates a new server network
        /// </summary>
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
            TcpListener.Start(); // Starts the listener
            Listening = true;

            while (Listening)
            {
                var client = TcpListener.AcceptTcpClient(); // Accepts a new TcpClient from the listener //
                
                var uid = FindNextUid();

                if (uid == -1)
                {
                    client.Close();
                    // We are full. Soz bruh //
                    continue;
                }

                ClientJoined?.Invoke(uid, client); // Send an event that we've received a client.

                new Thread(() => BeginClientListen(uid, client)).Start();
            }
        }

        /// <summary>
        /// Will listen for client commands being sent to the server from the client
        /// </summary>
        /// <param name="uid">The uid of the client</param>
        /// <param name="client">the client</param>
        private void BeginClientListen(int uid, TcpClient client)
        {
            try
            {
                var stream = client.GetStream();

                while (stream.CanRead && Listening)
                {
                    var chunk = new byte[1024]; // Assuming the name isn't over 1024 bytes... //

                    var size = stream.Read(chunk, 0, chunk.Length);

                    var packet = Encoding.UTF8.GetString(chunk, 0, size);

                    PacketReceived?.Invoke(uid, packet); // Send an event that we've received a packet
                }
            }
            catch (IOException)
            {
                if (Clients.ContainsKey(uid))
                    Clients.Remove(uid);

                try
                {
                    client.Close();
                }
                catch
                {
                    // Ignore //
                }

                ClientLeft?.Invoke(uid);
            }
        }

        /// <summary>
        /// Stop accepting new clients and closes all the connections
        /// </summary>
        public void Stop()
        {
            Listening = false; // Stops the loop that is listening for clients.

            foreach (var client in Clients)
            {
                ClientLeft?.Invoke(client.Key); // Send an event that this client has disconnected  
                client.Value.Close(); // Now disconnect the client.
            }

            TcpListener.Stop(); // Stop the server.
        }


        /// <summary>
        /// Sends the specified string arrays to all of the clients.
        /// </summary>
        /// <param name="strs"></param>
        public void SendStringsGlobal(string[] strs)
        {
            foreach (var client in Clients)
                SendStrings(client.Key, strs);
        }

        /// <summary>
        /// Sends the specified string to all clients.
        /// </summary>
        /// <param name="str">The string to send</param>
        public void SendStringGlobal(string str)
        {
            foreach (var client in Clients)
                SendString(client.Key, str);
        }

        /// <summary>
        /// Will send the specified string to the specified client. Will append string with new line.
        /// </summary>
        /// <param name="uid">The client to send the data to</param>
        /// <param name="str">The string data to send</param>
        public void SendString(int uid, string str)
        {
            if (!Clients.ContainsKey(uid)) return;

            var stream = Clients[uid].GetStream();

            var bytes = Encoding.UTF8.GetBytes(str + "\n");

            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Sends an array of strings to the specified client. Will append a newline to each entry
        /// </summary>
        /// <param name="uid">The UID to send it to</param>
        /// <param name="strs">The array of strings</param>
        public void SendStrings(int uid, string[] strs)
        {
            if (!Clients.ContainsKey(uid)) return;

            var stream = Clients[uid].GetStream();

            var memStream = new MemoryStream();

            foreach (var bytes in strs.Select(str => Encoding.UTF8.GetBytes(str + "\n")))
            {
                memStream.Write(bytes, 0, bytes.Length);
            }

            var buffer = memStream.GetBuffer();
            stream.Write(buffer, 0, buffer.Length);
        }

        private int FindNextUid()
        {
            for (var i = 0; i < 40; i++)
            {
                if (!Clients.ContainsKey(i))
                    return i;
            }
            return -1;
        }
    }
}
