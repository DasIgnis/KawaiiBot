using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiBot
{
    public class Neuron
    {
        public List<double> input, weights;
        public double output;

        public Neuron(List<double> weights)
        {
            this.weights = weights;
        }

        public Neuron()
        {
            this.weights = new List<double>();
        }

        public double calculate(List<double> input, Func<double, double> activator)
        {
            this.input = input;
            double sum = 0;
            for (int i = 0; i < weights.Count; i++)
                sum += input[i] * weights[i];
            output = activator(sum);
            return output;
        }
    }

}
