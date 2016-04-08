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
    public class Device : IEquatable<Device>
    {
        public string id { get; set; }

        public string OwnerId { get; set; }

        public string MachineId { get; set; }

        public int OS { get; set; }

        public DateTime LastSuccesfulSync {get; set; }

        [Version]
        public string Version { get; set; }

        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Device);
        }

        public bool Equals(Device other)
        {
            if (other == null)
                return false;

            return
            (
                object.ReferenceEquals(this.id, other.id) ||
                this.id != null &&
                this.id.Equals(other.id)
            ) &&
            (
                object.ReferenceEquals(this.OwnerId, other.OwnerId) ||
                this.OwnerId != null &&
                this.OwnerId.Equals(other.OwnerId)
            ) &&
            (
                object.ReferenceEquals(this.MachineId, other.MachineId) ||
                this.MachineId != null &&
                this.MachineId.Equals(other.MachineId)
            ) &&
            (
                object.ReferenceEquals(this.LastSuccesfulSync, other.LastSuccesfulSync) ||
                this.LastSuccesfulSync != null &&
                this.LastSuccesfulSync.Equals(other.LastSuccesfulSync)
            ) &&
            this.OS.Equals(other.OS);
        }
    }
}