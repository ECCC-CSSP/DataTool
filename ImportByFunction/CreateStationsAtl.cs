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
        public bool CreateStationsAtl(List<string> justProvList)
        {
            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            List<string> ProvList = new List<string>() { "NB", "NL", "NS", "PE" };

            lblStatus.Text = "Starting ... CreateStationsAtl";
            Application.DoEvents();

            int StartNBCreateStationAtl = int.Parse(textBoxNBCreateStationsAtl.Text);
            int StartNLCreateStationAtl = int.Parse(textBoxNLCreateStationsAtl.Text);
            int StartNSCreateStationAtl = int.Parse(textBoxNSCreateStationsAtl.Text);
            int StartPECreateStationAtl = int.Parse(textBoxPECreateStationsAtl.Text);

            Dictionary<string, int> SubsectorDict = new Dictionary<string, int>();

            foreach (string Prov in ProvList)
            {
                if (justProvList.Contains(Prov))
                {
                    string ProvinceTxt = "";
                    switch (Prov)
                    {
                        case "NB":
                            {
                                ProvinceTxt = "New Brunswick";
                            }
                            break;
                        case "NL":
                            {
                                ProvinceTxt = "Newfoundland and Labrador";
                            }
                            break;
                        case "NS":
                            {
                                ProvinceTxt = "Nova Scotia";
                            }
                            break;
                        case "PE":
                            {
                                ProvinceTxt = "Prince Edward Island";
                            }
                            break;
                        default:
                            break;
                    }

                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                    TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, ProvinceTxt, TVTypeEnum.Province);
                    if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return false;


                    // filling the TideDict global variable for future use
                    List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
                    if (tvItemModelSubsectorList.Count == 0)
                    {
                        richTextBoxStatus.AppendText("Error: could not find any Subsectors under province " + tvItemModelProv.TVText + "\r\n");
                        return false;
                    }

                    SubsectorDict.Clear();
                    foreach (var tv in tvItemModelSubsectorList)
                    {
                        SubsectorDict.Add(tv.TVText.Substring(0, 13), tv.TVItemID);
                    }

                    if (SubsectorDict.Count != tvItemModelSubsectorList.Count)
                    {
                        richTextBoxStatus.AppendText("Error: should have the same number of item in SubsectorDict and TempSubsectorValueList\r\n");
                        return false;
                    }

                    using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                    {
                        List<TempData.ASGADStation> staList = (from c in dbDT.ASGADStations
                                                               orderby c.ASGADStationID
                                                               where c.PROV == Prov
                                                               select c).ToList<TempData.ASGADStation>();

                        int TotalCount = staList.Count();
                        int Count = 0;
                        foreach (TempData.ASGADStation ast in staList)
                        {
                            if (Cancel) return false;

                            Count += 1;
                            lblStatus.Text = (Count * 100 / TotalCount).ToString() + "% ... CreateStationsAll for " + Prov;
                            lblStatus2.Text = Count + " of " + TotalCount;
                            Application.DoEvents();

                            switch (Prov)
                            {
                                case "NB":
                                    {
                                        textBoxNBCreateStationsAtl.Text = Count.ToString();
                                        if (StartNBCreateStationAtl > Count)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                case "NL":
                                    {
                                        textBoxNLCreateStationsAtl.Text = Count.ToString();
                                        if (StartNLCreateStationAtl > Count)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                case "NS":
                                    {
                                        textBoxNSCreateStationsAtl.Text = Count.ToString();
                                        if (StartNSCreateStationAtl > Count)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                case "PE":
                                    {
                                        textBoxPECreateStationsAtl.Text = Count.ToString();
                                        if (StartPECreateStationAtl > Count)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            if (!CreateStation(ast, SubsectorDict))
                            {
                                return false;
                            }

                        }
                    }
                }
            }

            return true;
        }

        public bool CreateStation(TempData.ASGADStation ast, Dictionary<string, int> SubsectorDict)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            // WQM Station item
            string WQMSiteTVText = tvItemService.CleanText(ast.STAT_NBR);
            string Locator = ast.PROV + "-" + ast.AREA + "-" + ast.SECTOR + "-" + ast.SUBSECTOR;
            int tvItemID = SubsectorDict[Locator];

            TVItemModel tvItemModelMWQMSiteExist = tvItemService.PostCreateTVItem(tvItemID, WQMSiteTVText, WQMSiteTVText, TVTypeEnum.MWQMSite);
            if (!CheckModelOK<TVItemModel>(tvItemModelMWQMSiteExist)) return false;

            MWQMSiteModel mwqmSiteModelNew = mwqmSiteService.GetMWQMSiteModelWithMWQMSiteTVItemIDDB(tvItemModelMWQMSiteExist.TVItemID);
            if (!string.IsNullOrWhiteSpace(mwqmSiteModelNew.Error))
            {
                mwqmSiteModelNew.Error = "";
                mwqmSiteModelNew.MWQMSiteTVItemID = tvItemModelMWQMSiteExist.TVItemID;
                mwqmSiteModelNew.MWQMSiteNumber = ast.STAT_NBR;
                mwqmSiteModelNew.MWQMSiteTVText = tvItemService.CleanText(ast.STAT_NBR.Trim());

                //string TextEN = tvItemService.CleanText(ast.STAT_NBR.Trim());
                //string TextFR = tvItemService.CleanText(ast.STAT_NBR.Trim());

                int Ordinal = 999;
                int.TryParse(ast.SORT_ORDER, out Ordinal);
                mwqmSiteModelNew.Ordinal = Ordinal;
                if (ast.STAT_TYPE.Trim() == "KS")
                {
                    // nothing for now
                }
                else
                {
                    // nothing for now
                }

                mwqmSiteModelNew.MWQMSiteLatestClassification = MWQMSiteLatestClassificationEnum.Error;
                mwqmSiteModelNew.MWQMSiteDescription = (string.IsNullOrWhiteSpace(ast.STAT_NAME) ? "Describe " + mwqmSiteModelNew.MWQMSiteNumber : ast.STAT_NAME);
                MWQMSiteModel mwqmSiteModelRet = mwqmSiteService.PostAddMWQMSiteDB(mwqmSiteModelNew);
                if (!CheckModelOK<MWQMSiteModel>(mwqmSiteModelRet)) return false;

                List<Coord> coordList2 = new List<Coord>() 
                        {
                            new Coord()
                            {
                                Lat = (ast.LAT == null ? 0 : (float)ast.LAT),
                                Lng = (ast.LONG == null ? 0 : (float)ast.LONG),
                            }
                        };

                if (coordList2[0].Lng == -361.04598999023437)
                {
                    coordList2[0].Lng = -61.046f;
                }

                if (coordList2[0].Lat == 449.45498657226562)
                {
                    coordList2[0].Lat = 49.45498657226562f;
                }

                MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.MWQMSite, tvItemModelMWQMSiteExist.TVItemID);
                if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;                  

            }

            return true;
        }
    }
}
