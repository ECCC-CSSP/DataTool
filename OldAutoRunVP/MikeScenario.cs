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
    
    public partial class MikeScenario
    {
        public MikeScenario()
        {
            this.MikeParameters = new HashSet<MikeParameter>();
            this.MikeScenarioBoundaryConditions = new HashSet<MikeScenarioBoundaryCondition>();
            this.MikeScenarioFiles = new HashSet<MikeScenarioFile>();
            this.MikeSources = new HashSet<MikeSource>();
        }
    
        public int MikeScenarioID { get; set; }
        public int CSSPItemID { get; set; }
        public string ScenarioName { get; set; }
        public string ScenarioSummary { get; set; }
        public string ScenarioStatus { get; set; }
        public Nullable<System.DateTime> ScenarioStartDateAndTime { get; set; }
        public Nullable<System.DateTime> ScenarioEndDateAndTime { get; set; }
        public Nullable<System.DateTime> ScenarioStartExecutionDateAndTime { get; set; }
        public Nullable<double> ScenarioExecutionTimeInMinutes { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<int> ModifiedByID { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        public virtual CSSPItem CSSPItem { get; set; }
        public virtual ICollection<MikeParameter> MikeParameters { get; set; }
        public virtual ICollection<MikeScenarioBoundaryCondition> MikeScenarioBoundaryConditions { get; set; }
        public virtual ICollection<MikeScenarioFile> MikeScenarioFiles { get; set; }
        public virtual ICollection<MikeSource> MikeSources { get; set; }
    }
}
