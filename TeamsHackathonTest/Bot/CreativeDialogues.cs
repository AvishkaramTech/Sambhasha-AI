using Azure.AI.OpenAI;
using Azure;
using Microsoft.Bot.Builder.Dialogs;
using TeamsHackathonTest.Interfaces;
using Microsoft.Bot.Builder;
using System.Diagnostics;
using Microsoft.Bot.Schema;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TeamsHackathonTest.OpenAI;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TeamsHackathonTest.Models;
using TeamsHackathonTest.Helper;
using Microsoft.Graph.Models;
using TeamsHackathonTest.Card;
using AISuggestionAvish.Interfaces;
using MySqlX.XDevAPI;
using AISuggestionAvish.Helper;

namespace TeamsHackathonTest.Bot
{
    public class CreativeDialogues:ComponentDialog
    {
        private readonly IOpenAiClient _openAiClient;
        private readonly IConfiguration _configuration;
        private readonly IDbHelper _dbHelper;
        private readonly IGraphAuthHepler _authHepler;
        private readonly IProactiveHelper _proactiveHelper;
        private readonly IRunningTaskHelper _runningTaskHelper;
        private readonly IServiceProvider _serviceProvider;

        public CreativeDialogues(
            IOpenAiClient openAiClient,
            IConfiguration configuration,
            IDbHelper dbHelper,
            IGraphAuthHepler authHepler,
            IProactiveHelper proactiveHelper,
            IRunningTaskHelper runningTaskHelper,
            IServiceProvider serviceProvider
            )
        {
            _openAiClient = openAiClient;
            _configuration = configuration;
            _dbHelper = dbHelper;
            _authHepler = authHepler;
            _proactiveHelper = proactiveHelper;
            _runningTaskHelper = runningTaskHelper;
            _serviceProvider = serviceProvider;
        }
        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            TeamDetails teamDetails = null;
            UserActivityInfo activityInfo = new UserActivityInfo();
            if (innerDc.Context.Activity?.Value is JObject cardValue)
            {
                // Extract the user response from the adaptive card
                string parsedCardData = ParseTeamsChannelId(innerDc.Context.Activity.Value.ToString());
                try
                {
                    parsedCardData = StringHelper.RemoveHtmlTags(parsedCardData, _configuration["appName"]);
                    parsedCardData = parsedCardData.Replace(_configuration["appName"], string.Empty);
                    var response = JsonConvert.DeserializeObject<AdaptiveCardResponse>(parsedCardData);

                    // Process the response as needed
                    await ProcessResponseAsync(innerDc.Context, response);
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                if (innerDc.Context.Activity.Conversation.ConversationType.ToLower() != "groupchat")
                {
                    teamDetails = await TeamsInfo.GetTeamDetailsAsync(innerDc.Context);
                    activityInfo.TeamID = teamDetails.AadGroupId;
                    activityInfo.ChannelID = teamDetails.Id;
                    activityInfo.IsGroupChat = false;
                }
                else
                {
                    activityInfo.ChannelID = innerDc.Context.Activity.Conversation.Id;
                    activityInfo.IsGroupChat = true;
                }
                activityInfo.TenantID = ParseTenantID(innerDc.Context.Activity.ChannelData.ToString());
                activityInfo.ServiceURL = innerDc.Context.Activity.ServiceUrl;
                //await StartListeningToChannelAsync(innerDc.Context, activityInfo);
                await ProcessCommandAsync(activityInfo, StringHelper.RemoveHtmlTags(innerDc.Context.Activity.Text, _configuration["appName"]),innerDc.Context);
                return await innerDc.EndDialogAsync();                
            }
            return await innerDc.EndDialogAsync();
        }

        private async Task ProcessResponseAsync(ITurnContext turnContext, AdaptiveCardResponse response)
        {
            // Access the properties from the user response
            var userInput = response.Value;
            var additionalData = response.Message.Trim();
            if (!userInput)
                return;
            //getting initiated user info
            var user = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id);


