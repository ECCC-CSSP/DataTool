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
    
    public partial class releve_sanitaire
    {
        public releve_sanitaire()
        {
            this.releve_sanitaire_secteur = new HashSet<releve_sanitaire_secteur>();
        }
    
        public int releve_sanitaire_id { get; set; }
        public Nullable<int> no { get; set; }
        public string region { get; set; }
        public string equipe { get; set; }
        public Nullable<int> annee { get; set; }
        public Nullable<System.DateTime> date_debut { get; set; }
        public Nullable<System.DateTime> date_fin { get; set; }
        public string commentaire { get; set; }
    
        public virtual ICollection<releve_sanitaire_secteur> releve_sanitaire_secteur { get; set; }
    }
}