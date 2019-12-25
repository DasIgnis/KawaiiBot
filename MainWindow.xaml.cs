using AIMLbot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace KawaiiBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        BackgroundWorker asyncWorker;
        AIMLbot.Bot bot;
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
        }

        async void launchWorkerAsync(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string key = e.Argument as String;

            try
            {

                var Bot = new Telegram.Bot.TelegramBotClient(key);
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
                            //if (msgText.Text == "/saysomething")
                            //{
                            //    await Bot.SendTextMessageAsync(msgText.Chat.Id, "Ня!", replyToMessageId: msgText.MessageId);
                            //}
                            //if (msgText.Text.ToUpper() == "YARE YARE")
                            //{
                            //    await Bot.SendPhotoAsync(msgText.Chat.Id, "https://i.pinimg.com/originals/bd/6e/5a/bd6e5a259c33a226550960b91ff857c4.jpg");
                            //    await Bot.SendTextMessageAsync(msgText.Chat.Id, "Daze, красавчик ;^)");
                            //}
                            Request r = new Request(msgText.Text.ToUpper(), user, bot);
                            Result res = bot.Chat(r);
                            if (res.Output != "")
                                await Bot.SendTextMessageAsync(msgText.Chat.Id, res.Output);
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
    }
}
