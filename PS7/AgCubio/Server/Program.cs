using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Model;
using System.Text.RegularExpressions;
using Network_Controller;

namespace Server
{
    class Program
    {
        /// <summary>
        /// Gets the world constants.
        /// </summary>
        /// <value>
        /// The constants.
        /// </value>
        public static Constants Constants { get; private set; }

        /// <summary>
        /// Gets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        public static ServerNetworkManager Server { get; private set; }

        /// <summary>
        /// Gets the webserver.
        /// </summary>
        public static WebServer WebServer { get; private set; }

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
        public static Dictionary<int, Team> Teams { get; private set; }

        /// <summary>
        /// Gets the random generator.
        /// </summary>
        /// <value>
        /// The random generator.
        /// </value>
        public static Random Random { get; private set; }

        public static void Main(string[] args)
        {
            Constants = new Constants("world_parameters.xml");
            World = new World();
            NewCubes = new List<Cube>();
            Teams = new Dictionary<int, Team>();
            Random = new Random();

            // Very local function to generate colors //

            for (var i = 0; i < Constants.MaxFood; i++)
            {
                // Add a bunch of food cubes //
                var foodCube = new Cube
                {
                    Color = MakeColorFood(i),
                    IsFood = true,
                    Mass = 1,
                    Uid = 10 + i, // Food Cubes get UID's 10 - Constants.MaxFood
                    X = Random.Next(World.Width),
                    Y = Random.Next(World.Height)
                };

                World.AddFoodCube(foodCube);
                NewCubes.Add(foodCube);
            }

            for (var i = 0; i < 10; i++)
            {
                var foodCube = new Cube
                {
                    Color = Color.YellowGreen,
                    IsFood = true,
                    Mass = Constants.FoodValue * 15,
                    Uid = i, // Viruses get UID's 0 - 9
                    X = Random.Next(World.Width / 2) + World.Width / 4, // Places viruses near center of map.
                    Y = Random.Next(World.Height / 2) + World.Height / 4
                };

                World.AddFoodCube(foodCube);
                NewCubes.Add(foodCube);
            }

            Console.WriteLine("Server Started...");

            ServerNetworkManager.ClientLeft += ServerNetwork_ClientLeft;
            ServerNetworkManager.ClientJoined += ServerNetwork_ClientJoined;
            ServerNetworkManager.ClientSentName += ServerNetwork_ClientSentName;
            ServerNetworkManager.PacketReceived += ServerNetwork_PacketReceived;
            ServerNetworkManager.RequestUid += GetNextPlayerUid;

            WebServer.PageRequested += WebServer_PageRequested;

            Console.WriteLine("Listening for connections...");
            Server = new ServerNetworkManager(Constants.Port);
            WebServer = new WebServer();

            ThreadPool.QueueUserWorkItem(UpdateState); // Start the crazy loop

            WebServer.StartAsync(Constants.WebPort);

            try
            {
                Server.Listen();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                Console.Read();
            }
        }

        private static string WebServer_PageRequested(PageRequestEventArgs arg)
        {
            return "<h1>TODO: Implement this</h1>";
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

                    var newCube = (Cube)player.Clone();
                    newCube.Uid = GetNextPlayerUid();

                    teamId = GenerateTeamId();

                    player.TeamId = teamId;
                    newCube.TeamId = teamId;

                    newCube.X += Random.Next(-Constants.MaxSplitDistance, Constants.MaxSplitDistance);
                    newCube.Y += Random.Next(-Constants.MaxSplitDistance, Constants.MaxSplitDistance);

                    World.AddPlayerCube(newCube);

                    var cubes = new[] { player, newCube };

                    Teams.Add(teamId, new Team(teamId, cubes, DateTime.Now + TimeSpan.FromSeconds(7)));
                }
                else
                {
                    // We've already split before //

                    // Will get all teammates (including self) //
                    var team = Teams[teamId];

                    if (team.Cubes.Count > Constants.MaxNumOfSplit) return;

                    foreach (var member in team.Cubes.ToArray())
                    {
                        member.Mass /= 2;

                        var newCube = (Cube)player.Clone();
                        newCube.Uid = GetNextPlayerUid();

                        newCube.X += Random.Next(-Constants.MaxSplitDistance, Constants.MaxSplitDistance);
                        newCube.Y += Random.Next(-Constants.MaxSplitDistance, Constants.MaxSplitDistance);

                        World.AddPlayerCube(newCube);

                        team.Cubes.Add(newCube);
                    }

                    team.KeepAlive = DateTime.Now + TimeSpan.FromSeconds(7); // 7 seconds from now //
                }
            }

