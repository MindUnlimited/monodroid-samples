using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

namespace Todo
{
    public class GroupGroupMembership
    {
        public GroupGroupMembership()
        {
        }

        public string ID { get; set; }

        public string MemberID { get; set; }

        public string MembershipID { get; set; }

        public bool Manages { get; set; }

        public bool Coaches { get; set; }

        public int Status { get; set; }
    }
}
