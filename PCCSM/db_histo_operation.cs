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

    public partial class db_histo_operation
    {
        public int id_db_histo_operation { get; set; }
        public Nullable<System.DateTime> date_operation { get; set; }
        public string auteur { get; set; }
        public Nullable<int> type_operation { get; set; }
        public Nullable<int> contact_principal { get; set; }
        public Nullable<int> contact_secondaire { get; set; }
        public Nullable<decimal> x { get; set; }
        public Nullable<decimal> y { get; set; }
        public string description { get; set; }
        public Nullable<int> ordonnanceID { get; set; }
        public Nullable<int> id_geo_pollution_p { get; set; }
        public Nullable<int> id_geo_terrain_p { get; set; }
        public Nullable<int> id_geo_banc_coquillier_s { get; set; }
        public Nullable<int> id_geo_station_p { get; set; }
        public Nullable<int> ID_infrastructures { get; set; }
        public Nullable<int> id_geo_secteur_s { get; set; }
        public Nullable<int> ID_Tournee { get; set; }
        public Nullable<int> id_db_mesure { get; set; }
        public DbGeography geographie { get; set; }
    }
}
