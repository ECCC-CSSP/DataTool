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
    
    public partial class CSSPItemTypesUserAuthorization
    {
        public int CSSPItemTypesUserAuthorizationID { get; set; }
        public Nullable<int> AppUserInfoID { get; set; }
        public Nullable<int> CSSPItemTypeID { get; set; }
        public Nullable<bool> AuthorizeRead { get; set; }
        public Nullable<bool> AuthorizeEdit { get; set; }
        public Nullable<bool> AuthorizeDelete { get; set; }
        public Nullable<bool> AuthorizeCreate { get; set; }
        public Nullable<bool> AuthorizeTVRead { get; set; }
        public Nullable<bool> AuthorizeTVEdit { get; set; }
        public Nullable<bool> AuthorizeTVDelete { get; set; }
        public Nullable<bool> AuthorizeTVCreate { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<int> ModifiedByID { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        public virtual AppUserInfo AppUserInfo { get; set; }
        public virtual CSSPItemType CSSPItemType { get; set; }
    }
}
