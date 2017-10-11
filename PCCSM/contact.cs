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
    
    public partial class contact
    {
        public contact()
        {
            this.contact_groupe = new HashSet<contact_groupe>();
            this.db_banc_source_info = new HashSet<db_banc_source_info>();
            this.laboratoire_operateur = new HashSet<laboratoire_operateur>();
        }
    
        public int contact_id { get; set; }
        public string nom { get; set; }
        public string prenom { get; set; }
        public Nullable<int> organisation { get; set; }
        public Nullable<int> titre { get; set; }
        public string telephone { get; set; }
        public string cellulaire { get; set; }
        public string fax { get; set; }
        public string courriel { get; set; }
        public string adresse_postale { get; set; }
        public Nullable<int> ville { get; set; }
        public string code_postal { get; set; }
        public Nullable<int> province { get; set; }
        public string nom_user { get; set; }
        public string commentaire { get; set; }
        public Nullable<int> ID_Infra { get; set; }
    
        public virtual ICollection<contact_groupe> contact_groupe { get; set; }
        public virtual ICollection<db_banc_source_info> db_banc_source_info { get; set; }
        public virtual ICollection<laboratoire_operateur> laboratoire_operateur { get; set; }
    }
}