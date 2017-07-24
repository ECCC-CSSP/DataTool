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
        public bool CreateMapInfosFromAreas_Sectors_SubsectorsKML()
        {
            lblStatus.Text = "Starting ... CreateMapInfosFromAreas_Sectors_SubsectorsKML";
            Application.DoEvents();

            if (Cancel) return false;

            int StartSubsectorCreateMapInfosFromAreas_Sectors_SubsectorsKML = int.Parse(textBoxSubsectorCreateMapInfosFromAreas_Sectors_SubsectorsKML.Text);

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            lblStatus.Text = " ... CreateMapInfosFromAreas_Sectors_SubsectorsKML";
            Application.DoEvents();

            FileInfo fi = new FileInfo(@"C:\DataTool\ImportByFunction\Data_inputs\Areas_Sectors_SubsectorsFinal.kml");

            if (!fi.Exists)
            {
                richTextBoxStatus.AppendText("File not found [" + fi.FullName + "]\r\n");
                return false;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(fi.FullName);

            List<SubSector> PolList = new List<SubSector>();
            string Prov = "";
            string PolType = "";
            string PolName = "";
            XmlNode StartNode = doc.ChildNodes[1].ChildNodes[0].ChildNodes[5];
            int TotalCount = StartNode.ChildNodes.Count;
            if (TotalCount != 6)
            {
                richTextBoxStatus.AppendText("Total Count was not 6");
                return false;
            }
            int Count = 0;
            foreach (XmlNode n in StartNode.ChildNodes)
            {
                if (Cancel) return false;

                //richTextBoxStatus.AppendText(n.Name + "\r\n");
                if (n.Name == "Folder")
                {
                    foreach (XmlNode n1 in n)
                    {
                        if (Cancel) return false;

                        //richTextBoxStatus.AppendText("\t" + n1.Name + "\r\n");
                        if (n1.Name == "name")
                        {
                            Prov = n1.InnerText;
                            //richTextBoxStatus.AppendText("\t" + n1.InnerText + "\r\n");
                        }
                        else if (n1.Name == "Folder")
                        {
                            foreach (XmlNode n2 in n1)
                            {
                                if (Cancel) return false;

                                if (n2.Name == "name")
                                {
                                    PolType = n2.InnerText;
                                    //richTextBoxStatus.AppendText("\t\t\t" + n2.InnerText + "\r\n");
                                }
                                else if (n2.Name == "Placemark")
                                {
                                    foreach (XmlNode n3 in n2)
                                    {
                                        if (Cancel) return false;

                                        if (n3.Name == "name")
                                        {
                                            PolName = n3.InnerText;
                                            lblStatus.Text = "Starting ... CreateMapInfosFromAreas_Sectors_SubsectorsKML " + n3.InnerText;
                                            Application.DoEvents();

                                            //richTextBoxStatus.AppendText("\t\t\t" + n3.InnerText + "\r\n");
                                        }
                                        else if (n3.Name == "Polygon")
                                        {
                                            Application.DoEvents();

                                            List<Coord> coordList = new List<Coord>();
                                            string PolText = n3.ChildNodes[0].ChildNodes[0].ChildNodes[0].InnerText.Trim().Replace("\r\n", "");

                                            string[] PolTextArr = PolText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                            //richTextBoxStatus.AppendText(Prov + "\t" + PolType + "\t" + PolName + "\r\n" + PolText + "\r\n");
                                            int countElem = 0;
                                            foreach (string s in PolTextArr)
                                            {
                                                if (Cancel) return false;

                                                string[] coordVal = s.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                                if (coordVal.Count() != 3)
                                                {
                                                    richTextBoxStatus.AppendText("coordVal.Count() should be equal to 3. It's [" + coordVal.Count() + "]\r\n");
                                                    return false;
                                                }
                                                coordList.Add(new Coord() { Lng = float.Parse(coordVal[0]), Lat = float.Parse(coordVal[1]), Ordinal = countElem });
                                                countElem += 1;
                                                //richTextBoxStatus.AppendText(s + " ");
                                            }
                                            //richTextBoxStatus.AppendText("\r\n");

                                            StringBuilder sb = new StringBuilder();
                                            sb.Append("POLYGON ((");
                                            int countNode = 0;
                                            foreach (Coord c in coordList)
                                            {
                                                if (countNode == 0)
                                                {
                                                    sb.Append(c.Lng + " " + c.Lat);
                                                }
                                                else
                                                {
                                                    sb.Append(", " + c.Lng + " " + c.Lat);
                                                }
                                                countNode += 1;
                                            }
                                            sb.Append(", " + coordList[0].Lng + " " + coordList[0].Lat);
                                            sb.Append("))");

                                            if (Cancel) return false;

                                            string ProvTxt = "";
                                            if (Prov == "NB")
                                            {
                                                ProvTxt = "New Brunswick";
                                            }
                                            else if (Prov == "NS")
                                            {
                                                ProvTxt = "Nova Scotia";
                                            }
                                            else if (Prov == "PE")
                                            {
                                                ProvTxt = "Prince Edward Island";
                                            }
                                            else if (Prov == "NL")
                                            {
                                                ProvTxt = "Newfoundland and Labrador";
                                            }

                                            if (ProvTxt == "")
                                            {
                                                richTextBoxStatus.AppendText("ProvTxt should not be empty\r\n");
                                                return false;
                                            }

                                            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                                            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

                                            TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, ProvTxt, TVTypeEnum.Province);
                                            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return false;

                                            if (PolType == "Areas" || PolType == "Sectors" || PolType == "Subsectors")
                                            {
                                                string areaTxt = PolName.Substring(0, 5);
                                                TVItemModel tvItemModelArea = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelProv.TVItemID, areaTxt, TVTypeEnum.Area);
                                                if (!CheckModelOK<TVItemModel>(tvItemModelArea)) return false;

                                                lblStatus.Text = "Starting ... CreateMapInfosFromAreas_Sectors_SubsectorsKML - Area " + areaTxt;
                                                Application.DoEvents();

                                                if (PolType == "Areas")
                                                {
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
                                                else
                                                {
                                                    if (PolType == "Sectors" || PolType == "Subsectors")
                                                    {
                                                        string sectorTxt = PolName.Substring(0, 9);
                                                        TVItemModel tvItemModelSector = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelArea.TVItemID, sectorTxt, TVTypeEnum.Sector);
                                                        if (!CheckModelOK<TVItemModel>(tvItemModelSector)) return false;

                                                        lblStatus.Text = "Starting ... CreateMapInfosFromAreas_Sectors_SubsectorsKML - Sector " + sectorTxt;
                                                        Application.DoEvents();

                                                        if (PolType == "Sectors")
                                                        {
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
                                                        else
                                                        {
                                                            if (PolType == "Subsectors")
                                                            {
                                                                Count += 1;

                                                                string subsectorTxt = PolName.Substring(0, 13);
                                                                TVItemModel tvItemModelSubsector = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelSector.TVItemID, subsectorTxt, TVTypeEnum.Subsector);
                                                                if (!CheckModelOK<TVItemModel>(tvItemModelSubsector)) return false;

                                                                lblStatus.Text = "Starting ... CreateMapInfosFromAreas_Sectors_SubsectorsKML - Subsector " + subsectorTxt;
                                                                lblStatus2.Text = Count.ToString();
                                                                textBoxSubsectorCreateMapInfosFromAreas_Sectors_SubsectorsKML.Text = Count.ToString();
                                                                Application.DoEvents();

                                                                List<Coord> coordList2 = new List<Coord>() 
                                                                {
                                                                   new Coord()
                                                                    {
                                                                        Lat = (from c in coordList select c.Lat).Average(),
                                                                        Lng = (from c in coordList select c.Lng).Average(),
                                                                    }
                                                                };

                                                                TideSiteService tideSiteService = new TideSiteService(LanguageEnum.en, user);
                                                                UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);

                                                                MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.Subsector, tvItemModelSubsector.TVItemID);
                                                                if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                                                                mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Polygon, TVTypeEnum.Subsector, tvItemModelSubsector.TVItemID);
                                                                if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                                                                // doing tidesite
                                                                string TideStatNameTxt = tvItemService.CleanText("Tide Site for [" + subsectorTxt + "]\r\n");

                                                                TVItemModel tvItemModelTideSite = tvItemService.PostCreateTVItem(tvItemModelSubsector.TVItemID, TideStatNameTxt, TideStatNameTxt, TVTypeEnum.TideSite);
                                                                if (!CheckModelOK<TVItemModel>(tvItemModelTideSite)) return false;

                                                                TideSiteModel tideSiteModelExist = tideSiteService.GetTideSiteModelWithTideSiteTVItemIDDB(tvItemModelTideSite.TVItemID);
                                                                if (!string.IsNullOrWhiteSpace(tideSiteModelExist.Error))
                                                                {
                                                                    TideSiteModel tideSiteModelNew = new TideSiteModel();
                                                                    tideSiteModelNew.TideSiteTVItemID = tvItemModelTideSite.TVItemID;
                                                                    tideSiteModelNew.TideSiteTVText = TideStatNameTxt;
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

                                                                    List<MapInfoModel> mapInfoModelList = mapInfoService.GetMapInfoModelListWithTVItemIDDB(tvItemModelTideSite.TVItemID);
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
                                                            else
                                                            {
                                                                richTextBoxStatus.AppendText("PolType should be one of Areas, Sectors or Subsectors\r\n");
                                                                return false;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        richTextBoxStatus.AppendText("PolType should be one of Areas, Sectors or Subsectors\r\n");
                                                        return false;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                richTextBoxStatus.AppendText("PolType should be one of Areas, Sectors or Subsectors\r\n");
                                                return false;
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
