using System;
using System.Net.Http;
using System.Threading.Tasks;
using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using Serilog;
using Serilog.Formatting.Json;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace websitechangenotifier
{
    class Program
    {
          private static Dictionary<Uri, string> allUrisFound = new Dictionary<Uri, string>();

        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss:fff zzz}] [{ThreadId}] [{Level:u3}] - {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Demo starting up!");

            // await DemoPageRequester();
            await DemoSimpleCrawler();
            StringBuilder fileContents = new StringBuilder();
            foreach (KeyValuePair<Uri, string> kvp in allUrisFound)
            {
                fileContents.AppendLine($"{kvp.Key}, {kvp.Value}");
            }
            System.IO.File.WriteAllText(@"AllUrisFound.txt", fileContents.ToString());
            Log.Information("Demo done!");
            //Console.ReadKey();
        }

        private static async Task DemoSimpleCrawler()
        {
            var config = new CrawlConfiguration
            {
                MaxPagesToCrawl = 5,
                MaxLinksPerPage = 0,
                MinCrawlDelayPerDomainMilliSeconds = 3000,
            };
            var crawler = new PoliteWebCrawler(config);

            crawler.PageCrawlCompleted += Crawler_PageCrawlCompleted;

            //   var crawlResult = await crawler.CrawlAsync(new Uri("https://www.kingsleighprimary.co.uk/parents/letters/"));
            //var crawlResult2 = await crawler.CrawlAsync(new Uri("https://www.kingsleighprimary.co.uk/classes/reception/"));                        
            var crawlResult2 = await crawler.CrawlAsync(new Uri("https://www.kingsleighprimary.co.uk"));

        }

        private static void Crawler_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            Log.Information($"------- Completed [{e.CrawledPage.Uri}].");


            string theString = e.CrawledPage.AngleSharpHtmlDocument.Body.InnerHtml;

            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = BitConverter.ToString(
                  md5.ComputeHash(Encoding.UTF8.GetBytes(theString))
                ).Replace("-", String.Empty);
            }

            allUrisFound[e.CrawledPage.Uri] = hash;
            // Log.Information($"Found {String.Join(",",e.CrawledPage.ParsedLinks.Select(t=>t.HrefValue))}");
        }

        private static async Task DemoPageRequester()
        {
            var pageRequester =
                new PageRequester(new CrawlConfiguration(), new WebContentExtractor());

            //var result = await pageRequester.MakeRequestAsync(new Uri("http://google.com"));
            var result = await pageRequester.MakeRequestAsync(new Uri("https://www.kingsleighprimary.co.uk/parents/"));
            Log.Information($"{result}", new { url = result.Uri, status = Convert.ToInt32(result.HttpResponseMessage.StatusCode) });

        }
    }
}
