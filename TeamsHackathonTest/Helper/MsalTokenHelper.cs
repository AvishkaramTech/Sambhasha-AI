using Microsoft.Identity.Client;

namespace TeamsHackathonTest.Helper
{
    public class MsalTokenHelper
    {
        public async Task<string> GetToken(string clientID,string clientSecret,string redirectUrl,string tenantID)
        {
            
            string AUTHORITY = "https://login.microsoftonline.com/common";
            var clientapp = ConfidentialClientApplicationBuilder.Create(clientID)
                  .WithClientSecret(clientSecret)
                  .WithRedirectUri(redirectUrl)
                  .WithAuthority(AUTHORITY)
             .WithTenantId(tenantID)
                  .Build();
            var authResult = await clientapp.AcquireTokenForClient(new[] { "https://graph.microsoft.com" + "/.default" }).ExecuteAsync();
            return authResult.AccessToken;
        }

    }
}
