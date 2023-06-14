using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using TeamsHackathonTest.Models;

namespace AISuggestionAvish.Interfaces
{
    public interface IRunningTaskHelper
    {
        Task StartListeningToChannelAsync(UserActivityInfo activityInfo);
    }
}
