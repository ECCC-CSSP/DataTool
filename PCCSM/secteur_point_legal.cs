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
    
    public partial class secteur_point_legal
    {
        public int secteur_point_legal_id { get; set; }
        public Nullable<int> secteur_id { get; set; }
        public Nullable<int> point_legal_id { get; set; }
        public Nullable<int> secteur_point_legal_no { get; set; }
    
        public virtual point_legal point_legal { get; set; }
        public virtual secteur secteur { get; set; }
    }
}