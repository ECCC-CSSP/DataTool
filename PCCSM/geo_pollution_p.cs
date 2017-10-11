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
    
    public partial class geo_pollution_p
    {
        public geo_pollution_p()
        {
            this.evaluation_risque = new HashSet<evaluation_risque>();
        }
    
        public int id_geo_pollution_p { get; set; }
        public Nullable<int> groupe_id { get; set; }
        public string code { get; set; }
        public string region { get; set; }
        public string zone_e { get; set; }
        public string secteur { get; set; }
        public string proprietaire { get; set; }
        public string adresse { get; set; }
        public Nullable<decimal> pente { get; set; }
        public Nullable<int> utilisation { get; set; }
        public Nullable<int> reseau_hydro { get; set; }
        public Nullable<double> x { get; set; }
        public Nullable<double> y { get; set; }
        public string description { get; set; }
        public string description_diffusable { get; set; }
        public string observateur { get; set; }
        public Nullable<System.DateTime> date_observation { get; set; }
        public string status { get; set; }
        public Nullable<bool> diffusable { get; set; }
    
        public virtual ICollection<evaluation_risque> evaluation_risque { get; set; }
        public virtual tr_pollution_code tr_pollution_code { get; set; }
    }
}