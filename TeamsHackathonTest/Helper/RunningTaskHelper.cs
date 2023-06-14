using AISuggestionAvish.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Graph.Models;
using Microsoft.Graph;
using System.Globalization;
using TeamsHackathonTest.Card;
using TeamsHackathonTest.Helper;
using TeamsHackathonTest.Models;
using TeamsHackathonTest.Interfaces;
using StringHelper = TeamsHackathonTest.Helper.StringHelper;

namespace AISuggestionAvish.Helper
{
    public class RunningTaskHelper:IRunningTaskHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IGraphAuthHepler _authHepler;
        private readonly IDbHelper _dbHelper;
        private readonly IProactiveHelper _proactiveHelper;

        public RunningTaskHelper(IConfiguration configuration,IGraphAuthHepler graphAuthHepler,IDbHelper dbHelper,IProactiveHelper proactiveHelper)
        {
            _configuration = configuration;
            _authHepler = graphAuthHepler;
            _dbHelper = dbHelper;
            _proactiveHelper = proactiveHelper;
        }

        public async Task StartListeningToChannelAsync(UserActivityInfo activityInfo)
        {
            try
            {
                var _graphClient = await _authHepler.GetAPPOnlyAuthenticatedClientAsync(activityInfo.TenantID);
                // var userInfo = await _graphClient.Users["nischit@avishdev1.onmicrosoft.com"].GetAsync().ConfigureAwait(false);
                GraphHelper graphHelper = new GraphHelper(_graphClient, _configuration);
                var deltaTokenForChannel = await _dbHelper.GetDeltaTokenForChannel(activityInfo.ChannelID);
                ChatMessageCollectionResponse response = null;
                await _dbHelper.SetLastRun(activityInfo.ChannelID);
                await _dbHelper.SetCurrentRun(activityInfo.ChannelID);
                if (activityInfo.IsGroupChat)
                {
                    //response = await graphHelper.GetChatMessageDelta(activityInfo.ChannelID, deltaTokenForChannel);
                }
                else
                {
                    response = await graphHelper.GetChannelMessageDelta(activityInfo.ChannelID, activityInfo.TeamID, deltaTokenForChannel);
                    var deltaToken = StringHelper.GetDeltaToken(response);
                    await _dbHelper.UpdateDeltaToken(deltaToken, activityInfo.ChannelID);
                }
                bool continueRun = true;
                int blankRunCount = 0;                
                while (continueRun)
                {
                    try
                    {
                        var lastSacnnedDate = await _dbHelper.GetCurrentRun(activityInfo.ChannelID);
                        //var lastRun = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5));
                        
                        ChatMessageCollectionResponse chatMessageCollection = null;
                        if (activityInfo.IsGroupChat)
                        {
                            chatMessageCollection = await _graphClient.Chats[activityInfo.ChannelID].Messages.GetAsync();
                        }
                        else
                        {

                            chatMessageCollection = await _graphClient.Teams[activityInfo.TeamID].Channels[activityInfo.ChannelID].Messages.GetAsync().ConfigureAwait(false);
                        }
                        var messages = chatMessageCollection.Value.Where(x => x.CreatedDateTime.Value.UtcDateTime > lastSacnnedDate);
                        await _dbHelper.SetCurrentRun(activityInfo.ChannelID);
                        //threads.Serialize();
                        foreach (var thread in messages)
                        {
                            if (thread.Body.Content.ToLower().Contains("help"))
                            {
                                await _proactiveHelper.SendInTeams(activityInfo.ServiceURL, InteractiveCard.UserInteractionCard(thread.Body.Content, TypeEnum.suggestion, activityInfo), activityInfo);
                            }
                            if (thread.Body.Content.ToLower().Contains("summerize"))
                            {
                                await _proactiveHelper.SendInTeams(activityInfo.ServiceURL, InteractiveCard.UserInteractionCard(thread.Body.Content, TypeEnum.summary, activityInfo), activityInfo);
                                //await _proactiveHelper.SendInTeams(activityInfo.ServiceURL, InteractiveCard.UserInteractionCard(thread.Body.Content, TypeEnum.summary, activityInfo),activityInfo.ChannelID);
                            }
                        }
                        continueRun = await _dbHelper.GetListeningStatus(activityInfo.ChannelID);
                        if (!continueRun)
                            break;
                        if (messages.Count() == 0)
                        {
                            blankRunCount++;
                            if (blankRunCount == 60)//one minute inactive
                            {
                                continueRun = false;
                                await _dbHelper.SetIsStoppedListening(activityInfo.ChannelID);
                            }
                        }
                        await Task.Delay(TimeSpan.FromSeconds(8)); // Delay between polling requests
                        
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }
            catch (ServiceException e)
            {
                
            }
        }
    }
}
