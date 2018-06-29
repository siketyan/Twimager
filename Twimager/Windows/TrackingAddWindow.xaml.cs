using CoreTweet;
using System.Collections.ObjectModel;
using System.Windows;
using Twimager.Objects;

namespace Twimager.Windows
{
    /// <summary>
    /// AccountAddWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TrackingAddWindow
    {
        public AccountTracking Account { get; private set; }
        public ObservableCollection<List> Lists { get; private set; }

        public TrackingAddWindow()
        {
            InitializeComponent();
        }

        private async void InitAsync(object sender, RoutedEventArgs e)
        {
            var lists = await App.GetCurrent().Twitter.Lists.ListAsync();
            Lists = new ObservableCollection<List>(lists);
        }

        private async void AddAccountTrackingAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                var screenName = ScreenName.Text;
                var user = await App.GetCurrent().Twitter.Users.ShowAsync(screenName);

                Account = new AccountTracking
                {
                    Id = (long)user.Id,
                    ScreenName = user.ScreenName,
                    Name = user.Name,
                    ProfileImageUrl = user.ProfileImageUrlHttps
                };
            }
            catch
            {
                // TODO: User not found
            }

            Close();
        }
    }
}
