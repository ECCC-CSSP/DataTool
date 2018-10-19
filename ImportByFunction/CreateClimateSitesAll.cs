using CSSPEnumsDLL.Enums;
using CSSPModelsDLL.Models;
using CSSPDBDLL.Services;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        private bool CreateClimateSitesAll(TVItemModel tvItemModel, List<OldClimateHydrometric.ClimateStation> csList, string Prov)
        {
            if (Cancel) return false;

            int StartNBCreateClimateSitesAll = int.Parse(textBoxNBCreateClimateSitesAll.Text);
            int StartNFCreateClimateSitesAll = int.Parse(textBoxNLCreateClimateSitesAll.Text);
            int StartNSCreateClimateSitesAll = int.Parse(textBoxNSCreateClimateSitesAll.Text);
            int StartPECreateClimateSitesAll = int.Parse(textBoxPECreateClimateSitesAll.Text);
            int StartBCCreateClimateSitesAll = int.Parse(textBoxBCCreateClimateSitesAll.Text);
            int StartQCCreateClimateSitesAll = int.Parse(textBoxQCCreateClimateSitesAll.Text);

            int Total = csList.Count;
            int Count = 0;
            foreach (OldClimateHydrometric.ClimateStation cs in csList)
            {
                if (Cancel) return false;

                if (cs.Latitude == null)
                {
                    continue;
                }
                if (cs.ECDBID == 50009)
                {
                    continue;
                }

                Count += 1;
                lblStatus.Text = ((Count * 100) / Total).ToString() + " " + Prov + " ... CreateClimateSite";
                lblStatus2.Text = Count + " of " + Total;
                Application.DoEvents();

                switch (Prov)
                {
                    case "New Brunswick":
                        {
                            textBoxNBCreateClimateSitesAll.Text = Count.ToString();
                            if (StartNBCreateClimateSitesAll > Count)
                            {
                                continue;
                            }
                        }
                        break;
                    case "Newfoundland and Labrador":
                        {
                            textBoxNLCreateClimateSitesAll.Text = Count.ToString();
                            if (StartNFCreateClimateSitesAll > Count)
                            {
                                continue;
                            }
                        }
                        break;
                    case "Nova Scotia":
                        {
                            textBoxNSCreateClimateSitesAll.Text = Count.ToString();
                            if (StartNSCreateClimateSitesAll > Count)
                            {
                                continue;
                            }
                        }
                        break;
                    case "Prince Edward Island":
                        {
                            textBoxPECreateClimateSitesAll.Text = Count.ToString();
                            if (StartPECreateClimateSitesAll > Count)
                            {
                                continue;
                            }
                        }
                        break;
                    case "British Columbia":
                        {
                            textBoxBCCreateClimateSitesAll.Text = Count.ToString();
                            if (StartBCCreateClimateSitesAll > Count)
                            {
                                continue;
                            }
                        }
                        break;
                    case "Québec":
                        {
                            textBoxQCCreateClimateSitesAll.Text = Count.ToString();
                            if (StartQCCreateClimateSitesAll > Count)
                            {
                                continue;
                            }
                        }
                        break;
                    default:
                        break;
                }

                ClimateSiteService climateSiteService = new ClimateSiteService(LanguageEnum.en, user);
                MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
                TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                // Climat Station Type
                string TVText = tvItemService.CleanText(cs.ClimateStationName + " (" + cs.ClimateID + ")");

                TVItemModel tvItemModelClimateSiteRet = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModel.TVItemID, TVText, TVTypeEnum.ClimateSite);
                if (!string.IsNullOrWhiteSpace(tvItemModelClimateSiteRet.Error))
                {
                    tvItemModelClimateSiteRet = tvItemService.PostCreateTVItem(tvItemModel.TVItemID, TVText, TVText, TVTypeEnum.ClimateSite);
                    if (!CheckModelOK<TVItemModel>(tvItemModelClimateSiteRet)) return false;
                }

                Application.DoEvents();

                string prov = "";
                if (cs.Province == "PEI")
                {
                    prov = "PE";
                }
                if (cs.Province == "NB")
                {
                    prov = "NB";
                }
                if (cs.Province == "NS")
                {
                    prov = "NS";
                }
                if (cs.Province == "NFLD")
                {
                    prov = "NL";
                }
                if (cs.Province == "BC")
                {
                    prov = "BC";
                }
                if (cs.Province == "QUE")
                {
                    prov = "QC";
                }

                ClimateSiteModel climateSiteModelNew = new ClimateSiteModel();
                climateSiteModelNew.ClimateSiteTVItemID = tvItemModelClimateSiteRet.TVItemID;
                climateSiteModelNew.ECDBID = cs.ECDBID;
                climateSiteModelNew.ClimateSiteName = cs.ClimateStationName;
                climateSiteModelNew.Province = prov;
                climateSiteModelNew.Elevation_m = cs.Elevation;
                climateSiteModelNew.ClimateID = cs.ClimateID;
                climateSiteModelNew.WMOID = cs.WMOID;
                climateSiteModelNew.TCID = cs.TCID;
                climateSiteModelNew.TimeOffset_hour = cs.TimeOffset_hour;
                climateSiteModelNew.File_desc = cs.File_desc;
                climateSiteModelNew.HourlyStartDate_Local = cs.HourlyStartDate;
                climateSiteModelNew.HourlyEndDate_Local = cs.HourlyEndDate;
                climateSiteModelNew.HourlyNow = cs.HourlyNow;
                climateSiteModelNew.DailyStartDate_Local = cs.DailyStartDate;
                climateSiteModelNew.DailyEndDate_Local = cs.DailyEndDate;
                climateSiteModelNew.DailyNow = cs.DailyNow;
                climateSiteModelNew.MonthlyStartDate_Local = cs.MonthlyStartDate;
                climateSiteModelNew.MonthlyEndDate_Local = cs.MonthlyEndDate;
                climateSiteModelNew.MonthlyNow = cs.MonthlyNow;

                ClimateSiteModel climateSiteModelRet = climateSiteService.GetClimateSiteModelWithClimateSiteTVItemIDDB(tvItemModelClimateSiteRet.TVItemID);
                if (!string.IsNullOrWhiteSpace(climateSiteModelRet.Error))
                {
                    climateSiteModelRet = climateSiteService.PostAddClimateSiteDB(climateSiteModelNew);
                    if (!CheckModelOK<ClimateSiteModel>(climateSiteModelRet)) return false;
                }

                List<Coord> coordList2 = new List<Coord>() 
                {
                    new Coord()
                    {
                        Lat = (float)cs.Latitude,
                        Lng = (float)cs.Longitude,
                    }
                };

                MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.ClimateSite, tvItemModelClimateSiteRet.TVItemID);
                if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;
            }

            return true;
        }
    }
}
