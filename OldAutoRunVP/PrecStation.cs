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
    
    public partial class PrecStation
    {
        public int PrecStationID { get; set; }
        public int PrecProvinceID { get; set; }
        public string PrecStationName { get; set; }
        public string PrecFTPAddress { get; set; }
        public Nullable<double> PrecStationLatitude { get; set; }
        public Nullable<double> PrecStationLongitude { get; set; }
    
        public virtual PrecProvince PrecProvince { get; set; }
    }
}