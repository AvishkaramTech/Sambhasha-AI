

using Azure.Core;
using Microsoft.Extensions.Primitives;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Rest;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace TeamsHackathonTest.Helper
{
    public  class GraphAuthHepler: IGraphAuthHepler
    {
        private readonly IConfiguration _configuration;

        public GraphAuthHepler(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public  async Task<GraphServiceClient> GetAPPOnlyAuthenticatedClientAsync(string tenentID)
        {
            Dictionary<string,object> dict = new Dictionary<string,object>();
            dict.Add("tenantID", tenentID);
            dict.Add("clientID", _configuration["clientID"]);
            dict.Add("clientSecret", _configuration["clientSecret"]);
            dict.Add("redirectUrl", _configuration["redirectUrl"]);
            var authenticationProvider = new BaseBearerTokenAuthenticationProvider(new TokenProvider(dict));
            return new GraphServiceClient(authenticationProvider);           
        }
    
    }
    
}
