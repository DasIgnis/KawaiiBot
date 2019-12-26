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

namespace KawaiiBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        BackgroundWorker asyncWorker;
        AIMLbot.Bot bot;
        static Telegram.Bot.TelegramBotClient Bot;
        User user;
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

            user.Predicates.addSetting("favourite-animal", "default");
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
                        if (msgText.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                        {
                            Console.WriteLine(msgText.Text.ToUpper());
                            Request r = new Request(msgText.Text.ToUpper(), user, bot);
                            Result res = bot.Chat(r);
                            if (res.Output != "")
                            {
                                if (res.Output.EndsWith(".jpg."))
                                {
                                    await Bot.SendPhotoAsync(msgText.Chat.Id, res.Output.Remove(res.Output.Count() - 1, 1));
                                }
                                else if (res.Output == "time.")
                                {
                                    var time = "У меня " + DateTime.Now.Hour.ToString() +":"+ DateTime.Now.Minute.ToString() + ", сколько у вас - не знаю";
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
                                    speaker.Speak("Hello world.");


                                    speaker.SetOutputToWaveFile("soundfile.ogg");
                                    speaker.Speak(anec + "\n АХАХАХАХАХАХ");
                                    speaker.SetOutputToDefaultAudioDevice();

                                    var fStream = new FileStream("soundfile.ogg", FileMode.OpenOrCreate);
                                    await Bot.SendVoiceAsync(msgText.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fStream));
                                    fStream.Dispose();
                                }

                                else if (res.Output == "beauty.")
                                {                                
                                    var fStream = new FileStream("..\\..\\1beauty.jpg", FileMode.OpenOrCreate);
                                    await Bot.SendPhotoAsync(msgText.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fStream),"Красивее не сыскать");
                                }

                                else if (res.Output.Contains("poetryFlag"))
                                {
                                    var poem = res.Output.Replace("poetryFlag", "");
                                    SpeechSynthesizer speaker = new SpeechSynthesizer();

                                    speaker.Rate = 2;
                                    speaker.Volume = 100;
                                    speaker.Speak("Hello world.");


                                    speaker.SetOutputToWaveFile("soundfile.ogg");
                                    speaker.Speak(poem);
                                    speaker.SetOutputToDefaultAudioDevice();

                                    var fStream = new FileStream("soundfile.ogg", FileMode.OpenOrCreate);
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
                            DownloadFile(msgText.Photo[msgText.Photo.Length - 1].FileId, "");
                        }
                        offset = message.Id + 1;
                    }
                }

            } catch (Telegram.Bot.Exceptions.ApiRequestException exception)
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

        private static async void DownloadFile(string fileId, string path)
        {
            try
            {
                var file = await Bot.GetFileAsync(fileId);
                FileStream fs = new FileStream("./temp" + fileId + ".jpg", FileMode.Create);
                await Bot.DownloadFileAsync(file.FilePath, fs);
                fs.Close();
                fs.Dispose();
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
        }
    }
}
