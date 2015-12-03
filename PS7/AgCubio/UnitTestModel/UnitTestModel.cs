using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;

namespace UnitTestModel
{
    [TestClass]
    public class UnitTest1
    {
    
        /// <summary>
        /// All fields of an empty Cube should be their respective defaults.
        /// </summary>
        [TestMethod]
        public void CubeEmptyTest1()
        {
            Cube cube = new Cube();
            Assert.AreEqual(0, cube.Bottom);
            Assert.AreEqual(0, cube.Height);
            Assert.AreEqual(false, cube.IsFood);
            Assert.AreEqual(0, cube.Left);
            Assert.AreEqual(0, cube.Mass);
            Assert.AreEqual(null, cube.Name);
            Assert.AreEqual(0, cube.Right);
            Assert.AreEqual(0, cube.TeamId);
            Assert.AreEqual(0, cube.Top);
            Assert.AreEqual(0, cube.Uid);
            Assert.AreEqual(0, cube.Width);
            Assert.AreEqual(0, cube.X);
            Assert.AreEqual(0, cube.Y);
            Assert.AreEqual(0, cube.Color.ToArgb());
            Assert.AreEqual(true, cube.IsDead);
        }

        /// <summary>
        /// Nonempty cube should have unique attributes.
        /// </summary>
        [TestMethod]
        public void CubeNonEmptyTest1()
        {
            String jsonString = "{ \"loc_x\":926.0,\"loc_y\":682.0,\"argb_color\":-65536," +
                "\"team_id\":2947,\"uid\":5571,\"food\":false,\"Name\":\"Cubish\",\"Mass\":1000.0}";
            Cube cube = Cube.FromJson(jsonString);

            Assert.AreEqual(false, cube.IsFood);
            Assert.AreEqual(1000.0, cube.Mass);
            Assert.AreEqual("Cubish", cube.Name);
            Assert.AreEqual(2947, cube.TeamId);
            Assert.AreEqual(5571, cube.Uid);
            Assert.AreEqual(926.0, cube.X);
            Assert.AreEqual(682, 0, cube.Y);
            Assert.AreEqual(-65536, cube.Color.ToArgb());
        }


        /// <summary>
        /// Testing the other attributes of a nonempty cube that are not directly derived
        /// from a JSON string.
        /// </summary>
        [TestMethod]
        public void CubeNonEmptyTest2()
        {
            String jsonString = "{ \"loc_x\":425.0,\"loc_y\":382.0,\"argb_color\":-1655836," +
                "\"team_id\":5536,\"uid\":1312,\"food\":false,\"Name\":\"Stewie\",\"Mass\":5600.0}";
            Cube cube = Cube.FromJson(jsonString);

            Assert.AreEqual((int)Math.Pow(cube.Mass, .65), cube.Height);
            Assert.AreEqual(382 - (cube.Width / 2f), cube.Top);
            Assert.AreEqual(425 - (cube.Height / 2f), cube.Left);
            Assert.AreEqual(425 + (cube.Height / 2f), cube.Right);
            Assert.AreEqual(382 + (cube.Width / 2f), cube.Bottom);
            Assert.AreEqual((int)Math.Pow(cube.Mass, .65), cube.Width);
            Assert.AreEqual(false, cube.IsDead);
        }

        /// <summary>
        /// An empty world should contain no food and no players.
        /// </summary>
        [TestMethod]
        public void WorldEmptyTest1()
        {
            World myWorld = new World();

            Assert.AreEqual(0, myWorld.FoodCount);
            Assert.AreEqual(0, myWorld.PlayersCount);
        }

        /// <summary>
        /// The "Count" of both Food and Players lists in world should go up when adding
        /// food and player cubes.
        /// </summary>
        [TestMethod]
        public void WorldAddCubeTest1()
        {
            World myWorld = new World();

            myWorld.AddFoodCube(Cube.FromJson("{\"food\":true, \"uid\": 1}"));
            myWorld.AddFoodCube(Cube.FromJson("{\"food\":true, \"uid\": 2}")); //adding two food cubes
            myWorld.AddPlayerCube(Cube.FromJson("{\"food\":false, \"uid\": 3}"));
            myWorld.AddPlayerCube(Cube.FromJson("{\"food\":false, \"uid\": 4}"));//adding two players

            Assert.AreEqual(2, myWorld.FoodCount);
            Assert.AreEqual(2, myWorld.PlayersCount);// both counts should be two
        }
        
        
        /// <summary>
        /// Make sure food cube is obtained when its uid is inputted.
        /// </summary>
        [TestMethod]
        public void WorldGetFoodCubeTest1()
        {
            World myWorld = new World();
            Cube[] cubes = new Cube[3];

            for (int i = 0; i < 3; i++)// adds foods with uid's 0, 1, and 2
            {
                cubes[i] = Cube.FromJson("{\"food\":true, \"uid\":" + i + "}");
                myWorld.AddFoodCube(cubes[i]);
            }

            Assert.AreEqual(null, myWorld.GetFoodCube(5));
            Assert.AreEqual(cubes[0], myWorld.GetFoodCube(0));
            Assert.AreEqual(cubes[1], myWorld.GetFoodCube(1));
            Assert.AreEqual(cubes[2], myWorld.GetFoodCube(2));
        }

