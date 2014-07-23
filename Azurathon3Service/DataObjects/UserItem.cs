using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Azurathon3Service.DataObjects
{
    public class UserItem:EntityData
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}