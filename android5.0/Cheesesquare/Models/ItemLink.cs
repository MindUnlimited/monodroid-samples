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
using Newtonsoft.Json;

namespace MindSet.Models
{
    public class ItemLink
    {
        // no notify prop changed
        //public string id { get; set; }

        [JsonIgnore]
        private string _id;
        public string id
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                {
                    return _id;
                }
                return _id.ToUpper();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _id = value;
                }
                else
                {
                    _id = value.ToUpper();
                }
            }
        }

        //[JsonIgnore]
        //private string _ItemID;
        //public string ItemID
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(_ItemID))
        //        {
        //            return _ItemID;
        //        }
        //        return _ItemID.ToUpper();
        //    }
        //    set
        //    {
        //        if (string.IsNullOrEmpty(value))
        //        {
        //            _ItemID = value;
        //        }
        //        else
        //        {
        //            _ItemID = value.ToUpper();
        //        }
        //    }
        //}

        public string ItemID { get; set; }


        //public String OwnedBy { get; set; }
        [JsonIgnore]
        private string _OwnedBy;
        public string OwnedBy
        {
            get
            {
                if (string.IsNullOrEmpty(_OwnedBy))
                {
                    return _OwnedBy;
                }
                return _OwnedBy.ToUpper();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _OwnedBy = value;
                }
                else
                {
                    _OwnedBy = value.ToUpper();
                }
            }
        }

        [JsonIgnore]
        private string _Parent;
        public string Parent
        {
            get
            {
                if (string.IsNullOrEmpty(_Parent))
                {
                    return _Parent;
                }
                return _Parent.ToUpper();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _Parent = value;
                }
                else
                {
                    _Parent = value.ToUpper();
                }
            }
        }

        //public string Parent { get; set; }

        public int Order { get; set; }
    }
}