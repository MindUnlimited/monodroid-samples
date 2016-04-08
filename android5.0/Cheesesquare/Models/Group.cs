using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

namespace Todo
{
    public class Group : IEquatable<Group>
    {
        //public string ID { get; set; }

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
                _id = value.ToUpper();
            }
        }

        public string Name { get; set; }

        public Boolean isCoach { get; set; }

        [Version]
        public string Version { get; set; }

        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Group);
        }

        public bool Equals(Group other)
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
                object.ReferenceEquals(this.Name, other.Name) ||
                this.Name != null &&
                this.Name.Equals(other.Name)
            ) &&
            this.isCoach.Equals(other.isCoach);
        }
    }
}
