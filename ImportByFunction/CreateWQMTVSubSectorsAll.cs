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
        private bool CreateWQMTVSubSectorsAll()
        {
            if (Cancel) return false;

            int StartCreateWQMTVSubSectorsAll = int.Parse(textBoxCreateWQMTVSubSectorsAll.Text);

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelProvince = new TVItemModel();
            TVItemModel tvItemModelAreaExist = new TVItemModel();
            TVItemModel tvItemModelSectorExist = new TVItemModel();
            TVItemModel tvItemModelSubsectorExist = new TVItemModel();
            MWQMSubsectorModel mwqmSubsectorModel = new MWQMSubsectorModel();
            string provinceText = "";

            string OldProvText = "";
            string OldAreaText = "";
            string AREAText = "";
            string OldSectorText = "";
            string SECTORText = "";
            string OldSubsectorText = "";
            string SUBSECTORText = "";

            List<SubSector> subsectorList = new List<SubSector>();
            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                subsectorList = (from s in dbDT.ASGADSamples
                                 orderby s.PROV, s.AREA, s.SECTOR, s.SUBSECTOR
                                 select new SubSector
                                 {
                                     Province = s.PROV,
                                     Area = s.AREA,
                                     Sector = s.SECTOR,
                                     Subsector = s.SUBSECTOR,
                                 }).Distinct().ToList<SubSector>();
            }

            int Count = 0;
            int TotalCount = subsectorList.Count;
            foreach (SubSector ss in subsectorList)
            {
                if (Cancel) return false;

                Count += 1;

                lblStatus.Text = (Count * 100 / TotalCount).ToString() + "% ... CreateWQMTVSubSectorsAll";
                lblStatus.Text = Count + " of " + TotalCount;
                textBoxCreateWQMTVSubSectorsAll.Text = Count.ToString();
                Application.DoEvents();

                if (Count < StartCreateWQMTVSubSectorsAll)
                {
                    continue;
                }

                string DESCRIPT = "";
                DateTime? UPDATED = null;

                TempData.ASGADSubsect ssExist = new TempData.ASGADSubsect();
                using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
                {

                    ssExist = (from s in dbDT.ASGADSubsects
                               where s.PROV == ss.Province
                               && s.AREA == ss.Area
                               && s.SECTOR == ss.Sector
                               && s.SUBSECTOR == ss.Subsector
                               select s).FirstOrDefault<TempData.ASGADSubsect>();
                }

                if (ssExist != null)
                {
                    if (ssExist.DESCRIPT == null)
                    {
                        DESCRIPT = "";
                    }
                    else
                    {
                        DESCRIPT = ssExist.DESCRIPT;
                    }
                    UPDATED = ssExist.UPDATED;
                }

                AREAText = ss.Province + "-" + ss.Area;
                SECTORText = AREAText + "-" + ss.Sector;
                SUBSECTORText = SECTORText + "-" + ss.Subsector;

                switch (ss.Province)
                {
                    case "PE":
                        {
                            provinceText = "Prince Edward Island";
                        }
                        break;
                    case "NB":
                        {
                            provinceText = "New Brunswick";
                        }
                        break;
                    case "NL":
                        {
                            provinceText = "Newfoundland and Labrador";
                        }
                        break;
                    case "NS":
                        {
                            provinceText = "Nova Scotia";
                        }
                        break;
                    default:
                        break;
                }

                TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                MWQMSubsectorService mwqmSubsectorService = new MWQMSubsectorService(LanguageEnum.en, user);

                if (OldProvText != provinceText)
                {
                    tvItemModelProvince = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, provinceText, TVTypeEnum.Province);
                    if (!CheckModelOK<TVItemModel>(tvItemModelProvince)) return false;

                    OldProvText = provinceText;
                }

                if (OldAreaText != AREAText)
                {
                    tvItemModelAreaExist = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelProvince.TVItemID, AREAText, TVTypeEnum.Area);
                    if (!string.IsNullOrWhiteSpace(tvItemModelAreaExist.Error))
                    {
                        tvItemModelAreaExist = tvItemService.PostCreateTVItem(tvItemModelProvince.TVItemID, AREAText, AREAText, TVTypeEnum.Area);
                        if (!CheckModelOK<TVItemModel>(tvItemModelAreaExist)) return false;
                    }

                    OldAreaText = AREAText;
                }

                if (OldSectorText != SECTORText)
                {
                    tvItemModelSectorExist = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelAreaExist.TVItemID, SECTORText, TVTypeEnum.Sector);
                    if (!string.IsNullOrWhiteSpace(tvItemModelSectorExist.Error))
                    {
                        tvItemModelSectorExist = tvItemService.PostCreateTVItem(tvItemModelAreaExist.TVItemID, SECTORText, SECTORText, TVTypeEnum.Sector);
                        if (!CheckModelOK<TVItemModel>(tvItemModelSectorExist)) return false;
                    }

                    OldSectorText = SECTORText;
                }

                if (OldSubsectorText != SUBSECTORText)
                {
                    tvItemModelSubsectorExist = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelSectorExist.TVItemID, SUBSECTORText, TVTypeEnum.Subsector);
                    if (!string.IsNullOrWhiteSpace(tvItemModelSubsectorExist.Error))
                    {
                        tvItemModelSubsectorExist = tvItemService.PostCreateTVItem(tvItemModelSectorExist.TVItemID, SUBSECTORText, SUBSECTORText, TVTypeEnum.Subsector);
                        if (!CheckModelOK<TVItemModel>(tvItemModelSubsectorExist)) return false;
                    }

                    OldSubsectorText = SUBSECTORText;
                }

                string Locator = ss.Province + "-" + ss.Area + "-" + ss.Sector + "-" + ss.Subsector;
                 MWQMSubsectorModel mwqmSubsectorModelExist = mwqmSubsectorService.GetMWQMSubsectorModelWithMWQMSubsectorTVItemIDDB(tvItemModelSubsectorExist.TVItemID);
                if (!string.IsNullOrWhiteSpace(mwqmSubsectorModelExist.Error))
                {
                    string TextEN = tvItemService.CleanText(DESCRIPT);
                    string TextFR = tvItemService.CleanText(DESCRIPT);

                    MWQMSubsectorModel mwqmSubsectorModelNew = new MWQMSubsectorModel();
                    mwqmSubsectorModelNew.MWQMSubsectorTVItemID = tvItemModelSubsectorExist.TVItemID;
                    mwqmSubsectorModelNew.SubsectorHistoricKey = Locator;
                    mwqmSubsectorModelNew.MWQMSubsectorTVText = TextEN;

                    MWQMSubsectorModel mwqmSubsectorModelRet = mwqmSubsectorService.PostAddMWQMSubsectorDB(mwqmSubsectorModelNew);
                    if (!CheckModelOK<MWQMSubsectorModel>(mwqmSubsectorModelExist)) return false;

                    MWQMSubsectorLanguageModel mwqmSubsectorLanguageModelRet = mwqmSubsectorService._MWQMSubsectorLanguageService.GetMWQMSubsectorLanguageModelWithMWQMSubsectorIDAndLanguageDB(tvItemModelSubsectorExist.TVItemID, LanguageEnum.fr);
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
