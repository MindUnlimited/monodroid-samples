using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

namespace Todo
{
    public class Group
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

        public override string ToString()
        {
            return Name;
        }
    }
}
