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
    
    public partial class WQMRun
    {
        public WQMRun()
        {
            this.WQMSamples = new HashSet<WQMSample>();
        }
    
        public int RunID { get; set; }
        public int SubsectorID { get; set; }
        public int RunNumber { get; set; }
        public Nullable<System.DateTime> RunStartDateTime { get; set; }
        public Nullable<System.DateTime> RunEndDateTime { get; set; }
        public Nullable<int> TideStartID { get; set; }
        public Nullable<int> TideEndID { get; set; }
        public Nullable<bool> Autocalc { get; set; }
        public Nullable<int> SelectedPrecipStationID { get; set; }
        public Nullable<decimal> PPT24 { get; set; }
        public Nullable<decimal> PPT48 { get; set; }
        public Nullable<decimal> PPT72 { get; set; }
        public string Note { get; set; }
        public System.DateTime Updated { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<int> ModifiedByID { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        public virtual WQMPrecipStation WQMPrecipStation { get; set; }
        public virtual WQMSubsector WQMSubsector { get; set; }
        public virtual WQMTide WQMTide { get; set; }
        public virtual WQMTide WQMTide1 { get; set; }
        public virtual ICollection<WQMSample> WQMSamples { get; set; }
    }
}
