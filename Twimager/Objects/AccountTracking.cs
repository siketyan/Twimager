using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using CoreTweet;
using Newtonsoft.Json;

namespace Twimager.Objects
{
    public class AccountTracking : ITracking, INotifyPropertyChanged
    {
        private const string DirectoryBase = "Accounts";

        public event PropertyChangedEventHandler PropertyChanged;


        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenName
        {
            get => _screenName;
            set
            {
                _screenName = value;
                OnPropertyChanged("ScreenName");
            }
        }

        [JsonProperty("name")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        [JsonProperty("profile_image_url")]
        public string ProfileImageUrl
        {
            get => _profileImageUrl;
            set
            {
                _profileImageUrl = value;
                OnPropertyChanged("ProfileImageUrl");
            }
        }

        [JsonProperty("is_completed")]
        public bool IsCompleted { get; set; }

        [JsonProperty("oldest")]
        public long? Oldest { get; set; }

        [JsonProperty("latest")]
        public long? Latest { get; set; }

        [JsonIgnore]
        public string Directory { get => $"{DirectoryBase}/{Id}"; }


        private string _screenName;
        private string _name;
        private string _profileImageUrl;

        private static Config Config => App.GetCurrent().Config;
        private static Tokens Twitter => App.GetCurrent().Twitter;

        public async Task UpdateSummaryAsync()
        {
            var user = await Twitter.Users.ShowAsync(Id);

            ScreenName = user.ScreenName;
            Name = user.Name;
            ProfileImageUrl = user.ProfileImageUrlHttps;
        }

        public async Task<IEnumerable<Status>> GetStatusesAsync()
        {
            if (!IsCompleted)
            {
                return await Twitter.Statuses.UserTimelineAsync(
                    Id,
                    200,
                    trim_user: true,
                    exclude_replies: Config.IgnoreReplies,
                    include_rts: !Config.IgnoreRetweets,
                    max_id: Oldest
                );
            }

            return await Twitter.Statuses.UserTimelineAsync(
                Id,
                200,
                trim_user: true,
                exclude_replies: Config.IgnoreReplies,
                include_rts: !Config.IgnoreRetweets,
                since_id: Latest
            );
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            return $"Account: {ScreenName} ({Id})";
        }
    }
}
