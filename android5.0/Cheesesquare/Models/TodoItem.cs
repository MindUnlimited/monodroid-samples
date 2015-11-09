using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

namespace Todo
{
    public class TodoItem
    {
        public TodoItem()
        {
        }

        public string Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "complete")]
        public bool Complete { get; set; }

        //[JsonProperty(PropertyName = "notes")]
        //public string Notes { get; set; }

        [Version]
        public string Version { get; set; }

        public override string ToString()
        {
            return "    Title: " + Text + "\n    Complete: " + Complete;
        }
    }
}
