using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace websitechangenotifier
{
    public class ExternalDataManipulator
    {
        private const string fileName = "AllUrisFound.txt";
        ILogger logger;
        public ExternalDataManipulator( ILogger logger )
        {

        }

        internal async Task<Dictionary<Uri, string>> LoadPreviousResults()
        {
            Dictionary<Uri, string> previousResults = new Dictionary<Uri, string>();

            if (System.IO.File.Exists( fileName ))
            {
                //Load in the previous results
                var previousItems = await System.IO.File.ReadAllLinesAsync( fileName );
                foreach (string lineItem in previousItems)
                {
                    var lineData = lineItem.Split( ',' );
                    previousResults[new Uri( lineData[0].Trim() )] = lineData[1].Trim();
                }
            }
            return previousResults;
        }

        internal async Task ExportData( string fileName, Dictionary<Uri, string> itemToExport )
        {
            logger.LogInformation( $"Logging to {fileName}" );
            StringBuilder fileContents = new StringBuilder();
            foreach (KeyValuePair<Uri, string> kvp in itemToExport.OrderBy( t => t.Key.AbsoluteUri ))
            {
                fileContents.AppendLine( $"{kvp.Key}, {kvp.Value}" );
            }

            try
            {
                await System.IO.File.WriteAllTextAsync( fileName, fileContents.ToString() );
            }
            catch(Exception ex)
            {
                logger.LogError( $"Unable to write out {fileName}. Error was:", ex );
            }
        }

        internal void StoreCurrentResults( Dictionary<Uri, string> allUrisFound )
        {
            ExportData( fileName, allUrisFound );
        }

        internal StringBuilder GetUris( Dictionary<Uri, string> itemToExport )
        {
            StringBuilder fileContents = new StringBuilder();
            foreach (KeyValuePair<Uri, string> kvp in itemToExport.OrderBy( t => t.Key.AbsoluteUri ))
            {
                fileContents.AppendLine( $"{kvp.Key}" );
            }
            return fileContents;
        }
    }
}