using CSSPEnumsDLL.Enums;
using CSSPEnumsDLL.Services;
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

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CreateSanitaryBC()
        {
            lblStatus.Text = "Starting ... CreateSanitaryBC";
            Application.DoEvents();

            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);
            BaseEnumService _BaseEnumSerice = new BaseEnumService(LanguageEnum.en);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelBC = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "British Columbia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelBC)) return false;

            List<TVItemModel> TVItemModelSubsectorList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelBC.TVItemID, TVTypeEnum.Subsector);
            if (TVItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelBC.TVText + "\r\n");
                return false;
            }

            List<TempData.BCPolSource> bcPolSourceList = new List<TempData.BCPolSource>();

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                bcPolSourceList = (from c in dbDT.BCPolSources
                                   orderby c.BCPolSourceID
                                   select c).ToList<TempData.BCPolSource>();
            }

            int StartBCCreateSanitarysBC = int.Parse(textBoxBCCreateSanitaryBC.Text);

            int TotalCount = bcPolSourceList.Count();
            int Count = 0;
            foreach (TempData.BCPolSource bcps in bcPolSourceList)
            {
                if (Cancel) return false;

                Count += 1;
                lblStatus.Text = Count + " of " + TotalCount + " ... LoadSanitary BC";
                Application.DoEvents();

                textBoxBCCreateSanitaryBC.Text = Count.ToString();

                if (StartBCCreateSanitarysBC > Count)
                {
                    continue;
                }

                TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                PolSourceObservationService polSourceObservationService = new PolSourceObservationService(LanguageEnum.en, user);
                PolSourceObservationIssueService polSourceObservationIssueService = new PolSourceObservationIssueService(LanguageEnum.en, user);
                PolSourceSiteService polSourceSiteService = new PolSourceSiteService(LanguageEnum.en, user);
                MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

                PolSourceSiteModel polSourceSiteModelNew = new PolSourceSiteModel();

                polSourceSiteModelNew.Oldsiteid = int.Parse(bcps.Key_.Substring(2));

                string LCODE = tvItemService.CleanText(bcps.LCode);
                string ICODE = tvItemService.CleanText(bcps.ICode);

                if (LCODE.Trim() == "")
                {
                    LCODE = "--";
                }

                if (ICODE.Trim() == "")
                {
                    ICODE = "--";
                }

                List<PolSourceObsInfoEnum> polSourceObsInfoList = GetPolSourceType(LCODE.Trim(), ICODE.Trim(), "BC");

                string ObservationInfo = ((int)polSourceObsInfoList[0]).ToString() + "," + ((int)polSourceObsInfoList[1]).ToString() + ",";

                List<MapInfoModel> mapInfoModelListSubsector = new List<MapInfoModel>();
                if (bcps.Y_calc == 48.984828948974609f && bcps.X_calc == -123.02297973632812f)
                {
                    mapInfoModelListSubsector = mapInfoService.GetMapInfoModelWithLatAndLngInPolygonWithTVTypeDB(49.01f, (float)bcps.X_calc, TVTypeEnum.Subsector);
                }
                else if (bcps.Y_calc == 49.3099250793457f && bcps.X_calc == -121.77302551269531f)
                {
                    mapInfoModelListSubsector = mapInfoService.GetMapInfoModelWithLatAndLngInPolygonWithTVTypeDB((float)bcps.Y_calc, (float)-122.2, TVTypeEnum.Subsector);
                }
                else if (bcps.Y_calc == 48.3582878112793f && bcps.X_calc == -123.73906707763672f)
                {
                    mapInfoModelListSubsector = mapInfoService.GetMapInfoModelWithLatAndLngInPolygonWithTVTypeDB((float)bcps.Y_calc, (float)-123.72, TVTypeEnum.Subsector);
                }
                else if (bcps.Y_calc == 51.266487121582031f && bcps.X_calc == -128.20162963867187f)
                {
                    mapInfoModelListSubsector = mapInfoService.GetMapInfoModelWithLatAndLngInPolygonWithTVTypeDB((float)bcps.Y_calc, (float)-128, TVTypeEnum.Subsector);
                }
                else if (bcps.Y_calc == 55.91217041015625f && bcps.X_calc == -130.01992797851562f)
                {
                    mapInfoModelListSubsector = mapInfoService.GetMapInfoModelWithLatAndLngInPolygonWithTVTypeDB((float)bcps.Y_calc, (float) -129, TVTypeEnum.Subsector);
                }
                else if (bcps.Y_calc == 55.912052154541016f && bcps.X_calc == -130.01669311523437f)
                {
                    mapInfoModelListSubsector = mapInfoService.GetMapInfoModelWithLatAndLngInPolygonWithTVTypeDB((float)bcps.Y_calc, (float)-129, TVTypeEnum.Subsector);
                }
                else
                {
                    mapInfoModelListSubsector = mapInfoService.GetMapInfoModelWithLatAndLngInPolygonWithTVTypeDB((float)bcps.Y_calc, (float)bcps.X_calc, TVTypeEnum.Subsector);
                }

                if (mapInfoModelListSubsector.Count == 0)
                {
                    richTextBoxStatus.AppendText("GetMapInfoModelWithLatAndLngInPolygonWithTVTypeDB did not find anything\r\n");
                    return false;
                }

                // doing EN TVText
                string PolSourceSiteTVTextEN = _BaseEnumSerice.GetEnumText_PolSourceObsInfoEnum(polSourceObsInfoList[0]);
                if (polSourceObsInfoList.Count > 1)
                {
                    if (polSourceObsInfoList[1] != PolSourceObsInfoEnum.Error)
                    {
                        PolSourceSiteTVTextEN += " - " + _BaseEnumSerice.GetEnumText_PolSourceObsInfoEnum(polSourceObsInfoList[1]);
                    }
                }

                int Site = (int)bcps.OBJECTID;

                PolSourceSiteTVTextEN = tvItemService.CleanText(PolSourceSiteTVTextEN + " - " + "      ".Substring(0, 6 - Site.ToString().Length) + Site.ToString());

                Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-CA");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-CA");

                // Doing FR TVText
                PolSourceObservationService polSourceObservationService2 = new PolSourceObservationService(LanguageEnum.fr, user);

                string PolSourceSiteTVTextFR = _BaseEnumSerice.GetEnumText_PolSourceObsInfoEnum(polSourceObsInfoList[0]);
                if (polSourceObsInfoList.Count > 1)
                {
                    if (polSourceObsInfoList[1] != PolSourceObsInfoEnum.Error)
                    {
                        PolSourceSiteTVTextFR += " - " + _BaseEnumSerice.GetEnumText_PolSourceObsInfoEnum(polSourceObsInfoList[1]);
                    }
                }

                PolSourceSiteTVTextFR = tvItemService.CleanText(PolSourceSiteTVTextFR + " - " + "      ".Substring(0, 6 - Site.ToString().Length) + Site.ToString());

                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-CA");

                TVItemModel tvItemModelPolSourceSite = tvItemService.PostCreateTVItem(mapInfoModelListSubsector[0].TVItemID, PolSourceSiteTVTextEN, PolSourceSiteTVTextFR, TVTypeEnum.PolSourceSite);
                if (!CheckModelOK<TVItemModel>(tvItemModelPolSourceSite)) return false;

                List<Coord> coordList = new List<Coord>() 
                        {
                            new Coord()
                            {
                                Lat = (float)bcps.Y_calc,
                                Lng = (float)bcps.X_calc,
                            }
                        };

                MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.PolSourceSite, tvItemModelPolSourceSite.TVItemID);
                if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                polSourceSiteModelNew.PolSourceSiteTVItemID = tvItemModelPolSourceSite.TVItemID;
                polSourceSiteModelNew.PolSourceSiteTVText = PolSourceSiteTVTextEN;
                polSourceSiteModelNew.Site = Site;
                polSourceSiteModelNew.Oldsiteid = Site;
                        
                PolSourceSiteModel polSourceSiteModelRet = polSourceSiteService.GetPolSourceSiteModelWithPolSourceSiteTVItemIDDB(tvItemModelPolSourceSite.TVItemID);
                if (!string.IsNullOrWhiteSpace(polSourceSiteModelRet.Error))
                {
                    polSourceSiteModelRet = polSourceSiteService.PostAddPolSourceSiteDB(polSourceSiteModelNew);
                    if (!CheckModelOK<PolSourceSiteModel>(polSourceSiteModelRet)) return false;
                }

                PolSourceObservationModel polSourceObservationModelNew = new PolSourceObservationModel();
                if (bcps.yyyymmdd == null)
                {
                    polSourceObservationModelNew.ObservationDate_Local = new DateTime(2050, 1, 1);
                }
                else
                {
                    polSourceObservationModelNew.ObservationDate_Local = (DateTime)bcps.yyyymmdd;
                }

                string Inspector = "unknown";
                if (!string.IsNullOrWhiteSpace(bcps.Verified))
                {
                    Inspector = bcps.Verified + " - (BC)";
                }

                string TVTextInspectorEN = "Inspector " + Inspector;
                string TVTextInspectorFR = "Inspecteur " + Inspector;

                TVItemModel tvItemModelContactRet = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, TVTextInspectorEN, TVTypeEnum.Contact);
                if (!string.IsNullOrWhiteSpace(tvItemModelContactRet.Error))
                {
                    tvItemModelContactRet = tvItemService.PostCreateTVItem(tvItemModelRoot.TVItemID, TVTextInspectorEN, TVTextInspectorFR, TVTypeEnum.Contact);
                    if (!CheckModelOK<TVItemModel>(tvItemModelContactRet)) return false;
                }

                polSourceObservationModelNew.ContactTVItemID = tvItemModelContactRet.TVItemID;
                string Observation = "(empty)";
                if (!string.IsNullOrWhiteSpace(bcps.Remarks))
                {
                    Observation = LCODE.Trim().ToUpper() + " - " + ICODE.Trim().ToUpper() + " - " + bcps.Remarks;
                }
                polSourceObservationModelNew.Observation_ToBeDeleted = Observation;
                polSourceObservationModelNew.PolSourceSiteID = polSourceSiteModelRet.PolSourceSiteID;

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

            }

            return true;

        }
    }
}
