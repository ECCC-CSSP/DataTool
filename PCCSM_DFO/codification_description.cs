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
    
    public partial class codification_description
    {
        public int codification_description_id { get; set; }
        public Nullable<int> codification_id { get; set; }
        public string valeur { get; set; }
        public Nullable<int> langue { get; set; }
        public Nullable<int> etat { get; set; }
    
        public virtual codification codification { get; set; }
        public virtual codification codification1 { get; set; }
        public virtual codification codification2 { get; set; }
    }
}
