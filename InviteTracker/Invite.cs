using Newtonsoft.Json;

namespace HAL9000
{
    public class Invite
    {
        [JsonProperty("ID")]
        public string Id { get; set; }

        [JsonProperty("Uses")]
        public int Uses { get; set; }
    }
}