using System;
using System.Collections.Generic;
using System.Text;

namespace KawaiiBot
{
    public class Rule
    {
        public string id;
        public List<string> premises;
        public List<string> conclusions;
        public string description;
        public double CF;

        public Rule(string _id, List<string> _premises, List<string> _conclusions, string _description, double _CF)
        {
            id = _id;
            premises = _premises;
            premises.Sort();
            conclusions = _conclusions;
            description = _description;
            CF = _CF;
        }
    }
}
