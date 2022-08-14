using CoreTweet;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Twimager.Enums;
using Twimager.Objects;
using Twimager.Utilities;

namespace Twimager.Windows
{
    /// <summary>
    /// StatusWindow.xaml の相互作用ロジック
    /// </summary>
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
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

        private bool _isCanceled;
        private string _status = "Initializing...";
        private readonly App _app;
        private readonly Logger _logger;
        private readonly Config _config;
        private readonly Tokens _twitter;

        public UpdateWindow(ITracking tracking)
        {
            InitializeComponent();

            Tracking = tracking;
            _app = App.GetCurrent();
            _logger = _app.Logger;
            _config = _app.Config;
            _twitter = _app.Twitter;

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
            _app.IsBusy = true;
            await _logger.LogAsync($"Starting to update: {Tracking}");

            await _logger.LogAsync($"Updating summary");
            var task = Tracking.UpdateSummaryAsync();
            if (task == null)
            {
                await _logger.LogAsync("The tracking does not need update summary, skipping");
            }
            else
            {
                await task;
                await _logger.LogAsync("Successfully updated summary");
            }

            var dir = $"{App.Destination}/{Tracking.Directory}";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                await _logger.LogAsync($"Created directory: {dir}");
            }

            using (var wc = new HttpClient())
            {
                while (true)
                {
                    IEnumerable<Status> statuses;

                    try
                    {
                        await _logger.LogAsync("Loading statuses");
                        statuses = await Tracking.GetStatusesAsync();
                    }
                    catch (TwitterException e)
                    {
                        await _logger.LogExceptionAsync(e);

                        if (e.RateLimit.Remaining == 0)
                        {
                            var dialog = new TaskDialog
                            {
                                Icon = TaskDialogStandardIcon.Error,
                                StandardButtons = TaskDialogStandardButtons.Ok,
                                Caption = "Twimager",
                                InstructionText = "The rate limit of Twitter API exceeded.",
                                Text = $"Try again after {e.RateLimit.Reset.LocalDateTime.ToString(CultureInfo.CurrentCulture)}"
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

                    var enumerable = statuses.ToList();
                    if (enumerable.All(x => x.Id == (Tracking.IsCompleted ? Tracking.Latest : Tracking.Oldest)))
                    {
                        if (!Tracking.IsCompleted)
                        {
                            Tracking.IsCompleted = true;

                            await _logger.LogAsync("Completed updating to the oldest status, saving");
                            _config.Save();
                        }

                        break;
                    }

                    var sorted = (Tracking.Oldest < enumerable.Last().Id) switch
                    {
                        true => enumerable.OrderByDescending(x => x.Id),
                        false => enumerable.OrderBy(x => x.Id),
                    };

                    foreach (var status in sorted)
                    {
                        await _logger.LogAsync($"+ {status.Id}");
                        var isCanceled = await DownloadMediaAsync(wc, status, dir);

                        if (Tracking.Oldest == null || Tracking.Oldest > status.Id) Tracking.Oldest = status.Id;
                        if (Tracking.Latest == null || Tracking.Latest < status.Id) Tracking.Latest = status.Id;
                        _config.Save();

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

            await _logger.LogAsync("Successfully updated the tracking");
            _app.IsBusy = false;
            Close();
        }

        private async Task<bool> DownloadMediaAsync(HttpClient wc, Status status, string destination)
        {
            var entities = status.ExtendedEntities;
            if (entities == null)
            {
#pragma warning disable CS0618
                if (status.IsTruncated ?? false)
#pragma warning restore CS0618
                {
                    await _logger.LogAsync("  Truncated status, loading full status");

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
                    string url, name, file = null;

                    try
                    {
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
                        await _logger.LogAsync($"  + {name}");

                        await using var stream = new FileStream(file, FileMode.Create, FileAccess.Write);
                        await (await wc.GetStreamAsync(url)).CopyToAsync(stream);
                    }
                    catch
                    {
                        if (File.Exists(file)) File.Delete(file);

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

                        retryBtn.Click += (_, _) =>
                        {
                            result = ErrorDialogResult.Retry;
                            dialog.Close();
                        };

                        skipBtn.Click += (_, _) =>
                        {
                            result = ErrorDialogResult.Skip;
                            dialog.Close();
                        };

                        cancelBtn.Click += (_, _) =>
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

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
