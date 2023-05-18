using Helper_1080.Helper;
using Helper_1080.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Helper_1080.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {
        ForumApi forumApi = new();
        private CardInfoModel dateInfoCard = new();
        int allCount = 50;

        public Home()
        {
            this.InitializeComponent();


            dateInfoCard.content = "xx";
            dateInfoCard.lightContent = "剩余下载额度";
            dateInfoCard.rightContent = allCount.ToString();
        }


        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

            var downList = await Task.Run(() => forumApi.getDownLogToday());

            var downCountToday = downList.Count;
            dateInfoCard.content = (allCount - downCountToday).ToString();
            dateInfoCard.backRatio = (double)(allCount - downCountToday) / allCount;

            downInfoCard.updateData();
        }
    }
}
