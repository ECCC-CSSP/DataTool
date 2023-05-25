using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using CSSPEnumsDLL.Enums;
using CSSPModelsDLL.Models;
using CSSPDBDLL.Services;
using CSSPDBDLL;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CreateStationsQC()
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            TVItemLanguageService tvItemLanguageService = new TVItemLanguageService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelQC = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelQC)) return false;

            List<TVItemModel> tvItemModelSubsectorQCList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorQCList.Count == 0)
            {
                richTextBoxStatus.AppendText("Could not find TVItem Subsector under British Columbia\r\n");
                return false;
            }

            lblStatus.Text = "Starting ... CreateStationsQC";
            Application.DoEvents();

            int StartQCCreateStationQC = int.Parse(textBoxQCCreateStationsQC.Text);

            List<Obs> obsTypeList = new List<Obs>();
            List<string> sectorList = new List<string>();
            List<TT> tideTextInDBList = new List<TT>();
            List<AM> analyseMethodInDBList = new List<AM>();
            List<Mat> matrixInDBList = new List<Mat>();
            List<Lab> labInDBList = new List<Lab>();
            List<SampleStatus> sampleStatusInDBList = new List<SampleStatus>();
            List<TempData.QCSubsectorAssociation> qcSubAssList = new List<TempData.QCSubsectorAssociation>();

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                qcSubAssList = (from c in dbDT.QCSubsectorAssociations
                                select c).ToList<TempData.QCSubsectorAssociation>();
            }

            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {
                sectorList = (from s in dbQC.geo_stations_p
                              where s.secteur != null
                              select s.secteur).Distinct().OrderBy(c => c).ToList();
            }

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
            List<TVItemModel> tvItemModelSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.MWQMSite);

            TempData.QCSubsectorAssociation qcSubsectAss = new TempData.QCSubsectorAssociation();

            // doing every sector with the exception of MS__

            List<PCCSM.geo_stations_p> staQCList = new List<PCCSM.geo_stations_p>();
            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {

                staQCList = (from c in dbQC.geo_stations_p
                             where (c.x != null && c.y != null)
                             && c.secteur != null
                             orderby c.secteur, c.station
                             select c).ToList<PCCSM.geo_stations_p>();
            }

            List<TVItemLanguage> tvItemSiteLanguageList = new List<TVItemLanguage>();
            //int TVItemIDSSOld = 0;
            int count = 0;
            int total = staQCList.Count;
            foreach (PCCSM.geo_stations_p geoStat in staQCList)
            {
                if (Cancel) return false;

                textBoxQCCreateStationsQC.Text = count.ToString();

                if (StartQCCreateStationQC > count)
                {
                    continue;
                }

                qcSubsectAss = (from c in qcSubAssList
                                where c.QCSectorText == geoStat.secteur
                                select c).FirstOrDefault<TempData.QCSubsectorAssociation>();

                if (qcSubsectAss == null)
                {
                    richTextBoxStatus.AppendText(geoStat.secteur + " does not exist\r\n");
                    //return false;
                    continue;
                }

                TVItemModel tvItemModelSubsector = (from c in tvItemModelSubsectorList
                                                    where c.TVText.StartsWith(qcSubsectAss.SubsectorText)
                                                    select c).FirstOrDefault();

                if (tvItemModelSubsector == null)
                {
                    richTextBoxStatus.AppendText("could not find subsector" + geoStat.secteur + "\r\n");
                    //return false;
                    continue;
                }

                continue;

                //if (TVItemIDSSOld != tvItemModelSubsector.TVItemID)
                //{
                //    using (CSSPDBEntities db2 = new CSSPDBEntities())
                //    {
                //        tvItemSiteLanguageList = (from c in db2.TVItems
                //                                  from cl in db2.TVItemLanguages
                //                                  where c.TVItemID == cl.TVItemID
                //                                  && c.ParentID == tvItemModelSubsector.TVItemID
                //                                  && c.TVType == (int)TVTypeEnum.MWQMSite
                //                                  select cl).ToList();

                //    }

                //    TVItemIDSSOld = tvItemModelSubsector.TVItemID;
                //}              

                //bool IsActive = true;
                //if (geoStat.status != null)
                //{
                //    IsActive = (geoStat.status.Substring(0, 1) == "i" ? false : true);
                //}

                //string PreText = "";
                //if (geoStat.secteur.Length < qcSubsectAss.SubsectorText.Length)
                //{
                //    PreText = "";
                //}
                //else
                //{
                //    if (geoStat.secteur.StartsWith(qcSubsectAss.SubsectorText))
                //    {
                //        PreText = geoStat.secteur.Substring(qcSubsectAss.SubsectorText.Length) + "_";
                //    }
                //    else
                //    {
                //        PreText = geoStat.secteur + "_";
                //    }
                //}
                //if (PreText.StartsWith(".") || PreText.StartsWith("_"))
                //{
                //    PreText = PreText.Substring(1);
                //}

                //string MWQMSiteTVText = PreText + "0000".Substring(0, 4 - geoStat.station.ToString().Length) + geoStat.station.ToString();

                //string subsector = tvItemModelSubsector.TVText;
                //if (subsector.Contains(" "))
                //{
                //    subsector = subsector.Substring(0, subsector.IndexOf(" "));
                //}

                //count += 1;
                //lblStatus.Text = $"{subsector} --- {MWQMSiteTVText} --- { count.ToString()}/{total.ToString()}";
                //Application.DoEvents();


                //List<MapInfoModel> mapInfoModelList = mapInfoService.GetMapInfoModelWithLatAndLngInPolygonWithTVTypeDB((float)geoStat.y, (float)geoStat.x, TVTypeEnum.Subsector);
                //if (mapInfoModelList.Count == 0)
                //{
                //    richTextBoxStatus.AppendText($"{geoStat.x}, {geoStat.y}, {geoStat.station}, {geoStat.secteur}\r\n");
                //    //return false;
                //    continue;
                //}

                //TVItemModel tvItemModel = (from c in tvItemModelSiteList
                //                           where c.ParentID == tvItemModelSubsector.TVItemID
                //                           && c.TVText.EndsWith(MWQMSiteTVText)
                //                           select c).FirstOrDefault();

                ////TVItemModel tvItemModel = tvItemModelMWQMSiteList.Where(c => c.TVText.EndsWith(MWQMSiteTVText)).FirstOrDefault();
                //if (tvItemModel == null)
                //{
                //    TVItemModel tvItemModelRet = tvItemService.PostCreateTVItem(tvItemModelSubsector.TVItemID, MWQMSiteTVText, MWQMSiteTVText, TVTypeEnum.MWQMSite);
                //    if (!CheckModelOK<TVItemModel>(tvItemModelRet))
                //    {
                //        //return false;
                //        continue;
                //    }

                //    if (geoStat.status == null)
                //    {
                //        tvItemModelRet.IsActive = false;
                //    }
                //    else
                //    {
                //        tvItemModelRet.IsActive = (geoStat.status.Substring(0, 1) == "i" ? false : true);
                //    }

                //    TVItemModel tvItemModelRet2 = tvItemService.PostUpdateTVItemDB(tvItemModelRet);
                //    if (!CheckModelOK<TVItemModel>(tvItemModelRet2))
                //    {
                //        //return false;
                //        continue;
                //    }

                //    List<Coord> coordList2 = new List<Coord>()
                //            {
                //                new Coord()
                //                {
                //                    Lat = (float)geoStat.y,
                //                    Lng = (float)geoStat.x,
                //                }
                //            };

                //    MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.MWQMSite, tvItemModelRet2.TVItemID);
                //    if (!CheckModelOK<MapInfoModel>(mapInfoModelRet))
                //    {
                //        //return false;
                //        continue;
                //    }

                //    // should add the QC station to WQMSite
                //    MWQMSiteModel mwqmSiteModelNew = new MWQMSiteModel()
                //    {
                //        MWQMSiteTVItemID = tvItemModelRet2.TVItemID,
                //        MWQMSiteNumber = geoStat.station.ToString(),
                //        Ordinal = (int)geoStat.station,
                //        MWQMSiteTVText = MWQMSiteTVText,
                //        MWQMSiteDescription = "--"
                //    };

                //    MWQMSiteModel mwqmSiteModelRet = mwqmSiteService.PostAddMWQMSiteDB(mwqmSiteModelNew);
                //    if (!CheckModelOK<MWQMSiteModel>(mwqmSiteModelRet))
                //    {
                //        //return false;
                //        continue;
                //    }

                //}
                //else
                //{
                //    TVItemLanguage tvItemLanguageEN = (from c in tvItemSiteLanguageList
                //                                       where c.TVItemID == tvItemModel.TVItemID
                //                                       && c.Language == (int)LanguageEnum.en
                //                                       select c).FirstOrDefault();

                //    TVItemLanguage tvItemLanguageFR = (from c in tvItemSiteLanguageList
                //                                       where c.TVItemID == tvItemModel.TVItemID
                //                                       && c.Language == (int)LanguageEnum.fr
                //                                       select c).FirstOrDefault();

                //    if (tvItemLanguageEN.TVText != MWQMSiteTVText || tvItemLanguageFR.TVText != MWQMSiteTVText)
                //    {
                //        foreach (LanguageEnum language in new List<LanguageEnum>() { LanguageEnum.en, LanguageEnum.fr })
                //        {


                //            TVItemLanguageModel tvItemLanguageModel = tvItemLanguageService.GetTVItemLanguageModelWithTVItemIDAndLanguageDB(tvItemModel.TVItemID, language);
                //            if (!CheckModelOK<TVItemLanguageModel>(tvItemLanguageModel))
                //            {
                //                //return false;
                //                continue;
                //            }

                //            if (tvItemLanguageModel.TVText != MWQMSiteTVText)
                //            {
                //                tvItemLanguageModel.TVText = MWQMSiteTVText;

                //                TVItemLanguageModel tvItemLanguageModelRet = tvItemLanguageService.PostUpdateTVItemLanguageDB(tvItemLanguageModel);
                //                if (!CheckModelOK<TVItemLanguageModel>(tvItemLanguageModelRet))
                //                {
                //                    //return false;
                //                    continue;
                //                }
                //            }
                //        }
                //    }
                //}
            }

            return true;
        }
    }
}
