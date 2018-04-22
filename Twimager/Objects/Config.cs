using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;

namespace Twimager.Objects
{
    public class Config
    {
        public Credentials Credentials { get; set; }
        public ObservableCollection<Account> Accounts { get; set; } = new ObservableCollection<Account>();


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
                var config = JsonConvert.DeserializeObject<Config>(json);
                config._path = path;

                return config;
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
