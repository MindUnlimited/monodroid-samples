using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;
using Android.Graphics;

namespace Todo
{
    public class User
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
        public Bitmap Thumbnail { get; set; }

        [Version]
        public string Version { get; set; }
    }
}
