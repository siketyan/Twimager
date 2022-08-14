using Microsoft.WindowsAPICodePack.Dialogs;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Twimager.Objects;
using Twimager.Utilities;

namespace Twimager.Windows
{
    /// <summary>
    /// PurgeWindow.xaml の相互作用ロジック
    /// </summary>
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
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
                OnPropertyChanged("Current");
            }
        }

        public long Count
        {
            get => _count;
            private set
            {
                _count = value;
                OnPropertyChanged("Count");
            }
        }

        private string _status = "Calculating...";
        private string _fileName;
        private readonly string _directory;
        private long _current;
        private long _count;
        private readonly App _app;
        private readonly Logger _logger;
        private readonly ITracking _tracking;

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
            await _logger.LogAsync($"Starting to purge: {_tracking}");

            var files = await Task.Run(() => Directory.GetFiles(_directory, "*.*", SearchOption.AllDirectories));

            Status = "Deleting files...";
            Count = files.Length;

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

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
