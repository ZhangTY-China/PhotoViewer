using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace PhotoViewer
{
    public partial class ImageViewer : UserControl
    {
        private const string TAG = "ImageViewer";
        private List<string> imagePaths = new List<string>();
        private int currentIndex = 0;
        private double zoomFactor = 1.0;

        private Point _dragStartPosition;
        private bool _isDragging = false;

        public ImageViewer()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Focusable = true;
            Focus();
            
            KeyUp += OnKeyUpHandler;
            ImageScrollViewer.PreviewMouseWheel += OnMouseWheelHandler;
            ImageScrollViewer.PreviewMouseDown += OnMouseDown;
            ImageScrollViewer.MouseMove += OnMouseMove;
            ImageScrollViewer.PreviewMouseUp += OnMouseUp;
        }
        
        public void LoadImages(List<string> paths)
        {
            Logger.i(TAG, "load image paths = " + paths);
            imagePaths = paths;
            if (imagePaths.Count > 0)
            {
                LoadImage(imagePaths[0]);
            }
        }

        private void LoadImage(String path)
        {
            Logger.i(TAG, "load image path = " + path);
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
            
            // 确保控件获得焦点，以便接收鼠标滚轮事件
            Focusable = true;
            Focus();
        }
        
        private void MoveImageIndex(int i)
        {
            if (imagePaths.Count == 0) return;
            currentIndex = (currentIndex + i + imagePaths.Count) % imagePaths.Count;
            LoadImage(imagePaths[currentIndex]);
        }
        
        private void OnKeyUpHandler(object sender, KeyEventArgs e)
        {
            Logger.i(TAG, "Key pressed: " + e.Key);
            switch (e.Key)
            {
                case Key.Left:
                    MoveImageIndex(-1);
                    break;
                case Key.Right:
                    MoveImageIndex(1);
                    break;
                
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _dragStartPosition = e.GetPosition(ImageScrollViewer);
                _isDragging = true;
                ImageScrollViewer.CaptureMouse();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPosition = e.GetPosition(ImageScrollViewer);
                double offsetX = currentPosition.X - _dragStartPosition.X;
                double offsetY = currentPosition.Y - _dragStartPosition.Y;

                ImageScrollViewer.ScrollToHorizontalOffset(ImageScrollViewer.HorizontalOffset - offsetX);
                ImageScrollViewer.ScrollToVerticalOffset(ImageScrollViewer.VerticalOffset - offsetY);

                _dragStartPosition = currentPosition;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                _isDragging = false;
                ImageScrollViewer.ReleaseMouseCapture();
            }
        }

        private void OnMouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            Logger.i(TAG, "Mouse wheel event triggered with delta: " + e.Delta);
            
            // 如果按下了Ctrl键，执行缩放操作
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Logger.i(TAG, "Ctrl key is pressed, performing zoom");
                Point mousePosition = e.GetPosition(DisplayImage);
                Logger.i(TAG, "Offset (" + ImageScrollViewer.HorizontalOffset + ", " + ImageScrollViewer.VerticalOffset + ") | Viewport (" + ImageScrollViewer.ViewportHeight + ", " + ImageScrollViewer.ViewportWidth + ") | mouse (" + mousePosition.X + ", " + mousePosition.Y + ")");

                double scale = e.Delta > 0 ? 1.1 : 0.9; // 调整回缩比例为 0.9
                zoomFactor *= scale;

                // Calculate the center of the zoom relative to the image
                double imageCenterX = mousePosition.X * zoomFactor;
                double imageCenterY = mousePosition.Y * zoomFactor;

                // Smoothly animate width and height changes
                DoubleAnimation widthAnimation = new DoubleAnimation(
                    ((BitmapImage)DisplayImage.Source).PixelWidth * zoomFactor,
                    new Duration(TimeSpan.FromMilliseconds(200)));
                DoubleAnimation heightAnimation = new DoubleAnimation(
                    ((BitmapImage)DisplayImage.Source).PixelHeight * zoomFactor,
                    new Duration(TimeSpan.FromMilliseconds(200)));
                widthAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                heightAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                DisplayImage.BeginAnimation(WidthProperty, widthAnimation);
                DisplayImage.BeginAnimation(HeightProperty, heightAnimation);

                // Calculate the new scroll position relative to the mouse position
                // double newHorizontalOffset = (mousePosition.X * scale - ImageScrollViewer.ViewportWidth / 2) + (ImageScrollViewer.HorizontalOffset - mousePosition.X) * (scale - 1);
                // double newVerticalOffset = (mousePosition.Y * scale - ImageScrollViewer.ViewportHeight / 2) + (ImageScrollViewer.VerticalOffset - mousePosition.Y) * (scale - 1);

                double newVerticalOffset = scale * ImageScrollViewer.VerticalOffset + (1 - scale) * (ImageScrollViewer.ViewportHeight / 2 - mousePosition.Y);
                double newHorizontalOffset = scale * ImageScrollViewer.HorizontalOffset + (1 - scale) * (ImageScrollViewer.ViewportWidth / 2 - mousePosition.X);
                Logger.i(TAG, "new Offset (" + newHorizontalOffset + ", " + newVerticalOffset + ")   ");
                
                // Apply smooth scrolling animation
                DoubleAnimation horizontalScrollAnimation = new DoubleAnimation(
                    newHorizontalOffset,
                    new Duration(TimeSpan.FromMilliseconds(200)));
                DoubleAnimation verticalScrollAnimation = new DoubleAnimation(
                    newVerticalOffset,
                    new Duration(TimeSpan.FromMilliseconds(200)));
                horizontalScrollAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                verticalScrollAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                ImageScrollViewer.BeginAnimation(ScrollViewerBehavior.HorizontalOffsetProperty, horizontalScrollAnimation);
                ImageScrollViewer.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, verticalScrollAnimation);
                
                // 标记事件已处理，防止ScrollViewer处理滚动
                e.Handled = true;
            }
            else
            {
                // 如果没有按下Ctrl键，让ScrollViewer处理正常的滚动
                // 不设置e.Handled = true，让事件继续传递
                Logger.i(TAG, "No Ctrl key, allowing normal scrolling");
            }
        }
    }
}