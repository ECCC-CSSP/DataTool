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
    
    public partial class contact_groupe
    {
        public int contact_groupe_id { get; set; }
        public Nullable<int> contact_id { get; set; }
        public Nullable<int> groupe_id { get; set; }
    
        public virtual contact contact { get; set; }
    }
}