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
    
    public partial class Inf_Infrastructures
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Inf_Infrastructures()
        {
            this.evaluation_risque = new HashSet<evaluation_risque>();
        }
    
        public int ID_Infrastructures { get; set; }
        public Nullable<int> ParentID { get; set; }
        public Nullable<int> ID_reseau { get; set; }
        public string Nom { get; set; }
        public Nullable<int> TypeEntite { get; set; }
        public string SE_Numero_SOMAE { get; set; }
        public string SE_Region { get; set; }
        public Nullable<System.DateTime> SE_Mise_operation { get; set; }
        public Nullable<int> SE_Traitement { get; set; }
        public string EC_Categorrie_priorite { get; set; }
        public string SUR_Adresse { get; set; }
        public Nullable<decimal> x { get; set; }
        public Nullable<decimal> y { get; set; }
        public Nullable<bool> Status { get; set; }
        public string Description_diffusable { get; set; }
        public string Commentaires { get; set; }
        public string secteur { get; set; }
        public Nullable<decimal> pente { get; set; }
        public Nullable<int> reseau_hydro { get; set; }
        public System.Data.Entity.Spatial.DbGeography geographie { get; set; }
        public Nullable<decimal> x_deplacement { get; set; }
        public Nullable<decimal> y_deplacement { get; set; }
        public string Commentaires_original { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<evaluation_risque> evaluation_risque { get; set; }
        public virtual Inf_TypeEntite Inf_TypeEntite { get; set; }
    }
}
