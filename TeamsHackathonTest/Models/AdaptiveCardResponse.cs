using TeamsHackathonTest.Helper;

namespace TeamsHackathonTest.Models
{
    public class AdaptiveCardResponse
    {
        public bool Value { get; set; }
        public string? Message { get; set; }
        public string? ChannelID { get; set; }
        public string? TeamID { get; set; }
        public string? TenantID { get; set; }
        public TypeEnum Type { get; set; }
    }
}
