using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using TeamsHackathonTest.Interfaces;
using static Dapper.SqlMapper;

namespace TeamsHackathonTest.Helper
{
    public class DbHelper:IDbHelper
    {
        private readonly ConnectionHelper _connectionHelper;

        public DbHelper(ConnectionHelper connectionHelper)
        {
            _connectionHelper = connectionHelper;
        }
        public async Task AddDeltaToken(string token,string channelID)
        {
            string sql = "insert into tblconversations(LastDeltaToken) value (@deltaToken) where ChannelID=@channelID;";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@deltaToken", token);
            dynamicParameters.Add("@channelID", channelID);
            using(IDbConnection db = new MySqlConnection(_connectionHelper.ConnectionStr))
            {
                await db.ExecuteAsync(sql, dynamicParameters, commandType: CommandType.Text);
            }
        }
        public async Task UpdateDeltaToken(string token, string channelID)
        {
            string sql = "update tblconversations set LastDeltaToken = @deltaToken where ChannelID=@channelID;";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@deltaToken", token);
            dynamicParameters.Add("@channelID", channelID);
            using (IDbConnection db = new MySqlConnection(_connectionHelper.ConnectionStr))
            {
                await db.ExecuteAsync(sql, dynamicParameters, commandType: CommandType.Text);
            }
        }
        public async Task InsertNewConversationChannel (string token, string channelID,bool isTeamChannel)
        {
            string sql = "insert into tblconversations(ChannelID,IsTeamChannel,LastRun) value (@channelID,@isTeamsChannel,utc_timestamp());";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@channelID", channelID);
            dynamicParameters.Add("@isTeamsChannel", isTeamChannel);
            using (IDbConnection db = new MySqlConnection(_connectionHelper.ConnectionStr))
            {
                await db.ExecuteAsync(sql, dynamicParameters, commandType: CommandType.Text);
            }
        }
        public async Task DeleteExistingConversationChannel(string channelID)
        {
            string sql = "delete from tblconversations where ChannelID = @channelID;";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@channelID", channelID);
            using (IDbConnection db = new MySqlConnection(_connectionHelper.ConnectionStr))
            {
                await db.ExecuteAsync(sql, dynamicParameters, commandType: CommandType.Text);
            }
        }
        public async Task<string> GetDeltaTokenForChannel(string channelID)
        {

            string sql = "select LastDeltaToken from tblconversations where ChannelID=@channelID;";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@channelID", channelID);
            string lastDeltaToken = string.Empty;
            try
            {
                using (IDbConnection db = new MySqlConnection(_connectionHelper.ConnectionStr))
                {
                    lastDeltaToken = await db.ExecuteScalarAsync<string>(sql, dynamicParameters, commandType: CommandType.Text);
                }
            }
            catch(Exception ex)
            {

            }
            return lastDeltaToken;
        }

        public async Task SetIsActiveListening(string channelID)
        {
            string sql = "update tblconversations set IsActivelyListening=@listenStatus where ChannelID=@channelID;";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@channelID", channelID);
            dynamicParameters.Add("@listenStatus", true);
            try
            {
                using (IDbConnection db = new MySqlConnection(_connectionHelper.ConnectionStr))
                {
                   await db.ExecuteAsync(sql, dynamicParameters, commandType: CommandType.Text);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task SetIsStoppedListening(string channelID)
        {
            string sql = "update tblconversations set IsActivelyListening=@listenStatus where ChannelID=@channelID;";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@channelID", channelID);
            dynamicParameters.Add("@listenStatus", false);
            try
            {
                using (IDbConnection db = new MySqlConnection(_connectionHelper.ConnectionStr))
                {
                    await db.ExecuteAsync(sql, dynamicParameters, commandType: CommandType.Text);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<bool> GetListeningStatus(string channelID)
        {
            string sql = "select IsActivelyListening from tblconversations where ChannelID=@channelID;";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@channelID", channelID);
            try
            {
                using (IDbConnection db = new MySqlConnection(_connectionHelper.ConnectionStr))
                {
                    var status = await db.ExecuteScalarAsync<bool>(sql, dynamicParameters, commandType: CommandType.Text);
                    return status;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task SetLastRun(string channelID)
        {
            string sql = "update tblconversations set LastRun=utc_timestamp() where ChannelID=@channelID;";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@channelID", channelID);
            try
            {
                using (IDbConnection db = new MySqlConnection(_connectionHelper.ConnectionStr))
                {
                    await db.ExecuteAsync(sql, dynamicParameters, commandType: CommandType.Text);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<DateTime> GetLastRun(string channelID)
        {
            string sql = "select LastRun from tblconversations where ChannelID=@channelID;";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@channelID", channelID);
            try
            {
                using (IDbConnection db = new MySqlConnection(_connectionHelper.ConnectionStr))
                {
                   var lastRun = await db.ExecuteScalarAsync<DateTime>(sql, dynamicParameters, commandType: CommandType.Text);
                    return lastRun;
                }
            }
            catch (Exception ex)
            {
                return DateTime.UtcNow;
            }
        }

        public async Task SetCurrentRun(string channelID)
        {
            string sql = "update tblconversations set CurrentRun=utc_timestamp() where ChannelID=@channelID;";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@channelID", channelID);
            try
            {
                using (IDbConnection db = new MySqlConnection(_connectionHelper.ConnectionStr))
                {
                    await db.ExecuteAsync(sql, dynamicParameters, commandType: CommandType.Text);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<DateTime> GetCurrentRun(string channelID)
        {
            string sql = "select CurrentRun from tblconversations where ChannelID=@channelID;";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@channelID", channelID);
            try
            {
                using (IDbConnection db = new MySqlConnection(_connectionHelper.ConnectionStr))
                {
                    var lastRun = await db.ExecuteScalarAsync<DateTime>(sql, dynamicParameters, commandType: CommandType.Text);
                    return lastRun;
                }
            }
            catch (Exception ex)
            {
                return DateTime.UtcNow;
            }
        }
    }
}
