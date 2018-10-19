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
using CSSPDBDLL.Services;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CreateUseOfClimateSitesAtl(List<string> justProvList)
        {
            if (Cancel) return false;

            List<string> ProvList = new List<string>() { "NB", "NL", "NS", "PE" };

            lblStatus.Text = "Starting .. CreateUseOfClimateSitesAll";
            Application.DoEvents();

            int StartNBCreateUseOfClimateSitesAtl = int.Parse(textBoxNBCreateUseOfClimateSitesAtl.Text);
            int StartNFCreateUseOfClimateSitesAtl = int.Parse(textBoxNLCreateUseOfClimateSitesAtl.Text);
            int StartNSCreateUseOfClimateSitesAtl = int.Parse(textBoxNSCreateUseOfClimateSitesAtl.Text);
            int StartPECreateUseOfClimateSitesAtl = int.Parse(textBoxPECreateUseOfClimateSitesAtl.Text);

            MWQMSubsectorService mwqmSubsectorService = new MWQMSubsectorService(LanguageEnum.en, user);

            ClimateSiteModel ClimateSiteModel1 = new ClimateSiteModel();
            ClimateSiteModel ClimateSiteModel2 = new ClimateSiteModel();
            ClimateSiteModel ClimateSiteModel3 = new ClimateSiteModel();

            List<MWQMSubsectorModel> mwqmSubsectorModelList = mwqmSubsectorService.GetAllMWQMSubsectorModelDB();

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                foreach (string Prov in ProvList)
                {
                    if (justProvList.Contains(Prov))
                    {
                        List<TempData.ASGADSubsect> asgadSubsectList = (from c in dbDT.ASGADSubsects
                                                                        where c.PROV == Prov
                                                                        select c).ToList<TempData.ASGADSubsect>();

                        int Count = 0;
                        int TotalCount = asgadSubsectList.Count;
                        foreach (TempData.ASGADSubsect asgadSubsect in asgadSubsectList)
                        {
                            if (Cancel) return false;

                            string Locator = asgadSubsect.PROV + "-" + asgadSubsect.AREA + "-" + asgadSubsect.SECTOR + "-" + asgadSubsect.SUBSECTOR;
                            Locator = Locator.Replace("NF", "NL");

                            Count += 1;
                            lblStatus.Text = ((Count * 100) / TotalCount).ToString() + " " + Prov + " " + Locator;
                            lblStatus2.Text = Count + " of " + TotalCount;
                            Application.DoEvents();

                            switch (Prov)
                            {
                                case "NB":
                                    {
                                        textBoxNBCreateUseOfClimateSitesAtl.Text = Count.ToString();
                                        if (StartNBCreateUseOfClimateSitesAtl > Count)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                case "NL":
                                    {
                                        textBoxNLCreateUseOfClimateSitesAtl.Text = Count.ToString();
                                        if (StartNFCreateUseOfClimateSitesAtl > Count)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                case "NS":
                                    {
                                        textBoxNSCreateUseOfClimateSitesAtl.Text = Count.ToString();
                                        if (StartNSCreateUseOfClimateSitesAtl > Count)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                case "PE":
                                    {
                                        textBoxPECreateUseOfClimateSitesAtl.Text = Count.ToString();
                                        if (StartPECreateUseOfClimateSitesAtl > Count)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }


                            lblStatus.Text = Locator;
                            Application.DoEvents();

                            int MWQMSubsectorTVItemID = (from c in mwqmSubsectorModelList
                                                         where c.SubsectorHistoricKey == Locator
                                                         select c.MWQMSubsectorTVItemID).FirstOrDefault<int>();

                            if (MWQMSubsectorTVItemID == 0)
                            {
                                richTextBoxStatus.AppendText("Locator [" + Locator + "] from wqmsList\r\n");
                                //return false;
                            }

                            if (!WriteUsePrecToDB(asgadSubsect, MWQMSubsectorTVItemID))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
        private bool WriteUsePrecToDB(TempData.ASGADSubsect asgadSubsect, int MWQMSubsectorTVItemID)
        {
            if (Cancel) return false;

            if (asgadSubsect.PRECIPID != "")
            {
                if (!AddUseOfSite(MWQMSubsectorTVItemID, asgadSubsect.PRECIPID)) return false;
            }

            if (asgadSubsect.PRECIPID2 != "")
            {
                if (!AddUseOfSite(MWQMSubsectorTVItemID, asgadSubsect.PRECIPID2)) return false;
            }

            if (asgadSubsect.PRECIPID3 != "")
            {
                if (!AddUseOfSite(MWQMSubsectorTVItemID, asgadSubsect.PRECIPID3)) return false;
            }

            return true;
        }

        private bool AddUseOfSite(int MWQMSubsectorTVItemID, string PrecID)
        {
            ClimateSiteService climateSiteService = new ClimateSiteService(LanguageEnum.en, user);
            UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);

            ClimateSiteModel climateSiteModel = climateSiteService.GetClimateSiteModelWithClimateIDDB(PrecID);
            if (string.IsNullOrWhiteSpace(climateSiteModel.Error))
            {
                UseOfSiteModel useOfSiteModelExist = useOfSiteService.GetUseOfSiteModelWithSiteTVItemIDAndSubsectorTVItemIDDB(climateSiteModel.ClimateSiteTVItemID, MWQMSubsectorTVItemID);
                if (!string.IsNullOrWhiteSpace(useOfSiteModelExist.Error))
                {
                    UseOfSiteModel useOfSiteModelNew = new UseOfSiteModel()
                    {
                        SiteTVItemID = climateSiteModel.ClimateSiteTVItemID,
                        SubsectorTVItemID = MWQMSubsectorTVItemID,
                        Ordinal = 1,
                        SiteType = SiteTypeEnum.Climate,
                        StartYear = DateTime.Now.Year,
                        EndYear = DateTime.Now.Year,
                    };

                    UseOfSiteModel useOfSiteModelRet = useOfSiteService.PostAddUseOfSiteDB(useOfSiteModelNew);
                    if (!CheckModelOK<UseOfSiteModel>(useOfSiteModelRet)) return false;
                }
            }

            return true;
        }
    }
}
