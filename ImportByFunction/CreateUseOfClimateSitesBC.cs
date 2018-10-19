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
        public bool CreateUseOfClimateSitesBC()
        {
            if (Cancel) return false;

            int StartBCCreateUseOfClimateSitesBC = int.Parse(textBoxBCCreateUseOfClimateSitesBC.Text);

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelBC = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "British Columbia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelBC)) return false;

            lblStatus.Text = "Starting ...CreateUseOfClimateSitesBC";
            Application.DoEvents();

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

                    ClimateSiteModel climateSiteModel = climateSiteService.GetClimateSiteModelWithClimateIDDB(clim);
                    if (!CheckModelOK<ClimateSiteModel>(climateSiteModel))
                    {
                        richTextBoxStatus.AppendText("Could not find ClimateSite with ClimateID [" + clim + "]\r\n");
                        return false;
                    }

                    bcClimateSiteList.Add(new BCClimateSite() { ClimateID = climateSiteModel.ClimateID, ClimateSiteID = climateSiteModel.ClimateSiteID, ClimateSiteTVItemID = climateSiteModel.ClimateSiteTVItemID });
                }
            }

            List<BCUseOfClimateSite> bcUseOfClimateSiteList = new List<BCUseOfClimateSite>();

            Application.DoEvents();

            List<TVItemModel> tvItemModelSubsectorList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelBC.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count() == 0)
            {
                richTextBoxStatus.AppendText("Could not find TVItem for subsector under BC\r\n");
                return false;
            }

            int TotalCount = tvItemModelSubsectorList.Count();
            int Count = 0;
            foreach (TVItemModel tvItemModelSubsector in tvItemModelSubsectorList)
            {
                if (Cancel) return false;

                Count += 1;
                lblStatus.Text = (Count * 100 / TotalCount).ToString() + " ... Subsector " + tvItemModelSubsector.TVText;
                lblStatus2.Text = Count + " of " + TotalCount;
                Application.DoEvents();

                textBoxBCCreateUseOfClimateSitesBC.Text = Count.ToString();
                if (StartBCCreateUseOfClimateSitesBC > Count)
                {
                    continue;
                }

                List<TempData.BCPrecipitation> bcPrecList = new List<TempData.BCPrecipitation>();

                using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                {
                    string TVText = tvItemModelSubsector.TVText.Substring(0, 4);

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
                int OldClimateSiteTVItemID = 0;
                foreach (TempData.BCPrecipitation bcp in bcPrecList)
                {
                    if (Cancel) return false;

                    int ClimateSiteID = 0;
                    int ClimateSiteTVItemID = 0;
                    if (OldClimateID != bcp.ClimateID)
                    {
                        ClimateSiteID = (from c in bcClimateSiteList where c.ClimateID == bcp.ClimateID select c.ClimateSiteID).FirstOrDefault<int>();
                        if (ClimateSiteID == 0)
                        {
                            richTextBoxStatus.AppendText("Could not find ClimateSiteID for ClimateID [" + bcp.ClimateID + "] in CreateBCUseOfClimateSitesAll \r\n");
                            return false;
                        }
                        ClimateSiteTVItemID = (from c in bcClimateSiteList where c.ClimateID == bcp.ClimateID select c.ClimateSiteTVItemID).FirstOrDefault<int>();
                        if (ClimateSiteTVItemID == 0)
                        {
                            richTextBoxStatus.AppendText("Could not find ClimateSiteTVItemID for ClimateID [" + bcp.ClimateID + "] in CreateBCUseOfClimateSitesAll \r\n");
                            return false;
                        }
                        OldClimateID = bcp.ClimateID;
                        OldClimateSiteID = ClimateSiteID;
                        OldClimateSiteTVItemID = ClimateSiteTVItemID;
                    }
                    else
                    {
                        ClimateSiteID = OldClimateSiteID;
                        ClimateSiteTVItemID = OldClimateSiteTVItemID;
                    }

                    UseOfSiteModel useOfSiteModelNew = new UseOfSiteModel()
                    {
                        SiteTVItemID = ClimateSiteTVItemID,
                        SubsectorTVItemID = tvItemModelSubsector.TVItemID,
                        Ordinal = 1,
                        UseWeight = false,
                        Weight_perc = 0,
                        UseEquation = false,
                        SiteType = SiteTypeEnum.Climate,
                        StartYear = (int)1980,
                        EndYear = (int)2014,

                    };

                    UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);

                    UseOfSiteModel useOfSiteModelExist = useOfSiteService.GetUseOfSiteModelExist(useOfSiteModelNew);
                    if (!string.IsNullOrWhiteSpace(useOfSiteModelExist.Error))
                    {
                        UseOfSiteModel useOfSiteModelRet = useOfSiteService.PostAddUseOfSiteDB(useOfSiteModelNew);
                        if (!CheckModelOK<UseOfSiteModel>(useOfSiteModelRet)) return false;
                    }
                }

            }

            return true;
        }
    }
}
