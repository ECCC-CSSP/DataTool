﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OldASGAD
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class OldASGADEntities : DbContext
    {
        public OldASGADEntities()
            : base("name=OldASGADEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<ASGADDefPrecipStation> ASGADDefPrecipStations { get; set; }
        public DbSet<ASGADMPN> ASGADMPNs { get; set; }
        public DbSet<ASGADPrecipStation> ASGADPrecipStations { get; set; }
        public DbSet<ASGADPrecipValue> ASGADPrecipValues { get; set; }
        public DbSet<ASGADRun> ASGADRuns { get; set; }
        public DbSet<ASGADSampleCode> ASGADSampleCodes { get; set; }
        public DbSet<ASGADSample> ASGADSamples { get; set; }
        public DbSet<ASGADStation> ASGADStations { get; set; }
        public DbSet<ASGADSubsecDefPrecipStation> ASGADSubsecDefPrecipStations { get; set; }
        public DbSet<ASGADSubsector> ASGADSubsectors { get; set; }
        public DbSet<ASGADTide> ASGADTides { get; set; }
        public DbSet<sysdiagram> sysdiagrams { get; set; }
    }
}
