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
        public bool CreatePrecipitationsAtl(List<string> justProvList)
        {
            if (Cancel) return false;

            List<string> ProvList = new List<string>() { "NB", "NL", "NS", "PE" };

            lblStatus.Text = "Staring CreatePrecipitationsAll";
            Application.DoEvents();

            int StartNBCreatePrecipitationsAtl = int.Parse(textBoxNBCreatePrecipitationsAtl.Text);
            int StartNLCreatePrecipitationsAtl = int.Parse(textBoxNLCreatePrecipitationsAtl.Text);
            int StartNSCreatePrecipitationsAtl = int.Parse(textBoxNSCreatePrecipitationsAtl.Text);
            int StartPECreatePrecipitationsAtl = int.Parse(textBoxPECreatePrecipitationsAtl.Text);

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                foreach (string Prov in ProvList)
                {
                    if (justProvList.Contains(Prov))
                    {
                        List<string> asgadPrecStnList = (from c in dbDT.ASGADPrecDatas
                                                         where c.p == Prov
                                                         orderby c.PRECIPID
                                                         select c.PRECIPID).Distinct().ToList<string>();

                        int TotalCount = asgadPrecStnList.Count();
                        int Count = 0;
                        foreach (string ps in asgadPrecStnList)
                        {
                            if (Cancel)                                return false;

                            Count += 1;
                            lblStatus.Text = (Count * 100 / TotalCount).ToString() + "% of CreatePrecipitations Atlantic";
                            lblStatus2.Text = Count + " of " + TotalCount;
                            Application.DoEvents();

                            switch (Prov)
                            {
                                case "NB":
                                    {
                                        textBoxNBCreatePrecipitationsAtl.Text = Count.ToString();
                                        if (StartNBCreatePrecipitationsAtl > Count)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                case "NL":
                                    {
                                        textBoxNLCreatePrecipitationsAtl.Text = Count.ToString();
                                        if (StartNLCreatePrecipitationsAtl > Count)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                case "NS":
                                    {
                                        textBoxNSCreatePrecipitationsAtl.Text = Count.ToString();
                                        if (StartNSCreatePrecipitationsAtl > Count)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                case "PE":
                                    {
                                        textBoxPECreatePrecipitationsAtl.Text = Count.ToString();
                                        if (StartPECreatePrecipitationsAtl > Count)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }


                            if (ps == "")
                            {
                                continue;
                            }

                            ClimateSiteService climateSiteService = new ClimateSiteService(LanguageEnum.en, user);

                            ClimateSiteModel climateSiteModelExist = climateSiteService.GetClimateSiteModelWithClimateIDDB(ps);
                            if (!CheckModelOK<ClimateSiteModel>(climateSiteModelExist)) return false;

                            List<TempData.ASGADPrecData> asgadPrecDataList = (from c in dbDT.ASGADPrecDatas
                                                                              where c.PRECIPID == ps
                                                                              select c).ToList<TempData.ASGADPrecData>();

                            CreatePrecFillDB(asgadPrecDataList.ToList(), climateSiteModelExist.ClimateSiteID);
                        }
                    }
                }
            }

            return true;
        }
        public bool CreatePrecFillDB(List<TempData.ASGADPrecData> asgadPrecDataList, int ClimateSiteID)
        {

            foreach (TempData.ASGADPrecData apd in asgadPrecDataList)
            {
                if (Cancel) return false;

                Application.DoEvents();

                DateTime? SAMP_DATE = apd.SAMP_DATE;

                ClimateDataValueModel climateDataValueModelNew = new ClimateDataValueModel()
                {
                    ClimateSiteID = ClimateSiteID,
                    DateTime_Local = (DateTime)SAMP_DATE,
                    Keep = true,
                    StorageDataType = StorageDataTypeEnum.Archived,
                    RainfallEntered_mm = apd.PRECIP,
                };

                ClimateSiteService climateSiteService = new ClimateSiteService(LanguageEnum.en, user);
                ClimateDataValueService climateDataValueService = new ClimateDataValueService(LanguageEnum.en, user);

                ClimateDataValueModel climateDataValueModel = climateDataValueService.GetClimateDataValueModelExitDB(climateDataValueModelNew);
                if (!string.IsNullOrWhiteSpace(climateDataValueModel.Error))
                {
                    ClimateDataValueModel climateDataValueModelRet = climateDataValueService.PostAddClimateDataValueDB(climateDataValueModelNew);
                    if (!CheckModelOK<ClimateDataValueModel>(climateDataValueModelRet)) return false;
                }
                else
                {
                    climateDataValueModel.RainfallEntered_mm = climateDataValueModelNew.RainfallEntered_mm;
                    ClimateDataValueModel climateDataValueModelRet = climateDataValueService.PostUpdateClimateDataValueDB(climateDataValueModel);
                    if (!CheckModelOK<ClimateDataValueModel>(climateDataValueModelRet)) return false;
                }

            }
            return true;
        }
    }
}
