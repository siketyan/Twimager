﻿using System.Collections.ObjectModel;
using System.Windows;
using CoreTweet;
using Microsoft.WindowsAPICodePack.Dialogs;
using Twimager.Objects;

namespace Twimager.Windows
{
    /// <summary>
    /// AccountAddWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TrackingAddWindow
    {
        public ITracking Tracking { get; private set; }
        public ObservableCollection<List> Lists { get; private set; }

        public TrackingAddWindow()
        {
            InitializeComponent();

            Lists = new ObservableCollection<List>();
            DataContext = this;
        }

        private async void InitAsync(object sender, RoutedEventArgs e)
        {
            foreach (var list in await App.GetCurrent().Twitter.Lists.ListAsync(true))
            {
                Lists.Add(list);
            }
        }

        private async void AddAccountTrackingAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                var screenName = ScreenName.Text;
                var user = await App.GetCurrent().Twitter.Users.ShowAsync(screenName);

                Tracking = new AccountTracking
                {
                    Id = (long)user.Id,
                    ScreenName = user.ScreenName,
                    Name = user.Name,
                    ProfileImageUrl = user.ProfileImageUrlHttps
                };
            }
            catch
            {
                var dialog = new TaskDialog
                {
                    Icon = TaskDialogStandardIcon.Error,
                    StandardButtons = TaskDialogStandardButtons.Ok,
                    Caption = "Twimager",
                    InstructionText = "The user is not found.",
                    Text = "You have to put screen name of the user that exists into the box."
                };

                dialog.Show();
                return;
            }

            Close();
        }

        private void AddListTracking(object sender, RoutedEventArgs e)
        {
            var item = ListName.SelectedItem;
            if (item == null || !(item is List))
            {
                var dialog = new TaskDialog
                {
                    Icon = TaskDialogStandardIcon.Error,
                    StandardButtons = TaskDialogStandardButtons.Ok,
                    Caption = "Twimager",
                    InstructionText = "List is not selected.",
                    Text = "You have to select a list from the dropdown box."
                };

                dialog.Show();
                return;
            }

            var list = item as List;
            Tracking = new ListTracking
            {
                Id = list.Id,
                Name = list.Name,
                FullName = list.FullName
            };
            
            Close();
        }
    }
}
