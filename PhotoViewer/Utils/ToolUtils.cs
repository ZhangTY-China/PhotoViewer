namespace PhotoViewer.Utils;

public static class ToolUtils
{
    private const string Tag = "ToolUtils";
    public static readonly string[] PhotoTypes = [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".ico"];
    public static string OpenFolderDialog()
    {
        // 使用 WPF 的方式实现文件夹选择，不依赖 WindowsForms
        try
        {
            // 方法1：使用 OpenFileDialog 并获取其目录路径
            var openFileDialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "选择文件夹",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // 获取所选文件所在目录
                return openFileDialog.FolderName;
            }
            Logger.E(Tag, "OpenFolderDialog ShowDialog = false");
        }
        catch (Exception ex)
        {
            Logger.E(Tag, $"OpenFolderDialog catch exception = {ex.Message}");
        }

        return "";
    }
}