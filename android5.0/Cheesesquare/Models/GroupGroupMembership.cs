using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

namespace Todo
{
    public class GroupGroupMembership : IEquatable<GroupGroupMembership>
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

        [Version]
        public string Version { get; set; }

        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GroupGroupMembership);
        }

        public bool Equals(GroupGroupMembership other)
        {
            if (other == null)
                return false;

            return
            (
                object.ReferenceEquals(this.ID, other.ID) ||
                this.ID != null &&
                this.ID.Equals(other.ID)
            ) &&
            (
                object.ReferenceEquals(this.MemberID, other.MemberID) ||
                this.MemberID != null &&
                this.MemberID.Equals(other.MemberID)
            ) &&
            (
                object.ReferenceEquals(this.MembershipID, other.MembershipID) ||
                this.MembershipID != null &&
                this.MembershipID.Equals(other.MembershipID)
            ) &&
            this.Manages.Equals(other.Manages) && 
            this.Coaches.Equals(other.Coaches) &&
            this.Status.Equals(other.Status);
        }
    }
}
