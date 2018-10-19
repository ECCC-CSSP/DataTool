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
using CSSPDBDLL.Services;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        private bool CreateHydrometricSitesAll(TVItemModel tvItemModel, List<OldClimateHydrometric.HydrometricStation> hsList, string Prov)
        {
            if (Cancel) return false;

            int StartNBCreateHydrometricSitesAll = int.Parse(textBoxNBCreateHydrometricSitesAll.Text);
            int StartNFCreateHydrometricSitesAll = int.Parse(textBoxNLCreateHydrometricSitesAll.Text);
            int StartNSCreateHydrometricSitesAll = int.Parse(textBoxNSCreateHydrometricSitesAll.Text);
            int StartPECreateHydrometricSitesAll = int.Parse(textBoxPECreateHydrometricSitesAll.Text);
            int StartBCCreateHydrometricSitesAll = int.Parse(textBoxBCCreateHydrometricSitesAll.Text);
            int StartQCCreateHydrometricSitesAll = int.Parse(textBoxQCCreateHydrometricSitesAll.Text);

            int Total = hsList.Count;
            int Count = 0;
            foreach (OldClimateHydrometric.HydrometricStation hs in hsList)
            {
                if (Cancel) return false;

                Count += 1;
                lblStatus.Text = ((Count * 100) / Total).ToString() + " " + Prov + " ... CreateHydrometric";
                lblStatus2.Text = Count + " of " + Total;
                Application.DoEvents();

                switch (Prov)
                {
                    case "New Brunswick":
                        {
                            textBoxNBCreateHydrometricSitesAll.Text = Count.ToString();
                            if (StartNBCreateHydrometricSitesAll > Count)
                            {
                                continue;
                            }
                        }
                        break;
                    case "Newfoundland and Labrador":
                        {
                            textBoxNLCreateHydrometricSitesAll.Text = Count.ToString();
                            if (StartNFCreateHydrometricSitesAll > Count)
                            {
                                continue;
                            }
                        }
                        break;
                    case "Nova Scotia":
                        {
                            textBoxNSCreateHydrometricSitesAll.Text = Count.ToString();
                            if (StartNSCreateHydrometricSitesAll > Count)
                            {
                                continue;
                            }
                        }
                        break;
                    case "Prince Edward Island":
                        {
                            textBoxPECreateHydrometricSitesAll.Text = Count.ToString();
                            if (StartPECreateHydrometricSitesAll > Count)
                            {
                                continue;
                            }
                        }
                        break;
                    case "British Columbia":
                        {
                            textBoxBCCreateHydrometricSitesAll.Text = Count.ToString();
                            if (StartBCCreateHydrometricSitesAll > Count)
                            {
                                continue;
                            }
                        }
                        break;
                    case "Québec":
                        {
                            textBoxQCCreateHydrometricSitesAll.Text = Count.ToString();
                            if (StartQCCreateHydrometricSitesAll > Count)
                            {
                                continue;
                            }
                        }
                        break;
                    default:
                        break;
                }


                HydrometricSiteService hydrometricSiteService = new HydrometricSiteService(LanguageEnum.en, user);
                MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
                TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                // Hydrometric Station Type
                string TVText = tvItemService.CleanText(hs.HydrometricStationName + (hs.FedStationNumber == null ? "" : (" F(" + hs.FedStationNumber) + ")") + (hs.QuebecStationNumber == null ? "" : (" Q(" + hs.QuebecStationNumber) + ")"));
                int Level = GetLevel(tvItemModel.TVPath) + 1;

                TVItemModel tvItemModelHydrometricSiteExist = tvItemService.PostCreateTVItem(tvItemModel.TVItemID, TVText, TVText, TVTypeEnum.HydrometricSite);
                if (!CheckModelOK<TVItemModel>(tvItemModelHydrometricSiteExist)) return false;

                Application.DoEvents();

                HydrometricSiteModel hydrometricModelSiteExist = hydrometricSiteService.GetHydrometricSiteModelWithHydrometricSiteTVItemIDDB(tvItemModelHydrometricSiteExist.TVItemID);
                if (!string.IsNullOrWhiteSpace(hydrometricModelSiteExist.Error))
                {

                    HydrometricSiteModel hydrometricSiteModelNew = new HydrometricSiteModel();
                    hydrometricSiteModelNew.HydrometricSiteTVItemID = tvItemModelHydrometricSiteExist.TVItemID;
                    hydrometricSiteModelNew.FedSiteNumber = hs.FedStationNumber;
                    hydrometricSiteModelNew.QuebecSiteNumber = hs.QuebecStationNumber;
                    hydrometricSiteModelNew.HydrometricSiteName = hs.HydrometricStationName;
                    hydrometricSiteModelNew.Description = hs.Description;
                    hydrometricSiteModelNew.Province = hs.Province;
                    hydrometricSiteModelNew.Elevation_m = hs.Elevation;
                    hydrometricSiteModelNew.StartDate_Local = hs.StartDate;
                    hydrometricSiteModelNew.EndDate_Local = hs.EndDate;
                    hydrometricSiteModelNew.TimeOffset_hour = hs.TimeOffset_hour;
                    hydrometricSiteModelNew.DrainageArea_km2 = hs.DrainageArea;
                    hydrometricSiteModelNew.IsNatural = hs.IsNatural;
                    hydrometricSiteModelNew.IsActive = hs.IsActive;
                    hydrometricSiteModelNew.Sediment = hs.Sediment;
                    hydrometricSiteModelNew.RHBN = hs.RHBN;
                    hydrometricSiteModelNew.RealTime = hs.RealTime;
                    hydrometricSiteModelNew.HasRatingCurve = hs.HasRatingCurve;

                    if (hs.FedStationNumber == "02QB018")
                    {
                        if (hs.StartDate > hs.EndDate)
                        {
                            hydrometricSiteModelNew.EndDate_Local = hs.StartDate;
                        }
                    }

                    HydrometricSiteModel hydrometricSiteModelRet = hydrometricSiteService.PostAddHydrometricSiteDB(hydrometricSiteModelNew);
                    if (!CheckModelOK<HydrometricSiteModel>(hydrometricSiteModelRet)) return false;

                    using (OldClimateHydrometric.OldClimateHydrometricDBEntities oldClimateHydrometric = new OldClimateHydrometric.OldClimateHydrometricDBEntities())
                    {

                        List<OldClimateHydrometric.RatingCurve> rcList = (from c in oldClimateHydrometric.RatingCurves
                                                                          where c.HydrometricStationID == hs.HydrometricStationID
                                                                          select c).ToList<OldClimateHydrometric.RatingCurve>();

                        foreach (OldClimateHydrometric.RatingCurve rc in rcList)
                        {
                            RatingCurveService ratingCurveService = new RatingCurveService(LanguageEnum.en, user);
                            RatingCurveValueService ratingCurveValueService = new RatingCurveValueService(LanguageEnum.en, user);

                            RatingCurveModel ratingCurveModelNew = new RatingCurveModel();
                            ratingCurveModelNew.RatingCurveNumber = rc.RatingCurveNumber;
                            ratingCurveModelNew.HydrometricSiteID = hydrometricSiteModelRet.HydrometricSiteID;

                            RatingCurveModel ratingCurveModelRet = ratingCurveService.PostAddRatingCurveDB(ratingCurveModelNew);
                            if (!CheckModelOK<RatingCurveModel>(ratingCurveModelRet)) return false;

                            List<OldClimateHydrometric.RatingCurveValue> rcvList = (from c in oldClimateHydrometric.RatingCurveValues
                                                                                    where c.RatingCurveID == rc.RatingCurveID
                                                                                    select c).ToList<OldClimateHydrometric.RatingCurveValue>();



                            foreach (OldClimateHydrometric.RatingCurveValue rcv in rcvList)
                            {
                                RatingCurveValueModel ratingCurveValueModelNew = new RatingCurveValueModel();
                                ratingCurveValueModelNew.StageValue_m = rcv.StageValue;
                                ratingCurveValueModelNew.DischargeValue_m3_s = rcv.DischargeValue;
                                ratingCurveValueModelNew.RatingCurveID = ratingCurveModelRet.RatingCurveID;

                                RatingCurveValueModel ratingCurveValueModelRet = ratingCurveValueService.PostAddRatingCurveValueDB(ratingCurveValueModelNew);
                                if (!CheckModelOK<RatingCurveValueModel>(ratingCurveValueModelRet)) return false;
                            }
                        }
                    }

                    List<Coord> coordList2 = new List<Coord>() 
                        {
                            new Coord()
                            {
                                Lat = (float)hs.Latitude,
                                Lng = (float)hs.Longitude,
                            }
                        };

                    MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.HydrometricSite, tvItemModelHydrometricSiteExist.TVItemID);
                    if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;
                }
            }

            return true;
        }
    }
}
