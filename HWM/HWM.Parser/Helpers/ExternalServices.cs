using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;
using Newtonsoft.Json;

using HWM.Parser.Entities.LeaderGuild;


namespace HWM.Parser.Helpers
{
    [SupportedOSPlatform("windows")]
    public sealed class ExternalServices
    {
        private static readonly Lazy<ExternalServices> requestHelper =
            new Lazy<ExternalServices>(() => new ExternalServices());

        private ExternalServices() { }

        public static ExternalServices Instance 
        { 
            get 
            { 
                return requestHelper.Value; 
            } 
        }

        public HtmlDocument GetHtml(string url)
        {
            var doc = new HtmlDocument();
            var client = new HttpClient();
            byte[] bytes = client.GetByteArrayAsync(url).Result;
            Encoding encoding = Encoding.GetEncoding("windows-1251");
            string html = encoding.GetString(bytes, 0, bytes.Length);

            doc.LoadHtml(html);

            return doc;
        }

        public void DownloadImage(string url, string fileName)
        {
            var client = new HttpClient();
            var response = client.GetAsync(url).Result;

            response.EnsureSuccessStatusCode();

            byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            Image image = Image.FromStream(new MemoryStream(bytes));

            image.Save(fileName);
        }

        public void SaveJson(IEnumerable<Follower> data, string path)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            Task task = new Task(() => File.WriteAllTextAsync(path, json));

            task.RunSynchronously();
        }

        public IEnumerable<Follower> LoadJson(string path)
        {
            string json = File.ReadAllTextAsync(path).Result;

            return JsonConvert.DeserializeObject<IEnumerable<Follower>>(json);
        }
    }
}
