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
        public bool CreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML()
        {
            lblStatus.Text = "Starting ... CreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML";
            Application.DoEvents();

            if (Cancel) return false;

            int StartAreaCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML = int.Parse(textBoxAreaCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text);
            int StartSectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML = int.Parse(textBoxSectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text);
            int StartSubsectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML = int.Parse(textBoxSubsectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text);

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelArea = new TVItemModel();
            TVItemModel tvItemModelSector = new TVItemModel();
            TVItemModel tvItemModelSubsector = new TVItemModel();
            TVItemModel tvItemModelBC = new TVItemModel();

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            tvItemModelBC = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "British Columbia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelBC)) return false;

            List<SubSector> SubsectorList = new List<SubSector>();

            // ----- doing Areas ----

            FileInfo fi = new FileInfo(@"C:\DataTool\ImportByFunction\Data_inputs\BC Area.kml");


            if (!fi.Exists)
            {
                richTextBoxStatus.AppendText("File not found [" + fi.FullName + "]\r\n");
                return false;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(fi.FullName);

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                string Area = "";
                XmlNode StartNode = doc.ChildNodes[1].ChildNodes[0].ChildNodes[4];
                int TotalAreaCount = StartNode.ChildNodes.Count;
                if (TotalAreaCount != 6)
                {
                    richTextBoxStatus.AppendText("Did not find 6 polygon in BC Area\r\n");
                    return false;
                }
                int CountArea = 0;
                foreach (XmlNode n in StartNode.ChildNodes)
                {
                    if (Cancel) return false;

                    CountArea += 1;

                    textBoxAreaCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text = CountArea.ToString();

                    if (StartAreaCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML > CountArea)
                    {
                        continue;
                    }

                    if (n.Name == "Placemark")
                    {
                        foreach (XmlNode n1 in n.ChildNodes)
                        {
                            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

                            if (n1.Name == "name")
                            {
                                Area = tvItemService.CleanText(n1.InnerText);
                                Application.DoEvents();

                                lblStatus.Text = " ... CreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML - Area " + Area;
                                lblStatus2.Text = CountArea.ToString();
                                Application.DoEvents();

                            }
                            if (n1.Name == "Polygon")
                            {
                                XmlNode coordNode = n1.ChildNodes[1].ChildNodes[0].ChildNodes[0];
                                if (coordNode.Name != "coordinates")
                                {
                                    richTextBoxStatus.AppendText("coordNode Name is not coordinates\r\n");
                                    return false;
                                }
                                List<Coord> coordList = new List<Coord>();
                                string[] coordStr = coordNode.InnerText.Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                int Ordinal = 0;
                                foreach (string cs in coordStr)
                                {
                                    string[] coordVal = cs.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    if (coordVal.Count() != 3)
                                    {
                                        richTextBoxStatus.AppendText("coordVal should have 3 values\r\n");
                                        return false;
                                    }
                                    Coord c = new Coord() { Lng = float.Parse(coordVal[0]), Lat = float.Parse(coordVal[1]), Ordinal = Ordinal };
                                    coordList.Add(c);
                                    Application.DoEvents();
                                    Ordinal += 1;
                                }

                                TempData.BCName bcName = (from c in dbDT.BCNames
                                                          where c.Accronym == Area
                                                          select c).FirstOrDefault<TempData.BCName>();

                                if (!Area.Contains("("))
                                {
                                    Area += " (" + bcName.Name + ")";
                                }

                                tvItemModelArea = tvItemService.PostCreateTVItem(tvItemModelBC.TVItemID, Area, Area, TVTypeEnum.Area);
                                if (!CheckModelOK<TVItemModel>(tvItemModelArea)) return false;

                                List<Coord> coordList2 = new List<Coord>() 
                                                            {
                                                                new Coord()
                                                                {
                                                                    Lat = (from c in coordList select c.Lat).Average(),
                                                                    Lng = (from c in coordList select c.Lng).Average(),
                                                                }
                                                            };

                                MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.Area, tvItemModelArea.TVItemID);
                                if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                                mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Polygon, TVTypeEnum.Area, tvItemModelArea.TVItemID);
                                if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                            }
                        }
                    }
                    //richTextBoxStatus.AppendText(n.Name + "\r\n");
                }

                //  ----- doing BC Sectors grouping ----


                List<string> GBE = new List<string>() { "DP", "DS", "HS", "JI", "LM", "MS", "OK", "PR", "ST", "SU" };
                List<string> GBW = new List<string>() { "BS", "CB", "GA", "GI", "LH", "LI", "NA", "NH", "PL", "QB", "SA", "SC", "SI", "TK" };
                List<string> NCQC = new List<string>() { "JS", "NC", "QC", "QI" };
                List<string> WCVI = new List<string>() { "BK", "JA", "NW", "UT", "SK", "VI" };

                fi = new FileInfo(@"C:\DataTool\ImportByFunction\Data_inputs\BCSectorGroups_final.kml");


                if (!fi.Exists)
                {
                    richTextBoxStatus.AppendText("File not found [" + fi.FullName + "]\r\n");
                    return false;
                }

                doc = new XmlDocument();
                doc.Load(fi.FullName);

                string Sector = "";
                StartNode = doc.ChildNodes[1].ChildNodes[0].ChildNodes[4];
                int TotalSectorCount = StartNode.ChildNodes.Count;
                if (TotalSectorCount != 36)
                {
                    richTextBoxStatus.AppendText("Did not find 66 polygon in BC Sector\r\n");
                    return false;
                }
                int CountSector = 0;
                foreach (XmlNode ns in StartNode.ChildNodes)
                {
                    if (Cancel) return false;

                    CountSector += 1;

                    textBoxSectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text = CountSector.ToString();

                    lblStatus.Text = " ... CreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML";
                    lblStatus2.Text = CountSector + " of " + TotalSectorCount;
                    Application.DoEvents();

                    if (StartSectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML > CountSector)
                    {
                        continue;
                    }

                    if (ns.Name == "Folder")
                    {
                        foreach (XmlNode n in ns)
                        {
                            if (n.Name == "Placemark")
                            {
                                foreach (XmlNode n1 in n.ChildNodes)
                                {                                   
                                    if (n1.Name == "name")
                                    {
                                        Sector = n1.InnerText.Trim();
                                        Application.DoEvents();
                                        Area = Sector.Substring(0, 1);
                                        if (GBE.Contains(Sector.Substring(0, 2)))
                                        {
                                            Area = "GBE";
                                        }
                                        else if (GBW.Contains(Sector.Substring(0, 2)))
                                        {
                                            Area = "GBW";
                                        }
                                        else if (NCQC.Contains(Sector.Substring(0, 2)))
                                        {
                                            Area = "NCQC";
                                        }
                                        else if (WCVI.Contains(Sector.Substring(0, 2)))
                                        {
                                            Area = "WCVI";
                                        }
                                        else
                                        {
                                            richTextBoxStatus.AppendText("Could not find the Sector grouping [" + Sector.Substring(0, 2) + "]\r\n");
                                            return false;
                                        }
                                        TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                                        MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

                                        tvItemModelArea = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelBC.TVItemID, Area, TVTypeEnum.Area);
                                        if (!CheckModelOK<TVItemModel>(tvItemModelArea)) return false;
                                    }
                                    if (n1.Name == "Polygon")
                                    {
                                        TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                                        MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

                                        XmlNode coordNode = n1.ChildNodes[2].ChildNodes[0].ChildNodes[0];
                                        if (coordNode.Name != "coordinates")
                                        {
                                            richTextBoxStatus.AppendText("coordNode Name is not coordinates\r\n");
                                            return false;
                                        }
                                        List<Coord> coordList = new List<Coord>();
                                        string[] coordStr = coordNode.InnerText.Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                        int Ordinal = 0;
                                        foreach (string cs in coordStr)
                                        {
                                            string[] coordVal = cs.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                            if (coordVal.Count() != 3)
                                            {
                                                richTextBoxStatus.AppendText("coordVal should have 3 values\r\n");
                                                return false;
                                            }
                                            Coord c = new Coord() { Lng = float.Parse(coordVal[0]), Lat = float.Parse(coordVal[1]), Ordinal = Ordinal };
                                            coordList.Add(c);
                                            Application.DoEvents();
                                            Ordinal += 1;
                                        }
                                        string TVText = tvItemService.CleanText(Sector.Substring(0, 2));

                                        TempData.BCName bcName = (from c in dbDT.BCNames
                                                                  where c.Accronym == TVText
                                                                  select c).FirstOrDefault<TempData.BCName>();

                                        TVText += " (" + bcName.Name + ")";

                                        lblStatus.Text = " ... CreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML - Sector " + TVText;
                                        Application.DoEvents();

                                        tvItemModelSector = tvItemService.PostCreateTVItem(tvItemModelArea.TVItemID, TVText, TVText, TVTypeEnum.Sector);
                                        if (!CheckModelOK<TVItemModel>(tvItemModelSector)) return false;

                                        List<Coord> coordList2 = new List<Coord>() 
                                                            {
                                                                new Coord()
                                                                {
                                                                    Lat = (from c in coordList select c.Lat).Average(),
                                                                    Lng = (from c in coordList select c.Lng).Average(),
                                                                }
                                                            };

                                        MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.Sector, tvItemModelSector.TVItemID);
                                        if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                                        mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Polygon, TVTypeEnum.Sector, tvItemModelSector.TVItemID);
                                        if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                                    }
                                }
                            }
                        }
                    }
                }

                // ----- doing Subsectors ----

                fi = new FileInfo(@"C:\DataTool\ImportByFunction\Data_inputs\BC_Sectors_ManualFinal.kml");


                if (!fi.Exists)
                {
                    richTextBoxStatus.AppendText("File not found [" + fi.FullName + "]\r\n");
                    return false;
                }

                doc = new XmlDocument();
                doc.Load(fi.FullName);


                string Subsector = "";
                StartNode = doc.ChildNodes[1].ChildNodes[0].ChildNodes[4];
                int TotalSubsectorCount = StartNode.ChildNodes.Count;
                if (TotalSubsectorCount != 160)
                {
                    richTextBoxStatus.AppendText("Did not find 160 polygon in BC Subsector\r\n");
                    return false;
                }
                int CountSubsector = 0;
                foreach (XmlNode ns in StartNode.ChildNodes)
                {
                    if (Cancel) return false;

                    CountSubsector += 1;

                    textBoxSubsectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text = CountSubsector.ToString();

                    lblStatus.Text = " ... CreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML";
                    lblStatus2.Text = CountSubsector + " of " + TotalSubsectorCount;
                    Application.DoEvents();

                    if (StartSubsectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML > CountSubsector)
                    {
                        continue;
                    }
                    
                    if (ns.Name == "Folder")
                    {
                        foreach (XmlNode n in ns)
                        {
                            if (n.Name == "Placemark")
                            {
                                foreach (XmlNode n1 in n.ChildNodes)
                                {
                                    if (n1.Name == "name")
                                    {
                                        Subsector = n1.InnerText.Trim();
                                        Application.DoEvents();

                                    }
                                    if (n1.Name == "Polygon")
                                    {
                                        TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                                        MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
                                        TideSiteService tideSiteService = new TideSiteService(LanguageEnum.en, user);
                                        UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);

                                        Sector = Subsector.Substring(0, 2);

                                        tvItemModelSector = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelBC.TVItemID, Sector, TVTypeEnum.Sector);
                                        if (!CheckModelOK<TVItemModel>(tvItemModelSector)) return false;

                                        XmlNode coordNode = n1.ChildNodes[2].ChildNodes[0].ChildNodes[0];
                                        if (coordNode.Name != "coordinates")
                                        {
                                            richTextBoxStatus.AppendText("coordNode Name is not coordinates\r\n");
                                            return false;
                                        }
                                        List<Coord> coordList = new List<Coord>();
                                        string[] coordStr = coordNode.InnerText.Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                        int Ordinal = 0;
                                        foreach (string cs in coordStr)
                                        {
                                            string[] coordVal = cs.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                            if (coordVal.Count() != 3)
                                            {
                                                richTextBoxStatus.AppendText("coordVal should have 3 values\r\n");
                                                return false;
                                            }
                                            Coord c = new Coord() { Lng = float.Parse(coordVal[0]), Lat = float.Parse(coordVal[1]), Ordinal = Ordinal };
                                            coordList.Add(c);
                                            Application.DoEvents();
                                            Ordinal += 1;
                                        }

                                        Subsector = tvItemService.CleanText(Subsector);

                                        TempData.BCName bcName = (from c in dbDT.BCNames
                                                                  where c.Accronym == Subsector
                                                                  select c).FirstOrDefault<TempData.BCName>();

                                        Subsector += " (" + bcName.Name + ")";

                                        lblStatus.Text = " ... CreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML - Subsector " + Subsector;
                                        Application.DoEvents();

                                        tvItemModelSubsector = tvItemService.PostCreateTVItem(tvItemModelSector.TVItemID, Subsector, Subsector, TVTypeEnum.Subsector);
                                        if (!CheckModelOK<TVItemModel>(tvItemModelSubsector)) return false;

                                        List<Coord> coordList2 = new List<Coord>() 
                                                            {
                                                                new Coord()
                                                                {
                                                                    Lat = (from c in coordList select c.Lat).Average(),
                                                                    Lng = (from c in coordList select c.Lng).Average(),
                                                                }
                                                            };

                                        MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.Subsector, tvItemModelSubsector.TVItemID);
                                        if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                                        mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Polygon, TVTypeEnum.Subsector, tvItemModelSubsector.TVItemID);
                                        if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                                        // doing tidesite
                                        string TideStatNameTxt = tvItemService.CleanText("Tide Site for [" + Subsector + "]\r\n");

                                        TVItemModel tvItemModelTideSite = tvItemService.PostCreateTVItem(tvItemModelSubsector.TVItemID, TideStatNameTxt, TideStatNameTxt, TVTypeEnum.TideSite);
                                        if (!CheckModelOK<TVItemModel>(tvItemModelTideSite)) return false;

                                        TideSiteModel tideSiteModelExist = tideSiteService.GetTideSiteModelWithTideSiteTVItemIDDB(tvItemModelTideSite.TVItemID);
                                        if (!string.IsNullOrWhiteSpace(tideSiteModelExist.Error))
                                        {
                                            TideSiteModel tideSiteModelNew = new TideSiteModel();
                                            tideSiteModelNew.TideSiteTVItemID = tvItemModelTideSite.TVItemID;
                                            tideSiteModelNew.TideSiteTVText = TideStatNameTxt;
                                            tideSiteModelNew.WebTideModel = WebTideDataSetEnum.vigf8.ToString();

                                            TideSiteModel tideSiteModelRet = tideSiteService.PostAddTideSiteDB(tideSiteModelNew);
                                            if (!CheckModelOK<TideSiteModel>(tideSiteModelRet)) return false;

                                            UseOfSiteModel useOfSiteModelNew = new UseOfSiteModel();
                                            useOfSiteModelNew.SiteTVItemID = tideSiteModelRet.TideSiteTVItemID;
                                            useOfSiteModelNew.SubsectorTVItemID = tvItemModelSubsector.TVItemID;
                                            useOfSiteModelNew.Ordinal = 1;
                                            useOfSiteModelNew.UseWeight = false;
                                            useOfSiteModelNew.Weight_perc = 0;
                                            useOfSiteModelNew.UseEquation = false;
                                            useOfSiteModelNew.SiteType = SiteTypeEnum.Tide;
                                            useOfSiteModelNew.StartYear = DateTime.Now.Year;
                                            useOfSiteModelNew.EndYear = DateTime.Now.Year;

                                            UseOfSiteModel useOfSiteModel = useOfSiteService.PostAddUseOfSiteDB(useOfSiteModelNew);
                                            if (!CheckModelOK<UseOfSiteModel>(useOfSiteModel)) return false;

                                            List<MapInfoModel> mapInfoModelList = mapInfoService.GetMapInfoModelListWithTVItemIDDB(tvItemModelSubsector.TVItemID);
                                            if (mapInfoModelList.Count == 0)
                                            {
                                                List<Coord> coordList3 = new List<Coord>() 
                                                                        {
                                                                           new Coord()
                                                                            {
                                                                                Lat = (from c in coordList select c.Lat).Average(),
                                                                                Lng = (from c in coordList select c.Lng).Average(),
                                                                            }
                                                                        };

                                                MapInfoModel mapInfoModelRet2 = mapInfoService.CreateMapInfoObjectDB(coordList3, MapInfoDrawTypeEnum.Point, TVTypeEnum.TideSite, tvItemModelTideSite.TVItemID);
                                                if (!CheckModelOK<MapInfoModel>(mapInfoModelRet2)) return false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
