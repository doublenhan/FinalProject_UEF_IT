//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HeThongQuanLyDoAnCNTT.DataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class TrackingPaper
    {
        public int ID { get; set; }
        public string Contents { get; set; }
        public string Description { get; set; }
        public string TrackingTime { get; set; }
        public Nullable<int> ID_Student { get; set; }
        public Nullable<int> ID_Subject { get; set; }
        public string FileUpload { get; set; }
    
        public virtual Student Student { get; set; }
        public virtual Subject Subject { get; set; }
    }
}