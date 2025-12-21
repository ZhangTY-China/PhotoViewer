using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PhotoViewer.Utils;

namespace PhotoViewer
{
    public partial class ImageViewer
    {
        private new const string Tag = "ImageViewer";
        private List<string> _imagePaths = new List<string>();
        private int _currentIndex;
        private double _zoomFactor = 1.0;
        private double _maxZoomFactor = 1.0; // 记录初始缩放倍率

        private Point _dragStartPosition;
        private bool _isDragging;


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
            Logger.I(Tag, "load image paths = " + paths);
            _imagePaths = paths;
            if (_imagePaths.Count > 0)
            {
                LoadImage(_imagePaths[0]);
            }
        }

        private void LoadImage(String path)
        {
            Logger.I(Tag, "load image path = " + path);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            // 移除DecodePixelWidth限制，让图片以原始分辨率加载
            // 添加缓存选项以提高性能
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            // 确保图片以高质量加载
            bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmap.EndInit();
            // 确保图片数据完全加载
            bitmap.Freeze();
            DisplayImage.Source = bitmap;

            // 计算适合窗口的初始缩放比例
            if (bitmap.PixelWidth > 0 && bitmap.PixelHeight > 0)
            {
                _zoomFactor = Math.Min(
                    ImageScrollViewer.ActualWidth / bitmap.PixelWidth,
                    ImageScrollViewer.ActualHeight / bitmap.PixelHeight
                );
                
                // 记录初始缩放倍率
                _maxZoomFactor = _zoomFactor;
                
                DisplayImage.Width = bitmap.PixelWidth * _zoomFactor;
                DisplayImage.Height = bitmap.PixelHeight * _zoomFactor;
            }
            
            // 确保控件获得焦点，以便接收鼠标滚轮事件
            Focusable = true;
            Focus();
        }
        
        private void MoveImageIndex(int i)
        {
            if (_imagePaths.Count == 0) return;
            _currentIndex = (_currentIndex + i + _imagePaths.Count) % _imagePaths.Count;
            LoadImage(_imagePaths[_currentIndex]);
        }
        
        private void OnKeyUpHandler(object sender, KeyEventArgs e)
        {
            Logger.I(Tag, "Key pressed: " + e.Key);
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
            Logger.I(Tag, "Mouse wheel event triggered with delta: " + e.Delta);
            double scale = e.Delta > 0 ? 1.1 : 0.9; // 调整回缩比例为 0.9
            
            // 计算新的缩放因子
            double newZoomFactor = _zoomFactor * scale;
            
            // 限制缩放倍率不小于初始倍率（不能比原始倍率更小）
            if (newZoomFactor < _maxZoomFactor)
            {
                Logger.I(Tag, "Zoom factor would be smaller than initial zoom factor, limiting to initial zoom factor");
                newZoomFactor = _maxZoomFactor;
            }
            
            // 如果缩放因子没有变化，则不执行任何操作
            if (Math.Abs(newZoomFactor - _zoomFactor) < 0.001)
            {
                Logger.I(Tag, "Zoom factor unchanged, skipping zoom operation");
                e.Handled = true;
                return;
            }
            
            // 获取当前鼠标在ScrollViewer中的位置
            Point mousePositionInScrollViewer = e.GetPosition(ImageScrollViewer);
            
            // 计算鼠标在图片中的相对位置（考虑当前缩放和滚动偏移）
            double relativeX = (ImageScrollViewer.HorizontalOffset + mousePositionInScrollViewer.X) / _zoomFactor;
            double relativeY = (ImageScrollViewer.VerticalOffset + mousePositionInScrollViewer.Y) / _zoomFactor;
            
            // 更新缩放因子
            _zoomFactor = newZoomFactor;
            
            // 直接设置图片大小，不使用动画
            DisplayImage.Width = ((BitmapImage)DisplayImage.Source).PixelWidth * _zoomFactor;
            DisplayImage.Height = ((BitmapImage)DisplayImage.Source).PixelHeight * _zoomFactor;
            
            // 计算新的滚动位置，确保鼠标指向的图片内容位置不变
            double newHorizontalOffset = relativeX * _zoomFactor - mousePositionInScrollViewer.X;
            double newVerticalOffset = relativeY * _zoomFactor - mousePositionInScrollViewer.Y;
            
            Logger.I(Tag, "new Offset (" + newHorizontalOffset + ", " + newVerticalOffset + ")   ");
            
            // 直接设置滚动位置，不使用动画
            ImageScrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
            ImageScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
            
            // 标记事件已处理，防止ScrollViewer处理滚动
            e.Handled = true;
        }
    }
}