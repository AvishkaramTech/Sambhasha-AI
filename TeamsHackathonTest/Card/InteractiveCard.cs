using AdaptiveCards;
using Microsoft.Bot.Schema;
using Microsoft.Graph.Models;
using Newtonsoft.Json.Linq;
using TeamsHackathonTest.Helper;
using TeamsHackathonTest.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Attachment = Microsoft.Bot.Schema.Attachment;

namespace TeamsHackathonTest.Card
{
    public  class InteractiveCard
    {
        public static Attachment UserInteractionCard(string userMessage,TypeEnum typeEnum,UserActivityInfo activityInfo)
        {
            var adaptiveCard = new AdaptiveCard();
            adaptiveCard.Body.Add(new AdaptiveTextBlock
            {
                Text = (typeEnum==TypeEnum.summary)?"Hey! Want me to summerize todays conversation?":"Hey!! Want help?",
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder,
                Wrap = true
            });
            
                // Create an action set for the options
                var actionSet = new AdaptiveActionSet();

                // Add the "Yes" option
                var yesAction = new AdaptiveSubmitAction
                {
                    Title = "Yes",
                    Data = new JObject { { "response", "{\"value\":true,\"message\":\"" + userMessage + "\",\"type\":\"" + typeEnum + "\",\"channelID\":\"" + activityInfo.ChannelID + "\",\"teamID\":\"" + activityInfo.TeamID + "\",\"tenantID\":\"" + activityInfo.TenantID + "\"}" } }
                };
                actionSet.Actions.Add(yesAction);

                // Add the "No" option
                var noAction = new AdaptiveSubmitAction
                {
                    Title = "No",
                    Data = new JObject { { "response", "{\"value\":false,\"message\":\"" + userMessage + "\",\"type\":" + typeEnum + "}" } }
                };
                actionSet.Actions.Add(noAction);

                // Add the action set to the card
                adaptiveCard.Body.Add(actionSet);
            

            // Create a message activity
            var message = Activity.CreateMessageActivity();

            // Attach the adaptive card to the message
            message.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = adaptiveCard
                }
            };
            var attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };
            return attachment;
        }

        public static Attachment SummaryResponse(string response,UserActivityInfo activityInfo)
        {
            var adaptiveCard = new AdaptiveCard();
            var topics = StringHelper.ConvertBulletToString(response);
            foreach (var topic in topics)
            {
                adaptiveCard.Body.Add(new AdaptiveTextBlock() 
                {
                    Text = topic,
                    Size = AdaptiveTextSize.Medium,
                    Weight = AdaptiveTextWeight.Bolder,
                    Wrap = true
                });
            }
            if (!activityInfo.IsGroupChat)
            {
                var yesAction = new AdaptiveSubmitAction
                {
                    Title = "Add to planner.",
                    Data = new JObject { { "response", "{\"value\":true,\"message\":\"" + response + "\",\"type\":\"" + TypeEnum.CreatePlanner + "\",\"channelID\":\"" + activityInfo.ChannelID + "\",\"teamID\":\"" + activityInfo.TeamID + "\",\"tenantID\":\"" + activityInfo.TenantID + "\"}" } }
                };
                var actionSet = new AdaptiveActionSet();
                actionSet.Actions.Add(yesAction);
                adaptiveCard.Body.Add(actionSet);
            }
            var message = Activity.CreateMessageActivity();

            // Attach the adaptive card to the message
            message.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = adaptiveCard
                }
            };
            var attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };
            return attachment;
        }
    }
}
