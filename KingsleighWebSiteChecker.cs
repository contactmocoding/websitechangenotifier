using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MoSoftwareEnterprises.Kingsleigh
{
    public static class KingsleighWebSiteChecker
    {
        //"0 0 */6 * * *"
        [FunctionName("KingsleighWebSiteChecker")]
        public async static Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            await websitechangenotifier.Program.Main(null);
        }
    }
}
