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
    
    public partial class modele_dilution
    {
        public int id_modele_dilution { get; set; }
        public Nullable<int> id_modele { get; set; }
        public string dilution { get; set; }
        public Nullable<int> maree { get; set; }
        public Nullable<double> distance_calculee { get; set; }
        public Nullable<double> distance_minimale { get; set; }
        public Nullable<bool> diffuser { get; set; }
        public Nullable<int> ID_Infrastructure { get; set; }
    
        public virtual modele modele { get; set; }
    }
}
