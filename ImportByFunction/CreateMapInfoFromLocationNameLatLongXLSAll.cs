using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSSPEnumsDLL.Enums;
using CSSPModelsDLL.Models;
using CSSPDBDLL.Services;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        private bool CreateMapInfoFromLocationNameLatLongXLSAll(string Prov)
        {
            lblStatus.Text = "Starting ... CreateMapInfoFromLocationNameLatLongXLSAll";
            Application.DoEvents();

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCountry = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCountry)) return false;

            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            int StartNBCreateMapInfoFromLocationNameLatLongXLSAll = int.Parse(textBoxNBCreateMapInfoFromLocationNameLatLongXLSAll.Text);
            int StartNLCreateMapInfoFromLocationNameLatLongXLSAll = int.Parse(textBoxNLCreateMapInfoFromLocationNameLatLongXLSAll.Text);
            int StartNSCreateMapInfoFromLocationNameLatLongXLSAll = int.Parse(textBoxNSCreateMapInfoFromLocationNameLatLongXLSAll.Text);
            int StartPECreateMapInfoFromLocationNameLatLongXLSAll = int.Parse(textBoxPECreateMapInfoFromLocationNameLatLongXLSAll.Text);
            int StartBCCreateMapInfoFromLocationNameLatLongXLSAll = int.Parse(textBoxBCCreateMapInfoFromLocationNameLatLongXLSAll.Text);
            int StartQCCreateMapInfoFromLocationNameLatLongXLSAll = int.Parse(textBoxQCCreateMapInfoFromLocationNameLatLongXLSAll.Text);

            if (Prov == "")
            {
                using (TempData.TempDataToolDBEntities dbTD = new TempData.TempDataToolDBEntities())
                {
                    var locationList = (from c in dbTD.LocationNameLatLngs
                                        where (c.Municipality == "" || c.Municipality == null)
                                        select new { c.Province, c.Lat, c.Lng });

                    int CountTotal = locationList.Count();
                    int Count = 0;
                    foreach (var l in locationList)
                    {
                        Count += 1;

                        lblStatus.Text = (Count * 100 / CountTotal).ToString() + " ... CreateMapInfoFromLocationNameLatLongXLSAll for provinces ";
                        Application.DoEvents();

                        TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                        TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCountry.TVItemID, l.Province.Trim(), TVTypeEnum.Province);
                        if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return false;

                        List<Coord> coordList = new List<Coord>() { new Coord() { Lat = (float)l.Lat, Lng = (float)l.Lng, Ordinal = 0 } };

                        MapInfoModel mapInfoModel = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Province, tvItemModelProv.TVItemID);
                        if (!CheckModelOK<MapInfoModel>(mapInfoModel)) return false;

                        CreateMapInfoFromLocationNameLatLongXLSAll(l.Province.Trim());
                    }
                }
            }
            else
            {
                TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCountry.TVItemID, Prov, TVTypeEnum.Province);
                if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return false;

                using (TempData.TempDataToolDBEntities dbTD = new TempData.TempDataToolDBEntities())
                {
                    var locationList = (from c in dbTD.LocationNameLatLngs
                                        where (c.Municipality != "" && c.Municipality != null)
                                        && c.Province == Prov
                                        select new { c.Municipality, c.Lat, c.Lng });

                    int CountTotal = locationList.Count();
                    int Count = 0;
                    foreach (var l in locationList)
                    {
                        Count += 1;
                        lblStatus.Text = (Count * 100 / CountTotal).ToString() + " ... CreateMapInfoFromLocationNameLatLongXLSAll for " + l.Municipality.Trim();
                        lblStatus2.Text = Count + " of " + CountTotal;
                        Application.DoEvents();


                        switch (Prov)
                        {
                            case "New Brunswick":
                                {
                                    textBoxNBCreateMapInfoFromLocationNameLatLongXLSAll.Text = Count.ToString();
                                    if (StartNBCreateMapInfoFromLocationNameLatLongXLSAll > Count)
                                    {
                                        continue;
                                    }
                                }
                                break;
                            case "Newfoundland and Labrador":
                                {
                                    textBoxNLCreateMapInfoFromLocationNameLatLongXLSAll.Text = Count.ToString();
                                    if (StartNLCreateMapInfoFromLocationNameLatLongXLSAll > Count)
                                    {
                                        continue;
                                    }
                                }
                                break;
                            case "Nova Scotia":
                                {
                                    textBoxNSCreateMapInfoFromLocationNameLatLongXLSAll.Text = Count.ToString();
                                    if (StartNSCreateMapInfoFromLocationNameLatLongXLSAll > Count)
                                    {
                                        continue;
                                    }
                                }
                                break;
                            case "Prince Edward Island":
                                {
                                    textBoxPECreateMapInfoFromLocationNameLatLongXLSAll.Text = Count.ToString();
                                    if (StartPECreateMapInfoFromLocationNameLatLongXLSAll > Count)
                                    {
                                        continue;
                                    }
                                }
                                break;
                            case "British Columbia":
                                {
                                    textBoxBCCreateMapInfoFromLocationNameLatLongXLSAll.Text = Count.ToString();
                                    if (StartBCCreateMapInfoFromLocationNameLatLongXLSAll > Count)
                                    {
                                        continue;
                                    }
                                }
                                break;
                            case "Québec":
                                {
                                    textBoxQCCreateMapInfoFromLocationNameLatLongXLSAll.Text = Count.ToString();
                                    if (StartQCCreateMapInfoFromLocationNameLatLongXLSAll > Count)
                                    {
                                        continue;
                                    }
                                }
                                break;
                            default:
                                break;
                        }

                        TVItemModel tvItemModelMuni = tvItemService.PostCreateTVItem(tvItemModelProv.TVItemID, l.Municipality.Trim(), l.Municipality.Trim(), TVTypeEnum.Municipality);
                        if (!CheckModelOK<TVItemModel>(tvItemModelMuni)) return false;

                        List<Coord> coordList = new List<Coord>() { new Coord() { Lat = (float)l.Lat, Lng = (float)l.Lng, Ordinal = 0 } };

                        MapInfoModel mapInfoModel = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Municipality, tvItemModelMuni.TVItemID);
                        if (!CheckModelOK<MapInfoModel>(mapInfoModel)) return false;

                    }
                }
            }

            return true;
        }
    }
}
