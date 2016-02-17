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
using Microsoft.WindowsAzure.MobileServices;

namespace Cheesesquare.Models
{
    public class Device
    {
        public string id { get; set; }

        public string OwnerId { get; set; }

        public string MachineId { get; set; }

        public int OS { get; set; }

        public DateTime LastSuccesfulSync {get; set; }

        [Version]
        public string Version { get; set; }
    }
}