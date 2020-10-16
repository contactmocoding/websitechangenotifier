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
                logger.LogInformation( $"Reading previous results from  {fileName}." );
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

        internal async Task ExportData( string fileNameToExport, Dictionary<Uri, string> itemToExport )
        {
            logger.LogInformation( $"Logging to {fileNameToExport}" );
            StringBuilder fileContents = new StringBuilder();
            foreach (KeyValuePair<Uri, string> kvp in itemToExport.OrderBy( t => t.Key.AbsoluteUri ))
            {
                fileContents.AppendLine( $"{kvp.Key}, {kvp.Value}" );
            }

            try
            {
                logger.LogInformation( $"Trying to write out {fileNameToExport}.");
                await System.IO.File.WriteAllTextAsync( fileNameToExport, fileContents.ToString() );
                logger.LogInformation( $"Finished writing out {fileNameToExport}." );
            }
            catch(Exception ex)
            {
                logger.LogError( $"Unable to write out {fileNameToExport}. Error was:", ex );
            }
        }

        internal async Task StoreCurrentResults( Dictionary<Uri, string> allUrisFound )
        {
            await ExportData( fileName, allUrisFound );
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