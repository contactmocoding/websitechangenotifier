using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace websitechangenotifier
{
    public class ExternalDataManipulator
    {
        internal async Task<Dictionary<Uri, string>> LoadPreviousResults()
        {
            Dictionary<Uri, string> previousResults = new Dictionary<Uri, string>();
                        //Load in the previous results
            var previousItems = await System.IO.File.ReadAllLinesAsync(@"AllUrisFound.txt");
            foreach (string lineItem in previousItems)
            {
                var lineData = lineItem.Split(',');
                previousResults[new Uri(lineData[0])] = lineData[1];
            }
            return previousResults;
        }
       
        internal async Task ExportData(string fileName, Dictionary<Uri,string> itemToExport)
        {
            StringBuilder fileContents = new StringBuilder();
            foreach (KeyValuePair<Uri, string> kvp in itemToExport.OrderBy(t => t.Key.AbsoluteUri))
            {
                fileContents.AppendLine($"{kvp.Key}, {kvp.Value}");
            }
            await System.IO.File.WriteAllTextAsync(fileName, fileContents.ToString());
        }

        internal StringBuilder GetUris( Dictionary<Uri,string> itemToExport)
        {
            StringBuilder fileContents = new StringBuilder();
            foreach (KeyValuePair<Uri, string> kvp in itemToExport.OrderBy(t => t.Key.AbsoluteUri))
            {
                fileContents.AppendLine($"{kvp.Key}");
            }
            return fileContents;
        }
    }
}