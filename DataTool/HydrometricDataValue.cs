//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataTool
{
    using System;
    using System.Collections.Generic;
    
    public partial class HydrometricDataValue
    {
        public int HydrometricDataValueID { get; set; }
        public int HydrometricDataStartDateID { get; set; }
        public System.DateTime HydrometricDataDateTime { get; set; }
        public double Flow_m3ps { get; set; }
    
        public virtual HydrometricDataStartDate HydrometricDataStartDate { get; set; }
    }
}
