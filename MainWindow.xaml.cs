using AIMLbot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Speech.Synthesis;
using System.Drawing;
using System.Drawing.Imaging;

using Image = System.Drawing.Image;
using Path = System.IO.Path;

namespace KawaiiBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<long, User> users = new Dictionary<long, User>();
        BackgroundWorker asyncWorker;
        AIMLbot.Bot bot;
        static Telegram.Bot.TelegramBotClient Bot;
        static NeuralNetwork net;
        User user;
        static ImageProcessor generator;
        public MainWindow()
        {
            InitializeComponent();

            asyncWorker = new BackgroundWorker();
            asyncWorker.DoWork += launchWorkerAsync;

            bot = new AIMLbot.Bot();
            bot.loadSettings();
            bot.isAcceptingUserInput = false;
            bot.loadAIMLFromFiles();
            bot.isAcceptingUserInput = true;
            user = new User("Уважаемый", bot);

            user.Predicates.addSetting("favouriteanimal", "default");
            user.Predicates.addSetting("name", "default");

            Config.current = Config.LoadFromJson(Path.Combine("..", "..", "config.json"));
            generator = new ImageProcessor();
            initNet();
            net.LoadFromJson(Path.Combine("..", "..", "bot.json"));
        }

        void initNet()
        {
            int[] structure = Config.current.net_sctructure.Split(';').Select((c) => int.Parse(c)).ToArray();
            if (structure.Length < 2 || structure[0] != Config.current.width + Config.current.height || structure[structure.Length - 1] != Config.current.figures.Count)
            {
                return;
            };

            net = new NeuralNetwork(structure, x => 1 / (1 + Math.Exp(-x)));
        }

        static private Bitmap executeImage(Bitmap btm, int cx, int cy, int width, int height)
        {
            Bitmap res = new Bitmap(width, height);
            for (int x = cx - width / 2; x < cx + width / 2; x++)
            {
                for (int y = cy - height / 2; y < cy + height / 2; y++)
                {
                    res.SetPixel(x - cx + width / 2, y - cy + height / 2, btm.GetPixel(x, y));
                }
            }
            return res;
        }

        static private string predict(Image image)
        {
            try
            {
                Bitmap prepared = executeImage(new Bitmap(image), image.Width / 2, image.Height / 2,
                        Config.current.width, Config.current.height);
                Bitmap proc;
                Sample fig = generator.GenerateFigure(prepared, Config.current.figure_id(Config.current.figure), out proc);

                net.Predict(fig);

                return fig.ToString();
            }
            catch (Exception r)
            {
                // Something unexpected went wrong.
                // Maybe it is also necessary to terminate / restart the application.
                return r.ToString();
            }
        }

        async void launchWorkerAsync(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string key = e.Argument as String;

            try
            {
                Bot = new Telegram.Bot.TelegramBotClient(key);
                await Bot.SetWebhookAsync("");
                int offset = 0;
                while (true)
                {
                    var messages = await Bot.GetUpdatesAsync(offset);
                    foreach (var message in messages)
                    {
                        var msgText = message.Message;
                        User currentUser;
                        if (users.ContainsKey(msgText.Chat.Id))
                        {
                            currentUser = users[msgText.Chat.Id];
                        }
                        else
                        {
                            currentUser = new User("Уважаемый", bot);

                            currentUser.Predicates.addSetting("favouriteanimal", "default");
                            currentUser.Predicates.addSetting("name", "default");

                            users[msgText.Chat.Id] = currentUser;
                        }

                        if (msgText.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                        {
                            Console.WriteLine(msgText.Text.ToUpper());
                            Request r = new Request(msgText.Text, currentUser, bot);
                            Result res = bot.Chat(r);
                            if (res.Output != "")
                            {
                                if (res.Output.EndsWith(".jpg."))
                                {
                                    await Bot.SendPhotoAsync(msgText.Chat.Id, res.Output.Remove(res.Output.Count() - 1, 1));
                                }
                                else if (res.Output == "time.")
                                {
                                    var time = "У меня " + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ", сколько у вас - не знаю";
                                    await Bot.SendTextMessageAsync(msgText.Chat.Id, time);
                                }
                                else if (res.Output == "foot.")
                                {
                                    string table = football.getTable();
                                    await Bot.SendTextMessageAsync(msgText.Chat.Id, table);
                                }
                                else if (res.Output == "anec.")
                                {
                                    string anec = Anecdot.anec();
                                    await Bot.SendTextMessageAsync(msgText.Chat.Id, anec);
                                }
                                else if (res.Output == "voice.")
                                {
                                    string anec = Anecdot.anec();

                                    SpeechSynthesizer speaker = new SpeechSynthesizer();

                                    speaker.Rate = 2;
                                    speaker.Volume = 100;

                                    speaker.SetOutputToWaveFile("soundfile.wav");
                                    speaker.Speak(anec + "\n АХАХАХАХАХАХ");
                                    speaker.SetOutputToDefaultAudioDevice();

                                    var fStream = new FileStream("soundfile.wav", FileMode.OpenOrCreate);
                                    await Bot.SendVoiceAsync(msgText.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fStream));
                                    fStream.Dispose();
                                }
                                else if (res.Output == "beauty.")
                                {
                                    var fStream = new FileStream("..\\..\\1beauty.jpg", FileMode.OpenOrCreate);
                                    await Bot.SendPhotoAsync(msgText.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fStream), "Красивее не сыскать");
                                }
                                else if (res.Output.Contains("poetryFlag"))
                                {
                                    var poem = res.Output.Replace("poetryFlag", "");
                                    SpeechSynthesizer speaker = new SpeechSynthesizer();

                                    speaker.Rate = 2;
                                    speaker.Volume = 100;

                                    speaker.SetOutputToWaveFile("soundfile.wav");
                                    speaker.Speak(poem);
                                    speaker.SetOutputToDefaultAudioDevice();

                                    var fStream = new FileStream("soundfile.wav", FileMode.OpenOrCreate);
                                    await Bot.SendVoiceAsync(msgText.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fStream));
                                    fStream.Dispose();
                                }
                                else
                                {
                                    await Bot.SendTextMessageAsync(msgText.Chat.Id, res.Output);
                                }
                            }
                        }
                        else if (msgText.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
                        {
                            ProcessImage(msgText.Photo[msgText.Photo.Length - 1].FileId, "", msgText.Chat.Id);
                        }
                        offset = message.Id + 1;
                    }
                }

            }
            catch (Telegram.Bot.Exceptions.ApiRequestException exception)
            {
                Console.WriteLine(exception.Message);
                asyncWorker.RunWorkerAsync("999037946:AAHbd0xIjp5l6iS0aGuVB-jIP2R4a99EUFo");
            }
        }

        private void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!asyncWorker.IsBusy)
            {
                asyncWorker.RunWorkerAsync("999037946:AAHbd0xIjp5l6iS0aGuVB-jIP2R4a99EUFo");
            }
        }

        private static async void ProcessImage(string fileId, string path, long chatId)
        {
            try
            {
                string fileName = "./temp" + fileId + ".jpg";
                var file = await Bot.GetFileAsync(fileId);
                FileStream fs = new FileStream(fileName, FileMode.Create);
                await Bot.DownloadFileAsync(file.FilePath, fs);
                fs.Close();
                fs.Dispose();

                Image img = System.Drawing.Image.FromFile(fileName);

                string cl_name = predict(img);
                

                await Bot.SendTextMessageAsync(chatId, cl_name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading: " + ex.Message);
            }
        }

        private void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            bot = new AIMLbot.Bot();
            bot.loadSettings();
            bot.isAcceptingUserInput = false;
            bot.loadAIMLFromFiles();
            bot.isAcceptingUserInput = true;
            user = new User("Уважаемый", bot);

            user.Predicates.addSetting("favouriteanimal", "default");
            user.Predicates.addSetting("name", "default");
        }
    }
}
