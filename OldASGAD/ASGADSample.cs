//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OldASGAD
{
    using System;
    using System.Collections.Generic;
    
    public partial class ASGADSample
    {
        public int SampleId { get; set; }
        public int RunId { get; set; }
        public int StationId { get; set; }
        public decimal FecCol { get; set; }
        public Nullable<decimal> Salinity { get; set; }
        public Nullable<decimal> WaterTemp { get; set; }
        public System.DateTime Updated { get; set; }
        public string SampleTimeOld { get; set; }
        public Nullable<System.DateTime> SampleDateTime { get; set; }
    
        public virtual ASGADRun ASGADRun { get; set; }
        public virtual ASGADStation ASGADStation { get; set; }
    }
}
