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
    
    public partial class RatingCurve
    {
        public RatingCurve()
        {
            this.RatingCurveValues = new HashSet<RatingCurveValue>();
        }
    
        public int RatingCurveID { get; set; }
        public int HydrometricStationID { get; set; }
        public string RatingCurveNumber { get; set; }
    
        public virtual HydrometricStation HydrometricStation { get; set; }
        public virtual ICollection<RatingCurveValue> RatingCurveValues { get; set; }
    }
}
