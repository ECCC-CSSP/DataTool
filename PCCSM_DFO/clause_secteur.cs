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
    
    public partial class clause_secteur
    {
        public int clause_secteur_id { get; set; }
        public Nullable<int> clause_id { get; set; }
        public Nullable<int> secteur_id { get; set; }
    
        public virtual clause clause { get; set; }
        public virtual secteur secteur { get; set; }
    }
}
