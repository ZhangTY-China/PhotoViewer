using System.Windows;

namespace PhotoViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string TAG = "MainWindow";
    public MainWindow()
    {
        Logger.i(TAG, "App init");
        InitializeComponent();
    }

    private void ViewImages_Click(object sender, RoutedEventArgs e)
    {
        // 创建图片查看View
        var imageViewer = new ImageViewer();

        // 模拟一些图片路径 - 实际应用中可以从文件对话框或数据库获取
        var imagePaths = new List<string>
        {
            "F:\\Shot By Z5Ⅱ\\20250726\\DSC_0055.JPG",
            "F:\\Shot By Z5Ⅱ\\20250726\\DSC_0068.JPG",
            "F:\\Shot By Z5Ⅱ\\20251001_无锡\\DSC_1176.JPG",
        };
        
        // 订阅 Loaded 事件
        imageViewer.Loaded += (_, _) =>
        {
            // 在这里调用你的函数
            imageViewer.LoadImages(imagePaths);
        };
        
        // 创建新窗口显示图片查看器
        var viewerWindow = new Window
        {
            Title = "Image Viewer",
            Content = imageViewer,
            Width = SystemParameters.PrimaryScreenWidth * 0.7,  // 屏幕宽度的70%
            Height = SystemParameters.PrimaryScreenWidth * 0.7 / 16 * 9, // 屏幕高度的70%
        };

        viewerWindow.ShowDialog();
    }
}