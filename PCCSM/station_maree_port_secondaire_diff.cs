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
    
    public partial class station_maree_port_secondaire_diff
    {
        public int station_maree_port_secondaire_diff_id { get; set; }
        public Nullable<int> PortSecondaireID { get; set; }
        public Nullable<int> annee { get; set; }
        public Nullable<int> diff_MH_Min { get; set; }
        public Nullable<decimal> diff_MH_maree_moy_metre { get; set; }
        public Nullable<decimal> diff_MH_grande_maree_metre { get; set; }
        public Nullable<decimal> diff_MB_Min { get; set; }
        public Nullable<decimal> diff_MB_maree_moy_metre { get; set; }
        public Nullable<decimal> diff_MB_grande_maree_metre { get; set; }
        public Nullable<decimal> marnage_maree_moyenne { get; set; }
        public Nullable<decimal> marnage_grande_maree { get; set; }
    }
}
