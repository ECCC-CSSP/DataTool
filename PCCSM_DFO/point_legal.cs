//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PCCSM_DFO
{
    using System;
    using System.Collections.Generic;
    
    public partial class point_legal
    {
        public point_legal()
        {
            this.secteur_point_legal = new HashSet<secteur_point_legal>();
        }
    
        public int point_legal_id { get; set; }
        public Nullable<decimal> x { get; set; }
        public Nullable<decimal> y { get; set; }
        public Nullable<int> etat { get; set; }
        public string commentaire { get; set; }
    
        public virtual ICollection<secteur_point_legal> secteur_point_legal { get; set; }
    }
}
