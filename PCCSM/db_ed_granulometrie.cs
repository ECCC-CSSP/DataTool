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
    
    public partial class db_ed_granulometrie
    {
        public int id_db_ed_granulometrie { get; set; }
        public int id_db_ed_donnees_gisement { get; set; }
        public Nullable<double> pourc_galet { get; set; }
        public Nullable<double> pourc_gravier { get; set; }
        public Nullable<double> pourc_sable { get; set; }
        public Nullable<double> pourc_argile { get; set; }
    
        public virtual db_ed_donnees_gisement db_ed_donnees_gisement { get; set; }
    }
}
