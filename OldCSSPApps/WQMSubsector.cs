//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OldCSSPApps
{
    using System;
    using System.Collections.Generic;
    
    public partial class WQMSubsector
    {
        public WQMSubsector()
        {
            this.WQMRuns = new HashSet<WQMRun>();
            this.WQMStations = new HashSet<WQMStation>();
            this.WQMSubsecDefPrecipStations = new HashSet<WQMSubsecDefPrecipStation>();
        }
    
        public int SubsectorID { get; set; }
        public int CSSPItemID { get; set; }
        public string SubsectorHistoricKey { get; set; }
        public string SubsectorCode { get; set; }
        public string DfoCode { get; set; }
        public string SubsectorName { get; set; }
        public string SubsectorDesc { get; set; }
        public string Map { get; set; }
        public Nullable<System.DateTime> Updated { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<int> ModifiedByID { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        public virtual CSSPItem CSSPItem { get; set; }
        public virtual ICollection<WQMRun> WQMRuns { get; set; }
        public virtual ICollection<WQMStation> WQMStations { get; set; }
        public virtual ICollection<WQMSubsecDefPrecipStation> WQMSubsecDefPrecipStations { get; set; }
    }
}
