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

        [JsonProperty("latest")]
        public long? Latest { get; set; }

        [JsonIgnore]
        public string Directory { get => $"{DirectoryBase}/{ReplaceInvalidChars(Query)}"; }


        private Tokens Twitter
        {
            get => App.GetCurrent().Twitter;
        }

        private long? _oldest;
        private long? _latest;

        public Task UpdateSummaryAsync() => null; // Ignore

        public async Task<IEnumerable<Status>> GetStatusesAsync()
        {
            SearchResult statuses;

            if (Latest == null)
            {
                statuses = await Twitter.Search.TweetsAsync(
                    Query,
                    count: 200,
                    max_id: _oldest
                );

                if (_latest == null) _latest = statuses.First().Id;
                if (statuses.Count() <= 1)
                {
                    Latest = _latest;
                    return null;
                }

                _oldest = statuses.Last().Id;
            }
            else
            {
                statuses = await Twitter.Search.TweetsAsync(
                    Query,
                    count: 200,
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
