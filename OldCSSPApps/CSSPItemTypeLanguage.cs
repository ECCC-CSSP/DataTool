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
    
    public partial class CSSPItemTypeLanguage
    {
        public int CSSPItemTypeID { get; set; }
        public string Language { get; set; }
        public string CSSPItemTypeText { get; set; }
        public System.DateTime LastModifiedDate { get; set; }
        public int ModifiedByID { get; set; }
        public bool IsActive { get; set; }
    
        public virtual CSSPItemType CSSPItemType { get; set; }
    }
}