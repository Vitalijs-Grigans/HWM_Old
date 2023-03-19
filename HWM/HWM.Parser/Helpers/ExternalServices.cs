using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;


namespace HWM.Parser.Helpers
{
    [SupportedOSPlatform("windows")]
    public sealed class ExternalServices
    {
        // Singleton object implementation
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

        // Asynchronous method to retrieve image from external source
        public async Task DownloadImageAsync(string url, string fileName)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            Image image = Image.FromStream(new MemoryStream(bytes));

            image.Save(fileName);
        }

        // Asynchronous method to retrieve html source from external site
        public async Task<HtmlDocument> GetHtmlAsync(string url)
        {
            var doc = new HtmlDocument();
            var client = new HttpClient();
            byte[] bytes = await client.GetByteArrayAsync(url);

            // Extend encoding set for supporting Cyrillic web resources
            Encoding encoding = Encoding.GetEncoding("windows-1251");
            string html = encoding.GetString(bytes, 0, bytes.Length);

            doc.LoadHtml(html);

            return doc;
        }
    }
}
