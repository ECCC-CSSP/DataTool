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
    
    public partial class db_ed_echantillonnage
    {
        public db_ed_echantillonnage()
        {
            this.db_ed_donnees_gisement = new HashSet<db_ed_donnees_gisement>();
        }
    
        public int id_db_ed_echantillonnage { get; set; }
        public int id_reference { get; set; }
        public Nullable<int> annee { get; set; }
        public Nullable<System.DateTime> date_echant { get; set; }
        public string type_echantillonnage { get; set; }
    
        public virtual ICollection<db_ed_donnees_gisement> db_ed_donnees_gisement { get; set; }
        public virtual tr_reference tr_reference { get; set; }
    }
}