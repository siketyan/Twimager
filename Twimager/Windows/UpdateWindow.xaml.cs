using System.ComponentModel;
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

        private string _status;

        public UpdateWindow(Account account)
        {
            InitializeComponent();

            Account = account;
            DataContext = this;
        }

        public void ShowWithPosition()
        {
            var screen = Forms.Screen.PrimaryScreen;
            var area = screen.WorkingArea;

            Left = area.Right - Width - WindowMargin;
            Top = area.Bottom - Height - WindowMargin;

            Show();
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
