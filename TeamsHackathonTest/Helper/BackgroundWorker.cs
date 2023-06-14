using AISuggestionAvish.Interfaces;
using TeamsHackathonTest.Models;

namespace AISuggestionAvish.Helper
{
    public class BackgroundWorker : BackgroundService
    {
        //public BackgroundWorker(IRunningTaskHelper runningTaskHelper)
        //{
        //    _runningTaskHelper = runningTaskHelper;
        //}
        public string _TeamID;
        public string _ChannelID;
        public string _ServiceURL;
        public string _tenantID;
        public bool _isGroupChat;
        private readonly IRunningTaskHelper _runningTaskHelper;

        public string TeamID
        {
            get { return _TeamID; }
            set { _TeamID = value; }
        }
        public string ChannelID
        {
            get { return _ChannelID; }
            set { _ChannelID = value; }
        }
        public string ServiceURL
        {
            get { return _ServiceURL; }
            set { _ServiceURL = value; }
        }
        public string TenantID
        {
            get { return _tenantID; }
            set { _tenantID = value; }
        }
        public bool IsGroupChat
        {
            get { return _isGroupChat; }
            set { _isGroupChat = value; }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (string.IsNullOrEmpty(_ChannelID))
                {
                    continue;
                }
                UserActivityInfo userActivityInfo = new UserActivityInfo
                {
                    ChannelID = _ChannelID,
                    ServiceURL = _ServiceURL,
                    IsGroupChat = _isGroupChat,
                    TeamID = _TeamID,
                    TenantID = _tenantID
                };
                _ChannelID=string.Empty;
                _ServiceURL = string.Empty;
                _isGroupChat = false;
                _TeamID = string.Empty;
                _tenantID=string.Empty;
             //  await _runningTaskHelper.StartListeningToChannelAsync(userActivityInfo);
                Thread.Sleep(1000);
            }
           
        }
    }
}
