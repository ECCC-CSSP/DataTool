using CSSPDBDLL;
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
using CSSPEnumsDLL.Enums;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CreateRunsBC()
        {
            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelProv = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "British Columbia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return false;

            List<TVItemModel> tvItemModelSubsectorList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelProv.TVText + "\r\n");
                return false;
            }

            lblStatus.Text = "Starting ... CreateRunsBC";
            Application.DoEvents();

            int StartBCCreateRunsBC = int.Parse(textBoxBCCreateRunsBC.Text);

            List<BCStation> BCWQMSiteList = new List<BCStation>();
            List<AM> analyseMethodInDBList = new List<AM>();
            List<Mat> matrixInDBList = new List<Mat>();
            List<Lab> labInDBList = new List<Lab>();

            int TotalCount = tvItemModelSubsectorList.Count();
            int Count = 0;
            foreach (TVItemModel tvItemModelSubsector in tvItemModelSubsectorList)
            {
                if (Cancel) return false;

                Count += 1;
                lblStatus.Text = (Count * 100 / TotalCount).ToString() + " ... CreateRunsBC for " + tvItemModelSubsector.TVText;
                lblStatus2.Text = Count + " of " + TotalCount;
                Application.DoEvents();

                textBoxBCCreateRunsBC.Text = Count.ToString();

                if (StartBCCreateRunsBC > Count)
                {
                    continue;
                }

                // doing land runs

                List<TempData.BCLandSampleStation> bcLandSampleStation = new List<TempData.BCLandSampleStation>();

                using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                {
                    string TVText = tvItemModelSubsector.TVText.Substring(0, 4);

                    bcLandSampleStation = (from c in dbDT.BCLandSampleStations
                                           where c.SS_SHELLFISH_SECTOR == TVText
                                           orderby c.BCLandSampleStationID
                                           select c).ToList<TempData.BCLandSampleStation>();

                }

                int countSta = 0;
                int totalCountSta = bcLandSampleStation.Count;
                foreach (TempData.BCLandSampleStation bcmss in bcLandSampleStation)
                {
                    if (Cancel) return false;

                    countSta += 1;
                    lblStatus2.Text = "Doing " + countSta + " of " + totalCountSta + " ... CreateRunsBC for " + tvItemModelSubsector.TVText;
                    Application.DoEvents();

                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                    string TVText = bcmss.SS_STATION_CODE;
                    TVItemModel tvItemModelMWQMSite = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelSubsector.TVItemID, TVText, TVTypeEnum.MWQMSite);
                    if (!CheckModelOK<TVItemModel>(tvItemModelMWQMSite)) return false;

                    List<TempData.BCLandSample> bcLandSampleList = new List<TempData.BCLandSample>();

                    using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                    {

                        bcLandSampleList = (from c in dbDT.BCLandSamples
                                            where c.SR_STATION_CODE == bcmss.SS_STATION_CODE
                                            orderby c.SR_READING_DATE
                                            select c).ToList<TempData.BCLandSample>();
                    }

                    foreach (TempData.BCLandSample bcms in bcLandSampleList)
                    {
                        if (Cancel) return false;

                        Application.DoEvents();

                        // doing WQMRun

                        //MWQMSampleModel mwqmSampleModel = new MWQMSampleModel();

                        DateTime DayOfSample = (DateTime)(bcms.SR_READING_DATE);
                        //string SampleTime = bcms.SR_READING_TIME;

                        //if (string.IsNullOrWhiteSpace(SampleTime))
                        //{
                        //    SampleTime = "0000";
                        //}

                        //DateTime SampleDate = new DateTime(DayOfSample.Year, DayOfSample.Month, DayOfSample.Day, (SampleTime.Length == 1 ? 0 : (SampleTime.Length == 3 ? (int.Parse(SampleTime.Substring(0, 1))) : (int.Parse(SampleTime.Substring(0, 1))))), (SampleTime.Length == 1 ? 0 : (SampleTime.Length == 3 ? (int.Parse(SampleTime.Substring(1, 2))) : (SampleTime.Substring(2, 2) == "60" ? 59 : (int.Parse(SampleTime.Substring(2, 2)))))), 0);

                        MWQMRunModel mwqmRunModelNew = new MWQMRunModel()
                        {
                            SubsectorTVItemID = tvItemModelSubsector.TVItemID,
                            DateTime_Local = DayOfSample,
                            StartDateTime_Local = DayOfSample,
                            EndDateTime_Local = DayOfSample,
                            RunSampleType = SampleTypeEnum.Routine,
                            RunNumber = 1,
                        };

                        MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
                        TideSiteService tideSiteService = new TideSiteService(LanguageEnum.en, user);
                        UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);
                        TideDataValueService tideDataValueService = new TideDataValueService(LanguageEnum.en, user);

                        MWQMRunModel mwqmRunModelExist = mwqmRunService.GetMWQMRunModelExistDB(mwqmRunModelNew);
                        if (mwqmRunModelExist == null)
                        {
                            string TVTextRun = DayOfSample.Year.ToString() + " " + 
                                (DayOfSample.Month < 10 ? "0" : "") + DayOfSample.Month.ToString() + " " + 
                                (DayOfSample.Day < 10 ? "0" : "") + DayOfSample.Day.ToString();

                            TVItemModel tvItemModel = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelSubsector.TVItemID, TVTextRun, TVTypeEnum.MWQMRun);
                            if (!string.IsNullOrWhiteSpace(tvItemModel.Error))
                            {
                                tvItemModel = tvItemService.PostAddChildTVItemDB(tvItemModelSubsector.TVItemID, TVTextRun, TVTypeEnum.MWQMRun);
                                if (!string.IsNullOrWhiteSpace(tvItemModel.Error))
                                {
                                    richTextBoxStatus.AppendText(tvItemModel.Error + "\r\n");
                                    return false;
                                }
                            }

                            mwqmRunModelNew.SubsectorTVItemID = tvItemModelSubsector.TVItemID;
                            mwqmRunModelNew.MWQMRunTVItemID = tvItemModel.TVItemID;
                            mwqmRunModelNew.DateTime_Local = DayOfSample;
                            mwqmRunModelNew.StartDateTime_Local = null;
                            mwqmRunModelNew.EndDateTime_Local = null;
                            mwqmRunModelNew.RunSampleType = SampleTypeEnum.Routine;
                            mwqmRunModelNew.RunNumber = 1;

                            string Comments = null;

                            TempData.BCSurvey bcSurvey = new TempData.BCSurvey();

                            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                            {
                                bcSurvey = (from c in dbDT.BCSurveys
                                            where c.S_ID_NUMBER == bcms.SR_SURVEY
                                            select c).FirstOrDefault<TempData.BCSurvey>();
                            }

                            if (bcSurvey != null)
                            {
                                mwqmRunModelNew.StartDateTime_Local = bcSurvey.S_START_DATE;
                                mwqmRunModelNew.EndDateTime_Local = bcSurvey.S_END_DATE;
                                if (mwqmRunModelNew.StartDateTime_Local > mwqmRunModelNew.EndDateTime_Local)
                                {
                                    mwqmRunModelNew.EndDateTime_Local = mwqmRunModelNew.StartDateTime_Local;
                                }
                                Comments = bcSurvey.S_DESCRIPTION + "\r\n" + bcSurvey.S_COMMENT;
                            }

                            string TextEN = "--";
                            if (!string.IsNullOrWhiteSpace(Comments))
                            {
                                TextEN = Comments.Trim();
                            }

                            mwqmRunModelNew.RunComment = TextEN;
                            mwqmRunModelNew.RunWeatherComment = TextEN;
                            mwqmRunModelNew.LabReceivedDateTime_Local = null;
                            mwqmRunModelNew.TemperatureControl1_C = null;
                            mwqmRunModelNew.TemperatureControl2_C = null;
                            mwqmRunModelNew.SeaStateAtStart_BeaufortScale = null;
                            mwqmRunModelNew.SeaStateAtEnd_BeaufortScale = null;
                            mwqmRunModelNew.WaterLevelAtBrook_m = null;
                            mwqmRunModelNew.WaveHightAtStart_m = null;
                            mwqmRunModelNew.WaveHightAtEnd_m = null;
                            mwqmRunModelNew.SampleCrewInitials = null;
                            mwqmRunModelNew.AnalyzeMethod = null;
                            mwqmRunModelNew.SampleMatrix = null;
                            mwqmRunModelNew.Laboratory = null;
                            mwqmRunModelNew.SampleStatus = null;
                            mwqmRunModelNew.LabSampleApprovalContactTVItemID = null;
                            mwqmRunModelNew.LabAnalyzeBath1IncubationStartDateTime_Local = null;
                            mwqmRunModelNew.LabRunSampleApprovalDateTime_Local = null;

                            switch (bcms.SR_ANALYSIS_TYPE)
                            {
                                case "MF":
                                    {
                                        mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.MF;
                                    }
                                    break;
                                case "MPN":
                                    {
                                        mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.MPN;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            switch (bcms.SR_SAMPLE_TYPE)
                            {
                                case "W":
                                    {
                                        mwqmRunModelNew.SampleMatrix = SampleMatrixEnum.W;
                                    }
                                    break;
                                case "S":
                                    {
                                        mwqmRunModelNew.SampleMatrix = SampleMatrixEnum.S;
                                    }
                                    break;
                                case "B":
                                    {
                                        mwqmRunModelNew.SampleMatrix = SampleMatrixEnum.B;
                                    }
                                    break;
                                default:
                                    break;
                            }


                            switch (bcms.SR_SAMPLE_AGENCY)
                            {
                                case 0:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_0;
                                    }
                                    break;
                                case 1:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_1;
                                    }
                                    break;
                                case 2:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_2;
                                    }
                                    break;
                                case 3:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_3;
                                    }
                                    break;
                                case 4:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_4;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            MWQMRunModel mwqmRunModelRet = mwqmRunService.PostAddMWQMRunDB(mwqmRunModelNew);
                            if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return false;
                        }

                        // doing tide


                        //List<UseOfSiteModel> useOfSiteModelList = useOfSiteService.GetUseOfSiteModelListWithSiteTypeAndSubsectorTVItemIDDB(SiteTypeEnum.Tide, tvItemModelSubsector.TVItemID);
                        //if (useOfSiteModelList.Count == 0)
                        //{
                        //    richTextBoxStatus.AppendText("Could not find UseOfTideSite for subsector " + tvItemModelSubsector.TVText + "\r\n");
                        //    return false;
                        //}

                        //TideDataValueModel tideDataValueModelNew = new TideDataValueModel()
                        //{
                        //    TideSiteTVItemID = useOfSiteModelList[0].SiteTVItemID,
                        //    DateTime_Local = new DateTime(SampleDate.Year, SampleDate.Month, SampleDate.Day),
                        //    Keep = true,
                        //    TideDataType = TideDataTypeEnum.Min60,
                        //    StorageDataType = StorageDataTypeEnum.Archived,
                        //    TideStart = null,
                        //    TideEnd = null,
                        //};

                        //if (string.IsNullOrEmpty(bcms.SR_TIDE_CODE))
                        //{
                        //    tideDataValueModelNew.TideStart = TideTextEnum.MidTide;
                        //}
                        //else if (bcms.SR_TIDE_CODE == "L")
                        //{
                        //    tideDataValueModelNew.TideStart = TideTextEnum.LowTide;
                        //}
                        //else if (bcms.SR_TIDE_CODE == "H")
                        //{
                        //    tideDataValueModelNew.TideStart = TideTextEnum.HighTide;
                        //}
                        //else if (bcms.SR_TIDE_CODE == "F")
                        //{
                        //    tideDataValueModelNew.TideStart = TideTextEnum.MidTideRising;
                        //}
                        //else if (bcms.SR_TIDE_CODE == "E")
                        //{
                        //    tideDataValueModelNew.TideStart = TideTextEnum.MidTideFalling;
                        //}
                        //else
                        //{
                        //    tideDataValueModelNew.TideStart = TideTextEnum.MidTide;
                        //}

                        //TideDataValueModel tideDataValueModelRet = tideDataValueService.GetTideDataValueModelExistDB(tideDataValueModelNew);
                        //if (!string.IsNullOrWhiteSpace(tideDataValueModelRet.Error))
                        //{
                        //    tideDataValueModelRet = tideDataValueService.PostAddTideDataValueDB(tideDataValueModelNew);
                        //    if (!CheckModelOK<TideDataValueModel>(tideDataValueModelRet)) return false;

                        //}
                    }
                }

                // doing Marine runs

                List<TempData.BCMarineSampleStation> bcMarineSampleStation = new List<TempData.BCMarineSampleStation>();

                using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                {
                    string TVText = tvItemModelSubsector.TVText.Substring(0, 4);

                    bcMarineSampleStation = (from c in dbDT.BCMarineSampleStations
                                             where c.SS_SHELLFISH_SECTOR == TVText
                                             orderby c.BCMarineSampleStationID
                                             select c).ToList<TempData.BCMarineSampleStation>();

                }


                countSta = 0;
                totalCountSta = bcMarineSampleStation.Count;
                foreach (TempData.BCMarineSampleStation bcmss in bcMarineSampleStation)
                {
                    if (Cancel) return false;

                    countSta += 1;
                    lblStatus2.Text = "Doing " + countSta + " of " + totalCountSta + " ... CreateRunsBC for " + tvItemModelSubsector.TVText;
                    Application.DoEvents();

                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                    string TVText = bcmss.SS_STATION_CODE;
                    TVItemModel tvItemModelMWQMSite = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelSubsector.TVItemID, TVText, TVTypeEnum.MWQMSite);
                    if (!CheckModelOK<TVItemModel>(tvItemModelMWQMSite)) return false;

                    List<TempData.BCMarineSample> bcMarineSampleList = new List<TempData.BCMarineSample>();

                    using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                    {

                        bcMarineSampleList = (from c in dbDT.BCMarineSamples
                                            where c.SR_STATION_CODE == bcmss.SS_STATION_CODE
                                            orderby c.SR_READING_DATE
                                            select c).ToList<TempData.BCMarineSample>();
                    }

                    foreach (TempData.BCMarineSample bcms in bcMarineSampleList)
                    {
                        if (Cancel) return false;

                        Application.DoEvents();

                        // doing WQMRun

                        //MWQMSampleModel mwqmSampleModel = new MWQMSampleModel();

                        DateTime DayOfSample = (DateTime)(bcms.SR_READING_DATE);
                        //string SampleTime = bcms.SR_READING_TIME;

                        //DateTime SampleDate = new DateTime(DayOfSample.Year, DayOfSample.Month, DayOfSample.Day, (SampleTime.Length == 1 ? 0 : (SampleTime.Length == 3 ? (int.Parse(SampleTime.Substring(0, 1))) : (int.Parse(SampleTime.Substring(0, 1))))), (SampleTime.Length == 1 ? 0 : (SampleTime.Length == 3 ? (int.Parse(SampleTime.Substring(1, 2))) : (SampleTime.Substring(2, 2) == "60" ? 59 : (int.Parse(SampleTime.Substring(2, 2)))))), 0);

                        MWQMRunModel mwqmRunModelNew = new MWQMRunModel();

                        MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
                        TideSiteService tideSiteService = new TideSiteService(LanguageEnum.en, user);
                        UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);
                        TideDataValueService tideDataValueService = new TideDataValueService(LanguageEnum.en, user);

                        MWQMRunModel mwqmRunModelNew2 = new MWQMRunModel()
                        {
                            SubsectorTVItemID = tvItemModelSubsector.TVItemID,
                            DateTime_Local = DayOfSample,
                            StartDateTime_Local = DayOfSample,
                            EndDateTime_Local = DayOfSample,
                            RunSampleType = SampleTypeEnum.Routine,
                            RunNumber = 1,
                        };

                        MWQMRunModel mwqmRunModelExist = mwqmRunService.GetMWQMRunModelExistDB(mwqmRunModelNew2);
                        if (!string.IsNullOrWhiteSpace(mwqmRunModelExist.Error))
                        {
                            string TVTextRun = DayOfSample.Year.ToString() + " " +
                               (DayOfSample.Month < 10 ? "0" : "") + DayOfSample.Month.ToString() + " " +
                               (DayOfSample.Day < 10 ? "0" : "") + DayOfSample.Day.ToString();

                            TVItemModel tvItemModel = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelSubsector.TVItemID, TVTextRun, TVTypeEnum.MWQMRun);
                            if (!string.IsNullOrWhiteSpace(tvItemModel.Error))
                            {
                                tvItemModel = tvItemService.PostAddChildTVItemDB(tvItemModelSubsector.TVItemID, TVTextRun, TVTypeEnum.MWQMRun);
                                if (!string.IsNullOrWhiteSpace(tvItemModel.Error))
                                {
                                    richTextBoxStatus.AppendText(tvItemModel.Error + "\r\n");
                                    return false;
                                }
                            }

                            mwqmRunModelNew.SubsectorTVItemID = tvItemModelSubsector.TVItemID;
                            mwqmRunModelNew.MWQMRunTVItemID = tvItemModel.TVItemID;
                            mwqmRunModelNew.DateTime_Local = DayOfSample;
                            mwqmRunModelNew.StartDateTime_Local = null;
                            mwqmRunModelNew.EndDateTime_Local = null;
                            mwqmRunModelNew.RunSampleType = SampleTypeEnum.Routine;
                            mwqmRunModelNew.RunNumber = 1;

                            string Comments = null;

                            TempData.BCSurvey bcSurvey = new TempData.BCSurvey();

                            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                            {
                                bcSurvey = (from c in dbDT.BCSurveys
                                            where c.S_ID_NUMBER == bcms.SR_SURVEY
                                            select c).FirstOrDefault<TempData.BCSurvey>();
                            }

                            if (bcSurvey != null)
                            {
                                mwqmRunModelNew.StartDateTime_Local = bcSurvey.S_START_DATE;
                                mwqmRunModelNew.EndDateTime_Local = bcSurvey.S_END_DATE;
                                if (mwqmRunModelNew.StartDateTime_Local > mwqmRunModelNew.EndDateTime_Local)
                                {
                                    mwqmRunModelNew.EndDateTime_Local = mwqmRunModelNew.StartDateTime_Local;
                                }
                                Comments = bcSurvey.S_DESCRIPTION + "\r\n" + bcSurvey.S_COMMENT;
                            }

                            string TextEN = "--";
                            if (!string.IsNullOrWhiteSpace(Comments))
                            {
                                TextEN = Comments.Trim();
                            }

                            mwqmRunModelNew.RunComment = TextEN;
                            mwqmRunModelNew.RunWeatherComment = TextEN;
                            mwqmRunModelNew.LabReceivedDateTime_Local = null;
                            mwqmRunModelNew.TemperatureControl1_C = null;
                            mwqmRunModelNew.TemperatureControl2_C = null;
                            mwqmRunModelNew.SeaStateAtStart_BeaufortScale = null;
                            mwqmRunModelNew.SeaStateAtEnd_BeaufortScale = null;
                            mwqmRunModelNew.WaterLevelAtBrook_m = null;
                            mwqmRunModelNew.WaveHightAtStart_m = null;
                            mwqmRunModelNew.WaveHightAtEnd_m = null;
                            mwqmRunModelNew.SampleCrewInitials = null;
                            mwqmRunModelNew.AnalyzeMethod = null;
                            mwqmRunModelNew.SampleMatrix = null;
                            mwqmRunModelNew.Laboratory = null;
                            mwqmRunModelNew.SampleStatus = null;
                            mwqmRunModelNew.LabSampleApprovalContactTVItemID = null;
                            mwqmRunModelNew.LabAnalyzeBath1IncubationStartDateTime_Local = null;
                            mwqmRunModelNew.LabRunSampleApprovalDateTime_Local = null;

                            switch (bcms.SR_ANALYSIS_TYPE)
                            {
                                case "MF":
                                    {
                                        mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.MF;
                                    }
                                    break;
                                case "MPN":
                                    {
                                        mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.MPN;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            switch (bcms.SR_SAMPLE_TYPE)
                            {
                                case "W":
                                    {
                                        mwqmRunModelNew.SampleMatrix = SampleMatrixEnum.W;
                                    }
                                    break;
                                case "S":
                                    {
                                        mwqmRunModelNew.SampleMatrix = SampleMatrixEnum.S;
                                    }
                                    break;
                                case "B":
                                    {
                                        mwqmRunModelNew.SampleMatrix = SampleMatrixEnum.B;
                                    }
                                    break;
                                default:
                                    break;
                            }


                            switch (bcms.SR_SAMPLE_AGENCY)
                            {
                                case 0:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_0;
                                    }
                                    break;
                                case 1:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_1;
                                    }
                                    break;
                                case 2:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_2;
                                    }
                                    break;
                                case 3:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_3;
                                    }
                                    break;
                                case 4:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_4;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            MWQMRunModel mwqmRunModelRet = mwqmRunService.PostAddMWQMRunDB(mwqmRunModelNew);
                            if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return false;
                        }

                        // doing tide

                        //List<UseOfSiteModel> useOfSiteModelList = useOfSiteService.GetUseOfSiteModelListWithSiteTypeAndSubsectorTVItemIDDB(SiteTypeEnum.Tide, tvItemModelSubsector.TVItemID);
                        //if (useOfSiteModelList.Count == 0)
                        //{
                        //    richTextBoxStatus.AppendText("Could not find UseOfTideSite for subsector " + tvItemModelSubsector.TVText + "\r\n");
                        //    return false;
                        //}

                        //TideDataValueModel tideDataValueModelNew = new TideDataValueModel()
                        //{
                        //    TideSiteTVItemID = useOfSiteModelList[0].SiteTVItemID,
                        //    DateTime_Local = new DateTime(SampleDate.Year, SampleDate.Month, SampleDate.Day),
                        //    Keep = true,
                        //    TideDataType = TideDataTypeEnum.Min60,
                        //    StorageDataType = StorageDataTypeEnum.Archived,
                        //    TideStart = null,
                        //    TideEnd = null,
                        //};


                        //if (string.IsNullOrEmpty(bcms.SR_TIDE_CODE))
                        //{
                        //    tideDataValueModelNew.TideStart = TideTextEnum.MidTide;
                        //}
                        //else if (bcms.SR_TIDE_CODE == "L")
                        //{
                        //    tideDataValueModelNew.TideStart = TideTextEnum.LowTide;
                        //}
                        //else if (bcms.SR_TIDE_CODE == "H")
                        //{
                        //    tideDataValueModelNew.TideStart = TideTextEnum.HighTide;
                        //}
                        //else if (bcms.SR_TIDE_CODE == "F")
                        //{
                        //    tideDataValueModelNew.TideStart = TideTextEnum.MidTideRising;
                        //}
                        //else if (bcms.SR_TIDE_CODE == "E")
                        //{
                        //    tideDataValueModelNew.TideStart = TideTextEnum.MidTideFalling;
                        //}
                        //else
                        //{
                        //    tideDataValueModelNew.TideStart = TideTextEnum.MidTide;
                        //}
                        //TideDataValueModel tideDataValueModelRet = tideDataValueService.GetTideDataValueModelExistDB(tideDataValueModelNew);
                        //if (!string.IsNullOrWhiteSpace(tideDataValueModelRet.Error))
                        //{
                        //    tideDataValueModelRet = tideDataValueService.PostAddTideDataValueDB(tideDataValueModelNew);
                        //    if (!CheckModelOK<TideDataValueModel>(tideDataValueModelRet)) return false;

                        //}
                    }
                }
            }

            return true;
        }
    }
}
