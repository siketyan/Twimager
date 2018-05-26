using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Twimager.Objects;

namespace Twimager.Windows
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        public ObservableCollection<ITracking> Trackings { get; }

        private int _current;
        private App _app;

        public MainWindow()
        {
            InitializeComponent();

            _app = App.GetCurrent();
            Trackings = _app.Config.Trackings;

            DataContext = this;
        }

        private void AddAccount(object sender, RoutedEventArgs e)
        {
            var window = new AccountAddWindow();
            window.ShowDialog();

            if (window.Account == null) return;

            Trackings.Add(window.Account);
            _app.Config.Save();
        }

        private void RemoveAccount(object sender, RoutedEventArgs e)
        {
            var account = AccountsList.SelectedItem;
            if (account == null) return;

            Trackings.Remove(account as Account);
            _app.Config.Save();
        }

        private void UpdateAccount(object sender, RoutedEventArgs e)
        {
            var id = (long)(sender as Control).Tag;
            var account = Trackings.FirstOrDefault(x => (x as Account).Id == id);

            new UpdateWindow(account).ShowAndUpdate();
        }

        private void UpdateAll(object sender, RoutedEventArgs e)
        {
            _current = 0;
            UpdateNext();
        }

        private void UpdateNext()
        {
            var account = Trackings[_current];
            var window = new UpdateWindow(account);
            if (_current < Trackings.Count - 1)
            {
                window.Closing += (sender, e) =>
                {
                    UpdateNext();
                };
            }

            window.ShowAndUpdate();
            _current++;
        }
    }
}
