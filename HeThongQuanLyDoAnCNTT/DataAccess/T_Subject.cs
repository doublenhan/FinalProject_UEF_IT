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
    
    public partial class T_Subject
    {
        public int ID { get; set; }
        public string ID_Subject { get; set; }
        public string T_SubjectName { get; set; }
        public string Contents { get; set; }
        public string Description { get; set; }
        public Nullable<int> ID_Teacher { get; set; }
        public Nullable<bool> isActive { get; set; }
        public Nullable<bool> isDelete { get; set; }
    
        public virtual Teacher Teacher { get; set; }
    }
}
