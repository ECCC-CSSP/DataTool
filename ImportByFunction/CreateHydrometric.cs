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
using CSSPWebToolsDBDLL.Services;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CreateHydrometric(List<string> justProvList)
        {
            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelNB = new TVItemModel();
            TVItemModel tvItemModelNL = new TVItemModel();
            TVItemModel tvItemModelNS = new TVItemModel();
            TVItemModel tvItemModelPE = new TVItemModel();
            TVItemModel tvItemModelBC = new TVItemModel();
            TVItemModel tvItemModelQC = new TVItemModel();
            List<OldClimateHydrometric.HydrometricStation> hsList = new List<OldClimateHydrometric.HydrometricStation>();

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

            lblStatus.Text = "Starting ... CreateHydrometric";
            Application.DoEvents();

            List<string> shortProvList = new List<string>() { "NB", "NL", "NS", "PE", "BC", "QC" };
            List<string> provList = new List<string>() { "New Brunswick", "Newfoundland and Labrador", "Nova Scotia", "Prince Edward Island", "British Columbia", "Québec" };
            for (int j = 0; j < provList.Count; j++)
            {
                if (Cancel)
                {
                    return false;
                }

                if (!justProvList.Contains(shortProvList[j]))
                {
                    continue;
                }
                string prov = shortProvList[j];
                using (OldClimateHydrometric.OldClimateHydrometricDBEntities oldClimateHydrometric = new OldClimateHydrometric.OldClimateHydrometricDBEntities())
                {
                    hsList = (from c in oldClimateHydrometric.HydrometricStations
                              where c.Province == prov
                              orderby c.HydrometricStationID
                              select c).ToList<OldClimateHydrometric.HydrometricStation>();
                }

                switch (shortProvList[j])
                {
                    case "NB":
                        {
                            if (!CreateHydrometricSitesAll(tvItemModelNB, hsList, provList[j]))
                            {
                                richTextBoxStatus.AppendText("Could not LoadHydrometricSitesAll with ProvName [" + provList[j] + "] ShortProvName [" + shortProvList[j] + "]\r\n");
                                return false;
                            }
                        }
                        break;
                    case "NL":
                        {
                            if (!CreateHydrometricSitesAll(tvItemModelNL, hsList, provList[j]))
                            {
                                richTextBoxStatus.AppendText("Could not LoadHydrometricSitesAll with ProvName [" + provList[j] + "] ShortProvName [" + shortProvList[j] + "]\r\n");
                                return false;
                            }
                        }
                        break;
                    case "NS":
                        {
                            if (!CreateHydrometricSitesAll(tvItemModelNS, hsList, provList[j]))
                            {
                                richTextBoxStatus.AppendText("Could not LoadHydrometricSitesAll with ProvName [" + provList[j] + "] ShortProvName [" + shortProvList[j] + "]\r\n");
                                return false;
                            }
                        }
                        break;
                    case "PE":
                        {
                            if (!CreateHydrometricSitesAll(tvItemModelPE, hsList, provList[j]))
                            {
                                richTextBoxStatus.AppendText("Could not LoadHydrometricSitesAll with ProvName [" + provList[j] + "] ShortProvName [" + shortProvList[j] + "]\r\n");
                                return false;
                            }
                        }
                        break;
                    case "BC":
                        {
                            if (!CreateHydrometricSitesAll(tvItemModelBC, hsList, provList[j]))
                            {
                                richTextBoxStatus.AppendText("Could not LoadHydrometricSitesAll with ProvName [" + provList[j] + "] ShortProvName [" + shortProvList[j] + "]\r\n");
                                return false;
                            }
                        }
                        break;
                    case "QC":
                        {
                            if (!CreateHydrometricSitesAll(tvItemModelQC, hsList, provList[j]))
                            {
                                richTextBoxStatus.AppendText("Could not LoadHydrometricSitesAll with ProvName [" + provList[j] + "] ShortProvName [" + shortProvList[j] + "]\r\n");
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
