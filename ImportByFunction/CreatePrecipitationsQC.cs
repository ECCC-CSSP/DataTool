using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using CSSPEnumsDLL.Enums;
using CSSPWebToolsDBDLL;
using CSSPModelsDLL.Models;
using CSSPWebToolsDBDLL.Services;

namespace ImportByFunction
{
    public class ClimateIDAndQCStation_MeteoID
    {
        public int Station_MeteoID { get; set; }
        public string Station_Nom { get; set; }
        public int ClimateSiteID { get; set; }
        public string ClimateSite_Nom { get; set; }
    }
    public partial class ImportByFunction
    {
        public bool LoadPrecipitationsQC()
        {
            List<ClimateIDAndQCStation_MeteoID> climateIDAndQCStation_MeteoIDList = new List<ClimateIDAndQCStation_MeteoID>();

            LoadClimateIDAndStation_Meteo(climateIDAndQCStation_MeteoIDList);
            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelQC = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelQC)) return false;

            lblStatus.Text = "Starting ... CreatePrecipitationQC";
            Application.DoEvents();

            int StartQCCreatePrecipitationsQC = int.Parse(textBoxQCCreatePrecipitationsQC.Text);

            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {
                List<PCCSM.station_meteo> stationMeteoList = (from c in dbQC.station_meteo
                                                              select c).ToList<PCCSM.station_meteo>();

                List<PCCSM.station_meteo_precipitation_journaliere> precList = (from c in dbQC.station_meteo_precipitation_journaliere
                                                                                orderby c.station_meteo_precipitation_journaliere1
                                                                                select c).ToList<PCCSM.station_meteo_precipitation_journaliere>();

                int Count = 0;
                int TotalCount = precList.Count();
                foreach (PCCSM.station_meteo_precipitation_journaliere prec in precList)
                {
                    if (Cancel) return false;

                    Count += 1;
                    lblStatus.Text = (Count * 100 / TotalCount).ToString() + " ... CreatePrecipitationsQC";
                    lblStatus2.Text = "Doing " + Count + " of " + TotalCount;
                    Application.DoEvents();

                    textBoxQCCreatePrecipitationsQC.Text = Count.ToString();
                    if (StartQCCreatePrecipitationsQC > Count)
                    {
                        continue;
                    }

                    int ClimateSiteID = (from c in stationMeteoList
                                          from s in climateIDAndQCStation_MeteoIDList
                                          where c.ID == prec.stationID
                                          && c.ID == s.Station_MeteoID
                                          select s.ClimateSiteID).FirstOrDefault<int>();
                    if (ClimateSiteID == 0)
                    {
                        richTextBoxStatus.AppendText("Could not find ClimateSiteID from stationMeteoList in CreateQCPrecipitationAll\r\n");
                        return false;
                    }

                    DateTime datePrec = (DateTime)new DateTime(prec.dateprecipitation.Value.Year, prec.dateprecipitation.Value.Month, prec.dateprecipitation.Value.Day);

                    ClimateDataValueModel climateDataValueModelNew = new ClimateDataValueModel()
                    {
                        ClimateSiteID = (int)ClimateSiteID,
                        DateTime_Local = (DateTime)datePrec,
                        Keep = true,
                        StorageDataType = StorageDataTypeEnum.Archived,
                        RainfallEntered_mm = (double?)prec.precipitation24hmm,
                    };

                    ClimateDataValueService climateDataValueService = new ClimateDataValueService(LanguageEnum.en, user);

                    ClimateDataValueModel climateDataValueModel = climateDataValueService.GetClimateDataValueModelExitDB(climateDataValueModelNew);
                    if (string.IsNullOrWhiteSpace(climateDataValueModel.Error))
                    {
                        ClimateDataValueModel climateDataValueModelRet = climateDataValueService.PostAddClimateDataValueDB(climateDataValueModelNew);
                        if (!CheckModelOK<ClimateDataValueModel>(climateDataValueModelRet)) return false;
                    }
                }
            }
            return true;
        }

        public void LoadClimateIDAndStation_Meteo(List<ClimateIDAndQCStation_MeteoID> climateIDAndQCStation_MeteoIDList)
        {
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 1,
                Station_Nom = "Baie-Comeau A (CGBC)",
                ClimateSiteID = 662,
                ClimateSite_Nom = "BAIE-COMEAU A"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 2,
                Station_Nom = "Baie-Comeau (CWFW)",
                ClimateSiteID = 654,
                ClimateSite_Nom = "BAIE-COMEAU"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 3,
                Station_Nom = "Baie-Comeau (CYBC)",
                ClimateSiteID = 662,
                ClimateSite_Nom = "BAIE-COMEAU A"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 4,
                Station_Nom = "Baie-Johan-Beetz (7040MMN)",
                ClimateSiteID = 657,
                ClimateSite_Nom = "BAIE JOHAN BEETZ"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 5,
                Station_Nom = "Baie-Johan-Beetz (7040MMN) et Natashquan (CYNA)",
                ClimateSiteID = 657,
                ClimateSite_Nom = "BAIE JOHAN BEETZ"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 6,
                Station_Nom = "Bic-2 (7050761)",
                ClimateSiteID = 712,
                ClimateSite_Nom = "BIC"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 7,
                Station_Nom = "Lourdes de Blanc-Sablon A (CYBX)",
                ClimateSiteID = 718,
                ClimateSite_Nom = "BLANC SABLON"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 8,
                Station_Nom = "Bonaventure-2 (7050818)",
                ClimateSiteID = 725,
                ClimateSite_Nom = "BONAVENTURE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 9,
                Station_Nom = "Cap-Chat-2 (70510D0)",
                ClimateSiteID = 755,
                ClimateSite_Nom = "CAP CHAT"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 10,
                Station_Nom = "Cap-des-Rosiers (7051055)",
                ClimateSiteID = 760,
                ClimateSite_Nom = "CAP DES ROSIERS"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 11,
                Station_Nom = "Chevery (CWDM)",
                ClimateSiteID = 796,
                ClimateSite_Nom = "CHEVERY"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 12,
                Station_Nom = "Fontenelle (705KL75)",
                ClimateSiteID = 891,
                ClimateSite_Nom = "FONTENELLE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 13,
                Station_Nom = "Forestville - Route-385 (7046710)",
                ClimateSiteID = 892,
                ClimateSite_Nom = "FORESTVILLE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 14,
                Station_Nom = "Godbout (7042749)",
                ClimateSiteID = 917,
                ClimateSite_Nom = "GODBOUT"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 15,
                Station_Nom = "Grandes-Bergeronnes (7042840)",
                ClimateSiteID = 925,
                ClimateSite_Nom = "GRANDES BERGERONNES"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 16,
                Station_Nom = "Havre-Saint-Pierre (CYGV)",
                ClimateSiteID = 938,
                ClimateSite_Nom = "HAVRE-SAINT-PIERRE A"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 17,
                Station_Nom = "Iles-de-la-Madeleine A (CYGR)",
                ClimateSiteID = 966,
                ClimateSite_Nom = "ILES DE LA MADELEINE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 18,
                Station_Nom = "Havre-aux-Maisons-2 (70530A0)",
                ClimateSiteID = 933,
                ClimateSite_Nom = "HAVRE AUX MAISONS"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 19,
                Station_Nom = "Les Buissons (7044288)",
                ClimateSiteID = 1104,
                ClimateSite_Nom = "LES BUISSONS"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 20,
                Station_Nom = "Les Buissons (7044288) et Baie-Comeau (CYBC)",
                ClimateSiteID = 1104,
                ClimateSite_Nom = "LES BUISSONS"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 21,
                Station_Nom = "Longue-Pointe-de-Mingan (CWBT)",
                ClimateSiteID = 1116,
                ClimateSite_Nom = "LONGUE-POINTE-DE-MINGAN"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 22,
                Station_Nom = "Natashquan A (CMNT)",
                ClimateSiteID = 1217,
                ClimateSite_Nom = "NATASHQUAN A"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 23,
                Station_Nom = "Natashquan (CYNA)",
                ClimateSiteID = 1217,
                ClimateSite_Nom = "NATASHQUAN A"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 24,
                Station_Nom = "Nouvelle (7055705)",
                ClimateSiteID = 1245,
                ClimateSite_Nom = "NOUVELLE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 25,
                Station_Nom = "Pentecôte (7045910)",
                ClimateSiteID = 1266,
                ClimateSite_Nom = "PENTECOTE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 26,
                Station_Nom = "Pentecôte (7045910) et Pointe-des-Monts (CWTG)",
                ClimateSiteID = 1266,
                ClimateSite_Nom = "PENTECOTE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 27,
                Station_Nom = "Pointe Noire CS (CWIP)",
                ClimateSiteID = 1287,
                ClimateSite_Nom = "POINTE NOIRE CS"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 28,
                Station_Nom = "Pointe-des-Monts (CWTG)",
                ClimateSiteID = 1284,
                ClimateSite_Nom = "POINTE DES MONTS"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 29,
                Station_Nom = "Port-Daniel (7056120)",
                ClimateSiteID = 1290,
                ClimateSite_Nom = "PORT DANIEL"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 30,
                Station_Nom = "Rivière-au-Tonnerre (704FEG0)",
                ClimateSiteID = 1333,
                ClimateSite_Nom = "RIVIERE AU TONNERRE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 31,
                Station_Nom = "Sept-Îles UA",
                ClimateSiteID = 1405,
                ClimateSite_Nom = "SEPT-ILES A"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 32,
                Station_Nom = "Sept-Îles (CXZV)",
                ClimateSiteID = 1404,
                ClimateSite_Nom = "SEPT-ILES"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 33,
                Station_Nom = "Sept-Îles A (CYZV)",
                ClimateSiteID = 1405,
                ClimateSite_Nom = "SEPT-ILES A"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 34,
                Station_Nom = "Saint-Arsène (7056890)",
                ClimateSiteID = 1434,
                ClimateSite_Nom = "ST ARSENE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 35,
                Station_Nom = "Tadoussac (7048320)",
                ClimateSiteID = 1601,
                ClimateSite_Nom = "TADOUSSAC"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 36,
                Station_Nom = "Pointe de l'Islet",
                ClimateSiteID = 1283,
                ClimateSite_Nom = "POINTE DE L'ISLET"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 37,
                Station_Nom = "Gaspé A (CYGP)",
                ClimateSiteID = 909,
                ClimateSite_Nom = "GASPE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 38,
                Station_Nom = "Sainte-Anne-des-Monts (7056850)",
                ClimateSiteID = 1562,
                ClimateSite_Nom = "STE ANNE DES MONTS"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 39,
                Station_Nom = "Sacré-Coeur-3 (7046738 )",
                ClimateSiteID = 1373,
                ClimateSite_Nom = "SACRE COEUR"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 40,
                Station_Nom = "Cap Whittle (CWQW)",
                ClimateSiteID = 771,
                ClimateSite_Nom = "CAPE WHITTLE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 41,
                Station_Nom = "New Carlisle 1 (CWOC)",
                ClimateSiteID = 1223,
                ClimateSite_Nom = "NEW CARLISLE 1"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 42,
                Station_Nom = "New-Richmond-2 (7055431)",
                ClimateSiteID = 1225,
                ClimateSite_Nom = "NEW RICHMOND"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 43,
                Station_Nom = "Caplan (7051120)",
                ClimateSiteID = 772,
                ClimateSite_Nom = "CAPLAN"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 44,
                Station_Nom = "Ladrière (705LG09)",
                ClimateSiteID = 1082,
                ClimateSite_Nom = "LADRIERE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 45,
                Station_Nom = "Percé-2 (7055921)",
                ClimateSiteID = 1267,
                ClimateSite_Nom = "PERCE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 46,
                Station_Nom = "Rimouski",
                ClimateSiteID = 1329,
                ClimateSite_Nom = "RIMOUSKI"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 47,
                Station_Nom = "Grande-Vallée (7052865)",
                ClimateSiteID = 924,
                ClimateSite_Nom = "GRANDE VALLEE"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 48,
                Station_Nom = "Natashquan A (CYNA)",
                ClimateSiteID = 1217,
                ClimateSite_Nom = "NATASHQUAN A"
            });
            climateIDAndQCStation_MeteoIDList.Add(new ClimateIDAndQCStation_MeteoID()
            {
                Station_MeteoID = 49,
                Station_Nom = "Havre-Saint-Pierre A (CYGV",
                ClimateSiteID = 935,
                ClimateSite_Nom = "HAVRE ST PIERRE A"
            });
        }
    }
}
