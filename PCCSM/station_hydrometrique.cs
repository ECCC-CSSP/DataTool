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
    
    public partial class station_hydrometrique
    {
        public station_hydrometrique()
        {
            this.station_hydrometrique_debit = new HashSet<station_hydrometrique_debit>();
            this.station_hydrometrique_secteur = new HashSet<station_hydrometrique_secteur>();
        }
    
        public string station { get; set; }
        public string nom { get; set; }
        public string description { get; set; }
        public string etat { get; set; }
        public string municipalite { get; set; }
        public string region_administrative { get; set; }
        public string lac_cours_eau { get; set; }
        public string donnee_ddiffusee { get; set; }
        public string coordonnees { get; set; }
        public Nullable<double> y { get; set; }
        public Nullable<double> x { get; set; }
    
        public virtual ICollection<station_hydrometrique_debit> station_hydrometrique_debit { get; set; }
        public virtual ICollection<station_hydrometrique_secteur> station_hydrometrique_secteur { get; set; }
    }
}