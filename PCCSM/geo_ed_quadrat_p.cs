//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PCCSM
{
    using System;
    using System.Collections.Generic;
    
    public partial class geo_ed_quadrat_p
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public geo_ed_quadrat_p()
        {
            this.db_ed_donnees_gisement = new HashSet<db_ed_donnees_gisement>();
        }
    
        public int id_geo_ed_quadrat_p { get; set; }
        public int id_tr_reference { get; set; }
        public Nullable<int> id_geo_banc_coquillier_s { get; set; }
        public string lieu { get; set; }
        public string nom_station { get; set; }
        public Nullable<double> x_ll84 { get; set; }
        public Nullable<double> y_ll84 { get; set; }
        public Nullable<double> x { get; set; }
        public Nullable<double> y { get; set; }
        public Nullable<byte> zone_utm { get; set; }
        public Nullable<double> x_utm { get; set; }
        public Nullable<double> y_utm { get; set; }
        public string commentaire { get; set; }
        public string new_nom_station { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<db_ed_donnees_gisement> db_ed_donnees_gisement { get; set; }
        public virtual geo_banc_coquillier_s geo_banc_coquillier_s { get; set; }
        public virtual tr_reference tr_reference { get; set; }
    }
}
