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
    
    public partial class BCPrecipitation
    {
        public int BCPrecipitationID { get; set; }
        public Nullable<System.DateTime> WR_DATE { get; set; }
        public Nullable<int> WR_SURVEY { get; set; }
        public Nullable<double> WR_PRECIPITATION_RAIN { get; set; }
        public Nullable<double> WR_PRECIPITATION_SNOW { get; set; }
        public Nullable<double> WR_TOTAL_PRECIPITATION { get; set; }
        public string WR_RECORDING_STATION { get; set; }
        public string WR_RECORDING_STATION_CL { get; set; }
        public string ClimateID { get; set; }
    }
}
