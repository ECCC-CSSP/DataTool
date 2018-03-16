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
        public bool CreateRunsAtl(string Prov, string ShortProv)
        {
            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

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

            lblStatus.Text = "Staring ... CreateRunsAll";
            Application.DoEvents();

            int StartNBCreateRunsAtl = int.Parse(textBoxNBCreateRunsAtl.Text);
            int StartNLCreateRunsAtl = int.Parse(textBoxNLCreateRunsAtl.Text);
            int StartNSCreateRunsAtl = int.Parse(textBoxNSCreateRunsAtl.Text);
            int StartPECreateRunsAtl = int.Parse(textBoxPECreateRunsAtl.Text);

            Dictionary<string, int> TideDict = new Dictionary<string, int>();
            Dictionary<string, int> SubsectorDict = new Dictionary<string, int>();
            //Dictionary<string, string> WQMSiteDict = new Dictionary<string, string>();

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

            #endregion LoadVariables


            using (TempData.TempDataToolDBEntities dbDT2 = new TempData.TempDataToolDBEntities())
            {
                List<TempData.ASGADRun> asgadRunList = (from c in dbDT2.ASGADRuns
                                                        where c.PROV == ShortProv
                                                        orderby c.PROV, c.AREA, c.SECTOR, c.SUBSECTOR, c.RUN_NUMBER
                                                        select c).ToList<TempData.ASGADRun>();

                int TotalCount = asgadRunList.Count();
                int Count = 0;
                foreach (TempData.ASGADRun ar in asgadRunList)
                {
                    if (Cancel) return false;

                    Count += 1;
                    lblStatus.Text = (Count * 100 / TotalCount).ToString() + "... for " + Prov;
                    lblStatus2.Text = Count + " of " + TotalCount;
                    Application.DoEvents();

                    switch (ShortProv)
                    {
                        case "NB":
                            {
                                textBoxNBCreateRunsAtl.Text = Count.ToString();
                                if (StartNBCreateRunsAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "NL":
                            {
                                textBoxNLCreateRunsAtl.Text = Count.ToString();
                                if (StartNLCreateRunsAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "NS":
                            {
                                textBoxNSCreateRunsAtl.Text = Count.ToString();
                                if (StartNSCreateRunsAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "PE":
                            {
                                textBoxPECreateRunsAtl.Text = Count.ToString();
                                if (StartPECreateRunsAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    if (ar.TIDE_CODE == null || ar.TIDE_CODE.Trim() == "")
                    {
                        //int TideStartTVItemPathID = null;
                    }
                    else
                    {
                        ar.TIDE_CODE = GetTideCode(ar.TIDE_CODE);
                    }

                    if (ar.TIDEF_CODE == null || ar.TIDEF_CODE.Trim() == "")
                    {
                        //int? TideEndTVItemPathID = null;
                    }
                    else
                    {
                        ar.TIDEF_CODE = GetTideCode(ar.TIDEF_CODE);
                    }

                    string Locator = ar.PROV + "-" + ar.AREA + "-" + ar.SECTOR + "-" + ar.SUBSECTOR;
                    int SubsectorTVItemID = SubsectorDict[Locator];

                    MWQMRunModel mwqmRunModelNew = new MWQMRunModel();
                    DateTime RunDate = ((DateTime)ar.SAMP_DATE);
                    mwqmRunModelNew.DateTime_Local = ((DateTime)ar.SAMP_DATE);
                    mwqmRunModelNew.SubsectorTVItemID = SubsectorTVItemID;

                    string TextEN = "--";
                    if (!string.IsNullOrWhiteSpace(ar.NOTE))
                    {
                        TextEN = ar.NOTE.Trim();
                    }

                    mwqmRunModelNew.RunComment = TextEN;
                    mwqmRunModelNew.RunWeatherComment = TextEN;

                    if (string.IsNullOrEmpty(ar.START_TIME))
                    {
                        mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                    }
                    else
                    {
                        if (ar.START_TIME.Length == 1 || ar.START_TIME.Length == 2)
                        {
                            int Hour;
                            if (!int.TryParse(ar.START_TIME, out Hour))
                            {
                                mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                            }
                            else
                            {
                                if (Hour == 0 || Hour > 23)
                                {
                                    mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                                }
                                else
                                {
                                    mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                }
                            }
                        }
                        else if (ar.START_TIME.Length == 3)
                        {
                            int Hour;
                            if (!int.TryParse(ar.START_TIME.Substring(0, 1), out Hour))
                            {
                                mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                            }
                            else
                            {
                                if (Hour == 0 || Hour > 23)
                                {
                                    mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                                }
                                else
                                {
                                    int Min;
                                    if (!int.TryParse(ar.START_TIME.Substring(1, 2), out Min))
                                    {
                                        mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                    }
                                    else
                                    {
                                        if (Min > 59)
                                        {
                                            mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                        }
                                        else
                                        {
                                            mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, Min, 0);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            int Hour;
                            if (!int.TryParse(ar.START_TIME.Substring(0, 2), out Hour))
                            {
                                mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                            }
                            else
                            {
                                if (Hour == 0 || Hour > 23)
                                {
                                    mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                                }
                                else
                                {
                                    int Min;
                                    if (!int.TryParse(ar.START_TIME.Substring(2, 2), out Min))
                                    {
                                        mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                    }
                                    else
                                    {
                                        if (Min > 59)
                                        {
                                            mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                        }
                                        else
                                        {
                                            mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, Min, 0);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(ar.FIN_TIME))
                    {
                        mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                    }
                    else
                    {
                        if (ar.FIN_TIME.Length == 1 || ar.FIN_TIME.Length == 2)
                        {
                            int Hour;
                            if (!int.TryParse(ar.FIN_TIME, out Hour))
                            {
                                mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                            }
                            else
                            {
                                if (Hour == 0 || Hour > 23)
                                {
                                    mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                                }
                                else
                                {
                                    mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                }
                            }
                        }
                        else if (ar.FIN_TIME.Length == 3)
                        {
                            int Hour;
                            if (!int.TryParse(ar.FIN_TIME.Substring(0, 1), out Hour))
                            {
                                mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                            }
                            else
                            {
                                if (Hour == 0 || Hour > 23)
                                {
                                    mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                                }
                                else
                                {
                                    int Min;
                                    if (!int.TryParse(ar.FIN_TIME.Substring(1, 2), out Min))
                                    {
                                        mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                    }
                                    else
                                    {
                                        if (Min > 59)
                                        {
                                            mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                        }
                                        else
                                        {
                                            mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, Min, 0);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            int Hour;
                            if (!int.TryParse(ar.FIN_TIME.Substring(0, 2), out Hour))
                            {
                                mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                            }
                            else
                            {
                                if (Hour == 0 || Hour > 23)
                                {
                                    mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                                }
                                else
                                {
                                    int Min;
                                    if (!int.TryParse(ar.FIN_TIME.Substring(2, 2), out Min))
                                    {
                                        mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                    }
                                    else
                                    {
                                        if (Min > 59)
                                        {
                                            mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                        }
                                        else
                                        {
                                            mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, Min, 0);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    switch (ar.TIDE_CODE)
                    {
                        case "LOW TIDE":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.LowTide;
                            }
                            break;
                        case "HIGH TIDE":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.HighTide;
                            }
                            break;
                        case "MID TIDE":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.MidTide;
                            }
                            break;
                        case "LOW FALLING":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.LowTideFalling;
                            }
                            break;
                        case "HIGH FALLING":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.HighTideFalling;
                            }
                            break;
                        case "MID FALLING":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.MidTideFalling;
                            }
                            break;
                        case "LOW RISING":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.LowTideRising;
                            }
                            break;
                        case "HIGH RISING":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.HighTideRising;
                            }
                            break;
                        case "MID RISING":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.MidTideRising;
                            }
                            break;
                        default:
                            break;
                    }

                    switch (ar.TIDEF_CODE)
                    {
                        case "LOW TIDE":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.LowTide;
                            }
                            break;
                        case "HIGH TIDE":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.HighTide;
                            }
                            break;
                        case "MID TIDE":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.MidTide;
                            }
                            break;
                        case "LOW FALLING":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.LowTideFalling;
                            }
                            break;
                        case "HIGH FALLING":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.HighTideFalling;
                            }
                            break;
                        case "MID FALLING":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.MidTideFalling;
                            }
                            break;
                        case "LOW RISING":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.LowTideRising;
                            }
                            break;
                        case "HIGH RISING":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.HighTideRising;
                            }
                            break;
                        case "MID RISING":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.MidTideRising;
                            }
                            break;
                        default:
                            break;
                    }

                    mwqmRunModelNew.RainDay1_mm = ar.PPT24;
                    mwqmRunModelNew.RainDay2_mm = ar.PPT48;
                    mwqmRunModelNew.RainDay3_mm = ar.PPT72;

                    DateTime SAMP_DATE_A = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);

                    if (mwqmRunModelNew.StartDateTime_Local > mwqmRunModelNew.EndDateTime_Local)
                    {
                        mwqmRunModelNew.EndDateTime_Local = mwqmRunModelNew.StartDateTime_Local;
                    }


                    MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
                    UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);
                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                    if (ar.RUN_NUMBER == null)
                    {
                        mwqmRunModelNew.RunNumber = 1;
                        mwqmRunModelNew.RunSampleType = SampleTypeEnum.Routine;
                    }
                    else
                    {
                        int rn = (int)ar.RUN_NUMBER;
                        mwqmRunModelNew.RunNumber = rn;
                        if (rn == 1)
                        {
                            mwqmRunModelNew.RunSampleType = SampleTypeEnum.Routine;
                        }
                        else if (rn == 2)
                        {
                            mwqmRunModelNew.RunSampleType = SampleTypeEnum.RainCMPRoutine;
                        }
                        else
                        {
                            mwqmRunModelNew.RunSampleType = SampleTypeEnum.Study;
                        }
                    }
                    MWQMRunModel mwqmRunModelExist = mwqmRunService.GetMWQMRunModelExistDB(mwqmRunModelNew);
                    if (!string.IsNullOrWhiteSpace(mwqmRunModelExist.Error))
                    {
                        string TVText = mwqmRunModelNew.StartDateTime_Local.Value.Year.ToString() + " " +
                            (mwqmRunModelNew.StartDateTime_Local.Value.Month > 9 ? mwqmRunModelNew.StartDateTime_Local.Value.Month.ToString() : "0" + mwqmRunModelNew.StartDateTime_Local.Value.Month.ToString()) + " " +
                            (mwqmRunModelNew.StartDateTime_Local.Value.Day > 9 ? mwqmRunModelNew.StartDateTime_Local.Value.Day.ToString() : "0" + mwqmRunModelNew.StartDateTime_Local.Value.Day.ToString());

                        TVItemModel tvItemModel = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(SubsectorTVItemID, TVText, TVTypeEnum.MWQMRun);
                        if (!string.IsNullOrWhiteSpace(tvItemModel.Error))
                        {
                            TVItemModel tvItemModelRet = tvItemService.PostAddChildTVItemDB(SubsectorTVItemID, TVText, TVTypeEnum.MWQMRun);
                            if (!string.IsNullOrWhiteSpace(tvItemModelRet.Error))
                            {
                                richTextBoxStatus.AppendText(tvItemModelRet.Error + "\r\n");
                                return false;
                            }
                            mwqmRunModelNew.MWQMRunTVItemID = tvItemModelRet.TVItemID;
                        }
                        else
                        {
                            mwqmRunModelNew.MWQMRunTVItemID = tvItemModel.TVItemID;
                        }

                        if (mwqmRunModelNew.RainDay1_mm > 999)
                        {
                            mwqmRunModelNew.RainDay1_mm = 999;
                        }
                        if (mwqmRunModelNew.RainDay2_mm > 999)
                        {
                            mwqmRunModelNew.RainDay2_mm = 999;
                        }
                        if (mwqmRunModelNew.RainDay3_mm > 999)
                        {
                            mwqmRunModelNew.RainDay3_mm = 999;
                        }

                        MWQMRunModel mwqmRunModelRet = mwqmRunService.PostAddMWQMRunDB(mwqmRunModelNew);
                        if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return false;

                    }
                    else
                    {
                        mwqmRunModelExist.RunNumber = mwqmRunModelNew.RunNumber;
                        mwqmRunModelExist.RunSampleType = mwqmRunModelNew.RunSampleType;

                        mwqmRunModelExist.Tide_Start = mwqmRunModelNew.Tide_Start;
                        mwqmRunModelExist.Tide_End = mwqmRunModelNew.Tide_End;

                        mwqmRunModelExist.RainDay1_mm = mwqmRunModelNew.RainDay1_mm;
                        mwqmRunModelExist.RainDay2_mm = mwqmRunModelNew.RainDay2_mm;
                        mwqmRunModelExist.RainDay3_mm = mwqmRunModelNew.RainDay3_mm;

                        if (mwqmRunModelExist.RainDay1_mm > 999)
                        {
                            mwqmRunModelExist.RainDay1_mm = 999;
                        }
                        if (mwqmRunModelExist.RainDay2_mm > 999)
                        {
                            mwqmRunModelExist.RainDay2_mm = 999;
                        }
                        if (mwqmRunModelExist.RainDay3_mm > 999)
                        {
                            mwqmRunModelExist.RainDay3_mm = 999;
                        }
                        MWQMRunModel mwqmRunModelRet = mwqmRunService.PostUpdateMWQMRunDB(mwqmRunModelExist);
                        if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return false;
                    }
                }
            }

            return true;
        }
        public bool CreateRunsAtlMOU(string Prov, string ShortProv)
        {
            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

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

            lblStatus.Text = "Staring ... CreateRunsAll";
            Application.DoEvents();

            int StartNBCreateRunsAtl = int.Parse(textBoxNBCreateRunsAtl.Text);
            int StartNLCreateRunsAtl = int.Parse(textBoxNLCreateRunsAtl.Text);
            int StartNSCreateRunsAtl = int.Parse(textBoxNSCreateRunsAtl.Text);
            int StartPECreateRunsAtl = int.Parse(textBoxPECreateRunsAtl.Text);

            Dictionary<string, int> TideDict = new Dictionary<string, int>();
            Dictionary<string, int> SubsectorDict = new Dictionary<string, int>();
            //Dictionary<string, string> WQMSiteDict = new Dictionary<string, string>();

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

            #endregion LoadVariables


            using (TempData.TempDataToolDBEntities dbDT2 = new TempData.TempDataToolDBEntities())
            {
                List<TempData.ASGADRunMOU> asgadRunMOUList = (from c in dbDT2.ASGADRunMOUs
                                                        where c.PROV == ShortProv
                                                        orderby c.PROV, c.AREA, c.SECTOR, c.SUBSECTOR, c.RUN_NUMBER
                                                        select c).ToList<TempData.ASGADRunMOU>();

                int TotalCount = asgadRunMOUList.Count();
                int Count = 0;
                foreach (TempData.ASGADRunMOU ar in asgadRunMOUList)
                {
                    if (Cancel) return false;

                    Count += 1;
                    lblStatus.Text = (Count * 100 / TotalCount).ToString() + "... for " + Prov;
                    lblStatus2.Text = Count + " of " + TotalCount;
                    Application.DoEvents();

                    switch (ShortProv)
                    {
                        case "NB":
                            {
                                textBoxNBCreateRunsAtl.Text = Count.ToString();
                                if (StartNBCreateRunsAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "NL":
                            {
                                textBoxNLCreateRunsAtl.Text = Count.ToString();
                                if (StartNLCreateRunsAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "NS":
                            {
                                textBoxNSCreateRunsAtl.Text = Count.ToString();
                                if (StartNSCreateRunsAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        case "PE":
                            {
                                textBoxPECreateRunsAtl.Text = Count.ToString();
                                if (StartPECreateRunsAtl > Count)
                                {
                                    continue;
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    if (ar.TIDE_CODE == null || ar.TIDE_CODE.Trim() == "")
                    {
                        //int TideStartTVItemPathID = null;
                    }
                    else
                    {
                        ar.TIDE_CODE = GetTideCode(ar.TIDE_CODE);
                    }

                    if (ar.TIDEF_CODE == null || ar.TIDEF_CODE.Trim() == "")
                    {
                        //int? TideEndTVItemPathID = null;
                    }
                    else
                    {
                        ar.TIDEF_CODE = GetTideCode(ar.TIDEF_CODE);
                    }

                    string Locator = ar.PROV + "-" + ar.AREA + "-" + ar.SECTOR + "-" + ar.SUBSECTOR;
                    int SubsectorTVItemID = SubsectorDict[Locator];

                    MWQMRunModel mwqmRunModelNew = new MWQMRunModel();
                    DateTime RunDate = ((DateTime)ar.SAMP_DATE);
                    mwqmRunModelNew.DateTime_Local = ((DateTime)ar.SAMP_DATE);
                    mwqmRunModelNew.SubsectorTVItemID = SubsectorTVItemID;

                    string TextEN = "--";
                    if (!string.IsNullOrWhiteSpace(ar.NOTE))
                    {
                        TextEN = ar.NOTE.Trim();
                    }

                    mwqmRunModelNew.RunComment = TextEN;
                    mwqmRunModelNew.RunWeatherComment = TextEN;

                    if (string.IsNullOrEmpty(ar.START_TIME))
                    {
                        mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                    }
                    else
                    {
                        if (ar.START_TIME.Length == 1 || ar.START_TIME.Length == 2)
                        {
                            int Hour;
                            if (!int.TryParse(ar.START_TIME, out Hour))
                            {
                                mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                            }
                            else
                            {
                                if (Hour == 0 || Hour > 23)
                                {
                                    mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                                }
                                else
                                {
                                    mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                }
                            }
                        }
                        else if (ar.START_TIME.Length == 3)
                        {
                            int Hour;
                            if (!int.TryParse(ar.START_TIME.Substring(0, 1), out Hour))
                            {
                                mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                            }
                            else
                            {
                                if (Hour == 0 || Hour > 23)
                                {
                                    mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                                }
                                else
                                {
                                    int Min;
                                    if (!int.TryParse(ar.START_TIME.Substring(1, 2), out Min))
                                    {
                                        mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                    }
                                    else
                                    {
                                        if (Min > 59)
                                        {
                                            mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                        }
                                        else
                                        {
                                            mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, Min, 0);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            int Hour;
                            if (!int.TryParse(ar.START_TIME.Substring(0, 2), out Hour))
                            {
                                mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                            }
                            else
                            {
                                if (Hour == 0 || Hour > 23)
                                {
                                    mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                                }
                                else
                                {
                                    int Min;
                                    if (!int.TryParse(ar.START_TIME.Substring(2, 2), out Min))
                                    {
                                        mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                    }
                                    else
                                    {
                                        if (Min > 59)
                                        {
                                            mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                        }
                                        else
                                        {
                                            mwqmRunModelNew.StartDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, Min, 0);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(ar.FIN_TIME))
                    {
                        mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                    }
                    else
                    {
                        if (ar.FIN_TIME.Length == 1 || ar.FIN_TIME.Length == 2)
                        {
                            int Hour;
                            if (!int.TryParse(ar.FIN_TIME, out Hour))
                            {
                                mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                            }
                            else
                            {
                                if (Hour == 0 || Hour > 23)
                                {
                                    mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                                }
                                else
                                {
                                    mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                }
                            }
                        }
                        else if (ar.FIN_TIME.Length == 3)
                        {
                            int Hour;
                            if (!int.TryParse(ar.FIN_TIME.Substring(0, 1), out Hour))
                            {
                                mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                            }
                            else
                            {
                                if (Hour == 0 || Hour > 23)
                                {
                                    mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                                }
                                else
                                {
                                    int Min;
                                    if (!int.TryParse(ar.FIN_TIME.Substring(1, 2), out Min))
                                    {
                                        mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                    }
                                    else
                                    {
                                        if (Min > 59)
                                        {
                                            mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                        }
                                        else
                                        {
                                            mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, Min, 0);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            int Hour;
                            if (!int.TryParse(ar.FIN_TIME.Substring(0, 2), out Hour))
                            {
                                mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                            }
                            else
                            {
                                if (Hour == 0 || Hour > 23)
                                {
                                    mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);
                                }
                                else
                                {
                                    int Min;
                                    if (!int.TryParse(ar.FIN_TIME.Substring(2, 2), out Min))
                                    {
                                        mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                    }
                                    else
                                    {
                                        if (Min > 59)
                                        {
                                            mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, 0, 0);
                                        }
                                        else
                                        {
                                            mwqmRunModelNew.EndDateTime_Local = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day, Hour, Min, 0);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    switch (ar.TIDE_CODE)
                    {
                        case "LOW TIDE":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.LowTide;
                            }
                            break;
                        case "HIGH TIDE":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.HighTide;
                            }
                            break;
                        case "MID TIDE":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.MidTide;
                            }
                            break;
                        case "LOW FALLING":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.LowTideFalling;
                            }
                            break;
                        case "HIGH FALLING":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.HighTideFalling;
                            }
                            break;
                        case "MID FALLING":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.MidTideFalling;
                            }
                            break;
                        case "LOW RISING":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.LowTideRising;
                            }
                            break;
                        case "HIGH RISING":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.HighTideRising;
                            }
                            break;
                        case "MID RISING":
                            {
                                mwqmRunModelNew.Tide_Start = TideTextEnum.MidTideRising;
                            }
                            break;
                        default:
                            break;
                    }

                    switch (ar.TIDEF_CODE)
                    {
                        case "LOW TIDE":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.LowTide;
                            }
                            break;
                        case "HIGH TIDE":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.HighTide;
                            }
                            break;
                        case "MID TIDE":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.MidTide;
                            }
                            break;
                        case "LOW FALLING":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.LowTideFalling;
                            }
                            break;
                        case "HIGH FALLING":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.HighTideFalling;
                            }
                            break;
                        case "MID FALLING":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.MidTideFalling;
                            }
                            break;
                        case "LOW RISING":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.LowTideRising;
                            }
                            break;
                        case "HIGH RISING":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.HighTideRising;
                            }
                            break;
                        case "MID RISING":
                            {
                                mwqmRunModelNew.Tide_End = TideTextEnum.MidTideRising;
                            }
                            break;
                        default:
                            break;
                    }

                    mwqmRunModelNew.RainDay1_mm = ar.PPT24;
                    mwqmRunModelNew.RainDay2_mm = ar.PPT48;
                    mwqmRunModelNew.RainDay3_mm = ar.PPT72;

                    DateTime SAMP_DATE_A = new DateTime(RunDate.Year, RunDate.Month, RunDate.Day);

                    if (mwqmRunModelNew.StartDateTime_Local > mwqmRunModelNew.EndDateTime_Local)
                    {
                        mwqmRunModelNew.EndDateTime_Local = mwqmRunModelNew.StartDateTime_Local;
                    }

                    mwqmRunModelNew.RunSampleType = SampleTypeEnum.RainCMPRoutine;

                    MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
                    UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);
                    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                    mwqmRunModelNew.RunNumber = 1;
                    MWQMRunModel mwqmRunModelExist = mwqmRunService.GetMWQMRunModelExistDB(mwqmRunModelNew);
                    if (!string.IsNullOrWhiteSpace(mwqmRunModelExist.Error))
                    {
                        string TVText = mwqmRunModelNew.StartDateTime_Local.Value.Year.ToString() + " " +
                            (mwqmRunModelNew.StartDateTime_Local.Value.Month > 9 ? mwqmRunModelNew.StartDateTime_Local.Value.Month.ToString() : "0" + mwqmRunModelNew.StartDateTime_Local.Value.Month.ToString()) + " " +
                            (mwqmRunModelNew.StartDateTime_Local.Value.Day > 9 ? mwqmRunModelNew.StartDateTime_Local.Value.Day.ToString() : "0" + mwqmRunModelNew.StartDateTime_Local.Value.Day.ToString());

                        TVItemModel tvItemModel = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(SubsectorTVItemID, TVText, TVTypeEnum.MWQMRun);
                        if (!string.IsNullOrWhiteSpace(tvItemModel.Error))
                        {
                            TVItemModel tvItemModelRet = tvItemService.PostAddChildTVItemDB(SubsectorTVItemID, TVText, TVTypeEnum.MWQMRun);
                            if (!string.IsNullOrWhiteSpace(tvItemModelRet.Error))
                            {
                                richTextBoxStatus.AppendText(tvItemModelRet.Error + "\r\n");
                                return false;
                            }
                            mwqmRunModelNew.MWQMRunTVItemID = tvItemModelRet.TVItemID;
                        }
                        else
                        {
                            mwqmRunModelNew.MWQMRunTVItemID = tvItemModel.TVItemID;
                        }

                        if (mwqmRunModelNew.RainDay1_mm > 999)
                        {
                            mwqmRunModelNew.RainDay1_mm = 999;
                        }
                        if (mwqmRunModelNew.RainDay2_mm > 999)
                        {
                            mwqmRunModelNew.RainDay2_mm = 999;
                        }
                        if (mwqmRunModelNew.RainDay3_mm > 999)
                        {
                            mwqmRunModelNew.RainDay3_mm = 999;
                        }
                        MWQMRunModel mwqmRunModelRet = mwqmRunService.PostAddMWQMRunDB(mwqmRunModelNew);
                        if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return false;

                    }
                    else
                    {
                        mwqmRunModelExist.Tide_Start = mwqmRunModelNew.Tide_Start;
                        mwqmRunModelExist.Tide_End = mwqmRunModelNew.Tide_End;

                        mwqmRunModelExist.RainDay1_mm = mwqmRunModelNew.RainDay1_mm;
                        mwqmRunModelExist.RainDay2_mm = mwqmRunModelNew.RainDay2_mm;
                        mwqmRunModelExist.RainDay3_mm = mwqmRunModelNew.RainDay3_mm;

                        if (mwqmRunModelExist.RainDay1_mm > 999)
                        {
                            mwqmRunModelExist.RainDay1_mm = 999;
                        }
                        if (mwqmRunModelExist.RainDay2_mm > 999)
                        {
                            mwqmRunModelExist.RainDay2_mm = 999;
                        }
                        if (mwqmRunModelExist.RainDay3_mm > 999)
                        {
                            mwqmRunModelExist.RainDay3_mm = 999;
                        }
                        MWQMRunModel mwqmRunModelRet = mwqmRunService.PostUpdateMWQMRunDB(mwqmRunModelExist);
                        if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return false;
                    }
                }
            }

            return true;
        }
    }
}
