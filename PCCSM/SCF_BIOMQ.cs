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
    
    public partial class SCF_BIOMQ
    {
        public int Id_BIOM { get; set; }
        public int Id_Colonie { get; set; }
        public int Id_Espece { get; set; }
        public short Annee { get; set; }
        public string Date { get; set; }
        public string Nombre { get; set; }
        public Nullable<int> Chiffre { get; set; }
        public string Methode { get; set; }
        public string Remarque { get; set; }
        public Nullable<int> Id_Reference { get; set; }
        public bool DernierInventaire { get; set; }
        public Nullable<int> Id_TypePublication { get; set; }
    
        public virtual SCF_Colonies SCF_Colonies { get; set; }
        public virtual SCF_Especes SCF_Especes { get; set; }
    }
}
