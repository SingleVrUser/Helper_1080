using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Helper_1080.ContentPages.Tools
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class rar : Page
    {
        ObservableCollection<fileInfo> FileNameList = new();
        List<string> shareBaiduLinkList = new();
        List<string> share115LinkList = new();
        List<string> down115LinkList = new();
        List<string> fichierList = new();

        //保存路径
        string SaveRAReXtractFilesPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "eXtract");

        public rar()
        {
            this.InitializeComponent();
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Link;

            e.DragUIOverride.Caption = "拖拽获取压缩包内文件信息";

        }

        private async void tryAddtoClipboard(string ClipboardText)
        {


            //创建一个数据包
            DataPackage dataPackage = new DataPackage();
            //设置创建包里的文本内容
            //string ClipboardText = JsonSerializer.Serialize(content);
            dataPackage.SetText(ClipboardText);

            //把数据包放到剪贴板里
            Clipboard.SetContent(dataPackage);

            DataPackageView dataPackageView = Clipboard.GetContent();
            string text = await dataPackageView.GetTextAsync();
            if (text == ClipboardText)
            {
                LightDismissTeachingTip.Subtitle = @"已添加到剪贴板";
                LightDismissTeachingTip.IsOpen = true;

                await Task.Delay(1000);
                LightDismissTeachingTip.IsOpen = false;
            }
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            //获取拖入文件信息
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();

                List<string> filesPath = new();

                //显示进度条
                RarProgressBar.Visibility = Visibility;

                await Task.Delay(1000);

                FileNameList.Clear();

                foreach (var item in items)
                {
                    //文件
                    if (item.IsOfType(StorageItemTypes.File))
                    {
                        var File = item as StorageFile;

                        //rar压缩包
                        if (File.FileType == ".rar")
                        {
                            FileNameList.Add(new fileInfo() { Name = File.DisplayName, Path = File.Path ,FileType = FileType.Rar});
                        }
                        //文本
                        else if (File.FileType == ".txt")
                        {
                            FileNameList.Add(new fileInfo() { Name = File.DisplayName, Path = File.Path, FileType = FileType.Txt });
                        }
                    }
                }

                infoBar.Severity = InfoBarSeverity.Success;
                infoBar.Message = $"拖入{items.Count}个文件";
            }

            if (FileNameList.Count == 0) return;

            VisualStateManager.GoToState(this, "ShowDropResult", true);

            foreach (var item in FileNameList)
            {
                //fc2969832.txt
                FileInfo detailFile = null;

                switch (item.FileType)
                {
                    case FileType.Rar:
                    {
                        var fileName = Path.GetFileNameWithoutExtension(item.Path);
                        var outPath = Path.Combine(SaveRAReXtractFilesPath, fileName);

                        if (!Directory.Exists(outPath))
                        {
                            Helper.local.ProgressRun("bz", $"x -y -o:{outPath} {item.Path}");
                        }

                        var theFolder = new DirectoryInfo(outPath);
                        var filesList = theFolder.GetFiles();
                        detailFile = filesList.FirstOrDefault(x => Regex.Match(x.Name, @"[a-z0-9]\.txt", RegexOptions.IgnoreCase).Success);

                        //百度轉存二維碼,提取碼(注意區分數字1和字母l)：877a.png
                        var shareImage = filesList.FirstOrDefault(x => x.Extension == ".png");
                        if (shareImage == null) continue;

                        item.QRcodeImagePath = shareImage.FullName;

                        item.baiduShareInfo = new();

                        var SharePassword = "解压密码";
                        var passwordResult = Regex.Match(shareImage.Name, @"百度轉存二維碼,提取碼\(注意區分數字1和字母l\)：(\w*)");
                        if (passwordResult.Success)
                        {
                            SharePassword = passwordResult.Groups[1].Value;
                            item.baiduShareInfo.sharePassword = SharePassword;
                        }

                        Bitmap image;
                        image = (Bitmap)System.Drawing.Image.FromFile(shareImage.FullName);
                        LuminanceSource source;
                        source = new BitmapLuminanceSource(image);
                        var bitmap = new BinaryBitmap(new HybridBinarizer(source));
                        var result = new MultiFormatReader().decode(bitmap);

                        if (result != null)
                        {
                            item.baiduShareInfo.shareLink = result.Text.Trim();
                        }

                        break;
                    }
                    case FileType.Txt:
                        detailFile = new FileInfo(item.Path);

                        
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (detailFile == null) continue;

                //读取文件
                using var sr = new StreamReader(detailFile.FullName);

                var originalContent = await sr.ReadToEndAsync();

                item.DetailFileContent = originalContent;

                //获取下载链接
                var contentLine = originalContent.Split('\n');
                for (var i = 0; i < contentLine.Count(); i++)
                {
                    var line = contentLine[i].Trim();
                    if (line.Contains("解壓密碼："))
                    {
                        var passwdResult = Regex.Match(line, @"解壓密碼：(.*)");
                        if (passwdResult.Success)
                        {
                            item.CompressedPassword = passwdResult.Groups[1].Value;
                        }
                    }
                    else if (line.Contains("magnet"))
                    {
                        var link = line;
                        if (item.Links.TryGetValue("magnet", out var itemLink))
                        {
                            itemLink.Add(link);
                        }
                        else
                        {
                            item.Links.Add("magnet", new List<string> { link });
                        }
                    }
                    else if (line.Contains("ed2k://"))
                    {
                        var link = line;
                        if (item.Links.TryGetValue("ed2k", out var itemLink))
                        {
                            itemLink.Add(link);
                        }
                        else
                        {
                            item.Links.Add("ed2k", new List<string> { link });
                        }
                    }
                    else if (line.Contains('|') && Regex.Match(line, @"\w.*\|\d.*?\|\w{40}\|\w{40}").Success)
                    {
                        var link = line;
                        if (item.Links.TryGetValue("115转存链接", out var itemLink))
                        {
                            itemLink.Add(link);
                        }
                        else
                        {
                            item.Links.Add("115转存链接", new List<string> { link });
                        }
                    }
                    else if (line.Contains("http"))
                    {
                        if (line.Contains("1fichier"))
                        {
                            var linkResult = Regex.Match(line, @"[:： ]+(https?:.*)");
                            if (linkResult.Success)
                            {
                                string link = linkResult.Groups[1].Value;
                                if (item.Links.TryGetValue("1fichier", out var itemLink))
                                {
                                    itemLink.Add(link);
                                }
                                else
                                {
                                    item.Links.Add("1fichier", new List<string> { link });
                                }
                            }
                        }
                        else if (line.Contains("pan.baidu.com"))
                        {
                            var panLink = Regex.Match(line, @"链接：(http://pan.baidu.com/s/\w+) 密码：(\w+)");
                            if (panLink.Success)
                            {
                                var link = panLink.Groups[1].Value;
                                var password = panLink.Groups[2].Value;

                                item.baiduShareInfo = new baiduShareInfo
                                {
                                    shareLink = link,
                                    sharePassword =password
                                };
                            }
                        }
                        else
                        {
                            var linkResult = Regex.Match(line, @"^http.*");
                            if (linkResult.Success)
                            {
                                var link = linkResult.Value;
                                if (item.Links.TryGetValue("直链", out var itemLink))
                                {
                                    itemLink.Add(link);
                                }
                                else
                                {
                                    item.Links.Add("直链", new List<string> { link });
                                }
                            }
                        }
                    }
                }
            }

            //隐藏进度条
            RarProgressBar.Visibility = Visibility.Collapsed;

            GetLinkFromRarInfo();

            shareBaiduLinkCount_TextBlock.Text = shareBaiduLinkList.Count().ToString();
            down115LinkCount_TextBlock.Text = down115LinkList.Count().ToString();
            share115LinkCount_TextBlock.Text = share115LinkList.Count().ToString();
            fichier_TextBlock.Text = fichierList.Count().ToString();

            FileListView.SelectedIndex = 0;


        }

        private void GetLinkFromRarInfo()
        {
            shareBaiduLinkList.Clear();
            down115LinkList.Clear();
            share115LinkList.Clear();
            fichierList.Clear();

            foreach (var item in FileNameList)
            {


                //百度网盘分享链接
                if (!string.IsNullOrEmpty(item.baiduShareInfo?.shareLink))
                {
                    shareBaiduLinkList.Add(item.baiduShareInfo.shareLinkWithPwd);
                }

                var currentDown115Links = new List<string>();

                // 115下载只需要用一种方式 
                var down115Method = string.Empty;

                //其他链接
                foreach (var linkDict in item.Links)
                {
                    switch (linkDict.Key)
                    {
                        case ("ed2k" or "magnet" or "直链"):
                            if (down115Method == string.Empty || down115Method == linkDict.Key)
                            {
                                currentDown115Links.AddRange(linkDict.Value);

                                down115Method = linkDict.Key;
                            }

                            break;
                        case "1fichier":
                            fichierList.Add(string.Join('\n', linkDict.Value));
                            break;
                        case "115转存链接":
                            share115LinkList.Add(string.Join('\n', linkDict.Value));
                            break;
                    }
                }

                // 如果存在非rar格式的链接，则不要rar
                var noRarList = currentDown115Links.Where(x => !x.Contains(".rar")).ToList();
                if (noRarList.Count > 0)
                {
                    currentDown115Links = noRarList;
                }

                down115LinkList.Add(string.Join("\n",currentDown115Links));
            }

        }

        private void FileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var fileInfo = (sender as ListView)?.SelectedItem as fileInfo;

            FormatContentGrid.RowDefinitions.Clear();
            FormatContentGrid.Children.Clear();

            //百度分享二维码图片
            if (fileInfo?.QRcodeImagePath != null)
            {
                QRcodeImage.Source = new BitmapImage(new Uri(fileInfo.QRcodeImagePath));
                QRcodeStackPanel.Visibility = Visibility.Visible;
            }
            else
            {
                QRcodeStackPanel.Visibility = Visibility.Collapsed;
            }

            // 解压密码
            if (!string.IsNullOrEmpty(fileInfo?.CompressedPassword))
            {
                FormatContentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                var passwordValueTextBlock = new TextBlock() { Text = fileInfo.CompressedPassword, TextWrapping = TextWrapping.Wrap };
                passwordValueTextBlock.Tapped += TextBlock_Tapped;

                passwordValueTextBlock.PointerEntered += TextBlock_PointerEntered;
                passwordValueTextBlock.PointerExited += TextBlock_PointerExited;

                passwordValueTextBlock.SetValue(Grid.ColumnProperty, 1);

                FormatContentGrid.Children.Add(passwordValueTextBlock);
            }

            var passwordTitleTextBlock = new Controls.TitleTextBlock("解压密码");
            FormatContentGrid.Children.Add(passwordTitleTextBlock);

            //百度网盘分享
            if (fileInfo?.baiduShareInfo != null)
            {
                if (fileInfo.baiduShareInfo.sharePassword != null)
                {
                    baiduSharePassword_TextBlock.Text = fileInfo.baiduShareInfo.sharePassword;
                }

                if (fileInfo.baiduShareInfo.shareLink != null)
                {
                    FormatContentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    //var baiduShareLinkTitle_TextBlock = new TextBlock() { Text = "百度分享链接"};

                    Controls.TitleTextBlock baiduShareLinkTitle_TextBlock = new Controls.TitleTextBlock("百度分享链接");
                    baiduShareLinkTitle_TextBlock.SetValue(Grid.RowProperty, 1);
                    FormatContentGrid.Children.Add(baiduShareLinkTitle_TextBlock);

                    var baiduShareLinkValue_TextBlock = new TextBlock() { Text = fileInfo.baiduShareInfo.shareLinkWithPwd, TextWrapping = TextWrapping.Wrap };
                    baiduShareLinkValue_TextBlock.Tapped += TextBlock_Tapped;

                    baiduShareLinkValue_TextBlock.PointerEntered += TextBlock_PointerEntered;
                    baiduShareLinkValue_TextBlock.PointerExited += TextBlock_PointerExited;

                    baiduShareLinkValue_TextBlock.SetValue(Grid.RowProperty, 1);
                    baiduShareLinkValue_TextBlock.SetValue(Grid.ColumnProperty, 1);
                    FormatContentGrid.Children.Add(baiduShareLinkValue_TextBlock);
                }



            }

            //原文信息
            OriginalContent_Text.Text = fileInfo?.DetailFileContent;

            var gridRowIndex = FormatContentGrid.RowDefinitions.Count;

            // 链接信息
            for (var i = 0;i< fileInfo?.Links.Count; i++)
            {
                //第一行为解压密码
                var rowIndex = i + gridRowIndex;

                var item = fileInfo.Links.ToArray()[i];
                FormatContentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                var titleTextBlock = new Controls.TitleTextBlock(item.Key);
                titleTextBlock.SetValue(Grid.RowProperty, rowIndex);
                FormatContentGrid.Children.Add(titleTextBlock);

                var valueTextBlock = new TextBlock() { Text = string.Join('\n', item.Value), TextWrapping = TextWrapping.Wrap };
                valueTextBlock.Tapped += TextBlock_Tapped;
                valueTextBlock.PointerEntered += TextBlock_PointerEntered;
                valueTextBlock.PointerExited += TextBlock_PointerExited;

                valueTextBlock.SetValue(Grid.RowProperty, rowIndex);
                valueTextBlock.SetValue(Grid.ColumnProperty, 1);
                FormatContentGrid.Children.Add(valueTextBlock);
            }
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var text = (sender as TextBlock).Text;
            tryAddtoClipboard(text);
        }

        private void TextBlock_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        }

        private void TextBlock_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }

        private void CopyBaiduLink_Click(object sender, RoutedEventArgs e)
        {
            if (shareBaiduLinkList.Count > 0)
            {
                tryAddtoClipboard(string.Join('\n', shareBaiduLinkList));
            }
        }

        private void Copy115DownLink_Click(object sender, RoutedEventArgs e)
        {
            if (down115LinkList.Count > 0)
            {
                tryAddtoClipboard(string.Join('\n', down115LinkList));
            }
        }

        private void Copy115ShareLink_Click(object sender, RoutedEventArgs e)
        {
            if (share115LinkList.Count > 0)
            {
                tryAddtoClipboard(string.Join('\n', share115LinkList));
            }
        }

        private void Copy1Fichier_Click(object sender, RoutedEventArgs e)
        {
            if (fichierList.Count > 0)
            {
                tryAddtoClipboard(string.Join('\n', fichierList));
            }
        }
    }
    public class fileInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string QRcodeImagePath { get; set; }
        public string DetailFileContent { get; set; }
        public baiduShareInfo baiduShareInfo { get; set; }
        
        public Dictionary<string, List<string>> Links { get; set; } = new();

        public string CompressedPassword {get;set;}

        public FileType FileType { get; set; }
    }

    public enum FileType{Rar,Txt}

    public class baiduShareInfo
    {
        public string shareLink { get; set; }
        public string sharePassword { get; set; }
        public string shareLinkWithPwd
        {
            get
            {
                return $"{shareLink}?pwd={sharePassword}";
            }
        }

    }
}
