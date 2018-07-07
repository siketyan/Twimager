using Microsoft.WindowsAPICodePack.Dialogs;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using CoreTweet;
using Twimager.Enums;
using Twimager.Objects;
using System.Collections.Generic;
using System;

namespace Twimager.Windows
{
    /// <summary>
    /// StatusWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class UpdateWindow : INotifyPropertyChanged
    {
        private const int WindowMargin = 32;

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

        private bool _isCanceled = false;
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
            var area = SystemParameters.WorkArea;

            Left = area.Right - Width - WindowMargin;
            Top = area.Bottom - Height - WindowMargin;
        }

        private async void UpdateAsync()
        {
            App.GetCurrent().IsBusy = true;

            var task = Tracking.UpdateSummaryAsync();
            if (task != null) await task;

            var dir = $"{App.Destination}/{Tracking.Directory}";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using (var wc = new WebClient())
            {
                while (true)
                {
                    IEnumerable<Status> statuses;

                    try
                    {
                        statuses = await Tracking.GetStatusesAsync();
                    }
                    catch (TwitterException e)
                    {
                        if (e.RateLimit.Remaining == 0)
                        {
                            var dialog = new TaskDialog
                            {
                                Icon = TaskDialogStandardIcon.Error,
                                StandardButtons = TaskDialogStandardButtons.Ok,
                                Caption = "Twimager",
                                InstructionText = "The rate limit of Twitter API exceeded.",
                                Text = $"Try again after {e.RateLimit.Reset.LocalDateTime.ToString()}"
                            };

                            dialog.Show();
                        }
                        else
                        {
                            var dialog = new TaskDialog
                            {
                                Icon = TaskDialogStandardIcon.Error,
                                StandardButtons = TaskDialogStandardButtons.Ok,
                                Caption = "Twimager",
                                InstructionText = "Failed to get statuses from Twitter API.",
                                Text = $"Check the tracking target exists and try again.",
                                DetailsExpandedText = e.Message,
                                ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandContent
                            };

                            dialog.Show();
                        }

                        break;
                    }

                    if (!statuses.Any(x => x.Id != (Tracking.IsCompleted ? Tracking.Latest : Tracking.Oldest)))
                    {
                        if (!Tracking.IsCompleted)
                        {
                            Tracking.IsCompleted = true;
                            App.GetCurrent().Config.Save();
                        }

                        break;
                    }

                    if (Tracking.Oldest < statuses.Last().Id)
                    {
                        statuses.OrderByDescending(x => x.Id);
                    }
                    else
                    {
                        statuses.OrderBy(x => x.Id);
                    }
                    
                    foreach (var status in statuses)
                    {
                        var isCanceled = await DownloadMediaAsync(wc, status, dir);
                        
                        if (Tracking.Oldest == null || Tracking.Oldest > status.Id) Tracking.Oldest = status.Id;
                        if (Tracking.Latest == null || Tracking.Latest < status.Id) Tracking.Latest = status.Id;
                        App.GetCurrent().Config.Save();

                        if (!_isCanceled) _isCanceled = isCanceled;
                        if (isCanceled) break;
                    }

                    if (_isCanceled)
                    {
                        Close();
                        return;
                    }
                }
            }
            
            App.GetCurrent().IsBusy = false;
            Close();
        }

        private async Task<bool> DownloadMediaAsync(WebClient wc, Status status, string destination)
        {
            var entities = status.ExtendedEntities;
            if (entities == null)
            {
                if (status.IsTruncated ?? false)
                {
                    entities = (
                        await _twitter.Statuses.ShowAsync(
                            status.Id,
                            tweet_mode: TweetMode.Extended
                        )
                    ).ExtendedEntities;
                }

                if (entities == null) return false;
            }

            foreach (var media in entities.Media)
            {
                var result = ErrorDialogResult.Retry;

                while (true)
                {
                    try
                    {
                        string url, name, file;

                        if (media.Type == "video")
                        {
                            var variant = media.VideoInfo.Variants.OrderByDescending(x => x.Bitrate ?? 0).First();

                            url = variant.Url.Split('?').First();
                            name = Path.GetFileName(url);
                            file = $"{destination}/{name}";
                        }
                        else
                        {
                            url = media.MediaUrlHttps;
                            name = Path.GetFileName(url);
                            file = $"{destination}/{name}";

                            url += ":orig";
                        }

                        Status = name;
                        
                        if (File.Exists(file)) break;
                        await wc.DownloadFileTaskAsync(url, file);
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
