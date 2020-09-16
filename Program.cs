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
using System.Diagnostics;
using System.Net;

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
            Stopwatch timedRun = new Stopwatch();
            timedRun.Start();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss:fff zzz}] [{ThreadId}] [{Level:u3}] - {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("log.txt")
                .CreateLogger();
            ExternalDataManipulator dataManipulator = new ExternalDataManipulator();

            previouslyFoundUris = await dataManipulator.LoadPreviousResults();
            await CrawlPages();
            await dataManipulator.ExportData(@"ChangedUrisFound.txt", changedPages);
            await dataManipulator.ExportData(@"NewUrisFound.txt", newPages);
            await dataManipulator.ExportData(@"AllUrisFound.txt", allUrisFound);

            if (changedPages.Any())
            {
                Log.Information($"Sending Changed pages email containing {changedPages.Count} items");
                new EmailHelpers().SendEmail($"Kingsleigh have {changedPages.Count} CHANGED pages", dataManipulator.GetUris(changedPages).ToString());
            }

            if (newPages.Any())
            {
                Log.Information($"Sending NEW pages email containing {newPages.Count} items");
                new EmailHelpers().SendEmail($"Kingsleigh have {newPages.Count} NEW pages", dataManipulator.GetUris(newPages).ToString());
            }

            Log.Information($"Completed in {timedRun.Elapsed}!");
        }

        private static async Task CrawlPages()
        {
            PoliteWebCrawler crawler = await SetupCrawler();
            var crawlResult = await crawler.CrawlAsync(new Uri("https://mylearningbook.co.uk/Book"));
        }

        private static async Task<PoliteWebCrawler> SetupCrawler()
        {
            var baseAddress = new Uri("https://mylearningbook.co.uk/");
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };

            var client = new HttpClient(handler) { BaseAddress = baseAddress };

            //usually i make a standard request without authentication, eg: to the home page.
            //by doing this request you store some initial cookie values, that might be used in the subsequent login request and checked by the server
            var homePageResult = client.GetAsync("/");
            homePageResult.Result.EnsureSuccessStatusCode();

            var content = new FormUrlEncodedContent(new[]
            {
                //the name of the form values must be the name of <input /> tags of the login form, in this case the tag is <input type="text" name="username">
                new KeyValuePair<string, string>("email", "@gmail.com"),
                new KeyValuePair<string, string>("ReturnUrl", "%2fHome%2fChild%2f331"),
            });
            var loginResult = await client.PostAsync("Logon?ReturnUrl=%2fHome%2fChild%2f331", content);
            loginResult.EnsureSuccessStatusCode();


            var config = new CrawlConfiguration
            {
                MaxPagesToCrawl = 0,
                MaxLinksPerPage = 0,
                MinCrawlDelayPerDomainMilliSeconds = 3000,
                IsExternalPageCrawlingEnabled = false,
                IsExternalPageLinksCrawlingEnabled = false,
            };
            var crawler = new PoliteWebCrawler(config,null, null, null,new PageRequester(config, new WebContentExtractor(), client), null, null, null, null);

            crawler.PageCrawlCompleted += Crawler_PageCrawlCompleted;
            crawler.PageCrawlDisallowed += Crawler_PageCrawlDisallowed;
            return crawler;
        }

        private static void Crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            Log.Error($"Unable to parse {e.PageToCrawl.Uri.AbsoluteUri} because {e.DisallowedReason}");
        }

        private static void Crawler_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            string pageContent = e.CrawledPage.AngleSharpHtmlDocument.Body.OuterHtml;
            string hash = ComputeCheckSum(pageContent);

            string retrievedValue = null;
            bool foundSomething = previouslyFoundUris.TryGetValue(e.CrawledPage.Uri, out retrievedValue);
            if (!foundSomething)
            {
                //Did not find it previously.
                //Alert about the new page
                Log.Information($"NEW page: {e.CrawledPage.Uri}");
                newPages[e.CrawledPage.Uri] = pageContent;
            }
            else if (retrievedValue != hash)
            {
                //Found it previously, but the content has changed
                //Alert about content changed
                Log.Information($"CHANGED page: {e.CrawledPage.Uri}");
                changedPages[e.CrawledPage.Uri] = pageContent;
            }

            allUrisFound[e.CrawledPage.Uri] = hash;
        }

        private static string ComputeCheckSum(string pageContent)
        {
            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = BitConverter.ToString(
                  md5.ComputeHash(Encoding.UTF8.GetBytes(pageContent))
                ).Replace("-", String.Empty);
            }

            return hash;
        }
    }
}
