using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CoreTweet;
using Twimager.Objects;
using Twimager.Resources;
using Twimager.Utilities;
using Twimager.Windows;

namespace Twimager
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App
    {
        public const string Destination = "Downloads";
        private const string LogDirectory = "Logs";
        private const string ConfigFile = "config.json";

        public bool IsBusy { get; set; }
        public Logger Logger { get; private set; }
        public Config Config { get; private set; }
        public Tokens Twitter { get; private set; }

        public static App GetCurrent()
        {
            return (App)Current;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                await Init();
            }
            catch (Exception ex)
            {
                await Logger.LogAsync(ex.Message);
                throw;
            }
        }

        private async Task Init()
        {
            Directory.CreateDirectory(LogDirectory);
            Logger = new Logger($"{LogDirectory}/{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");

            await Logger.LogAsync("Loading config");
            Config = Config.Open(ConfigFile);

            if (Config.Credentials == null)
            {
                await Logger.LogAsync("Starting to authorize a Twitter account");
                var window = new AuthWindow();
                window.ShowDialog();

                var result = window.Result;
                Config.Credentials = new Credentials
                {
                    AccessToken = result.AccessToken,
                    AccessTokenSecret = result.AccessTokenSecret
                };

                await Logger.LogAsync("Successfully authorized");
                Config.Save();
            }

            await Logger.LogAsync("Logging into Twitter API");
            Twitter = Tokens.Create(
                TwitterKeys.ConsumerKey,
                TwitterKeys.ConsumerSecret,
                Config.Credentials.AccessToken,
                Config.Credentials.AccessTokenSecret
            );

            var me = await Twitter.Account.VerifyCredentialsAsync();
            await Logger.LogAsync($"Successfully logged in as {me.ScreenName}");

            await Logger.LogAsync("Starting app");
            new MainWindow().ShowDialog();

            await Logger.LogAsync("Shutting down");
            Shutdown();
        }
    }
}
