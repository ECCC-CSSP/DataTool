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
    
    public partial class SanitaryJoeSite
    {
        public SanitaryJoeSite()
        {
            this.SanitaryJoeObs = new HashSet<SanitaryJoeOb>();
        }
    
        public int SanitaryJoeSiteID { get; set; }
        public Nullable<int> siteid { get; set; }
        public string Locator { get; set; }
        public Nullable<int> Site { get; set; }
        public Nullable<int> Zone { get; set; }
        public Nullable<double> easting { get; set; }
        public Nullable<double> northing { get; set; }
        public string Datum { get; set; }
        public Nullable<double> latitude { get; set; }
        public Nullable<double> longitude { get; set; }
    
        public virtual ICollection<SanitaryJoeOb> SanitaryJoeObs { get; set; }
    }
}
