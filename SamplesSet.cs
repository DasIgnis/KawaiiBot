using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections;

namespace KawaiiBot
{
    /// <summary>
    /// Выборка образов. Могут быть как классифицированные (обучающая, тестовая выборки), так и не классифицированные (обработка)
    /// </summary>
    public class SamplesSet : IEnumerable
    {
        /// <summary>
        /// Накопленные обучающие образы
        /// </summary>
        public List<Sample> samples;

        public SamplesSet()
        {
            samples = new List<Sample>();
        }

        public SamplesSet(int size)
        {
            samples = new List<Sample>(size);
        }

        /// <summary>
        /// Добавление образа к коллекции
        /// </summary>
        /// <param name="image"></param>
        public void AddSample(Sample image)
        {
            samples.Add(image);
        }
        public int Count { get { return samples.Count; } }

        public IEnumerator GetEnumerator()
        {
            return samples.GetEnumerator();
        }

        /// <summary>
        /// Реализация доступа по индексу
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Sample this[int i]
        {
            get { return samples[i]; }
            set { samples[i] = value; }
        }

        public double ErrorsCount()
        {
            double correct = 0;
            double wrong = 0;
            foreach (var sample in samples)
                if (sample.Correct()) ++correct; else ++wrong;
            return correct / (correct + wrong);
        }
        // Тут бы ещё сохранение в файл и чтение сделать, вообще классно было бы
    }
}
