using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Network_Controller
{
    /// <summary>
    /// Network class for the server implementation of AgCubio.
    /// </summary>
    public class ServerNetworkManager
    {
        /// <summary>
        /// Gets or sets the uid start range. This is the starting point of where to generate UIDs
        /// </summary>
        /// <value>
        /// The uid start range.
        /// </value>
        public int UidStartRange { get; set; }

        /// <summary>
        /// Event called when a client joins the server
        /// </summary>
        public static event Action<Client> ClientJoined;


        /// <summary>
        /// Event called when a string packet is received
        /// </summary>
        public static event Action<Client, string> PacketReceived;

        /// <summary>
        /// Occurs when the player first sends their name
        /// </summary>
        public static event Action<Client> ClientSentName;


        /// <summary>
        /// Event called with a client leaves the server
        /// </summary>
        public static event Action<Client> ClientLeft;

        /// <summary>
        /// Occurs when the server is requesting a UID.
        /// </summary>
        public static event Func<int> RequestUID; 


        /// <summary>
        /// A dicitonary of all the clients connected to the server. Using the UID as the key.
        /// </summary>
        public Dictionary<int, Client> Clients { get; }

        /// <summary>
        /// Gets the TCP listener.
        /// </summary>
        /// <value>
        /// The TCP listener.
        /// </value>
        public TcpListener TcpListener { get; }

        /// <summary>
        /// Gets if the server is currently accepting connection. 
        /// To set to false, close the connection by using <see cref="Stop()"/>
        /// </summary>
        public bool Listening { get; private set; }

        /// <summary>
        /// Creates a new server network
        /// </summary>
        public ServerNetworkManager(int port, int uidStartRange)
        {
            UidStartRange = uidStartRange;
            Clients = new Dictionary<int, Client>();
            TcpListener = TcpListener.Create(port);
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
                var clientSocket = TcpListener.AcceptTcpClient(); // Accepts a new TcpClient from the listener //

                if (RequestUID == null) throw new InvalidOperationException("RequestUID event must be supplied.");

                var uid = RequestUID.Invoke();

                if (uid == -1)
                {
                    // We were unable to get the UID //

                    clientSocket.Close();
                    continue;
                }

                var client = new Client(clientSocket, uid);

                ClientJoined?.Invoke(client); // Send an event that we've received a client.
                lock (Clients)
                {
                    Clients.Add(uid, client);
                }

                new Thread(() => BeginClientListen(client)).Start();
            }
        }
        
        /// <summary>
        /// Will listen for client commands being sent to the server from the client
        /// </summary>
        /// <param name="client">the client</param>
        private void BeginClientListen(Client client)
        {
            try
            {
                var stream = client.TcpClient.GetStream();

                while (stream.CanRead && Listening)
                {
                    var chunk = new byte[1024]; // Assuming the name isn't over 1024 bytes... //

                    var size = stream.Read(chunk, 0, chunk.Length);

                    var packet = Encoding.UTF8.GetString(chunk, 0, size);

                    if (!client.Loaded)
                    {
                        client.Name = packet;
                        client.Loaded = true;

                        ClientSentName?.Invoke(client);
                    }

                    PacketReceived?.Invoke(client, packet); // Send an event that we've received a packet
                }
            }
            catch (IOException)
            {
                DisconnectClient(client);
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
                DisconnectClient(client.Value);
            }

            TcpListener.Stop(); // Stop the server.
        }


        /// <summary>
        /// Sends the specified string arrays to all of the clients.
        /// </summary>
        /// <param name="strs"></param>
        public void SendStringsGlobal(string[] strs)
        {
            /* 
                There's always a chance that the collection will
                be modified within the loop (because if they disconnect, 
                they removed from the dictionary). Due to this, we must
                turn the client dictionary into an array to modify the collection
                within the loop.
            */
            var clients = Clients.ToArray();
            foreach (var client in clients)
                SendStrings(client.Key, strs);
            
        }

        /// <summary>
        /// Sends the specified string to all clients.
        /// </summary>
        /// <param name="str">The string to send</param>
        public void SendStringGlobal(string str)
        {
            /* 
                There's always a chance that the collection will
                be modified within the loop (because if they disconnect, 
                they removed from the dictionary). Due to this, we must
                turn the client dictionary into an array to modify the collection
                within the loop.
            */
            var clients = Clients.ToArray(); 
            foreach (var client in clients)
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

            var client = Clients[uid];

            if (!client.Loaded) return;

            try
            {
                if (!CheckConnected(client)) return;

                var stream = client.TcpClient.GetStream();

                var bytes = Encoding.UTF8.GetBytes(str + "\n");

                stream.Write(bytes, 0, bytes.Length);
            }
            catch (IOException)
            {
                DisconnectClient(client);
            }
        }


        /// <summary>
        /// Sends an array of strings to the specified client. Will append a newline to each entry
        /// </summary>
        /// <param name="uid">The UID to send it to</param>
        /// <param name="strs">The array of strings</param>
        public void SendStrings(int uid, string[] strs)
        {
            if (!Clients.ContainsKey(uid)) return;

            var client = Clients[uid];

            if (!client.Loaded) return;

            try
            {
                if (!CheckConnected(client)) return;

                var stream = client.TcpClient.GetStream();

                foreach (var bytes in strs.Select(str => Encoding.UTF8.GetBytes(str + "\n")))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (IOException)
            {
                DisconnectClient(client);
            }
        }
        
        
        /// <summary>
        /// Will close socket if client has been disconnected.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>True if connected, false if not connected</returns>
        private bool CheckConnected(Client client)
        {
            if (client.TcpClient.Connected) return true;
            DisconnectClient(client);
            return false;
        }

        private void DisconnectClient(Client client)
        {
            if (client == null || client.Disconnected) return;
            
            try
            {
                client.TcpClient.Close();
            }
            catch
            {
                // Ignore errors //
            }

            lock (Clients)
            {
                Clients.Remove(client.Uid);
            }

            client.Disconnected = true;
            ClientLeft?.Invoke(client);
        }
    }
    

    /// <summary>
    /// A class wrapping different aspects of a client
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Gets or sets the TCP client.
        /// </summary>
        /// <value>
        /// The TCP client.
        /// </value>
        public TcpClient TcpClient { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the uid.
        /// </summary>
        /// <value>
        /// The uid.
        /// </value>
        public int Uid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Client" /> is loaded.
        /// </summary>
        /// <value>
        /// <c>True</c> if client loaded; otherwise, <c>false</c>.
        /// </value>
        public bool Loaded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Client" /> is disconnected.
        /// </summary>
        /// <value>
        /// <c>True</c> if disconnected; otherwise, <c>false</c>.
        /// </value>
        public bool Disconnected { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client" /> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="uid">The UID</param>
        public Client(TcpClient socket, int uid)
        {
            TcpClient = socket;
            Uid = uid;
        }
    }
}
