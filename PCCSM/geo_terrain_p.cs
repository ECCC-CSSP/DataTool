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

    public partial class geo_terrain_p
    {
        public geo_terrain_p()
        {
            this.db_document = new HashSet<db_document>();
        }
    
        public int id_geo_terrain_p { get; set; }
        public string region { get; set; }
        public string nom { get; set; }
        public string code { get; set; }
        public Nullable<System.DateTime> date_terrain { get; set; }
        public string auteur { get; set; }
        public string commentaire { get; set; }
        public Nullable<decimal> x { get; set; }
        public Nullable<decimal> y { get; set; }
        public Nullable<bool> etat_actif { get; set; }
        public string url { get; set; }
        public DbGeography geographie { get; set; }
        public Nullable<decimal> x_deplacement { get; set; }
        public Nullable<decimal> y_deplacement { get; set; }
    
        public virtual ICollection<db_document> db_document { get; set; }
    }
}
