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
    
    public partial class Site
    {
        public int SiteID { get; set; }
        public Nullable<int> siteid2 { get; set; }
        public string Locator { get; set; }
        public Nullable<float> Site1 { get; set; }
        public Nullable<int> Zone { get; set; }
        public Nullable<float> easting { get; set; }
        public Nullable<float> northing { get; set; }
        public string Datum { get; set; }
        public Nullable<float> latitude { get; set; }
        public Nullable<float> longitude { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<int> ModifiedByID { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        public virtual Site Sites1 { get; set; }
        public virtual Site Site2 { get; set; }
    }
}
