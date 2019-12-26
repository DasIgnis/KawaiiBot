using System;
using System.Collections.Generic;
using System.Text;

namespace KawaiiBot
{
    public class Fact
    {
        public string id;
        public string description;
        public double CF;

        public Fact(string _id, string _descrition, double _CF)
        {
            this.id = _id;
            this.description = _descrition;
            this.CF = _CF;
        }
    }
}
