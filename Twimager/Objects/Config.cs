using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;

namespace Twimager.Objects
{
    public class Config
    {
        [JsonProperty("ignore_retweets")]
        public bool IgnoreRetweets { get; set; } = true;

        [JsonProperty("ignore_replies")]
        public bool IgnoreReplies { get; set; } = true;

        [JsonProperty("credentials")]
        public Credentials Credentials { get; set; }

        [JsonProperty("trackings")]
        public ObservableCollection<ITracking> Trackings { get; set; } = new();


        [JsonIgnore]
        private string _path;


        public Config(string path)
        {
            _path = path;
        }

        public static Config Open(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                var config = JsonConvert.DeserializeObject<Config>(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });

                if (config != null)
                {
                    config._path = path;
                }

                return config;
            }
            catch (FileNotFoundException)
            {
                return new Config(path);
            }
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            File.WriteAllText(_path, json);
        }
    }
}
