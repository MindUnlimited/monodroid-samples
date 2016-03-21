using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

namespace Todo
{
    public class UserGroupMembership // only used for pointing to default group from user, connecting user id with def group id
    {
        public string ID { get; set; } // user id

        //public string MemberID { get; set; } 

        //public string MembershipID { get; set; } // group id

        [JsonIgnore]
        private string _MembershipID;

        public string MembershipID
        {
            get
            {
                if (string.IsNullOrEmpty(_MembershipID))
                {
                    return _MembershipID;
                }
                return _MembershipID.ToUpper();
            }
            set
            {
                _MembershipID = value.ToUpper();
            }
        }

    }
}
