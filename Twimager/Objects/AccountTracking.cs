using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreTweet;
using CoreTweet.Core;
using Newtonsoft.Json;

namespace Twimager.Objects
{
    public class AccountTracking : ITracking
    {
        private const string DirectoryBase = "Accounts";


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

        [JsonIgnore]
        public string Directory { get => $"{DirectoryBase}/{Id}"; }


        private Tokens Twitter
        {
            get => App.GetCurrent().Twitter;
        }

        private long? _oldest;
        private long? _latest;

        public async Task UpdateSummaryAsync()
        {
            var user = await Twitter.Users.ShowAsync(Id);

            ScreenName = user.ScreenName;
            Name = user.Name;
            ProfileImageUrl = user.ProfileImageUrlHttps;
        }

        public async Task<IEnumerable<Status>> GetStatusesAsync()
        {
            ListedResponse<Status> statuses;

            if (Latest == null)
            {
                statuses = await Twitter.Statuses.UserTimelineAsync(
                    Id,
                    200,
                    trim_user: true,
                    exclude_replies: false,
                    include_rts: false,
                    max_id: _oldest
                );

                if (_latest == null) _latest = statuses.First().Id;
                if (statuses.Count <= 1)
                {
                    Latest = _latest;
                    return null;
                }

                _oldest = statuses.Last().Id;
            }
            else
            {
                statuses = await Twitter.Statuses.UserTimelineAsync(
                    Id,
                    200,
                    trim_user: true,
                    exclude_replies: false,
                    include_rts: false,
                    since_id: _latest
                );
                
                if (!statuses.Any())
                {
                    Latest = _latest;
                    return null;
                }

                _latest = statuses.First().Id;
            }

            return statuses;
        }

        public void Reset()
        {
            Latest = _latest = _oldest = null;
            App.GetCurrent().Config.Save();
        }
    }
}
