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
    
    public partial class modele
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public modele()
        {
            this.mod_contamination_s = new HashSet<mod_contamination_s>();
            this.modele_dilution = new HashSet<modele_dilution>();
        }
    
        public int id_modele { get; set; }
        public Nullable<int> id_cour_zone_etude { get; set; }
        public Nullable<int> concentration { get; set; }
        public string commentaire { get; set; }
        public Nullable<int> auteur { get; set; }
        public string fichier_cormix { get; set; }
        public Nullable<int> debit { get; set; }
        public Nullable<int> ID_Infrastructures { get; set; }
        public Nullable<bool> diffuser { get; set; }
    
        public virtual cour_zone_etude cour_zone_etude { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mod_contamination_s> mod_contamination_s { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<modele_dilution> modele_dilution { get; set; }
    }
}
