using Newtonsoft.Json;
using System.IO;

namespace Twimager.Objects
{
    public class Config
    {
        public Credentials Credentials { get; set; }


        [JsonIgnore]
        private readonly string _path;


        public Config(string path)
        {
            _path = path;
        }

        public static Config Open(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<Config>(json);
            }
            catch (FileNotFoundException)
            {
                return new Config(path);
            }
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(_path, json);
        }
    }
}
