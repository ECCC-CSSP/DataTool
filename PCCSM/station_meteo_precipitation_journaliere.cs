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
    
    public partial class station_meteo_precipitation_journaliere
    {
        public int station_meteo_precipitation_journaliere1 { get; set; }
        public Nullable<int> stationID { get; set; }
        public Nullable<System.DateTime> dateprecipitation { get; set; }
        public Nullable<decimal> precipitation24hmm { get; set; }
        public string precipitation24hmm_indicateur { get; set; }
        public string station_de_remplacement_ID { get; set; }
        public Nullable<decimal> precipitation24hmm_octobre { get; set; }
        public string precipitation24hmm_indicateur_octobre { get; set; }
        public Nullable<decimal> precipitation24hmm_novembre { get; set; }
        public string precipitation24hmm_indicateur_novembre { get; set; }
        public string commentaire { get; set; }
    }
}
