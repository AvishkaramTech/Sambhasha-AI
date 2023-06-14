using Microsoft.Kiota.Abstractions.Authentication;

namespace TeamsHackathonTest.Helper
{
    public class TokenProvider : IAccessTokenProvider
    {
        private readonly Dictionary<string, object> _data;
        public TokenProvider(Dictionary<string, object> data)
        {
            _data = data;
        }
        public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> additionalAuthenticationContext = default,
            CancellationToken cancellationToken = default)
        {
            MsalTokenHelper tokenHelper = new MsalTokenHelper();
            string clientID = _data["clientID"].ToString();
            string clientSecret = _data["clientSecret"].ToString();
            string redirectUrl = _data["redirectUrl"].ToString();
            string tenantID = _data["tenantID"].ToString();
            var token = await tokenHelper.GetToken(clientID, clientSecret, redirectUrl, tenantID);
            // string token = "eyJ0eXAiOiJKV1QiLCJub25jZSI6IkQ3YklEQ21Vc3ZzTkxqLWx2ZlIwSjlDS01rOG9ZVFZuV24wak90THRFNGsiLCJhbGciOiJSUzI1NiIsIng1dCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyIsImtpZCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC85OWJjZWMxYS0xYThjLTQ0NjQtOGQ1YS03ZjRhMmVmOGY1YWUvIiwiaWF0IjoxNjg1NTIzNzExLCJuYmYiOjE2ODU1MjM3MTEsImV4cCI6MTY4NTYxMDQxMSwiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IkFUUUF5LzhUQUFBQVNXeWRORVN6N1JhUzdobW4zVGZza29WVnkwRUl6ZWRlMHRybG96K0Q4VmxtVFVHWFI0OGMzemZpQ0dLS1VieisiLCJhbXIiOlsicHdkIl0sImFwcF9kaXNwbGF5bmFtZSI6IkdyYXBoIEV4cGxvcmVyIiwiYXBwaWQiOiJkZThiYzhiNS1kOWY5LTQ4YjEtYThhZC1iNzQ4ZGE3MjUwNjQiLCJhcHBpZGFjciI6IjAiLCJmYW1pbHlfbmFtZSI6IkdhdXRhbSIsImdpdmVuX25hbWUiOiJOaXNjaGl0IiwiaWR0eXAiOiJ1c2VyIiwiaXBhZGRyIjoiMTYzLjUzLjI3LjIzNSIsIm5hbWUiOiJOaXNjaGl0IEdhdXRhbSIsIm9pZCI6ImViMmQyYzFkLTQxYjktNGVlOC1hMmZkLWIwYzNlMzcxMTg1MCIsInBsYXRmIjoiMyIsInB1aWQiOiIxMDAzMjAwMDlCODlFMjJGIiwicmgiOiIwLkFTb0FHdXk4bVl3YVpFU05XbjlLTHZqMXJnTUFBQUFBQUFBQXdBQUFBQUFBQUFBcUFMRS4iLCJzY3AiOiJDYWxlbmRhcnMuUmVhZCBDYWxlbmRhcnMuUmVhZFdyaXRlIENoYXQuUmVhZCBDaGF0LlJlYWRCYXNpYyBDb250YWN0cy5SZWFkV3JpdGUgRGV2aWNlTWFuYWdlbWVudENvbmZpZ3VyYXRpb24uUmVhZC5BbGwgRGV2aWNlTWFuYWdlbWVudE1hbmFnZWREZXZpY2VzLlByaXZpbGVnZWRPcGVyYXRpb25zLkFsbCBEZXZpY2VNYW5hZ2VtZW50TWFuYWdlZERldmljZXMuUmVhZC5BbGwgRGV2aWNlTWFuYWdlbWVudFJCQUMuUmVhZC5BbGwgRGV2aWNlTWFuYWdlbWVudFNlcnZpY2VDb25maWcuUmVhZC5BbGwgRGlyZWN0b3J5LlJlYWRXcml0ZS5BbGwgRmlsZXMuUmVhZFdyaXRlLkFsbCBHcm91cC5SZWFkV3JpdGUuQWxsIElkZW50aXR5Umlza0V2ZW50LlJlYWQuQWxsIE1haWwuUmVhZCBNYWlsLlJlYWRXcml0ZSBNYWlsYm94U2V0dGluZ3MuUmVhZFdyaXRlIE5vdGVzLlJlYWRXcml0ZS5BbGwgb3BlbmlkIFBlb3BsZS5SZWFkIFBsYWNlLlJlYWQgUHJlc2VuY2UuUmVhZCBQcmVzZW5jZS5SZWFkLkFsbCBwcm9maWxlIFJlcG9ydHMuUmVhZC5BbGwgU2l0ZXMuUmVhZFdyaXRlLkFsbCBUYXNrcy5SZWFkV3JpdGUgVXNlci5SZWFkIFVzZXIuUmVhZEJhc2ljLkFsbCBVc2VyLlJlYWRXcml0ZSBVc2VyLlJlYWRXcml0ZS5BbGwgZW1haWwgQ2hhbm5lbE1lc3NhZ2UuUmVhZC5BbGwiLCJzdWIiOiIyOWNSeWJOenUyd1pQbDkxN0hJWGNBTy1kaGdyT1JyWmtDQkRFSUIwbktNIiwidGVuYW50X3JlZ2lvbl9zY29wZSI6IkFTIiwidGlkIjoiOTliY2VjMWEtMWE4Yy00NDY0LThkNWEtN2Y0YTJlZjhmNWFlIiwidW5pcXVlX25hbWUiOiJOaXNjaGl0QGF2aXNoZGV2MS5vbm1pY3Jvc29mdC5jb20iLCJ1cG4iOiJOaXNjaGl0QGF2aXNoZGV2MS5vbm1pY3Jvc29mdC5jb20iLCJ1dGkiOiJ5QjhiOFhCTVMwU1BLdE51cnh6TEFBIiwidmVyIjoiMS4wIiwid2lkcyI6WyI3Mjk4MjdlMy05YzE0LTQ5ZjctYmIxYi05NjA4ZjE1NmJiYjgiLCIyOTIzMmNkZi05MzIzLTQyZmQtYWRlMi0xZDA5N2FmM2U0ZGUiLCI2OTA5MTI0Ni0yMGU4LTRhNTYtYWE0ZC0wNjYwNzViMmE3YTgiLCJmMjhhMWY1MC1mNmU3LTQ1NzEtODE4Yi02YTEyZjJhZjZiNmMiLCJmZTkzMGJlNy01ZTYyLTQ3ZGItOTFhZi05OGMzYTQ5YTM4YjEiLCI2MmU5MDM5NC02OWY1LTQyMzctOTE5MC0wMTIxNzcxNDVlMTAiLCJmMmVmOTkyYy0zYWZiLTQ2YjktYjdjZi1hMTI2ZWU3NGM0NTEiLCJmMDIzZmQ4MS1hNjM3LTRiNTYtOTVmZC03OTFhYzAyMjYwMzMiLCJiNzlmYmY0ZC0zZWY5LTQ2ODktODE0My03NmIxOTRlODU1MDkiXSwieG1zX2NjIjpbIkNQMSJdLCJ4bXNfc3NtIjoiMSIsInhtc19zdCI6eyJzdWIiOiJKdUdBdEU2aDJyVDgxbFB6MFMwWTBaYVVITW55VEhXUVc1NUJEN0xBclFNIn0sInhtc190Y2R0IjoxNTM5MDc2OTM3fQ.ogR4p4ZM4fmh6rmj7ZOGy-BtPJ3gyyiV-8y4Fe2XSSSwh3bnmld-Fgi04fwq-naY5hf42RfV7N-aQc22m_uffX89uANM-UidPwRftMjtnFNJAz6dOxKI_rCd-VRagQ2yplwqh_nBPg8RubRJ3_YObitPGO5-8vEYLEeRnpV6nRRtabVfc1m1q6387iYN0UQEqiUqfULsLFnjku4LKUE73Elx19DpnSbf5J4SFc02CNMHQ4ZJ0SsoDHg3uqteH7JwN0uC-xUNRccVTCkMbe8Oe3nn3sU06CIt5sgO7AljFQHvYaiJaIQA4gbPev6Q-3whwf-uKbr8teo0nSuSXz-iBA";
            if (token == null)
            {
                return string.Empty;
            }
            // get the token and return it in your own way
            return token;
        }

        public AllowedHostsValidator AllowedHostsValidator { get; }
    }
    
}
