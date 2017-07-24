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
        public bool CreateSamplesBC()
        {
            lblStatus.Text = "Starting ... CreateSamplesBC";
            Application.DoEvents();

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelProv = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "British Columbia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return false;

            List<TVItemModel> BCSubSectorLangList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            if (BCSubSectorLangList.Count == 0) return false;

            List<BCStation> BCWQMSiteList = new List<BCStation>();
            TVItemModel BCSubSector = new TVItemModel();
            List<TT> tideTextInDBList = new List<TT>();
            List<AM> analyseMethodInDBList = new List<AM>();
            List<Mat> matrixInDBList = new List<Mat>();
            List<Lab> labInDBList = new List<Lab>();

            int StartBCCreateSamplesBC = int.Parse(textBoxBCCreateSamplesBC.Text);

            int TotalCount = BCSubSectorLangList.Count();
            int Count = 0;
            foreach (TVItemModel tvItemModelSubsector in BCSubSectorLangList)
            {
                if (Cancel) return false;

                Count += 1;
                lblStatus.Text = (Count * 100 / TotalCount).ToString() + " ... CreateSamplesBC of " + tvItemModelSubsector.TVText;
                lblStatus2.Text = Count + " of " + TotalCount;
                Application.DoEvents();

                textBoxBCCreateSamplesBC.Text = Count.ToString();

                if (StartBCCreateSamplesBC > Count)
                {
                    continue;
                }

                // doing land samples

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
                int totalSta = bcLandSampleStation.Count;
                foreach (TempData.BCLandSampleStation bcmss in bcLandSampleStation)
                {
                    if (Cancel) return false;

                    countSta += 1;
                    lblStatus2.Text = "Doing Land Sample " + countSta + " of " + totalSta;
                    Application.DoEvents();

                    string TVText = bcmss.SS_STATION_CODE;

                    BCSubSector = (from c in BCSubSectorLangList where c.TVText.StartsWith(bcmss.SS_SHELLFISH_SECTOR) select c).FirstOrDefault();
                    if (!CheckModelOK<TVItemModel>(BCSubSector)) return false;

                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                    MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);

                    TVItemModel tvItemModelMWQMSite = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(BCSubSector.TVItemID, TVText, TVTypeEnum.MWQMSite);
                    if (!CheckModelOK<TVItemModel>(tvItemModelMWQMSite)) return false;

                    List<TempData.BCLandSample> bcLandSampleList = new List<TempData.BCLandSample>();

                    using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                    {

                        bcLandSampleList = (from c in dbDT.BCLandSamples
                                            where c.SR_STATION_CODE == bcmss.SS_STATION_CODE
                                            orderby c.SR_READING_DATE
                                            select c).ToList<TempData.BCLandSample>();
                    }

                    int countSample = 0;
                    int TotalSample = bcLandSampleList.Count;
                    foreach (TempData.BCLandSample bcms in bcLandSampleList)
                    {
                        if (Cancel) return false;
                        countSample += 1;
                        lblStatus2.Text = "Doing Marine Sample " + countSta + " of " + totalSta + " Sample " + countSample + " of " + TotalSample;
                        Application.DoEvents();

                        DateTime DayOfSample = (DateTime)(bcms.SR_READING_DATE);
                        string SampleTime = bcms.SR_READING_TIME;

                        DateTime SampleDate = new DateTime(DayOfSample.Year, DayOfSample.Month, DayOfSample.Day, (SampleTime.Length == 1 ? 0 : (SampleTime.Length == 3 ? (int.Parse(SampleTime.Substring(0, 1))) : (int.Parse(SampleTime.Substring(0, 1))))), (SampleTime.Length == 1 ? 0 : (SampleTime.Length == 3 ? (int.Parse(SampleTime.Substring(1, 2))) : (SampleTime.Substring(2, 2) == "60" ? 59 : (int.Parse(SampleTime.Substring(2, 2)))))), 0);

                        int FecCol = 0;
                        if (bcms.SR_FECAL_COLIFORM_IND == "<" && bcms.SR_FECAL_COLIFORM == 2)
                        {
                            FecCol = 1;
                        }
                        else
                        {
                            FecCol = (int)bcms.SR_FECAL_COLIFORM;
                        }
                        MWQMSampleModel mwqmSampleModelNew = new MWQMSampleModel()
                        {
                            MWQMSiteTVItemID = tvItemModelMWQMSite.TVItemID,
                            SampleDateTime_Local = SampleDate,
                            Depth_m = bcms.SR_SAMPLE_DEPTH,
                            FecCol_MPN_100ml = FecCol,
                            Salinity_PPT = bcms.SR_SALINITY,
                            MWQMSampleNote = (string.IsNullOrWhiteSpace(bcms.SR_OBS) == true ? "--" : bcms.SR_OBS.Trim()),
                            WaterTemp_C = bcms.SR_TEMPERATURE,
                            SampleTypesText = ((int)SampleTypeEnum.Routine).ToString() + ",",
                            SampleTypeList = new List<SampleTypeEnum>() { SampleTypeEnum.Routine },
                        };

                        MWQMSampleModel mwqmSampleModelRet = mwqmSampleService.GetMWQMSampleModelExistDB(mwqmSampleModelNew);
                        if (!string.IsNullOrWhiteSpace(mwqmSampleModelRet.Error))
                        {
                            mwqmSampleModelRet = mwqmSampleService.PostAddMWQMSampleDB(mwqmSampleModelNew);
                            if (!CheckModelOK<MWQMSampleModel>(mwqmSampleModelRet)) return false;
                        }
                    }
                }

                // doing water samples
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
                totalSta = bcMarineSampleStation.Count;
                foreach (TempData.BCMarineSampleStation bcmss in bcMarineSampleStation)
                {
                    if (Cancel) return false;

                    countSta += 1;
                    lblStatus2.Text = "Doing Marine Sample " + countSta + " of " + totalSta;
                    Application.DoEvents();

                    string TVText = bcmss.SS_STATION_CODE;

                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                    MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);

                    BCSubSector = (from c in BCSubSectorLangList where c.TVText.StartsWith(bcmss.SS_SHELLFISH_SECTOR) select c).FirstOrDefault();
                    if (!CheckModelOK<TVItemModel>(BCSubSector)) return false;

                    TVItemModel tvItemModelMWQMSite = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(BCSubSector.TVItemID, TVText, TVTypeEnum.MWQMSite);
                    if (!CheckModelOK<TVItemModel>(tvItemModelMWQMSite)) return false;

                    List<TempData.BCMarineSample> bcMarineSampleList = new List<TempData.BCMarineSample>();

                    using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                    {

                        bcMarineSampleList = (from c in dbDT.BCMarineSamples
                                              where c.SR_STATION_CODE == bcmss.SS_STATION_CODE
                                              orderby c.SR_READING_DATE
                                              select c).ToList<TempData.BCMarineSample>();
                    }

                    int countSample = 0;
                    int TotalSample = bcMarineSampleList.Count;
                    foreach (TempData.BCMarineSample bcms in bcMarineSampleList)
                    {
                        if (Cancel) return false;
                        countSample += 1;
                        lblStatus2.Text = "Doing Marine Sample " + countSta + " of " + totalSta + " Sample " + countSample + " of " + TotalSample;
                        Application.DoEvents();

                        DateTime DayOfSample = (DateTime)(bcms.SR_READING_DATE);
                        string SampleTime = bcms.SR_READING_TIME;

                        DateTime SampleDate = new DateTime(DayOfSample.Year, DayOfSample.Month, DayOfSample.Day, (SampleTime.Length == 1 ? 0 : (SampleTime.Length == 3 ? (int.Parse(SampleTime.Substring(0, 1))) : (int.Parse(SampleTime.Substring(0, 1))))), (SampleTime.Length == 1 ? 0 : (SampleTime.Length == 3 ? (int.Parse(SampleTime.Substring(1, 2))) : (SampleTime.Substring(2, 2) == "60" ? 59 : (int.Parse(SampleTime.Substring(2, 2)))))), 0);

                        int FecCol = 0;
                        if (bcms.SR_FECAL_COLIFORM_IND == "<" && bcms.SR_FECAL_COLIFORM == 2)
                        {
                            FecCol = 1;
                        }
                        else
                        {
                            FecCol = (int)bcms.SR_FECAL_COLIFORM;
                        }

                        MWQMSampleModel mwqmSampleModelNew = new MWQMSampleModel()
                        {
                            MWQMSiteTVItemID = tvItemModelMWQMSite.TVItemID,
                            SampleDateTime_Local = SampleDate,
                            Depth_m = bcms.SR_SAMPLE_DEPTH,
                            FecCol_MPN_100ml = FecCol,
                            Salinity_PPT = bcms.SR_SALINITY,
                            MWQMSampleNote = (string.IsNullOrWhiteSpace(bcms.SR_OBS) == true ? "--" : bcms.SR_OBS.Trim()),
                            WaterTemp_C = bcms.SR_TEMPERATURE,
                            SampleTypesText = ((int)SampleTypeEnum.Routine).ToString() + ",",
                            SampleTypeList = new List<SampleTypeEnum>() { SampleTypeEnum.Routine },
                        };

                        MWQMSampleModel mwqmSampleModelRet = mwqmSampleService.GetMWQMSampleModelExistDB(mwqmSampleModelNew);
                        if (!string.IsNullOrWhiteSpace(mwqmSampleModelRet.Error))
                        {
                            mwqmSampleModelRet = mwqmSampleService.PostAddMWQMSampleDB(mwqmSampleModelNew);
                            if (!CheckModelOK<MWQMSampleModel>(mwqmSampleModelRet)) return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
