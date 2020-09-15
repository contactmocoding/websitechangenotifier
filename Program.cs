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
          private static Dictionary<Uri, string> previouslyFoundUris = new Dictionary<Uri, string>();
          private static Dictionary<Uri, string> newPages = new Dictionary<Uri, string>();
          private static Dictionary<Uri, string> changedPages = new Dictionary<Uri, string>();

        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss:fff zzz}] [{ThreadId}] [{Level:u3}] - {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            //Load in the previous results
            var previousItems = System.IO.File.ReadAllLines(@"AllUrisFound.txt");
            foreach (string lineItem in previousItems)
            {
                var lineData = lineItem.Split(',');
                previouslyFoundUris[new Uri(lineData[0])] = lineData[1];
            }
            Log.Information("Demo starting up!");

            // await DemoPageRequester();
            await DemoSimpleCrawler();
            ExportData(@"ChangedUrisFound.txt",changedPages );
            ExportData(@"NewUrisFound.txt",newPages );
            ExportData(@"AllUrisFound.txt",allUrisFound );
            Log.Information("Demo done!");
            //Console.ReadKey();
        }

        private static void ExportData(string fileName, Dictionary<Uri,string> itemToExport)
        {
            StringBuilder fileContents = new StringBuilder();
            foreach (KeyValuePair<Uri, string> kvp in itemToExport.OrderBy(t => t.Key.AbsoluteUri))
            {
                fileContents.AppendLine($"{kvp.Key}, {kvp.Value}");
            }
            System.IO.File.WriteAllText(fileName, fileContents.ToString());
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


            string pageContent = e.CrawledPage.AngleSharpHtmlDocument.Body.OuterHtml;

            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = BitConverter.ToString(
                  md5.ComputeHash(Encoding.UTF8.GetBytes(pageContent))
                ).Replace("-", String.Empty);
            }

            if (previouslyFoundUris[e.CrawledPage.Uri] ==null)
            {
                //Did not find it previously.
                //Alert about the new page
                newPages[e.CrawledPage.Uri] = pageContent;
            }
            else if(previouslyFoundUris[e.CrawledPage.Uri] !=hash)
            {
                //Found it previously, but the content has changed
                //Alert about content changed
                changedPages[e.CrawledPage.Uri] = pageContent;
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
