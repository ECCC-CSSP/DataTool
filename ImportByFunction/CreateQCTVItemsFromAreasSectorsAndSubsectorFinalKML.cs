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
        public bool CreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML()
        {
            lblStatus.Text = "Starting ... CreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML";
            Application.DoEvents();

            if (Cancel) return false;

            int StartAreaCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML = int.Parse(textBoxAreaCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text);
            int StartSectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML = int.Parse(textBoxSectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text);
            int StartSubsectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML = int.Parse(textBoxSubsectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text);

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelArea = new TVItemModel();
            TVItemModel tvItemModelSector = new TVItemModel();
            TVItemModel tvItemModelSubsector = new TVItemModel();
            TVItemModel tvItemModelQC = new TVItemModel();

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            tvItemModelQC = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelQC)) return false;

            List<TempData.QCSubSector> tempQCSubSectorList = new List<TempData.QCSubSector>();

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                tempQCSubSectorList = (from c in dbDT.QCSubSectors
                                       select c).ToList<TempData.QCSubSector>();
            }

            List<SubSector> SubsectorList = new List<SubSector>();


            // ----- doing Areas ----

            FileInfo fi = new FileInfo(@"C:\DataTool\ImportByFunction\Data_inputs\QC_Areas_Final.kml");


            if (!fi.Exists)
            {
                richTextBoxStatus.AppendText("File not found [" + fi.FullName + "]\r\n");
                return false;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(fi.FullName);

            string Area = "";
            XmlNode StartNode = doc.ChildNodes[1].ChildNodes[0].ChildNodes[4];
            int TotalAreaCount = StartNode.ChildNodes.Count;
            int CountArea = 0;
            if (TotalAreaCount != 12)
            {
                richTextBoxStatus.AppendText("Did not find 12 polygon in QC Area\r\n");
                return false;
            }
            foreach (XmlNode n in StartNode.ChildNodes)
            {
                if (Cancel) return false;

                CountArea += 1;

                textBoxAreaCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text = CountArea.ToString();

                lblStatus.Text = " ... CreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML";
                lblStatus2.Text = CountArea + " of " + TotalAreaCount;
                Application.DoEvents();

                if (StartAreaCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML > CountArea)
                {
                    continue;
                }

                if (n.Name == "Placemark")
                {
                    foreach (XmlNode n1 in n.ChildNodes)
                    {
                        if (n1.Name == "name")
                        {
                            Area = n1.InnerText;
                            Application.DoEvents();
                        }
                        if (n1.Name == "Polygon")
                        {
                            XmlNode coordNode = n1.ChildNodes[0].ChildNodes[0].ChildNodes[0];
                            if (coordNode == null)
                            {
                                richTextBoxStatus.AppendText("coordNode should not be Null for [" + Area + "]\r\n");
                                return false;
                            }
                            if (coordNode.Name != "coordinates")
                            {
                                richTextBoxStatus.AppendText("coordNode Name is not coordinates");
                                return false;
                            }
                            List<Coord> coordList = new List<Coord>();
                            string[] coordStr = coordNode.InnerText.Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            int Ordinal = 0;
                            foreach (string cs in coordStr)
                            {
                                string[] coordVal = cs.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                Coord c = new Coord() { Lng = float.Parse(coordVal[0]), Lat = float.Parse(coordVal[1]), Ordinal = Ordinal };
                                coordList.Add(c);
                                Application.DoEvents();
                                Ordinal += 1;
                            }


                            string DescEN = (from c in tempQCSubSectorList
                                             where c.QCSubSectorLetter == Area
                                             select c.QCSubSectorTextEN).FirstOrDefault<string>();

                            string DescFR = (from c in tempQCSubSectorList
                                             where c.QCSubSectorLetter == Area
                                             select c.QCSubSectorTextFR).FirstOrDefault<string>();

                            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

                            string AreaCleanTextEN = tvItemService.CleanText(Area + (string.IsNullOrEmpty(DescEN) ? " (vide)" : " (" + DescEN + ")")) + " ";
                            string AreaCleanTextFR = tvItemService.CleanText(Area + (string.IsNullOrEmpty(DescFR) ? " (vide)" : " (" + DescFR + ")")) + " ";

                            lblStatus.Text = " ... CreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML - Area " + AreaCleanTextEN;
                            Application.DoEvents();

                            tvItemModelArea = tvItemService.PostCreateTVItem(tvItemModelQC.TVItemID, AreaCleanTextEN, AreaCleanTextFR, TVTypeEnum.Area);
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

            // ----- doing Sectors ----

            fi = new FileInfo(@"C:\DataTool\ImportByFunction\Data_inputs\QC_Sectors_Final.kml");


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
            int CountSector = 0;
            if (TotalSectorCount != 119)
            {
                richTextBoxStatus.AppendText("Did not find 119 polygon in QC Sector\r\n");
                return false;
            }
            foreach (XmlNode n in StartNode.ChildNodes)
            {
                if (Cancel) return false;

                CountSector += 1;

                textBoxSectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text = CountSector.ToString();

                lblStatus.Text = " ... CreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML";
                lblStatus2.Text = CountSector + " of " + TotalSectorCount;
                Application.DoEvents();

                if (StartSectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML > CountSector)
                {
                    continue;
                }

                lblStatus.Text = " ... CreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML";
                lblStatus2.Text = CountSector + " of " + TotalSectorCount;
                Application.DoEvents();

                if (n.Name == "Placemark")
                {
                    foreach (XmlNode n1 in n.ChildNodes)
                    {
                        if (n1.Name == "name")
                        {
                            Sector = n1.InnerText.Trim();
                            Application.DoEvents();
                            Area = Sector.Substring(0, 1);

                            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                            tvItemModelArea = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelQC.TVItemID, Area, TVTypeEnum.Area);
                            if (!CheckModelOK<TVItemModel>(tvItemModelArea)) return false;
                                 
                        }
                        if (n1.Name == "Polygon")
                        {
                            XmlNode coordNode = n1.ChildNodes[0].ChildNodes[0].ChildNodes[0];
                            if (coordNode == null)
                            {
                                richTextBoxStatus.AppendText("coordNode should not be Null for [" + Sector + "]\r\n");
                                return false;
                            }
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
                                Coord c = new Coord() { Lng = float.Parse(coordVal[0]), Lat = float.Parse(coordVal[1]), Ordinal = Ordinal };
                                coordList.Add(c);
                                Application.DoEvents();
                                Ordinal += 1;
                            }


                            string DescEN = (from c in tempQCSubSectorList
                                             where c.QCSubSectorLetter == Sector
                                             select c.QCSubSectorTextEN).FirstOrDefault<string>();

                            string DescFR = (from c in tempQCSubSectorList
                                             where c.QCSubSectorLetter == Sector
                                             select c.QCSubSectorTextFR).FirstOrDefault<string>();


                            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

                            string TextEN = tvItemService.CleanText(Sector + (string.IsNullOrEmpty(DescEN) ? " (vide)" : " (" + DescEN + ")")) + " ";
                            string TextFR = tvItemService.CleanText(Sector + (string.IsNullOrEmpty(DescFR) ? " (vide)" : " (" + DescFR + ")")) + " ";

                            lblStatus.Text = " ... CreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML - Sector " + TextEN;
                            Application.DoEvents();

                            tvItemModelSector = tvItemService.PostCreateTVItem(tvItemModelArea.TVItemID, TextEN, TextFR, TVTypeEnum.Sector);
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
                //richTextBoxStatus.AppendText(n.Name + "\r\n");
            }

            // ----- doing Subsectors ----

            fi = new FileInfo(@"C:\DataTool\ImportByFunction\Data_inputs\QC_Subsectors_Final.kml");


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
            int CountSubsector = 0;
            if (TotalSubsectorCount != 356)
            {
                richTextBoxStatus.AppendText("Did not find 356 polygon in QC Subsector\r\n");
                return false;
            }
            foreach (XmlNode n in StartNode.ChildNodes)
            {
                if (Cancel) return false;

                CountSubsector += 1;

                textBoxSubsectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text = CountSubsector.ToString();

                lblStatus.Text = " ... CreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML";
                lblStatus2.Text = CountSubsector + " of " + TotalSubsectorCount;
                Application.DoEvents();

                if (StartSubsectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML > CountSubsector)
                {
                    continue;
                }

                if (n.Name == "Placemark")
                {
                    foreach (XmlNode n1 in n.ChildNodes)
                    {
                        if (n1.Name == "name")
                        {
                            Subsector = n1.InnerText.Trim();
                            Application.DoEvents();
                            Sector = Subsector.Substring(0, 4);

                            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                            tvItemModelSector = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelQC.TVItemID, Sector, TVTypeEnum.Sector);
                            if (!CheckModelOK<TVItemModel>(tvItemModelSector)) return false;

                        }
                        if (n1.Name == "Polygon")
                        {
                            XmlNode coordNode = n1.ChildNodes[0].ChildNodes[0].ChildNodes[0];
                            if (coordNode == null)
                            {
                                richTextBoxStatus.AppendText("coordNode should not be Null for [" + Subsector + "]\r\n");
                                return false;
                            }
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
                                Coord c = new Coord() { Lng = float.Parse(coordVal[0]), Lat = float.Parse(coordVal[1]), Ordinal = Ordinal };
                                coordList.Add(c);
                                Application.DoEvents();
                                Ordinal += 1;
                            }


                            string DescEN = (from c in tempQCSubSectorList
                                             where c.QCSubSectorLetter == Subsector
                                             select c.QCSubSectorTextEN).FirstOrDefault<string>();

                            string DescFR = (from c in tempQCSubSectorList
                                             where c.QCSubSectorLetter == Subsector
                                             select c.QCSubSectorTextFR).FirstOrDefault<string>();

                            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
                            TideSiteService tideSiteService = new TideSiteService(LanguageEnum.en, user);
                            UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);

                            string SubsectorCleanTextEN = tvItemService.CleanText(Subsector + (string.IsNullOrEmpty(DescEN) ? " (vide)" : " (" + DescEN + ")")) + " ";
                            string SubsectorCleanTextFR = tvItemService.CleanText(Subsector + (string.IsNullOrEmpty(DescFR) ? " (vide)" : " (" + DescFR + ")")) + " ";

                            lblStatus.Text = " ... CreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML - Subsector " + SubsectorCleanTextEN;
                            Application.DoEvents();

                            tvItemModelSubsector = tvItemService.PostCreateTVItem(tvItemModelSector.TVItemID, SubsectorCleanTextEN, SubsectorCleanTextFR, TVTypeEnum.Subsector);
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
                            string TideStatNameTxt = tvItemService.CleanText("Tide Site for [" + SubsectorCleanTextEN + "]\r\n");

                            TVItemModel tvItemModelTideSite = tvItemService.PostCreateTVItem(tvItemModelSubsector.TVItemID, TideStatNameTxt, TideStatNameTxt, TVTypeEnum.TideSite);
                            if (!CheckModelOK<TVItemModel>(tvItemModelTideSite)) return false;

                            TideSiteModel tideSiteModelExist = tideSiteService.GetTideSiteModelWithTideSiteTVItemIDDB(tvItemModelTideSite.TVItemID);
                            if (!string.IsNullOrWhiteSpace(tideSiteModelExist.Error))
                            {
                                TideSiteModel tideSiteModelNew = new TideSiteModel();
                                tideSiteModelNew.TideSiteTVItemID = tvItemModelTideSite.TVItemID;
                                tideSiteModelNew.TideSiteTVText = (TideStatNameTxt.Length > 99 ? TideStatNameTxt.Substring(0, 98) : TideStatNameTxt);
                                tideSiteModelNew.WebTideModel = WebTideDataSetEnum.nwatl.ToString();

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
                //richTextBoxStatus.AppendText(n.Name + "\r\n");
            }

            return true;
        }
    }
}
