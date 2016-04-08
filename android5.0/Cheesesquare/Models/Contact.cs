using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Todo.Models
{
    public class Contact : IEquatable<Contact>
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string PictureUrl { get; set; }

        [Version]
        public string Version { get; set; }

        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Contact);
        }

        public bool Equals(Contact other)
        {
            if (other == null)
                return false;

            return 
            (
                object.ReferenceEquals(this.Id, other.Id) ||
                this.Id != null &&
                this.Id.Equals(other.Id)
            ) &&
            (
                object.ReferenceEquals(this.Name, other.Name) ||
                this.Name != null &&
                this.Name.Equals(other.Name)
            ) &&
            (
                object.ReferenceEquals(this.PictureUrl, other.PictureUrl) ||
                this.PictureUrl != null &&
                this.PictureUrl.Equals(other.PictureUrl)
            );
        }

    }
}