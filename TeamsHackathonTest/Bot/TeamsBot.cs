//using Azure.AI.OpenAI;
//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.Teams;
//using Microsoft.Bot.Schema;
//using Microsoft.Bot.Schema.Teams;
//using Microsoft.Graph;
//using Microsoft.Graph.Models;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System.Globalization;
//using TeamsHackathonTest.Card;
//using TeamsHackathonTest.Helper;
//using TeamsHackathonTest.Interfaces;
//using TeamsHackathonTest.Models;
//using TeamsHackathonTest.OpenAI;
//using StringHelper = TeamsHackathonTest.Helper.StringHelper;
//using TeamInfo = Microsoft.Bot.Schema.Teams.TeamInfo;

//namespace TeamsHackathonTest.Bot
//{
//    public class TeamsBot: TeamsActivityHandler
//    {
//        public TeamsBot(IGraphAuthHepler authHepler,IProactiveHelper proactiveHelper,IOpenAiClient openAiClient,IFileHelper fileHelper,IConfiguration  configuration,IDbHelper dbHelper)
//        {
//            _authHepler = authHepler;
//            _proactiveHelper = proactiveHelper;
//            _openAiClient = openAiClient;
//            _fileHelper = fileHelper;
//            _configuration = configuration;
//            _dbHelper = dbHelper;
//        }
            
//        private readonly IGraphAuthHepler _authHepler;
//        private readonly IProactiveHelper _proactiveHelper;
//        private readonly IOpenAiClient _openAiClient;
//        private readonly IFileHelper _fileHelper;
//        private readonly IConfiguration _configuration;
//        private readonly IDbHelper _dbHelper;

//        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
//        {
           
//            TeamDetails teamDetails = null;
//            UserActivityInfo activityInfo = new UserActivityInfo();
//            if (turnContext.Activity?.Value is JObject cardValue)
//            {
//                // Extract the user response from the adaptive card
//                string parsedCardData = ParseTeamsChannelId(turnContext.Activity.Value.ToString());
//                try
//                {
//                    parsedCardData = StringHelper.RemoveHtmlTags(parsedCardData, _configuration["appName"]);
//                    parsedCardData = parsedCardData.Replace(_configuration["appName"], string.Empty);
//                    var response = JsonConvert.DeserializeObject<AdaptiveCardResponse>(parsedCardData);

//                    // Process the response as needed
//                    await ProcessResponseAsync(turnContext, response);
//                }
//                catch (Exception ex)
//                {

//                }
//            }
//            else
//            {
//                if (turnContext.Activity.Conversation.ConversationType.ToLower() != "groupchat")
//                {
//                    teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext);
//                    activityInfo.TeamID = teamDetails.AadGroupId;
//                    activityInfo.ChannelID = teamDetails.Id;
//                    activityInfo.IsGroupChat = false;
//                }
//                else
//                {
//                    activityInfo.ChannelID = turnContext.Activity.Conversation.Id;
//                    activityInfo.IsGroupChat = true;
//                }
//                activityInfo.TenantID = ParseTenantID(turnContext.Activity.ChannelData.ToString());
//                activityInfo.ServiceURL = turnContext.Activity.ServiceUrl;
//                await StartListeningToChannelAsync(turnContext,activityInfo);
//            }

//        }

        
//        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken) {
//            var aas = teamInfo;
//            foreach (var member in membersAdded)
//            {
//                if (member.Id == turnContext.Activity.Recipient.Id)
//                {
//                    // Bot was added to a channel or chat
//                    // Perform any necessary initialization or welcome message here

//                    // Store the conversation reference for future use
//                    var conversationReference = turnContext.Activity.GetConversationReference();

//                    await _dbHelper.InsertNewConversationChannel(string.Empty, teamInfo.Id);
//                    // Send a welcome message
//                    await turnContext.SendActivityAsync("Hello! I'm your Teams bot.", cancellationToken: cancellationToken);
//                }
//            }
//        }
//        public string ParseTeamsChannelId(string jsonString)
//        {
//            JObject json = JObject.Parse(jsonString);
//           return json["response"]?.ToString();
//           // _teamID = json["teamsTeamId"].ToString();            
//        }
//        private string ParseTenantID(string jstring)
//        {
//            JObject json = JObject.Parse(jstring);
//            return (string)json["tenant"]["id"];
//        }
//        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
//        {
            
//            if (turnContext.Activity.MembersAdded != null)
//            {
//                foreach (var member in turnContext.Activity.MembersAdded)
//                {
//                    if (member.Id == turnContext.Activity.Recipient.Id)
//                    {
//                        string channelID = string.Empty;
//                        if (turnContext.Activity.Conversation.ConversationType.ToLower() != "groupchat")
//                        {
//                            var teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext);                            
//                            channelID = teamDetails.Id;                            
//                        }
//                        else
//                        {
//                            channelID = turnContext.Activity.Conversation.Id;                            
//                        }
//                        await _dbHelper.InsertNewConversationChannel(string.Empty, channelID);
//                        // Handle new member added to the conversation
//                    }
//                }
//            }
//            if(turnContext.Activity.MembersRemoved != null)
//            {
//                foreach (var member in turnContext.Activity.MembersAdded)
//                {
//                    if (member.Id == turnContext.Activity.Recipient.Id)
//                    {
//                        string channelID = string.Empty;
//                        if (turnContext.Activity.Conversation.ConversationType.ToLower() != "groupchat")
//                        {
//                            var teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext);
//                            channelID = teamDetails.Id;
//                        }
//                        else
//                        {
//                            channelID = turnContext.Activity.Conversation.Id;
//                        }
//                        await _dbHelper.DeleteExistingConversationChannel(channelID);
//                        // Handle new member added to the conversation
//                    }
//                }
//            }

