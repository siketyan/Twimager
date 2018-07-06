using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

            if (window.Tracking == null) return;

            Trackings.Add(window.Tracking);
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

        private void ResetTracking(object sender, RoutedEventArgs e)
        {
            if (!(TrackingsList.SelectedItem is ITracking tracking)) return;
            tracking.Reset();

            var dialog = new TaskDialog
            {
                Icon = TaskDialogStandardIcon.Information,
                StandardButtons = TaskDialogStandardButtons.Ok,
                Caption = "Twimager",
                InstructionText = "Successfully reset the tracking.",
                Text = "Update the tracking to scan and repair medias."
            };

            dialog.Show();
        }

        private void PurgeTracking(object sender, RoutedEventArgs e)
        {
            if (!(TrackingsList.SelectedItem is ITracking tracking)) return;

            var question = new TaskDialog
            {
                Icon = TaskDialogStandardIcon.Warning,
                StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No,
                Caption = "Twimager",
                InstructionText = "Are you sure you want to purge the tracking?",
                Text = "All medias will be lost permanently and this operation will be irreversible."
            };

            if (question.Show() != TaskDialogResult.Yes) return;

            tracking.Reset();
            new PurgeWindow(tracking).ShowAndPurge();
        }

        private void MoveTrackingUp(object sender, RoutedEventArgs e)
        {
            var index = TrackingsList.SelectedIndex;
            if (!(TrackingsList.SelectedItem is ITracking tracking) || index == 0) return;

            Trackings.Remove(tracking);
            Trackings.Insert(index - 1, tracking);
        }

        private void MoveTrackingDown(object sender, RoutedEventArgs e)
        {
            var index = TrackingsList.SelectedIndex;
            if (!(TrackingsList.SelectedItem is ITracking tracking) || index == Trackings.Count()) return;

            Trackings.Remove(tracking);
            Trackings.Insert(index + 1, tracking);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            var dialog = new TaskDialog
            {
                Icon = TaskDialogStandardIcon.Warning,
                StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No,
                Caption = "Twimager",
                InstructionText = "Are you sure you want to exit?",
                Text = "All working tasks will be canceled and the data may be lost."
            };

            e.Cancel = dialog.Show() != TaskDialogResult.Yes;
        }
    }
}
