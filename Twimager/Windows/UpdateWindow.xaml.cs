using CoreTweet;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Twimager.Enums;
using Twimager.Objects;
using Forms = System.Windows.Forms;

namespace Twimager.Windows
{
    /// <summary>
    /// StatusWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class UpdateWindow : INotifyPropertyChanged
    {
        private const int WindowMargin = 32;
        private const string Destination = "Downloads";

        public event PropertyChangedEventHandler PropertyChanged;

        public string Status {
            get => _status;
            private set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public Account Account { get; }

        private string _status = "Initializing...";
        private Tokens _twitter;
        private UpdateType _type;

        public UpdateWindow(Account account, UpdateType type)
        {
            InitializeComponent();

            Account = account;
            _twitter = App.GetCurrent().Twitter;
            _type = type;

            DataContext = this;
        }

        public void ShowWithPosition()
        {
            var screen = Forms.Screen.PrimaryScreen;
            var area = screen.WorkingArea;

            Left = area.Right - Width - WindowMargin;
            Top = area.Bottom - Height - WindowMargin;

            Show();
            UpdateAsync(null, null);
        }

        private async void UpdateAsync(object sender, RoutedEventArgs e)
        {
            var dir = $"{Destination}/{Account.ScreenName}";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using (var wc = new WebClient())
            {
                if (_type == UpdateType.Past)
                {
                    long? oldest = null;
                    while (true)
                    {
                        var statuses = await _twitter.Statuses.UserTimelineAsync(
                            Account.Id,
                            200,
                            trim_user: true,
                            exclude_replies: true,
                            include_rts: false,
                            max_id: oldest
                        );

                        if (!statuses.Any()) break;
                        foreach (var status in statuses)
                        {
                            await DownloadMediaAsync(wc, status, dir);
                        }

                        oldest = statuses.Last().Id;
                    }
                }
                else
                {
                    while (true)
                    {
                        var statuses = await _twitter.Statuses.UserTimelineAsync(
                            Account.Id,
                            200,
                            trim_user: true,
                            exclude_replies: true,
                            include_rts: false,
                            since_id: Account.Latest
                        );

                        if (!statuses.Any()) break;
                        foreach (var status in statuses)
                        {
                            await DownloadMediaAsync(wc, status, dir);
                        }

                        Account.Latest = statuses.First().Id;
                    }
                }
            }

            Close();
        }

        private async Task DownloadMediaAsync(WebClient wc, Status status, string destination)
        {
            var entities = status.ExtendedEntities;
            if (entities == null) return;

            foreach (var media in entities.Media)
            {
                //try
                //{
                    var url = media.MediaUrlHttps;
                    var name = Path.GetFileName(url);
                    var file = $"{destination}/{name}";

                    Status = name;

                    if (File.Exists(file)) continue;
                    await wc.DownloadFileTaskAsync($"{url}:orig", file);
                //}
                //catch
                //{
                    // TODO: When cannot download
                //}
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
