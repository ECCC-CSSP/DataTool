using CSSPEnumsDLL.Enums; 
using CSSPModelsDLL.Models;
using CSSPWebToolsDBDLL.Services;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CreateClimate(List<string> justProvList)
        {
            lblStatus.Text = "Starting ... CreateClimate";
            Application.DoEvents();

            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelNB = new TVItemModel();
            TVItemModel tvItemModelNL = new TVItemModel();
            TVItemModel tvItemModelNS = new TVItemModel();
            TVItemModel tvItemModelPE = new TVItemModel();
            TVItemModel tvItemModelBC = new TVItemModel();
            TVItemModel tvItemModelQC = new TVItemModel();
            List<OldClimateHydrometric.ClimateStation> csList = new List<OldClimateHydrometric.ClimateStation>();

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            tvItemModelNB = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "New Brunswick", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelNB)) return false;

            tvItemModelNL = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Newfoundland and Labrador", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelNL)) return false;

            tvItemModelNS = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Nova Scotia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelNS)) return false;

            tvItemModelPE = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Prince Edward Island", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelPE)) return false;

            tvItemModelBC = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "British Columbia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelBC)) return false;

            tvItemModelQC = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelQC)) return false;

            List<string> shortProvList = new List<string>() { "NB", "NFLD", "NS", "PEI", "BC", "QUE" };
            List<string> provList = new List<string>() { "New Brunswick", "Newfoundland and Labrador", "Nova Scotia", "Prince Edward Island", "British Columbia", "Québec" };
            for (int j = 0; j < provList.Count; j++)
            {
                if (Cancel) return false;

                if (!justProvList.Contains(shortProvList[j]))
                {
                    continue;
                }
                string prov = shortProvList[j];
                using (OldClimateHydrometric.OldClimateHydrometricDBEntities oldClimateHydrometric = new OldClimateHydrometric.OldClimateHydrometricDBEntities())
                {
                    csList = (from c in oldClimateHydrometric.ClimateStations
                              where c.Province == prov
                              orderby c.ClimateStationID
                              select c).ToList<OldClimateHydrometric.ClimateStation>();
                }


                switch (shortProvList[j])
                {
                    case "NB":
                        {
                            if (!CreateClimateSitesAll(tvItemModelNB, csList, provList[j]))
                            {
                                richTextBoxStatus.AppendText("Could not LoadClimateSitesAll with ProvName [" + provList[j] + "] ShortProvName [" + shortProvList[j] + "]\r\n");
                                return false;
                            }
                        }
                        break;
                    case "NFLD":
                        {
                            if (!CreateClimateSitesAll(tvItemModelNL, csList, provList[j]))
                            {
                                richTextBoxStatus.AppendText("Could not LoadClimateSitesAll with ProvName [" + provList[j] + "] ShortProvName [" + shortProvList[j] + "]\r\n");
                                return false;
                            }
                        }
                        break;
                    case "NS":
                        {
                            if (!CreateClimateSitesAll(tvItemModelNS, csList, provList[j]))
                            {
                                richTextBoxStatus.AppendText("Could not LoadClimateSitesAll with ProvName [" + provList[j] + "] ShortProvName [" + shortProvList[j] + "]\r\n");
                                return false;
                            }
                        }
                        break;
                    case "PEI":
                        {
                            if (!CreateClimateSitesAll(tvItemModelPE, csList, provList[j]))
                            {
                                richTextBoxStatus.AppendText("Could not LoadClimateSitesAll with ProvName [" + provList[j] + "] ShortProvName [" + shortProvList[j] + "]\r\n");
                                return false;
                            }
                        }
                        break;
                    case "BC":
                        {
                            if (!CreateClimateSitesAll(tvItemModelBC, csList, provList[j]))
                            {
                                richTextBoxStatus.AppendText("Could not LoadClimateSitesAll with ProvName [" + provList[j] + "] ShortProvName [" + shortProvList[j] + "]\r\n");
                                return false;
                            }
                        }
                        break;
                    case "QUE":
                        {
                            if (!CreateClimateSitesAll(tvItemModelQC, csList, provList[j]))
                            {
                                richTextBoxStatus.AppendText("Could not LoadClimateSitesAll with ProvName [" + provList[j] + "] ShortProvName [" + shortProvList[j] + "]\r\n");
                                return false;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            return true;
        }
    }
}