            Server.SendStringGlobal(player.ToJson());
        }


        private static void UpdateState(object state)
        {
            state = state ?? new object();

            while (true)
            {
                lock (state)
                {
                    // Random 1 in 3 chances to make a food //
                    if (World.GetFoodCubes().Count(b => !b.IsDead) < Constants.MaxFood && Random.Next(0, 3) % 3 == 0)
                    {
                        var newFoodCube = new Cube
                        {
                            Color = MakeColorFood(Random.Next()),
                            IsFood = true,
                            Mass = 1,
                            Uid = GetNextFoodUid(), // Food Cubes get UID's 10 - Constants.MaxFood
                            X = Random.Next(World.Width),
                            Y = Random.Next(World.Height)
                        };

                        World.RemoveFoodCube(newFoodCube.Uid); // Remove any that might be there //
                        World.AddFoodCube(newFoodCube);

                        lock (NewCubes)
                        {
                            NewCubes.Add(newFoodCube);
                        }
                    }

                    // O(n * f + n ^ n), where n = player.length, f = food.length or O(fn^n) //
                    var players = World.GetPlayerCubes().Where(cube => !cube.IsDead).ToArray();
                    foreach (var player in players)
                    {
                        var playerRect = player.AsRectangle;

                        // If the delta movement is smaller than this, just snap to location //
                        const float snapDistance = 3f;

                        // evaluate speed with respect to player's mass //
                        var speed = MathUtils.Clamp(Constants.TopSpeed - (float)(player.Mass / 500f), Constants.LowSpeed,
                            Constants.TopSpeed);

                        // Smooth Movement //
                        if (player.X > player.TargetX)
                            player.X = player.X - player.TargetX < snapDistance
                                ? // If the delta distance is less
                                player.TargetX
                                : // Snap to location
                                player.X - speed; // Interpolate to location

                        if (player.X < player.TargetX)
                            player.X = player.TargetX - player.X < snapDistance
                                ? // If the delta distance is less
                                player.TargetX
                                : // Snap to location
                                player.X + speed; // Interpolate to location

                        if (player.Y > player.TargetY)
                            player.Y = player.Y - player.TargetY < snapDistance
                                ? // If the delta distance is less
                                player.TargetY
                                : // Snap to location
                                player.Y - speed; // Interpolate to location

                        if (player.Y < player.TargetY)
                            player.Y = player.TargetY - player.Y < snapDistance
                                ? // If the delta distance is less
                                player.TargetY
                                : // Snap to location
                                player.Y + speed; // Interpolate to location

                        // Clamp them to the bounds of the map //
                        player.X = MathUtils.Clamp(player.X, (player.Width / 2), Constants.Width - (player.Width / 2));
                        player.Y = MathUtils.Clamp(player.Y, (player.Height / 2), Constants.Height - (player.Height / 2));

                        // Apply decay //
                        player.Mass -= Math.Sqrt(player.Mass) / Constants.AttritionRate;

                        foreach (
                            var food in
                                from food in World.GetFoodCubes()
                                let foodRect = food.AsRectangle
                                where foodRect.IntersectsWith(playerRect)
                                select food)
                        {
                            if (food.Uid < 10) // checks if food is a virus.
                            {
                                if (food.Mass > player.Mass / 2) break;

                                player.Mass *= .8; // cube loses a fifth of their mass

                                var teamId = player.TeamId;

                                if (teamId == 0)
                                {
                                    // This is the first time splitting //

                                    player.Mass /= 2;

                                    var newCube = (Cube)player.Clone();
                                    newCube.Uid = GetNextPlayerUid();

                                    teamId = GenerateTeamId();

                                    player.TeamId = teamId;
                                    newCube.TeamId = teamId;

                                    newCube.X += Random.Next(-Constants.MaxSplitDistance, Constants.MaxSplitDistance);
                                    newCube.Y += Random.Next(-Constants.MaxSplitDistance, Constants.MaxSplitDistance);

                                    World.AddPlayerCube(newCube);

                                    var cubes = new[] { player, newCube };

                                    Teams.Add(teamId, new Team(teamId, cubes, DateTime.Now + TimeSpan.FromSeconds(7)));
                                }
                                else
                                {
                                    // We've already split before //

                                    // Will get all teammates (including self) //
                                    var team = Teams[teamId];

                                    player.Mass /= 2;

                                    var newCube = (Cube)player.Clone();
                                    newCube.Uid = GetNextPlayerUid();

                                    newCube.X += Random.Next(-Constants.MaxSplitDistance, Constants.MaxSplitDistance);
                                    newCube.Y += Random.Next(-Constants.MaxSplitDistance, Constants.MaxSplitDistance);

                                    World.AddPlayerCube(newCube);

                                    team.Cubes.Add(newCube);

                                    team.KeepAlive = DateTime.Now + TimeSpan.FromSeconds(7); // 7 seconds from now //
                                }

                                Server.SendStringGlobal(player.ToJson());

                            }
                            else
                            {
                                player.Mass += food.Mass;
                            }

                            food.Mass = 0;

                            Server.SendStringGlobal(food.ToJson());
                        }

                        // Don't worry, chaining "where's" doesn't execute until you evaluate the enumeration. This function is not O(3n) //
                        foreach (
                            var otherPlayer in
                                players.Where(otherPlayer => otherPlayer.Uid != player.Uid)
                                    .Where(otherPlayer => otherPlayer.AsRectangle.IntersectsWith(playerRect))
                                    .Where(otherPlayer => !otherPlayer.IsDead))
                        {
                            if (otherPlayer.Mass >= player.Mass * Constants.AbsorbConstant &&
                                !otherPlayer.IsOnTeam(player) && false)
                            {
                                // We ded yo //
                                otherPlayer.Mass += player.Mass;
                                player.Mass = 0;
                            }

                            if (player.Mass >= otherPlayer.Mass * Constants.AbsorbConstant &&
                                !otherPlayer.IsOnTeam(player) && false)
                            {
                                // They ded yo //
                                player.Mass += otherPlayer.Mass;
                                otherPlayer.Mass = 0;
                            }

                            // Handle collisions here //

                            // The are coming in from the left, push them back left //
                            if (otherPlayer.Right > player.Left && otherPlayer.Right <= player.Right)
                                otherPlayer.X -= Math.Min(otherPlayer.Right - player.Left, 5f); 

                            // The are coming in from the right, push them back right //
                            else if (otherPlayer.Left > player.Right && otherPlayer.Left <= player.Left)
                                otherPlayer.X += Math.Min(player.Left - otherPlayer.Right, 5f);

                            // The are coming in from the top, push them back up //
                            else if (otherPlayer.Bottom > player.Top && otherPlayer.Bottom <= player.Bottom)
                                otherPlayer.Y += Math.Min(otherPlayer.Bottom - player.Top, 5f);

                            // The are coming in from the right, push them back right //
                            else if (otherPlayer.Top > player.Bottom && otherPlayer.Top <= player.Top)
                                otherPlayer.Y -= Math.Min(player.Top - otherPlayer.Bottom, 5f);

                            // We'll send the other player's data if they die //
                            Server.SendStringGlobal(otherPlayer.ToJson());
                        }
                    }

                    var teams = Teams.Select(team => team.Value).Where(team => team.KeepAlive < DateTime.Now).ToArray();
                    foreach (var team in teams)
                    {
                        // Time to re-merge //

                        var playerToMergeTo = team.Cubes[0];

                        foreach (var otherCubes in team.Cubes.Skip(1))
                        {
                            playerToMergeTo.Mass += otherCubes.Mass;
                            otherCubes.Mass = 0;
                            otherCubes.TeamId = 0;
                        }

                        playerToMergeTo.TeamId = 0;
                        Teams.Remove(team.TeamID);
                    }

                    Server.SendStringsGlobal(World.GetPlayerCubes().Select(cube => cube.ToJson()).ToArray());
                    Server.SendStringsGlobal(NewCubes.Select(cube => cube.ToJson()).ToArray());

                    lock (NewCubes)
                    {
                        NewCubes.Clear();
                    }

                    Thread.Sleep(1000 / Constants.HeartbeatsPerSecond);
                }
            }
        }

        private static void ServerNetwork_ClientSentName(Client client)
        {
            var width = (int)Math.Pow(Constants.PlayerStartMass, 0.65);

            var cube = new Cube
            {
                Color = MakeColorPlayer(Random.Next()),
                IsFood = false,
                Mass = Constants.PlayerStartMass,
                Name = client.Name,
                TeamId = 0,
                Uid = client.Uid,
                X = Random.Next(1001 - width) + (width / 2),
                Y = Random.Next(1001 - width) + (width / 2)
            };

            // Add player cube //
            World.AddPlayerCube(cube);

            Server.SendStringGlobal(cube.ToJson());

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

            // Stop moving //
            clientCube.TargetX = clientCube.X;
            clientCube.TargetY = clientCube.Y;
        }

        /// <summary>
        /// Runs a function after the specified amount of time
        /// </summary>
        /// <param name="msDelay">The delay time in milliseconds.</param>
        /// <param name="function">The function.</param>
        public static void DelayFunction(int msDelay, Action function)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(msDelay);
                function();
            });
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

            // If we can't get empty, get slot of killed block //

            for (var i = 10; i < Constants.MaxFood + 10; i++)
            {
                if (World.GetFoodCube(i)?.IsDead ?? true)
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
            // Try get empty first //
            for (var i = Constants.MaxFood + 10; i < int.MaxValue; i++)
            {
                if (!World.PlayerCubeExists(i))
                    return i;
            }

            // If we can't get empty, get slot of killed block //

            for (var i = Constants.MaxFood + 10; i < int.MaxValue; i++)
            {
                if (World.GetPlayerCube(i)?.IsDead ?? true)
                    return i;
            }

            return -1; // Should never hit this //
        }

        private static int GenerateTeamId()
        {
            // Start it at 1, iterate "forever" //
            for (var i = 1; i < int.MaxValue; i++)
            {
                if (!Teams.ContainsKey(i))
                    return i;
            }
            return -1; // Should never hit this point //
        }

        /// <summary>
        /// Makes a color from the specified index, using the food's color scheme.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static Color MakeColorFood(int index)
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

        /// <summary>
        /// Makes a color from the specified index, using the player's color scheme.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static Color MakeColorPlayer(int index)
        {
            switch (index % 5)
            {
                case 0:
                    return Color.Red;
                case 1:
                    return Color.Black;
                case 2:
                    return Color.Blue;
                case 3:
                    return Color.Orange;
                default:
                    return Color.Green;
            }
        }

        /// <summary>
        /// Represents a team of cubes
        /// </summary>
        public class Team
        {
            /// <summary>
            /// Gets the team identifier.
            /// </summary>
            /// <value>
            /// The team identifier.
            /// </value>
            public int TeamID { get; }

            /// <summary>
            /// Gets the cubes on this team.
            /// </summary>
            /// <value>
            /// The cubes.
            /// </value>
            public List<Cube> Cubes { get; }

            /// <summary>
            /// Gets or sets the time to keep alive this team.
            /// </summary>
            /// <value>
            /// The keep alive.
            /// </value>
            public DateTime KeepAlive { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Team" /> class.
            /// </summary>
            /// <param name="teamId"></param>
            /// <param name="cubes">The cubes.</param>
            /// <param name="keepAlive">The keep alive time span.</param>
            public Team(int teamId, IEnumerable<Cube> cubes, DateTime keepAlive)
            {
                TeamID = teamId;
                Cubes = new List<Cube>(cubes);
                KeepAlive = keepAlive;
            }

        }
    }
}
