using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Server
{
    /// <summary>
    /// This class contains the default constants of the AgCubio world.
    /// </summary>
    public class Constants
    {
        public int Port { get; private set; }
        public int WebPort { get; private set; }
        public int Height { get; }
        public int Width => Height;
        public int HeartbeatsPerSecond { get; private set; }
        public int TopSpeed { get; private set; }
        public int LowSpeed { get; private set; }
        public int AttritionRate { get; private set; }
        public int FoodValue { get; private set; }
        public float PlayerStartMass { get; private set; }
        public int MaxSplitDistance { get; private set; }
        public int MaxNumOfSplit { get; private set; }
        public int MaxFood { get; private set; }
        public float MinSplitMass { get; private set; }
        public double AbsorbConstant { get; private set; }
        public float MaxViewRange { get; private set; }

        public Constants(string filename)
        {
            try {

                using (XmlReader reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "parameters":
                                    break;

                                case "port":
                                    reader.Read();
                                    Port = int.Parse(reader.Value);
                                    break;

                                case "webport":
                                    reader.Read();
                                    Port = int.Parse(reader.Value);
                                    break;

                                case "height":
                                    reader.Read();
                                    Height = int.Parse(reader.Value);
                                    break;

                                case "heartbeats_per_second":
                                    reader.Read();
                                    HeartbeatsPerSecond = int.Parse(reader.Value);
                                    break;

                                case "top_speed":
                                    reader.Read();
                                    TopSpeed = int.Parse(reader.Value);
                                    break;
                                    
                                case "low_speed":
                                    reader.Read();
                                    LowSpeed = int.Parse(reader.Value);
                                    break;

                                case "attrition_rate":
                                    reader.Read();
                                    AttritionRate = int.Parse(reader.Value);
                                    break;

                                case "food_value":
                                    reader.Read();
                                    FoodValue = int.Parse(reader.Value);
                                    break;

                                case "player_start_mass":
                                    reader.Read();
                                    PlayerStartMass = float.Parse(reader.Value);
                                    break;

                                case "max_split_distance":
                                    reader.Read();
                                    MaxSplitDistance = int.Parse(reader.Value);
                                    break;

                                case "max_num_of_split":
                                    reader.Read();
                                    MaxNumOfSplit = int.Parse(reader.Value);
                                    break;

                                case "max_food":
                                    reader.Read();
                                    MaxFood = int.Parse(reader.Value);
                                    break;

                                case "min_split_mass":
                                    reader.Read();
                                    MinSplitMass = float.Parse(reader.Value);
                                    break;

                                case "absorb_constant":
                                    reader.Read();
                                    AbsorbConstant = double.Parse(reader.Value);
                                    break;

                                case "max_view_range":
                                    reader.Read();
                                    MaxViewRange = float.Parse(reader.Value);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e) // If file cannot be read
            {
                Console.WriteLine(e.Message);

                //Default values
                Port = 11000;
                WebPort = 11100;
                Height = 1000;
                HeartbeatsPerSecond = 25;
                TopSpeed = 5;
                LowSpeed = 1;
                AttritionRate = 200;
                FoodValue = 1;
                PlayerStartMass = 1000;
                MaxSplitDistance = 150;
                MaxNumOfSplit = 8;
                MaxFood = 5000;
                MinSplitMass = 100;
                AbsorbConstant = 1.25;
                MaxViewRange = 10000;
            }
        }
    }
}
