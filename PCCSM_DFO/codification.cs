//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PCCSM_DFO
{
    using System;
    using System.Collections.Generic;
    
    public partial class codification
    {
        public codification()
        {
            this.clauses = new HashSet<clause>();
            this.clause_espece = new HashSet<clause_espece>();
            this.clause_interdiction = new HashSet<clause_interdiction>();
            this.clause_methode = new HashSet<clause_methode>();
            this.clause_periode = new HashSet<clause_periode>();
            this.codification1 = new HashSet<codification>();
            this.codification_description = new HashSet<codification_description>();
            this.codification_description1 = new HashSet<codification_description>();
            this.contacts = new HashSet<contact>();
            this.contacts1 = new HashSet<contact>();
            this.contacts2 = new HashSet<contact>();
            this.contacts3 = new HashSet<contact>();
            this.contact_communication = new HashSet<contact_communication>();
            this.contact_groupe = new HashSet<contact_groupe>();
            this.ordonnances = new HashSet<ordonnance>();
            this.ordonnances1 = new HashSet<ordonnance>();
            this.ordonnances2 = new HashSet<ordonnance>();
        }
    
        public int codification_id { get; set; }
        public Nullable<int> group_id { get; set; }
        public Nullable<int> parent_id { get; set; }
        public string description { get; set; }
        public Nullable<int> etat { get; set; }
        public Nullable<int> codification_old { get; set; }
    
        public virtual ICollection<clause> clauses { get; set; }
        public virtual ICollection<clause_espece> clause_espece { get; set; }
        public virtual ICollection<clause_interdiction> clause_interdiction { get; set; }
        public virtual ICollection<clause_methode> clause_methode { get; set; }
        public virtual ICollection<clause_periode> clause_periode { get; set; }
        public virtual ICollection<codification> codification1 { get; set; }
        public virtual codification codification2 { get; set; }
        public virtual ICollection<codification_description> codification_description { get; set; }
        public virtual ICollection<codification_description> codification_description1 { get; set; }
        public virtual ICollection<contact> contacts { get; set; }
        public virtual ICollection<contact> contacts1 { get; set; }
        public virtual ICollection<contact> contacts2 { get; set; }
        public virtual ICollection<contact> contacts3 { get; set; }
        public virtual ICollection<contact_communication> contact_communication { get; set; }
        public virtual ICollection<contact_groupe> contact_groupe { get; set; }
        public virtual ICollection<ordonnance> ordonnances { get; set; }
        public virtual ICollection<ordonnance> ordonnances1 { get; set; }
        public virtual ICollection<ordonnance> ordonnances2 { get; set; }
    }
}
