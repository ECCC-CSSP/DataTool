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
    
    public partial class BV_N1M_S_caracterisation_milieu
    {
        public long OBJECTID { get; set; }
        public Nullable<double> pente_moyenne { get; set; }
        public Nullable<int> pente_nombre_de_points { get; set; }
        public Nullable<double> drainage_moyen { get; set; }
        public Nullable<int> drainage_nombre_de_points { get; set; }
        public Nullable<double> occ_sol_eleve_nb { get; set; }
        public Nullable<double> occ_sol_faible_nb { get; set; }
        public Nullable<double> occ_sol_modere_nb { get; set; }
        public Nullable<double> occ_sol_eleve_pourc { get; set; }
        public Nullable<double> occ_sol_faible_pourc { get; set; }
        public Nullable<double> occ_sol_modere_pourc { get; set; }
        public Nullable<double> occ_sol_eleve_idm_complet_count { get; set; }
        public Nullable<double> occ_sol_faible_idm_complet_count { get; set; }
        public Nullable<double> occ_sol_modere_idm_complet_count { get; set; }
        public Nullable<double> occ_sol_eleve_idm_complet_pourc { get; set; }
        public Nullable<double> occ_sol_faible_idm_complet_pourc { get; set; }
        public Nullable<double> occ_sol_modere_idm_complet_pourc { get; set; }
        public string pente_resultat { get; set; }
        public string drainage_resultat { get; set; }
        public string occupation_resultat { get; set; }
        public string resultat_final { get; set; }
    }
}