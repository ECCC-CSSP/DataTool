using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using CSSPEnumsDLL.Enums;
using CSSPDBDLL;
using CSSPModelsDLL.Models;
using CSSPDBDLL.Services;
using CSSPEnumsDLL.Services;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CreateSanitaryAtl(string Prov, string ShortProv, string DonPatDavJoeDavC)
        {
            lblStatus.Text = "Starting ... CreateSanitaryAll " + DonPatDavJoeDavC + " for " + Prov;
            Application.DoEvents();

            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);
            BaseEnumService _BaseEnumService = new BaseEnumService(LanguageEnum.en);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelProv = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, Prov, TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return false;

            List<TVItemModel> TVItemModelSubsectorList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            if (TVItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelProv.TVText + "\r\n");
                return false;
            }

            List<string> DPD = new List<string>() { "Don", "Pat", "Dav", "DavC", "Joe" };
            if (!DPD.Contains(DonPatDavJoeDavC))
            {
                richTextBoxStatus.AppendText("DonPatDav does not contain 'Don' 'Pat' 'Dav' or 'DavC' in LoadSanitaryAll\r\n");
                return false;
            }

            int StartNBDonCreateSanitarysAtl = int.Parse(textBoxNBDonCreateSanitaryAtl.Text);
            int StartNBPatCreateSanitarysAtl = int.Parse(textBoxNBPatCreateSanitaryAtl.Text);
            int StartNBDavCreateSanitarysAtl = int.Parse(textBoxNBJoeCreateSanitaryAtl.Text);
            int StartNLDonCreateSanitarysAtl = int.Parse(textBoxNLDonCreateSanitaryAtl.Text);
            int StartNLDavCCreateSanitarysAtl = int.Parse(textBoxNLDavCCreateSanitaryAtl.Text);
            int StartNSDonCreateSanitarysAtl = int.Parse(textBoxNSDonCreateSanitaryAtl.Text);
            int StartNSDavCreateSanitarysAtl = int.Parse(textBoxNSDavCreateSanitaryAtl.Text);
            int StartPEDonCreateSanitarysAtl = int.Parse(textBoxPEDonCreateSanitaryAtl.Text);
            int StartPEDavCreateSanitarysAtl = int.Parse(textBoxPEDavCreateSanitaryAtl.Text);

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                List<SanitarySite> sanitarySiteList = new List<SanitarySite>();
                if (DonPatDavJoeDavC == "Don")
                {
                    sanitarySiteList = (from c in dbDT.SanitaryDonSites
                                        let Locator = c.Locator
                                        where Locator.Substring(0, 2) == ShortProv
                                        orderby c.SanitaryDonSiteID
                                        select new SanitarySite
                                        {
                                            Datum = c.Datum,
                                            easting = c.easting,
                                            latitude = c.latitude,
                                            Locator = c.Locator,
                                            longitude = c.longitude,
                                            northing = c.northing,
                                            SanitarySiteID = c.SanitaryDonSiteID,
                                            Site = c.Site,
                                            siteid = c.siteid,
                                            Zone = c.Zone,
                                        }).ToList<SanitarySite>();
                }
                else if (DonPatDavJoeDavC == "Pat")
                {
                    sanitarySiteList = (from c in dbDT.SanitaryPatSites
                                        let Locator = c.Locator
                                        where Locator.Substring(0, 2) == ShortProv
                                        orderby c.SanitaryPatSiteID
                                        select new SanitarySite
                                        {
                                            Datum = c.Datum,
                                            easting = c.easting,
                                            latitude = c.latitude,
                                            Locator = c.Locator,
                                            longitude = c.longitude,
                                            northing = c.northing,
                                            SanitarySiteID = c.SanitaryPatSiteID,
                                            Site = c.Site,
                                            siteid = c.siteid,
                                            Zone = c.Zone,
                                        }).ToList<SanitarySite>();
                }
                else if (DonPatDavJoeDavC == "Joe")
                {
                    sanitarySiteList = (from c in dbDT.SanitaryJoeSites
                                        let Locator = c.Locator
                                        where Locator.Substring(0, 2) == ShortProv
                                        orderby c.SanitaryJoeSiteID
                                        select new SanitarySite
                                        {
                                            Datum = c.Datum,
                                            easting = c.easting,
                                            latitude = c.latitude,
                                            Locator = c.Locator,
                                            longitude = c.longitude,
                                            northing = c.northing,
                                            SanitarySiteID = c.SanitaryJoeSiteID,
                                            Site = c.Site,
                                            siteid = c.siteid,
                                            Zone = c.Zone,
                                        }).ToList<SanitarySite>();
                }
                else if (DonPatDavJoeDavC == "Dav")
                {
                    sanitarySiteList = (from c in dbDT.SanitaryDavSites
                                        let Locator = c.Locator
                                        where Locator.Substring(0, 2) == ShortProv
                                        orderby c.SanitaryDavSiteID
                                        select new SanitarySite
                                        {
                                            Datum = c.Datum,
                                            easting = c.easting,
                                            latitude = c.latitude,
                                            Locator = c.Locator,
                                            longitude = c.longitude,
                                            northing = c.northing,
                                            SanitarySiteID = c.SanitaryDavSiteID,
                                            Site = c.Site,
                                            siteid = c.siteid,
                                            Zone = c.Zone,
                                        }).ToList<SanitarySite>();
                }
                else if (DonPatDavJoeDavC == "DavC")
                {
                    sanitarySiteList = (from c in dbDT.SanitaryDavCSites
                                        let Locator = c.Locator
                                        where Locator.Substring(0, 2) == ShortProv
                                        orderby c.SanitaryDavCSiteID
                                        select new SanitarySite
                                        {
                                            Datum = c.Datum,
                                            easting = c.easting,
                                            latitude = c.latitude,
                                            Locator = c.Locator,
                                            longitude = c.longitude,
                                            northing = c.northing,
                                            SanitarySiteID = c.SanitaryDavCSiteID,
                                            Site = c.Site,
                                            siteid = c.siteid,
                                            Zone = c.Zone,
                                        }).ToList<SanitarySite>();
                }
                else
                {
                    richTextBoxStatus.AppendText("DonPatDat is not one of 'Don', 'Pat', 'Dav', 'DavC', 'Joe'\r\n");
                    return false;
                }

                int TotalCount = sanitarySiteList.Count();
                int Count = 0;

                foreach (SanitarySite ss in sanitarySiteList)
                {
                    if (Cancel) return false;

                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                    PolSourceSiteService polSourceSiteService = new PolSourceSiteService(LanguageEnum.en, user);
                    MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
                    PolSourceObservationService polSourceObservationService = new PolSourceObservationService(LanguageEnum.en, user);
                    PolSourceObservationIssueService polSourceObservationIssueService = new PolSourceObservationIssueService(LanguageEnum.en, user);

                    Count += 1;
                    lblStatus.Text = (Count * 100 / TotalCount).ToString() + " ... CreateSanitaryAll " + DonPatDavJoeDavC + " for " + Prov;
                    lblStatus2.Text = Count + " of " + TotalCount;
                    Application.DoEvents();

                    switch (ShortProv)
                    {
                        case "NB":
                            {
                                switch (DonPatDavJoeDavC)
                                {
                                    case "Don":
                                        {
                                            textBoxNBDonCreateSanitaryAtl.Text = Count.ToString();
                                            if (StartNBDonCreateSanitarysAtl > Count)
                                            {
                                                continue;
                                            }
                                        }
                                        break;
                                    case "Pat":
                                        {
                                            textBoxNBPatCreateSanitaryAtl.Text = Count.ToString();
                                            if (StartNBPatCreateSanitarysAtl > Count)
                                            {
                                                continue;
                                            }
                                        }
                                        break;
                                    case "Joe":
                                        {
                                            textBoxNBJoeCreateSanitaryAtl.Text = Count.ToString();
                                            if (StartNBDavCreateSanitarysAtl > Count)
                                            {
                                                continue;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case "NL":
                            {
                                switch (DonPatDavJoeDavC)
                                {
                                    case "Don":
                                        {
                                            textBoxNLDonCreateSanitaryAtl.Text = Count.ToString();
                                            if (StartNLDonCreateSanitarysAtl > Count)
                                            {
                                                continue;
                                            }
                                        }
                                        break;
                                    case "DavC":
                                        {
                                            textBoxNLDavCCreateSanitaryAtl.Text = Count.ToString();
                                            if (StartNLDavCCreateSanitarysAtl > Count)
                                            {
                                                continue;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case "NS":
                            {
                                switch (DonPatDavJoeDavC)
                                {
                                    case "Don":
                                        {
                                            textBoxNSDonCreateSanitaryAtl.Text = Count.ToString();
                                            if (StartNSDonCreateSanitarysAtl > Count)
                                            {
                                                continue;
                                            }
                                        }
                                        break;
                                    case "Dav":
                                        {
                                            textBoxNSDavCreateSanitaryAtl.Text = Count.ToString();
                                            if (StartNSDavCreateSanitarysAtl > Count)
                                            {
                                                continue;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case "PE":
                            {
                                switch (DonPatDavJoeDavC)
                                {
                                    case "Don":
                                        {
                                            textBoxPEDonCreateSanitaryAtl.Text = Count.ToString();
                                            if (StartPEDonCreateSanitarysAtl > Count)
                                            {
                                                continue;
                                            }
                                        }
                                        break;
                                    case "Dav":
                                        {
                                            textBoxPEDavCreateSanitaryAtl.Text = Count.ToString();
                                            if (StartPEDavCreateSanitarysAtl > Count)
                                            {
                                                continue;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    string TVText = ss.Locator;
                    TVItemModel tvItemModelSubsector = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelProv.TVItemID, TVText, TVTypeEnum.Subsector);
                    if (!CheckModelOK<TVItemModel>(tvItemModelSubsector)) return false;

                    List<SanitaryObsSite> sanitaryObsList = new List<SanitaryObsSite>();
                    if (DonPatDavJoeDavC == "Don")
                    {
                        sanitaryObsList = (from c in dbDT.SanitaryDonObs
                                           where c.SanitaryDonSiteID == ss.SanitarySiteID
                                           orderby c.ObsDate descending
                                           select new SanitaryObsSite
                                           {
                                               Active = c.Active,
                                               Name_Inspector = c.Name_Inspector,
                                               ObsDate = c.ObsDate,
                                               Observations = c.Observations,
                                               Risk_Assessment = c.Risk_Assessment,
                                               SanitaryObsID = c.SanitaryDonObsID,
                                               SanitarySiteID = c.SanitaryDonSiteID,
                                               siteid = c.siteid,
                                               Status = c.Status,
                                               Type = c.Type
                                           }).ToList<SanitaryObsSite>();
                    }
                    else if (DonPatDavJoeDavC == "Pat")
                    {
                        sanitaryObsList = (from c in dbDT.SanitaryPatObs
                                           where c.SanitaryPatSiteID == ss.SanitarySiteID
                                           orderby c.ObsDate descending
                                           select new SanitaryObsSite
                                           {
                                               Active = c.Active,
                                               Name_Inspector = c.Name_Inspector,
                                               ObsDate = c.ObsDate,
                                               Observations = c.Observations,
                                               Risk_Assessment = c.Risk_Assessment,
                                               SanitaryObsID = c.SanitaryPatObsID,
                                               SanitarySiteID = c.SanitaryPatSiteID,
                                               siteid = c.siteid,
                                               Status = c.Status,
                                               Type = c.Type
                                           }).ToList<SanitaryObsSite>();
                    }
                    else if (DonPatDavJoeDavC == "Joe")
                    {
                        sanitaryObsList = (from c in dbDT.SanitaryJoeObs
                                           where c.SanitaryJoeSiteID == ss.SanitarySiteID
                                           orderby c.ObsDate descending
                                           select new SanitaryObsSite
                                           {
                                               Active = c.Active,
                                               Name_Inspector = c.Name_Inspector,
                                               ObsDate = c.ObsDate,
                                               Observations = c.Observations,
                                               Risk_Assessment = c.Risk_Assessment,
                                               SanitaryObsID = c.SanitaryJoeObsID,
                                               SanitarySiteID = c.SanitaryJoeSiteID,
                                               siteid = c.siteid,
                                               Status = c.Status,
                                               Type = c.Type
                                           }).ToList<SanitaryObsSite>();
                    }
                    else if (DonPatDavJoeDavC == "Dav")
                    {
                        sanitaryObsList = (from c in dbDT.SanitaryDavObs
                                           where c.SanitaryDavSiteID == ss.SanitarySiteID
                                           orderby c.ObsDate descending
                                           select new SanitaryObsSite
                                           {
                                               Active = c.Active,
                                               Name_Inspector = c.Name_Inspector,
                                               ObsDate = c.ObsDate,
                                               Observations = c.Observations,
                                               Risk_Assessment = c.Risk_Assessment,
                                               SanitaryObsID = c.SanitaryDavObsID,
                                               SanitarySiteID = c.SanitaryDavSiteID,
                                               siteid = c.siteid,
                                               Status = c.Status,
                                               Type = c.Type
                                           }).ToList<SanitaryObsSite>();
                    }
                    else if (DonPatDavJoeDavC == "DavC")
                    {
                        sanitaryObsList = (from c in dbDT.SanitaryDavCObs
                                           where c.SanitaryDavCSiteID == ss.SanitarySiteID
                                           orderby c.ObsDate descending
                                           select new SanitaryObsSite
                                           {
                                               Active = c.Active,
                                               Name_Inspector = c.Name_Inspector,
                                               ObsDate = c.ObsDate,
                                               Observations = c.Observations,
                                               Risk_Assessment = c.Risk_Assessment,
                                               SanitaryObsID = c.SanitaryDavCObsID,
                                               SanitarySiteID = c.SanitaryDavCSiteID,
                                               siteid = c.siteid,
                                               Status = c.Status,
                                               Type = c.Type
                                           }).ToList<SanitaryObsSite>();
                    }
                    else
                    {
                        richTextBoxStatus.AppendText("DonPatDat is not one of 'Don', 'Pat', 'Dav', 'DavC', 'Joe'\r\n");
                        return false;
                    }

                    List<PolSourceObsInfoEnum> polSourceObsInfoList = new List<PolSourceObsInfoEnum>() { PolSourceObsInfoEnum.Error };
                    string ObservationInfo = ((int)PolSourceObsInfoEnum.Error).ToString();
                    if (sanitaryObsList.Count > 0)
                    {
                        if (string.IsNullOrWhiteSpace(sanitaryObsList.FirstOrDefault().Type))
                        {
                            if (sanitaryObsList.Count > 1)
                            {
                                polSourceObsInfoList = GetPolSourceType(sanitaryObsList[1].Type.Substring(0, 2), "--", "AT");
                            }
                        }
                        else
                        {
                            polSourceObsInfoList = GetPolSourceType(sanitaryObsList.FirstOrDefault().Type.Substring(0, 2), "--", "AT");
                        }
                        ObservationInfo = ((int)polSourceObsInfoList[0]).ToString() + "," + ((int)polSourceObsInfoList[1]).ToString() + ",";
                    }

                    // doing EN TVText
                    string PolSourceSiteTVTextEN = _BaseEnumService.GetEnumText_PolSourceObsInfoEnum(polSourceObsInfoList[0]);
                    if (polSourceObsInfoList.Count > 1)
                    {
                        if (polSourceObsInfoList[1] != PolSourceObsInfoEnum.Error)
                        {
                            PolSourceSiteTVTextEN += " - " + _BaseEnumService.GetEnumText_PolSourceObsInfoEnum(polSourceObsInfoList[1]);
                        }
                    }

                    PolSourceSiteTVTextEN = tvItemService.CleanText(PolSourceSiteTVTextEN + " - " + "      ".Substring(0, 6 - ss.Site.ToString().Length) + ss.Site);

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

                    PolSourceSiteTVTextFR = tvItemService.CleanText(PolSourceSiteTVTextFR + " - " + "      ".Substring(0, 6 - ss.Site.ToString().Length) + ss.Site);

                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-CA");

                    PolSourceSiteModel polSourceSiteModelNew = new PolSourceSiteModel();


                    TVItemModel tvItemModelPolSourceSiteRet = tvItemService.PostCreateTVItem(tvItemModelSubsector.TVItemID, PolSourceSiteTVTextEN, PolSourceSiteTVTextFR, TVTypeEnum.PolSourceSite);
                    if (!CheckModelOK<TVItemModel>(tvItemModelPolSourceSiteRet)) return false;

                    PolSourceSiteModel polSourceSiteModelRet = polSourceSiteService.GetPolSourceSiteModelWithPolSourceSiteTVItemIDDB(tvItemModelPolSourceSiteRet.TVItemID);
                    if (!string.IsNullOrWhiteSpace(polSourceSiteModelRet.Error))
                    {
                        polSourceSiteModelNew.Oldsiteid = ss.siteid;
                        polSourceSiteModelNew.Site = ss.Site; 
                        polSourceSiteModelNew.Temp_Locator_CanDelete = ss.Locator;
                        //polSourceSiteModelNew.IsActive = true;
                        if (sanitaryObsList.Count > 0)
                        {
                            //polSourceSiteModelNew.IsActive = (bool)sanitaryObsList.FirstOrDefault().Active;
                        } 
                        polSourceSiteModelNew.SiteID = ss.siteid;
                        polSourceSiteModelNew.PolSourceSiteTVItemID = tvItemModelPolSourceSiteRet.TVItemID;
                        polSourceSiteModelNew.PolSourceSiteTVText = PolSourceSiteTVTextEN; // not used but don't remove for now
                        polSourceSiteModelNew.InactiveReason = PolSourceInactiveReasonEnum.Error;

                        polSourceSiteModelRet = polSourceSiteService.PostAddPolSourceSiteDB(polSourceSiteModelNew);
                        if (!CheckModelOK<PolSourceSiteModel>(polSourceSiteModelRet)) return false;

                        List<Coord> coordList = new List<Coord>() 
                        {
                            new Coord()
                            {
                                Lat = (float)ss.latitude,
                                Lng = (float)ss.longitude,
                            }
                        };

                        MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.PolSourceSite, tvItemModelPolSourceSiteRet.TVItemID);
                        if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;
                    }

                    foreach (SanitaryObsSite sdo in sanitaryObsList)
                    {
                        if (Cancel) return false;

                        if (string.IsNullOrEmpty(sdo.Observations))
                        {
                            sdo.Observations = "(empty)";
                        }
                        string TVTextObs = tvItemService.CleanText(sdo.Type + " " + sdo.Observations);

                        PolSourceObservationModel polSourceObservationModelNew = new PolSourceObservationModel();
                        polSourceObservationModelNew.ObservationDate_Local = (DateTime)sdo.ObsDate;
                        if (((DateTime)polSourceObservationModelNew.ObservationDate_Local).Year == 209)
                        {
                            polSourceObservationModelNew.ObservationDate_Local = new DateTime(2009, ((DateTime)polSourceObservationModelNew.ObservationDate_Local).Month, ((DateTime)polSourceObservationModelNew.ObservationDate_Local).Day);
                        }

                        string TVTextInspectorEN = "Inspector " + (sdo.Name_Inspector != null && sdo.Name_Inspector.Length > 0 ? sdo.Name_Inspector : "unknown");
                        string TVTextInspectorFR = "Inspecteur " + (sdo.Name_Inspector != null && sdo.Name_Inspector.Length > 0 ? sdo.Name_Inspector : "inconnue");
                        TVItemModel tvItemModelRet = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, TVTextInspectorEN, TVTypeEnum.Contact);
                        if (!string.IsNullOrWhiteSpace(tvItemModelRet.Error))
                        {
                            tvItemModelRet = tvItemService.PostCreateTVItem(tvItemModelRoot.TVItemID, TVTextInspectorEN, TVTextInspectorFR, TVTypeEnum.Contact);
                            if (!CheckModelOK<TVItemModel>(tvItemModelRet)) return false;
                        }

                        polSourceObservationModelNew.Observation_ToBeDeleted = TVTextObs; 

                        polSourceObservationModelNew.ContactTVItemID = tvItemModelRet.TVItemID;
                        polSourceObservationModelNew.PolSourceSiteID = polSourceSiteModelRet.PolSourceSiteID;

                        polSourceObsInfoList = GetPolSourceType(sdo.Type.Trim(), "--", "AT");
                        ObservationInfo = (int)polSourceObsInfoList[0] + "," + (int)polSourceObsInfoList[1] + ",";

                        PolSourceObservation polSourceObservationExist = polSourceObservationService.GetPolSourceObservationExistDB(polSourceObservationModelNew);
                        if (polSourceObservationExist != null)
                        {
                            // already has the PolSourceObservation for this date.
                            continue;
                        }

                        if (sdo.Status == null)
                        {
                            sdo.Status = "P";
                        }
                        PolSourceObsInfoEnum polSourceObsInfo = GetPolSourceStatus(sdo.Status.Trim(), "--", "AT");
                        ObservationInfo += (int)polSourceObsInfo + ",";

                        if (sdo.Risk_Assessment == null)
                        {
                            sdo.Risk_Assessment = "MOD";
                        }

                        polSourceObsInfo = GetPolSourceRisk(sdo.Risk_Assessment.Trim(), "--", "AT");
                        ObservationInfo += (int)polSourceObsInfo + ",";

                        polSourceObservationModelNew.PolSourceSiteID = polSourceSiteModelRet.PolSourceSiteID;
                        polSourceObservationModelNew.Observation_ToBeDeleted = TVTextObs;

                        PolSourceSiteModel polSourceSiteModel = polSourceSiteService.PostUpdatePolSourceSiteDB(polSourceSiteModelRet);

                        PolSourceObservationModel polSourceObservationModelRet = polSourceObservationService.PostAddPolSourceObservationDB(polSourceObservationModelNew);
                        if (!CheckModelOK<PolSourceObservationModel>(polSourceObservationModelRet)) return false;

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

                }
            } 

            return true;
        }
    }
}
