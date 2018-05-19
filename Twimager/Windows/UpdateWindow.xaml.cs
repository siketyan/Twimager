﻿using CoreTweet;
using Microsoft.WindowsAPICodePack.Dialogs;
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
                if (Account.Latest == null)
                {
                    long? oldest = null;
                    var isFirst = true;
                    while (true)
                    {
                        Status = "Loading timeline...";

                        var statuses = await _twitter.Statuses.UserTimelineAsync(
                            Account.Id,
                            200,
                            trim_user: true,
                            exclude_replies: false,
                            include_rts: false,
                            max_id: oldest
                        );
                        
                        if (statuses.Count <= 1) break;
                        foreach (var status in statuses)
                        {
                            if (await DownloadMediaAsync(wc, status, dir)) return;
                        }

                        oldest = statuses.Last().Id;

                        if (isFirst)
                        {
                            Account.Latest = statuses.First().Id;
                            isFirst = false;
                        }
                    }
                }
                else
                {
                    while (true)
                    {
                        Status = "Loading timeline...";

                        var statuses = await _twitter.Statuses.UserTimelineAsync(
                            Account.Id,
                            200,
                            trim_user: true,
                            exclude_replies: false,
                            include_rts: false,
                            since_id: Account.Latest
                        );
                        
                        if (!statuses.Any()) break;
                        foreach (var status in statuses)
                        {
                            if (await DownloadMediaAsync(wc, status, dir)) return;
                        }

                        Account.Latest = statuses.First().Id;
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