            CompletionsOptions completionsOptions = new CompletionsOptions();
            completionsOptions.Temperature = 0;
            switch (response.Type)
            {
                case TypeEnum.CreatePlanner:
                    {
                        var _graphClient = await _authHepler.GetAPPOnlyAuthenticatedClientAsync(response.TenantID);
                        GraphHelper graphHelper = new GraphHelper(_graphClient, _configuration);
                        ChatMessageCollectionResponse chatMessageCollection = null;
                        if (string.IsNullOrEmpty(response.TeamID))
                        {

                            chatMessageCollection = await _graphClient.Chats[response.ChannelID].Messages.GetAsync();
                            var lastRun = await _dbHelper.GetLastRun(response.ChannelID);
                            chatMessageCollection.Value = chatMessageCollection.Value.Where(x => x.CreatedDateTime.Value.UtcDateTime > lastRun).ToList();
                        }
                        else
                        {
                            string deltaToken = await _dbHelper.GetDeltaTokenForChannel(response.ChannelID);
                            chatMessageCollection = await graphHelper.GetChannelMessageDelta(response.ChannelID, response.TeamID, deltaToken);
                            //update with new delta token
                            deltaToken = string.Empty;
                            deltaToken = StringHelper.GetDeltaToken(chatMessageCollection);
                            await _dbHelper.UpdateDeltaToken(deltaToken, response.ChannelID);
                        }
                        completionsOptions.Prompts.Add(PromptHelper.CreatePrompt(PromptType.Tasks, response.Message, _configuration["appName"], chatMessageCollection.Value));
                    }
                    break;
                case TypeEnum.summary:
                    {
                        var _graphClient = await _authHepler.GetAPPOnlyAuthenticatedClientAsync(response.TenantID);
                        GraphHelper graphHelper = new GraphHelper(_graphClient, _configuration);
                        ChatMessageCollectionResponse chatMessageCollection = null;
                        if (string.IsNullOrEmpty(response.TeamID))
                        {
                            chatMessageCollection = await _graphClient.Chats[response.ChannelID].Messages.GetAsync();
                            var lastRun = await _dbHelper.GetLastRun(response.ChannelID);
                            chatMessageCollection.Value = chatMessageCollection.Value.Where(x => x.CreatedDateTime.Value.UtcDateTime > lastRun).ToList();

                        }
                        else
                        {
                            string deltaToken = await _dbHelper.GetDeltaTokenForChannel(response.ChannelID);
                            chatMessageCollection = await graphHelper.GetChannelMessageDelta(response.ChannelID, response.TeamID, deltaToken);
                            //update with new delta token
                            //deltaToken = string.Empty;
                            //deltaToken = StringHelper.GetDeltaToken(chatMessageCollection);
                            //await _dbHelper.UpdateDeltaToken(deltaToken, response.ChannelID);

                        }

                        completionsOptions.Prompts.Add(PromptHelper.CreatePrompt(PromptType.Summerization, additionalData, _configuration["appName"], chatMessageCollection.Value));
                    }
                    break;
                case TypeEnum.suggestion:
                    {
                        completionsOptions.Prompts.Add(PromptHelper.CreatePrompt(PromptType.QuestionAnswer, _configuration["appName"], additionalData));
                    }
                    break;
                default:
                    throw new ArgumentException("Enum not defined");

            }

