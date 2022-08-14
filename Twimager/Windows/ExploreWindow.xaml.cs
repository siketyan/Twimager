using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Twimager.Objects;
using Twimager.Utilities;
using Controls = System.Windows.Controls;

namespace Twimager.Windows
{
    /// <summary>
    /// ExploreWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ExploreWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Image> Images { get; }
        public ITracking Tracking { get; }

        private readonly Logger _logger;

        public ExploreWindow(ITracking tracking)
        {
            InitializeComponent();

            Images = new ObservableCollection<Image>();
            Tracking = tracking;
            _logger = App.GetCurrent().Logger;

            DataContext = this;
        }

        private async void InitAsync(object sender, RoutedEventArgs e)
        {
            await _logger.LogAsync("Loading images");

            var directory = $"{App.Destination}/{Tracking.Directory}";
            var files = Directory.GetFiles(directory);
            foreach (var file in files)
            {
                var extension = file.Split('.').Last().ToLower();
                if (extension != "jpg" && extension != "jpeg" &&
                    extension != "png" && extension != "gif")
                {
                    continue;
                }

                Images.Add(
                    new Image
                    {
                        Path = $"{AppDomain.CurrentDomain.BaseDirectory}/{file}".Replace('\\', '/')
                    }
                );
            }

            Loading.Visibility = Visibility.Collapsed;
        }

        private void PreviousImage(object sender, RoutedEventArgs e)
        {
            if (ImagesList.SelectedIndex == 0)
            {
                ImagesList.SelectedIndex = Images.Count - 1;
            }
            else
            {
                ImagesList.SelectedIndex--;
            }
        }

        private void NextImage(object sender, RoutedEventArgs e)
        {
            if (ImagesList.SelectedIndex == Images.Count - 1)
            {
                ImagesList.SelectedIndex = 0;
            }
            else
            {
                ImagesList.SelectedIndex++;
            }
        }

        private void CloseViewer(object sender, RoutedEventArgs e)
        {
            Viewer.Visibility = Visibility.Hidden;
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            var image = ImagesList.SelectedItem;
            if (image == null || !(image is Image)) return;

            var source = new BitmapImage();
            source.BeginInit();
            source.UriSource = new Uri((image as Image).Path);
            source.EndInit();

            SelectedImage.Source = source;
            Viewer.Visibility = Visibility.Visible;
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var viewer = sender as Controls.ScrollViewer;
            viewer?.ScrollToVerticalOffset(viewer.VerticalOffset - e.Delta);

            e.Handled = true;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
