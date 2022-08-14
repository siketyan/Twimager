using System.Windows;
using Twimager.Objects;

namespace Twimager.Windows
{
    /// <summary>
    /// ConfigWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigWindow
    {
        public bool IgnoreRetweets { get; set; }
        public bool IgnoreReplies { get; set; }

        private readonly Config _config;

        public ConfigWindow()
        {
            InitializeComponent();

            _config = App.GetCurrent().Config;
            IgnoreRetweets = _config.IgnoreRetweets;
            IgnoreReplies = _config.IgnoreReplies;

            DataContext = this;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            _config.IgnoreRetweets = IgnoreRetweets;
            _config.IgnoreReplies = IgnoreReplies;
            _config.Save();

            Close();
        }
    }
}