            completionsOptions.MaxTokens = 400;
            completionsOptions.User = user.Email;
            completionsOptions.ChoicesPerPrompt = 1;
            try
            {
                var client = _openAiClient.GetClient();
                OpenAiService aiService = new OpenAiService(client, _configuration);
                var aiResponse = await aiService.MakeRequest(completionsOptions);
                if (response.Type == TypeEnum.summary)
                {
                    UserActivityInfo activityInfo = new UserActivityInfo()
                    {
                        ChannelID = response.ChannelID,
                        TeamID = response.TeamID,
                        TenantID = response.TenantID,
                        IsGroupChat = string.IsNullOrEmpty(response.TeamID) ? true : false
                    };

                    var attachment = MessageFactory.Attachment(InteractiveCard.SummaryResponse(aiResponse, activityInfo));
                    await turnContext.SendActivityAsync(attachment);
                }
                else if (response.Type == TypeEnum.CreatePlanner)
                {
                    aiResponse = StringHelper.ButifyTaskList(aiResponse);
                    var tasksToAdd = JsonConvert.DeserializeObject<List<AISuggestedTask>>(aiResponse.Trim());
                    //creating graph client for application permission
                    var _graphClient = await _authHepler.GetAPPOnlyAuthenticatedClientAsync(response.TenantID);
                    //checking if planner plan exists
                    GraphHelper graphHelper = new GraphHelper(_graphClient, _configuration);
                    var plan = await graphHelper.CreateOrGetPlan(response, turnContext, user.UserPrincipalName);
                    foreach (var task in tasksToAdd)
                    {

                        await graphHelper.CreateTask(plan.Id, task.TaskName, task.Assignees);
                    }
                    if (tasksToAdd.Any())
                    {
                        await turnContext.SendActivityAsync("tasks added succesfully");
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("no tasks added succesfully");
                    }
                }
                else
                {
                    await turnContext.SendActivityAsync(aiResponse);
                }
                
            }
            catch (Exception ex)
            {

            }

        }
        private async Task ProcessCommandAsync(UserActivityInfo activityInfo,string option,ITurnContext turnContext)
        {
            if (string.IsNullOrEmpty(option))
            {
                return;
            }
            switch(option.ToLower())
            {
                case "start listening":
                    {
                        var isListening = await _dbHelper.GetListeningStatus(activityInfo.ChannelID);
                        if (isListening)
                        {
                            await turnContext.SendActivityAsync(_configuration["appName"] + " is listening");
                            return;
                        }
                        await turnContext.SendActivityAsync(_configuration["appName"] + " is now active in the conversation");
                        //var myBackgroundService = _serviceProvider.GetService<BackgroundWorker>();
                        //if (myBackgroundService != null)
                        //{
                        //    myBackgroundService.ServiceURL = activityInfo.ServiceURL;
                        //    myBackgroundService.TeamID = activityInfo.TeamID;
                        //    myBackgroundService.TenantID = activityInfo.TenantID;
                        //    myBackgroundService.IsGroupChat = activityInfo.IsGroupChat;
                        //    myBackgroundService.ChannelID = activityInfo.ChannelID;
                        //}
                        await _runningTaskHelper.StartListeningToChannelAsync(activityInfo);
                    }
                    break;
                case "stop listening":
                    {
                        await _dbHelper.SetIsStoppedListening(activityInfo.ChannelID);
                    }
                    break;
                case "summerize":
                    {
                        var user = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id);
                        CompletionsOptions completionsOptions = new CompletionsOptions();
                        completionsOptions.Temperature = 0;
                        var _graphClient = await _authHepler.GetAPPOnlyAuthenticatedClientAsync(activityInfo.TenantID);
                        GraphHelper graphHelper = new GraphHelper(_graphClient, _configuration);
                        ChatMessageCollectionResponse chatMessageCollection = null;
                        if (string.IsNullOrEmpty(activityInfo.TeamID))
                        {
                            chatMessageCollection = await _graphClient.Chats[activityInfo.ChannelID].Messages.GetAsync();
                            var lastRun = await _dbHelper.GetLastRun(activityInfo.ChannelID);
                            chatMessageCollection.Value = chatMessageCollection.Value.Where(x => x.CreatedDateTime.Value.UtcDateTime > lastRun).ToList();

                        }
                        else
                        {
                            string deltaToken = await _dbHelper.GetDeltaTokenForChannel(activityInfo.ChannelID);
                            chatMessageCollection = await graphHelper.GetChannelMessageDelta(activityInfo.ChannelID, activityInfo.TeamID, deltaToken);
                            //update with new delta token
                            //deltaToken = string.Empty;
                            //deltaToken = StringHelper.GetDeltaToken(chatMessageCollection);
                            //await _dbHelper.UpdateDeltaToken(deltaToken, response.ChannelID);

                        }

                        completionsOptions.Prompts.Add(PromptHelper.CreatePrompt(PromptType.Summerization, option, _configuration["appName"], chatMessageCollection.Value));
                        completionsOptions.MaxTokens = 400;
                        completionsOptions.User = user.Email;
                        completionsOptions.ChoicesPerPrompt = 1;
                        var client = _openAiClient.GetClient();
                        OpenAiService aiService = new OpenAiService(client, _configuration);
                        var aiResponse = await aiService.MakeRequest(completionsOptions);
                        
                            UserActivityInfo activity = new UserActivityInfo()
                            {
                                ChannelID = activityInfo.ChannelID,
                                TeamID = activityInfo.TeamID,
                                TenantID = activityInfo.TenantID,
                                IsGroupChat = string.IsNullOrEmpty(activityInfo.TeamID) ? true : false
                            };

                            var attachment = MessageFactory.Attachment(InteractiveCard.SummaryResponse(aiResponse, activity));
                            await turnContext.SendActivityAsync(attachment);
                        
                    }
                    break;
                default:
                    {
                        var user = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id);
                        var aiClient = _openAiClient.GetClient();
                        CompletionsOptions completionsOptions = new CompletionsOptions();
                        completionsOptions.Temperature = 0;
                        completionsOptions.Prompts.Add(PromptHelper.CreatePrompt(PromptType.QuestionAnswer, option, _configuration["appName"]));
                        completionsOptions.MaxTokens = 400;
                        completionsOptions.User = user.Email;
                        completionsOptions.ChoicesPerPrompt = 1;
                        OpenAiService aiService = new OpenAiService(aiClient, _configuration);
                        var aiResponse = await aiService.MakeRequest(completionsOptions);
                        await turnContext.SendActivityAsync(MessageFactory.Text(aiResponse));
                    }
                    break;
            }
        }

       
        private string ParseTeamsChannelId(string jsonString)
        {
            JObject json = JObject.Parse(jsonString);
            return json["response"]?.ToString();
            // _teamID = json["teamsTeamId"].ToString();            
        }
        private string ParseTenantID(string jstring)
        {
            JObject json = JObject.Parse(jstring);
            return (string)json["tenant"]["id"];
        }
    }
}
