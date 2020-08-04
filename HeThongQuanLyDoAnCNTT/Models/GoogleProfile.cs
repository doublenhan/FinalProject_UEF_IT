using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeThongQuanLyDoAnCNTT.Models
{
    public class GoogleProfile
    {
        public string id { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        //public ImageProfile picture { get; set; }
        public string picture { get; set; }

        //public List<Email> email { get; set; }
        public string gender { get; set; }
        public string ObjectType { get; set; }
    }
    public class Email
    {
        public string Value { get; set; }
        public string Type { get; set; }
    }
    public class ImageProfile
    {
        public string Url { get; set; }
    }
}