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
    using System.Data.Entity.Spatial;

    public partial class BV_N1M_S
    {
        public decimal geodb_oid { get; set; }
        public decimal OBJECTID_1 { get; set; }
        public Nullable<decimal> NO_SEQ_COU { get; set; }
        public string NO_COURS_D { get; set; }
        public string NOM_COURS_ { get; set; }
        public Nullable<int> NIVEAU_BAS { get; set; }
        public string NO_REG_HYD { get; set; }
        public string ECHELLE { get; set; }
        public string DAT_MAJ { get; set; }
        public string ENTITE { get; set; }
        public Nullable<decimal> SUP_KM2 { get; set; }
        public Nullable<decimal> Shape_Leng { get; set; }
        public Nullable<decimal> Shape_Area { get; set; }
        public DbGeography geographie { get; set; }
    }
}