        /// <summary>
        /// Make sure player cube is obtained when its uid is inputted.
        /// </summary>
        [TestMethod]
        public void WorldGetPlayerCubeTest1()
        {
            World myWorld = new World();
            Cube[] cubes = new Cube[3];

            for (int i = 0; i < 3; i++)// adds players with uid's 0, 1, and 2
            {
                cubes[i] = Cube.FromJson("{\"food\":false, \"uid\":" + i + "}");
                myWorld.AddPlayerCube(cubes[i]);
            }

            Assert.AreEqual(null, myWorld.GetPlayerCube(5));
            Assert.AreEqual(cubes[0], myWorld.GetPlayerCube(0));
            Assert.AreEqual(cubes[1], myWorld.GetPlayerCube(1));
            Assert.AreEqual(cubes[2], myWorld.GetPlayerCube(2));
        }

        /// <summary>
        /// Make sure Food cube is added if it didn't already exist.
        /// </summary>
        [TestMethod]
        public void WorldUpdateFoodCubeTest1()
        {
            World myWorld = new World();
            Cube cube = Cube.FromJson("{\"food\":true, \"mass\":20, \"uid\":0}");

            myWorld.UpdateFoodCube(cube);
            Assert.AreEqual(cube, myWorld.GetFoodCube(0));
            Assert.AreEqual(20, myWorld.GetFoodCube(0).Mass);
        }

        /// <summary>
        /// Make sure Food cube is removed if it is passed into UpdateFoodCube method
        /// </summary>
        [TestMethod]
        public void WorldUpdateFoodCubeTest2()
        {
            World myWorld = new World();
            Cube cube = Cube.FromJson("{\"food\":true, \"mass\":20, \"uid\":0}");

            myWorld.AddFoodCube(cube);
            myWorld.UpdateFoodCube(cube);
            Assert.AreEqual(null, myWorld.GetFoodCube(0));
        }

        /// <summary>
        /// Make sure Player cube is added if it didn't already exist.
        /// </summary>
        [TestMethod]
        public void WorldUpdatePlayerCubeTest1()
        {
            World myWorld = new World();
            Cube cube = Cube.FromJson("{\"food\":false, \"mass\":0, \"uid\":0}");

            myWorld.AddPlayerCube(cube);
            myWorld.UpdatePlayerCube(cube);
            Assert.AreEqual(null, myWorld.GetPlayerCube(0));
        }


        /// <summary>
        /// Make sure Player cube is removed if it is passed into UpdatePlayerCube method
        /// </summary>
        [TestMethod]
        public void WorldUpdatePlayerCubeTest2()
        {
            World myWorld = new World();
            Cube cube2 = Cube.FromJson("{\"food\":false, \"mass\":100, \"uid\":1}");

            myWorld.UpdatePlayerCube(cube2);
            Assert.AreEqual(cube2, myWorld.GetPlayerCube(1));
            Assert.AreEqual(100, myWorld.GetPlayerCube(1).Mass);

            cube2 = Cube.FromJson("{\"food\":false, \"mass\":1000, \"uid\":1}");
            myWorld.UpdatePlayerCube(cube2);
            Assert.AreEqual(cube2, myWorld.GetPlayerCube(1));
            Assert.AreEqual(1000, myWorld.GetPlayerCube(1).Mass);
        }

        /// <summary>
        /// Stress test with 5000 foods.
        /// </summary>
        [TestMethod]
        public void WorldStressTest1()
        {
            World myWorld = new World();

            for (int i = 0; i < 5000; i++)
            {
                myWorld.AddPlayerCube(new Cube {Uid = i});
                myWorld.AddFoodCube(Cube.FromJson("{\"food\":true,\"uid\":" + (5000 + i) + "}"));
            }

            Assert.AreEqual(5000, myWorld.FoodCount);
            Assert.AreEqual(5000, myWorld.PlayersCount);
        }

        /// <summary>
        /// Stress test with 10000 foods and players spread out over the petri dish.
        /// </summary>
        [TestMethod]
        public void WorldStressTest2()
        {
            World myWorld = new World();
            int uid = 0;
            int teamId = 0;

            for (int i = 0; i < 100; i++)// each row of cubes has the same teamId
            {
                for (int j = 0; j < 100; j++)
                {
                    string jsonString = "\"loc_x\":" + i + ",\"loc_y\":" + j + ",\"argb_color\":" +
                        i * j + ",\"team_id\":" + teamId + ",\"uid\":" + uid +
                        ",\"Name\":\"Stewie\",\"Mass\":" + (i + j + 1) + "}";


                    myWorld.AddPlayerCube(Cube.FromJson("{\"food\":false," + jsonString));
                    myWorld.AddFoodCube(Cube.FromJson("{\"food\":true," + jsonString));
                    Cube cube = myWorld.GetFoodCube(uid);

                    Assert.AreEqual(i, cube.X);
                    Assert.AreEqual(j, cube.Y);
                    Assert.AreEqual(i * j, cube.Color.ToArgb());
                    Assert.AreEqual(teamId, cube.TeamId);
                    Assert.AreEqual(uid, cube.Uid);
                    Assert.AreEqual(i + j + 1, cube.Mass);
                    Assert.AreEqual(false, cube.IsDead);
                    Assert.AreEqual(true, cube.IsFood);

                    cube = myWorld.GetPlayerCube(uid);
                    Assert.AreEqual(i, cube.X);
                    Assert.AreEqual(j, cube.Y);
                    Assert.AreEqual(i * j, cube.Color.ToArgb());
                    Assert.AreEqual(teamId, cube.TeamId);
                    Assert.AreEqual(uid, cube.Uid);
                    Assert.AreEqual(i + j + 1, cube.Mass);
                    Assert.AreEqual(false, cube.IsDead);
                    Assert.AreEqual(false, cube.IsFood);

                    uid++;
                }
                teamId++;
            }

            Assert.AreEqual(10000, myWorld.FoodCount);
            Assert.AreEqual(10000, myWorld.PlayersCount);
        }
    }
}
