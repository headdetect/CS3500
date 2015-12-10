using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Network_Controller.Properties;
using MySql.Data.MySqlClient;

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
            var stream = client.GetStream();
            
            var requestBuilder = new StringBuilder();

            int read = 0;
            byte[] chunk = new byte[1024];
            do
            {
                read = stream.Read(chunk, 0, 1024);
                requestBuilder.Append(Encoding.ASCII.GetString(chunk));

            } while (read == 1024);

            var request = requestBuilder.ToString();
            
            try {
                var uri = new Uri($"http://{request.Split('\n')[1].Substring("Host: ".Length)}{request.Split('\n')[0].Split(' ')[1]}".Replace("\r", string.Empty));
                
                var result = PageRequested?.Invoke(new PageRequestEventArgs(uri));

                if (result != null)
                {
                    var bytes = BuildOk(result);
                    stream.Write(bytes, 0, bytes.Length);
                }
                else
                {
                    var bytes = Build404();
                    stream.Write(bytes, 0, bytes.Length);
                }
                
            }
            catch(Exception e)
            {
                var bytes = BuildError(e);
                stream.Write(bytes, 0, bytes.Length);
            }
            
            try
            {
                client.Close();
            }
            catch
            {
                // We don't give AF //
            }
        }

        private static byte[] BuildOk(string content)
        {
            const string rn = "\r\n";
            var result = $"HTTP/1.1 200 OK{rn}Connection: close{rn}Content-Type: text/html; charset=UTF-8{rn}Content-Length: {content.Length}{rn}{rn}{content}";
            return Encoding.ASCII.GetBytes(result);
        }

        private static byte[] BuildError(Exception e)
        {
            const string rn = "\r\n";
            var content = string.Format(Resources._500, e.Message, e.StackTrace);
            var result = $"HTTP/1.1 500 {e.Message}{rn}Connection: close{rn}Content-Type: text/html; charset=UTF-8{rn}Content-Length: {content.Length}{rn}{rn}{content}";
            return Encoding.ASCII.GetBytes(result);
        }

        private static byte[] Build404()
        {
            const string rn = "\r\n";
            var content = Resources._404_html;
            var result = $"HTTP/1.1 404 Not Found Fam{rn}Connection: close{rn}Content-Type: text/html; charset=UTF-8{rn}Content-Length: {content.Length}{rn}{rn}{content}";
            return Encoding.ASCII.GetBytes(result);
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
