using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KawaiiBot
{
    public class Parser
    {
        public static List<Fact> getFactsInput(string filename)
        {
            List<Fact> facts = new List<Fact>();
            string[] lines = File.ReadAllLines(filename);
            foreach (string line in lines)
            {
                string id = line.Split(':')[0].Trim();
                string description = line.Split(':')[1].Split(';')[0];
                double CF = double.Parse(line.Split(':')[1].Split(';')[1].Trim());
                facts.Add(new Fact(id, description, CF));
            }
            return facts;
        }

        public static List<Rule> getRulesInput(string filename)
        {
            List<Rule> rules = new List<Rule>();
            string[] lines = File.ReadAllLines(filename);
            foreach (string line in lines)
            {
                string id = line.Split(':')[0];
                List<string> presc = new List<string>(line.Split(':')[1].Split(';')[0].Split(',').Select(s => s =  s.Trim()));
                List<string> conc = new List<string> { line.Split(':')[1].Split(';')[1].Trim() };
                string desc = line.Split(':')[1].Split(';')[2];
                double CF = double.Parse(line.Split(':')[1].Split(';')[3].Trim());
                rules.Add(new Rule(id, presc, conc, desc, CF));
            }
            return rules;
        }
    }
}
