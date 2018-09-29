using Microsoft.WindowsAPICodePack.Dialogs;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Twimager.Objects;
using Twimager.Utilities;

namespace Twimager.Windows
{
    /// <summary>
    /// PurgeWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PurgeWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Status
        {
            get => _status;
            private set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public string FileName
        {
            get => _fileName;
            private set
            {
                _fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        public long Current
        {
            get => _current;
            set
            {
                _current = value;
                Percentage = _current / _count;
                OnPropertyChanged("Current");
            }
        }

        public long Count
        {
            get => _count;
            set
            {
                _count = value;
                OnPropertyChanged("Count");
            }
        }

        public double Percentage
        {
            get => _percentage;
            set
            {
                _percentage = value;
                OnPropertyChanged("Percentage");
            }
        }

        private string _status = "Calclating...";
        private string _fileName;
        private string _directory;
        private long _current;
        private long _count;
        private double _percentage;
        private App _app;
        private Logger _logger;
        private ITracking _tracking;

        public PurgeWindow(ITracking tracking)
        {
            InitializeComponent();

            _app = App.GetCurrent();
            _logger = _app.Logger;
            _tracking = tracking;
            _directory = FileName = $"{App.Destination}/{tracking.Directory}";

            DataContext = this;
        }

        public void ShowAndPurge()
        {
            Show();
            PurgeAsync();
        }

        private async void PurgeAsync()
        {
            _app.IsBusy = true;
            await _logger.LogAsync($"Starting to purge: {_tracking.ToString()}");

            var files = await Task.Run(() =>
            {
                return Directory.GetFiles(_directory, "*.*", SearchOption.AllDirectories);
            });

            Status = "Deleting files...";
            Count = files.Count();

            foreach (var file in files)
            {
                FileName = file;
                Current++;

                await _logger.LogAsync($"- {file}");
                await Task.Run(() =>
                {
                    if (File.Exists(file)) File.Delete(file);
                });
            }

            if (Directory.Exists(_directory)) Directory.Delete(_directory);

            var dialog = new TaskDialog
            {
                Icon = TaskDialogStandardIcon.Information,
                StandardButtons = TaskDialogStandardButtons.Ok,
                Caption = "Twimager",
                InstructionText = "Successfully purge the medias of the tracking.",
                Text = "Update the tracking to re-download medias."
            };

            Close();
            dialog.Show();

            await _logger.LogAsync("Successfully purged the tracking");
            _app.IsBusy = false;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
