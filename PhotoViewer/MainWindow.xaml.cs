using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PhotoViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ViewImages_Click(object sender, RoutedEventArgs e)
    {
        // 创建图片查看View
        var imageViewer = new ImageViewer();

        // 模拟一些图片路径 - 实际应用中可以从文件对话框或数据库获取
        var imagePaths = new List<string>
        {
            "C:\\Users\\Public\\Pictures\\Sample Pictures\\Chrysanthemum.jpg",
            "C:\\Users\\Public\\Pictures\\Sample Pictures\\Desert.jpg",
            "C:\\Users\\Public\\Pictures\\Sample Pictures\\Hydrangeas.jpg"
        };

        imageViewer.LoadImages(imagePaths);

        // 创建新窗口显示图片查看器
        var viewerWindow = new Window
        {
            Title = "Image Viewer",
            Content = imageViewer,
            Width = 800,
            Height = 600
        };

        viewerWindow.ShowDialog();
    }
}