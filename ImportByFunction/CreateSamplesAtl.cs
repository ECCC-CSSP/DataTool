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
using CSSPWebToolsDBDLL;
using CSSPModelsDLL.Models;
using CSSPWebToolsDBDLL.Services;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CreateSamplesAtl(string Prov, string ShortProv)
        {
            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);
            MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);

            TVItemModel tvItemModelArea = new TVItemModel();
            TVItemModel tvItemModelSector = new TVItemModel();
            TVItemModel tvItemModelSubsector = new TVItemModel();
            TVItemModel tvItemModelQC = new TVItemModel();

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelProv = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, Prov, TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return false;

            List<TVItemModel> TVItemModelSubsectorList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            if (TVItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelProv.TVText + "\r\n");
                return false;
            }

            //int StartNewOrdinal = 100;
            lblStatus.Text = "Staring ... CreateSamplesAll";
            Application.DoEvents();

            int StartNBCreateSamplesAtl = int.Parse(textBoxNBCreateSamplesAtl.Text);
            int StartNLCreateSamplesAtl = int.Parse(textBoxNLCreateSamplesAtl.Text);
            int StartNSCreateSamplesAtl = int.Parse(textBoxNSCreateSamplesAtl.Text);
            int StartPECreateSamplesAtl = int.Parse(textBoxPECreateSamplesAtl.Text);


            Dictionary<string, int> SubsectorDict = new Dictionary<string, int>();
            Dictionary<string, int> MWQMSiteDict = new Dictionary<string, int>();
            Dictionary<string, int> MWQMRunDict = new Dictionary<string, int>();

            #region LoadVariables

            SubsectorDict.Clear();
            foreach (TVItemModel tvItemModel in TVItemModelSubsectorList)
            {
                SubsectorDict.Add(tvItemModel.TVText.Substring(0, 13), tvItemModel.TVItemID);
            }

            if (SubsectorDict.Count != TVItemModelSubsectorList.Count)
            {
                richTextBoxStatus.AppendText("Error: should have the same number of item in SubsectorDict and TempSubsectorValueList\r\n");
                return false;
            }

            MWQMSiteDict.Clear();
            foreach (KeyValuePair<string, int> kvp in SubsectorDict)
            {
                lblStatus.Text = kvp.Key;
                lblStatus.Refresh();
                Application.DoEvents();

                TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                // filling the MWQMSite global variable for future use
                List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(kvp.Value, TVTypeEnum.MWQMSite);

                foreach (TVItemModel tvItemModel in tvItemModelMWQMSiteList)
                {
                    MWQMSiteDict.Add(kvp.Key + tvItemModel.TVText, tvItemModel.TVItemID);
                }

                // filling the MWQMRun global variable for future use
                List<TVItemModel> tvItemModelMWQMRunList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(kvp.Value, TVTypeEnum.MWQMRun);

                foreach (TVItemModel tvItemModel in tvItemModelMWQMRunList)
                {
                    //try
                    //{
                    //    int ExistingTVItemID = MWQMRunDict[kvp.Key + tvItemModel.TVText];
                    //    if (ExistingTVItemID > 0)
                    //    {
                    //        List<MWQMSampleModel> mwqmSampleModelList2 = mwqmSampleService.GetMWQMSampleModelListWithMWQMRunTVItemIDDB(tvItemModel.TVItemID);
                    //        if (mwqmSampleModelList2.Count == 0)
                    //        {
                    //            MWQMRunModel mwqmRunModel = mwqmRunService.PostDeleteMWQMRunTVItemIDDB(tvItemModel.TVItemID);
                    //            if (!string.IsNullOrWhiteSpace(mwqmRunModel.Error))
                    //            {
                    //                // could be non existant
                    //            }
                    //            TVItemModel tvItemModelRet = tvItemService.PostDeleteTVItemWithTVItemIDDB(tvItemModel.TVItemID);
                    //            if (!string.IsNullOrWhiteSpace(tvItemModelRet.Error))
                    //            {
                    //                // could be non existant
                    //            }
                    //        }

                    //        mwqmSampleModelList2 = mwqmSampleService.GetMWQMSampleModelListWithMWQMRunTVItemIDDB(ExistingTVItemID);
                    //        if (mwqmSampleModelList2.Count == 0)
                    //        {
                    //            MWQMRunModel mwqmRunModel = mwqmRunService.PostDeleteMWQMRunTVItemIDDB(ExistingTVItemID);
                    //            if (!string.IsNullOrWhiteSpace(mwqmRunModel.Error))
                    //            {
                    //                // could be non existant
                    //            }
                    //            TVItemModel tvItemModelRet = tvItemService.PostDeleteTVItemWithTVItemIDDB(ExistingTVItemID);
                    //            if (!string.IsNullOrWhiteSpace(tvItemModelRet.Error))
                    //            {
                    //                // could be non existant
                    //            }
                    //        }
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    //  good it does not exist then add it
                    MWQMRunDict.Add(kvp.Key + tvItemModel.TVText, tvItemModel.TVItemID);
                    //}
                }
            }

            #endregion LoadVariables

            List<MWQMSampleModel> mwqmSampleModelList = new List<MWQMSampleModel>();
            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                List<TempData.ASGADSample> sampList = (from c in dbDT.ASGADSamples
                                                       orderby c.ASGADSampleID
                                                       where c.PROV == ShortProv
                                                       orderby c.PROV, c.AREA, c.SECTOR, c.SUBSECTOR, c.STAT_NBR, c.SAMP_DATE
                                                       select c).ToList<TempData.ASGADSample>();

                int TotalCount = sampList.Count();
                int Count = 0;
                string OldLocator = "";
                int OldSiteTVItemID = 0;
                foreach (TempData.ASGADSample asamp in sampList)
                {

                    if (Cancel) return false;

                    Count += 1;
                    lblStatus.Text = (Count * 100 / TotalCount).ToString() + "% .. LoadSamplesAll for " + Prov;
                    lblStatus2.Text = Count + " of " + TotalCount + " ... LoadSamplesAll for " + Prov;
                    Application.DoEvents();

                    //if (!(asamp.PROV == "NB" && asamp.AREA == "06" && asamp.SECTOR == "020" && asamp.SUBSECTOR == "002" && asamp.SAMP_DATE == new DateTime(2014, 6, 2)))
                    //    continue;

                    switch (ShortProv)
                    {
                        case "NB":
                            {
                                textBoxNBCreateSamplesAtl.Text = Count.ToString();
                                if (StartNBCreateSamplesAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "NL":
                            {
                                textBoxNLCreateSamplesAtl.Text = Count.ToString();
                                if (StartNLCreateSamplesAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "NS":
                            {
                                textBoxNSCreateSamplesAtl.Text = Count.ToString();
                                if (StartNSCreateSamplesAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "PE":
                            {
                                textBoxPECreateSamplesAtl.Text = Count.ToString();
                                if (StartPECreateSamplesAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    string Locator = asamp.PROV + "-" + asamp.AREA + "-" + asamp.SECTOR + "-" + asamp.SUBSECTOR;
                    int SubsectorTVItemID = TVItemModelSubsectorList.Where(c => c.TVText.StartsWith(Locator + " ")).Select(c => c.TVItemID).FirstOrDefault();
                    int TVItemID = 0;
                    try
                    {
                        TVItemID = MWQMSiteDict[Locator + asamp.STAT_NBR];
                    }
                    catch (Exception ex)
                    {
                        TVItemModel tvItemModelSite = tvItemServiceR.PostAddChildTVItemDB(SubsectorTVItemID, asamp.STAT_NBR, TVTypeEnum.MWQMSite);
                        if (!string.IsNullOrWhiteSpace(tvItemModelSite.Error))
                        {
                            return false;
                        }
                        MWQMSiteModel mwqmSiteModelNew = new MWQMSiteModel()
                        {
                            MWQMSiteNumber = asamp.STAT_NBR,
                            MWQMSiteTVText = asamp.STAT_NBR,
                            Ordinal = 0,
                            MWQMSiteDescription = "",
                            MWQMSiteLatestClassification = MWQMSiteLatestClassificationEnum.Unclassified,
                            MWQMSiteTVItemID = tvItemModelSite.TVItemID
                        };

                        MWQMSiteModel mwqmSiteModelRet = mwqmSiteService.PostAddMWQMSiteDB(mwqmSiteModelNew);
                        if (!string.IsNullOrWhiteSpace(mwqmSiteModelRet.Error))
                        {
                            return false;
                        }
                        TVItemID = mwqmSiteModelRet.MWQMSiteTVItemID;

                        MWQMSiteDict.Add(Locator + asamp.STAT_NBR, TVItemID);
                    }

                    if (OldLocator != Locator || OldSiteTVItemID != TVItemID)
                    {
                        mwqmSampleModelList = mwqmSampleService.GetMWQMSampleModelListWithMWQMSiteTVItemIDDB(TVItemID);
                        OldLocator = Locator;
                        OldSiteTVItemID = TVItemID;
                    }
                    //try
                    //{
                    //    int TVItemID2 = MWQMSiteDict[Locator + asamp.STAT_NBR];
                    //}
                    //catch (Exception)
                    //{
                    //    richTextBoxStatus.AppendText(Locator + " --- " + asamp.STAT_NBR + "\r\n");
                    //    continue;
                    //    //return false;
                    //}

                    DateTime SampDate = ((DateTime)asamp.SAMP_DATE);
                    MWQMSampleModel mwqmSampleModelNew = new MWQMSampleModel()
                    {
                        MWQMSiteTVItemID = TVItemID,
                        SampleDateTime_Local = SampDate,
                        FecCol_MPN_100ml = (asamp.FEC_COL == 1.9 ? (int)1 : (int)asamp.FEC_COL),
                        Salinity_PPT = asamp.SALINITY,
                        WaterTemp_C = (asamp.WATERTEMP < 0 ? 0 : asamp.WATERTEMP),
                        SampleTypesText = ((int)SampleTypeEnum.Routine).ToString() + ",",
                        SampleTypeList = new List<SampleTypeEnum>() { SampleTypeEnum.Routine },
                    };

                    int Hour = 0;
                    int Minute = 0;
                    if (!string.IsNullOrWhiteSpace(asamp.SAMP_TIME))
                    {
                        if (asamp.SAMP_TIME.Length < 4)
                        {
                            richTextBoxStatus.AppendText("asamp.SAMP_TIME.Length < 3. Please fix in ASGADSamples");
                            break;
                        }
                        if (!int.TryParse(asamp.SAMP_TIME.Substring(0, 2), out Hour))
                        {
                            break;
                        }
                        if (!int.TryParse(asamp.SAMP_TIME.Substring(2, 2), out Minute))
                        {
                            if (asamp.SAMP_TIME == "13.8")
                            {
                                Minute = 8;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (Hour < 0)
                        {
                            Hour = 0;
                        }
                        if (Hour > 23)
                        {
                            Hour = 23;
                        }
                        if (Minute < 0)
                        {
                            Minute = 0;
                        }
                        if (Minute > 59)
                        {
                            Minute = 59;
                        }

                        mwqmSampleModelNew.SampleDateTime_Local = new DateTime(mwqmSampleModelNew.SampleDateTime_Local.Year, mwqmSampleModelNew.SampleDateTime_Local.Month, mwqmSampleModelNew.SampleDateTime_Local.Day, Hour, Minute, 0);
                    }

                    if (asamp.WATERTEMP == 65.0)
                    {
                        mwqmSampleModelNew.WaterTemp_C = 6.5f;
                    }


                    int Year = mwqmSampleModelNew.SampleDateTime_Local.Year;
                    int Month = mwqmSampleModelNew.SampleDateTime_Local.Month;
                    int Day = mwqmSampleModelNew.SampleDateTime_Local.Day;
                    Hour = mwqmSampleModelNew.SampleDateTime_Local.Hour;
                    Minute = mwqmSampleModelNew.SampleDateTime_Local.Minute;

                    MWQMRunModel mwqmRunModelToCheck = new MWQMRunModel()
                    {
                        SubsectorTVItemID = SubsectorTVItemID,
                        DateTime_Local = new DateTime(Year, Month, Day),
                        RunSampleType = SampleTypeEnum.Routine,
                        RunNumber = 1
                    };

                    string DateText = Year + " " + (Month > 9 ? Month.ToString() : "0" + Month.ToString()) + " " + (Day > 9 ? Day.ToString() : "0" + Day.ToString());
                    try
                    {
                        mwqmSampleModelNew.MWQMRunTVItemID = MWQMRunDict[Locator + DateText];
                    }
                    catch (Exception)
                    {
                        int selij = 232;
                        //MWQMRunModel mwqmRunModelRet = mwqmRunService.PostAddMWQMRunDB(mwqmRunModelToCheck);
                    }

                    MWQMSampleModel mwqmSampleModelExist = (from c in mwqmSampleModelList
                                                            where c.MWQMSiteTVItemID == mwqmSampleModelNew.MWQMSiteTVItemID
                                                            //&& c.MWQMRunTVItemID == mwqmSampleModelNew.MWQMRunTVItemID
                                                            && c.SampleDateTime_Local == mwqmSampleModelNew.SampleDateTime_Local
                                                            select c).FirstOrDefault();


                    if (mwqmSampleModelExist == null) // should add
                    {
                        MWQMSampleModel mwqmSampleModelRet = mwqmSampleService.PostAddMWQMSampleDB(mwqmSampleModelNew);
                        if (!CheckModelOK<MWQMSampleModel>(mwqmSampleModelRet)) return false;
                    }
                    else
                    {
                        if (mwqmSampleModelNew.MWQMRunTVItemID != mwqmSampleModelExist.MWQMRunTVItemID
                            || mwqmSampleModelNew.FecCol_MPN_100ml != mwqmSampleModelExist.FecCol_MPN_100ml
                            || mwqmSampleModelNew.Salinity_PPT != mwqmSampleModelExist.Salinity_PPT
                            || mwqmSampleModelNew.WaterTemp_C != mwqmSampleModelExist.WaterTemp_C
                            || mwqmSampleModelNew.SampleTypesText != mwqmSampleModelExist.SampleTypesText)
                        {
                            mwqmSampleModelExist.MWQMRunTVItemID = mwqmSampleModelNew.MWQMRunTVItemID;
                            mwqmSampleModelExist.FecCol_MPN_100ml = mwqmSampleModelNew.FecCol_MPN_100ml;
                            mwqmSampleModelExist.Salinity_PPT = mwqmSampleModelNew.Salinity_PPT;
                            mwqmSampleModelExist.WaterTemp_C = mwqmSampleModelNew.WaterTemp_C;
                            mwqmSampleModelExist.SampleTypesText = mwqmSampleModelNew.SampleTypesText;

                            MWQMSampleModel mwqmSampleModelRet = mwqmSampleService.PostUpdateMWQMSampleDB(mwqmSampleModelExist);
                            if (!CheckModelOK<MWQMSampleModel>(mwqmSampleModelRet)) return false;
                        }
                    }
                }
            }

            return true;
        }
        public bool CreateSamplesAtlMOU(string Prov, string ShortProv)
        {
            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);
            MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);

            TVItemModel tvItemModelArea = new TVItemModel();
            TVItemModel tvItemModelSector = new TVItemModel();
            TVItemModel tvItemModelSubsector = new TVItemModel();
            TVItemModel tvItemModelQC = new TVItemModel();

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelProv = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, Prov, TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return false;

            List<TVItemModel> TVItemModelSubsectorList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            if (TVItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelProv.TVText + "\r\n");
                return false;
            }

            //int StartNewOrdinal = 100;
            lblStatus.Text = "Staring ... CreateSamplesAll";
            Application.DoEvents();

            int StartNBCreateSamplesAtl = int.Parse(textBoxNBCreateSamplesAtl.Text);
            int StartNLCreateSamplesAtl = int.Parse(textBoxNLCreateSamplesAtl.Text);
            int StartNSCreateSamplesAtl = int.Parse(textBoxNSCreateSamplesAtl.Text);
            int StartPECreateSamplesAtl = int.Parse(textBoxPECreateSamplesAtl.Text);


            Dictionary<string, int> SubsectorDict = new Dictionary<string, int>();
            Dictionary<string, int> MWQMSiteDict = new Dictionary<string, int>();
            Dictionary<string, int> MWQMRunDict = new Dictionary<string, int>();

            #region LoadVariables

            SubsectorDict.Clear();
            foreach (TVItemModel tvItemModel in TVItemModelSubsectorList)
            {
                SubsectorDict.Add(tvItemModel.TVText.Substring(0, 13), tvItemModel.TVItemID);
            }

            if (SubsectorDict.Count != TVItemModelSubsectorList.Count)
            {
                richTextBoxStatus.AppendText("Error: should have the same number of item in SubsectorDict and TempSubsectorValueList\r\n");
                return false;
            }

            MWQMSiteDict.Clear();
            foreach (KeyValuePair<string, int> kvp in SubsectorDict)
            {
                lblStatus.Text = kvp.Key;
                lblStatus.Refresh();
                Application.DoEvents();

                TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                // filling the MWQMSite global variable for future use
                List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(kvp.Value, TVTypeEnum.MWQMSite);

                foreach (TVItemModel tvItemModel in tvItemModelMWQMSiteList)
                {
                    MWQMSiteDict.Add(kvp.Key + tvItemModel.TVText, tvItemModel.TVItemID);
                }

                // filling the MWQMRun global variable for future use
                List<TVItemModel> tvItemModelMWQMRunList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(kvp.Value, TVTypeEnum.MWQMRun);

                foreach (TVItemModel tvItemModel in tvItemModelMWQMRunList)
                {
                    //try
                    //{
                    //    int ExistingTVItemID = MWQMRunDict[kvp.Key + tvItemModel.TVText];
                    //    if (ExistingTVItemID > 0)
                    //    {
                    //        List<MWQMSampleModel> mwqmSampleModelList2 = mwqmSampleService.GetMWQMSampleModelListWithMWQMRunTVItemIDDB(tvItemModel.TVItemID);
                    //        if (mwqmSampleModelList2.Count == 0)
                    //        {
                    //            MWQMRunModel mwqmRunModel = mwqmRunService.PostDeleteMWQMRunTVItemIDDB(tvItemModel.TVItemID);
                    //            if (!string.IsNullOrWhiteSpace(mwqmRunModel.Error))
                    //            {
                    //                // could be non existant
                    //            }
                    //            TVItemModel tvItemModelRet = tvItemService.PostDeleteTVItemWithTVItemIDDB(tvItemModel.TVItemID);
                    //            if (!string.IsNullOrWhiteSpace(tvItemModelRet.Error))
                    //            {
                    //                // could be non existant
                    //            }
                    //        }

                    //        mwqmSampleModelList2 = mwqmSampleService.GetMWQMSampleModelListWithMWQMRunTVItemIDDB(ExistingTVItemID);
                    //        if (mwqmSampleModelList2.Count == 0)
                    //        {
                    //            MWQMRunModel mwqmRunModel = mwqmRunService.PostDeleteMWQMRunTVItemIDDB(ExistingTVItemID);
                    //            if (!string.IsNullOrWhiteSpace(mwqmRunModel.Error))
                    //            {
                    //                // could be non existant
                    //            }
                    //            TVItemModel tvItemModelRet = tvItemService.PostDeleteTVItemWithTVItemIDDB(ExistingTVItemID);
                    //            if (!string.IsNullOrWhiteSpace(tvItemModelRet.Error))
                    //            {
                    //                // could be non existant
                    //            }
                    //        }
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    //  good it does not exist then add it
                    MWQMRunDict.Add(kvp.Key + tvItemModel.TVText, tvItemModel.TVItemID);
                    //}
                }
            }

            #endregion LoadVariables

            List<MWQMSampleModel> mwqmSampleModelList = new List<MWQMSampleModel>();
            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                List<TempData.ASGADSampleMOU> sampMOUList = (from c in dbDT.ASGADSampleMOUs
                                                             orderby c.ASGADSampleMOUID
                                                             where c.PROV == ShortProv
                                                             orderby c.PROV, c.AREA, c.SECTOR, c.SUBSECTOR, c.STAT_NBR, c.SAMP_DATE
                                                             select c).ToList<TempData.ASGADSampleMOU>();

                int TotalCount = sampMOUList.Count();
                int Count = 0;
                string OldLocator = "";
                int OldSiteTVItemID = 0;
                foreach (TempData.ASGADSampleMOU asamp in sampMOUList)
                {

                    if (Cancel) return false;

                    Count += 1;
                    lblStatus.Text = (Count * 100 / TotalCount).ToString() + "% .. LoadSamplesAll for " + Prov;
                    lblStatus2.Text = Count + " of " + TotalCount + " ... LoadSamplesAll for " + Prov;
                    Application.DoEvents();

                    //if (!(asamp.PROV == "NB" && asamp.AREA == "06" && asamp.SECTOR == "020" && asamp.SUBSECTOR == "002" && asamp.SAMP_DATE == new DateTime(2014, 6, 2)))
                    //    continue;

                    switch (ShortProv)
                    {
                        case "NB":
                            {
                                textBoxNBCreateSamplesAtl.Text = Count.ToString();
                                if (StartNBCreateSamplesAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "NL":
                            {
                                textBoxNLCreateSamplesAtl.Text = Count.ToString();
                                if (StartNLCreateSamplesAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "NS":
                            {
                                textBoxNSCreateSamplesAtl.Text = Count.ToString();
                                if (StartNSCreateSamplesAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "PE":
                            {
                                textBoxPECreateSamplesAtl.Text = Count.ToString();
                                if (StartPECreateSamplesAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    string Locator = asamp.PROV + "-" + asamp.AREA + "-" + asamp.SECTOR + "-" + asamp.SUBSECTOR;
                    int SubsectorTVItemID = TVItemModelSubsectorList.Where(c => c.TVText.StartsWith(Locator + " ")).Select(c => c.TVItemID).FirstOrDefault();
                    int TVItemID = 0;
                    try
                    {
                        TVItemID = MWQMSiteDict[Locator + asamp.STAT_NBR];
                    }
                    catch (Exception ex)
                    {
                        TVItemModel tvItemModelSite = tvItemServiceR.PostAddChildTVItemDB(SubsectorTVItemID, asamp.STAT_NBR, TVTypeEnum.MWQMSite);
                        if (!string.IsNullOrWhiteSpace(tvItemModelSite.Error))
                        {
                            return false;
                        }
                        MWQMSiteModel mwqmSiteModelNew = new MWQMSiteModel()
                        {
                            MWQMSiteNumber = asamp.STAT_NBR,
                            MWQMSiteTVText = asamp.STAT_NBR,
                            Ordinal = 0,
                            MWQMSiteDescription = "",
                            MWQMSiteLatestClassification = MWQMSiteLatestClassificationEnum.Unclassified,
                            MWQMSiteTVItemID = tvItemModelSite.TVItemID
                        };

                        MWQMSiteModel mwqmSiteModelRet = mwqmSiteService.PostAddMWQMSiteDB(mwqmSiteModelNew);
                        if (!string.IsNullOrWhiteSpace(mwqmSiteModelRet.Error))
                        {
                            return false;
                        }
                        TVItemID = mwqmSiteModelRet.MWQMSiteTVItemID;

                        MWQMSiteDict.Add(Locator + asamp.STAT_NBR, TVItemID);
                    }

                    if (OldLocator != Locator || OldSiteTVItemID != TVItemID)
                    {
                        mwqmSampleModelList = mwqmSampleService.GetMWQMSampleModelListWithMWQMSiteTVItemIDDB(TVItemID);
                        OldLocator = Locator;
                        OldSiteTVItemID = TVItemID;
                    }
                    //try
                    //{
                    //    int TVItemID2 = MWQMSiteDict[Locator + asamp.STAT_NBR];
                    //}
                    //catch (Exception)
                    //{
                    //    richTextBoxStatus.AppendText(Locator + " --- " + asamp.STAT_NBR + "\r\n");
                    //    continue;
                    //    //return false;
                    //}

                    DateTime SampDate = ((DateTime)asamp.SAMP_DATE);
                    MWQMSampleModel mwqmSampleModelNew = new MWQMSampleModel()
                    {
                        MWQMSiteTVItemID = TVItemID,
                        SampleDateTime_Local = SampDate,
                        FecCol_MPN_100ml = (asamp.FEC_COL == 1.9 ? (int)1 : (int)asamp.FEC_COL),
                        Salinity_PPT = asamp.SALINITY,
                        WaterTemp_C = (asamp.WATERTEMP < 0 ? 0 : asamp.WATERTEMP),
                        SampleTypesText = ((int)SampleTypeEnum.RainCMPRoutine).ToString() + ",",
                        SampleTypeList = new List<SampleTypeEnum>() { SampleTypeEnum.RainCMPRoutine },
                    };

                    int Hour = 0;
                    int Minute = 0;
                    if (!string.IsNullOrWhiteSpace(asamp.SAMP_TIME))
                    {
                        if (asamp.SAMP_TIME.Length < 4)
                        {
                            richTextBoxStatus.AppendText("asamp.SAMP_TIME.Length < 3. Please fix in ASGADSamples");
                            break;
                        }
                        if (!int.TryParse(asamp.SAMP_TIME.Substring(0, 2), out Hour))
                        {
                            break;
                        }
                        if (!int.TryParse(asamp.SAMP_TIME.Substring(2, 2), out Minute))
                        {
                            if (asamp.SAMP_TIME == "13.8")
                            {
                                Minute = 8;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (Hour < 0)
                        {
                            Hour = 0;
                        }
                        if (Hour > 23)
                        {
                            Hour = 23;
                        }
                        if (Minute < 0)
                        {
                            Minute = 0;
                        }
                        if (Minute > 59)
                        {
                            Minute = 59;
                        }

                        mwqmSampleModelNew.SampleDateTime_Local = new DateTime(mwqmSampleModelNew.SampleDateTime_Local.Year, mwqmSampleModelNew.SampleDateTime_Local.Month, mwqmSampleModelNew.SampleDateTime_Local.Day, Hour, Minute, 0);
                    }

                    if (asamp.WATERTEMP == 65.0)
                    {
                        mwqmSampleModelNew.WaterTemp_C = 6.5f;
                    }


                    int Year = mwqmSampleModelNew.SampleDateTime_Local.Year;
                    int Month = mwqmSampleModelNew.SampleDateTime_Local.Month;
                    int Day = mwqmSampleModelNew.SampleDateTime_Local.Day;
                    Hour = mwqmSampleModelNew.SampleDateTime_Local.Hour;
                    Minute = mwqmSampleModelNew.SampleDateTime_Local.Minute;

                    MWQMRunModel mwqmRunModelToCheck = new MWQMRunModel()
                    {
                        SubsectorTVItemID = SubsectorTVItemID,
                        DateTime_Local = new DateTime(Year, Month, Day),
                        RunSampleType = SampleTypeEnum.RainCMPRoutine,
                        RunNumber = 1
                    };

                    string DateText = Year + " " + (Month > 9 ? Month.ToString() : "0" + Month.ToString()) + " " + (Day > 9 ? Day.ToString() : "0" + Day.ToString());
                    try
                    {
                        mwqmSampleModelNew.MWQMRunTVItemID = MWQMRunDict[Locator + DateText];
                    }
                    catch (Exception)
                    {
                        int selij = 232;
                        //MWQMRunModel mwqmRunModelRet = mwqmRunService.PostAddMWQMRunDB(mwqmRunModelToCheck);
                    }

                    MWQMSampleModel mwqmSampleModelExist = (from c in mwqmSampleModelList
                                                            where c.MWQMSiteTVItemID == mwqmSampleModelNew.MWQMSiteTVItemID
                                                            //&& c.MWQMRunTVItemID == mwqmSampleModelNew.MWQMRunTVItemID
                                                            && c.SampleDateTime_Local == mwqmSampleModelNew.SampleDateTime_Local
                                                            select c).FirstOrDefault();


                    if (mwqmSampleModelExist == null) // should add
                    {
                        MWQMSampleModel mwqmSampleModelRet = mwqmSampleService.PostAddMWQMSampleDB(mwqmSampleModelNew);
                        if (!CheckModelOK<MWQMSampleModel>(mwqmSampleModelRet)) return false;
                    }
                    else
                    {
                        if (mwqmSampleModelNew.MWQMRunTVItemID != mwqmSampleModelExist.MWQMRunTVItemID
                            || mwqmSampleModelNew.FecCol_MPN_100ml != mwqmSampleModelExist.FecCol_MPN_100ml
                            || mwqmSampleModelNew.Salinity_PPT != mwqmSampleModelExist.Salinity_PPT
                            || mwqmSampleModelNew.WaterTemp_C != mwqmSampleModelExist.WaterTemp_C
                            || mwqmSampleModelNew.SampleTypesText != mwqmSampleModelExist.SampleTypesText)
                        {
                            mwqmSampleModelExist.MWQMRunTVItemID = mwqmSampleModelNew.MWQMRunTVItemID;
                            mwqmSampleModelExist.FecCol_MPN_100ml = mwqmSampleModelNew.FecCol_MPN_100ml;
                            mwqmSampleModelExist.Salinity_PPT = mwqmSampleModelNew.Salinity_PPT;
                            mwqmSampleModelExist.WaterTemp_C = mwqmSampleModelNew.WaterTemp_C;
                            mwqmSampleModelExist.SampleTypesText = mwqmSampleModelNew.SampleTypesText;

                            MWQMSampleModel mwqmSampleModelRet = mwqmSampleService.PostUpdateMWQMSampleDB(mwqmSampleModelExist);
                            if (!CheckModelOK<MWQMSampleModel>(mwqmSampleModelRet)) return false;
                        }
                    }
                }
            }

            return true;
        }
        private Coord GetMaxLatLng(string Locator)
        {
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            MWQMSubsectorService mwqmSubsectorService = new MWQMSubsectorService(LanguageEnum.en, user);

            Coord coord = new Coord();

            MWQMSubsectorModel mwqmSubsectorModel = mwqmSubsectorService.GetMWQMSubsectorModelWithSubsectorHistoricKeyDB(Locator);
            if (!CheckModelOK<MWQMSubsectorModel>(mwqmSubsectorModel)) return new Coord() { Lat = 0, Lng = 0, Ordinal = 0 };

            List<MapInfoModel> mapInfoModelList = mapInfoService.GetMapInfoModelListWithTVItemIDDB(mwqmSubsectorModel.MWQMSubsectorTVItemID);
            if (mapInfoModelList.Count == 0) return new Coord() { Lat = 0, Lng = 0, Ordinal = 0 };

            coord.Lat = (float)(from m in mapInfoModelList select m.LatMax).Max();
            coord.Lng = (float)(from m in mapInfoModelList select m.LngMax).Max();

            return coord;
        }
    }
}
