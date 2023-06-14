using Microsoft.Bot.Schema;
using TeamsHackathonTest.Models;

namespace TeamsHackathonTest.Helper
{
    public interface IProactiveHelper
    {
        Task<string> SendInTeams(string serviceUrl, Attachment attachment, UserActivityInfo userActivityInfo);
    }
}
