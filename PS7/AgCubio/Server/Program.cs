using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Model;
using ServerNetworkController;

namespace Server
{
    class Program
    {
        public static void Main(string[] args)
        {
            var n = new World();
            var random = new Random();

            for (var i = 0; i < 6000; i++)
            {
                n.AddCube(new Cube
                {
                    Color = Color.FromArgb(i % 255, i / 255, 20),
                    IsFood = true,
                    Mass = 1, 
                    Uid = i + 40, // UID's 0 - 40 belong to players
                    X = random.Next(World.Width),
                    Y = random.Next(World.Height)
                });
            }

            ServerNetwork.ClientLeft += ServerNetwork_ClientLeft;
            ServerNetwork.ClientJoined += ServerNetwork_ClientJoined;

            var server = new ServerNetwork();
            server.Listen();
        }

        private static void ServerNetwork_ClientJoined(int uid, TcpClient client)
        {
            Console.WriteLine($"Client Joined {uid}");
        }

        private static void ServerNetwork_ClientLeft(int obj)
        {
            Console.WriteLine($"Client Left {obj}");
        }
    }
}
