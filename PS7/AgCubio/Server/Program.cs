﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Model;
using ServerNetworkController;
using Timer = System.Timers.Timer;

namespace Server
{
    class Program
    {
        /// <summary>
        /// Gets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        public static ServerNetwork Server { get; private set; }

        /// <summary>
        /// Gets the world.
        /// </summary>
        /// <value>
        /// The world.
        /// </value>
        public static World World { get; private set; }

        /// <summary>
        /// Gets the new cubes.
        /// </summary>
        /// <value>
        /// The new cubes.
        /// </value>
        public static List<Cube> NewCubes { get; private set; } 

        public static void Main(string[] args)
        {
            World = new World();
            var random = new Random();
            NewCubes = new List<Cube>();

            for (var i = 0; i < 6000; i++)
            {
                // Add a bunch of food cubes //
                World.AddCube(new Cube
                {
                    Color = Color.FromArgb(i % 255, i / 255, 20),
                    IsFood = true,
                    Mass = 1, 
                    Uid = i + 40, // UID's 0 - 40 belong to players
                    X = random.Next(World.Width),
                    Y = random.Next(World.Height)
                });

                NewCubes.AddRange(World.Food.Values);
            }

            Console.WriteLine("Server Started...");

            ServerNetwork.ClientLeft += ServerNetwork_ClientLeft;
            ServerNetwork.ClientJoined += ServerNetwork_ClientJoined;
            ServerNetwork.ClientSentName += ServerNetwork_ClientSentName;

            var t = new Timer(1000d / Constants.HeartbeatsPerSecond);
            t.Elapsed += T_Elapsed;
            t.Start();

            Console.WriteLine("Listening for connections...");
            Server = new ServerNetwork();
            Server.Listen();
        }

        private static void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Update the state of the clients //

            NewCubes.Clear();

            //TODO: Add new cubes at random 

            lock (World)
            {
                // Force size to prevent reallocation //
                var blobs = new List<string>(NewCubes.Count + World.Players.Count);

                blobs.AddRange(NewCubes.Select(food => food.ToJson()));
                blobs.AddRange(World.Players.Select(player => player.Value.ToJson()));

                // To prevent multiple enumerations, we keep it an array //
                Server.SendStringsGlobal(blobs.ToArray());
            }
            
        }

        private static void ServerNetwork_ClientSentName(Client client)
        {
            var cube = new Cube
            {
                Color = Color.Blue,
                IsFood = false,
                Mass = 400,
                Name = client.Name,
                TeamId = 0,
                Uid = client.Uid,
                X = 0,
                Y = 0
            };

            // Add player cube //
            World.AddCube(cube);

            Server.SendString(client.Uid, cube.ToJson());

            // Send all the food and player cubes //

            lock (World)
            {
                // Force size to prevent reallocation //
                var blobs = new List<string>(World.Food.Count + World.Players.Count);

                blobs.AddRange(World.Food.Select(food => food.Value.ToJson()));
                blobs.AddRange(World.Players.Select(player => player.Value.ToJson()));

                // To prevent multiple enumerations, we keep it an array //
                Server.SendStrings(client.Uid, blobs.ToArray());
            }

            Console.WriteLine($"Sending cube info to: {client.Name}");
        }

        private static void ServerNetwork_ClientJoined(Client client)
        {
            Console.WriteLine($"Client Joined ({client.Uid})");
        }

        private static void ServerNetwork_ClientLeft(Client client)
        {
            Console.WriteLine($"Client Left ({client.Uid} {client.Name})");
        }
    }
}
