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
    
    public partial class tr_pollution_code
    {
        public tr_pollution_code()
        {
            this.geo_pollution_p = new HashSet<geo_pollution_p>();
        }
    
        public string code { get; set; }
        public string id_pollution_type { get; set; }
        public string categorie { get; set; }
        public Nullable<int> id_pollut_ty_sr { get; set; }
        public string type { get; set; }
        public string source { get; set; }
        public Nullable<bool> diffusable { get; set; }
        public Nullable<bool> afficher_description_diffusable { get; set; }
        public string description_diffusion { get; set; }
        public Nullable<bool> calcul_pente { get; set; }
        public string fichier { get; set; }
    
        public virtual ICollection<geo_pollution_p> geo_pollution_p { get; set; }
    }
}