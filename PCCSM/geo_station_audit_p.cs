//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PCCSM
{
    using System;
    using System.Collections.Generic;
    
    public partial class geo_station_audit_p
    {
        public int id_geo_station_audit_p { get; set; }
        public Nullable<int> id_geo_station_p { get; set; }
        public string waypoint { get; set; }
        public Nullable<decimal> distance_PRISM { get; set; }
        public Nullable<decimal> distance_GPS_EC { get; set; }
        public Nullable<decimal> distance_GPS_Consultant { get; set; }
        public Nullable<decimal> distance_calculee { get; set; }
        public Nullable<System.DateTime> date_audit { get; set; }
        public string observateur { get; set; }
        public string GPS_Audit { get; set; }
        public string GPS_Consultant { get; set; }
        public string commentaire { get; set; }
        public Nullable<decimal> x { get; set; }
        public Nullable<decimal> y { get; set; }
        public System.Data.Entity.Spatial.DbGeography geographie { get; set; }
    }
}
