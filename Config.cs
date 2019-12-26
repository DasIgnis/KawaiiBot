using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiBot
{
    public class Config
    {
        public static Config current;
        public List<string> figures;
        public int epoches = 20;
        public int training_size = 700, test_size = 300;
        public int width = 640;
        public int height = 480;
        public int camera_width = 640;
        public int camera_height = 480;
        public string net_sctructure = "400;500;20;2";
        public double accuracy = 0.7;
        public bool parallel = true;
        public string figure;
        public int snapshot_timer_cooldown = 5000;
        public Config SaveAsJson(string filepath)
        {
            using (StreamWriter file = File.CreateText(filepath))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, this);
            }
            return this;
        }

        public static Config LoadFromJson(string filepath)
        {
            using (StreamReader r = new StreamReader(filepath))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<Config>(json);
            }
        }
        
        public int figure_id(string figure)
        {
            return figures.FindIndex(x => x == figure);
        }
    };
}
