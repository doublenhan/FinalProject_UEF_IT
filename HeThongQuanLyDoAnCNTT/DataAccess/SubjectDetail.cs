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
    
    public partial class SubjectDetail
    {
        public int ID { get; set; }
        public int ID_Student { get; set; }
        public Nullable<int> PrecentComplete { get; set; }
        public int ID_Subject { get; set; }
    
        public virtual Student Student { get; set; }
        public virtual Subject Subject { get; set; }
    }
}
