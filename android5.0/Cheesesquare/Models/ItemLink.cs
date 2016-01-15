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
    public class ItemLink
    {
            // no notify prop changed
            public string Id { get; set; }

            public string ItemID { get; set; }

            public String OwnedBy { get; set; }

            public string Parent { get; set; }

            public int Order { get; set; }
    }
}