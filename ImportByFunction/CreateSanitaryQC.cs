using CSSPDBDLL;
using CSSPModelsDLL.Models;
using CSSPDBDLL.Services;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using CSSPEnumsDLL.Enums;
using CSSPEnumsDLL.Services;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public class TVItemSubsectorAndCoordCentroid
        {
            public int TVItemID { get; set; }
            public float Lat { get; set; }
            public float Lng { get; set; }
        }
        public bool CreateSanitaryQC()
        {
            if (Cancel) return false;

            if (!CreateQCPolSourceAll())
            {
                return false;
            }
            if (!CreateQCPolSourceNullAll())
            {
                return false;
            }

            return true;
        }
        public bool CreateQCPolSourceAll()
        {
            if (Cancel) return false;

            lblStatus.Text = "Starting ... CreateSanitaryQC - CreateQCPolSourceAll";
            Application.DoEvents();

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelQC = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelQC)) return false;

            List<TVItemModel> TVItemModelSubsectorList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
            if (TVItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelQC.TVText + "\r\n");
                return false;
            }

            List<string> NoSecList = new List<string>() { "M", "G-00" };

            List<TempData.QCSecteurMPol> qcSecteurMPol = new List<TempData.QCSecteurMPol>();
            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                qcSecteurMPol = (from c in dbDT.QCSecteurMPols
                                 select c).ToList<TempData.QCSecteurMPol>();
            }

            List<Obs> obsTypeList = new List<Obs>();
            List<string> sectorList = new List<string>();
            List<TempData.QCSubsectorAssociation> qcsubsectorAssociationList = new List<TempData.QCSubsectorAssociation>();

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                qcsubsectorAssociationList = (from c in dbDT.QCSubsectorAssociations
                                              select c).ToList<TempData.QCSubsectorAssociation>();
            }

            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {
                sectorList = (from s in dbQC.geo_pollution_p
                              select s.secteur).Distinct().ToList();
            }

            List<string> sectorOrderedList = (from c in sectorList
                                              orderby c
                                              select c).ToList();

            int StartQCCreateSanitarysQC = int.Parse(textBoxQCCreateSanitaryQC.Text);

            int TotalCount = sectorOrderedList.Count();
            int Count = 0;
            foreach (string sec in sectorOrderedList)
            {
                Count += 1;

                TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                PolSourceSiteService polSourceSiteService = new PolSourceSiteService(LanguageEnum.en, user);
                MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
                PolSourceObservationService polSourceObservationService = new PolSourceObservationService(LanguageEnum.en, user);
                PolSourceObservationIssueService polSourceObservationIssueService = new PolSourceObservationIssueService(LanguageEnum.en, user);

                lblStatus.Text = (Count * 100 / TotalCount).ToString() + " ... CreateSanitaryQC for sector " + sec;
                lblStatus2.Text = Count + " of " + TotalCount;
                Application.DoEvents();

                textBoxQCCreateSanitaryQC.Text = Count.ToString();

                if (StartQCCreateSanitarysQC > Count)
                {
                    continue;
                }

                if (sec != null)
                {
                    Application.DoEvents();

                    TVItemModel tvItemModelSubsector = new TVItemModel();

                    if (sec.StartsWith("M") || sec.StartsWith("G-00"))
                    {
                    }
                    else
                    {
                        TempData.QCSubsectorAssociation qcsubAss = (from c in qcsubsectorAssociationList
                                                                    where c.QCSectorText == sec
                                                                    select c).FirstOrDefault<TempData.QCSubsectorAssociation>();

                        tvItemModelSubsector = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelQC.TVItemID, qcsubAss.SubsectorText, TVTypeEnum.Subsector);
                        if (!CheckModelOK<TVItemModel>(tvItemModelSubsector)) return false;
                    }

                    List<PCCSM.geo_pollution_p> polQCList = new List<PCCSM.geo_pollution_p>();
                    using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
                    {

                        polQCList = (from c in dbQC.geo_pollution_p
                                     where c.secteur == sec
                                     && c.id_geo_pollution_p != 0
                                     && (c.x != null && c.y != null)
                                     //&& c.ex.exlure_importation == false
                                     select c).ToList<PCCSM.geo_pollution_p>();
                    }

                    int countPol = 0;
                    int totalCountPol = polQCList.Count;
                    foreach (PCCSM.geo_pollution_p pqc in polQCList)
                    {
                        if (Cancel)
                        {
                            return false;
                        }

                        countPol += 1;
                        lblStatus2.Text = "Doing " + countPol + " of " + totalCountPol;
                        Application.DoEvents();

                        PolSourceSiteModel polSourceSiteModelNew = new PolSourceSiteModel();

                        string Code = tvItemService.CleanText(pqc.code.ToUpper());
                        List<PolSourceObsInfoEnum> polSourceObsInfoList = GetPolSourceType(Code.Trim(), "--", "QC");

                        string ObservationInfo = (int)polSourceObsInfoList[0] + "," + (int)polSourceObsInfoList[1] + ",";

                        polSourceSiteModelNew.IsPointSource = true;
                        //if (pqc.status == "actif")
                        //{
                        //    polSourceSiteModelNew.IsActive = true;
                        //}
                        //else
                        //{
                        //    polSourceSiteModelNew.IsActive = false;
                        //}
                        polSourceSiteModelNew.Oldsiteid = pqc.id_geo_pollution_p;

                        string SectText = (from c in qcSecteurMPol
                                           where c.geo_pollution_id == pqc.id_geo_pollution_p
                                           select c.Subsector).FirstOrDefault<string>();

                        if (string.IsNullOrWhiteSpace(SectText))
                        {
                            List<MapInfoModel> mapInfoModelList = mapInfoService.GetMapInfoModelWithLatAndLngInPolygonWithTVTypeDB((float)pqc.y, (float)pqc.x, TVTypeEnum.Subsector);

                            foreach (MapInfoModel mapInfoModel in mapInfoModelList)
                            {
                                List<MapInfoPointModel> mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithMapInfoIDDB(mapInfoModel.MapInfoID);

                                List<Coord> coordList2 = new List<Coord>();
                                foreach (MapInfoPointModel mapInfoPointModel in mapInfoPointModelList)
                                {
                                    coordList2.Add(new Coord() { Lat = (float)mapInfoPointModel.Lat, Lng = (float)mapInfoPointModel.Lng, Ordinal = mapInfoPointModel.Ordinal });
                                }

                                if (mapInfoService.CoordInPolygon(coordList2, new Coord() { Lat = (float)pqc.y, Lng = (float)pqc.x, Ordinal = 0 }))
                                {
                                    TVItemModel tvItemModelSS = tvItemService.GetTVItemModelWithTVItemIDDB(mapInfoModel.TVItemID);
                                    SectText = tvItemModelSS.TVText.Substring(0, tvItemModelSS.TVText.IndexOf(" ")).Trim();

                                    using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                                    {
                                        TempData.QCSecteurMPol qcSecteurMPolExist = (from c in dbDT.QCSecteurMPols
                                                                                     where c.Subsector == SectText
                                                                                     && c.geo_pollution_id == pqc.id_geo_pollution_p
                                                                                     select c).FirstOrDefault();

                                        if (qcSecteurMPolExist == null)
                                        {
                                            TempData.QCSecteurMPol qcsmpol = new TempData.QCSecteurMPol()
                                            {
                                                geo_pollution_id = pqc.id_geo_pollution_p,
                                                Subsector = SectText,
                                            };

                                            dbDT.QCSecteurMPols.Add(qcsmpol);
                                            try
                                            {
                                                dbDT.SaveChanges();
                                            }
                                            catch (Exception ex)
                                            {
                                                richTextBoxStatus.AppendText("Error saving new TempData.QCSecteurMPol [" + ex.Message + "]");
                                                return false;
                                            }
                                        }
                                    }

                                    break;
                                }
                            }

                            if (string.IsNullOrWhiteSpace(SectText))
                            {
                                int MapInfoID = 0;
                                float MinDist = 10000000f;
                                foreach (TVItemModel tvItemModel in TVItemModelSubsectorList)
                                {
                                    List<MapInfoPointModel> mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModel.TVItemID, TVTypeEnum.Subsector, MapInfoDrawTypeEnum.Point);

                                    float tempDist = (float)mapInfoService.CalculateDistance(mapInfoPointModelList[0].Lat, mapInfoPointModelList[0].Lng, (double)pqc.y, (double)pqc.x, mapInfoService.R);

                                    if (tempDist < MinDist)
                                    {
                                        MapInfoID = mapInfoPointModelList[0].MapInfoID;
                                        MinDist = tempDist;
                                    }
                                }

                                MapInfoModel mapInfoModel = mapInfoService.GetMapInfoModelWithMapInfoIDDB(MapInfoID);
                                if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                                {
                                    richTextBoxStatus.AppendText("Error [" + mapInfoModel.Error + "]");
                                    return false;
                                }

                                TVItemModel tvItemModelSS = tvItemService.GetTVItemModelWithTVItemIDDB(mapInfoModel.TVItemID);
                                SectText = tvItemModelSS.TVText.Substring(0, tvItemModelSS.TVText.IndexOf(" ")).Trim();
                            }
                        }

                        if (string.IsNullOrWhiteSpace(SectText))
                        {
                            richTextBoxStatus.AppendText(pqc.y + " " + pqc.x + " " + pqc.id_geo_pollution_p + "\r\n");
                            continue;
                        }

                        tvItemModelSubsector = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelQC.TVItemID, SectText, TVTypeEnum.Subsector);
                        if (!CheckModelOK<TVItemModel>(tvItemModelSubsector)) return false;

                        // doing EN TVText
                        string PolSourceSiteTVTextEN = _BaseEnumService.GetEnumText_PolSourceObsInfoEnum(polSourceObsInfoList[0]);
                        if (polSourceObsInfoList.Count > 1)
                        {
                            if (polSourceObsInfoList[1] != PolSourceObsInfoEnum.Error)
                            {
                                PolSourceSiteTVTextEN += " - " + _BaseEnumService.GetEnumText_PolSourceObsInfoEnum(polSourceObsInfoList[1]);
                            }
                        }

                        PolSourceSiteTVTextEN = tvItemService.CleanText(PolSourceSiteTVTextEN + " - " + "      ".Substring(0, 6 - pqc.id_geo_pollution_p.ToString().Length) + pqc.id_geo_pollution_p.ToString());

                        Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-CA");
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-CA");

                        // Doing FR TVText
                        PolSourceObservationService polSourceObservationService2 = new PolSourceObservationService(LanguageEnum.fr, user);

                        string PolSourceSiteTVTextFR = _BaseEnumService.GetEnumText_PolSourceObsInfoEnum(polSourceObsInfoList[0]);
                        if (polSourceObsInfoList.Count > 1)
                        {
                            if (polSourceObsInfoList[1] != PolSourceObsInfoEnum.Error)
                            {
                                PolSourceSiteTVTextFR += " - " + _BaseEnumService.GetEnumText_PolSourceObsInfoEnum(polSourceObsInfoList[1]);
                            }
                        }

                        PolSourceSiteTVTextFR = tvItemService.CleanText(PolSourceSiteTVTextFR + " - " + "      ".Substring(0, 6 - pqc.id_geo_pollution_p.ToString().Length) + pqc.id_geo_pollution_p.ToString());

                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-CA");

                        TVItemModel tvItemModelPolSourceSite = tvItemService.PostCreateTVItem(tvItemModelSubsector.TVItemID, PolSourceSiteTVTextEN, PolSourceSiteTVTextFR, TVTypeEnum.PolSourceSite);
                        if (!CheckModelOK<TVItemModel>(tvItemModelPolSourceSite)) return false;

                        List<Coord> coordList = new List<Coord>()
                        {
                            new Coord()
                            {
                                Lat = (float)pqc.y,
                                Lng = (float)pqc.x,
                            }
                        };

                        MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.PolSourceSite, tvItemModelPolSourceSite.TVItemID);
                        if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                        polSourceSiteModelNew.PolSourceSiteTVItemID = tvItemModelPolSourceSite.TVItemID;
                        polSourceSiteModelNew.PolSourceSiteTVText = PolSourceSiteTVTextEN;

                        PolSourceSiteModel polSourceSiteModelRet = polSourceSiteService.GetPolSourceSiteModelWithPolSourceSiteTVItemIDDB(tvItemModelPolSourceSite.TVItemID);
                        if (!string.IsNullOrWhiteSpace(polSourceSiteModelRet.Error))
                        {
                            polSourceSiteModelRet = polSourceSiteService.PostAddPolSourceSiteDB(polSourceSiteModelNew);
                            if (!CheckModelOK<PolSourceSiteModel>(polSourceSiteModelRet)) return false;
                        }

                        string TextObs = tvItemService.CleanText(string.IsNullOrEmpty(pqc.description) ? "" : pqc.description);

                        PolSourceObservationModel polSourceObservationModelNew = new PolSourceObservationModel();
                        if (pqc.date_observation == null)
                        {
                            polSourceObservationModelNew.ObservationDate_Local = new DateTime(2050, 1, 1);
                        }
                        else
                        {
                            polSourceObservationModelNew.ObservationDate_Local = (DateTime)(pqc.date_observation.Value).AddHours(1);
                        }

                        string observateur = "unknown";
                        if (pqc.observateur != null)
                        {
                            if (pqc.observateur.Length > 98)
                            {
                                observateur = pqc.observateur.Substring(0, 60);
                            }
                            else
                            {
                                observateur = pqc.observateur;
                            }
                        }

                        string observation = "(empty)";
                        if (pqc.description != null)
                        {
                            observation = Code.ToString().ToUpper() + " - " + pqc.description;
                        }

                        string TVTextInspectorEN = "Inspector " + observateur + " - (QC)";
                        string TVTextInspectorFR = "Inspecteur " + observateur + " - (QC)";

                        TVItemModel tvItemModelContact = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, TVTextInspectorEN, TVTypeEnum.Contact);
                        if (!string.IsNullOrWhiteSpace(tvItemModelContact.Error))
                        {
                            tvItemModelContact = tvItemService.PostCreateTVItem(tvItemModelRoot.TVItemID, TVTextInspectorEN, TVTextInspectorFR, TVTypeEnum.Contact);
                            if (!CheckModelOK<TVItemModel>(tvItemModelContact)) return false;
                        }

                        polSourceObservationModelNew.ContactTVItemID = tvItemModelContact.TVItemID;

                        polSourceObservationModelNew.PolSourceSiteID = polSourceSiteModelRet.PolSourceSiteID;
                        polSourceObservationModelNew.Observation_ToBeDeleted = observation;

                        PolSourceObservationModel polSourceObservationModelRet = polSourceObservationService.GetPolSourceObservationModelExistDB(polSourceObservationModelNew);
                        if (!string.IsNullOrWhiteSpace(polSourceObservationModelRet.Error))
                        {
                            polSourceObservationModelRet = polSourceObservationService.PostAddPolSourceObservationDB(polSourceObservationModelNew);
                            if (!CheckModelOK<PolSourceObservationModel>(polSourceObservationModelRet)) return false;
                        }

                        PolSourceObservationIssueModel polSourceObservationIssueModelNew = new PolSourceObservationIssueModel()
                        {
                            PolSourceObservationID = polSourceObservationModelNew.PolSourceObservationID,
                            ObservationInfo = ObservationInfo,
                            PolSourceObsInfoList = polSourceObsInfoList,
                            Ordinal = 0,
                        };

                        PolSourceObservationIssueModel polSourceObservationIssueModelRet = polSourceObservationIssueService.GetPolSourceObservationIssueModelExistDB(polSourceObservationIssueModelNew);
                        if (!string.IsNullOrWhiteSpace(polSourceObservationIssueModelRet.Error))
                        {
                            polSourceObservationIssueModelRet = polSourceObservationIssueService.PostAddPolSourceObservationIssueDB(polSourceObservationIssueModelNew);
                            if (!CheckModelOK<PolSourceObservationModel>(polSourceObservationModelRet)) return false;
                        }

                        // do historic PolSourceObservation

                        List<PCCSM.db_histo_operation> histoPolSourceList = new List<PCCSM.db_histo_operation>();
                        using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
                        {
                            histoPolSourceList = (from c in dbQC.geo_pollution_p
                                                  from h in dbQC.db_histo_operation
                                                  where c.id_geo_pollution_p == h.id_geo_pollution_p
                                                  && c.id_geo_pollution_p == pqc.id_geo_pollution_p
                                                  select h).ToList();

                        }

                        foreach (PCCSM.db_histo_operation hist in histoPolSourceList)
                        {
                            Application.DoEvents();

                            PolSourceObservationModel polSourceObservationModelNew2 = new PolSourceObservationModel();

                            if (hist.date_operation == null)
                            {
                                polSourceObservationModelNew2.ObservationDate_Local = new DateTime(1900, 1, 1);
                            }
                            else
                            {
                                polSourceObservationModelNew2.ObservationDate_Local = (DateTime)hist.date_operation;
                            }

                            observateur = "unknown";
                            if (hist.auteur != null)
                            {
                                if (hist.auteur.Length > 98)
                                {
                                    observateur = hist.auteur.Substring(0, 60);
                                }
                                else
                                {
                                    observateur = hist.auteur;
                                }
                            }

                            observation = "(vide)";
                            if (hist.description != null)
                            {
                                observation = hist.description;
                            }

                            TVTextInspectorEN = "Inspector " + observateur + " - (QC)";
                            TVTextInspectorFR = "Inspecteur " + observateur + " - (QC)";

                            tvItemModelContact = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, TVTextInspectorEN, TVTypeEnum.Contact);
                            if (!string.IsNullOrWhiteSpace(tvItemModelContact.Error))
                            {
                                tvItemModelContact = tvItemService.PostCreateTVItem(tvItemModelRoot.TVItemID, TVTextInspectorEN, TVTextInspectorFR, TVTypeEnum.Contact);
                                if (!CheckModelOK<TVItemModel>(tvItemModelContact)) return false;
                            }

                            polSourceObservationModelNew2.ContactTVItemID = tvItemModelContact.TVItemID;

                            polSourceObservationModelNew2.PolSourceSiteID = polSourceSiteModelRet.PolSourceSiteID;
                            polSourceObservationModelNew2.Observation_ToBeDeleted = observation;

                            PolSourceObservationModel polSourceObservationModelRet2 = polSourceObservationService.GetPolSourceObservationModelExistDB(polSourceObservationModelNew2);
                            if (!string.IsNullOrWhiteSpace(polSourceObservationModelRet2.Error))
                            {

                                polSourceObservationModelRet2 = polSourceObservationService.PostAddPolSourceObservationDB(polSourceObservationModelNew2);
                                if (!CheckModelOK<PolSourceObservationModel>(polSourceObservationModelRet2)) return false;
                            }

                            PolSourceObservationIssueModel polSourceObservationIssueModelNew2 = new PolSourceObservationIssueModel()
                            {
                                PolSourceObservationID = polSourceObservationModelNew2.PolSourceObservationID,
                                ObservationInfo = ObservationInfo,
                                PolSourceObsInfoList = polSourceObsInfoList,
                                Ordinal = 0,
                            };

                            PolSourceObservationIssueModel polSourceObservationIssueModelRet2 = polSourceObservationIssueService.GetPolSourceObservationIssueModelExistDB(polSourceObservationIssueModelNew2);
                            if (!string.IsNullOrWhiteSpace(polSourceObservationIssueModelRet2.Error))
                            {
                                polSourceObservationIssueModelRet2 = polSourceObservationIssueService.PostAddPolSourceObservationIssueDB(polSourceObservationIssueModelNew2);
                                if (!CheckModelOK<PolSourceObservationModel>(polSourceObservationModelRet2)) return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
        public bool CreateQCPolSourceNullAll()
        {
            lblStatus.Text = "Starting ... CreateSanitaryQC - CreateQCPolSourceNullAll";
            Application.DoEvents();

            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelQC = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelQC)) return false;

            List<TVItemModel> TVItemModelSubsectorList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
            if (TVItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelQC.TVText + "\r\n");
                return false;
            }

            List<MapInfo> mapInfoList = (from c in TVItemModelSubsectorList
                                         from m in tvItemServiceR.db.MapInfos
                                         where c.TVItemID == m.TVItemID
                                         && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon
                                         select m).ToList();

            List<TVItemSubsectorAndCoordCentroid> TVItemSubsectorAndCoordCentroidList = (from c in mapInfoList
                                                                                         let lat = (c.LatMax - c.LatMin) / 2 + c.LatMin
                                                                                         let lng = (c.LngMax - c.LngMin) / 2 + c.LngMin
                                                                                         select new TVItemSubsectorAndCoordCentroid
                                                                                         {
                                                                                             TVItemID = c.TVItemID,
                                                                                             Lat = (float)lat,
                                                                                             Lng = (float)lng,
                                                                                         }).ToList();

            List<string> sectorList = new List<string>();
            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {
                sectorList = (from s in dbQC.geo_pollution_p
                              select s.secteur).Distinct().ToList();
            }

            List<string> sectorOrderedList = (from c in sectorList
                                              orderby c
                                              select c).ToList();

            Application.DoEvents();

            List<PCCSM.geo_pollution_p> polQCList = new List<PCCSM.geo_pollution_p>();
            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {
                polQCList = (from c in dbQC.geo_pollution_p
                             where c.secteur == null
                             && c.id_geo_pollution_p != 0
                             && (c.x != null && c.y != null)
                             select c).ToList<PCCSM.geo_pollution_p>();
            }

            int TotalCount = polQCList.Count();
            int Count = 0;
            foreach (PCCSM.geo_pollution_p pqc in polQCList)
            {
                Count += 1;
                lblStatus.Text = (Count * 100 / TotalCount).ToString() + "... CreateSanitaryQC for sector " + pqc.secteur;
                Application.DoEvents();

                TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                PolSourceSiteService polSourceSiteService = new PolSourceSiteService(LanguageEnum.en, user);
                MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
                PolSourceObservationService polSourceObservationService = new PolSourceObservationService(LanguageEnum.en, user);
                PolSourceObservationIssueService polSourceObservationIssueService = new PolSourceObservationIssueService(LanguageEnum.en, user);

                PolSourceSiteModel polSourceSiteModelNew = new PolSourceSiteModel();

                string Code = tvItemService.CleanText(pqc.code.ToUpper());

                List<PolSourceObsInfoEnum> polSourceObsInfoList = GetPolSourceType(Code.Trim(), "", "QC");
                string ObservationInfo = (int)polSourceObsInfoList[0] + "," + (int)polSourceObsInfoList[1] + ",";
                polSourceSiteModelNew.IsPointSource = true;
                //if (pqc.status == "actif")
                //{
                //    polSourceSiteModelNew.IsActive = true;
                //}
                //else
                //{
                //    polSourceSiteModelNew.IsActive = false;
                //}
                polSourceSiteModelNew.InactiveReason = PolSourceInactiveReasonEnum.Error;
                polSourceSiteModelNew.Oldsiteid = pqc.id_geo_pollution_p;

                // Pollution Source Type
                string PolSourceSiteTVText = tvItemService.CleanText(Code + "      ".Substring(0, 6 - pqc.id_geo_pollution_p.ToString().Length) + pqc.id_geo_pollution_p.ToString());

                List<MapInfoModel> mapInfoModelListSubsector = mapInfoService.GetMapInfoModelWithLatAndLngInPolygonWithTVTypeDB((float)pqc.y, (float)pqc.x, TVTypeEnum.Subsector);
                int TempTVItemID = 0;
                if (mapInfoModelListSubsector.Count == 0)
                {
                    float SmallestDistance = 100000000f;
                    foreach (TVItemSubsectorAndCoordCentroid tvItemSubsectorAndCoordCentroid in TVItemSubsectorAndCoordCentroidList)
                    {
                        float TempDistance = (float)mapInfoService.CalculateDistance((double)pqc.y, (double)pqc.x, (double)tvItemSubsectorAndCoordCentroid.Lat, (double)tvItemSubsectorAndCoordCentroid.Lng, mapInfoService.R);
                        if (SmallestDistance > TempDistance)
                        {
                            TempTVItemID = tvItemSubsectorAndCoordCentroid.TVItemID;
                            SmallestDistance = TempDistance;
                        }
                    }
                }
                else
                {
                    TempTVItemID = mapInfoModelListSubsector[0].TVItemID;
                }

                if (TempTVItemID == 0)
                {
                    richTextBoxStatus.AppendText("TempTVItemID equal 0");
                    return false;
                }

                TVItemModel tvItemModelPolSourceSite = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(TempTVItemID, PolSourceSiteTVText, TVTypeEnum.PolSourceSite);
                if (!CheckModelOK<TVItemModel>(tvItemModelPolSourceSite))
                {
                    tvItemModelPolSourceSite = tvItemService.PostCreateTVItem(TempTVItemID, PolSourceSiteTVText, PolSourceSiteTVText, TVTypeEnum.PolSourceSite);
                    if (!CheckModelOK<TVItemModel>(tvItemModelPolSourceSite)) return false;
                };

                List<Coord> coordList = new List<Coord>()
                        {
                            new Coord()
                            {
                                Lat = (float)pqc.y,
                                Lng = (float)pqc.x,
                            }
                        };

                MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.PolSourceSite, tvItemModelPolSourceSite.TVItemID);
                if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                polSourceSiteModelNew.PolSourceSiteTVItemID = tvItemModelPolSourceSite.TVItemID;
                polSourceSiteModelNew.PolSourceSiteTVText = PolSourceSiteTVText;

                PolSourceSiteModel polSourceSiteModelRet = polSourceSiteService.GetPolSourceSiteModelWithPolSourceSiteTVItemIDDB(tvItemModelPolSourceSite.TVItemID);
                if (!string.IsNullOrWhiteSpace(polSourceSiteModelRet.Error))
                {
                    polSourceSiteModelRet = polSourceSiteService.PostAddPolSourceSiteDB(polSourceSiteModelNew);
                    if (!CheckModelOK<PolSourceSiteModel>(polSourceSiteModelRet)) return false;
                }

                string TVTextObservation = tvItemService.CleanText(string.IsNullOrEmpty(pqc.description) ? "" : pqc.description); ;

                PolSourceObservationModel polSourceObservationModelNew = new PolSourceObservationModel();
                if (pqc.date_observation == null)
                {
                    polSourceObservationModelNew.ObservationDate_Local = new DateTime(1970, 1, 1);
                }
                else
                {
                    polSourceObservationModelNew.ObservationDate_Local = (DateTime)(pqc.date_observation.Value).AddHours(1);
                }
                string Observateur = "";
                if (pqc.observateur != null)
                {
                    if (pqc.observateur.Length > 98)
                    {
                        Observateur = pqc.observateur.Substring(0, 60);
                    }
                    else
                    {
                        Observateur = pqc.observateur;
                    }
                }

                string TVText = "Inspector " + Observateur;
                TVText = (TVText.Length > 50 ? TVText.Substring(0, 50) : TVText);
                TVItemModel tvItemModelContact = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, TVText, TVTypeEnum.Contact);
                if (!string.IsNullOrWhiteSpace(tvItemModelContact.Error))
                {
                    tvItemModelContact = tvItemService.PostCreateTVItem(tvItemModelRoot.TVItemID, TVText, TVText, TVTypeEnum.Contact);
                    if (!CheckModelOK<TVItemModel>(tvItemModelContact)) return false;
                }

                polSourceObservationModelNew.ContactTVItemID = tvItemModelContact.TVItemID;
                polSourceObservationModelNew.Observation_ToBeDeleted = TVTextObservation;
                polSourceObservationModelNew.PolSourceSiteID = polSourceSiteModelRet.PolSourceSiteID;

                PolSourceObservationModel polSourceObservationModelRet = polSourceObservationService.GetPolSourceObservationModelExistDB(polSourceObservationModelNew);
                if (!string.IsNullOrWhiteSpace(polSourceObservationModelRet.Error))
                {
                    PolSourceObservationModel polSourceObservationModel = polSourceObservationService.PostAddPolSourceObservationDB(polSourceObservationModelNew);
                    if (!CheckModelOK<PolSourceObservationModel>(polSourceObservationModel)) return false;
                }

                PolSourceObservationIssueModel polSourceObservationIssueModelNew = new PolSourceObservationIssueModel()
                {
                    PolSourceObservationID = polSourceObservationModelNew.PolSourceObservationID,
                    ObservationInfo = ObservationInfo,
                    PolSourceObsInfoList = polSourceObsInfoList,
                    Ordinal = 0,
                };

                PolSourceObservationIssueModel polSourceObservationIssueModelRet = polSourceObservationIssueService.GetPolSourceObservationIssueModelExistDB(polSourceObservationIssueModelNew);
                if (!string.IsNullOrWhiteSpace(polSourceObservationIssueModelRet.Error))
                {
                    polSourceObservationIssueModelRet = polSourceObservationIssueService.PostAddPolSourceObservationIssueDB(polSourceObservationIssueModelNew);
                    if (!CheckModelOK<PolSourceObservationModel>(polSourceObservationModelRet)) return false;
                }

                // do historic PolSourceObservation

                List<PCCSM.db_histo_operation> histoPolSourceList = new List<PCCSM.db_histo_operation>();
                using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
                {
                    histoPolSourceList = (from c in dbQC.geo_pollution_p
                                          from h in dbQC.db_histo_operation
                                          where c.id_geo_pollution_p == h.id_geo_pollution_p
                                          && c.id_geo_pollution_p == pqc.id_geo_pollution_p
                                          select h).ToList();
                }

                foreach (PCCSM.db_histo_operation hist in histoPolSourceList)
                {
                    Application.DoEvents();

                    PolSourceObservationModel polSourceObservationModelNew2 = new PolSourceObservationModel();
                    if (hist.date_operation == null)
                    {
                        polSourceObservationModelNew2.ObservationDate_Local = new DateTime(1970, 1, 1);
                    }
                    else
                    {
                        polSourceObservationModelNew2.ObservationDate_Local = (DateTime)hist.date_operation;
                    }
                    if (hist.auteur != null)
                    {
                        if (hist.auteur.Length > 98)
                        {
                            Observateur = hist.auteur.Substring(0, 60);
                        }
                        else
                        {
                            Observateur = hist.auteur;
                        }
                    }

                    TVText = "Inspector " + Observateur;
                    TVText = (TVText.Length > 50 ? TVText.Substring(0, 50) : TVText);
                    TVItemModel tvItemModelContact2 = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, TVText, TVTypeEnum.Contact);
                    if (!string.IsNullOrWhiteSpace(tvItemModelContact.Error))
                    {
                        tvItemModelContact = tvItemService.PostCreateTVItem(tvItemModelRoot.TVItemID, TVText, TVText, TVTypeEnum.Contact);
                        if (!CheckModelOK<TVItemModel>(tvItemModelContact)) return false;
                    }

                    polSourceObservationModelNew2.ContactTVItemID = tvItemModelContact2.TVItemID;

                    polSourceObservationModelNew2.Observation_ToBeDeleted = TVTextObservation;
                    polSourceObservationModelNew2.PolSourceSiteID = polSourceSiteModelRet.PolSourceSiteID;

                    PolSourceObservationModel polSourceObservationModelRet2 = polSourceObservationService.GetPolSourceObservationModelExistDB(polSourceObservationModelNew2);
                    if (!string.IsNullOrWhiteSpace(polSourceObservationModelRet2.Error))
                    {
                        polSourceObservationModelRet2 = polSourceObservationService.PostAddPolSourceObservationDB(polSourceObservationModelNew2);
                        if (!CheckModelOK<PolSourceObservationModel>(polSourceObservationModelRet2)) return false;
                    }

                    PolSourceObservationIssueModel polSourceObservationIssueModelNew2 = new PolSourceObservationIssueModel()
                    {
                        PolSourceObservationID = polSourceObservationModelNew2.PolSourceObservationID,
                        ObservationInfo = ObservationInfo,
                        PolSourceObsInfoList = polSourceObsInfoList,
                        Ordinal = 0,
                    };

                    PolSourceObservationIssueModel polSourceObservationIssueModelRet2 = polSourceObservationIssueService.GetPolSourceObservationIssueModelExistDB(polSourceObservationIssueModelNew2);
                    if (!string.IsNullOrWhiteSpace(polSourceObservationIssueModelRet2.Error))
                    {
                        polSourceObservationIssueModelRet2 = polSourceObservationIssueService.PostAddPolSourceObservationIssueDB(polSourceObservationIssueModelNew2);
                        if (!CheckModelOK<PolSourceObservationModel>(polSourceObservationModelRet2)) return false;
                    }
                }
            }

            return true;
        }
    }
}
