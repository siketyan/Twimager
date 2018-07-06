using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CoreTweet;
using Newtonsoft.Json;

namespace Twimager.Objects
{
    public class SearchTracking : ITracking
    {
        private const string DirectoryBase = "Searches";
        private const char DefaultPathChar = '_';


        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("is_completed")]
        public bool IsCompleted { get; set; }

        [JsonProperty("oldest")]
        public long? Oldest { get; set; }

        [JsonProperty("latest")]
        public long? Latest { get; set; }

        [JsonIgnore]
        public string Directory { get => $"{DirectoryBase}/{ReplaceInvalidChars(Query)}"; }


        private Tokens Twitter
        {
            get => App.GetCurrent().Twitter;
        }

        public Task UpdateSummaryAsync() => null; // Ignore

        public async Task<IEnumerable<Status>> GetStatusesAsync()
        {
            if (!IsCompleted)
            {
                return await Twitter.Search.TweetsAsync(
                    Query,
                    count: 200,
                    max_id: Oldest
                );
            }
            else
            {
                return await Twitter.Search.TweetsAsync(
                    Query,
                    count: 200,
                    since_id: Latest
                );
            }
        }

        private string ReplaceInvalidChars(string str)
        {
            foreach (var ch in Path.GetInvalidPathChars())
            {
                str = str.Replace(ch, DefaultPathChar);
            }

            return str;
        }
    }
}
