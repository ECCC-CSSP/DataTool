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
    
    public partial class mactre2012_inventaire_idm
    {
        public int id { get; set; }
        public string lieu { get; set; }
        public string secteur_coquillier { get; set; }
        public Nullable<System.DateTime> date { get; set; }
        public Nullable<double> latitude { get; set; }
        public Nullable<double> longitude { get; set; }
        public string utilisation_dun_quadrat { get; set; }
        public string mactre { get; set; }
        public string couteau { get; set; }
        public string mye { get; set; }
        public System.Data.Entity.Spatial.DbGeography geographie { get; set; }
    }
}
