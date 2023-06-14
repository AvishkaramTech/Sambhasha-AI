using Microsoft.Graph.Models;
using System.Security.Cryptography.Xml;
using System.Text;
using TeamsHackathonTest.Helper;

namespace TeamsHackathonTest.OpenAI
{
    public static class PromptHelper
    {
        public static string CreatePrompt(PromptType promptType,string enquiry,string appName, List<ChatMessage> chatMessage=null)
        {
            if(enquiry.Length<1)
                return string.Empty;
            StringBuilder stringBuilder = new StringBuilder();            
            switch (promptType)
            {
                case PromptType.Intent:
                    stringBuilder.Append("What is the user's intent.select one from this list [login,logout,matter,case,upcoming deadlines,upcoming appointments,my day,files,my recent files,create-newmatter,matter deadlines,analytics,unknown]\"\"\"" + $" User: {enquiry},Intent:\r\n");
                    break;
                case PromptType.QuestionAnswer:
                    stringBuilder.Append(enquiry);
                    break;
                case PromptType.TagMyFav:
                    stringBuilder.Append($"Topic:I'm looking for suggested links on the context of {enquiry}. Can you provide me with relevant articles, blog posts, or resources that I can explore? Ignore message where user is asking for summary");
                    break;
                case PromptType.Summerization:
                    stringBuilder.Append(GenerateOneMonthConversationPrompt(chatMessage, appName));
                    break;
                case PromptType.Tasks:
                    stringBuilder.Append($"Create a list of tasks that can be added in task managemnet tools task subject from the conversation.\"\"\" {GenerateOneMonthConversationPrompt(chatMessage, appName, true)}");
                    break;
                default:
                    stringBuilder.Append(enquiry);
                    break;
            }
            return stringBuilder.ToString();
        }
        public static string GenerateOneMonthConversationPrompt(List<ChatMessage> messageCollection,string appName,bool forTask=false)
        {
            //var todaysMessages = messageCollection.Value.Where(x => x.CreatedDateTime.Value.UtcDateTime.Date == DateTime.UtcNow.Date).OrderBy(y=>y.CreatedDateTime.Value.UtcDateTime);
            StringBuilder builder = new StringBuilder("Conversation:\r\n");
            foreach (var message in messageCollection)
            {
                if (message.From?.User != null)
                {
                    if (forTask)
                    {
                        builder.AppendLine("from id="+message.From.User.Id + ": " + StringHelper.RemoveHtmlTags(message.Body.Content, appName));
                    }
                    else
                    {
                        builder.AppendLine(message.From.User.DisplayName + ": " + StringHelper.RemoveHtmlTags(message.Body.Content, appName));
                    }
                }
            }
            if (forTask)
            {
                builder.AppendLine("Please generate tasks list in JSON format like[{assignees:[list of from id should be string in \"\"],taskName:example task name}] from this conversation. Please create task name in well understandable language in english. And don't include the conversation where user is asking to summerize the conversation.");
            }
            else
            {
                builder.AppendLine("Please generate a summary of the conversation.Focus on the key points discussed.Provide a concise and coherent summary in bullet points.And don't include the conversation where user is asking to summerize the conversation");
            }
            return builder.ToString();
        }
    }
    
    public enum PromptType
    {
        Intent,
        QuestionAnswer,
        TagMyFav,
        Summerization,
        Tasks
    }
}
