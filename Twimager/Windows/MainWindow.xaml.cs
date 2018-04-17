using System.Collections.Generic;
using System.Windows;
using Twimager.Objects;

namespace Twimager.Windows
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        private List<Account> _accounts;

        public MainWindow()
        {
            InitializeComponent();
            _accounts = new List<Account>();
        }

        private void AddAccount(object sender, RoutedEventArgs e)
        {
            var window = new AccountAddWindow();
            window.ShowDialog();

            _accounts.Add(window.Account);
        }
    }
}
