namespace TeamsHackathonTest.Helper
{
    public class ConnectionHelper
    {
        private readonly IConfiguration _configuration;

        public ConnectionHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string ConnectionStr { 
            get {
                return _configuration["ConnectionStrings:MySqlConnection"];
            }
        }
    }
}
