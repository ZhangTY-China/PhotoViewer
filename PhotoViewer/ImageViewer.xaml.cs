using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
                DisplayImage.Source = new BitmapImage(new System.Uri(imagePaths[0]));
            }
        }

        private void PreviousImage_Click(object sender, RoutedEventArgs e)
        {
            if (imagePaths.Count == 0) return;

            currentIndex = (currentIndex - 1 + imagePaths.Count) % imagePaths.Count;
            DisplayImage.Source = new BitmapImage(new System.Uri(imagePaths[currentIndex]));
        }

        private void NextImage_Click(object sender, RoutedEventArgs e)
        {
            if (imagePaths.Count == 0) return;

            currentIndex = (currentIndex + 1) % imagePaths.Count;
            DisplayImage.Source = new BitmapImage(new System.Uri(imagePaths[currentIndex]));
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            zoomFactor *= 1.2;
            DisplayImage.Width = DisplayImage.Source.Width * zoomFactor;
            DisplayImage.Height = DisplayImage.Source.Height * zoomFactor;
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            zoomFactor /= 1.2;
            DisplayImage.Width = DisplayImage.Source.Width * zoomFactor;
            DisplayImage.Height = DisplayImage.Source.Height * zoomFactor;
        }
    }
}