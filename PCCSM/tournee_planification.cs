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
    
    public partial class tournee_planification
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tournee_planification()
        {
            this.db_echantillonnage_planification = new HashSet<db_echantillonnage_planification>();
        }
    
        public int tournee_planification_id { get; set; }
        public Nullable<int> annee { get; set; }
        public Nullable<int> mois { get; set; }
        public string commentaire { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<db_echantillonnage_planification> db_echantillonnage_planification { get; set; }
    }
}
