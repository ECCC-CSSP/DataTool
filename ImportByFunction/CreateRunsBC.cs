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
                                           where c.SS_SHELLFI == TVText
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

                    string TVText = bcmss.SS_STATION;
                    TVItemModel tvItemModelMWQMSite = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelSubsector.TVItemID, TVText, TVTypeEnum.MWQMSite);
                    if (!CheckModelOK<TVItemModel>(tvItemModelMWQMSite)) return false;

                    List<TempData.BCLandSample> bcLandSampleList = new List<TempData.BCLandSample>();

                    using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                    {

                        bcLandSampleList = (from c in dbDT.BCLandSamples
                                            where c.SR_STATION_CODE == bcmss.SS_STATION
                                            orderby c.SR_READING_DATE
                                            select c).ToList<TempData.BCLandSample>();
                    }



                    foreach (TempData.BCLandSample bcms in bcLandSampleList)
                    {
                        if (Cancel) return false;

                        Application.DoEvents();


                        DateTime DayOfSample = (DateTime)(bcms.SR_READING_DATE);

                        SampleTypeEnum sampleType = SampleTypeEnum.Routine;

                        if (bcms.SR_SAMPLE_TYPE == "S")
                        {
                            sampleType = SampleTypeEnum.Sediment;
                        }

                        if (bcms.SR_SAMPLE_TYPE == "B")
                        {
                            sampleType = SampleTypeEnum.Bivalve;
                        }

                        MWQMRunModel mwqmRunModelNew = new MWQMRunModel()
                        {
                            SubsectorTVItemID = tvItemModelSubsector.TVItemID,
                            DateTime_Local = DayOfSample,
                            StartDateTime_Local = DayOfSample,
                            EndDateTime_Local = DayOfSample,
                            RunSampleType = sampleType,
                            RunNumber = 1,
                        };

                        MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
                        TideSiteService tideSiteService = new TideSiteService(LanguageEnum.en, user);
                        UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);
                        TideDataValueService tideDataValueService = new TideDataValueService(LanguageEnum.en, user);

                        MWQMRunModel mwqmRunModelExist = mwqmRunService.GetMWQMRunModelExistDB(mwqmRunModelNew);
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
                            mwqmRunModelNew.RunSampleType = sampleType;
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
                            mwqmRunModelNew.Tide_Start = null;
                            mwqmRunModelNew.Tide_End = null;

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

                            if (mwqmRunModelNew.SubsectorTVItemID == null)
                            {
                                int sleifj = 234;
                            }
                            MWQMRunModel mwqmRunModelRet = mwqmRunService.PostAddMWQMRunDB(mwqmRunModelNew);
                            if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return false;
                        }
                        else
                        {
                            bool ShouldUpdate2 = false;

                            if (mwqmRunModelExist.StartDateTime_Local != null || mwqmRunModelExist.EndDateTime_Local != null)
                            {
                                mwqmRunModelExist.StartDateTime_Local = null;
                                mwqmRunModelExist.EndDateTime_Local = null;
                                ShouldUpdate2 = true;
                            }
                            if (mwqmRunModelExist.RunSampleType != sampleType)
                            {
                                mwqmRunModelExist.RunSampleType = sampleType;
                                ShouldUpdate2 = true;
                            }

                            mwqmRunModelExist.RunNumber = 1;

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
                                if (mwqmRunModelExist.StartDateTime_Local != bcSurvey.S_START_DATE || mwqmRunModelExist.EndDateTime_Local != bcSurvey.S_END_DATE)
                                {
                                    mwqmRunModelExist.StartDateTime_Local = bcSurvey.S_START_DATE;
                                    mwqmRunModelExist.EndDateTime_Local = bcSurvey.S_END_DATE;
                                    if (mwqmRunModelExist.StartDateTime_Local > mwqmRunModelExist.EndDateTime_Local)
                                    {
                                        mwqmRunModelExist.EndDateTime_Local = mwqmRunModelExist.StartDateTime_Local;
                                    }

                                    ShouldUpdate2 = true;
                                }
                                Comments = bcSurvey.S_DESCRIPTION + "\r\n" + bcSurvey.S_COMMENT;
                            }

                            string TextEN = "--";
                            if (!string.IsNullOrWhiteSpace(Comments))
                            {
                                TextEN = Comments.Trim();
                            }

                            if (mwqmRunModelExist.RunComment != TextEN || mwqmRunModelExist.RunWeatherComment != TextEN)
                            {
                                mwqmRunModelExist.RunComment = TextEN;
                                mwqmRunModelExist.RunWeatherComment = TextEN;
                                ShouldUpdate2 = true;
                            }

                            mwqmRunModelExist.LabReceivedDateTime_Local = null;
                            mwqmRunModelExist.TemperatureControl1_C = null;
                            mwqmRunModelExist.TemperatureControl2_C = null;
                            mwqmRunModelExist.SeaStateAtStart_BeaufortScale = null;
                            mwqmRunModelExist.SeaStateAtEnd_BeaufortScale = null;
                            mwqmRunModelExist.WaterLevelAtBrook_m = null;
                            mwqmRunModelExist.WaveHightAtStart_m = null;
                            mwqmRunModelExist.WaveHightAtEnd_m = null;
                            mwqmRunModelExist.SampleCrewInitials = null;
                            mwqmRunModelExist.AnalyzeMethod = null;
                            mwqmRunModelExist.SampleMatrix = null;
                            mwqmRunModelExist.Laboratory = null;
                            mwqmRunModelExist.SampleStatus = null;
                            mwqmRunModelExist.LabSampleApprovalContactTVItemID = null;
                            mwqmRunModelExist.LabAnalyzeBath1IncubationStartDateTime_Local = null;
                            mwqmRunModelExist.LabRunSampleApprovalDateTime_Local = null;
                            mwqmRunModelExist.Tide_Start = null;
                            mwqmRunModelExist.Tide_End = null;

                            switch (bcms.SR_ANALYSIS_TYPE)
                            {
                                case "MF":
                                    {
                                        if (mwqmRunModelExist.AnalyzeMethod != AnalyzeMethodEnum.MF)
                                        {
                                            mwqmRunModelExist.AnalyzeMethod = AnalyzeMethodEnum.MF;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case "MPN":
                                    {
                                        if (mwqmRunModelExist.AnalyzeMethod != AnalyzeMethodEnum.MPN)
                                        {
                                            mwqmRunModelExist.AnalyzeMethod = AnalyzeMethodEnum.MPN;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            switch (bcms.SR_SAMPLE_TYPE)
                            {
                                case "W":
                                    {
                                        if (mwqmRunModelExist.SampleMatrix != SampleMatrixEnum.W)
                                        {
                                            mwqmRunModelExist.SampleMatrix = SampleMatrixEnum.W;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case "S":
                                    {
                                        if (mwqmRunModelExist.SampleMatrix != SampleMatrixEnum.S)
                                        {
                                            mwqmRunModelExist.SampleMatrix = SampleMatrixEnum.S;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case "B":
                                    {
                                        if (mwqmRunModelExist.SampleMatrix != SampleMatrixEnum.B)
                                        {
                                            mwqmRunModelExist.SampleMatrix = SampleMatrixEnum.B;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            switch (bcms.SR_SAMPLE_AGENCY)
                            {
                                case 0:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_0)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_0;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case 1:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_1)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_1;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case 2:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_2)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_2;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case 3:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_3)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_3;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case 4:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_4)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_4;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            if (ShouldUpdate2)
                            {
                                if (mwqmRunModelNew.SubsectorTVItemID == null)
                                {
                                    int sleifj = 234;
                                }
                                MWQMRunModel mwqmRunModelRet = mwqmRunService.PostUpdateMWQMRunDB(mwqmRunModelExist);
                                if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return false;
                            }
                        }
                    }
                }

                // doing Marine runs

                List<TempData.BCMarineSampleStation> bcMarineSampleStation = new List<TempData.BCMarineSampleStation>();

                using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                {
                    string TVText = tvItemModelSubsector.TVText.Substring(0, 4);

                    bcMarineSampleStation = (from c in dbDT.BCMarineSampleStations
                                             where c.SS_SHELLFI == TVText
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

                    string TVText = bcmss.SS_STATION;
                    TVItemModel tvItemModelMWQMSite = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelSubsector.TVItemID, TVText, TVTypeEnum.MWQMSite);
                    if (!CheckModelOK<TVItemModel>(tvItemModelMWQMSite)) return false;

                    List<TempData.BCMarineSample> bcMarineSampleList = new List<TempData.BCMarineSample>();

                    using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                    {

                        bcMarineSampleList = (from c in dbDT.BCMarineSamples
                                              where c.SR_STATION_CODE == bcmss.SS_STATION
                                              orderby c.SR_READING_DATE
                                              select c).ToList<TempData.BCMarineSample>();
                    }

                    foreach (TempData.BCMarineSample bcms in bcMarineSampleList)
                    {
                        if (Cancel) return false;

                        Application.DoEvents();


                        DateTime DayOfSample = (DateTime)(bcms.SR_READING_DATE);

                        SampleTypeEnum sampleType = SampleTypeEnum.Routine;

                        if (bcms.SR_SAMPLE_TYPE == "S")
                        {
                            sampleType = SampleTypeEnum.Sediment;
                        }

                        if (bcms.SR_SAMPLE_TYPE == "B")
                        {
                            sampleType = SampleTypeEnum.Bivalve;
                        }

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
                            RunSampleType = sampleType,
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
                            mwqmRunModelNew.RunSampleType = sampleType;
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

                            switch (bcms.SR_TIDE_CODE)
                            {
                                case "L":
                                    {
                                        mwqmRunModelNew.Tide_Start = TideTextEnum.LowTide;
                                        mwqmRunModelNew.Tide_End = TideTextEnum.LowTide;
                                    }
                                    break;
                                case "H":
                                    {
                                        mwqmRunModelNew.Tide_Start = TideTextEnum.HighTide;
                                        mwqmRunModelNew.Tide_End = TideTextEnum.HighTide;
                                    }
                                    break;
                                case "F":
                                    {
                                        mwqmRunModelNew.Tide_Start = TideTextEnum.MidTideFalling;
                                        mwqmRunModelNew.Tide_End = TideTextEnum.MidTideFalling;
                                    }
                                    break;
                                case "E":
                                    {
                                        mwqmRunModelNew.Tide_Start = TideTextEnum.MidTideRising;
                                        mwqmRunModelNew.Tide_End = TideTextEnum.MidTideRising;
                                    }
                                    break;
                                default:
                                    {
                                        mwqmRunModelNew.Tide_Start = TideTextEnum.MidTide;
                                        mwqmRunModelNew.Tide_End = TideTextEnum.MidTide;
                                    }
                                    break;
                            }

                            if (mwqmRunModelNew.SubsectorTVItemID == null)
                            {
                                int sleifj = 234;
                            }

                            MWQMRunModel mwqmRunModelRet = mwqmRunService.PostAddMWQMRunDB(mwqmRunModelNew);
                            if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return false;
                        }
                        else
                        {
                            bool ShouldUpdate = false;

                            if (mwqmRunModelExist.StartDateTime_Local != null || mwqmRunModelExist.EndDateTime_Local != null)
                            {
                                mwqmRunModelExist.StartDateTime_Local = null;
                                mwqmRunModelExist.EndDateTime_Local = null;
                                ShouldUpdate = true;
                            }
                            if (mwqmRunModelExist.RunSampleType != sampleType)
                            {
                                mwqmRunModelExist.RunSampleType = sampleType;
                                ShouldUpdate = true;
                            }

                            mwqmRunModelExist.RunNumber = 1;

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
                                if (mwqmRunModelExist.StartDateTime_Local != bcSurvey.S_START_DATE || mwqmRunModelExist.EndDateTime_Local != bcSurvey.S_END_DATE)
                                {
                                    mwqmRunModelExist.StartDateTime_Local = bcSurvey.S_START_DATE;
                                    mwqmRunModelExist.EndDateTime_Local = bcSurvey.S_END_DATE;
                                    if (mwqmRunModelExist.StartDateTime_Local > mwqmRunModelExist.EndDateTime_Local)
                                    {
                                        mwqmRunModelExist.EndDateTime_Local = mwqmRunModelExist.StartDateTime_Local;
                                    }

                                    ShouldUpdate = true;
                                }
                                Comments = bcSurvey.S_DESCRIPTION + "\r\n" + bcSurvey.S_COMMENT;
                            }

                            string TextEN = "--";
                            if (!string.IsNullOrWhiteSpace(Comments))
                            {
                                TextEN = Comments.Trim();
                            }

                            if (mwqmRunModelExist.RunComment != TextEN || mwqmRunModelExist.RunWeatherComment != TextEN)
                            {
                                mwqmRunModelExist.RunComment = TextEN;
                                mwqmRunModelExist.RunWeatherComment = TextEN;
                                ShouldUpdate = true;
                            }

                            mwqmRunModelExist.LabReceivedDateTime_Local = null;
                            mwqmRunModelExist.TemperatureControl1_C = null;
                            mwqmRunModelExist.TemperatureControl2_C = null;
                            mwqmRunModelExist.SeaStateAtStart_BeaufortScale = null;
                            mwqmRunModelExist.SeaStateAtEnd_BeaufortScale = null;
                            mwqmRunModelExist.WaterLevelAtBrook_m = null;
                            mwqmRunModelExist.WaveHightAtStart_m = null;
                            mwqmRunModelExist.WaveHightAtEnd_m = null;
                            mwqmRunModelExist.SampleCrewInitials = null;
                            mwqmRunModelExist.AnalyzeMethod = null;
                            mwqmRunModelExist.SampleMatrix = null;
                            mwqmRunModelExist.Laboratory = null;
                            mwqmRunModelExist.SampleStatus = null;
                            mwqmRunModelExist.LabSampleApprovalContactTVItemID = null;
                            mwqmRunModelExist.LabAnalyzeBath1IncubationStartDateTime_Local = null;
                            mwqmRunModelExist.LabRunSampleApprovalDateTime_Local = null;
                            mwqmRunModelExist.Tide_Start = null;
                            mwqmRunModelExist.Tide_End = null;

                            switch (bcms.SR_ANALYSIS_TYPE)
                            {
                                case "MF":
                                    {
                                        if (mwqmRunModelExist.AnalyzeMethod != AnalyzeMethodEnum.MF)
                                        {
                                            mwqmRunModelExist.AnalyzeMethod = AnalyzeMethodEnum.MF;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                case "MPN":
                                    {
                                        if (mwqmRunModelExist.AnalyzeMethod != AnalyzeMethodEnum.MPN)
                                        {
                                            mwqmRunModelExist.AnalyzeMethod = AnalyzeMethodEnum.MPN;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            switch (bcms.SR_SAMPLE_TYPE)
                            {
                                case "W":
                                    {
                                        if (mwqmRunModelExist.SampleMatrix != SampleMatrixEnum.W)
                                        {
                                            mwqmRunModelExist.SampleMatrix = SampleMatrixEnum.W;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                case "S":
                                    {
                                        if (mwqmRunModelExist.SampleMatrix != SampleMatrixEnum.S)
                                        {
                                            mwqmRunModelExist.SampleMatrix = SampleMatrixEnum.S;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                case "B":
                                    {
                                        if (mwqmRunModelExist.SampleMatrix != SampleMatrixEnum.B)
                                        {
                                            mwqmRunModelExist.SampleMatrix = SampleMatrixEnum.B;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            switch (bcms.SR_SAMPLE_AGENCY)
                            {
                                case 0:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_0)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_0;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                case 1:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_1)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_1;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                case 2:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_2)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_2;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                case 3:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_3)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_3;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                case 4:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_4)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_4;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            switch (bcms.SR_TIDE_CODE)
                            {
                                case "L":
                                    {
                                        if (mwqmRunModelExist.Tide_Start != TideTextEnum.LowTide || mwqmRunModelExist.Tide_End != TideTextEnum.LowTide)
                                        {
                                            mwqmRunModelExist.Tide_Start = TideTextEnum.LowTide;
                                            mwqmRunModelExist.Tide_End = TideTextEnum.LowTide;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                case "H":
                                    {
                                        if (mwqmRunModelExist.Tide_Start != TideTextEnum.HighTide || mwqmRunModelExist.Tide_End != TideTextEnum.HighTide)
                                        {
                                            mwqmRunModelExist.Tide_Start = TideTextEnum.HighTide;
                                            mwqmRunModelExist.Tide_End = TideTextEnum.HighTide;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                case "F":
                                    {
                                        if (mwqmRunModelExist.Tide_Start != TideTextEnum.MidTideFalling || mwqmRunModelExist.Tide_End != TideTextEnum.MidTideFalling)
                                        {
                                            mwqmRunModelExist.Tide_Start = TideTextEnum.MidTideFalling;
                                            mwqmRunModelExist.Tide_End = TideTextEnum.MidTideFalling;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                case "E":
                                    {
                                        if (mwqmRunModelExist.Tide_Start != TideTextEnum.MidTideRising || mwqmRunModelExist.Tide_End != TideTextEnum.MidTideRising)
                                        {
                                            mwqmRunModelExist.Tide_Start = TideTextEnum.MidTideRising;
                                            mwqmRunModelExist.Tide_End = TideTextEnum.MidTideRising;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                                default:
                                    {
                                        if (mwqmRunModelExist.Tide_Start != TideTextEnum.MidTide || mwqmRunModelExist.Tide_End != TideTextEnum.MidTide)
                                        {
                                            mwqmRunModelExist.Tide_Start = TideTextEnum.MidTide;
                                            mwqmRunModelExist.Tide_End = TideTextEnum.MidTide;
                                            ShouldUpdate = true;
                                        }
                                    }
                                    break;
                            }

                            if (ShouldUpdate)
                            {
                                if (mwqmRunModelNew.SubsectorTVItemID == null)
                                {
                                    int sleifj = 234;
                                }

                                MWQMRunModel mwqmRunModelRet = mwqmRunService.PostUpdateMWQMRunDB(mwqmRunModelExist);
                                if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
