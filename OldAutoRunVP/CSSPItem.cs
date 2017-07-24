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
    
    public partial class CSSPItem
    {
        public CSSPItem()
        {
            this.BoxModels = new HashSet<BoxModel>();
            this.CSSPItemFiles = new HashSet<CSSPItemFile>();
            this.CSSPItemLanguages = new HashSet<CSSPItemLanguage>();
            this.CSSPItems1 = new HashSet<CSSPItem>();
            this.Infrastructures = new HashSet<Infrastructure>();
            this.MikeScenarios = new HashSet<MikeScenario>();
            this.Scenarios = new HashSet<Scenario>();
        }
    
        public int CSSPItemID { get; set; }
        public Nullable<int> CSSPParentItemID { get; set; }
        public Nullable<int> CSSPTypeItemID { get; set; }
        public string CSSPItemPath { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<int> ModifiedByID { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        public virtual ICollection<BoxModel> BoxModels { get; set; }
        public virtual ICollection<CSSPItemFile> CSSPItemFiles { get; set; }
        public virtual ICollection<CSSPItemLanguage> CSSPItemLanguages { get; set; }
        public virtual ICollection<CSSPItem> CSSPItems1 { get; set; }
        public virtual CSSPItem CSSPItem1 { get; set; }
        public virtual CSSPTypeItem CSSPTypeItem { get; set; }
        public virtual ICollection<Infrastructure> Infrastructures { get; set; }
        public virtual ICollection<MikeScenario> MikeScenarios { get; set; }
        public virtual ICollection<Scenario> Scenarios { get; set; }
    }
}
