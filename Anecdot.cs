using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KawaiiBot
{
    class Anecdot
    {

        public static string anec()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var client = new WebClient();
            client.Encoding = Encoding.GetEncoding(1251);

            var res = client.DownloadString("http://rzhunemogu.ru/RandJSON.aspx?CType=1");

            Dictionary<string, string> an = new Dictionary<string, string>();
            try
            {
               an  = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
            }

            catch
            {
                var bear = "Идет медведь по лесу, видит, машина горит. Сел в нее и сгорел.";
                var hat = "Купил мужик шляпу, а она ему как раз.";

                var defaults = new List<String> { bear, hat };

                Random rnd = new Random();

                int ind = rnd.Next(0, 1);
                return defaults[ind];
            }






            return an["content"];
        }
    }
}
