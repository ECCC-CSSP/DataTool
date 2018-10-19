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
using CSSPDBDLL;
using CSSPModelsDLL.Models;
using CSSPDBDLL.Services;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CreatePrecipitationsBC()
        {
            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelBC = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "British Columbia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelBC)) return false;

            lblStatus.Text = "Starting ... CreatePrecipitationBC";
            Application.DoEvents();

            int StartBCCreatePrecipitationsBC = int.Parse(textBoxBCCreatePrecipitationsBC.Text);          

            // This will import both the UseOfClimateSite and the precipitation
            // 16 precipitation is not associated with a run
            // these will be omitted

            List<BCClimateSite> bcClimateSiteList = new List<BCClimateSite>();

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                List<string> UniqueBCClimateList = (from c in dbDT.BCPrecipitations
                                                    select c.ClimateID).Distinct().ToList<string>();

                foreach (string clim in UniqueBCClimateList)
                {
                    ClimateSiteService climateSiteService = new ClimateSiteService(LanguageEnum.en, user);

                    ClimateSiteModel climateSiteModelBC = climateSiteService.GetClimateSiteModelWithClimateIDDB(clim);
                    if (!CheckModelOK<ClimateSiteModel>(climateSiteModelBC)) return false;
                   
                    bcClimateSiteList.Add(new BCClimateSite() { ClimateID = climateSiteModelBC.ClimateID, ClimateSiteID = climateSiteModelBC.ClimateSiteID, ClimateSiteTVItemID = climateSiteModelBC.ClimateSiteTVItemID });
                }
            }

            List<BCUseOfClimateSite> bcUseOfClimateSiteList = new List<BCUseOfClimateSite>();


            List<TVItemModel> tvItemModelSubsectorList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelBC.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Could not find any TVItem Subsector under BC \r\n");
                return false;
            }

            int TotalCount = tvItemModelSubsectorList.Count();
            int Count = 0;
            foreach (TVItemModel tvItemModel in tvItemModelSubsectorList)
            {
                if (Cancel) return false;

                Count += 1;
                lblStatus.Text = (Count * 100 / TotalCount).ToString() + " ... CreatePrecipitationBC for " + tvItemModel.TVText;
                lblStatus2.Text = Count + " of " + TotalCount;
                Application.DoEvents();

                textBoxBCCreatePrecipitationsBC.Text = Count.ToString();
                if (StartBCCreatePrecipitationsBC > Count)
                {
                    continue;
                }

                List<TempData.BCPrecipitation> bcPrecList = new List<TempData.BCPrecipitation>();

                using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                {
                    string TVText = tvItemModel.TVText.Substring(0, 4);

                    bcPrecList = (from p in dbDT.BCPrecipitations
                                  from r in dbDT.BCSurveys
                                  where p.WR_SURVEY == r.S_ID_NUMBER
                                  && r.S_SHELLFISH_SECTOR == TVText
                                  && p.ClimateID != null
                                  orderby p.ClimateID
                                  select p).ToList<TempData.BCPrecipitation>();
                }

                BCUseOfClimateSite bcUseOfClimateSite = new BCUseOfClimateSite();
                string OldClimateID = "";
                int OldClimateSiteID = 0;
                int CountBCP = 0;
                int TotalCountBCP = bcPrecList.Count;
                foreach (TempData.BCPrecipitation bcp in bcPrecList)
                {
                    if (Cancel)
                    {
                        return false;
                    }

                    CountBCP += 1;
                    lblStatus2.Text = "Doing " + CountBCP + " of " + TotalCountBCP;
                    Application.DoEvents();

                    int ClimateSiteID = 0;
                    if (OldClimateID != bcp.ClimateID)
                    {
                        ClimateSiteID = (from c in bcClimateSiteList where c.ClimateID == bcp.ClimateID select c.ClimateSiteID).FirstOrDefault<int>();
                        if (ClimateSiteID == 0)
                        {
                            richTextBoxStatus.AppendText("Could not find ClimateSiteID for ClimateID [" + bcp.ClimateID + "] in CreateBCUseOfClimateSitesAll \r\n");
                            return false;
                        }
                        OldClimateID = bcp.ClimateID;
                        OldClimateSiteID = ClimateSiteID;
                    }
                    else
                    {
                        ClimateSiteID = OldClimateSiteID;
                    }

                    DateTime SampleDate = bcp.WR_DATE.Value;
                    DateTime SampleDatePlusOneDay = SampleDate.AddDays(1);

                    ClimateDataValueModel climateDataValueModelNew = new ClimateDataValueModel()
                    {
                        ClimateSiteID = ClimateSiteID,
                        DateTime_Local = (DateTime)SampleDate,
                        Keep = true,
                        StorageDataType = StorageDataTypeEnum.Archived,
                        RainfallEntered_mm = bcp.WR_PRECIPITATION_RAIN,
                    };

                    ClimateDataValueService climateDataValueService = new ClimateDataValueService(LanguageEnum.en, user);

                    ClimateDataValueModel climateDataValueModel = climateDataValueService.GetClimateDataValueModelExitDB(climateDataValueModelNew);
                    if (string.IsNullOrWhiteSpace(climateDataValueModel.Error))
                    {
                        ClimateDataValueModel climateDataValueModelRet = climateDataValueService.PostAddClimateDataValueDB(climateDataValueModelNew);
                        if (!CheckModelOK<ClimateDataValueModel>(climateDataValueModelRet)) return false;
                    }
                }
            }

            return true;
        }
    }
}
