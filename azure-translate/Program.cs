using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace azure_translate
{
    internal class Program
    {
        //Set your subscription key from the azure console here
        private static readonly string subscriptionKey = "<Your Key here>";
        //Set your region here
        private static readonly string region = "westeurope";

        //Set API version. Current version is 3.0
        private static readonly string apiVersion = "3.0";

        private static async Task Main(string[] args)
        {
            //Call translate function
            await Translate("The quick brown fox jumps over the lazy dog");
        }


        public static async Task Translate(string text, string sourceLangaugeCode = "en",
            string targetLangaugeCode = "de")
        {
            //Prepare request
            var uri =
                $"https://api-eur.cognitive.microsofttranslator.com/translate?api-version={apiVersion}&from={sourceLangaugeCode}&to={targetLangaugeCode}";

            var httpWebRequest = (HttpWebRequest) WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            httpWebRequest.Headers.Add("Ocp-Apim-Subscription-Region", region);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            Debug.WriteLine("text to be translated: " + text);


            await using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                //prepare json
                var json = "[{\'Text\':\'" + text + "\'}]";
                Debug.WriteLine("json to be sent: " + json);
                await streamWriter.WriteAsync(json);
                await streamWriter.FlushAsync();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var translation = await streamReader.ReadToEndAsync();
                //Parse return value and get translation
                var a = JArray.Parse(translation);
                foreach (var o in a.Children<JObject>())
                foreach (var p in o.Properties())
                foreach (var s in p.Value.Children<JObject>())
                {
                    var rootObjects = JsonConvert.DeserializeObject<TranslationReturnObject>(s.ToString());
                    translation = rootObjects.Text;
                }

                Console.WriteLine(translation);
            }
        }
    }

    public class TranslationReturnObject
    {
        public string Text { get; set; }
        public string Langauge { get; set; }
    }
}