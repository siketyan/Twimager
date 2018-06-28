using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void AddTracking(object sender, RoutedEventArgs e)
        {
            var window = new TrackingAddWindow();
            window.ShowDialog();

            if (window.Account == null) return;

            Trackings.Add(window.Account);
            _app.Config.Save();
        }

        private void RemoveTracking(object sender, RoutedEventArgs e)
        {
            var tracking = TrackingsList.SelectedItem;
            if (tracking == null || !(tracking is ITracking)) return;

            Trackings.Remove(tracking as ITracking);
            _app.Config.Save();
        }

        private void UpdateTracking(object sender, RoutedEventArgs e)
        {
            new UpdateWindow(
                (sender as Button).Tag as ITracking
            ).ShowAndUpdate();
        }

        private void UpdateAll(object sender, RoutedEventArgs e)
        {
            if (!Trackings.Any())
            {
                var dialog = new TaskDialog
                {
                    Icon = TaskDialogStandardIcon.Error,
                    StandardButtons = TaskDialogStandardButtons.Ok,
                    Caption = "Twimager",
                    InstructionText = "Couldn't find any trackings.",
                    Text = "You have to add tracking(s) before update."
                };

                dialog.Show();
                return;
            }

            _current = 0;
            UpdateNext();
        }

        private void UpdateNext()
        {
            var tracking = Trackings[_current];
            var window = new UpdateWindow(tracking);
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

        private void Deselect(object sender, MouseButtonEventArgs e)
        {
            TrackingsList.SelectedItem = null;
        }
    }
}
