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
    
    public partial class romm_site_observation_p
    {
        public int id_romm_site_observation_p { get; set; }
        public Nullable<decimal> y { get; set; }
        public Nullable<decimal> x { get; set; }
        public System.Data.Entity.Spatial.DbGeography geographie { get; set; }
    }
}