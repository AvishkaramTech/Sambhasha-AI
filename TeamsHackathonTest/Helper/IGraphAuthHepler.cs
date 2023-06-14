using Microsoft.Graph;

namespace TeamsHackathonTest.Helper
{
    public interface IGraphAuthHepler
    {
        Task<GraphServiceClient> GetAPPOnlyAuthenticatedClientAsync(string tenentID);
    }
}
