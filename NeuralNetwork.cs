using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiBot
{
    public class NeuralNetwork : BaseNetwork
    {
        public double[] output;
        public List<List<Neuron>> neurons;
        Func<double, double> activator;
        public double learning_rate;
        Random r;
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        public NeuralNetwork(int[] structure, Func<double, double> activator)
        {
            this.activator = activator;
            this.r = new Random((int)DateTime.Now.Ticks);
            this.neurons = new List<List<Neuron>>();
            ReInit(structure);
        }
        public override double[] getOutput()
        {
            return output;
        }

        public override int Predict(Sample sample)
        {
            Calculate(ref sample);
            sample.processOutput();
            return sample.recognizedClassId;
        }
        public double GetRandomNumber(double minimum, double maximum)
        {
            return r.NextDouble() * (maximum - minimum) + minimum;
        }
        public double random(double left = 0, double right = 1, double seed = 666)
        {
            return GetRandomNumber(-1, 1);
        }

        public override void ReInit(int[] structure, double initialLearningRate = 0.25)
        {
            learning_rate = initialLearningRate;
            neurons.Clear();

            neurons.Add(new List<Neuron>());
            int count = structure[0];
            for (int j = 0; j < count; j++)
            {
                neurons[0].Add(new Neuron(new List<double>() { 1 }));
            }

            for (int i = 1; i < structure.Length; i++)
            {
                count = structure[i];
                neurons.Add(new List<Neuron>());
                for (int j = 0; j < count; j++)
                {
                    neurons[i].Add(
                        new Neuron(Enumerable.Range(0, structure[i - 1]).Select(x => random()).ToList())
                    );
                }
            }
        }

        public override double TestOnDataSet(SamplesSet testSet)
        {
            for (int i = 0; i < testSet.Count; i++)
            {
                var sample = testSet.samples[i];
                Calculate(ref sample);
                sample.processOutput();
            }
            return testSet.ErrorsCount();
        }

        public override int Train(Sample sample, bool parallel = true)
        {
            throw new NotImplementedException();
        }

        public void Calculate(ref Sample sample, bool parallel = false)
        {
            List<double> input = sample.input.ToList();
            List<double> output = Enumerable.Repeat(0d, neurons[0].Count).ToList();
            if (parallel)
            {
                Parallel.For(0, neurons[0].Count, j =>
                {
                    output[j] = neurons[0][j].calculate(input, x => x);
                });
            }
            else
            {
                for (int j = 0; j < neurons[0].Count; j++)
                {
                    output[j] = neurons[0][j].calculate(new List<double>() { input[j] }, x => x);
                }
            }

            for (int i = 1; i < neurons.Count; i++)
            {
                output = Enumerable.Repeat(0d, neurons[i].Count).ToList();
                if (parallel)
                {
                    Parallel.For(0, neurons[i].Count, j =>
                    {
                        output[j] = neurons[i][j].calculate(input, activator);
                    });
                }
                else
                {
                    for (int j = 0; j < neurons[i].Count; j++)
                    {
                        output[j] = neurons[i][j].calculate(input, activator);
                    }
                }

                input = output;
            }
            sample.output = output.ToArray();
            this.output = output.ToArray();
        }

        public void BackwardError(Sample sample, bool parallel = true)
        {
            var sigma = Enumerable.Range(0, sample.output.Length).Select(k => sample.error[k] * neurons[neurons.Count - 1][k].output * (1 - neurons[neurons.Count - 1][k].output)).ToList();
            List<double> sigma_old;
            sigma_old = sigma;
            for (int layer = neurons.Count - 2; 0 <= layer; layer--)
            {
                sigma = Enumerable.Repeat(0d, neurons[layer].Count).ToList();
                if (parallel)
                {
                    Parallel.For(0, neurons[layer].Count, h =>
                    {
                        for (int k = 0; k < neurons[layer + 1].Count; k++)
                        {
                            sigma[h] += sigma_old[k] * neurons[layer + 1][k].weights[h];
                        }
                        sigma[h] *= neurons[layer][h].output * (1 - neurons[layer][h].output);
                        for (int k = 0; k < neurons[layer + 1].Count; k++)
                        {
                            neurons[layer + 1][k].weights[h] += learning_rate * sigma_old[k] * neurons[layer][h].output;
                        }
                    });
                }
                else
                {
                    for (int h = 0; h < neurons[layer].Count; h++)
                    {
                        for (int k = 0; k < neurons[layer + 1].Count; k++)
                        {
                            sigma[h] += sigma_old[k] * neurons[layer + 1][k].weights[h];
                        }
                        sigma[h] *= neurons[layer][h].output * (1 - neurons[layer][h].output);
                        for (int k = 0; k < neurons[layer + 1].Count; k++)
                        {
                            neurons[layer + 1][k].weights[h] += learning_rate * sigma_old[k] * neurons[layer][h].output;
                        }
                    }
                }
                sigma_old = sigma;
            }
        }

        public override double TrainOnDataSet(SamplesSet samplesSet, int epochs_count, double acceptable_erorr, bool parallel = true)
        {
            double error = 0;
            stopWatch.Restart();
            for (int i = 0; i < epochs_count; i++)
            {
                error = 0;
                for (int j = 0; j < samplesSet.Count; j++)
                {
                    var sample = samplesSet[j];
                    Calculate(ref sample, parallel);
                    sample.processOutput();
                    error += sample.EstimatedError();
                    BackwardError(sample, parallel);
                }
                if (error < acceptable_erorr)
                    break;
            }
            return error;
        }

        public NeuralNetwork SaveAsJson(string filepath)
        {
            using (StreamWriter file = File.CreateText(filepath))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, this.neurons);
            }
            return this;
        }

        public NeuralNetwork LoadFromJson(string filepath)
        {
            using (StreamReader r = new StreamReader(filepath))
            {
                string json = r.ReadToEnd();
                this.neurons = JsonConvert.DeserializeObject<List<List<Neuron>>>(json);
                return this;
            }
        }
    }
}
