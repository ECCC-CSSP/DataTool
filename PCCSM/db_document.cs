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
    
    public partial class db_document
    {
        public int id_db_document { get; set; }
        public Nullable<int> id_geo_photographie_p { get; set; }
        public Nullable<int> id_geo_pollution_p { get; set; }
        public Nullable<int> id_geo_terrain_p { get; set; }
        public Nullable<int> id_geo_station_p { get; set; }
        public Nullable<int> id_geo_signalisation_p { get; set; }
        public Nullable<int> ID_Infrastructures { get; set; }
        public string secteur { get; set; }
        public string titre { get; set; }
        public string auteur { get; set; }
        public string rating { get; set; }
        public string serveur { get; set; }
        public string repertoire { get; set; }
        public string fichier { get; set; }
        public string commentaire { get; set; }
        public string description { get; set; }
        public string description_a { get; set; }
        public Nullable<System.DateTime> coll_date { get; set; }
        public Nullable<int> annee { get; set; }
        public Nullable<int> azymuth { get; set; }
        public Nullable<bool> diffusable { get; set; }
    
        public virtual geo_terrain_p geo_terrain_p { get; set; }
    }
}