//        }

//        public async Task StartListeningToChannelAsync(ITurnContext<IMessageActivity> turnContext,UserActivityInfo activityInfo)
//        {
//            try
//            {
//                var _graphClient = await _authHepler.GetAPPOnlyAuthenticatedClientAsync(activityInfo.TenantID);
//                // var userInfo = await _graphClient.Users["nischit@avishdev1.onmicrosoft.com"].GetAsync().ConfigureAwait(false);
//                GraphHelper graphHelper = new GraphHelper(_graphClient,_configuration);
//                var deltaTokenForChannel = await _dbHelper.GetDeltaTokenForChannel(activityInfo.ChannelID);
//                ChatMessageCollectionResponse response = null;
//                if (activityInfo.IsGroupChat)
//                {
//                    //response = await graphHelper.GetChatMessageDelta(activityInfo.ChannelID, deltaTokenForChannel);
//                }
//                else
//                {
//                    response = await graphHelper.GetChannelMessageDelta(activityInfo.ChannelID, activityInfo.TeamID, deltaTokenForChannel);
//                    var deltaToken = StringHelper.GetDeltaToken(response);
//                    await _dbHelper.UpdateDeltaToken(deltaToken, activityInfo.ChannelID);
//                }
//                bool continueRun = true;
//                int blankRunCount = 0;
//                await turnContext.SendActivityAsync("TestAi is now part your conversation!");
//                while (continueRun)
//                {
//                    try {
//                        var lastSacnnedDate = await _fileHelper.ReadFromFile();
//                        var lastRun = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5));
//                        if (!string.IsNullOrEmpty(lastSacnnedDate))
//                            lastRun = DateTime.Parse(lastSacnnedDate.Trim(), null, DateTimeStyles.RoundtripKind);
//                        ChatMessageCollectionResponse chatMessageCollection = null;
//                        if (activityInfo.IsGroupChat)
//                        {
//                            chatMessageCollection = await _graphClient.Chats[activityInfo.ChannelID].Messages.GetAsync();
//                        }
//                        else
//                        {

//                            chatMessageCollection = await _graphClient.Teams[activityInfo.TeamID].Channels[activityInfo.ChannelID].Messages.GetAsync().ConfigureAwait(false);
//                        }
//                        var messages = chatMessageCollection.Value.Where(x => x.CreatedDateTime.Value.UtcDateTime > lastRun);
//                        await _fileHelper.WriteToFile();
//                        //threads.Serialize();
//                        foreach (var thread in messages)
//                        {
//                            if (thread.Body.Content.ToLower().Contains("help"))
//                            {
//                                await _proactiveHelper.SendInTeams(activityInfo.ServiceURL, InteractiveCard.UserInteractionCard(thread.Body.Content, TypeEnum.suggestion, activityInfo),activityInfo);
//                            }
//                            if (thread.Body.Content.ToLower().Contains("summerize"))
//                            {
//                                await _proactiveHelper.SendInTeams(activityInfo.ServiceURL, InteractiveCard.UserInteractionCard(thread.Body.Content, TypeEnum.summary, activityInfo),activityInfo);
//                                //await _proactiveHelper.SendInTeams(activityInfo.ServiceURL, InteractiveCard.UserInteractionCard(thread.Body.Content, TypeEnum.summary, activityInfo),activityInfo.ChannelID);
//                            }
//                        }

//                        if (messages.Count() == 0)
//                        {
//                            blankRunCount++;
//                            if (blankRunCount == 60)//one minute inactive
//                            {
//                                continueRun = false;
//                            }
//                        }
//                        await Task.Delay(TimeSpan.FromSeconds(5)); // Delay between polling requests
//                    }
//                    catch (Exception ex)
//                    {

//                    }
//                }
                
//            }
//            catch(ServiceException e) 
//            {
//                if(e.ResponseStatusCode==403)
//                    await turnContext.SendActivityAsync("Sorry you need AdminConsent to use this application!!");
//                else if(e.ResponseStatusCode==401)
//                    await turnContext.SendActivityAsync("Sorry,I am not authorized here!! :(");
//            }
//        }
//        private async Task ProcessResponseAsync(ITurnContext<IMessageActivity> turnContext, AdaptiveCardResponse response)
//        {
//            // Access the properties from the user response
//            var userInput = response.Value;
//            var additionalData = response.Message.Trim();
//            if(!userInput)
//                return;
//            //getting initiated user info
//            var user = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id);

           
//            CompletionsOptions completionsOptions = new CompletionsOptions();
//            completionsOptions.Temperature = 0;
//            switch (response.Type)
//            {
//                case TypeEnum.CreatePlanner:
//                    {
//                        var _graphClient = await _authHepler.GetAPPOnlyAuthenticatedClientAsync(response.TenantID);
//                        GraphHelper graphHelper = new GraphHelper(_graphClient,_configuration);
//                        ChatMessageCollectionResponse chatMessageCollection = null;
//                        if (string.IsNullOrEmpty(response.TeamID))
//                        {
                            
