using System;
using System.Collections.Generic;
using System.Text;

namespace KawaiiBot
{
    public class ProofNodeCF
    {
        public Rule rule;
        public List<Fact> prevFacts;
        public List<ProofNodeCF> children;
        public List<Fact> nextFacts;
        public int depth;

        public ProofNodeCF(Rule _rule, List<Fact> _prevFacts, List<Fact> _nextFacts, List<ProofNodeCF> _children, int _depth)
        {
            rule = _rule;
            prevFacts = _prevFacts;
            children = _children;
            depth = _depth;
            nextFacts = _nextFacts;
        }
    }
}
