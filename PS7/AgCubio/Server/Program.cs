using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Model;
using Timer = System.Timers.Timer;
using System.Text.RegularExpressions;
using Network_Controller;

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
        public static ServerNetworkManager Server { get; private set; }

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

        /// <summary>
        /// Keeps track of how many teams there are.
        /// </summary>
        public static List<int> Teams { get; private set; }

        /// <summary>
        /// Gets the random generator.
        /// </summary>
        /// <value>
        /// The random generator.
        /// </value>
        public static Random Random { get; private set; }

        public static void Main(string[] args)
        {
            World = new World();
            NewCubes = new List<Cube>();
            Teams = new List<int>();
            Random = new Random();

            // Very local function to generate colors //
            
            for (var i = 0; i < Constants.MaxFood; i++)
            {
                // Add a bunch of food cubes //
                World.AddFoodCube(new Cube
                {
                    Color = MakeColor(i),
                    IsFood = true,
                    Mass = 1,
                    Uid = 10 + i, // Food Cubes get UID's 10 - Constants.MaxFood
                    X = Random.Next(World.Width),
                    Y = Random.Next(World.Height)
                });

                NewCubes.AddRange(World.GetFoodCubes());
            }

            Console.WriteLine("Server Started...");

            ServerNetworkManager.ClientLeft += ServerNetwork_ClientLeft;
            ServerNetworkManager.ClientJoined += ServerNetwork_ClientJoined;
            ServerNetworkManager.ClientSentName += ServerNetwork_ClientSentName;
            ServerNetworkManager.PacketReceived += ServerNetwork_PacketReceived;
            ServerNetworkManager.RequestUID += GetNextPlayerUid;

            var t = new Timer(1000d / Constants.HeartbeatsPerSecond);
            t.Elapsed += T_Elapsed;
            t.Start();

            Console.WriteLine("Listening for connections...");
            Server = new ServerNetworkManager(Constants.Port, Constants.MaxFood);

            try
            {
                Server.Listen();
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e);
                Console.Read();
            }
        }

        private static void ServerNetwork_PacketReceived(Client client, string packet)
        {
                if (!packet.StartsWith("(")) return; // We don't accept anything not "(something, something, something)" //

                var regex = new Regex(@"[+-]?(\d*[.])?\d+");
                var stringX = regex.Matches(packet)[0].Value;
                var stringY = regex.Matches(packet)[1].Value;

                var x = int.Parse(stringX);
                var y = int.Parse(stringY);

                if (!World.PlayerCubeExists(client.Uid)) return; // Should never happen //
                var player = World.GetPlayerCube(client.Uid);

                if (packet.StartsWith("(move"))
                {
                    // Is the move command //
                    
                    if (player.X == x && player.Y == y) return; // Ignore updating, they are the same //
                    
                    player.TargetX = MathUtils.Clamp(x, 0, Constants.Width);
                    player.TargetY = MathUtils.Clamp(y, 0, Constants.Height);

                    Console.WriteLine($"X => {player.TargetX}\nY => {player.TargetY}\n");

                    // Update the other cubes on our team //
                    var teammates = World.GetPlayerCubes().Where(cube => cube.TeamId != 0 && cube.TeamId == player.TeamId).ToArray();

                    foreach (var member in teammates)
                    {
                        member.TargetX = x;
                        member.TargetY = y;
                    }
                }

                if (packet.StartsWith("(split"))
                {
                    // Is the split command //
                    var teamId = player.TeamId;

                    if (player.Mass < Constants.MinSplitMass) return;

                    if (teamId == 0)
                    {
                        // This is the first time splitting //

                        player.Mass /= 2;

                        var newCube = (Cube) player.Clone();
                        newCube.Uid = GetNextPlayerUid();

                        teamId = GenerateTeamId();
                        player.TeamId = teamId;
                        newCube.TeamId = teamId;

                        World.AddPlayerCube(newCube);

                        //TODO: calculate new location

                        DelayFunction(10 * 1000, () =>
                        {
                            // Restore everything after 10 seconds //
                            player.Mass += newCube.Mass;
                            newCube.Mass = 0; // Remove cube

                            player.TeamId = 0;
                            
                            World.RemovePlayerCube(newCube.Uid);
                        });
                    }
                    else
                    {
                        // We've already split before //

                        // Will get all teammates (including self) //
                        var teammates = World.GetPlayerCubes().Where(cube => cube.TeamId == player.TeamId).ToArray();

                        foreach (var member in teammates)
                        {
                            member.Mass /= 2;

                            var newCube = (Cube)player.Clone();
                            newCube.Uid = GetNextPlayerUid();

                            World.AddPlayerCube(newCube);

                            //TODO: calculate new location

                            DelayFunction(10 * 1000, () =>
                            {
                                // Restore everything after 10 seconds //
                                member.Mass += newCube.Mass;
                                newCube.Mass = 0; // Remove cube

                                member.TeamId = 0;
                                
                                World.RemovePlayerCube(newCube.Uid);
                            });
                        }
                    }
                }

                Server.SendStringGlobal(player.ToJson());
        }
        

        private static void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Update the state of the clients //

            lock (NewCubes)
            {
                NewCubes.Clear();
            }
            
            // Random 1 in 3 chances to make a food //
            if (World.FoodCount < Constants.MaxFood && Random.Next(0, 3) % 3 == 0)
            {
                World.AddFoodCube(new Cube
                {
                    Color = MakeColor(Random.Next()),
                    IsFood = true,
                    Mass = 1,
                    Uid = GetNextFoodUid(), // Food Cubes get UID's 10 - Constants.MaxFood
                    X = Random.Next(World.Width),
                    Y = Random.Next(World.Height)
                });

                NewCubes.AddRange(World.GetFoodCubes());
            }

            // O(n * f + n ^ n), where n = player.length, f = food.length or O(fn^n) //
            var players = World.GetPlayerCubes().ToArray();
            foreach (var player in players)
            {
                var playerRect = player.AsRectangle;

                // If the delta movement is smaller than this, just snap to location //
                const float snapDistance = 3f;

                // evaluate speed with respect to player's mass //
                var speed = MathUtils.Clamp(
                    Constants.TopSpeed - (float) (player.Mass / 500f),
                    Constants.LowSpeed,
                    Constants.TopSpeed
                );
                
                // Smooth Movement //
                if (player.X > player.TargetX)
                    player.X = 
                        player.X - player.TargetX < snapDistance ? // If the delta distance is less
                            player.TargetX :                       // Snap to location
                            player.X - speed;                      // Interpolate to location

                if (player.X < player.TargetX)
                    player.X =
                        player.TargetX - player.X < snapDistance ?  // If the delta distance is less
                            player.TargetX :                        // Snap to location
                            player.X + speed;                       // Interpolate to location

                if (player.Y > player.TargetY)
                    player.Y =
                        player.Y - player.TargetY < snapDistance ? // If the delta distance is less
                            player.TargetY :                       // Snap to location
                            player.Y - speed;                      // Interpolate to location

                if (player.Y < player.TargetY)
                    player.Y =
                        player.TargetY - player.Y < snapDistance ? // If the delta distance is less
                            player.TargetY :                       // Snap to location
                            player.Y + speed;                      // Interpolate to location

                // Clamp them to the bounds of the map //
                player.X = MathUtils.Clamp(player.X, (player.Width / 2), Constants.Width - (player.Width / 2));
                player.Y = MathUtils.Clamp(player.Y, (player.Height / 2), Constants.Height - (player.Height / 2));

                // Apply decay //
                player.Mass -= Math.Sqrt(player.Mass) / Constants.AttritionRate;
                    
                foreach (var food in from food in World.GetFoodCubes()
                    let foodRect = food.AsRectangle
                    where foodRect.IntersectsWith(playerRect)
                    select food)
                {
                    player.Mass += food.Mass;
                    food.Mass = 0;

                    Server.SendStringGlobal(food.ToJson());
                }
                    
                // Don't worry, chaining "where's" doesn't execute until you evaluate the enumeration. This function is not O(3n) //
                foreach (var otherPlayer in players
                    .Where(otherPlayer => otherPlayer.Uid != player.Uid)
                    .Where(otherPlayer => otherPlayer.AsRectangle.IntersectsWith(playerRect)))
                {
                    if (otherPlayer.Mass >= player.Mass * Constants.AbsorbConstant && !otherPlayer.IsOnTeam(player))
                    {
                        // We ded yo //
                        otherPlayer.Mass += player.Mass;
                        player.Mass = 0;
                    }

                    if (player.Mass >= otherPlayer.Mass * Constants.AbsorbConstant && !otherPlayer.IsOnTeam(player))
                    {
                        // They ded yo //
                        player.Mass += otherPlayer.Mass;
                        otherPlayer.Mass = 0;
                    }
        
                    // Handle collisions here //

                    // They are left of center //
                    if (otherPlayer.Left < player.Right)
                        otherPlayer.X = player.Left + 5; // 5 units of padding //

                    // The are right of center
                    //if (otherPlayer.Right > player.Left)
                    //    otherPlayer.TargetX -= otherPlayer.Right - player.Left;

                    if (otherPlayer.Top < player.Bottom)
                        otherPlayer.TargetY += player.Bottom - otherPlayer.Top;
                    //if (otherPlayer.Bottom > player.Top)
                    //    otherPlayer.TargetY -= otherPlayer.Bottom - player.Top;


                    // We'll send the other player's data if they die //
                    Server.SendStringGlobal(otherPlayer.ToJson());
                }

                Server.SendStringGlobal(player.ToJson());
            }
        }

        private static void ServerNetwork_ClientSentName(Client client)
        {
            //TODO: Make sure random location does not intersect with players or viruses
            int width = (int)Math.Pow(Constants.PlayerStartMass, 0.65);

            var cube = new Cube
            {
                Color = Color.Blue,
                IsFood = false,
                Mass = Constants.PlayerStartMass,
                Name = client.Name,
                TeamId = 0,
                Uid = client.Uid,
                X = Random.Next(1001 - width) + ( width / 2),
                Y = Random.Next(1001 - width) + (width / 2)
            };
            
            // Add player cube //
            World.AddPlayerCube(cube);
            

            Server.SendString(client.Uid, cube.ToJson());

            // Send all the food and player cubes //
            
            // Force size to prevent reallocation //
            var blobs = new List<string>(World.FoodCount + World.PlayersCount);

            blobs.AddRange(World.GetFoodCubes().Select(food => food.ToJson()));
            blobs.AddRange(World.GetPlayerCubes().Select(player => player.ToJson()));

            // To prevent multiple enumerations, we keep it an array //
            Server.SendStrings(client.Uid, blobs.ToArray());
            

            Console.WriteLine($"Sending cube info to: {client.Name}");
        }

        private static void ServerNetwork_ClientJoined(Client client)
        {
            Console.WriteLine($"Client Joined ({client.Uid}) ({client.TcpClient.Client.LocalEndPoint})");
        }

        private static void ServerNetwork_ClientLeft(Client client)
        {
            Console.WriteLine($"Client Left ({client.Uid} {client.Name})");

            var clientCube = World.GetPlayerCube(client.Uid);

            if (clientCube == null) return;

            clientCube.Color = Color.Gray;
            clientCube.Name += " (Dead)";
        }

        /// <summary>
        /// Runs a function after the specified amount of time
        /// </summary>
        /// <param name="msDelay">The delay time in milliseconds.</param>
        /// <param name="function">The function.</param>
        public static Timer DelayFunction(double msDelay, Action function)
        {
            var firstRun = true;
            var timer = new Timer(msDelay);
            timer.Elapsed += (sender, args) =>
            {
                if (firstRun)
                {
                    firstRun = false;
                    return;
                }

                function();
                timer.Stop();
            };
            timer.Start();

            return timer;
        }

        /// <summary>
        /// Gets the next food uid.
        /// </summary>
        /// <returns></returns>
        public static int GetNextFoodUid()
        {
            for (var i = 10; i < Constants.MaxFood + 10; i++)
            {
                if (!World.FoodCubeExists(i))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Finds the next uid.
        /// </summary>
        /// <returns>The best match for a new UID</returns>
        private static int GetNextPlayerUid()
        {
            for (var i = Constants.MaxFood + 10; i < int.MaxValue; i++)
            {
                if (!World.PlayerCubeExists(i))
                    return i;
            }
            return -1; // Should never hit this //
        }

        private static int GenerateTeamId()
        {
            // Start it at 1, iterate "forever" //
            for (var i = 1; i < int.MaxValue; i++)
            {
                if (!Teams.Contains(i))
                    return i;
            }
            return -1; // Should never hit this point //
        }

        public static Color MakeColor(int index)
        {
            switch (index % 5)
            {
                case 0:
                    return Color.Maroon;
                case 1:
                    return Color.DarkGray;
                case 2:
                    return Color.DarkBlue;
                case 3:
                    return Color.DarkGoldenrod;
                default:
                    return Color.DarkOliveGreen;
            }
        }
    }
}
