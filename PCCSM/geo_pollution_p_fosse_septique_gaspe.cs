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
    
    public partial class geo_pollution_p_fosse_septique_gaspe
    {
        public int geo_pollution_p_fosse_septique_gaspe_id { get; set; }
        public Nullable<double> Id { get; set; }
        public string Acces { get; set; }
        public Nullable<double> M_boue { get; set; }
        public Nullable<double> M_liquide { get; set; }
        public string Type { get; set; }
        public string No_civ { get; set; }
        public string Rue { get; set; }
        public string Secteur { get; set; }
        public Nullable<double> M_annee { get; set; }
        public Nullable<double> Matricule { get; set; }
        public string Nom_permis { get; set; }
        public Nullable<double> x { get; set; }
        public Nullable<double> y { get; set; }
        public System.Data.Entity.Spatial.DbGeography geographie { get; set; }
    }
}
