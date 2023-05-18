﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Helper_1080
{
    internal class AppSettings
    {
        public static ApplicationDataContainer localSettings { get { return ApplicationData.Current.LocalSettings; } }


        public static string Cookie
        {
            get
            {
                return localSettings.Values["Cookie"] as string;
            }
            set
            {
                localSettings.Values["Cookie"] = value;
            }
        }

        public static string BaseUrl
        {
            get
            {
                string baseUrl = localSettings.Values["BaseUrl"] as string;
                if (string.IsNullOrEmpty(baseUrl))
                {
                    baseUrl = "https://www.x222x.me/";
                }
                return baseUrl;
            }
            set
            {
                localSettings.Values["BaseUrl"] = value;
            }
        }
    }
}
