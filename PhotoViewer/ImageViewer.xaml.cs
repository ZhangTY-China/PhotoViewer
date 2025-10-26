using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PhotoViewer
{
    public partial class ImageViewer : UserControl
    {
        private List<string> imagePaths = new List<string>();
        private int currentIndex = 0;
        private double zoomFactor = 1.0;

        public ImageViewer()
        {
            InitializeComponent();
        }
        public void LoadImages(List<string> paths)
        {
            imagePaths = paths;
            if (imagePaths.Count > 0)
            {
                LoadImage(imagePaths[0]);
            }
        }

        private void LoadImage(String path)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.DecodePixelWidth = (int)ImageScrollViewer.ActualWidth;
            bitmap.EndInit();
            DisplayImage.Source = bitmap;

            zoomFactor = Math.Min(
                ImageScrollViewer.ActualWidth / bitmap.PixelWidth,
                ImageScrollViewer.ActualHeight / bitmap.PixelHeight
            );
            
            DisplayImage.Width = ((BitmapImage)DisplayImage.Source).PixelWidth * zoomFactor;
            DisplayImage.Height = ((BitmapImage)DisplayImage.Source).PixelHeight * zoomFactor;
        }

        private void PreviousImage_Click(object sender, RoutedEventArgs e)
        {
            if (imagePaths.Count == 0) return;

            currentIndex = (currentIndex - 1 + imagePaths.Count) % imagePaths.Count;
            LoadImage(imagePaths[currentIndex]);
        }

        private void NextImage_Click(object sender, RoutedEventArgs e)
        {
            if (imagePaths.Count == 0) return;

            currentIndex = (currentIndex + 1) % imagePaths.Count;
            LoadImage(imagePaths[currentIndex]);
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            zoomFactor *= 1.2;
            DisplayImage.Width = ((BitmapImage)DisplayImage.Source).PixelWidth * zoomFactor;
            DisplayImage.Height = ((BitmapImage)DisplayImage.Source).PixelHeight * zoomFactor;
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            zoomFactor /= 1.2;
            DisplayImage.Width = ((BitmapImage)DisplayImage.Source).PixelWidth * zoomFactor;
            DisplayImage.Height = ((BitmapImage)DisplayImage.Source).PixelHeight * zoomFactor;
        }
    }
}