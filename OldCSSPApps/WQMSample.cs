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
    
    public partial class WQMSample
    {
        public int SampleID { get; set; }
        public int RunID { get; set; }
        public int StationID { get; set; }
        public Nullable<System.DateTime> SampleDateTime { get; set; }
        public decimal FecCol { get; set; }
        public Nullable<decimal> Salinity { get; set; }
        public Nullable<decimal> WaterTemp { get; set; }
        public System.DateTime Updated { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<int> ModifiedByID { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        public virtual WQMRun WQMRun { get; set; }
        public virtual WQMStation WQMStation { get; set; }
    }
}