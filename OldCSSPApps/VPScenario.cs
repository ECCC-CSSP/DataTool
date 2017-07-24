//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OldCSSPApps
{
    using System;
    using System.Collections.Generic;
    
    public partial class VPScenario
    {
        public VPScenario()
        {
            this.VPAmbients = new HashSet<VPAmbient>();
            this.VPSelectedCormixResults = new HashSet<VPSelectedCormixResult>();
            this.VPSelectedResults = new HashSet<VPSelectedResult>();
            this.VPValuedCormixResults = new HashSet<VPValuedCormixResult>();
            this.VPValuedResults = new HashSet<VPValuedResult>();
        }
    
        public int VPScenarioID { get; set; }
        public int CSSPItemID { get; set; }
        public string VPScenarioName { get; set; }
        public System.DateTime VPScenarioDate { get; set; }
        public Nullable<bool> UseAsBestEstimate { get; set; }
        public double EffluentFlow { get; set; }
        public double EffluentConcentration { get; set; }
        public double FroudeNumber { get; set; }
        public double PortDiameter { get; set; }
        public double PortDepth { get; set; }
        public double PortElevation { get; set; }
        public double VerticalAngle { get; set; }
        public double HorizontalAngle { get; set; }
        public double NumberOfPorts { get; set; }
        public double PortSpacing { get; set; }
        public double AcuteMixZone { get; set; }
        public double ChronicMixZone { get; set; }
        public double EffluentSalinity { get; set; }
        public double EffluentTemperature { get; set; }
        public double EffluentVelocity { get; set; }
        public string RawResults { get; set; }
        public string ParsedResults { get; set; }
        public string CormixSummaryResults { get; set; }
        public string CormixDetailResults { get; set; }
        public string CormixParsedResults { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<int> ModifiedByID { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        public virtual CSSPItem CSSPItem { get; set; }
        public virtual ICollection<VPAmbient> VPAmbients { get; set; }
        public virtual ICollection<VPSelectedCormixResult> VPSelectedCormixResults { get; set; }
        public virtual ICollection<VPSelectedResult> VPSelectedResults { get; set; }
        public virtual ICollection<VPValuedCormixResult> VPValuedCormixResults { get; set; }
        public virtual ICollection<VPValuedResult> VPValuedResults { get; set; }
    }
}
