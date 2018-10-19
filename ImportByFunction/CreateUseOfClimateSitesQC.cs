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
using CSSPDBDLL;
using CSSPModelsDLL.Models;
using CSSPDBDLL.Services;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CreateUseOfClimateSitesQC()
        {
            List<ClimateIDAndQCStation_MeteoID> climateIDAndQCStation_MeteoIDList = new List<ClimateIDAndQCStation_MeteoID>();

            LoadClimateIDAndStation_Meteo(climateIDAndQCStation_MeteoIDList);

            if (Cancel) return false;

            int StartQCCreateUseOfClimateSitesQC = int.Parse(textBoxQCCreateUseOfClimateSitesQC.Text);

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelQC = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelQC)) return false;

            lblStatus.Text = "Starting ... CreateUseOfClimateSitesQC";

            List<TempData.QCSubsectorAssociation> qcSubAssList = new List<TempData.QCSubsectorAssociation>();
            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                qcSubAssList = (from c in dbDT.QCSubsectorAssociations
                                select c).ToList<TempData.QCSubsectorAssociation>();
            }

            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {
                List<PCCSM.station_meteo> stationMeteoQCList = (from c in dbQC.station_meteo
                                                                select c).ToList<PCCSM.station_meteo>();
                int Count = 0;
                int TotalCount = stationMeteoQCList.Count();
                foreach (PCCSM.station_meteo sm in stationMeteoQCList)
                {
                    if (Cancel) return false;

                    Count += 1;
                    lblStatus.Text = (Count * 100 / TotalCount).ToString() + " ... CreateUseOfClimateSiteQC for " + sm.nom;
                    lblStatus2.Text = Count + " of " + TotalCount;
                    Application.DoEvents();

                    textBoxQCCreateUseOfClimateSitesQC.Text = Count.ToString();

                    if (StartQCCreateUseOfClimateSitesQC > Count)
                    {
                        continue;
                    }

                    var db_zoneList = (from c in dbQC.db_zone_secteur_station_meteo
                                       where c.id_station_meteo == sm.ID
                                       && c.secteur != null
                                       orderby c.secteur, c.annee
                                       select new { c.annee, c.secteur }).Distinct().ToList();

                    foreach (var dbz in db_zoneList)
                    {
                        string subsecText = (from c in qcSubAssList
                                             where c.QCSectorText == dbz.secteur
                                             select c.SubsectorText).FirstOrDefault<string>();

                        if (!string.IsNullOrEmpty(subsecText))
                        {
                            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                            ClimateSiteService climateSiteService = new ClimateSiteService(LanguageEnum.en, user);
                            UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);

                            TVItemModel tvItemSubsector = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelQC.TVItemID, subsecText, TVTypeEnum.Subsector);
                            if (!CheckModelOK<TVItemModel>(tvItemSubsector))
                            {
                                richTextBoxStatus.AppendText("Could not find subsector association for [" + dbz.secteur + "]\r\n");
                                return false;
                            }

                            UseOfSiteModel useOfSiteModelNew = new UseOfSiteModel()
                            {
                                SiteTVItemID = climateSiteService.GetClimateSiteModelWithClimateSiteIDDB(climateIDAndQCStation_MeteoIDList.Where(c => c.Station_MeteoID == sm.ID).FirstOrDefault().ClimateSiteID).ClimateSiteTVItemID,
                                SubsectorTVItemID = tvItemSubsector.TVItemID,
                                StartYear = (int)dbz.annee,
                                EndYear = (int)dbz.annee,
                                Ordinal = 0,
                                SiteType = SiteTypeEnum.Climate,
                            };

                            UseOfSiteModel useOfSiteModelExist = useOfSiteService.GetUseOfSiteModelExist(useOfSiteModelNew);
                            if (!string.IsNullOrWhiteSpace(useOfSiteModelExist.Error))
                            {
                                UseOfSiteModel useOfSiteModelRet = useOfSiteService.PostAddUseOfSiteDB(useOfSiteModelNew);
                                if (!CheckModelOK<UseOfSiteModel>(useOfSiteModelRet)) return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
