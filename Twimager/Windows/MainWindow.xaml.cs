using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Twimager.Enums;
using Twimager.Objects;

namespace Twimager.Windows
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        public ObservableCollection<Account> Accounts { get; }

        private int _current;
        private App _app;

        public MainWindow()
        {
            InitializeComponent();

            _app = App.GetCurrent();
            Accounts = _app.Config.Accounts;

            DataContext = this;
        }

        private void AddAccount(object sender, RoutedEventArgs e)
        {
            var window = new AccountAddWindow();
            window.ShowDialog();

            if (window.Account == null) return;

            Accounts.Add(window.Account);
            _app.Config.Save();
        }

        private void RemoveAccount(object sender, RoutedEventArgs e)
        {
            var account = AccountsList.SelectedItem;
            if (account == null) return;

            Accounts.Remove(account as Account);
            _app.Config.Save();
        }

        private void UpdateAccount(object sender, RoutedEventArgs e)
        {
            var id = (long)(sender as Control).Tag;
            var account = Accounts.FirstOrDefault(x => x.Id == id);

            new UpdateWindow(account, UpdateType.Past).ShowAndUpdate();
        }

        private void UpdateAll(object sender, RoutedEventArgs e)
        {
            _current = 0;
            UpdateNext();
        }

        private void UpdateNext()
        {
            var account = Accounts[_current];
            var window = new UpdateWindow(account, UpdateType.Past);
            if (_current < Accounts.Count - 1)
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
