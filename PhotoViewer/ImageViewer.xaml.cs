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
        private double initialZoomFactor = 1.0; // 记录初始缩放倍率

        private Point _dragStartPosition;
        private bool _isDragging = false;
        
        // 添加标志来跟踪动画是否正在进行
        private bool _isAnimating = false;

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
            
            // 记录初始缩放倍率
            initialZoomFactor = zoomFactor;
            
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
                
                double scale = e.Delta > 0 ? 1.1 : 0.9; // 调整回缩比例为 0.9
                
                // 计算新的缩放因子
                double newZoomFactor = zoomFactor * scale;
                
                // 限制缩放倍率不小于初始倍率（不能比原始倍率更小）
                if (newZoomFactor < initialZoomFactor)
                {
                    Logger.i(TAG, "Zoom factor would be smaller than initial zoom factor, limiting to initial zoom factor");
                    newZoomFactor = initialZoomFactor;
                }
                
                // 如果缩放因子没有变化，则不执行任何操作
                if (Math.Abs(newZoomFactor - zoomFactor) < 0.001)
                {
                    Logger.i(TAG, "Zoom factor unchanged, skipping zoom operation");
                    e.Handled = true;
                    return;
                }
                
                // 获取当前鼠标在ScrollViewer中的位置
                Point mousePositionInScrollViewer = e.GetPosition(ImageScrollViewer);
                
                // 计算鼠标在图片中的相对位置（考虑当前缩放和滚动偏移）
                double relativeX = (ImageScrollViewer.HorizontalOffset + mousePositionInScrollViewer.X) / zoomFactor;
                double relativeY = (ImageScrollViewer.VerticalOffset + mousePositionInScrollViewer.Y) / zoomFactor;
                
                // 更新缩放因子
                zoomFactor = newZoomFactor;
                
                // 直接设置图片大小，不使用动画
                DisplayImage.Width = ((BitmapImage)DisplayImage.Source).PixelWidth * zoomFactor;
                DisplayImage.Height = ((BitmapImage)DisplayImage.Source).PixelHeight * zoomFactor;
                
                // 计算新的滚动位置，确保鼠标指向的图片内容位置不变
                double newHorizontalOffset = relativeX * zoomFactor - mousePositionInScrollViewer.X;
                double newVerticalOffset = relativeY * zoomFactor - mousePositionInScrollViewer.Y;
                
                Logger.i(TAG, "new Offset (" + newHorizontalOffset + ", " + newVerticalOffset + ")   ");
                
                // 直接设置滚动位置，不使用动画
                ImageScrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
                ImageScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
                
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