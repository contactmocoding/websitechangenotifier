using System.Threading.Tasks;

namespace websitechangenotifier
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await new KingsleighCrawler().RunCrawler();
        }

    }
}
