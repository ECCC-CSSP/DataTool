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
    
    public partial class db_tournee_maree_temporaire_validation
    {
        public int id_db_tournee_maree_temporaire { get; set; }
        public string secteur { get; set; }
        public Nullable<int> ID_Tournee { get; set; }
        public Nullable<int> station { get; set; }
        public Nullable<int> annee { get; set; }
        public Nullable<int> mois { get; set; }
        public Nullable<int> jour { get; set; }
        public Nullable<System.DateTime> date_echantillonnage { get; set; }
        public Nullable<System.DateTime> debut { get; set; }
        public Nullable<System.DateTime> milieu { get; set; }
        public Nullable<System.DateTime> fin { get; set; }
        public Nullable<int> duree_min { get; set; }
        public Nullable<System.DateTime> maree_h_hre { get; set; }
        public Nullable<System.DateTime> maree_b_hre { get; set; }
        public Nullable<double> maree_h_m { get; set; }
        public Nullable<double> maree_b_m { get; set; }
        public Nullable<decimal> marnage { get; set; }
        public Nullable<int> maree_principale { get; set; }
        public Nullable<System.DateTime> maree_h_ps1_hre { get; set; }
        public Nullable<System.DateTime> maree_b_ps1_hre { get; set; }
        public Nullable<decimal> maree_h_ps1_metre { get; set; }
        public Nullable<decimal> maree_b_ps1_metre { get; set; }
        public Nullable<double> maree_h_ps1_min { get; set; }
        public Nullable<double> maree_b_ps1_min { get; set; }
        public Nullable<System.DateTime> maree_h_ps2_hre { get; set; }
        public Nullable<System.DateTime> maree_b_ps2_hre { get; set; }
        public Nullable<decimal> maree_h_ps2_metre { get; set; }
        public Nullable<decimal> maree_b_ps2_metre { get; set; }
        public Nullable<double> maree_h_ps2_min { get; set; }
        public Nullable<double> maree_b_ps2_min { get; set; }
        public Nullable<double> ajustement_ps1_ps2 { get; set; }
        public Nullable<System.DateTime> valid_maree_sec_hre { get; set; }
        public Nullable<decimal> valid_maree_sec_m { get; set; }
    }
}
