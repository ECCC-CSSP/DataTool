//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OldAutoRunVP
{
    using System;
    using System.Collections.Generic;
    
    public partial class DBLog
    {
        public int DBLogID { get; set; }
        public string DBLogTable { get; set; }
        public string DBLogActivity { get; set; }
        public string DBLogMoreInfo { get; set; }
        public System.DateTime DBLogDateTime { get; set; }
        public int EnteredByID { get; set; }
        public string IP { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<int> ModifiedByID { get; set; }
        public Nullable<bool> IsActive { get; set; }
    }
}