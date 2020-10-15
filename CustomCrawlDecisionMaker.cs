using System;
using Abot2.Core;
using Abot2.Poco;
using System.Linq;

namespace websitechangenotifier
{
    public class CustomCrawlDecisionMaker : CrawlDecisionMaker
    {
        public override CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            CrawlDecision decision = base.ShouldCrawlPage(pageToCrawl, crawlContext);
            if (decision.Allow && !pageToCrawl.IsRoot)
            {
                if (pageToCrawl.Uri.AbsoluteUri.EndsWith("/parents/letters/") //https://www.kingsleighprimary.co.uk/parents/letters/
                || pageToCrawl.Uri.AbsoluteUri.EndsWith("/blog/") //https://www.kingsleighprimary.co.uk/blog/
                || pageToCrawl.Uri.AbsoluteUri.Contains("/events/?kps-month=202")
                || pageToCrawl.Uri.AbsoluteUri.EndsWith("/parents/")
                || pageToCrawl.Uri.AbsoluteUri.EndsWith("/parents/newsletters/") //https://www.kingsleighprimary.co.uk/parents/newsletters/
                || pageToCrawl.Uri.AbsoluteUri.EndsWith("/blog-category/reception/")//https://www.kingsleighprimary.co.uk/blog-category/reception/
                || pageToCrawl.Uri.AbsoluteUri.EndsWith(".pdf")//Allow any upload file
               || pageToCrawl.Uri.AbsoluteUri.EndsWith("/policies/") //https://www.kingsleighprimary.co.uk/policies/
               || pageToCrawl.Uri.AbsoluteUri.EndsWith("/classes/reception/") //https://www.kingsleighprimary.co.uk/classes/reception/
                 )
                {
                    //Only include these
                }
                else
                {
                    decision = new CrawlDecision { Allow = false, Reason = "Ignoring intentionally" };
                }
            }

            return decision;
        }
    }
}