//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PCCSM_DFO
{
    using System;
    using System.Collections.Generic;
    
    public partial class geo_pccsm_acia_station
    {
        public int id_geo_pccsm_acia_station { get; set; }
        public string secteur { get; set; }
        public string nom_secteur { get; set; }
        public string zone_complement { get; set; }
        public Nullable<int> station { get; set; }
        public string type_station { get; set; }
        public Nullable<int> distance_station { get; set; }
        public Nullable<double> circonference_m { get; set; }
        public Nullable<System.DateTime> date_creation { get; set; }
        public Nullable<System.DateTime> date_maj { get; set; }
        public string auteur { get; set; }
        public string auteur_maj { get; set; }
        public string lat27 { get; set; }
        public string long27 { get; set; }
        public Nullable<double> x { get; set; }
        public Nullable<double> y { get; set; }
        public string commentaire { get; set; }
        public bool etat_actif { get; set; }
        public Nullable<int> temp { get; set; }
        public Nullable<bool> isvalidgeo { get; set; }
        public Nullable<int> derniere_annee { get; set; }
    }
}
