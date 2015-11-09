using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

namespace Todo
{
    public class Group
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public Boolean isCoach { get; set; }

        [Version]
        public string Version { get; set; }
    }
}
