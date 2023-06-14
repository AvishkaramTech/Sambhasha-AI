using Azure.AI.OpenAI;

namespace TeamsHackathonTest.OpenAI
{
    public class OpenAiService
    {
        private readonly Azure.AI.OpenAI.OpenAIClient _aiClient;
        private readonly IConfiguration _configuration;

        public OpenAiService(Azure.AI.OpenAI.OpenAIClient aiClient,IConfiguration configuration)
        {
            _aiClient = aiClient;
            _configuration = configuration;
        }
        public async Task<string> MakeRequest(CompletionsOptions completions)
        {

            var aiResponse = await _aiClient.GetCompletionsAsync(_configuration["OpenAi:ModelName"], // assumes a matching model deployment or model name
                                  completions);
            return aiResponse.Value.Choices[0].Text;
        }
    }
}
