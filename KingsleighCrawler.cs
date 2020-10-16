using System;
using Abot2.Crawler;

using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Abot2.Poco;
using System.Text;
using Microsoft.Extensions.Logging;

namespace websitechangenotifier
{
    public class KingsleighCrawler
    {
        private Dictionary<Uri, string> allUrisFound = new Dictionary<Uri, string>();
        private Dictionary<Uri, string> previouslyFoundUris = new Dictionary<Uri, string>();
        private Dictionary<Uri, string> newPages = new Dictionary<Uri, string>();
        private Dictionary<Uri, string> changedPages = new Dictionary<Uri, string>();
        ILogger logger;

        public KingsleighCrawler( ILogger logger )
        {
            this.logger = logger;
        }

        public async Task RunCrawler()
        {
            Stopwatch timedRun = new Stopwatch();
            timedRun.Start();
            ExternalDataManipulator dataManipulator = new ExternalDataManipulator( logger);

            previouslyFoundUris = await dataManipulator.LoadPreviousResults();
            logger.LogInformation( $"Previously found {previouslyFoundUris.Count} Uris" );
            await CrawlPages();
            logger.LogInformation( $"Finished Crawling. found {changedPages.Count} changes. Found {newPages.Count} new pages." );
            //await dataManipulator.ExportData( @"ChangedUrisFound.txt", changedPages );
            //await dataManipulator.ExportData( @"NewUrisFound.txt", newPages );
            //await dataManipulator.ExportData( @"AllUrisFound.txt", allUrisFound );
            await dataManipulator.StoreCurrentResults(allUrisFound);
            logger.LogInformation( "Exported data" );
            if (changedPages.Any())
            {
                logger.LogInformation( $"Sending Changed pages email containing {changedPages.Count} items" );
                new EmailHelpers().SendEmail( $"Kingsleigh have {changedPages.Count} CHANGED pages", dataManipulator.GetUris( changedPages ).ToString() );
            }

            if (newPages.Any())
            {
                logger.LogInformation( $"Sending NEW pages email containing {newPages.Count} items" );
                new EmailHelpers().SendEmail( $"Kingsleigh have {newPages.Count} NEW pages", dataManipulator.GetUris( newPages ).ToString() );
            }

            logger.LogInformation( $"Completed in {timedRun.Elapsed}!" );
        }

        private async Task CrawlPages()
        {
            PoliteWebCrawler crawler = SetupCrawler();
            var crawlResult = await crawler.CrawlAsync( new Uri( "https://www.kingsleighprimary.co.uk" ) );
        }

        private PoliteWebCrawler SetupCrawler()
        {
            var config = new CrawlConfiguration
            {
                MaxPagesToCrawl = 0,
                MaxLinksPerPage = 0,
                MinCrawlDelayPerDomainMilliSeconds = 1500,
            };

            var crawler = new PoliteWebCrawler( config, new CustomCrawlDecisionMaker(), null, null, null, null, null, null, null );

            crawler.PageCrawlCompleted += Crawler_PageCrawlCompleted;
            crawler.PageCrawlDisallowed += Crawler_PageCrawlDisallowed;
            return crawler;
        }

        private void Crawler_PageCrawlDisallowed( object sender, PageCrawlDisallowedArgs e )
        {
            logger.LogError( $"Unable to parse {e.PageToCrawl.Uri.AbsoluteUri} because {e.DisallowedReason}" );
        }

        private void Crawler_PageCrawlCompleted( object sender, PageCrawlCompletedArgs e )
        {
            string pageContent = e.CrawledPage.AngleSharpHtmlDocument.Body.OuterHtml;
            string hash = ComputeCheckSum( pageContent );

            string retrievedValue = null;
            bool foundSomething = previouslyFoundUris.TryGetValue( e.CrawledPage.Uri, out retrievedValue );
            if (!foundSomething)
            {
                //Did not find it previously.
                //Alert about the new page
                logger.LogInformation( $"NEW page: {e.CrawledPage.Uri}" );
                newPages[e.CrawledPage.Uri] = pageContent;
            }
            else if (retrievedValue != hash)
            {
                //Found it previously, but the content has changed
                //Alert about content changed
                logger.LogInformation( $"CHANGED page: {e.CrawledPage.Uri}" );
                changedPages[e.CrawledPage.Uri] = pageContent;
            }

            allUrisFound[e.CrawledPage.Uri] = hash;
        }

        private string ComputeCheckSum( string pageContent )
        {
            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = BitConverter.ToString(
                  md5.ComputeHash( Encoding.UTF8.GetBytes( pageContent ) )
                ).Replace( "-", String.Empty );
            }

            return hash;
        }
    }
}
