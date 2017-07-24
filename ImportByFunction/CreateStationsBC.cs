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
using CSSPModelsDLL.Models;
using CSSPWebToolsDBDLL.Services;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CreateStationsBC()
        {
            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelBC = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "British Columbia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelBC)) return false;

            List<TVItemModel> tvItemModelSubsectorBCList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelBC.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorBCList.Count == 0)
            {
                richTextBoxStatus.AppendText("Could not find TVItem Subsector under British Columbia\r\n");
                return false;
            }

            lblStatus.Text = "Starting ... LoadStationsBC";
            Application.DoEvents();

            int StartBCCreateStationBC = int.Parse(textBoxBCCreateStationsBC.Text);        

            List<BCStation> BCWQMSiteList = new List<BCStation>();
            TVItemModel TVItemModelSubsectorBC = new TVItemModel();
            List<TT> tideTextInDBList = new List<TT>();
            List<AM> analyseMethodInDBList = new List<AM>();
            List<Mat> matrixInDBList = new List<Mat>();
            List<Lab> labInDBList = new List<Lab>();

            int TotalCount = tvItemModelSubsectorBCList.Count();
            int Count = 0;
            foreach (TVItemModel tvItemModelSubsector in tvItemModelSubsectorBCList)
            {
                if (Cancel) return false;

                Count += 1;
                lblStatus.Text = (Count * 100 / TotalCount).ToString() + " ... CreateStationsBC for " + tvItemModelSubsector.TVText;
                lblStatus2.Text = Count + " of " + TotalCount;
                Application.DoEvents();

                textBoxBCCreateStationsBC.Text = Count.ToString();

                if (StartBCCreateStationBC > Count)
                {
                    continue;
                }

                // doing Land base stations

                List<TempData.BCLandSampleStation> bcLandSampleStation = new List<TempData.BCLandSampleStation>();

                using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                {
                    string TVText = tvItemModelSubsector.TVText.Substring(0, 4);

                    bcLandSampleStation = (from c in dbDT.BCLandSampleStations
                                           where c.SS_SHELLFISH_SECTOR == TVText
                                           orderby c.BCLandSampleStationID
                                           select c).ToList<TempData.BCLandSampleStation>();

                }

                int CountSta = 0;
                int TotalCountSta = bcLandSampleStation.Count;
                foreach (TempData.BCLandSampleStation bcmss in bcLandSampleStation)
                {
                    if (Cancel) return false;

                    CountSta += 1;
                    lblStatus2.Text = "Doing Land Base ... " + CountSta + " of " + TotalCountSta;
                    Application.DoEvents();

                    string TVText = bcmss.SS_STATION_CODE;

                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                    MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
                    MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);

                    TVItemModel tvItemModelMWQMSite = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelSubsector.TVItemID, TVText, TVTypeEnum.MWQMSite);
                    if (!string.IsNullOrWhiteSpace(tvItemModelMWQMSite.Error))
                    {
                        tvItemModelMWQMSite = tvItemService.PostCreateTVItem(tvItemModelSubsector.TVItemID, TVText, TVText, TVTypeEnum.MWQMSite);
                        if (!CheckModelOK<TVItemModel>(tvItemModelMWQMSite)) return false;

                        List<Coord> coordList2 = new List<Coord>() 
                        {
                            new Coord()
                            {
                                Lat = (float)(bcmss.LAT == null ? 48 : bcmss.LAT),
                                Lng = (float)(bcmss.LON == null ? -125 : bcmss.LON),
                            }
                        };

                        if (coordList2[0].Lat == 124.40966796875 && coordList2[0].Lng == -48.585498809814453)
                        {
                            coordList2[0].Lat = 48.585498809814453f;
                            coordList2[0].Lng = -124.40966796875f;
                        }

                        MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.MWQMSite, tvItemModelMWQMSite.TVItemID);
                        if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                        MWQMSiteModel mwqmSiteModelNew = new MWQMSiteModel()
                        {
                            MWQMSiteTVItemID = tvItemModelMWQMSite.TVItemID,
                            MWQMSiteNumber = bcmss.OID.ToString(),
                            MWQMSiteTVText = TVText,
                        };

                        MWQMSiteModel mwqmSiteModelRet = mwqmSiteService.PostAddMWQMSiteDB(mwqmSiteModelNew);
                        if (!CheckModelOK<MWQMSiteModel>(mwqmSiteModelRet)) return false;

                    }
                }

                // doing Marine base stations

                List<TempData.BCMarineSampleStation> bcMarineSampleStation = new List<TempData.BCMarineSampleStation>();

                using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                {
                    string TVText = tvItemModelSubsector.TVText.Substring(0, 4);

                    bcMarineSampleStation = (from c in dbDT.BCMarineSampleStations
                                             where c.SS_SHELLFISH_SECTOR == TVText
                                             orderby c.BCMarineSampleStationID
                                             select c).ToList<TempData.BCMarineSampleStation>();

                }

                CountSta = 0;
                TotalCountSta = bcMarineSampleStation.Count;
                foreach (TempData.BCMarineSampleStation bcmss in bcMarineSampleStation)
                {
                    if (Cancel) return false;

                    CountSta += 1;
                    lblStatus2.Text = "Doing Marine Base ... " + CountSta + " of " + TotalCountSta;
                    Application.DoEvents();

                    string TVText = bcmss.SS_STATION_CODE;

                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                    MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
                    MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);

                    TVItemModel tvItemModelMWQMSite = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelSubsector.TVItemID, TVText, TVTypeEnum.MWQMSite);
                    if (!string.IsNullOrWhiteSpace(tvItemModelMWQMSite.Error))
                    {
                        tvItemModelMWQMSite = tvItemService.PostCreateTVItem(tvItemModelSubsector.TVItemID, TVText, TVText, TVTypeEnum.MWQMSite);
                        if (!CheckModelOK<TVItemModel>(tvItemModelMWQMSite)) return false;

                        List<Coord> coordList2 = new List<Coord>() 
                        {
                            new Coord()
                            {
                                Lat = (float)bcmss.LAT,
                                Lng = (float)bcmss.LON,
                            }
                        };

                        MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.MWQMSite, tvItemModelMWQMSite.TVItemID);
                        if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                        MWQMSiteModel mwqmSiteModelNew = new MWQMSiteModel()
                        {
                            MWQMSiteTVItemID = tvItemModelMWQMSite.TVItemID,
                            MWQMSiteNumber = bcmss.OID.ToString(),
                            MWQMSiteTVText = TVText,
                        };

                        MWQMSiteModel mwqmSiteModelRet = mwqmSiteService.PostAddMWQMSiteDB(mwqmSiteModelNew);
                        if (!CheckModelOK<MWQMSiteModel>(mwqmSiteModelRet)) return false;

                    }
                }
            }

            return true;
        }
    }
}
