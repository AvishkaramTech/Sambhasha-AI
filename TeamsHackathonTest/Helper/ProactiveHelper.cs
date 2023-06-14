using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Builder;
using Microsoft.Graph.Models;
using TeamsHackathonTest.Models;

namespace TeamsHackathonTest.Helper
{
    public class ProactiveHelper:IProactiveHelper
    {
        private readonly MicrosoftAppCredentials _appCredentials;
        public ProactiveHelper(IConfiguration configuration)
        {
            _appCredentials = new MicrosoftAppCredentials(configuration["BOT_ID"], configuration["BOT_PASSWORD"]);
        }
        public async Task<string> SendInTeams(string serviceUrl, Microsoft.Bot.Schema.Attachment attachment,UserActivityInfo activityInfo)
        {
            try
            {
                var connectorClient = new ConnectorClient(new Uri(serviceUrl),_appCredentials);
                var newMessage = Activity.CreateMessageActivity();
                newMessage.Type = ActivityTypes.Message;
                newMessage.Conversation = new ConversationAccount(id: activityInfo.ChannelID);
                IMessageActivity activity = null;

                activity = MessageFactory.Attachment(attachment);

                activity.Summary = $"Message Received LTB"; // Ensure that the summary text is populated so the toast notifications aren't generic text.

                IMessageActivity message = Activity.CreateMessageActivity();
                message.Attachments.Add(attachment);

                message.Conversation = new ConversationAccount { ConversationType = activityInfo.IsGroupChat?"groupChat":"teams", Id = activityInfo.ChannelID};
                message.ChannelData = new TeamsChannelData { Tenant = new TenantInfo { Id = activityInfo.TenantID } };
                //ConversationParameters parameters = new ConversationParameters
                //{
                //    IsGroup = false,
                //    //Members = new[] { new ChannelAccount("nischit.gautam@avishkaram.com"), new ChannelAccount("elish.gyamaru@avishkaram.com") },
                //    ChannelData = new TeamsChannelData
                //    {

                //        //Team = new TeamInfo { Id= "1093e283-a646-499b-bdf9-cb075a7f8cf8" },
                //        Channel = new ChannelInfo { Id = channelID },
                //    },
                //    //Activity = MessageFactory.Text("hello testing nischit"),
                //    Activity = activity as Activity
                //};
                try
                {
                    await connectorClient.Conversations.SendToConversationAsync((Activity)message);
                    return ("success");
                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex, ex.Message);
                    return ("Failed");
                }
            }
            catch
            {
                return ("error");

            }

        }
    }
}
