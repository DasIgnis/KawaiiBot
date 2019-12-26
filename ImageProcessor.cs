using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiBot
{
    public class ImageProcessor
    {
        static public Dictionary<string, string[]> files = new Dictionary<string, string[]>();
        public void fillFiles()
        {
            foreach(var fig in Config.current.figures)
            {
                files[fig] = Directory.GetFiles(screenshots_dir(fig), "*.png");
            }
        }
        private Random rand = new Random();

        public Sample GenerateFigure()
        {
            var type = rand.Next(Config.current.figures.Count);
            return process_output(generate_figure(type), type);
        }
        public SamplesSet GenerateSet()
        {
            SamplesSet res = new SamplesSet();
            for (int i = 0; i < Config.current.figures.Count; i++)
            {
                var sel = Config.current.figures[i];
                int take = Math.Min(files[sel].Length, Config.current.training_size);
                //foreach(var el in files[sel].OrderBy(x => rand.Next()).Take(take))
                Parallel.ForEach(files[sel].OrderBy(x => rand.Next()).Take(take), el =>
                {
                    res.AddSample(process_output(fillImage(new Bitmap(el)), i));
                });
            }
            res.samples = res.samples.OrderBy(x => rand.Next()).ToList();
            return res;
        }
        public Sample GenerateFigure(Bitmap btmp, int type, out Bitmap proc)
        {
            proc = processBitmap(btmp);
            return process_output(fillImage(proc), type);
        }
        public Sample process_output(bool[,] img, int type)
        {
            double[] input = new double[Config.current.width + Config.current.height];
            for (int i = 0; i < Config.current.width + Config.current.height; i++)
                input[i] = 0;

            for (int i = 0; i < Config.current.width; i++)
                for (int j = 0; j < Config.current.height; j++)
                    if (img[i, j])
                    {
                        input[i] += 1;
                        input[Config.current.width + j] += 1;
                    }

            return new Sample(input, Config.current.figures.Count, type);
        }
        public byte grayTransform(Color pixel)
        {
            return (byte)(0.2126 * pixel.R + 0.7152 * pixel.G + 0.0722 * pixel.B);
        }
        public Bitmap grayShaded(Bitmap raw_image)
        {
            Bitmap gray_shades = new Bitmap(raw_image.Width, raw_image.Height);
            for (int i = 0; i < raw_image.Width; i++)
            {
                for (int j = 0; j < raw_image.Height; j++)
                {
                    System.Drawing.Color pixel = raw_image.GetPixel(i, j);
                    byte sum = grayTransform(pixel);
                    System.Drawing.Color transform = System.Drawing.Color.FromArgb(pixel.A, sum, sum, sum);
                    gray_shades.SetPixel(i, j, transform);
                }
            }
            return gray_shades;
        }
        public Bitmap processBitmap(Bitmap raw_image)
        {
            //ClearImage();
            double floor = 0;
            Bitmap prepared = grayShaded(raw_image);
            for (int i = 0; i < Config.current.width; i++)
                for (int j = 0; j < Config.current.height; j++)
                {
                    var p = prepared.GetPixel(i, j);
                    floor += p.R;
                }
            floor = floor / (Config.current.width * Config.current.height);
            for (int i = 0; i < Config.current.width; i++)
                for (int j = 0; j < Config.current.height; j++)
                {
                    var p = prepared.GetPixel(i, j);
                    prepared.SetPixel(i, j, p.R < floor ? Color.Black : Color.White);
                }
            return prepared;
        }
        public string screenshots_dir(string figure)
        {
            var res = Path.Combine("..", "..", $"screenshots({Config.current.width}x{Config.current.height})", figure.ToLower());
            bool exists = System.IO.Directory.Exists(res);

            if (!exists)
                System.IO.Directory.CreateDirectory(res);
            return res;
        }
        public Bitmap saveClassSample(string figure, Bitmap raw, out string path)
        {
            path = Path.Combine(screenshots_dir(figure), $"{Guid.NewGuid()}.png");
            var b = processBitmap(raw);
            b.Save(path, ImageFormat.Png);
            return b;
        }
        public bool[,] generate_figure(int type = -1)
        {
            if (type == -1 || type >= Config.current.figures.Count)
                type = rand.Next(Config.current.figures.Count);
            var selected_class = Config.current.figures[type];
            var f = files[selected_class];
            return fillImage(new Bitmap(f[rand.Next(f.Length)]));
        }
        public bool[,] fillImage(Bitmap btmp)
        {
            bool[,] img = new bool[Config.current.width, Config.current.height];
            for (int i = 0; i < Config.current.width; i++)
                for (int j = 0; j < Config.current.height; j++)
                {
                    var p = btmp.GetPixel(i, j);
                    if (p.R == 0 && p.G == 0 && p.B == 0)
                        img[i, j] = true;
                    img[i, j] = p.R == 0 && p.G == 0 && p.B == 0 ? true : false;
                }
            return img;
        }
    }
}
