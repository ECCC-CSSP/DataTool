//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PCCSM
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Spatial;

    public partial class gps_log
    {
        public long id_gps_log { get; set; }
        public Nullable<double> x { get; set; }
        public Nullable<double> y { get; set; }
        public Nullable<System.DateTime> gps_time { get; set; }
        public Nullable<System.DateTime> local_time { get; set; }
        public Nullable<double> speed { get; set; }
        public string bearing { get; set; }
        public string horzpdop { get; set; }
        public string vertpdop { get; set; }
        public Nullable<int> altitude { get; set; }
        public string usager { get; set; }
        public string commentaire { get; set; }
        public DbGeography geographie { get; set; }
    }
}
