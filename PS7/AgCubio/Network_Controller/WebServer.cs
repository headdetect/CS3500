using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Network_Controller
{
    public class WebServer
    {
        
        /// <summary>
        /// Called when a page is requested
        /// </summary>
        public static event Func<PageRequestEventArgs, string> PageRequested;
        
        /// <summary>
        /// Gets if the web server is running
        /// </summary>
        public bool Running { get; private set; }

        private TcpListener _listener;

        /// <summary>
        /// Starts the webserver (is a blocking method)
        /// </summary>
        /// <param name="port">Port to bind the server on</param>
        public void Start(int port)
        {
            _listener = TcpListener.Create(port);
            _listener.Start();

            Running = true;

            while (Running)
            {
                var socket = _listener.AcceptTcpClient();
                new Thread(() => RunRequest(socket)).Start();
            }
        }

        /// <summary>
        /// Starts the webserver (is a NON blocking method).
        /// Is not trueeee async.
        /// </summary>
        /// <param name="port">port to bind the server on</param>
        public void StartAsync(int port)
        {
            new Thread(() => Start(port)).Start();
        }

        private void RunRequest(TcpClient client)
        {
            var reader = new StreamReader(client.GetStream());
            var writer = new StreamWriter(client.GetStream());

            var request = reader.ReadToEnd();

            Console.WriteLine(request);

            var result = PageRequested?.Invoke(new PageRequestEventArgs(new Uri("https://google.com")));

            writer.Write(result);
        }

        /// <summary>
        /// Stops the web server.
        /// </summary>
        public void Stop()
        {
            Running = false;
            _listener.Stop();
        }
    }



    public class PageRequestEventArgs : EventArgs
    {
        /// <summary>
        /// The URI Sent by the client
        /// </summary>
        public Uri Uri { get; private set; }
        
        /// <summary>
        /// Constructor for building this event
        /// </summary>
        /// <param name="uri">The URI Recieved from the client</param>
        public PageRequestEventArgs(Uri uri)
        {
            Uri = uri;
        }
    }
}
