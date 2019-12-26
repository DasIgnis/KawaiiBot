using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KawaiiBot
{
    public class Program
    {
        static List<Fact> facts;
        static List<Rule> rules;

        static void printProofNodeCF(List<ProofNodeCF> nodes, List<string> res)
        {
            if (nodes == null)
            {
                return;
            }
            foreach (ProofNodeCF node in nodes)
            {
                if (node == null)
                {
                    return;
                }
                string sFacts = "";
                for (int i = 0; i < node.prevFacts.Count; i++)
                {
                    sFacts += new String('\t', node.depth) + node.prevFacts[i].id + " " + node.prevFacts[i].description + " CF=" + node.prevFacts[i].CF + "\n";
                }
                /*if (node.prevFacts.Count == 1)
                {
                    sFacts = facts[node.prevFacts[0]];
                }*/
                //Console.WriteLine(new String('\t', node.depth) + sFacts);
                //Console.WriteLine(new String('\t', node.depth + 1) + "[" + node.rule.id + "]");
                res.Add(new String('\t', node.depth) + sFacts);
                res.Add(new String('\t', node.depth + 1) + "[" + node.rule.id + "] CF=" + node.rule.CF + ")");
                string nFacts = "";
                for (int i = 0; i < node.nextFacts.Count; i++)
                {
                    nFacts += node.nextFacts[i].id + " " + node.nextFacts[i].description + " CF=" + node.nextFacts[i].CF + "\n";
                }
                //Console.WriteLine(new String('\t', node.depth + 1) + nFacts);
                res.Add(new String('\t', node.depth + 1) + nFacts);
                printProofNodeCF(node.children, res);
            }
        }


        static double calcCF(List<Fact> trueFacts, Rule rule)
        {
            List<Fact> premises = trueFacts.FindAll(x => rule.premises.Contains(x.id));
            double premiseCF = premises.Min(x => x.CF);
            double CF = rule.CF * premiseCF;
            return CF;
        }

        static List<ProofNodeCF> directProofAllCF(List<Fact> trueFacts, HashSet<Rule> appliedRules, int depth = 0)
        {
            HashSet<Rule> iterationAppliedRules = new HashSet<Rule>();
            foreach (Rule rule in rules)
            {
                if (!appliedRules.Contains(rule))
                {
                    List<string> trueFactsIds = trueFacts.Select(x => x.id).ToList();
                    var intersection = trueFactsIds.Intersect(rule.premises).ToList();
                    intersection.Sort();
                    if (rule.premises.SequenceEqual(intersection))
                    {
                        iterationAppliedRules.Add(rule);
                        appliedRules.Add(rule);
                    }
                }
            }
            if (iterationAppliedRules.Count == 0)
            {
                return null;
            }
            List<ProofNodeCF> toReturn = new List<ProofNodeCF>();
            List<Fact> conclusions = new List<Fact>();
            foreach (Rule rule in rules)
            {
                if (iterationAppliedRules.Contains(rule))
                {
                    double CF = 0.0;
                    Fact possible = trueFacts.Find(x => x.id == rule.conclusions[0]);
                    if (possible == null)
                    {
                        CF = calcCF(trueFacts, rule);
                    }
                    else
                    {
                        double CF1 = calcCF(trueFacts, rule);
                        double CF2 = possible.CF;
                        if (CF1 >= 0 && CF2 >= 0)
                        {
                            CF = CF1 + CF2 - CF1 * CF2;
                        }
                        else if (CF1 < 0 && CF2 < 0)
                        {
                            CF = CF1 + CF2 + CF1 * CF2;
                        }
                        else
                        {
                            CF = (CF1 + CF2) / (1 - Math.Min(Math.Abs(CF1), Math.Abs(CF2)));
                        }
                    }
                    string desc = facts.Find(x => x.id == rule.conclusions[0]).description;
                    Fact conclusion = new Fact(rule.conclusions[0], desc, CF);
                    conclusions.Add(conclusion);
                    List<Fact> premises = trueFacts.FindAll(x => rule.premises.Contains(x.id));
                    List<ProofNodeCF> res = directProofAllCF(trueFacts.Append(conclusion).ToList(), appliedRules, depth + 1);
                    if (res != null)
                    {
                        toReturn.Add(new ProofNodeCF(rule, premises, new List<Fact> { conclusion }, res, depth));
                    }
                    else
                    {
                        toReturn.Add(new ProofNodeCF(rule, premises, new List<Fact> { conclusion }, null, depth));
                    }
                }
            }

            var newRes = directProofAllCF(trueFacts.Concat(conclusions).ToList(), appliedRules, depth + 1);

            if (newRes == null)
            {
                return toReturn;
            }
            else {
                return toReturn.Concat(newRes).ToList();
            }
        }

        static ProofNodeCF directProofCF(string provable, List<Fact> trueFacts, HashSet<Rule> appliedRules, int depth = 0)
        {
            HashSet<Rule> iterationAppliedRules = new HashSet<Rule>();
            foreach (Rule rule in rules)
            {
                if (!appliedRules.Contains(rule))
                {
                    List<string> trueFactsIds = trueFacts.Select(x => x.id).ToList();
                    var intersection = trueFactsIds.Intersect(rule.premises).ToList();
                    intersection.Sort();
                    if (rule.premises.SequenceEqual(intersection))
                    {
                        if (rule.conclusions.Contains(provable))
                        {
                            List<Fact> premises = trueFacts.FindAll(x => rule.premises.Contains(x.id));
                            string desc = facts.Find(x => x.id == rule.conclusions[0]).description;
                            List<Fact> nextFacts = new List<Fact> { new Fact(rule.conclusions[0], desc, calcCF(trueFacts, rule)) };
                            return new ProofNodeCF(rule, premises, nextFacts, null, depth);
                        }
                        iterationAppliedRules.Add(rule);
                        appliedRules.Add(rule);
                    }
                }
            }
            if (iterationAppliedRules.Count == 0)
            {
                return null;
            }
            foreach (Rule rule in rules)
            {
                if (iterationAppliedRules.Contains(rule))
                {
                    double CF = 0.0;
                    Fact possible = trueFacts.Find(x => x.id == rule.conclusions[0]);
                    if (possible == null)
                    {
                        CF = calcCF(trueFacts, rule);
                    }
                    else
                    {
                        double CF1 = calcCF(trueFacts, rule);
                        double CF2 = possible.CF;
                        if (CF1 >= 0 && CF2 >= 0)
                        {
                            CF = CF1 + CF2 - CF1 * CF2;
                        }
                        else if (CF1 < 0 && CF2 < 0)
                        {
                            CF = CF1 + CF2 + CF1 * CF2;
                        }
                        else
                        {
                            CF = (CF1 + CF2) / (1 - Math.Min(Math.Abs(CF1), Math.Abs(CF2)));
                        }
                    }
                    string desc = facts.Find(x => x.id == rule.conclusions[0]).description;
                    Fact conclusion = new Fact(rule.conclusions[0], desc, CF);
                    List<Fact> premises = trueFacts.FindAll(x => rule.premises.Contains(x.id));
                    ProofNodeCF res = directProofCF(provable, trueFacts.Append(conclusion).ToList(), appliedRules, depth + 1);
                    if (res != null)
                    {
                        return new ProofNodeCF(rule, premises, new List<Fact> { conclusion }, new List<ProofNodeCF> { res }, depth);
                    }
                }
            }

            return null;
        }

        static public List<string> SetupFacts(List<string> true_facts, string provable)
        {
            facts = Parser.getFactsInput("..\\..\\FACTS.txt");
            rules = Parser.getRulesInput("..\\..\\RULES.txt");

            List<Fact> trueFacts = facts.FindAll(x => true_facts.Any(x.description.Contains));

            if (provable != "")
            {
                ProofNodeCF proven = directProofCF(provable,
                    trueFacts,
                    new HashSet<Rule>());
                List<string> proofstrings = new List<string>();
                printProofNodeCF(new List<ProofNodeCF> { proven }, proofstrings);
                return proofstrings;
            }
            else
            {
                List<ProofNodeCF> proofNodeCFs = directProofAllCF(true_facts, new HashSet<Rule>());
                List<string> proofstrings = new List<string>();
                printProofNodeCF(proofNodeCFs, proofstrings);
                return proofstrings;
            }
        }
    }
}
