using Helper_1080.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Helper_1080.Controls
{
    public sealed partial class InfoCard : UserControl
    {
        public CardInfoModel userInfoModel { get; set; }

        public InfoCard()
        {
            this.InitializeComponent();

            if (userInfoModel == null)
            {
                userInfoModel = new CardInfoModel();
                //userInfoModel.backRatio = 0.5;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            updateData();
        }

        public void updateData() 
        {
            double stackpanel_width = (double)cardStackPanel.GetValue(WidthProperty);
            double stackpanel_height = (double)cardStackPanel.GetValue(HeightProperty);

            //设置
            cardPolygon.Points = new PointCollection()
            {
                new Point(0, 0),
                new Point(0, stackpanel_height),
                //总长*比率（剩/总）
                new Point(stackpanel_width+50, stackpanel_height),
                //总长*比率（剩/总）
                new Point(stackpanel_width, 0),
            };

            //移动距离
            double moveSize = (stackpanel_width + 50) * (1 - userInfoModel.backRatio);

            //动画
            Storyboard1.Children[0].SetValue(DoubleAnimation.FromProperty, 0);
            Storyboard1.Children[0].SetValue(DoubleAnimation.ToProperty, -moveSize);
            Storyboard1.Begin();
        }
    }
}
