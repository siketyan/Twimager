using System.Collections.Generic;
using System.Threading.Tasks;
using CoreTweet;
using Newtonsoft.Json;

namespace Twimager.Objects
{
    public class ListTracking : ITracking
    {
        private const string DirectoryBase = "Lists";


        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("fullname")]
        public string FullName { get; set; }

        [JsonProperty("is_completed")]
        public bool IsCompleted { get; set; }

        [JsonProperty("oldest")]
        public long? Oldest { get; set; }

        [JsonProperty("latest")]
        public long? Latest { get; set; }

        [JsonIgnore]
        public string Directory { get => $"{DirectoryBase}/{Id}"; }


        private Tokens Twitter
        {
            get => App.GetCurrent().Twitter;
        }

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
                    include_rts: false,
                    max_id: Oldest
                );
            }
            else
            {
                return await Twitter.Lists.StatusesAsync(
                    Id,
                    count: 200,
                    include_rts: false,
                    since_id: Latest
                );
            }
        }
    }
}
