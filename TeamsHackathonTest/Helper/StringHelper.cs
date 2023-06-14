using Azure;
using Microsoft.Graph.Models;
using System.Text.RegularExpressions;

namespace TeamsHackathonTest.Helper
{
    public class StringHelper
    {
        public static string RemoveHtmlTags(string input,string appName)
        {
            // Remove HTML tags using regular expressions
            string result =  Regex.Replace(input, "<.*?>", string.Empty);
            result = result.Replace(appName, string.Empty);
            result = result.Trim();
            return result;
        }
        public static string GetPlanName(string teamID,string appName)
        {
            Guid existingGuid = Guid.Parse(teamID);
            string firstPart = existingGuid.ToString().Split('-')[0];
            string planName = appName + "-" + firstPart;
            return planName;
        }
        public static string[] ConvertBulletToString(string str)
        {
            string[] tasks = str.Split(new string[] { "• ", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Trim any leading or trailing whitespaces
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = tasks[i].Trim();
            }
            return tasks;
        }
        public static string GetDeltaToken(ChatMessageCollectionResponse response)
        {
            string deltaToken = null;
            string skipTokenURL = string.Empty;
            if(response.AdditionalData.TryGetValue("@odata.deltaLink", out object tokenLink))
            {
                skipTokenURL = (string)tokenLink;
            }
            else if (!string.IsNullOrEmpty(response.OdataNextLink))
            {
                skipTokenURL = response.OdataNextLink;
            }
            else
            {
                return skipTokenURL;
            }
            int deltaTokenStartIndex = skipTokenURL.IndexOf("$skiptoken=");
            if (deltaTokenStartIndex != -1)
            {
                deltaTokenStartIndex += "$skiptoken=".Length;
                int deltaTokenEndIndex = skipTokenURL.Length;
                if (deltaTokenEndIndex != -1)
                {
                    deltaToken = skipTokenURL.Substring(deltaTokenStartIndex, deltaTokenEndIndex - deltaTokenStartIndex);
                }
            }
            else
            {
                deltaTokenStartIndex = skipTokenURL.IndexOf("$deltatoken=");
                if (deltaTokenStartIndex != -1)
                {
                    deltaTokenStartIndex += "$deltatoken=".Length;
                    int deltaTokenEndIndex = skipTokenURL.Length;
                    if (deltaTokenEndIndex != -1)
                    {
                        deltaToken = skipTokenURL.Substring(deltaTokenStartIndex, deltaTokenEndIndex - deltaTokenStartIndex);
                    }
                }
            }
            

            return deltaToken;
        }
        public static string ButifyTaskList(string taskList)
        {
            if (taskList.Contains("Tasks List:"))
            {
                taskList = taskList.Replace("Tasks List:",string.Empty);
                taskList = Regex.Replace(taskList, @"\t|\n|\r", "");
            }
            taskList = taskList.Trim();
            return taskList;
        }
    }
}
