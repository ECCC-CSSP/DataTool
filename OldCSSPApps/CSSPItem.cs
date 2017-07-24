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
    
    public partial class CSSPItem
    {
        public CSSPItem()
        {
            this.AppTasks = new HashSet<AppTask>();
            this.BoxModels = new HashSet<BoxModel>();
            this.CSSPInfrastructures = new HashSet<CSSPInfrastructure>();
            this.CSSPItemFiles = new HashSet<CSSPItemFile>();
            this.CSSPItemLanguages = new HashSet<CSSPItemLanguage>();
            this.CSSPItemPaths = new HashSet<CSSPItemPath>();
            this.CSSPItemPaths1 = new HashSet<CSSPItemPath>();
            this.CSSPItemsUserAuthorizations = new HashSet<CSSPItemsUserAuthorization>();
            this.MikeScenarios = new HashSet<MikeScenario>();
            this.VPScenarios = new HashSet<VPScenario>();
            this.WQMSubsectors = new HashSet<WQMSubsector>();
        }
    
        public int CSSPItemID { get; set; }
        public int CSSPItemTypeID { get; set; }
        public System.DateTime LastModifiedDate { get; set; }
        public int ModifiedByID { get; set; }
        public bool IsActive { get; set; }
    
        public virtual ICollection<AppTask> AppTasks { get; set; }
        public virtual ICollection<BoxModel> BoxModels { get; set; }
        public virtual ICollection<CSSPInfrastructure> CSSPInfrastructures { get; set; }
        public virtual ICollection<CSSPItemFile> CSSPItemFiles { get; set; }
        public virtual ICollection<CSSPItemLanguage> CSSPItemLanguages { get; set; }
        public virtual ICollection<CSSPItemPath> CSSPItemPaths { get; set; }
        public virtual ICollection<CSSPItemPath> CSSPItemPaths1 { get; set; }
        public virtual CSSPItemType CSSPItemType { get; set; }
        public virtual ICollection<CSSPItemsUserAuthorization> CSSPItemsUserAuthorizations { get; set; }
        public virtual ICollection<MikeScenario> MikeScenarios { get; set; }
        public virtual ICollection<VPScenario> VPScenarios { get; set; }
        public virtual ICollection<WQMSubsector> WQMSubsectors { get; set; }
    }
}
