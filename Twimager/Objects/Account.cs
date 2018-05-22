using System.Linq;
using System.Threading.Tasks;
using CoreTweet;
using CoreTweet.Core;
using Newtonsoft.Json;

namespace Twimager.Objects
{
    public class Account : ITracking
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [JsonProperty("latest")]
        public long? Latest { get; set; }

        private long? _oldest;
        private long? _latest;
        private Tokens _twitter;

        public Account()
        {
            _twitter = App.GetCurrent().Twitter;
        }

        public async Task<ListedResponse<Status>> GetStatusesAsync()
        {
            ListedResponse<Status> statuses;

            if (Latest == null)
            {
                statuses = await _twitter.Statuses.UserTimelineAsync(
                    Id,
                    200,
                    trim_user: true,
                    exclude_replies: false,
                    include_rts: false,
                    max_id: _oldest
                );

                if (statuses.Count <= 1)
                {
                    Latest = _latest;
                    return null;
                }
            }
            else
            {
                statuses = await _twitter.Statuses.UserTimelineAsync(
                    Id,
                    200,
                    trim_user: true,
                    exclude_replies: false,
                    include_rts: false,
                    since_id: Latest
                );

                if (!statuses.Any()) return null;
            }

            return statuses;
        }
    }
}
