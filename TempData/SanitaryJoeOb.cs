//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TempData
{
    using System;
    using System.Collections.Generic;
    
    public partial class SanitaryJoeOb
    {
        public int SanitaryJoeObsID { get; set; }
        public int SanitaryJoeSiteID { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<int> siteid { get; set; }
        public Nullable<System.DateTime> ObsDate { get; set; }
        public string Name_Inspector { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Risk_Assessment { get; set; }
        public string Observations { get; set; }
    
        public virtual SanitaryJoeSite SanitaryJoeSite { get; set; }
    }
}
