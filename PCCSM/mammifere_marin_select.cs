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
    
    public partial class mammifere_marin_select
    {
        public int coordonnee_id { get; set; }
        public Nullable<int> romm_id { get; set; }
        public Nullable<decimal> latitude { get; set; }
        public Nullable<decimal> longitude { get; set; }
        public Nullable<int> nb_observation { get; set; }
        public Nullable<int> nb_individus_total { get; set; }
        public string periode_observation { get; set; }
        public System.Data.Entity.Spatial.DbGeography geographie { get; set; }
        public Nullable<int> nb_taxons { get; set; }
    }
}
