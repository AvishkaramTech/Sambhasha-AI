using Azure;
using Microsoft.Bot.Builder;
using Microsoft.Graph;
using Microsoft.Graph.Chats.Item.Messages.Delta;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.TeamsFx.Conversation;
using TeamsHackathonTest.Models;
using AdaptiveCardResponse = TeamsHackathonTest.Models.AdaptiveCardResponse;

namespace TeamsHackathonTest.Helper
{
    public class GraphHelper
    {
        private readonly GraphServiceClient _graphClient;
        private readonly IConfiguration _configuration;

        public GraphHelper(GraphServiceClient serviceClient,IConfiguration configuration)
        {
            this._graphClient = serviceClient;
            _configuration = configuration;
        }
        public async Task<PlannerPlan> CreateOrGetPlan(Models.AdaptiveCardResponse response,ITurnContext turnContext,string upn)
        {
            PlannerPlan plannerPlan = null;
            
            var plans = await _graphClient.Groups[response.TeamID].Planner.Plans.GetAsync().ConfigureAwait(false);
            if (plans == null)
            {
                await turnContext.SendActivityAsync($"Plan not created for this teams. Creating a plan named {StringHelper.GetPlanName(response?.TeamID, _configuration["appName"])}");
                plannerPlan = await CreatePlannerPlan(response);
            }
            else
            {
                plannerPlan = plans.Value.Where(x => x.Title == StringHelper.GetPlanName(response?.TeamID, _configuration["appName"])).FirstOrDefault();
                if (plannerPlan == null)
                {
                    await turnContext.SendActivityAsync($"Creating a plan named {StringHelper.GetPlanName(response?.TeamID, _configuration["appName"])}");
                    plannerPlan = await CreatePlannerPlan(response);
                }
            }
            return plannerPlan;
        }
        public async Task CreateTask(string planID,string title,List<string> assignees)
        {
            var plannerAssignment = new PlannerAssignments();
            if (assignees.Any())
            {
                plannerAssignment.AdditionalData = new Dictionary<string, object>();
                foreach (var user in assignees)
                {
                    plannerAssignment.AdditionalData.Add(user, new PlannerAssignment
                    {
                        OdataType = "#microsoft.graph.plannerAssignment",
                        OrderHint = " !",
                    });
                }
            }
            var task = new PlannerTask()
            {
                PlanId = planID,
               // BucketId = string.IsNullOrEmpty(plannerTask.BucketID) ? null : plannerTask.BucketID,
                Title = title,
                //DueDateTime = plannerTask.DueDateTime,
                //StartDateTime = plannerTask.StartDateTime,
                Priority = 5, // Currently, Planner interprets values 0 and 1 as "urgent", 2, 3 and 4 as "important", 5, 6, and 7 as "medium", and 8, 9, and 10 as "low"
                Assignments = plannerAssignment,
                //Details = new PlannerTaskDetails
                //{
                //    Description = plannerTask.Description
                //}
            };
            var result = await _graphClient.Planner.Tasks.PostAsync(task);
        }
        public async Task<ChatMessageCollectionResponse> GetChannelMessageDelta(string channelID,string teamID,string deltaToken)
        {
            

            var reqInfo = _graphClient.Teams[teamID].Channels[channelID].Messages.Delta.ToGetRequestInformation((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Count = true;
            });
            if (!string.IsNullOrEmpty(deltaToken))
            {
                reqInfo.UrlTemplate = reqInfo.UrlTemplate.Insert(reqInfo.UrlTemplate.Length - 1, ",%24deltatoken");
                reqInfo.QueryParameters.Add("%24deltatoken", deltaToken);
            }
            var result = await _graphClient.RequestAdapter.SendAsync(reqInfo, ChatMessageCollectionResponse.CreateFromDiscriminatorValue);            
            return result;
        }
        public async Task<ChatMessageCollectionResponse> GetChatMessageDelta(string channelID, string deltaToken)
        {
            var reqInfo = _graphClient.Chats[channelID].Messages.Delta.ToGetRequestInformation((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Count = true;
            });
            if (!string.IsNullOrEmpty(deltaToken))
            {
                reqInfo.UrlTemplate = reqInfo.UrlTemplate.Insert(reqInfo.UrlTemplate.Length - 1, ",%24deltatoken");
                reqInfo.QueryParameters.Add("%24deltatoken", deltaToken);
            }
            var result = await _graphClient.RequestAdapter.SendAsync(reqInfo, ChatMessageCollectionResponse.CreateFromDiscriminatorValue);
            var link = result.OdataNextLink.ToString();
            return result;
        }
        private async Task<PlannerPlan> CreatePlannerPlan(AdaptiveCardResponse response)
        {
            var plan = new PlannerPlan()
            {
                Owner = response.TeamID,
                //Container = new PlannerPlanContainer{ //container is not working 
                //   Url = $"https://graph.microsoft.com/beta/groups/{groupID}"
                //},
                Title = StringHelper.GetPlanName(response?.TeamID, _configuration["appName"]),
            };
            var plannerPlan = await _graphClient.Planner.Plans.PostAsync(plan).ConfigureAwait(false);
            return plannerPlan;
        }
        
    }
}
