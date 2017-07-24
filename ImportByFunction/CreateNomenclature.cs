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
        public bool CreateNomenclatureAll()
        {
            if (Cancel) return false;

            int StartCreateNomenclatureAll = int.Parse(textBoxCreateNomenclatureAll.Text);

            lblStatus.Text = "Starting ... CreateNomenclatureAll";
            Application.DoEvents();

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelProvince = new TVItemModel();
            TVItemModel tvItemModelAreaExist = new TVItemModel();
            TVItemModel tvItemModelSectorExist = new TVItemModel();
            TVItemModel tvItemModelSubsectorExist = new TVItemModel();

            List<TempData.Nomenclature> nomenclatureList = new List<TempData.Nomenclature>();
            using (TempData.TempDataToolDBEntities dbTD = new TempData.TempDataToolDBEntities())
            {
                nomenclatureList = (from c in dbTD.Nomenclatures
                                    orderby c.Locator
                                    select c).ToList<TempData.Nomenclature>();

            }
            string OldProvText = "";
            string OldAreaText = "";
            string OldSectorText = "";
            string OldSubsectorText = "";
            string PROVText = "";
            string AREAText = "";
            string SECTORText = "";
            string SUBSECTORText = "";
            int CountTotal = nomenclatureList.Count();
            int Count = 0;
            foreach (TempData.Nomenclature n in nomenclatureList)
            {
                if (Cancel) return false;

                Count += 1;

                lblStatus.Text = ((Count * 100) / CountTotal) + "% ... CreateNomenclatureAll";
                lblStatus2.Text = Count + " of " + CountTotal;
                textBoxCreateNomenclatureAll.Text = Count.ToString();
                Application.DoEvents();

                if (Count < StartCreateNomenclatureAll)
                {
                    continue;
                }

                string PROV = n.Locator.Substring(0, 2).ToUpper();

                if (PROV == "NB")
                {
                    PROVText = "New Brunswick";
                }
                else if (PROV == "NL")
                {
                    PROVText = "Newfoundland and Labrador";
                }
                else if (PROV == "NS")
                {
                    PROVText = "Nova Scotia";
                }
                else if (PROV == "PE")
                {
                    PROVText = "Prince Edward Island";
                }
                else
                {
                    richTextBoxStatus.AppendText("Error: First 2 letters of LOCATOR are not [NB] or [NL] or [NS] or [PE]\r\n");
                    return false;
                }

                string AREA = n.Locator.Substring(3, 2);
                string SECTOR = n.Locator.Substring(6, 3);
                string SUBSECTOR = n.Locator.Substring(10, 3);

                AREAText = PROV + "-" + AREA;
                SECTORText = PROV + "-" + AREA + "-" + SECTOR;
                SUBSECTORText = PROV + "-" + AREA + "-" + SECTOR + "-" + SUBSECTOR;

                TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                MWQMSubsectorService mwqmSubsectorService = new MWQMSubsectorService(LanguageEnum.en, user);

                if (OldProvText != PROVText)
                {
                    tvItemModelProvince = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, PROVText, TVTypeEnum.Province);
                    if (!CheckModelOK<TVItemModel>(tvItemModelProvince)) return false;

                    OldProvText = PROVText;
                }

                if (OldAreaText != AREAText)
                {
                    tvItemModelAreaExist = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelProvince.TVItemID, AREAText + " (" + n.Area + ")", TVTypeEnum.Area);
                    if (!string.IsNullOrWhiteSpace(tvItemModelAreaExist.Error))
                    {
                        tvItemModelAreaExist = tvItemService.PostCreateTVItem(tvItemModelProvince.TVItemID, AREAText + " (" + n.Area + ")", AREAText + " (" + n.Area + ")", TVTypeEnum.Area);
                        if (!CheckModelOK<TVItemModel>(tvItemModelAreaExist)) return false;
                    }

                    OldAreaText = AREAText;
                }

                if (OldSectorText != SECTORText)
                {
                    tvItemModelSectorExist = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelAreaExist.TVItemID, SECTORText + " (" + n.Sector + ")", TVTypeEnum.Sector);
                    if (!string.IsNullOrWhiteSpace(tvItemModelSectorExist.Error))
                    {
                        tvItemModelSectorExist = tvItemService.PostCreateTVItem(tvItemModelAreaExist.TVItemID, SECTORText + " (" + n.Sector + ")", SECTORText + " (" + n.Sector + ")", TVTypeEnum.Sector);
                        if (!CheckModelOK<TVItemModel>(tvItemModelSectorExist)) return false;
                    }

                    OldSectorText = SECTORText;
                }

                if (OldSubsectorText != SUBSECTORText)
                {
                    tvItemModelSubsectorExist = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelSectorExist.TVItemID, SUBSECTORText + " (" + n.Subsector + ")", TVTypeEnum.Subsector);
                    if (!string.IsNullOrWhiteSpace(tvItemModelSubsectorExist.Error))
                    {
                        tvItemModelSubsectorExist = tvItemService.PostCreateTVItem(tvItemModelSectorExist.TVItemID, SUBSECTORText + " (" + n.Subsector + ")", SUBSECTORText + " (" + n.Subsector + ")", TVTypeEnum.Subsector);
                        if (!CheckModelOK<TVItemModel>(tvItemModelSubsectorExist)) return false;
                    }

                    OldSubsectorText = SUBSECTORText;
                }

                // check if WQMSubsector is already there
                MWQMSubsectorModel mwqmSubsectorModelExist = mwqmSubsectorService.GetMWQMSubsectorModelWithMWQMSubsectorTVItemIDDB(tvItemModelSubsectorExist.TVItemID);
                if (!string.IsNullOrWhiteSpace(mwqmSubsectorModelExist.Error))
                {
                    string TextEN = tvItemService.CleanText(n.SubsectorDesc);
                    string TextFR = tvItemService.CleanText(n.SubsectorDesc);

                    MWQMSubsectorModel mwqmSubsectorModelNew = new MWQMSubsectorModel();
                    mwqmSubsectorModelNew.MWQMSubsectorTVItemID = tvItemModelSubsectorExist.TVItemID;
                    mwqmSubsectorModelNew.SubsectorHistoricKey = n.Locator;
                    //mwqmSubsectorModelNew.MWQMSubsectorTVText = TextEN;
                    mwqmSubsectorModelNew.SubsectorDesc = (string.IsNullOrWhiteSpace(TextEN) == true ? "(empty)" : TextEN);

                    MWQMSubsectorModel mwqmSubsectorModelRet = mwqmSubsectorService.PostAddMWQMSubsectorDB(mwqmSubsectorModelNew);
                    if (!CheckModelOK<MWQMSubsectorModel>(mwqmSubsectorModelRet)) return false;

                    MWQMSubsectorLanguageModel mwqmSubsectorLanguageModelRet = mwqmSubsectorService._MWQMSubsectorLanguageService.GetMWQMSubsectorLanguageModelWithMWQMSubsectorIDAndLanguageDB(mwqmSubsectorModelRet.MWQMSubsectorID, LanguageEnum.fr);
                    if (!CheckModelOK<MWQMSubsectorLanguageModel>(mwqmSubsectorLanguageModelRet)) return false;

                    mwqmSubsectorLanguageModelRet.SubsectorDesc = TextFR;

                    mwqmSubsectorLanguageModelRet = mwqmSubsectorService._MWQMSubsectorLanguageService.PostUpdateMWQMSubsectorLanguageDB(mwqmSubsectorLanguageModelRet);
                    if (!CheckModelOK<MWQMSubsectorLanguageModel>(mwqmSubsectorLanguageModelRet)) return false;

                }
            }

            return true;
        }
    }
}
