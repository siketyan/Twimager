using CoreTweet;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
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
        
        public ITracking Tracking { get; }

        private string _status = "Initializing...";
        private Tokens _twitter;
        
        public UpdateWindow(ITracking tracking)
        {
            InitializeComponent();

            Tracking = tracking;
            _twitter = App.GetCurrent().Twitter;

            DataContext = this;
        }

        public void ShowAndUpdate()
        {
            SetPosition();
            Show();
            UpdateAsync();
        }

        private void SetPosition()
        {
            var screen = Forms.Screen.PrimaryScreen;
            var area = screen.WorkingArea;

            Left = area.Right - Width - WindowMargin;
            Top = area.Bottom - Height - WindowMargin;
        }

        private async void UpdateAsync()
        {
            var dir = $"{Destination}/{Tracking.Directory}";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using (var wc = new WebClient())
            {
                while (true)
                {
                    var statuses = await Tracking.GetStatusesAsync();
                    if (statuses == null) break;

                    foreach (var status in statuses)
                    {
                        await DownloadMediaAsync(wc, status, dir);
                    }
                }
            }

            App.GetCurrent().Config.Save();
            Close();
        }

        private async Task<bool> DownloadMediaAsync(WebClient wc, Status status, string destination)
        {
            var entities = status.ExtendedEntities;
            if (entities == null) return false;

            foreach (var media in entities.Media)
            {
                var result = ErrorDialogResult.Retry;

                while (true)
                {
                    try
                    {
                        var url = media.MediaUrlHttps;
                        var name = Path.GetFileName(url);
                        var file = $"{destination}/{name}";

                        Status = name;

                        if (File.Exists(file)) break;
                        await wc.DownloadFileTaskAsync($"{url}:orig", file);
                    }
                    catch
                    {
                        var dialog = new TaskDialog
                        {
                            Icon = TaskDialogStandardIcon.Error,
                            Caption = "Twimager",
                            InstructionText = "Failed to download the image.",
                            Text = "An error occured while downloading."
                        };

                        var retryBtn = new TaskDialogButton { Text = "Retry" };
                        var skipBtn = new TaskDialogButton { Text = "Skip" };
                        var cancelBtn = new TaskDialogButton { Text = "Cancel" };

                        retryBtn.Click += (sender, e) =>
                        {
                            result = ErrorDialogResult.Retry;
                            dialog.Close();
                        };

                        skipBtn.Click += (sender, e) =>
                        {
                            result = ErrorDialogResult.Skip;
                            dialog.Close();
                        };

                        cancelBtn.Click += (sender, e) =>
                        {
                            result = ErrorDialogResult.Cancel;
                            dialog.Close();
                        };

                        dialog.Controls.Add(retryBtn);
                        dialog.Controls.Add(skipBtn);
                        dialog.Controls.Add(cancelBtn);
                        dialog.Show();
                    }

                    if (result != ErrorDialogResult.Retry) break;
                }

                if (result == ErrorDialogResult.Cancel)
                {
                    Close();
                    return true;
                }
            }

            return false;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
