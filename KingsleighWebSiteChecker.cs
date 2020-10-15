using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MoSoftwareEnterprises.Kingsleigh
{
    public static class KingsleighWebSiteChecker
    {
        //Run twice a day
        [FunctionName("KingsleighWebSiteChecker")]
        public async static Task Run([TimerTrigger( "0 0 */12 * * *" )]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            await websitechangenotifier.Program.Main(null);
        }
    }
}
