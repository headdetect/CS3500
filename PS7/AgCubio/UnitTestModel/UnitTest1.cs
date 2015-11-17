using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;

namespace UnitTestModel
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// Every field of an empty Cube should be their respective defaults.
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
        }

        /// <summary>
        /// Nonempty cube should have unique attributes
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
        /// Testing the other attributes of a nonempty cube
        /// </summary>
        [TestMethod]
        public void CubeNonEmptyTest2()
        {
            String jsonString = "{ \"loc_x\":425.0,\"loc_y\":382.0,\"argb_color\":-1655836," +
                "\"team_id\":5536,\"uid\":1312,\"food\":false,\"Name\":\"Stewie\",\"Mass\":5600.0}";
            Cube cube = Cube.FromJson(jsonString);

            Assert.AreEqual((int) Math.Pow(cube.Mass, .65), cube.Height);
            Assert.AreEqual(cube.Y - (cube.Width / 2f), cube.Top);
            Assert.AreEqual(cube.X - (cube.Height / 2f), cube.Left);
            Assert.AreEqual(cube.X + (cube.Height / 2f), cube.Right);
            Assert.AreEqual(cube.Y + (cube.Width / 2f), cube.Bottom);
            Assert.AreEqual(cube.Height, cube.Width);
        }

        [TestMethod]
        public void WorldEmptyTest1()
        {
            World myWorld = new World();

            Assert.AreEqual(0, myWorld.Food.Count);
            Assert.AreEqual(0, myWorld.Players.Count);
        }

        [TestMethod]
        public void WorldAddCubeTest1()
        {
            World myWorld = new World();

            myWorld.AddCube(Cube.FromJson("{\"food\":true}"));
            myWorld.AddCube(Cube.FromJson("{\"food\":true}"));
            myWorld.AddCube(Cube.FromJson("{\"food\":false}"));
            myWorld.AddCube(Cube.FromJson("{\"food\":false}"));

            Assert.AreEqual(2, myWorld.Food.Count);
            Assert.AreEqual(2, myWorld.Players.Count);
        }

        [TestMethod]
        public void WorldGetFoodCubeIndexTest1()
        {
            World myWorld = new World();


            myWorld.AddCube(Cube.FromJson("{\"food\":true, \"uid\":45}"));
            myWorld.AddCube(Cube.FromJson("{\"food\":true, \"uid\":54}"));

            Assert.AreEqual(-1, myWorld.GetFoodCubeIndex(47));
            Assert.AreEqual(0, myWorld.GetFoodCubeIndex(45));
            Assert.AreEqual(1, myWorld.GetFoodCubeIndex(54));
        }

        [TestMethod]
        public void WorldGetPlayerCubeIndexTest1()
        {
            World myWorld = new World();


            myWorld.AddCube(Cube.FromJson("{\"food\":false, \"uid\":45}"));
            myWorld.AddCube(Cube.FromJson("{\"food\":false, \"uid\":54}"));

            Assert.AreEqual(-1, myWorld.GetPlayerCubeIndex(47));
            Assert.AreEqual(0, myWorld.GetPlayerCubeIndex(45));
            Assert.AreEqual(1, myWorld.GetPlayerCubeIndex(54));
        }

        [TestMethod]
        public void WorldGetFoodCubeTest1()
        {
            World myWorld = new World();
            Cube[] cubes = new Cube[3];

            for (int i = 0; i < 3; i++)
            {
                cubes[i] = Cube.FromJson("{\"food\":true, \"uid\":" + i + "}");
                myWorld.AddCube(cubes[i]);
            }

            Assert.AreEqual(null, myWorld.GetFoodCube(5));
            Assert.AreEqual(cubes[0], myWorld.GetFoodCube(0));
            Assert.AreEqual(cubes[1], myWorld.GetFoodCube(1));
            Assert.AreEqual(cubes[2], myWorld.GetFoodCube(2));
        }

        [TestMethod]
        public void WorldGetPlayerCubeTest1()
        {
            World myWorld = new World();
            Cube[] cubes = new Cube[3];

            for (int i = 0; i < 3; i++)
            {
                cubes[i] = Cube.FromJson("{\"food\":false, \"uid\":" + i + "}");
                myWorld.AddCube(cubes[i]);
            }

            Assert.AreEqual(null, myWorld.GetPlayerCube(5));
            Assert.AreEqual(cubes[0], myWorld.GetPlayerCube(0));
            Assert.AreEqual(cubes[1], myWorld.GetPlayerCube(1));
            Assert.AreEqual(cubes[2], myWorld.GetPlayerCube(2));
        }

        [TestMethod]
        public void WorldUpdateFoodCubeTest1()
        {
            World myWorld = new World();
            Cube cube = Cube.FromJson("{\"food\":true, \"mass\":20, \"uid\":0}");

            myWorld.AddCube(cube);
            myWorld.UpdateFoodCube(cube);
            Assert.AreEqual(null, myWorld.GetFoodCube(0));

            myWorld.UpdateFoodCube(cube);
            Assert.AreEqual(cube, myWorld.GetFoodCube(0));
        }

        [TestMethod]
        public void WorldUpdatePlayerCubeTest1()
        {
            World myWorld = new World();
            Cube cube = Cube.FromJson("{\"food\":false, \"mass\":0, \"uid\":0}");
            Cube cube2 = Cube.FromJson("{\"food\":false, \"mass\":100, \"uid\":1}");

            myWorld.AddCube(cube);
            myWorld.UpdatePlayerCube(cube);
            Assert.AreEqual(null, myWorld.GetPlayerCube(0));

            myWorld.UpdatePlayerCube(cube2);
            Assert.AreEqual(cube2, myWorld.GetPlayerCube(1));

            cube2 = Cube.FromJson("{\"food\":false, \"mass\":1000, \"uid\":1}");
            myWorld.UpdatePlayerCube(cube2);
            Assert.AreEqual(cube2, myWorld.GetPlayerCube(1));
            Assert.AreEqual(1000, myWorld.GetPlayerCube(1).Mass);
        }
    }
}
