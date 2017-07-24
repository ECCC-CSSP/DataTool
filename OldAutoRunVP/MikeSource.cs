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
    
    public partial class MikeSource
    {
        public int MikeSourceID { get; set; }
        public int MikeScenarioID { get; set; }
        public string SourceName { get; set; }
        public string SourceNumberString { get; set; }
        public Nullable<bool> Include { get; set; }
        public Nullable<double> SourceFlow { get; set; }
        public Nullable<bool> IsContinuous { get; set; }
        public Nullable<System.DateTime> StartDateAndTime { get; set; }
        public Nullable<System.DateTime> EndDateAndTime { get; set; }
        public Nullable<double> SourcePollution { get; set; }
        public Nullable<double> SourceTemperature { get; set; }
        public Nullable<double> SourceSalinity { get; set; }
        public Nullable<double> SourceLat { get; set; }
        public Nullable<double> SourceLong { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<int> ModifiedByID { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        public virtual MikeScenario MikeScenario { get; set; }
    }
}
