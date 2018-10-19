using CSSPDBDLL;
using CSSPModelsDLL.Models;
using CSSPDBDLL.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSSPEnumsDLL.Enums;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CopyFileToNewPathIfNotAlreadyThere(string ServerFileName, string ServerFilePath, string NewServerFilePath)
        {
            try
            {
                // checking if file exist
                FileInfo fi = new FileInfo((ServerFilePath + @"\" + ServerFileName).Replace(@"\\", @"\"));

                // E:\inetpub\wwwroot\csspapps\App_Data\32984\32986\35909\35910\35928\35929\38756\Result\Gaspe_deb_SP08cont_R4_20150619_HAE_vent_40kmh_NO.m21fm - Result Files\Hydro.dfsu
                if (!fi.Exists)
                {
                    return false;
                }

                DirectoryInfo di = new DirectoryInfo(NewServerFilePath);
                if (!di.Exists)
                {
                    di.Create();
                }

                FileInfo fiNew = new FileInfo((di.FullName + @"\" + ServerFileName).Replace(@"\\", @"\"));

                if (!fiNew.Exists)
                {
                    File.Copy((fi.FullName).Replace(@"\\", @"\"), (di.FullName + @"\" + ServerFileName).Replace(@"\\", @"\"), false);
                }

            }
            catch (Exception ex)
            {
                string selijfl = ex.Message;
            }

            return true;
        }
        public bool CreateBoxModels(int InfrastructureTVItemID, string OldItemPath)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            BoxModelService boxModelService = new BoxModelService(LanguageEnum.en, user);
            BoxModelResultService boxModelResultService = new BoxModelResultService(LanguageEnum.en, user);

            using (OldCSSPApps.OldCSSPAppDB2Entities oldCSSPAppDB2 = new OldCSSPApps.OldCSSPAppDB2Entities())
            {
                int Level = GetLevel(OldItemPath.Replace("\\", "p")) + 1;
                List<OldCSSPApps.BoxModel> boxModelList = (from c in oldCSSPAppDB2.BoxModels
                                                           from cp in oldCSSPAppDB2.CSSPItemPaths
                                                           where c.CSSPItemID == cp.CSSPItemID
                                                           && cp.Path.StartsWith(OldItemPath)
                                                           && cp.Level == Level
                                                           select c).ToList<OldCSSPApps.BoxModel>();

                int CountBM = boxModelList.Count();
                int Count = 0;
                foreach (OldCSSPApps.BoxModel bm in boxModelList)
                {

                    Count += 1;
                    TVItemModel tvItemModelInfrastructure = tvItemService.GetTVItemModelWithTVItemIDDB(InfrastructureTVItemID);
                    if (!CheckModelOK<TVItemModel>(tvItemModelInfrastructure)) return false;

                    Application.DoEvents();

                    BoxModelModel boxModelModelNew = new BoxModelModel();
                    boxModelModelNew.InfrastructureTVItemID = InfrastructureTVItemID;
                    boxModelModelNew.Flow_m3_day = (double)bm.Flow;
                    boxModelModelNew.Depth_m = (double)bm.Depth;
                    boxModelModelNew.Temperature_C = (double)bm.Temperature;
                    boxModelModelNew.Dilution = (int)bm.Dilution;
                    boxModelModelNew.DecayRate_per_day = (double)bm.DecayRate;
                    boxModelModelNew.FCUntreated_MPN_100ml = (int)bm.FCUntreated;
                    boxModelModelNew.FCPreDisinfection_MPN_100ml = (int)bm.FCPreDisinfection;
                    boxModelModelNew.Concentration_MPN_100ml = (int)bm.Concentration;
                    boxModelModelNew.T90_hour = (double)bm.T90;
                    boxModelModelNew.FlowDuration_hour = (double)bm.FlowDuration;
                    boxModelModelNew.ScenarioName = bm.ScenarioName;

                    BoxModelModel boxModelModelRet = boxModelService.GetBoxModelModelWithInfrastructureTVItemIDAndScenarioNameDB(InfrastructureTVItemID, bm.ScenarioName);
                    if (!string.IsNullOrWhiteSpace(boxModelModelRet.Error))
                    {
                        boxModelModelRet = boxModelService.PostAddBoxModelDB(boxModelModelNew);
                        if (!CheckModelOK<BoxModelModel>(boxModelModelRet)) return false;
                    }

                    List<OldCSSPApps.BoxModelResult> boxModelResultList = (from c in oldCSSPAppDB2.BoxModelResults
                                                                           where c.BoxModelID == bm.BoxModelID
                                                                           select c).ToList<OldCSSPApps.BoxModelResult>();

                    foreach (OldCSSPApps.BoxModelResult bmr in boxModelResultList)
                    {
                        BoxModelResultModel boxModelResultModelNew = new BoxModelResultModel();
                        boxModelResultModelNew.BoxModelResultType = (BoxModelResultTypeEnum)(bmr.ResultType);
                        boxModelResultModelNew.Volume_m3 = (double)bmr.Volume;
                        boxModelResultModelNew.Surface_m2 = (double)bmr.Surface;
                        boxModelResultModelNew.Radius_m = (double)bmr.Radius;
                        boxModelResultModelNew.LeftSideDiameterLineAngle_deg = (double)bmr.LeftSideDiameterLineAngle;
                        boxModelResultModelNew.CircleCenterLatitude = (double)bmr.CircleCenterLatitude;
                        boxModelResultModelNew.CircleCenterLongitude = (double)bmr.CircleCenterLongitude;
                        boxModelResultModelNew.FixLength = (bool)bmr.FixLength;
                        boxModelResultModelNew.FixWidth = (bool)bmr.FixWidth;
                        boxModelResultModelNew.RectLength_m = (double)bmr.RectLength;
                        boxModelResultModelNew.RectWidth_m = (double)bmr.RectWidth;
                        boxModelResultModelNew.LeftSideLineAngle_deg = (double)bmr.LeftSideLineAngle;
                        boxModelResultModelNew.LeftSideLineStartLatitude = (double)bmr.LeftSideLineStartLatitude;
                        boxModelResultModelNew.LeftSideLineStartLongitude = (double)bmr.LeftSideLineStartLongitude;
                        boxModelResultModelNew.BoxModelID = boxModelModelRet.BoxModelID;

                        BoxModelResultModel boxModelResultModelRet = boxModelResultService.PostAddBoxModelResultDB(boxModelResultModelNew);
                        if (!CheckModelOK<BoxModelResultModel>(boxModelResultModelRet)) return false;
                    }
                }
            }

            Application.DoEvents();

            return true;
        }
        public bool CreateInfrastructures(int InfrastructureTVItemID, string OldItemPath, string NextType)
        {
            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            using (OldCSSPApps.OldCSSPAppDB2Entities oldCSSPAppDB2 = new OldCSSPApps.OldCSSPAppDB2Entities())
            {
                int LevelOld = GetLevel(OldItemPath.Replace("\\", "p")) + 1;
                List<OldCSSPApps.CSSPInfrastructure> infrastructureList = (from c in oldCSSPAppDB2.CSSPInfrastructures
                                                                           from cp in oldCSSPAppDB2.CSSPItemPaths
                                                                           where c.CSSPItemID == cp.CSSPItemID
                                                                           && cp.Path.StartsWith(OldItemPath)
                                                                           && cp.Level == LevelOld
                                                                           select c).ToList<OldCSSPApps.CSSPInfrastructure>();

                int CountInf = infrastructureList.Count();
                int Count = 0;
                foreach (OldCSSPApps.CSSPInfrastructure inf in infrastructureList)
                {
                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                    InfrastructureService infrastructureService = new InfrastructureService(LanguageEnum.en, user);
                    MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

                    Count += 1;
                    TVItemModel tvItemModelInfrastructure = tvItemService.GetTVItemModelWithTVItemIDDB(InfrastructureTVItemID);
                    if (!CheckModelOK<TVItemModel>(tvItemModelInfrastructure)) return false;

                    Application.DoEvents();

                    InfrastructureModel infrastructureModelNew = new InfrastructureModel();
                    infrastructureModelNew.InfrastructureTVItemID = InfrastructureTVItemID;

                    string TVText = "";

                    if (!string.IsNullOrWhiteSpace(inf.Category))
                    {
                        TVText = tvItemService.CleanText(inf.Category);

                        //TVItemModel tvItemModelInfCategory = tvItemService.PostCreateTVItem(tvItemModelRoot.TVItemID, TVText, TVText, TVTypeEnum.InfrastructureCategory);
                        //if (!CheckModelOK<TVItemModel>(tvItemModelInfCategory)) return false;
                        infrastructureModelNew.InfrastructureCategory = TVText.Substring(0, 1);
                    }

                    infrastructureModelNew.PrismID = inf.PrismID;
                    infrastructureModelNew.TPID = inf.TPID;
                    infrastructureModelNew.LSID = inf.LSID;
                    if (inf.SiteID == 0)
                    {
                        inf.SiteID = null;
                    }
                    infrastructureModelNew.SiteID = inf.SiteID;
                    infrastructureModelNew.Site = inf.Site;

                    if (!string.IsNullOrWhiteSpace(inf.InfrastructureType))
                    {
                        if (NextType == "wwtp")
                        {
                            TVText = tvItemService.CleanText("WWTP");
                            infrastructureModelNew.InfrastructureType = InfrastructureTypeEnum.WWTP;
                        }
                        else
                        {
                            TVText = tvItemService.CleanText("Lift Station");
                            infrastructureModelNew.InfrastructureType = InfrastructureTypeEnum.LiftStation;
                        }
                    }

                    infrastructureModelNew.TreatmentType = null;
                    infrastructureModelNew.DisinfectionType = null;
                    infrastructureModelNew.CollectionSystemType = null;
                    infrastructureModelNew.AlarmSystemType = null;


                    string TempString = "               Year of assessment:\t" + (inf.YearOfAssessment == null ? "[]" : inf.YearOfAssessment.ToString()) + "\r\n";
                    TempString += "              Design flow in m3/d:\t" + (inf.DesignFlow == null ? "[]" : inf.DesignFlow.ToString()) + "\r\n";
                    TempString += "             Average flow in m3/d:\t" + (inf.AverageFlow == null ? "[]" : inf.AverageFlow.ToString()) + "\r\n";
                    TempString += "                Peak flow in m3/d:\t" + (inf.PeakFlow == null ? "[]" : inf.PeakFlow.ToString()) + "\r\n";
                    TempString += "           Estimated flow in m3/d:\t" + (inf.EstimatedFlow == null ? "[]" : inf.EstimatedFlow.ToString()) + "\r\n";
                    TempString += "             Date of construction:\t" + (inf.DateOfConstruction == null ? "[]" : inf.DateOfConstruction.ToString()) + "\r\n";
                    TempString += "           Date of recent upgrade:\t" + (inf.DateOfRecentUpgrade == null ? "[]" : inf.DateOfRecentUpgrade.ToString()) + "\r\n";
                    TempString += "Number of visit to plant per week:\t" + (inf.NumbOfVisitToPlantPerWeek == null ? "[]" : inf.NumbOfVisitToPlantPerWeek.ToString()) + "\r\n";
                    TempString += "                 Has alarm system:\t" + (inf.HasAlarmSystem == null ? "[]" : inf.HasAlarmSystem.ToString()) + "\r\n";
                    TempString += "              Combined percentage:\t" + (inf.CombinedPercent == null ? "[]" : inf.CombinedPercent.ToString()) + "\r\n";
                    TempString += "\r\n";
                    TempString += "\r\n";
                    TempString += "Please add the contact info in the system\r\n";
                    TempString += "\r\n";
                    TempString += " Operator name:\t" + (inf.OperatorName == null ? "[]" : inf.OperatorName.ToString()) + "\r\n";
                    TempString += "  Operator tel:\t" + (inf.OperatorTelephone == null ? "[]" : inf.OperatorTelephone.ToString()) + "\r\n";
                    TempString += "Operator email:\t" + (inf.OperatorEmail == null ? "[]" : inf.OperatorEmail.ToString()) + "\r\n";
                    TempString += "Infrastructure type:\t" + (inf.InfrastructureType == null ? "[]" : inf.InfrastructureType.ToString()) + "\r\n";

                    infrastructureModelNew.TempCatchAllRemoveLater = TempString;
                    infrastructureModelNew.PopServed = inf.PopServed;
                    if (infrastructureModelNew.PopServed == 0)
                    {
                        infrastructureModelNew.PopServed = null;
                    }
                    infrastructureModelNew.Comment = inf.Comments + (string.IsNullOrWhiteSpace(inf.InputDataComments) ? "" : "\r\nInputDataComment: " + inf.InputDataComments);
                    infrastructureModelNew.TimeOffset_hour = inf.TimeZone;
                    infrastructureModelNew.InfrastructureTVText = tvItemModelInfrastructure.TVText;

                    if (string.IsNullOrWhiteSpace(infrastructureModelNew.Comment))
                    {
                        infrastructureModelNew.Comment = "empty";
                    }

                    InfrastructureModel infrastructureModelRet = infrastructureService.GetInfrastructureModelWithInfrastructureTVItemIDDB(InfrastructureTVItemID);
                    if (!string.IsNullOrWhiteSpace(infrastructureModelRet.Error))
                    {
                        infrastructureModelRet = infrastructureService.PostAddInfrastructureDB(infrastructureModelNew);
                        if (!CheckModelOK<InfrastructureModel>(infrastructureModelRet)) return false;

                        if (inf.Latitude == null || inf.Latitude == 0.0D || inf.Longitude == null || inf.Longitude == 0.0D)
                        {
                            TVItemModel tvItemModelSubsector = tvItemService.GetParentTVItemModelListWithTVItemIDForLocationDB(InfrastructureTVItemID).Where(c => c.TVType == TVTypeEnum.Subsector).FirstOrDefault();
                            if (!CheckModelOK<TVItemModel>(tvItemModelSubsector)) return false;

                            MapInfoModel mapInfoModelSubsector = mapInfoService.GetMapInfoModelListWithTVItemIDDB(tvItemModelSubsector.TVItemID).Where(c => c.MapInfoDrawType == MapInfoDrawTypeEnum.Polygon).FirstOrDefault();
                            if (!CheckModelOK<MapInfoModel>(mapInfoModelSubsector)) return false;

                            if (inf.Latitude == null || inf.Latitude == 0.0D)
                            {
                                inf.Latitude = mapInfoModelSubsector.LatMax;
                            }
                            if (inf.Longitude == null || inf.Longitude == 0.0D)
                            {
                                inf.Longitude = mapInfoModelSubsector.LngMax;
                            }
                        }

                        List<Coord> coordList = new List<Coord>()
                        {
                            new Coord()
                            {
                                Lat = (float)inf.Latitude,
                                Lng = (float)inf.Longitude,
                            }
                        };

                        MapInfoModel mapInfoModelRet = new MapInfoModel();

                        if (NextType == "wwtp")
                        {
                            mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.WasteWaterTreatmentPlant, InfrastructureTVItemID);
                            if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;
                        }
                        else if (NextType == "liftstation")
                        {
                            mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.LiftStation, InfrastructureTVItemID);
                            if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;
                        }

                        if (inf.OutfallLatitude == null || inf.OutfallLatitude == 0.0D || inf.OutfallLongitude == null || inf.OutfallLongitude == 0.0D)
                        {
                            TVItemModel tvItemModelSubsector = tvItemService.GetParentTVItemModelListWithTVItemIDForLocationDB(InfrastructureTVItemID).Where(c => c.TVType == TVTypeEnum.Subsector).FirstOrDefault();
                            if (!CheckModelOK<TVItemModel>(tvItemModelSubsector)) return false;

                            MapInfoModel mapInfoModelSubsector = mapInfoService.GetMapInfoModelListWithTVItemIDDB(tvItemModelSubsector.TVItemID).Where(c => c.MapInfoDrawType == MapInfoDrawTypeEnum.Polygon).FirstOrDefault();
                            if (!CheckModelOK<MapInfoModel>(mapInfoModelSubsector)) return false;

                            if (inf.OutfallLatitude == null || inf.OutfallLatitude == 0.0D)
                            {
                                inf.OutfallLatitude = mapInfoModelSubsector.LatMax;
                            }
                            if (inf.OutfallLongitude == null || inf.OutfallLongitude == 0.0D)
                            {
                                inf.OutfallLongitude = mapInfoModelSubsector.LngMax;
                            }
                        }

                        coordList = new List<Coord>()
                        {
                            new Coord()
                            {
                                Lat = (float)inf.OutfallLatitude,
                                Lng = (float)inf.OutfallLongitude,
                            }
                        };

                        mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Outfall, InfrastructureTVItemID);
                        if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                    }
                    else
                    {
                        infrastructureModelRet = infrastructureService.GetInfrastructureModelWithInfrastructureTVItemIDDB(infrastructureModelNew.InfrastructureTVItemID);
                        if (!string.IsNullOrWhiteSpace(infrastructureModelRet.Error))
                        {
                            infrastructureModelRet = infrastructureService.PostAddInfrastructureDB(infrastructureModelNew);
                            if (!CheckModelOK<InfrastructureModel>(infrastructureModelRet)) return false;

                            if (inf.Latitude == null || inf.Latitude == 0.0D || inf.Longitude == null || inf.Longitude == 0.0D)
                            {
                                TVItemModel tvItemModelSubsector = tvItemService.GetParentTVItemModelListWithTVItemIDForLocationDB(InfrastructureTVItemID).Where(c => c.TVType == TVTypeEnum.Subsector).FirstOrDefault();
                                if (!CheckModelOK<TVItemModel>(tvItemModelSubsector)) return false;

                                MapInfoModel mapInfoModelSubsector = mapInfoService.GetMapInfoModelListWithTVItemIDDB(tvItemModelSubsector.TVItemID).Where(c => c.MapInfoDrawType == MapInfoDrawTypeEnum.Polygon).FirstOrDefault();
                                if (!CheckModelOK<MapInfoModel>(mapInfoModelSubsector)) return false;

                                if (inf.Latitude == null || inf.Latitude == 0.0D)
                                {
                                    inf.Latitude = mapInfoModelSubsector.LatMax;
                                }
                                if (inf.Longitude == null || inf.Longitude == 0.0D)
                                {
                                    inf.Longitude = mapInfoModelSubsector.LngMax;
                                }
                            }

                            List<Coord> coordList = new List<Coord>()
                            {
                                new Coord()
                                {
                                    Lat = (float)inf.Latitude,
                                    Lng = (float)inf.Longitude,
                                }
                            };

                            MapInfoModel mapInfoModelRet = new MapInfoModel();

                            if (NextType == "wwtp")
                            {
                                mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.WasteWaterTreatmentPlant, InfrastructureTVItemID);
                                if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;
                            }
                            else if (NextType == "liftstation")
                            {
                                mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.LiftStation, InfrastructureTVItemID);
                                if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;
                            }

                            if (inf.OutfallLatitude == null || inf.OutfallLatitude == 0.0D || inf.OutfallLongitude == null || inf.OutfallLongitude == 0.0D)
                            {
                                TVItemModel tvItemModelSubsector = tvItemService.GetParentTVItemModelListWithTVItemIDForLocationDB(InfrastructureTVItemID).Where(c => c.TVType == TVTypeEnum.Subsector).FirstOrDefault();
                                if (!CheckModelOK<TVItemModel>(tvItemModelSubsector)) return false;

                                MapInfoModel mapInfoModelSubsector = mapInfoService.GetMapInfoModelListWithTVItemIDDB(tvItemModelSubsector.TVItemID).Where(c => c.MapInfoDrawType == MapInfoDrawTypeEnum.Polygon).FirstOrDefault();
                                if (!CheckModelOK<MapInfoModel>(mapInfoModelSubsector)) return false;

                                if (inf.OutfallLatitude == null || inf.OutfallLatitude == 0.0D)
                                {
                                    inf.OutfallLatitude = mapInfoModelSubsector.LatMax;
                                }
                                if (inf.OutfallLongitude == null || inf.OutfallLongitude == 0.0D)
                                {
                                    inf.OutfallLongitude = mapInfoModelSubsector.LngMax;
                                }
                            }

                            coordList = new List<Coord>()
                            {
                                new Coord()
                                {
                                    Lat = (float)inf.OutfallLatitude,
                                    Lng = (float)inf.OutfallLongitude,
                                }
                            };

                            mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Outfall, InfrastructureTVItemID);
                            if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;
                        }
                    }
                }
            }

            return true;
        }
        public bool CreateMikeBoundaryConditions(MikeScenarioModel mikeScenarioModelNew, OldCSSPApps.MikeScenario ms)
        {
            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);
            TideSiteService tideSiteServiceR = new TideSiteService(LanguageEnum.en, user);

            List<DataPathOfTide> dataPathOfTide = tideSiteServiceR.GetTideDataPathsDB();

            string TVText = "";

            TVItemModel tvItemModelMikeScenario = tvItemServiceR.GetTVItemModelWithTVItemIDDB(mikeScenarioModelNew.MikeScenarioTVItemID);
            if (!CheckModelOK<TVItemModel>(tvItemModelMikeScenario)) return false;

            using (OldCSSPApps.OldCSSPAppDB2Entities oldCSSPAppDB2 = new OldCSSPApps.OldCSSPAppDB2Entities())
            {
                List<OldCSSPApps.MikeBoundaryCondition> mikeBoundaryConditionList = (from c in oldCSSPAppDB2.MikeBoundaryConditions
                                                                                     where c.MikeScenarioID == ms.MikeScenarioID
                                                                                     select c).ToList<OldCSSPApps.MikeBoundaryCondition>();

                foreach (OldCSSPApps.MikeBoundaryCondition mbc in mikeBoundaryConditionList)
                {
                    lblStatus2.Text = "CreateMikeBoundaryCondition " + mbc.BoundaryConditionName + " ...";
                    Application.DoEvents();

                    for (int i = 1; i < 3; i++)
                    {
                        TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                        TideSiteService tideSiteService = new TideSiteService(LanguageEnum.en, user);
                        MikeBoundaryConditionService mikeBoundaryConditionService = new MikeBoundaryConditionService(LanguageEnum.en, user);
                        MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

                        TVText = tvItemService.CleanText(mbc.BoundaryConditionName.Replace("'", ""));
                        if (i == 2)
                        {
                            TVText = TVText + " (WT)";
                        }

                        TVItemModel tvItemModelMikeBoundaryCondition = tvItemService.PostCreateTVItem(tvItemModelMikeScenario.TVItemID, TVText, TVText, (i == 1 ? TVTypeEnum.MikeBoundaryConditionMesh : TVTypeEnum.MikeBoundaryConditionWebTide));
                        if (!CheckModelOK<TVItemModel>(tvItemModelMikeBoundaryCondition)) return false;

                        MikeBoundaryConditionModel mikeBoundaryConditionModelNew = new MikeBoundaryConditionModel();
                        mikeBoundaryConditionModelNew.MikeBoundaryConditionTVItemID = tvItemModelMikeBoundaryCondition.TVItemID;
                        mikeBoundaryConditionModelNew.MikeBoundaryConditionCode = mbc.BoundaryConditionCode;
                        mikeBoundaryConditionModelNew.MikeBoundaryConditionName = mbc.BoundaryConditionName.Replace("'", "");
                        mikeBoundaryConditionModelNew.MikeBoundaryConditionLength_m = mbc.BoundaryConditionLength;
                        mikeBoundaryConditionModelNew.MikeBoundaryConditionFormat = mbc.BoundaryConditionFormat;
                        mikeBoundaryConditionModelNew.MikeBoundaryConditionLevelOrVelocity = (MikeBoundaryConditionLevelOrVelocityEnum)(mbc.BoundaryConditionType == "SpecifiedLevel" ? 1 : 2);
                        WebTideDataSetEnum WebTideDataSet = (WebTideDataSetEnum)(from c in dataPathOfTide where c.WebTideDataSet.ToString() == mbc.WebTideModel select c.WebTideDataSet).FirstOrDefault();
                        mikeBoundaryConditionModelNew.WebTideDataSet = WebTideDataSet;
                        mikeBoundaryConditionModelNew.NumberOfWebTideNodes = mbc.NumberOfWebTideNodes;
                        mikeBoundaryConditionModelNew.MikeBoundaryConditionTVText = TVText;

                        MikeBoundaryConditionModel mikeBoundaryConditionModelRet = mikeBoundaryConditionService.GetMikeBoundaryConditionModelExistDB(mikeBoundaryConditionModelNew);
                        if (!string.IsNullOrWhiteSpace(mikeBoundaryConditionModelRet.Error))
                        {
                            mikeBoundaryConditionModelRet = mikeBoundaryConditionService.PostAddMikeBoundaryConditionDB(mikeBoundaryConditionModelNew);
                            if (!CheckModelOK<MikeBoundaryConditionModel>(mikeBoundaryConditionModelRet)) return false;
                        }

                        // Doing MapInfo
                        List<Coord> coordList = new List<Coord>();

                        if (i == 1)
                        {
                            coordList = (from c in oldCSSPAppDB2.MikeBoundaryConditionNodes
                                         where c.MikeBoundaryConditionID == mbc.MikeBoundaryConditionID
                                         orderby c.Ordinal
                                         select new Coord
                                         {
                                             Lat = (float)c.Lat,
                                             Lng = (float)c.Long,
                                             Ordinal = c.Ordinal
                                         }).ToList<Coord>();
                        }
                        else
                        {
                            coordList = (from c in oldCSSPAppDB2.MikeBoundaryConditionWebTideNodes
                                         where c.MikeBoundaryConditionID == mbc.MikeBoundaryConditionID
                                         orderby c.Ordinal
                                         select new Coord
                                         {
                                             Lat = (float)c.Lat,
                                             Lng = (float)c.Long,
                                             Ordinal = c.Ordinal
                                         }).ToList<Coord>();
                        }

                        if (i == 1)
                        {
                            MapInfoModel mapInfoModel = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Polyline, TVTypeEnum.MikeBoundaryConditionMesh, tvItemModelMikeBoundaryCondition.TVItemID);
                            if (!CheckModelOK<MapInfoModel>(mapInfoModel)) return false;
                        }
                        else
                        {
                            MapInfoModel mapInfoModel = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Polyline, TVTypeEnum.MikeBoundaryConditionWebTide, tvItemModelMikeBoundaryCondition.TVItemID);
                            if (!CheckModelOK<MapInfoModel>(mapInfoModel)) return false;
                        }
                    }
                }
            }

            return true;
        }
        public bool CreateMikeScenarioFiles(MikeScenarioModel mikeScenarioModelNew, OldCSSPApps.MikeScenario ms)
        {
            using (OldCSSPApps.OldCSSPAppDB2Entities oldCSSPAppDB2 = new OldCSSPApps.OldCSSPAppDB2Entities())
            {

                List<OldCSSPApps.MikeScenarioFile> mikeScenarioFileList = (from c in oldCSSPAppDB2.MikeScenarioFiles
                                                                           where c.MikeScenarioID == ms.MikeScenarioID
                                                                           select c).ToList<OldCSSPApps.MikeScenarioFile>();

                string OldTVItemPath = (from c in oldCSSPAppDB2.CSSPItems
                                        from cp in oldCSSPAppDB2.CSSPItemPaths
                                        where c.CSSPItemID == cp.CSSPItemID
                                        && c.CSSPItemID == ms.CSSPItemID
                                        select cp.Path).FirstOrDefault<string>();

                if (string.IsNullOrEmpty(OldTVItemPath))
                {
                    richTextBoxStatus.AppendText("Error: Could not find the OldTVItemPath for CSSPItemID [" + ms.CSSPItemID + "]\r\n");
                    return false;
                }

                foreach (OldCSSPApps.MikeScenarioFile msf in mikeScenarioFileList)
                {
                    List<OldCSSPApps.CSSPFile> csspFileList = (from c in oldCSSPAppDB2.CSSPFiles
                                                               where c.CSSPFileID == msf.CSSPFileID
                                                               select c).ToList<OldCSSPApps.CSSPFile>();

                    if (csspFileList.Count != 1)
                    {
                        richTextBoxStatus.AppendText("Error: csspFileList should only contain 1 item it has [" + csspFileList.Count + "]\r\n");
                        return false;
                    }

                    foreach (OldCSSPApps.CSSPFile cf in csspFileList)
                    {
                        TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                        TVFileService tvFileService = new TVFileService(LanguageEnum.en, user);
                        MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

                        lblStatus2.Text = "CreateMikeScenarioFiles " + cf.ServerFileName + " ...";
                        Application.DoEvents();

                        string ServerFileName = cf.ServerFileName.Replace(@"\\", @"\");
                        string ServerFilePath = cf.ServerFilePath.Replace(@"\\", @"\");
                        string NewServerFilePath = tvFileService.GetServerFilePath(mikeScenarioModelNew.MikeScenarioTVItemID);

                        TVItemModel tvItemModelFileRet = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(mikeScenarioModelNew.MikeScenarioTVItemID, ServerFileName, TVTypeEnum.File);
                        if (!string.IsNullOrWhiteSpace(tvItemModelFileRet.Error))
                        {
                            tvItemModelFileRet = tvItemService.PostCreateTVItem(mikeScenarioModelNew.MikeScenarioTVItemID, ServerFileName, ServerFileName, TVTypeEnum.File);
                            if (!CheckModelOK<TVItemModel>(tvItemModelFileRet)) return false;

                        }
                        if (!CopyFileToNewPathIfNotAlreadyThere(ServerFileName, ServerFilePath, NewServerFilePath))
                        {
                            richTextBoxStatus.AppendText("Could not copy ServerFileName [" + ServerFileName + "] from ServerFilePath [" + ServerFilePath + "] to NewServerFilePath [" + NewServerFilePath + "]\r\n");
                            return false;
                        }

                        TVFileModel tvFileModelNew = new TVFileModel();
                        tvFileModelNew.TVFileTVItemID = tvItemModelFileRet.TVItemID;

                        if (cf.Purpose == "Input")
                        {
                            tvFileModelNew.FilePurpose = FilePurposeEnum.MikeInput;
                        }
                        else if (cf.Purpose == "MikeResult")
                        {
                            tvFileModelNew.FilePurpose = FilePurposeEnum.MikeResultDFSU;
                        }
                        else if (cf.Purpose == "KMZResult")
                        {
                            tvFileModelNew.FilePurpose = FilePurposeEnum.MikeResultKMZ;
                        }
                        else
                        {
                            richTextBoxStatus.AppendText("FilePurpose not one of [Input, MikeResult, KMZResult]\r\n");
                            return false;
                        }
                        tvFileModelNew.Language = LanguageEnum.en;
                        tvFileModelNew.FileDescription = cf.FileDescription;
                        tvFileModelNew.FileType = tvFileService.GetFileType(cf.FileType.ToUpper());
                        tvFileModelNew.FileSize_kb = (int)(cf.FileSize / 1024);
                        tvFileModelNew.FileInfo = "MIKE Scenario File";
                        tvFileModelNew.FileCreatedDate_UTC = (DateTime)cf.FileCreatedDate;
                        tvFileModelNew.ServerFileName = cf.ServerFileName;
                        tvFileModelNew.ServerFilePath = NewServerFilePath;
                        tvFileModelNew.ClientFilePath = "";
                        tvFileModelNew.FileDescription = "Description of " + cf.ServerFileName;

                        TVFileModel tvFileModelRet = tvFileService.GetTVFileModelExistDB(tvFileModelNew);
                        if (!string.IsNullOrWhiteSpace(tvFileModelRet.Error))
                        {
                            tvFileModelRet = tvFileService.PostAddTVFileDB(tvFileModelNew);
                            if (!CheckModelOK<TVFileModel>(tvFileModelRet)) return false;

                            CoordModel coordModel = mapInfoService.GetParentLatLngDB(tvItemModelFileRet.TVItemID);

                            List<Coord> coordList2 = new List<Coord>()
                            {
                                new Coord()
                                {
                                    Lat = (float)coordModel.Lat,
                                    Lng = (float)coordModel.Lng,
                                }
                            };

                            MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.File, tvItemModelFileRet.TVItemID);
                            if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;
                        }
                    }
                }
            }

            return true;
        }
        public bool CreateMikeScenarios(int MunicipalityTVItemID, string OldItemPath)
        {
            lblStatus2.Text = "CreateMikeScenarios ...";
            Application.DoEvents();

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MikeScenarioService mikeScenarioService = new MikeScenarioService(LanguageEnum.en, user);

            string TVText = "";

            using (OldCSSPApps.OldCSSPAppDB2Entities oldCSSPAppDB2 = new OldCSSPApps.OldCSSPAppDB2Entities())
            {
                int Level = GetLevel(OldItemPath.Replace("\\", "p")) + 3;
                List<OldCSSPApps.MikeScenario> mikeScenarioList = (from c in oldCSSPAppDB2.MikeScenarios
                                                                   from cp in oldCSSPAppDB2.CSSPItemPaths
                                                                   where c.CSSPItemID == cp.CSSPItemID
                                                                   && cp.Path.StartsWith(OldItemPath)
                                                                   && cp.Level == Level
                                                                   select c).ToList<OldCSSPApps.MikeScenario>();

                int CountMike = mikeScenarioList.Count();
                int Count = 0;
                foreach (OldCSSPApps.MikeScenario ms in mikeScenarioList)
                {
                    lblStatus2.Text = "CreateMikeScenario " + Count + " of " + CountMike + " ...";
                    Application.DoEvents();

                    Count += 1;
                    TVItemModel tvItemModelMuni = tvItemService.GetTVItemModelWithTVItemIDDB(MunicipalityTVItemID);
                    if (!CheckModelOK<TVItemModel>(tvItemModelMuni)) return false;

                    TVText = ms.ScenarioName;
                    TVItemModel tvItemModelMikeScenario = tvItemService.PostCreateTVItem(tvItemModelMuni.TVItemID, TVText, TVText, TVTypeEnum.MikeScenario);
                    if (!CheckModelOK<TVItemModel>(tvItemModelMikeScenario)) return false;

                    Application.DoEvents();

                    MikeScenarioModel mikeScenarioModelNew = new MikeScenarioModel();
                    mikeScenarioModelNew.MikeScenarioTVItemID = tvItemModelMikeScenario.TVItemID;
                    if (ms.ScenarioStatus == "Changed")
                    {
                        mikeScenarioModelNew.ScenarioStatus = ScenarioStatusEnum.Changed;
                    }
                    else if (ms.ScenarioStatus == "Completed")
                    {
                        mikeScenarioModelNew.ScenarioStatus = ScenarioStatusEnum.Completed;
                    }
                    else if (ms.ScenarioStatus == "Created")
                    {
                        mikeScenarioModelNew.ScenarioStatus = ScenarioStatusEnum.Copied;
                    }
                    else if (ms.ScenarioStatus == "Error")
                    {
                        mikeScenarioModelNew.ScenarioStatus = ScenarioStatusEnum.Error;
                    }
                    else
                    {
                        mikeScenarioModelNew.ScenarioStatus = ScenarioStatusEnum.Error;
                    }
                    mikeScenarioModelNew.MikeScenarioStartDateTime_Local = (DateTime)ms.ScenarioStartDateAndTime;
                    mikeScenarioModelNew.MikeScenarioEndDateTime_Local = (DateTime)ms.ScenarioEndDateAndTime;
                    mikeScenarioModelNew.MikeScenarioStartExecutionDateTime_Local = ms.ScenarioStartExecutionDateAndTime;
                    mikeScenarioModelNew.MikeScenarioExecutionTime_min = ms.ScenarioExecutionTimeInMinutes;
                    mikeScenarioModelNew.NumberOfElements = ms.NumberOfElements;
                    mikeScenarioModelNew.NumberOfTimeSteps = ms.NumberOfTimeSteps;
                    mikeScenarioModelNew.NumberOfSigmaLayers = ms.NumberOfSigmaLayers;
                    mikeScenarioModelNew.NumberOfZLayers = ms.NumberOfZLayers;
                    mikeScenarioModelNew.NumberOfHydroOutputParameters = ms.NumberOfHydroOutputParameters;
                    mikeScenarioModelNew.NumberOfTransOutputParameters = ms.NumberOfTransOutputParameters;
                    mikeScenarioModelNew.EstimatedHydroFileSize = ms.EstimatedHydroFileSize;
                    mikeScenarioModelNew.EstimatedTransFileSize = ms.EstimatedTransFileSize;
                    mikeScenarioModelNew.MikeScenarioTVText = TVText;

                    List<OldCSSPApps.MikeParameter> mikeParameterList = (from c in oldCSSPAppDB2.MikeParameters
                                                                         where c.MikeScenarioID == ms.MikeScenarioID
                                                                         select c).ToList<OldCSSPApps.MikeParameter>();

                    foreach (OldCSSPApps.MikeParameter mp in mikeParameterList) // should only have one
                    {
                        mikeScenarioModelNew.WindSpeed_km_h = (double)mp.WindSpeed;
                        mikeScenarioModelNew.WindDirection_deg = (double)mp.WindDirection;
                        mikeScenarioModelNew.DecayFactor_per_day = (double)mp.DecayFactorPerDay;
                        mikeScenarioModelNew.DecayIsConstant = (bool)mp.DecayIsConstant;
                        mikeScenarioModelNew.DecayFactorAmplitude = (double)mp.DecayFactorAmplitude;
                        mikeScenarioModelNew.ResultFrequency_min = (int)mp.ResultFrequencyInMinutes;
                        mikeScenarioModelNew.AmbientTemperature_C = (double)mp.AmbientTemperature;
                        mikeScenarioModelNew.AmbientSalinity_PSU = (double)mp.AmbientSalinity;
                        mikeScenarioModelNew.ManningNumber = (double)mp.ManningNumber;

                        if (mikeScenarioModelNew.DecayFactor_per_day < mikeScenarioModelNew.DecayFactorAmplitude)
                        {
                            mikeScenarioModelNew.DecayFactorAmplitude = mikeScenarioModelNew.DecayFactor_per_day - 0.01D;
                        }
                    }

                    MikeScenarioModel mikeScenarioModelRet = mikeScenarioService.GetMikeScenarioModelWithMikeScenarioTVItemIDDB(tvItemModelMikeScenario.TVItemID);
                    if (!string.IsNullOrWhiteSpace(mikeScenarioModelRet.Error))
                    {
                        mikeScenarioModelRet = mikeScenarioService.PostAddMikeScenarioDB(mikeScenarioModelNew);
                        if (!CheckModelOK<MikeScenarioModel>(mikeScenarioModelRet)) return false;
                    }

                    if (!CreateMikeSources(mikeScenarioModelRet, ms))
                    {
                        return false;
                    }

                    if (!CreateMikeBoundaryConditions(mikeScenarioModelRet, ms))
                    {
                        return false;
                    }


                    if (!CreateMikeScenarioFiles(mikeScenarioModelRet, ms))
                    {
                        return false;
                    }

                    if (!SetProperMapInfoCoordToCentroid(mikeScenarioModelRet, ms))
                    {
                        return false;
                    }
                }
            }

            Application.DoEvents();

            return true;
        }
        public bool CreateMikeSources(MikeScenarioModel mikeScenarioModelNew, OldCSSPApps.MikeScenario ms)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MikeSourceService mikeSourceService = new MikeSourceService(LanguageEnum.en, user);
            MikeSourceStartEndService mikeSourceStartEndService = new MikeSourceStartEndService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            string TVText = "";

            TVItemModel tvItemModelMikeScenario = tvItemService.GetTVItemModelWithTVItemIDDB(mikeScenarioModelNew.MikeScenarioTVItemID);
            if (!CheckModelOK<TVItemModel>(tvItemModelMikeScenario)) return false;

            using (OldCSSPApps.OldCSSPAppDB2Entities oldCSSPAppDB2 = new OldCSSPApps.OldCSSPAppDB2Entities())
            {
                List<OldCSSPApps.MikeSource> mikeSourceList = (from c in oldCSSPAppDB2.MikeSources
                                                               where c.MikeScenarioID == ms.MikeScenarioID
                                                               select c).ToList<OldCSSPApps.MikeSource>();

                foreach (OldCSSPApps.MikeSource mSource in mikeSourceList)
                {
                    lblStatus2.Text = "CreateMikeSource " + mSource.SourceName + " ...";
                    Application.DoEvents();

                    TVText = mSource.SourceName;

                    TVItemModel tvItemModelMikeSource = tvItemService.PostCreateTVItem(tvItemModelMikeScenario.TVItemID, TVText, TVText, TVTypeEnum.MikeSource);
                    if (!CheckModelOK<TVItemModel>(tvItemModelMikeSource)) return false;

                    List<Coord> coordList2 = new List<Coord>()
                        {
                            new Coord()
                            {
                                Lat = (float)mSource.SourceLat,
                                Lng = (float)mSource.SourceLong,
                            }
                        };

                    MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.MikeSource, tvItemModelMikeSource.TVItemID);
                    if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;

                    MikeSourceModel mikeSourceModelNew = new MikeSourceModel();

                    mikeSourceModelNew.MikeSourceTVItemID = tvItemModelMikeSource.TVItemID;
                    //mikeSourceModelNew.MikeSourceID = mSource.MikeSourceID;
                    mikeSourceModelNew.IsContinuous = (bool)mSource.IsContinuous;
                    mikeSourceModelNew.Include = (bool)mSource.Include;
                    mikeSourceModelNew.IsRiver = false;
                    mikeSourceModelNew.SourceNumberString = mSource.SourceNumberString;
                    mikeSourceModelNew.MikeSourceTVText = TVText;

                    MikeSourceModel mikeSourceModelExist = mikeSourceService.GetMikeSourceModelWithMikeSourceTVItemIDDB(mikeSourceModelNew.MikeSourceTVItemID);
                    if (!string.IsNullOrWhiteSpace(mikeSourceModelExist.Error))
                    {
                        mikeSourceModelExist = mikeSourceService.PostAddMikeSourceDB(mikeSourceModelNew);
                        if (!CheckModelOK<MikeSourceModel>(mikeSourceModelExist)) return false;

                    }

                    List<OldCSSPApps.MikeSourceStartEnd> mikeSourceStartEndList = (from c in oldCSSPAppDB2.MikeSourceStartEnds
                                                                                   where c.MikeSourceID == mSource.MikeSourceID
                                                                                   select c).ToList<OldCSSPApps.MikeSourceStartEnd>();

                    foreach (OldCSSPApps.MikeSourceStartEnd mSourceStartEnd in mikeSourceStartEndList)
                    {
                        Application.DoEvents();

                        MikeSourceStartEndModel mikeSourceStartEndModelNew = new MikeSourceStartEndModel();
                        mikeSourceStartEndModelNew.MikeSourceID = mikeSourceModelExist.MikeSourceID;
                        mikeSourceStartEndModelNew.StartDateAndTime_Local = (DateTime)mSourceStartEnd.StartDateAndTime;
                        mikeSourceStartEndModelNew.EndDateAndTime_Local = (DateTime)mSourceStartEnd.StartDateAndTime;
                        if (mSourceStartEnd.EndDateAndTime != null)
                        {
                            mikeSourceStartEndModelNew.EndDateAndTime_Local = (DateTime)mSourceStartEnd.EndDateAndTime;
                        }
                        mikeSourceStartEndModelNew.SourceFlowStart_m3_day = (double)mSourceStartEnd.SourceFlowStart;
                        mikeSourceStartEndModelNew.SourceFlowEnd_m3_day = (double)mSourceStartEnd.SourceFlowEnd;
                        mikeSourceStartEndModelNew.SourcePollutionStart_MPN_100ml = (int)mSourceStartEnd.SourcePollutionStart;
                        if (mSourceStartEnd.SourcePollutionStart == 0 && mikeSourceModelExist.IsRiver == false)
                        {
                            mikeSourceModelExist.IsRiver = true;
                        }
                        mikeSourceStartEndModelNew.SourcePollutionEnd_MPN_100ml = (int)mSourceStartEnd.SourcePollutionEnd;
                        mikeSourceStartEndModelNew.SourceTemperatureStart_C = (double)mSourceStartEnd.SourceTemperatureStart;
                        mikeSourceStartEndModelNew.SourceTemperatureEnd_C = (double)mSourceStartEnd.SourceTemperatureEnd;
                        mikeSourceStartEndModelNew.SourceSalinityStart_PSU = (double)mSourceStartEnd.SourceSalinityStart;
                        mikeSourceStartEndModelNew.SourceSalinityEnd_PSU = (double)mSourceStartEnd.SourceSalinityEnd;

                        MikeSourceStartEndModel mikeSourceStartEndModelRet = mikeSourceStartEndService.GetMikeSourceStartEndModelExist(mikeSourceStartEndModelNew);
                        if (!string.IsNullOrWhiteSpace(mikeSourceStartEndModelRet.Error))
                        {
                            mikeSourceStartEndModelRet = mikeSourceStartEndService.PostAddMikeSourceStartEndDB(mikeSourceStartEndModelNew);
                            if (!CheckModelOK<MikeSourceStartEndModel>(mikeSourceStartEndModelRet)) return false;
                        }
                    }
                }
            }

            return true;
        }
        public bool CreateTVItemsAll(string Prov)
        {
            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCountry = new TVItemModel();
            if (Prov != "Maine")
            {
                tvItemModelCountry = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
                if (!CheckModelOK<TVItemModel>(tvItemModelCountry)) return false;

            }
            else
            {
                tvItemModelCountry = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "United States", TVTypeEnum.Country);
                if (!CheckModelOK<TVItemModel>(tvItemModelCountry)) return false;
            }

            TVItemModel tvItemModelProv = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCountry.TVItemID, Prov, TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return false;


            lblStatus.Text = "Starting ... CreateTVItemsAll";
            Application.DoEvents();

            int StartMECreateTVItemsAll = int.Parse(textBoxMECreateTVItemsAll.Text);
            int StartNBCreateTVItemsAll = int.Parse(textBoxNBCreateTVItemsAll.Text);
            int StartNLCreateTVItemsAll = int.Parse(textBoxNLCreateTVItemsAll.Text);
            int StartNSCreateTVItemsAll = int.Parse(textBoxNSCreateTVItemsAll.Text);
            int StartPECreateTVItemsAll = int.Parse(textBoxPECreateTVItemsAll.Text);
            int StartBCCreateTVItemsAll = int.Parse(textBoxBCCreateTVItemsAll.Text);
            int StartQCCreateTVItemsAll = int.Parse(textBoxQCCreateTVItemsAll.Text);

            using (OldCSSPApps.OldCSSPAppDB2Entities oldCSSPAppDB2 = new OldCSSPApps.OldCSSPAppDB2Entities())
            {
                int csspItemTypeProvID = (from c in oldCSSPAppDB2.CSSPItemTypes
                                          where c.CSSPItemTypeTextENLowered == "province"
                                          select c.CSSPItemTypeID).FirstOrDefault<int>();

                string OldCSSPItemPath = (from c in oldCSSPAppDB2.CSSPItems
                                          from cp in oldCSSPAppDB2.CSSPItemPaths
                                          from cl in oldCSSPAppDB2.CSSPItemLanguages
                                          where c.CSSPItemID == cp.CSSPItemID
                                          && c.CSSPItemID == cl.CSSPItemID
                                          && cl.Language == "en"
                                          && cl.CSSPItemText == Prov
                                          select cp.Path).FirstOrDefault<string>();

                // getting all the municipalities
                List<OldTVItem> OldItemMuniList = (from c in oldCSSPAppDB2.CSSPItems
                                                   from cp in oldCSSPAppDB2.CSSPItemPaths
                                                   from cl in oldCSSPAppDB2.CSSPItemLanguages
                                                   where c.CSSPItemID == cp.CSSPItemID
                                                   && c.CSSPItemID == cl.CSSPItemID
                                                   && cp.Path.StartsWith(OldCSSPItemPath)
                                                   && cp.Level == 4
                                                   && cl.Language == "en"
                                                   select new OldTVItem
                                                   {
                                                       CSSPItemID = c.CSSPItemID,
                                                       CSSPPath = cp.Path,
                                                       CSSPText = cl.CSSPItemText,
                                                   }).ToList<OldTVItem>();

                int TotalCount = OldItemMuniList.Count();
                int Count = 0;
                foreach (OldTVItem otiMuni in OldItemMuniList)
                {
                    if (Cancel) return false;

                    string CSSPTextNoAnd = otiMuni.CSSPText.Replace("&", "and");
                    Count += 1;

                    lblStatus.Text = (Count * 100 / TotalCount).ToString() + "... CreateTVItemsAll for " + Prov;
                    lblStatus2.Text = Count + " of " + TotalCount;
                    Application.DoEvents();

                    switch (Prov)
                    {
                        case "Maine":
                            {
                                textBoxMECreateTVItemsAll.Text = Count.ToString();
                                if (StartMECreateTVItemsAll > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "New Brunswick":
                            {
                                textBoxNBCreateTVItemsAll.Text = Count.ToString();
                                if (StartNBCreateTVItemsAll > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "Newfoundland and Labrador":
                            {
                                textBoxNLCreateTVItemsAll.Text = Count.ToString();
                                if (StartNLCreateTVItemsAll > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "Nova Scotia":
                            {
                                textBoxNSCreateTVItemsAll.Text = Count.ToString();
                                if (StartNSCreateTVItemsAll > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "Prince Edward Island":
                            {
                                textBoxPECreateTVItemsAll.Text = Count.ToString();
                                if (StartPECreateTVItemsAll > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "British Columbia":
                            {
                                textBoxBCCreateTVItemsAll.Text = Count.ToString();
                                if (StartBCCreateTVItemsAll > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "Québec":
                            {
                                textBoxQCCreateTVItemsAll.Text = Count.ToString();
                                if (StartQCCreateTVItemsAll > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    TempData.LocationNameLatLng locNameLatLng = new TempData.LocationNameLatLng();

                    using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                    {
                        locNameLatLng = (from c in dbDT.LocationNameLatLngs
                                         where c.Municipality == CSSPTextNoAnd
                                         && c.Province == Prov
                                         select c).FirstOrDefault<TempData.LocationNameLatLng>();

                        if (locNameLatLng == null)
                        {
                            richTextBoxStatus.AppendText("Could not find Municipality [" + CSSPTextNoAnd + "]\r\n");
                            return false;
                        }
                    }

                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                    MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
                    TVItemLinkService tvItemLinkService = new TVItemLinkService(LanguageEnum.en, user);

                    List<MapInfoModel> mapInfoModelListSubsector = mapInfoService.GetMapInfoModelWithLatAndLngInPolygonWithTVTypeDB((float)locNameLatLng.Lat, (float)locNameLatLng.Lng, TVTypeEnum.Subsector);
                    if (mapInfoModelListSubsector.Count == 0) return false;


                    TVItemModel tvItemModelSubsector = tvItemService.GetTVItemModelWithTVItemIDDB(mapInfoModelListSubsector[0].TVItemID);
                    if (!CheckModelOK<TVItemModel>(tvItemModelSubsector)) return false;

                    TVItemModel tvItemModelMuni = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelSubsector.TVItemID, CSSPTextNoAnd, TVTypeEnum.Municipality);
                    if (!string.IsNullOrWhiteSpace(tvItemModelMuni.Error))
                    {
                        tvItemModelMuni = tvItemService.PostCreateTVItem(tvItemModelSubsector.TVItemID, CSSPTextNoAnd, CSSPTextNoAnd, TVTypeEnum.Municipality);
                        if (!CheckModelOK<TVItemModel>(tvItemModelMuni)) return false;

                        if (locNameLatLng.Lat == null || locNameLatLng.Lat == 0.0D || locNameLatLng.Lng == null || locNameLatLng.Lng == 0.0D)
                        {
                            if (locNameLatLng.Lat == null || locNameLatLng.Lat == 0.0D)
                            {
                                locNameLatLng.Lat = mapInfoModelListSubsector[0].LatMax;
                            }
                            if (locNameLatLng.Lng == null || locNameLatLng.Lng == 0.0D)
                            {
                                locNameLatLng.Lng = mapInfoModelListSubsector[0].LngMax;
                            }
                        }

                        List<Coord> coordList2 = new List<Coord>()
                        {
                            new Coord()
                            {
                                Lat = (float)locNameLatLng.Lat,
                                Lng = (float)locNameLatLng.Lng,
                            }
                        };

                        MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.Municipality, tvItemModelMuni.TVItemID);
                        if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;
                    }

                    // Loading all MikeScenarios from the Municipality                            
                    if (!CreateMikeScenarios(tvItemModelMuni.TVItemID, otiMuni.CSSPPath))
                    {
                        richTextBoxStatus.AppendText("Error in CreateMikeScenario\r\n");
                        return false;
                    }

                    // getting all the WWTP
                    List<OldTVItem> OldItemWWTPListEN = (from c in oldCSSPAppDB2.CSSPItems
                                                         from cp in oldCSSPAppDB2.CSSPItemPaths
                                                         from cl in oldCSSPAppDB2.CSSPItemLanguages
                                                         where c.CSSPItemID == cp.CSSPItemID
                                                         && c.CSSPItemID == cl.CSSPItemID
                                                         && cp.Path.StartsWith(otiMuni.CSSPPath)
                                                         && cp.Level == 7
                                                         && cl.Language == "en"
                                                         select new OldTVItem
                                                         {
                                                             CSSPItemID = c.CSSPItemID,
                                                             CSSPPath = cp.Path,
                                                             CSSPText = cl.CSSPItemText,
                                                         }).ToList<OldTVItem>();

                    foreach (OldTVItem otiWWTP in OldItemWWTPListEN)
                    {

                        TVItemModel tvItemModelWWTP = tvItemService.PostCreateTVItem(tvItemModelMuni.TVItemID, otiWWTP.CSSPText, otiWWTP.CSSPText, TVTypeEnum.Infrastructure);
                        if (!CheckModelOK<TVItemModel>(tvItemModelWWTP)) return false;

                        if (!CreateBoxModels(tvItemModelWWTP.TVItemID, otiWWTP.CSSPPath))
                        {
                            return false;
                        }
                        if (!CreateVPScenarios(tvItemModelWWTP.TVItemID, otiWWTP.CSSPPath))
                        {
                            return false;
                        }

                        if (!CreateInfrastructures(tvItemModelWWTP.TVItemID, otiWWTP.CSSPPath, "wwtp"))
                        {
                            return false;
                        }


                        // getting all the LS
                        List<OldTVItem> OldItemLSListEN = (from c in oldCSSPAppDB2.CSSPItems
                                                           from cp in oldCSSPAppDB2.CSSPItemPaths
                                                           from cl in oldCSSPAppDB2.CSSPItemLanguages
                                                           where c.CSSPItemID == cp.CSSPItemID
                                                           && c.CSSPItemID == cl.CSSPItemID
                                                           && cp.Path.StartsWith(otiWWTP.CSSPPath)
                                                           && cp.Level == 9
                                                           && cl.Language == "en"
                                                           select new OldTVItem
                                                           {
                                                               CSSPItemID = c.CSSPItemID,
                                                               CSSPPath = cp.Path,
                                                               CSSPText = cl.CSSPItemText,
                                                           }).ToList<OldTVItem>();

                        foreach (OldTVItem otiLS in OldItemLSListEN)
                        {
                            TVItemModel tvItemModelLiftStation = tvItemService.PostCreateTVItem(tvItemModelMuni.TVItemID, otiLS.CSSPText, otiLS.CSSPText, TVTypeEnum.Infrastructure);
                            if (!CheckModelOK<TVItemModel>(tvItemModelLiftStation)) return false;

                            if (!CreateBoxModels(tvItemModelLiftStation.TVItemID, otiLS.CSSPPath))
                            {
                                return false;
                            }
                            if (!CreateVPScenarios(tvItemModelLiftStation.TVItemID, otiLS.CSSPPath))
                            {
                                return false;
                            }

                            if (!CreateInfrastructures(tvItemModelLiftStation.TVItemID, otiLS.CSSPPath, "liftstation"))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
        public bool CreateVPScenarios(int InfrastructureTVItemID, string OldItemPath)
        {
            using (OldCSSPApps.OldCSSPAppDB2Entities oldCSSPAppDB2 = new OldCSSPApps.OldCSSPAppDB2Entities())
            {
                int Level = GetLevel(OldItemPath.Replace("\\", "p")) + 1;
                List<OldCSSPApps.VPScenario> vpScenarioList = (from c in oldCSSPAppDB2.VPScenarios
                                                               from cp in oldCSSPAppDB2.CSSPItemPaths
                                                               where c.CSSPItemID == cp.CSSPItemID
                                                               && cp.Path.StartsWith(OldItemPath)
                                                               && cp.Level == Level
                                                               select c).ToList<OldCSSPApps.VPScenario>();

                int CountVP = vpScenarioList.Count();
                int Count = 0;
                foreach (OldCSSPApps.VPScenario vps in vpScenarioList)
                {
                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                    VPScenarioService vpScenarioService = new VPScenarioService(LanguageEnum.en, user);
                    VPAmbientService vpAmbientService = new VPAmbientService(LanguageEnum.en, user);

                    Count += 1;
                    TVItemModel tvItemModelInfratructure = tvItemService.GetTVItemModelWithTVItemIDDB(InfrastructureTVItemID);
                    if (!CheckModelOK<TVItemModel>(tvItemModelInfratructure)) return false;

                    Application.DoEvents();

                    VPScenarioModel vpScenarioModelNew = new VPScenarioModel();
                    vpScenarioModelNew.InfrastructureTVItemID = tvItemModelInfratructure.TVItemID;

                    string VPScenarioName = tvItemService.CleanText(vps.VPScenarioName.Trim());

                    if (vps.UseAsBestEstimate == null)
                    {
                        vpScenarioModelNew.UseAsBestEstimate = false;
                    }
                    else
                    {
                        vpScenarioModelNew.UseAsBestEstimate = true;
                    }

                    vpScenarioModelNew.EffluentFlow_m3_s = vps.EffluentFlow;
                    vpScenarioModelNew.EffluentConcentration_MPN_100ml = (int)vps.EffluentConcentration;
                    vpScenarioModelNew.FroudeNumber = vps.FroudeNumber;
                    vpScenarioModelNew.PortDiameter_m = vps.PortDiameter;
                    vpScenarioModelNew.PortDepth_m = vps.PortDepth;
                    vpScenarioModelNew.PortElevation_m = vps.PortElevation;
                    vpScenarioModelNew.VerticalAngle_deg = vps.VerticalAngle;
                    vpScenarioModelNew.HorizontalAngle_deg = vps.HorizontalAngle;
                    vpScenarioModelNew.NumberOfPorts = (int)vps.NumberOfPorts;
                    vpScenarioModelNew.PortSpacing_m = vps.PortSpacing;
                    vpScenarioModelNew.AcuteMixZone_m = vps.AcuteMixZone;
                    vpScenarioModelNew.ChronicMixZone_m = vps.ChronicMixZone;
                    vpScenarioModelNew.EffluentSalinity_PSU = vps.EffluentSalinity;
                    vpScenarioModelNew.EffluentTemperature_C = vps.EffluentTemperature;
                    vpScenarioModelNew.EffluentVelocity_m_s = vps.EffluentVelocity;
                    vpScenarioModelNew.VPScenarioStatus = ScenarioStatusEnum.Changed;
                    vpScenarioModelNew.VPScenarioName = VPScenarioName;

                    VPScenarioModel vpScenarioModelRet = vpScenarioService.GetVPScenarioModelWithInfrastructureTVItemIDAndVPScenarioNameDB(InfrastructureTVItemID, VPScenarioName);
                    if (!string.IsNullOrWhiteSpace(vpScenarioModelRet.Error))
                    {
                        vpScenarioModelRet = vpScenarioService.PostAddVPScenarioDB(vpScenarioModelNew);
                        if (!CheckModelOK<VPScenarioModel>(vpScenarioModelRet)) return false;
                    }

                    // Transfering the VPAmbients table
                    List<OldCSSPApps.VPAmbient> vpAmbientList = (from c in oldCSSPAppDB2.VPAmbients
                                                                 where c.VPScenarioID == vps.VPScenarioID
                                                                 select c).ToList<OldCSSPApps.VPAmbient>();

                    foreach (OldCSSPApps.VPAmbient vpa in vpAmbientList)
                    {

                        Application.DoEvents();

                        VPAmbientModel vpAmbientModelNew = new VPAmbientModel();
                        vpAmbientModelNew.Row = vpa.Row;
                        vpAmbientModelNew.MeasurementDepth_m = vpa.MeasurementDepth;
                        vpAmbientModelNew.CurrentSpeed_m_s = vpa.CurrentSpeed;
                        vpAmbientModelNew.CurrentDirection_deg = vpa.CurrentDirection;
                        vpAmbientModelNew.AmbientSalinity_PSU = vpa.VPAmbientSalinity;
                        vpAmbientModelNew.AmbientTemperature_C = vpa.VPAmbientTemperature;
                        vpAmbientModelNew.BackgroundConcentration_MPN_100ml = (int)vpa.BackgroundConcentration;
                        vpAmbientModelNew.PollutantDecayRate_per_day = vpa.PollutantDecayRate;
                        vpAmbientModelNew.FarFieldCurrentSpeed_m_s = vpa.FarFieldCurrentSpeed;
                        vpAmbientModelNew.FarFieldCurrentDirection_deg = vpa.FarFieldCurrentDirection;
                        vpAmbientModelNew.FarFieldDiffusionCoefficient = vpa.FarFieldDiffusionCoefficient;
                        vpAmbientModelNew.VPScenarioID = vpScenarioModelRet.VPScenarioID;

                        VPAmbientModel vpAmbientModelRet = vpAmbientService.GetVPAmbientModelWithVPScenarioIDAndRowDB(vpScenarioModelRet.VPScenarioID, vpa.Row);
                        if (!string.IsNullOrWhiteSpace(vpAmbientModelRet.Error))
                        {
                            vpAmbientModelRet = vpAmbientService.PostAddVPAmbientDB(vpAmbientModelNew);
                            if (!CheckModelOK<VPAmbientModel>(vpAmbientModelRet)) return false;
                        }
                    }

                    int VPAmbientCount = vpAmbientService.GetVPAmbientModelListWithVPScenarioIDDB(vpScenarioModelRet.VPScenarioID).Count();

                    if (VPAmbientCount == 5) // old vpscenario had only 5 Ambient rows, need to add 3 more
                    {
                        VPAmbientModel vpAmbientModelNew6 = new VPAmbientModel();
                        vpAmbientModelNew6.Row = 6;
                        vpAmbientModelNew6.MeasurementDepth_m = -999;
                        vpAmbientModelNew6.CurrentSpeed_m_s = -999;
                        vpAmbientModelNew6.CurrentDirection_deg = -999;
                        vpAmbientModelNew6.AmbientSalinity_PSU = -999;
                        vpAmbientModelNew6.AmbientTemperature_C = -999;
                        vpAmbientModelNew6.BackgroundConcentration_MPN_100ml = -999;
                        vpAmbientModelNew6.PollutantDecayRate_per_day = -999;
                        vpAmbientModelNew6.FarFieldCurrentSpeed_m_s = -999;
                        vpAmbientModelNew6.FarFieldCurrentDirection_deg = -999;
                        vpAmbientModelNew6.FarFieldDiffusionCoefficient = -999;
                        vpAmbientModelNew6.VPScenarioID = vpScenarioModelRet.VPScenarioID;

                        VPAmbientModel vpAmbientModelRet = vpAmbientService.GetVPAmbientModelWithVPScenarioIDAndRowDB(vpScenarioModelRet.VPScenarioID, vpAmbientModelNew6.Row);
                        if (!string.IsNullOrWhiteSpace(vpAmbientModelRet.Error))
                        {
                            vpAmbientModelRet = vpAmbientService.PostAddVPAmbientDB(vpAmbientModelNew6);
                            if (!CheckModelOK<VPAmbientModel>(vpAmbientModelRet)) return false;
                        }

                        VPAmbientModel vpAmbientModelNew7 = new VPAmbientModel();
                        vpAmbientModelNew7.Row = 7;
                        vpAmbientModelNew7.MeasurementDepth_m = -999;
                        vpAmbientModelNew7.CurrentSpeed_m_s = -999;
                        vpAmbientModelNew7.CurrentDirection_deg = -999;
                        vpAmbientModelNew7.AmbientSalinity_PSU = -999;
                        vpAmbientModelNew7.AmbientTemperature_C = -999;
                        vpAmbientModelNew7.BackgroundConcentration_MPN_100ml = -999;
                        vpAmbientModelNew7.PollutantDecayRate_per_day = -999;
                        vpAmbientModelNew7.FarFieldCurrentSpeed_m_s = -999;
                        vpAmbientModelNew7.FarFieldCurrentDirection_deg = -999;
                        vpAmbientModelNew7.FarFieldDiffusionCoefficient = -999;
                        vpAmbientModelNew7.VPScenarioID = vpScenarioModelRet.VPScenarioID;

                        vpAmbientModelRet = vpAmbientService.GetVPAmbientModelWithVPScenarioIDAndRowDB(vpScenarioModelRet.VPScenarioID, vpAmbientModelNew7.Row);
                        if (!string.IsNullOrWhiteSpace(vpAmbientModelRet.Error))
                        {
                            vpAmbientModelRet = vpAmbientService.PostAddVPAmbientDB(vpAmbientModelNew7);
                            if (!CheckModelOK<VPAmbientModel>(vpAmbientModelRet)) return false;
                        }

                        VPAmbientModel vpAmbientModelNew8 = new VPAmbientModel();
                        vpAmbientModelNew8.Row = 8;
                        vpAmbientModelNew8.MeasurementDepth_m = -999;
                        vpAmbientModelNew8.CurrentSpeed_m_s = -999;
                        vpAmbientModelNew8.CurrentDirection_deg = -999;
                        vpAmbientModelNew8.AmbientSalinity_PSU = -999;
                        vpAmbientModelNew8.AmbientTemperature_C = -999;
                        vpAmbientModelNew8.BackgroundConcentration_MPN_100ml = -999;
                        vpAmbientModelNew8.PollutantDecayRate_per_day = -999;
                        vpAmbientModelNew8.FarFieldCurrentSpeed_m_s = -999;
                        vpAmbientModelNew8.FarFieldCurrentDirection_deg = -999;
                        vpAmbientModelNew8.FarFieldDiffusionCoefficient = -999;
                        vpAmbientModelNew8.VPScenarioID = vpScenarioModelRet.VPScenarioID;

                        vpAmbientModelRet = vpAmbientService.GetVPAmbientModelWithVPScenarioIDAndRowDB(vpScenarioModelRet.VPScenarioID, vpAmbientModelNew8.Row);
                        if (!string.IsNullOrWhiteSpace(vpAmbientModelRet.Error))
                        {
                            vpAmbientModelRet = vpAmbientService.PostAddVPAmbientDB(vpAmbientModelNew8);
                            if (!CheckModelOK<VPAmbientModel>(vpAmbientModelRet)) return false;
                        }
                    }
                }

            }

            Application.DoEvents();

            return true;
        }
        public bool PointInPolygon(Point p, Point[] poly)
        {
            Point p1, p2;
            bool inside = false;
            if (poly.Length < 3)
            {
                return inside;
            }

            Point oldPoint = new Point(poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            for (int i = 0; i < poly.Length; i++)
            {
                Point newPoint = new Point(poly[i].X, poly[i].Y);

                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X) && ((long)p.Y - (long)p1.Y) * (long)(p2.X - p1.X) < ((long)p2.Y - (long)p1.Y) * (long)(p.X - p1.X))
                {
                    inside = !inside;
                }

                oldPoint = newPoint;

            }
            return inside;
        }
        public bool SetProperMapInfoCoordToCentroid(MikeScenarioModel mikeScenarioModelNew, OldCSSPApps.MikeScenario ms)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            TVFileService tvFileService = new TVFileService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            TVItemModel tvItemModelMikeScenario = tvItemService.GetTVItemModelWithTVItemIDDB(mikeScenarioModelNew.MikeScenarioTVItemID);
            if (!CheckModelOK<TVItemModel>(tvItemModelMikeScenario)) return false;

            TVFileModel tvFileModelMDF = new TVFileModel();
            List<TVFileModel> tvFileModelList = tvFileService.GetTVFileModelListWithParentTVItemIDDB(mikeScenarioModelNew.MikeScenarioTVItemID);
            if (tvFileModelList.Count > 0)
            {
                tvFileModelMDF = tvFileModelList.Where(c => c.FileType == FileTypeEnum.MDF).FirstOrDefault();
            }

            List<float> latList = new List<float>();
            List<float> lngList = new List<float>();

            if (tvFileModelMDF != null)
            {
                string NewServerPath = tvFileModelMDF.ServerFilePath;
                if (System.Environment.UserName != "leblancc")
                {
                    NewServerPath = NewServerPath.Replace(@"E:\", @"C:\");
                }

                FileInfo fiMDF = new FileInfo(NewServerPath + tvFileModelMDF.ServerFileName);

                if (!fiMDF.Exists)
                {
                    richTextBoxStatus.AppendText("Could not find file [" + fiMDF.FullName + "]\r\n");
                    return false;
                }

                StreamReader sr = fiMDF.OpenText();

                while (!sr.EndOfStream)
                {
                    string LineStr = sr.ReadLine();

                    if (LineStr.Contains("POINT = "))
                    {
                        string[] StrArr = LineStr.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                        if (StrArr.Count() != 6)
                        {
                            richTextBoxStatus.AppendText("StrArr.Count is not equal 6\r\n");
                            return false;
                        }

                        latList.Add(float.Parse(StrArr[2]));
                        lngList.Add(float.Parse(StrArr[1]));
                    }
                }

                sr.Close();
            }
            else
            {
                latList.Add(0);
                lngList.Add(0);
            }


            float latAverage = latList.Average();
            float lngAverage = lngList.Average();

            foreach (TVFileModel tvFileModel in tvFileModelList)
            {
                List<Coord> coordList2 = new List<Coord>()
                        {
                            new Coord()
                            {
                                Lat = (float)latAverage,
                                Lng = (float)lngAverage,
                            }
                        };

                MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList2, MapInfoDrawTypeEnum.Point, TVTypeEnum.File, tvFileModel.TVFileTVItemID);
                if (!CheckModelOK<MapInfoModel>(mapInfoModelRet)) return false;
            }

            return true;
        }

    }
}
