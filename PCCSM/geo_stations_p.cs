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

    public partial class geo_stations_p
    {
        public geo_stations_p()
        {
            this.db_echantillonnage_planification = new HashSet<db_echantillonnage_planification>();
            this.db_mesure = new HashSet<db_mesure>();
        }
    
        public int id_geo_station_p { get; set; }
        public Nullable<int> station { get; set; }
        public string zone_e { get; set; }
        public string zone_limitrophe { get; set; }
        public string secteur { get; set; }
        public string secteur_limitrophe { get; set; }
        public string region { get; set; }
        public string type_station { get; set; }
        public Nullable<System.DateTime> date_creation { get; set; }
        public string status { get; set; }
        public Nullable<bool> station_repere { get; set; }
        public string repere_visuel { get; set; }
        public Nullable<decimal> profondeur { get; set; }
        public string commentaire { get; set; }
        public Nullable<decimal> x { get; set; }
        public Nullable<decimal> y { get; set; }
        public DbGeography geographie { get; set; }
        public Nullable<decimal> x_deplacement { get; set; }
        public Nullable<decimal> y_deplacement { get; set; }
        public Nullable<System.DateTime> dernier_calcul { get; set; }
        public Nullable<decimal> mediane_15_tournee { get; set; }
        public Nullable<decimal> mediane_5_tourrnee { get; set; }
        public Nullable<decimal> pourcentage_43_15_tournee { get; set; }
        public Nullable<decimal> pourcentage_43_5_tournee { get; set; }
        public Nullable<decimal> pourcentage_260_15_tournee { get; set; }
        public Nullable<decimal> pourcentage_260_5_tournee { get; set; }
        public Nullable<int> dernier_cf { get; set; }
    
        public virtual ICollection<db_echantillonnage_planification> db_echantillonnage_planification { get; set; }
        public virtual ICollection<db_mesure> db_mesure { get; set; }
    }
}
