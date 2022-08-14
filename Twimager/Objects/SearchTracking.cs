using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public string Directory => $"{DirectoryBase}/{ReplaceInvalidChars(Query)}";


        private static Tokens Twitter => App.GetCurrent().Twitter;

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

            return await Twitter.Search.TweetsAsync(
                Query,
                count: 200,
                since_id: Latest
            );
        }

        private static string ReplaceInvalidChars(string str)
        {
            return Path
                .GetInvalidPathChars()
                .Aggregate(str, (current, ch) => current.Replace(ch, DefaultPathChar));
        }

        public override string ToString()
        {
            return $"Search: {Query}";
        }
    }
}
