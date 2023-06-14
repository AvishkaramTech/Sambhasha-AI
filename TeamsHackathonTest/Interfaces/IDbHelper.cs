namespace TeamsHackathonTest.Interfaces
{
    public interface IDbHelper
    {
        Task AddDeltaToken(string token, string channelID);
        Task DeleteExistingConversationChannel(string channelID);
        Task<DateTime> GetCurrentRun(string channelID);
        Task<string> GetDeltaTokenForChannel(string channelID);
        Task<DateTime> GetLastRun(string channelID);
        Task<bool> GetListeningStatus(string channelID);
        Task InsertNewConversationChannel(string token, string channelID,bool isTeamChannel);
        Task SetCurrentRun(string channelID);
        Task SetIsActiveListening(string channelID);
        Task SetIsStoppedListening(string channelID);
        Task SetLastRun(string channelID);
        Task UpdateDeltaToken(string token, string channelID);
    }
}
