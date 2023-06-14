using Azure.AI.OpenAI;

namespace TeamsHackathonTest.Interfaces
{
    public interface IOpenAiClient
    {
        OpenAIClient GetClient();
    }
}
