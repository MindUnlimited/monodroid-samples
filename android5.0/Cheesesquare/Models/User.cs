using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;
using Android.Graphics;

namespace Todo
{
    public class User : IComparable<User>, IEquatable<User>
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public bool isCoach { get; set; }

        public string Email { get; set; }

        public string MicrosoftID { get; set; }

        public string GoogleID { get; set; }

        public string FacebookID { get; set; }

        public string LoginType { get; set; }

        public string LoginUserId { get; set; }

        public string LoginPassword { get; set; }

        public string GUILanguage { get; set; }

        public string Culture { get; set; }

        public string TrainingProgramLanguages { get; set; }

        [JsonIgnore]
        public string PhotoId { get; set; }

        [JsonIgnore]
        public Bitmap Thumbnail { get; set; }

        [Version]
        public string Version { get; set; }

        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }

        public int CompareTo(User other)
        {
            if (this.Name != null && other.Name != null)
            {
                if (Name.ToUpper()[0] < other.Name.ToUpper()[0])
                    return -1;
                if (Name.ToUpper()[0] == other.Name.ToUpper()[0])
                    return 0;
                if (Name.ToUpper()[0] > other.Name.ToUpper()[0])
                    return 1;
            }
            return -1;
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(User other)
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
                object.ReferenceEquals(this.Culture, other.Culture) ||
                this.Culture != null &&
                this.Culture.Equals(other.Culture)
            ) &&
            (
                object.ReferenceEquals(this.Email, other.Email) ||
                this.Email != null &&
                this.Email.Equals(other.Email)
            ) &&
            (
                object.ReferenceEquals(this.FacebookID, other.FacebookID) ||
                this.FacebookID != null &&
                this.FacebookID.Equals(other.FacebookID)
            ) &&
            (
                object.ReferenceEquals(this.GoogleID, other.GoogleID) ||
                this.GoogleID != null &&
                this.GoogleID.Equals(other.GoogleID)
            ) &&
            (
                object.ReferenceEquals(this.GUILanguage, other.GUILanguage) ||
                this.GUILanguage != null &&
                this.GUILanguage.Equals(other.GUILanguage)
            ) &&
            (
                object.ReferenceEquals(this.LoginPassword, other.LoginPassword) ||
                this.LoginPassword != null &&
                this.LoginPassword.Equals(other.LoginPassword)
            ) &&
            (
                object.ReferenceEquals(this.LoginType, other.LoginType) ||
                this.LoginType != null &&
                this.LoginType.Equals(other.LoginType)
            ) &&
            (
                object.ReferenceEquals(this.LoginUserId, other.LoginUserId) ||
                this.LoginUserId != null &&
                this.LoginUserId.Equals(other.LoginUserId)
            ) &&
            (
                object.ReferenceEquals(this.MicrosoftID, other.MicrosoftID) ||
                this.MicrosoftID != null &&
                this.MicrosoftID.Equals(other.MicrosoftID)
            ) &&
            (
                object.ReferenceEquals(this.Name, other.Name) ||
                this.Name != null &&
                this.Name.Equals(other.Name)
            ) &&
            (
                object.ReferenceEquals(this.TrainingProgramLanguages, other.TrainingProgramLanguages) ||
                this.TrainingProgramLanguages != null &&
                this.TrainingProgramLanguages.Equals(other.TrainingProgramLanguages)
            ) 
            
            &&

            this.isCoach.Equals(other.isCoach);
        }
    }
}
