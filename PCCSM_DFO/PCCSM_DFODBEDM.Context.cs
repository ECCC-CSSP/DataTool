﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class pccsm_mpoEntities : DbContext
    {
        public pccsm_mpoEntities()
            : base("name=pccsm_mpoEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<clause> clauses { get; set; }
        public DbSet<clause_espece> clause_espece { get; set; }
        public DbSet<clause_interdiction> clause_interdiction { get; set; }
        public DbSet<clause_methode> clause_methode { get; set; }
        public DbSet<clause_periode> clause_periode { get; set; }
        public DbSet<clause_secteur> clause_secteur { get; set; }
        public DbSet<codification> codifications { get; set; }
        public DbSet<codification_description> codification_description { get; set; }
        public DbSet<contact> contacts { get; set; }
        public DbSet<contact_communication> contact_communication { get; set; }
        public DbSet<contact_groupe> contact_groupe { get; set; }
        public DbSet<db_histo_operation> db_histo_operation { get; set; }
        public DbSet<demande_de_site_region> demande_de_site_region { get; set; }
        public DbSet<echantillonnage_microbiol> echantillonnage_microbiol { get; set; }
        public DbSet<echantillonnage_toxicite> echantillonnage_toxicite { get; set; }
        public DbSet<geo_limite_5km_rive_l_3857> geo_limite_5km_rive_l_3857 { get; set; }
        public DbSet<geo_limite_5km_rive_l_4269> geo_limite_5km_rive_l_4269 { get; set; }
        public DbSet<geo_pccsm_acia_station> geo_pccsm_acia_station { get; set; }
        public DbSet<geo_segment_l> geo_segment_l { get; set; }
        public DbSet<geometry_columns> geometry_columns { get; set; }
        public DbSet<groupe_secteur> groupe_secteur { get; set; }
        public DbSet<historique_operation> historique_operation { get; set; }
        public DbSet<language> languages { get; set; }
        public DbSet<mrc> mrcs { get; set; }
        public DbSet<munic> munics { get; set; }
        public DbSet<ordonnance> ordonnances { get; set; }
        public DbSet<point_legal> point_legal { get; set; }
        public DbSet<secteur> secteurs { get; set; }
        public DbSet<secteur_coordonnees_epsg_3857> secteur_coordonnees_epsg_3857 { get; set; }
        public DbSet<secteur_description> secteur_description { get; set; }
        public DbSet<secteur_epsg3857> secteur_epsg3857 { get; set; }
        public DbSet<secteur_historique> secteur_historique { get; set; }
        public DbSet<secteur_point_archive> secteur_point_archive { get; set; }
        public DbSet<secteur_point_legal> secteur_point_legal { get; set; }
        public DbSet<seg_geo_segment> seg_geo_segment { get; set; }
        public DbSet<seg_limite_acces> seg_limite_acces { get; set; }
        public DbSet<seg_milieu> seg_milieu { get; set; }
        public DbSet<seg_processus_dominant> seg_processus_dominant { get; set; }
        public DbSet<seg_segment_usage> seg_segment_usage { get; set; }
        public DbSet<seg_segrivB_nad83mdb> seg_segrivB_nad83mdb { get; set; }
        public DbSet<seg_substrat> seg_substrat { get; set; }
        public DbSet<seg_tr_code> seg_tr_code { get; set; }
        public DbSet<seg_tr_interface_label_new> seg_tr_interface_label_new { get; set; }
        public DbSet<seg_tr_source_dvd> seg_tr_source_dvd { get; set; }
        public DbSet<seg_type_cote> seg_type_cote { get; set; }
        public DbSet<seg_zone_cotiere> seg_zone_cotiere { get; set; }
        public DbSet<segmentation_resultat> segmentation_resultat { get; set; }
        public DbSet<signalisation> signalisations { get; set; }
        public DbSet<spatial_ref_sys> spatial_ref_sys { get; set; }
        public DbSet<sysdiagram> sysdiagrams { get; set; }
        public DbSet<ligne_5km_cote> ligne_5km_cote { get; set; }
        public DbSet<organisation> organisations { get; set; }
        public DbSet<segment> segments { get; set; }
        public DbSet<temp_secteur> temp_secteur { get; set; }
    }
}
