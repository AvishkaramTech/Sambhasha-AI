using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using TeamsHackathonTest.Interfaces;

namespace TeamsHackathonTest.Bot
{
    public class InteractiveBot<T> : TeamsActivityHandler where T:Dialog
    {
        private readonly T _dialog;
        private readonly ConversationState _conversationState;
        private readonly IDbHelper _dbHelper;
        private readonly IConfiguration _configuration;

        public InteractiveBot(
            T dialog,
            ConversationState conversationState,
            IDbHelper dbHelper,
            IConfiguration configuration
            )
        {
            _dialog = dialog;
            _conversationState = conversationState;
            _dbHelper = dbHelper;
            _configuration = configuration;
        }
        protected override Task OnMessageUpdateActivityAsync(ITurnContext<IMessageUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.MembersAdded != null)
            {
                foreach (var member in turnContext.Activity.MembersAdded)
                {
                    bool isTeamsChannel = false;
                    if (member.Id == turnContext.Activity.Recipient.Id)
                    {
                        string channelID = string.Empty;
                        if (turnContext.Activity.Conversation?.ConversationType?.ToLower() != "groupchat")
                        {
                            var teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext);
                            channelID = teamDetails.Id;
                            isTeamsChannel = true;
                        }
                        else
                        {
                            channelID = turnContext.Activity.Conversation.Id;
                            isTeamsChannel = false;
                        }
                        await _dbHelper.InsertNewConversationChannel(string.Empty, channelID, isTeamsChannel);
                        await turnContext.SendActivityAsync(_configuration["appName"] + " is installed");
                        // Handle new member added to the conversation
                    }
                }
            }
            if (turnContext.Activity.MembersRemoved != null)
            {
                foreach (var member in turnContext.Activity.MembersAdded)
                {
                    if (member.Id == turnContext.Activity.Recipient.Id)
                    {
                        string channelID = string.Empty;
                        if (turnContext.Activity.Conversation.ConversationType.ToLower() != "groupchat")
                        {
                            var teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext);
                            channelID = teamDetails.Id;
                        }
                        else
                        {
                            channelID = turnContext.Activity.Conversation.Id;
                        }
                        await _dbHelper.DeleteExistingConversationChannel(channelID);
                        // Handle new member added to the conversation
                    }
                }
            }
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the Dialog with the new message Activity.
            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Conversation.ConversationType == "personal" && turnContext.Activity.Name != "composeExtension/query")
            {
                await turnContext.SendActivityAsync(new Activity { Type = ActivityTypes.Typing }, cancellationToken);

            }
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
