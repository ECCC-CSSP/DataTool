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
        public bool CreateTVItemJustRootAndProvinces()
        {
            if (Cancel) return false;

            lblStatus.Text = "Starting ... CreateTVItemJustRootAndProvinces";
            Application.DoEvents();

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            // root item should already be in the DB
            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            using (OldCSSPApps.OldCSSPAppDB2Entities oldCSSPAppDB2 = new OldCSSPApps.OldCSSPAppDB2Entities())
            {
                int csspItemTypeProvID = (from c in oldCSSPAppDB2.CSSPItemTypes
                                          where c.CSSPItemTypeTextENLowered == "root"
                                          select c.CSSPItemTypeID).FirstOrDefault<int>();

                string OldCSSPItemPath = (from c in oldCSSPAppDB2.CSSPItems
                                          from cp in oldCSSPAppDB2.CSSPItemPaths
                                          from cl in oldCSSPAppDB2.CSSPItemLanguages
                                          where c.CSSPItemID == cp.CSSPItemID
                                          && c.CSSPItemID == cl.CSSPItemID
                                          && cl.Language == "en"
                                          && cl.CSSPItemText == "Root"
                                          select cp.Path).FirstOrDefault<string>();




                // getting all the province
                List<OldTVItem> OldItemProvList = (from c in oldCSSPAppDB2.CSSPItems
                                                   from cp in oldCSSPAppDB2.CSSPItemPaths
                                                   from cl in oldCSSPAppDB2.CSSPItemLanguages
                                                   where c.CSSPItemID == cp.CSSPItemID
                                                   && c.CSSPItemID == cl.CSSPItemID
                                                   && cp.Path.StartsWith(OldCSSPItemPath)
                                                   && cp.Level == 2
                                                   && cl.Language == "en"
                                                   select new OldTVItem
                                                   {
                                                       CSSPItemID = c.CSSPItemID,
                                                       CSSPPath = cp.Path,
                                                       CSSPText = cl.CSSPItemText,
                                                   }).ToList<OldTVItem>();

                int TotalCount = OldItemProvList.Count();
                int Count = 0;
                foreach (OldTVItem otiProv in OldItemProvList)
                {
                    if (Cancel) return false;

                    Count += 1;

                    if (otiProv.CSSPText == "About" || otiProv.CSSPText == "Administrator")
                    {
                        continue;
                    }

                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                    MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

                    TVItemModel tvItemModelCountry = new TVItemModel();
                    if (otiProv.CSSPText == "Maine")
                    {
                        tvItemModelCountry = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "United States", TVTypeEnum.Country);
                        if (!CheckModelOK<TVItemModel>(tvItemModelCountry)) return false;
                    }
                    else
                    {
                        tvItemModelCountry = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
                        if (!CheckModelOK<TVItemModel>(tvItemModelCountry)) return false;
                    }

                    TVItemModel tvItemModelProvince = tvItemService.PostAddChildTVItemDB(tvItemModelCountry.TVItemID, otiProv.CSSPText, TVTypeEnum.Province);
                    if (!CheckModelOK<TVItemModel>(tvItemModelProvince)) return false;

                    List<Coord> coordList = new List<Coord>() { new Coord() { Lat = 40.0f, Lng = -90.0f, Ordinal = 0 } };
                    switch (otiProv.CSSPText)
                    {
                        case "New Brunswick":
                            {
                                coordList = new List<Coord>() { new Coord() { Lat = 46.44f, Lng = -66.24f, Ordinal = 0 } };
                            }
                            break;
                        case "Newfoundland and Labrador":
                            {
                                coordList = new List<Coord>() { new Coord() { Lat = 48.53f, Lng = -56.37f, Ordinal = 0 } };
                            }
                            break;
                        case "Nova Scotia":
                            {
                                coordList = new List<Coord>() { new Coord() { Lat = 45.08f, Lng = -63.24f, Ordinal = 0 } };
                            }
                            break;
                        case "Prince Edward Island":
                            {
                                coordList = new List<Coord>() { new Coord() { Lat = 46.31f, Lng = -63.45f, Ordinal = 0 } };
                            }
                            break;
                        case "British Columbia":
                            {
                                coordList = new List<Coord>() { new Coord() { Lat = 52.72f, Lng = -123.12f, Ordinal = 0 } };
                            }
                            break;
                        case "Québec":
                            {
                                coordList = new List<Coord>() { new Coord() { Lat = 51.32f, Lng = -70.04f, Ordinal = 0 } };
                            }
                            break;
                        case "Maine":
                            {
                                coordList = new List<Coord>() { new Coord() { Lat = 45.0f, Lng = -68.67f, Ordinal = 0 } };
                            }
                            break;
                        default:
                            {
                                if (!CheckModelOK<TVItemModel>(new TVItemModel() { Error = "Could not find province in CreateTVItemJustRootAndProvince"}))
                                    return false;
                            }
                            break;
                    }

                    MapInfoModel mapInfoModel = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Province, tvItemModelProvince.TVItemID);
                    if (!CheckModelOK<MapInfoModel>(mapInfoModel)) return false;
                }
            }
         
            return true;
        }
    }
}
