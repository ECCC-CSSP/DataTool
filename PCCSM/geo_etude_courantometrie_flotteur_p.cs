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
    
    public partial class geo_etude_courantometrie_flotteur_p
    {
        public int id_geo_etude_courantometrie_flotteur_p { get; set; }
        public int id_cour_terrain_p { get; set; }
        public int id_geo_etude_courantometrie_trajectoire_l { get; set; }
        public Nullable<double> latitude_y { get; set; }
        public Nullable<double> longitude_x { get; set; }
        public string profondeur_m { get; set; }
        public Nullable<double> distance_exu_km { get; set; }
        public Nullable<int> distance_exu_min { get; set; }
        public Nullable<double> courant_vit_cm_s { get; set; }
        public Nullable<int> courant_dir_degree_N { get; set; }
        public Nullable<double> vent_vit_km_s { get; set; }
        public Nullable<int> vent_dir_degree_N { get; set; }
        public Nullable<System.DateTime> dateheureflotteur { get; set; }
        public Nullable<int> seconde { get; set; }
        public Nullable<int> maree_H { get; set; }
        public Nullable<double> maree_MAR { get; set; }
        public Nullable<double> maree_GEO { get; set; }
        public System.Data.Entity.Spatial.DbGeography geographie { get; set; }
    
        public virtual geo_etude_courantometrie_trajectoire_l geo_etude_courantometrie_trajectoire_l { get; set; }
    }
}
