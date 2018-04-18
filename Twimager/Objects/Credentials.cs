using Newtonsoft.Json;

namespace Twimager.Objects
{
    public class Credentials
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("access_token_secret")]
        public string AccessTokenSecret { get; set; }
    }
}
