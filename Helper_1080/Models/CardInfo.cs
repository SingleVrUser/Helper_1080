using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Helper_1080.Models
{
    public class CardInfoModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        private string _content = "主内容";


        public string content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }
        public string lightContent { get; set; } = "普通内容";
        public string rightContent { get; set; } = null;
        public string cover { get; set; } = "/Assets/SVG/arrow_down_download_save_icon.svg";
        public double backRatio { get; set; } = 1;


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
