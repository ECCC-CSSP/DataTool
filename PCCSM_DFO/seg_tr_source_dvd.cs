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
    
    public partial class seg_tr_source_dvd
    {
        public int id_source_dvd { get; set; }
        public Nullable<int> id_dvd { get; set; }
        public string code_dvd { get; set; }
        public string titre { get; set; }
        public Nullable<int> auteur { get; set; }
        public string co_auteur { get; set; }
        public Nullable<System.DateTime> date_prise { get; set; }
        public string commentaire_fr { get; set; }
        public string commentaire_en { get; set; }
    }
}