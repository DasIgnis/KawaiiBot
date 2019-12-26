using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KawaiiBot
{
    public class Filters
    {
    }

    public class Area
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Competition
    {
        public int id { get; set; }
        public Area area { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string plan { get; set; }
        public DateTime lastUpdated { get; set; }
    }

    public class Season
    {
        public int id { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public int currentMatchday { get; set; }
        public object winner { get; set; }
    }

    public class Team
    {
        public int id { get; set; }
        public string name { get; set; }
        public string crestUrl { get; set; }
    }

    public class Table
    {
        public int position { get; set; }
        public Team team { get; set; }
        public int playedGames { get; set; }
        public int won { get; set; }
        public int draw { get; set; }
        public int lost { get; set; }
        public int points { get; set; }
        public int goalsFor { get; set; }
        public int goalsAgainst { get; set; }
        public int goalDifference { get; set; }
    }

    public class Standing
    {
        public string stage { get; set; }
        public string type { get; set; }
        public object group { get; set; }
        public List<Table> table { get; set; }
    }

    public class RootObject
    {
        public Filters filters { get; set; }
        public Competition competition { get; set; }
        public Season season { get; set; }

        [JsonProperty("standings")]
        public List<Standing> standings { get; set; }
    }
    public class football
    {
        public static String getTable()
        {


            var token = "b840898e0ffc4a0d9c9e4303c3aa37f4";
            var client = new WebClient();
            client.Headers.Add("X-Auth-Token", token);
            var res = client.DownloadString("http://api.football-data.org/v2/competitions/PL/standings");
            // Console.WriteLine(res);

            Dictionary<string, string> dict = new Dictionary<string, string>();

            RootObject r = JsonConvert.DeserializeObject<RootObject>(res);

            var table = r.standings[0].table;

            String str = "";

            foreach (var s in table)
            {
                var name = s.team.name;
                if (name.Contains("Wolverhampton"))
                {
                    name = "Wolves";
                }
                if (name.Contains("Brighton"))
                {
                    name = "Brighton ";
                }
                if (name.Contains("West"))
                {
                    name = "West Ham";
                }

                if (name.Contains("Tottenham"))
                {
                    name = "Tottenham";
                }

                if (name.Contains("Newcastle"))
                {
                    name = "Newcastle";
                }
                name = name.Replace("AFC", "");
                name = name.Replace("FC", "");
                name = name.Trim();
                str += s.position + ". " + name + " О:"+s.points.ToString() +"\n";
            }
            return str;
        }
    }
}
