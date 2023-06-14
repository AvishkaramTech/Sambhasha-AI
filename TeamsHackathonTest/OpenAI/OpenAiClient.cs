using Azure.AI.OpenAI;
using Azure;
using TeamsHackathonTest.Interfaces;
using Microsoft.Extensions.Azure;

namespace TeamsHackathonTest.OpenAI
{
    public class OpenAiClient:IOpenAiClient
    {
        private readonly IConfiguration _configuration;       
        public OpenAiClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public OpenAIClient GetClient()
        {
            //OpenAIClient client = new OpenAIClient(_configuration["OpenAPI_Key"]);
            OpenAIClient client = new OpenAIClient(new Uri(_configuration["OpenAi:Endpoint"]),new AzureKeyCredential(_configuration["OpenAi:Key"]));
            return client;
        }
    }
}
