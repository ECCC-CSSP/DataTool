//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataTool
{
    using System;
    using System.Collections.Generic;
    
    public partial class ClimateDataStartDate
    {
        public ClimateDataStartDate()
        {
            this.ClimateDataValues = new HashSet<ClimateDataValue>();
        }
    
        public int ClimateDataStartDateID { get; set; }
        public int ClimateStationID { get; set; }
        public System.DateTime ClimateDataDate { get; set; }
        public Nullable<System.DateTime> FromForcastDate { get; set; }
        public int ClimateDataType { get; set; }
        public bool IsForcastData { get; set; }
        public bool IsObservationData { get; set; }
        public bool IsArchivedData { get; set; }
    
        public virtual ClimateStation ClimateStation { get; set; }
        public virtual ICollection<ClimateDataValue> ClimateDataValues { get; set; }
    }
}