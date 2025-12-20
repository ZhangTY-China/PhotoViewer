using System.IO;
using System.Windows;
using PhotoViewer.Utils;

namespace PhotoViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string Tag = "MainWindow";
    public MainWindow()
    {
        Logger.i(Tag, "App init");
        InitializeComponent();
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        FolderPathTextBox.Text = ToolUtils.OpenFolderDialog();
    }

    private void ViewImages_Click(object sender, RoutedEventArgs e)
    {
        string folderPath = FolderPathTextBox.Text;
        
        // 检查路径是否有效
        if (string.IsNullOrWhiteSpace(folderPath) || folderPath.Equals("请选择一个包含图片的文件夹路径") || !Directory.Exists(folderPath))
        {
            Logger.e(Tag, $"ViewImage_Click path invalid, path = {folderPath}");
            return;
        }

        // 获取文件夹中的所有图片文件
        var imagePaths = new List<string>();

        try
        {
            // 递归查找所有图片文件
            var files = Directory.GetFiles(folderPath!, "*.*", SearchOption.AllDirectories)
                .Where(file => ToolUtils.PhotoTypes.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .ToList();

            imagePaths.AddRange(files);

            if (imagePaths.Count == 0)
            {
                Logger.e(Tag,$"ViewImage_Click no photos in folder {folderPath}");
                return;
            }

            // 创建图片查看View
            var imageViewer = new ImageViewer();
        
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
        catch (Exception ex)
        {
            Logger.e(Tag,$"ViewImage_Click catch exception {ex.Message}");
        }
    }
}