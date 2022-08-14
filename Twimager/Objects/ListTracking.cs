using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using CoreTweet;
using Newtonsoft.Json;

namespace Twimager.Objects
{
    public class ListTracking : ITracking, INotifyPropertyChanged
    {
        private const string DirectoryBase = "Lists";

        public event PropertyChangedEventHandler PropertyChanged;


        [JsonProperty("id")]
        public long Id { get; set; }

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

        [JsonProperty("fullname")]
        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged("FullName");
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


        private string _name;
        private string _fullName;

        private static Config Config => App.GetCurrent().Config;
        private static Tokens Twitter => App.GetCurrent().Twitter;

        public async Task UpdateSummaryAsync()
        {
            var list = await Twitter.Lists.ShowAsync(Id);

            Name = list.Name;
            FullName = list.FullName;
        }

        public async Task<IEnumerable<Status>> GetStatusesAsync()
        {
            if (!IsCompleted)
            {
                return await Twitter.Lists.StatusesAsync(
                    Id,
                    count: 200,
                    include_rts: !Config.IgnoreRetweets,
                    max_id: Oldest
                );
            }

            return await Twitter.Lists.StatusesAsync(
                Id,
                count: 200,
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
            return $"List: {FullName} ({Id})";
        }
    }
}
