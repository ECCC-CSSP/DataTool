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
    
    public partial class modele_region
    {
        public int Id2 { get; set; }
        public Nullable<long> S { get; set; }
        public Nullable<long> CARTO { get; set; }
        public Nullable<long> CONTAMINAT { get; set; }
        public string DESCRIPTIO { get; set; }
        public Nullable<long> REJET { get; set; }
        public Nullable<long> SOURCE { get; set; }
        public Nullable<long> ID_RESEAU { get; set; }
        public System.Data.Entity.Spatial.DbGeography geographie { get; set; }
    }
}