//                           chatMessageCollection = await _graphClient.Chats[response.ChannelID].Messages.GetAsync();
                          
//                        }
//                        else
//                        {
//                            string deltaToken = await _dbHelper.GetDeltaTokenForChannel(response.ChannelID);
//                            chatMessageCollection = await graphHelper.GetChannelMessageDelta(response.ChannelID,response.TeamID, deltaToken);
//                            //update with new delta token
//                            deltaToken = string.Empty;
//                            deltaToken = StringHelper.GetDeltaToken(chatMessageCollection);
//                            await _dbHelper.UpdateDeltaToken(deltaToken,response.ChannelID);
//                        }
//                        completionsOptions.Prompts.Add(PromptHelper.CreatePrompt(PromptType.Tasks, response.Message, _configuration["appName"], chatMessageCollection));
//                    }
//                    break;
//                case TypeEnum.summary:
//                    {
//                        var _graphClient = await _authHepler.GetAPPOnlyAuthenticatedClientAsync(response.TenantID);
//                        GraphHelper graphHelper = new GraphHelper(_graphClient,_configuration);
//                        ChatMessageCollectionResponse chatMessageCollection = null;
//                        if (string.IsNullOrEmpty(response.TeamID))
//                        {
//                            chatMessageCollection = chatMessageCollection = await _graphClient.Chats[response.ChannelID].Messages.GetAsync();
//                            //deltaToken = string.Empty;
//                            //deltaToken = StringHelper.GetDeltaToken(chatMessageCollection);
//                            //await _dbHelper.UpdateDeltaToken(deltaToken, response.ChannelID);

//                        }
//                        else
//                        {
//                            string deltaToken = await _dbHelper.GetDeltaTokenForChannel(response.ChannelID);
//                            chatMessageCollection = await graphHelper.GetChannelMessageDelta(response.ChannelID, response.TeamID, deltaToken);
//                            //update with new delta token
//                            //deltaToken = string.Empty;
//                            //deltaToken = StringHelper.GetDeltaToken(chatMessageCollection);
//                            //await _dbHelper.UpdateDeltaToken(deltaToken, response.ChannelID);

//                        }

//                        completionsOptions.Prompts.Add(PromptHelper.CreatePrompt(PromptType.Summerization, additionalData, _configuration["appName"], chatMessageCollection));
//                    }
//                    break;
//                case TypeEnum.suggestion:
//                    {
//                        completionsOptions.Prompts.Add(PromptHelper.CreatePrompt(PromptType.QuestionAnswer, _configuration["appName"],additionalData));
//                    }
//                    break;
//                default:
//                    throw new ArgumentException("Enum not defined");

//            }
            
//            completionsOptions.MaxTokens = 400;
//            completionsOptions.User = user.Email;
//            completionsOptions.ChoicesPerPrompt = 1;
//            try
//            {
//                var client = _openAiClient.GetClient();
//                OpenAiService aiService = new OpenAiService(client, _configuration);
//                var aiResponse = await aiService.MakeRequest(completionsOptions);
//               if (response.Type==TypeEnum.summary)
//               {
//                    UserActivityInfo activityInfo = new UserActivityInfo()
//                    {
//                        ChannelID = response.ChannelID,
//                        TeamID = response.TeamID,
//                        TenantID = response.TenantID,
//                        IsGroupChat = string.IsNullOrEmpty(response.TeamID)?true:false
//                    };

//                    var attachment = MessageFactory.Attachment(InteractiveCard.SummaryResponse(aiResponse, activityInfo));
//                    await turnContext.SendActivityAsync(attachment);
//               }
//               else if (response.Type == TypeEnum.CreatePlanner)
//               {
//                    aiResponse = StringHelper.ButifyTaskList(aiResponse);
//                    var tasksToAdd = JsonConvert.DeserializeObject<List<AISuggestedTask>>(aiResponse.Trim());
//                    //creating graph client for application permission
//                    var _graphClient = await _authHepler.GetAPPOnlyAuthenticatedClientAsync(response.TenantID);
//                    //checking if planner plan exists
//                    GraphHelper graphHelper = new GraphHelper(_graphClient,_configuration);
//                    var plan = await graphHelper.CreateOrGetPlan(response, turnContext,user.UserPrincipalName);
//                    foreach (var task in tasksToAdd) 
//                    {
                        
//                        await graphHelper.CreateTask(plan.Id, task.TaskName, task.Assignees);
//                    }
//               }
//               else
//               {
//                   await turnContext.SendActivityAsync(aiResponse);
//               }
               
//            }
//            catch (Exception ex)
//            {
               
//            }

//        }
//    }
    
//}
