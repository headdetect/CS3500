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
    }
}
