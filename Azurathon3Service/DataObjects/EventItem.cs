using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Azurathon3Service.DataObjects
{
    public class EventItem:EntityData
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public virtual UserItem UserItem { get; set; }

        public int Location { get; set; }
    }
}