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
    
    public partial class station_hydrometrique_debit
    {
        public int station_hydrometrique_debit_id { get; set; }
        public string station { get; set; }
        public Nullable<System.DateTime> debit_date { get; set; }
        public Nullable<decimal> debit { get; set; }
        public string remarque { get; set; }
    
        public virtual station_hydrometrique station_hydrometrique { get; set; }
    }
}
