//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OldAutoRunVP
{
    using System;
    using System.Collections.Generic;
    
    public partial class Observation
    {
        public int ObservationID { get; set; }
        public Nullable<int> id { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<int> siteid { get; set; }
        public Nullable<System.DateTime> Date_Site { get; set; }
        public string Name_Inspector { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Risk_Assessment { get; set; }
        public string Observations { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<int> ModifiedByID { get; set; }
        public Nullable<bool> IsActive { get; set; }
    }
}
