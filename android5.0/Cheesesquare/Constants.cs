using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Cheesesquare
{
    public static class Constants
    {
        public const string SenderID = "53371998202"; // Google API Project Number
        public const string ListenConnectionString = "Endpoint=sb://mindunlimited.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=bNeGF939mVBgwcyxqb/b6XdXy6oroNquT5SBHDhl4HA=";
        public const string NotificationHubName = "notificationhub";
    }
}