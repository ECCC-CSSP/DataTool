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
    
    public partial class db_feedback
    {
        public int id_db_feedback { get; set; }
        public string commentaire { get; set; }
        public string auteur { get; set; }
        public Nullable<System.DateTime> date_feedback { get; set; }
        public Nullable<double> x { get; set; }
        public Nullable<double> y { get; set; }
    }
}