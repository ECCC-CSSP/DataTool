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
        public bool CreateSamplesQC()
        {
            if (Cancel) return false;

            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelProv = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return false;

            List<TVItemModel> tvItemModelSubsectorList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelProv.TVText + "\r\n");
                return false;
            }

            lblStatus.Text = "Starting ... CreateRunsQC";
            Application.DoEvents();

            int StartQCCreateSamplesQC = int.Parse(textBoxQCCreateSamplesQC.Text);

            List<Obs> obsTypeList = new List<Obs>();
            List<string> sectorList = new List<string>();
            List<AM> analyseMethodInDBList = new List<AM>();
            List<Mat> matrixInDBList = new List<Mat>();
            List<Lab> labInDBList = new List<Lab>();
            List<SampleStatus> sampleStatusInDBList = new List<SampleStatus>();
            List<TempData.QCSubsectorAssociation> qcSubAssList = new List<TempData.QCSubsectorAssociation>();

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                qcSubAssList = (from c in dbDT.QCSubsectorAssociations
                                select c).ToList<TempData.QCSubsectorAssociation>();
            }

            List<PCCSM.geo_stations_p> staQCListAll = new List<PCCSM.geo_stations_p>();
            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {

                staQCListAll = (from c in dbQC.geo_stations_p
                                where (c.x != null && c.y != null)
                                && c.secteur != null
                                select c).ToList<PCCSM.geo_stations_p>();

                sectorList = (from s in dbQC.geo_stations_p
                              where s.secteur != null
                              orderby s.secteur
                              select s.secteur).Distinct().ToList();
            }

            List<TVItemModel> tvItemMWQMSiteAll = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.MWQMSite);
            List<TVItemModel> tvItemMWQMRunAll = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.MWQMRun);

            List<PCCSM.db_mesure> dbMesureListAll = new List<PCCSM.db_mesure>();
            List<PCCSM.db_tournee> dbtAll = new List<PCCSM.db_tournee>();

            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {
                dbMesureListAll = (from m in dbQC.db_mesure
                                   select m).ToList<PCCSM.db_mesure>();

                dbtAll = (from t in dbQC.db_tournee
                          select t).ToList();
            }

            int Count = 0;
            int TotalCount = sectorList.Count();
            foreach (string sec in sectorList)
            {
                if (Cancel) return false;

                Count += 1;
                lblStatus.Text = (Count * 100 / TotalCount).ToString() + " ... CreateRunsQC for " + sec;
                lblStatus2.Text = Count + " of " + TotalCount;
                Application.DoEvents();

                textBoxQCCreateSamplesQC.Text = Count.ToString();

                if (StartQCCreateSamplesQC > Count)
                {
                    continue;
                }

                TempData.QCSubsectorAssociation qcSubsectAss = (from c in qcSubAssList
                                                                where c.QCSectorText == sec
                                                                select c).FirstOrDefault<TempData.QCSubsectorAssociation>();

                if (qcSubsectAss == null)
                {
                    continue;
                }

                string TVText = qcSubsectAss.SubsectorText;
                TVItemModel tvItemModelSubsector = (from c in tvItemModelSubsectorList
                                                    where c.TVText.StartsWith(TVText)
                                                    select c).FirstOrDefault();

                if (tvItemModelSubsector == null)
                {
                    richTextBoxStatus.AppendText($"could not find tvItemmodelSubsector [{TVText}]\r\n");
                    return false;
                }

                List<MWQMRunModel> mwqmRunModelAll = mwqmRunService.GetMWQMRunModelListWithSubsectorTVItemIDDB(tvItemModelSubsector.TVItemID);

                List<PCCSM.geo_stations_p> staQCList = (from c in staQCListAll
                                                        where c.secteur == sec
                                                        select c).ToList<PCCSM.geo_stations_p>();

                int countSta = 0;
                int totalCountsta = staQCList.Count;
                foreach (PCCSM.geo_stations_p geoStat in staQCList)
                {
                    if (Cancel) return false;

                    countSta += 1;
                    lblStatus2.Text = countSta + " of " + totalCountsta + " ... CreateRunsQC for " + sec;
                    Application.DoEvents();

                    using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
                    {
                        Application.DoEvents();

                        bool IsActive = true;
                        if (geoStat.status != null)
                        {
                            IsActive = (geoStat.status.Substring(0, 1) == "i" ? false : true);
                        }
                        string PreText = "";
                        if (geoStat.secteur.Length < qcSubsectAss.SubsectorText.Length)
                        {
                            PreText = "";
                        }
                        else
                        {
                            if (geoStat.secteur.StartsWith(qcSubsectAss.SubsectorText))
                            {
                                PreText = geoStat.secteur.Substring(qcSubsectAss.SubsectorText.Length) + "_";
                            }
                            else
                            {
                                PreText = geoStat.secteur + "_";
                            }
                        }

                        if (PreText.StartsWith(".") || PreText.StartsWith("_"))
                        {
                            PreText = PreText.Substring(1);
                        }

                        string MWQMSiteTVText = PreText + "0000".Substring(0, 4 - geoStat.station.ToString().Length) + geoStat.station.ToString();

                        TVItemModel tvItemMWQMSiteExist = (from c in tvItemMWQMSiteAll
                                                           where c.ParentID == tvItemModelSubsector.TVItemID
                                                           && c.TVText == MWQMSiteTVText
                                                           && c.TVType == TVTypeEnum.MWQMSite
                                                           select c).FirstOrDefault();

                        if (tvItemMWQMSiteExist == null)
                        {
                            tvItemMWQMSiteExist = tvItemService.PostAddChildTVItemDB(tvItemModelSubsector.TVItemID, MWQMSiteTVText, TVTypeEnum.MWQMSite);
                            if (!CheckModelOK<TVItemModel>(tvItemMWQMSiteExist)) return false;
                        }

                        List<PCCSM.db_mesure> dbMesureList = (from m in dbMesureListAll
                                                              where m.id_geo_station_p == geoStat.id_geo_station_p
                                                              select m).ToList<PCCSM.db_mesure>();

                        List<MWQMSample> mwqmSampleCSSPList = new List<MWQMSample>();
                        using (CSSPDBEntities db2 = new CSSPDBEntities())
                        {
                            mwqmSampleCSSPList = (from c in db2.MWQMSamples
                                                  where c.MWQMSiteTVItemID == tvItemMWQMSiteExist.TVItemID
                                                  select c).ToList();
                        }

                        List<MWQMSample> mwqmSampleListToAdd = new List<MWQMSample>();
                        List<MWQMSample> mwqmSampleListToUpdate = new List<MWQMSample>();
                        foreach (PCCSM.db_mesure dbm in dbMesureList)
                        {

                            Application.DoEvents();

                            // getting Runs
                            PCCSM.db_tournee dbt = (from t in dbtAll
                                                    where t.ID_Tournee == dbm.id_tournee
                                                    select t).FirstOrDefault();

                            DateTime? SampDateTime = null;
                            DateTime? SampStartDateTime = null;
                            DateTime? SampEndDateTime = null;
                            if (dbm.hre_echantillonnage != null)
                            {
                                SampDateTime = (DateTime)dbm.hre_echantillonnage.Value.AddHours(1);
                                if (dbt.hre_fin != null)
                                {
                                    SampEndDateTime = ((DateTime)dbt.date_echantillonnage).AddHours(dbt.hre_fin.Value.Hour).AddMinutes(dbt.hre_fin.Value.Minute).AddHours(1);
                                }
                                if (dbt.hre_deb != null)
                                {
                                    SampStartDateTime = ((DateTime)dbt.date_echantillonnage).AddHours(dbt.hre_deb.Value.Hour).AddMinutes(dbt.hre_deb.Value.Minute).AddHours(1);
                                }
                            }
                            else
                            {
                                SampDateTime = (DateTime)dbt.date_echantillonnage;
                                if (dbt.hre_fin != null)
                                {
                                    SampEndDateTime = ((DateTime)dbt.date_echantillonnage).AddHours(dbt.hre_fin.Value.Hour).AddMinutes(dbt.hre_fin.Value.Minute).AddHours(1);
                                    SampDateTime = SampEndDateTime;
                                }
                                if (dbt.hre_deb != null)
                                {
                                    SampStartDateTime = ((DateTime)dbt.date_echantillonnage).AddHours(dbt.hre_deb.Value.Hour).AddMinutes(dbt.hre_deb.Value.Minute).AddHours(1);
                                    SampDateTime = SampStartDateTime;
                                }
                            }

                            // making sure MWQMRunExist
                            DateTime DateRun = new DateTime(((DateTime)SampDateTime).Year, ((DateTime)SampDateTime).Month, ((DateTime)SampDateTime).Day);

                            MWQMRunModel mwqmRunModelNew = new MWQMRunModel()
                            {
                                SubsectorTVItemID = tvItemModelSubsector.TVItemID,
                                DateTime_Local = DateRun,
                                RunSampleType = SampleTypeEnum.Routine,
                                RunNumber = 1,
                            };

                            MWQMRunModel mwqmRunModelExist = (from c in mwqmRunModelAll
                                                              where c.SubsectorTVItemID == tvItemModelSubsector.TVItemID
                                                              && c.DateTime_Local == DateRun
                                                              && c.RunSampleType == SampleTypeEnum.Routine
                                                              && c.RunNumber == 1
                                                              select c).FirstOrDefault();

                            if (mwqmRunModelExist == null)
                            {
                                string TVTextRun = DateRun.Year.ToString()
                                    + " " + (DateRun.Month > 9 ? DateRun.Month.ToString() : "0" + DateRun.Month.ToString())
                                    + " " + (DateRun.Day > 9 ? DateRun.Day.ToString() : "0" + DateRun.Day.ToString());

                                TVItemModel tvItemModelRunRet = (from c in tvItemMWQMRunAll
                                                                 where c.ParentID == tvItemModelSubsector.TVItemID
                                                                 && c.TVText == TVTextRun
                                                                 select c).FirstOrDefault();

                                if (tvItemModelRunRet == null)
                                {
                                    richTextBoxStatus.AppendText($"{tvItemModelSubsector.TVText} --- { TVTextRun } adding TVText\r\n");
                                    tvItemModelRunRet = tvItemService.PostAddChildTVItemDB(tvItemModelSubsector.TVItemID, TVTextRun, TVTypeEnum.MWQMRun);
                                    if (!CheckModelOK<TVItemModel>(tvItemModelRunRet)) return false;

                                    tvItemMWQMRunAll.Add(tvItemModelRunRet);
                                }

                                // add the run in the DB
                                mwqmRunModelNew.MWQMRunTVItemID = tvItemModelRunRet.TVItemID;
                                mwqmRunModelNew.AnalyzeMethod = null;
                                mwqmRunModelNew.SampleCrewInitials = null;
                                mwqmRunModelNew.DateTime_Local = DateRun;
                                if (SampEndDateTime == null)
                                {
                                    mwqmRunModelNew.EndDateTime_Local = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.EndDateTime_Local = (DateTime)SampEndDateTime;
                                }
                                if (SampStartDateTime == null)
                                {
                                    mwqmRunModelNew.StartDateTime_Local = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.StartDateTime_Local = (DateTime)SampStartDateTime;
                                }
                                if (dbt.analyse_datetime == null)
                                {
                                    mwqmRunModelNew.LabAnalyzeBath1IncubationStartDateTime_Local = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.LabAnalyzeBath1IncubationStartDateTime_Local = dbt.analyse_datetime;
                                }
                                if (dbt.hre_recep_lab == null)
                                {
                                    mwqmRunModelNew.LabReceivedDateTime_Local = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.LabReceivedDateTime_Local = dbt.hre_recep_lab;
                                }
                                mwqmRunModelNew.Laboratory = null;
                                mwqmRunModelNew.SampleMatrix = null;
                                //if (dbt.mer_etat_fin == null)
                                //{
                                //    mwqmRunModelNew.SeaStateAtEnd_BeaufortScale = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.SeaStateAtEnd_BeaufortScale = (BeaufortScaleEnum)dbt.mer_etat_fin;
                                //}
                                //if (dbt.mer_etat == null)
                                //{
                                //    mwqmRunModelNew.SeaStateAtStart_BeaufortScale = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.SeaStateAtStart_BeaufortScale = (BeaufortScaleEnum)dbt.mer_etat;
                                //}
                                mwqmRunModelNew.SampleStatus = null;
                                if (dbt.temp_glace_recep == null)
                                {
                                    mwqmRunModelNew.TemperatureControl1_C = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.TemperatureControl1_C = (float)dbt.temp_glace_recep;
                                }
                                mwqmRunModelNew.SubsectorTVItemID = tvItemModelSubsector.TVItemID;
                                if (dbt.validation_datetime == null)
                                {
                                    mwqmRunModelNew.LabRunSampleApprovalDateTime_Local = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.LabRunSampleApprovalDateTime_Local = dbt.validation_datetime;
                                }
                                if (dbt.validation == null)
                                {
                                    mwqmRunModelNew.LabSampleApprovalContactTVItemID = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.LabSampleApprovalContactTVItemID = null; // 1; // this will be changed in the future to reflect the actuall UserInfoID
                                }
                                if (dbt.niveau_eau == null)
                                {
                                    mwqmRunModelNew.WaterLevelAtBrook_m = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.WaterLevelAtBrook_m = (double)dbt.niveau_eau;
                                }
                                //if (dbt.vague_hauteur_fin == null)
                                //{
                                //    mwqmRunModelNew.WaveHightAtEnd_m = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.WaveHightAtEnd_m = dbt.vague_hauteur_fin;
                                //}
                                //if (dbt.vague_hauteur == null)
                                //{
                                //    mwqmRunModelNew.WaveHightAtStart_m = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.WaveHightAtStart_m = dbt.vague_hauteur;
                                //}

                                string TextEN = "--";
                                if (!string.IsNullOrWhiteSpace(dbt.commentaire))
                                {
                                    TextEN = dbt.commentaire.Trim();
                                }

                                //if (dbt.precipit == null)
                                //{
                                //    mwqmRunModelNew.RainDay1_mm = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.RainDay1_mm = (double)dbt.precipit;
                                //}
                                //if (dbt.precipit_3jant == null)
                                //{
                                //    mwqmRunModelNew.RainDay3_mm = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.RainDay3_mm = dbt.precipit_3jant;
                                //}


                                mwqmRunModelNew.Tide_Start = TideTextEnum.MidTide;

                                if (dbt.maree_principale != null)
                                {
                                    if (dbt.maree_principale == 594)
                                    {
                                        mwqmRunModelNew.Tide_Start = TideTextEnum.LowTide;
                                    }
                                    else if (dbt.maree_principale == 593)
                                    {
                                        mwqmRunModelNew.Tide_Start = TideTextEnum.HighTide;
                                    }
                                    else
                                    {
                                    }
                                }

                                mwqmRunModelNew.Tide_End = mwqmRunModelNew.Tide_Start;


                                mwqmRunModelNew.AnalyzeMethod = null;
                                if (dbt.cf_methode_analytique != null)
                                {
                                    switch (dbt.cf_methode_analytique.Value.ToString())
                                    {
                                        case "0":
                                            {
                                                mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.ZZ_0Q;
                                            }
                                            break;
                                        case "509":
                                            {
                                                mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.ZZ_509Q;
                                            }
                                            break;
                                        case "510":
                                            {
                                                mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.ZZ_510Q;
                                            }
                                            break;
                                        case "525":
                                            {
                                                mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.ZZ_525Q;
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                mwqmRunModelNew.SampleMatrix = SampleMatrixEnum.MPNQ;

                                mwqmRunModelNew.Laboratory = null;

                                if (dbt.laboratoire_operateur_id != null)
                                {
                                    switch (dbt.laboratoire_operateur_id.ToString())
                                    {
                                        case "1":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_0;
                                            }
                                            break;
                                        case "2":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_1;
                                            }
                                            break;
                                        case "3":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_2;
                                            }
                                            break;
                                        case "4":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_3;
                                            }
                                            break;
                                        case "5":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_4;
                                            }
                                            break;
                                        case "6":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_1Q;
                                            }
                                            break;
                                        case "7":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_2Q;
                                            }
                                            break;
                                        case "8":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_3Q;
                                            }
                                            break;
                                        case "9":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_4Q;
                                            }
                                            break;
                                        case "10":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_5Q;
                                            }
                                            break;
                                        case "11":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_11BC;
                                            }
                                            break;
                                        case "12":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_12BC;
                                            }
                                            break;
                                        case "14":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_14BC;
                                            }
                                            break;
                                        case "15":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_15BC;
                                            }
                                            break;
                                        case "16":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_16BC;
                                            }
                                            break;
                                        case "17":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_17BC;
                                            }
                                            break;
                                        case "18":
                                            {
                                                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_18BC;
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                // Doing Status
                                mwqmRunModelNew.SampleStatus = null;
                                if (dbt.status != null)
                                {
                                    if (dbt.status == 11)
                                    {
                                        mwqmRunModelNew.SampleStatus = SampleStatusEnum.Active;
                                    }
                                    else if (dbt.status == 606)
                                    {
                                        mwqmRunModelNew.SampleStatus = SampleStatusEnum.Archived;
                                    }
                                    else
                                    {

                                    }
                                }

                                if (mwqmRunModelNew.StartDateTime_Local > mwqmRunModelNew.EndDateTime_Local)
                                {
                                    mwqmRunModelNew.EndDateTime_Local = mwqmRunModelNew.StartDateTime_Local;
                                }

                                MWQMRunModel mwqmRunModel = (from c in mwqmRunModelAll
                                                             where c.SubsectorTVItemID == tvItemModelSubsector.TVItemID
                                                             && c.MWQMRunTVText == TVTextRun
                                                             && c.RunSampleType == mwqmRunModelNew.RunSampleType
                                                             select c).FirstOrDefault();

                                if (mwqmRunModel == null)
                                {
                                    richTextBoxStatus.AppendText($"{tvItemModelSubsector.TVText} --- { TVTextRun } adding MWQMRun\r\n");
                                    MWQMRunModel mwqmRunModelRet = mwqmRunService.PostAddMWQMRunDB(mwqmRunModelNew);
                                    if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return false;

                                    mwqmRunModelAll.Add(mwqmRunModelRet);
                                }
                            }

                            // doing MWQMSample

                            double? prof = null;
                            if (dbm.prof != null)
                            {
                                prof = (float)dbm.prof;
                            }
                            double? Sal = null;
                            if (dbm.sal != null)
                            {
                                Sal = (float)dbm.sal;
                            }
                            double? Temp = null;
                            if (dbm.temp != null)
                            {
                                Temp = (float)dbm.temp;
                            }
                            double? PH = null;
                            //if (dbm.ph != null)
                            //{
                            //    PH = (float)dbm.ph;
                            //}

                            bool UseForOpenData = true;
                            if (dbm.diffusable == null)
                            {
                                UseForOpenData = false;
                            }
                            else
                            {
                                if (!(bool)dbm.diffusable)
                                {
                                    UseForOpenData = false;
                                }
                            }

                            string sampleNote = "--";
                            if (!string.IsNullOrWhiteSpace(dbm.commentaire))
                            {
                                sampleNote = dbm.commentaire.Trim();
                            }
                            MWQMSampleModel mwqmSampleModelNew = new MWQMSampleModel()
                            {
                                MWQMSiteTVItemID = tvItemMWQMSiteExist.TVItemID,
                                SampleDateTime_Local = (DateTime)SampDateTime,
                                Depth_m = prof,
                                FecCol_MPN_100ml = (int)(dbm.cf == null ? 0 : dbm.cf),
                                Salinity_PPT = Sal,
                                WaterTemp_C = Temp,
                                PH = PH,
                                MWQMSampleNote = sampleNote,
                                SampleTypesText = ((int)SampleTypeEnum.Routine).ToString() + ",",
                                SampleTypeList = new List<SampleTypeEnum>() { SampleTypeEnum.Routine },
                                UseForOpenData = UseForOpenData,
                            };

                            if (mwqmSampleModelNew.PH == 88.180000305175781f)
                            {
                                mwqmSampleModelNew.PH = 8.8180000305175781f;
                            }

                            if (mwqmSampleModelNew.Salinity_PPT == 99.9000015258789)
                            {
                                mwqmSampleModelNew.Salinity_PPT = 9.99000015258789;
                            }
                            if (mwqmSampleModelNew.Salinity_PPT == 51.799999237060547)
                            {
                                mwqmSampleModelNew.Salinity_PPT = 5.1799999237060547;
                            }
                            if (mwqmSampleModelNew.PH == 20.600000381469727)
                            {
                                mwqmSampleModelNew.PH = 2.0600000381469727;
                            }

                            // new code to delet later
                            mwqmRunModelExist = (from c in mwqmRunModelAll
                                                 where c.SubsectorTVItemID == tvItemModelSubsector.TVItemID
                                                 && c.DateTime_Local == DateRun
                                                 && c.RunSampleType == SampleTypeEnum.Routine
                                                 && c.RunNumber == 1
                                                 select c).FirstOrDefault();



                            if (mwqmRunModelExist == null)
                            {
                                richTextBoxStatus.AppendText($"Could not find MWQMRunModel ss {tvItemModelSubsector.TVText} --- {DateRun.ToString("yyyy MM dd")}");
                                return false;
                            }

                            //if (mwqmSampleModelNew.WaterTemp_C < 0)
                            //{
                            //    mwqmSampleModelNew.WaterTemp_C = 0;
                            //}

                            //mwqmSampleModelNew.MWQMRunTVItemID = mwqmRunModelExist.MWQMRunTVItemID;

                            MWQMSample mwqmSampleNew = new MWQMSample()
                            {
                                MWQMSiteTVItemID = tvItemMWQMSiteExist.TVItemID,
                                MWQMRunTVItemID = mwqmRunModelExist.MWQMRunTVItemID,
                                SampleDateTime_Local = (DateTime)SampDateTime,
                                Depth_m = mwqmSampleModelNew.Depth_m,
                                FecCol_MPN_100ml = mwqmSampleModelNew.FecCol_MPN_100ml,
                                Salinity_PPT = mwqmSampleModelNew.Salinity_PPT,
                                WaterTemp_C = mwqmSampleModelNew.WaterTemp_C,
                                PH = mwqmSampleModelNew.PH,
                                SampleTypesText = "109,",
                                SampleType_old = 4,
                                Tube_10 = null,
                                Tube_1_0 = null,
                                Tube_0_1 = null,
                                ProcessedBy = null,
                                UseForOpenData = UseForOpenData,
                                LastUpdateDate_UTC = DateTime.UtcNow,
                                LastUpdateContactTVItemID = 2,
                            };

                            MWQMSampleLanguage mwqmSampleLanguageEnNew = new MWQMSampleLanguage()
                            {
                                Language = (int)LanguageEnum.en,
                                MWQMSampleNote = mwqmSampleModelNew.MWQMSampleNote,
                                TranslationStatus = (int)TranslationStatusEnum.Translated,
                                LastUpdateDate_UTC = DateTime.UtcNow,
                                LastUpdateContactTVItemID = 2,
                            };

                            mwqmSampleNew.MWQMSampleLanguages.Add(mwqmSampleLanguageEnNew);

                            MWQMSampleLanguage mwqmSampleLanguageFrNew = new MWQMSampleLanguage()
                            {
                                Language = (int)LanguageEnum.fr,
                                MWQMSampleNote = mwqmSampleModelNew.MWQMSampleNote,
                                TranslationStatus = (int)TranslationStatusEnum.NotTranslated,
                                LastUpdateDate_UTC = DateTime.UtcNow,
                                LastUpdateContactTVItemID = 2,
                            };

                            mwqmSampleNew.MWQMSampleLanguages.Add(mwqmSampleLanguageFrNew);

                            MWQMSample mwqmSampleExist = (from c in mwqmSampleCSSPList
                                                          where c.MWQMSiteTVItemID == tvItemMWQMSiteExist.TVItemID
                                                          && c.MWQMRunTVItemID == mwqmRunModelExist.MWQMRunTVItemID
                                                          && c.SampleDateTime_Local == (DateTime)SampDateTime
                                                          && c.Depth_m == mwqmSampleModelNew.Depth_m
                                                          && c.SampleTypesText == "109,"
                                                          select c).FirstOrDefault();

                            if (mwqmSampleExist == null)
                            {
                                mwqmSampleListToAdd.Add(mwqmSampleNew);
                            }

                            MWQMSample mwqmSampleExist2 = (from c in mwqmSampleCSSPList
                                                           where c.MWQMSiteTVItemID == tvItemMWQMSiteExist.TVItemID
                                                           && c.MWQMRunTVItemID == mwqmRunModelExist.MWQMRunTVItemID
                                                           && c.SampleDateTime_Local == (DateTime)SampDateTime
                                                           && c.Depth_m == mwqmSampleModelNew.Depth_m
                                                           && c.SampleTypesText == "109,"
                                                           select c).FirstOrDefault();

                            if (mwqmSampleExist2 != null)
                            {
                                if (mwqmSampleExist2.UseForOpenData != UseForOpenData)
                                {
                                    mwqmSampleExist2.UseForOpenData = UseForOpenData;
                                    mwqmSampleListToUpdate.Add(mwqmSampleExist2);
                                }
                            }

                        }

                        using (CSSPDBEntities db2 = new CSSPDBEntities())
                        {
                            try
                            {
                                if (mwqmSampleListToAdd.Count > 0)
                                {
                                    db2.MWQMSamples.AddRange(mwqmSampleListToAdd);
                                    db2.SaveChanges();
                                }
                            }
                            catch (Exception)
                            {
                                richTextBoxStatus.AppendText($"Could not add MWQMSampleList ss {tvItemModelSubsector.TVText} --- {tvItemMWQMSiteExist.TVText}");
                                return false;
                            }

                            if (mwqmSampleListToUpdate.Count > 0)
                            {
                                List<int> MWQMSampleIDList = (from c in mwqmSampleListToUpdate
                                                              select c.MWQMSampleID).ToList();

                                List<MWQMSample> mwqmSampleToChangeList = (from c in db2.MWQMSamples
                                                                           from u in MWQMSampleIDList
                                                                           where c.MWQMSampleID == u
                                                                           select c).ToList();

                                foreach (MWQMSample mwqmSample in mwqmSampleToChangeList)
                                {
                                    MWQMSample mwqmSampleChanged = mwqmSampleListToUpdate.Where(c => c.MWQMSampleID == mwqmSample.MWQMSampleID).FirstOrDefault();

                                    if (mwqmSampleChanged != null)
                                    {
                                        mwqmSample.UseForOpenData = mwqmSampleChanged.UseForOpenData;
                                    }
                                }

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not change MWQMSampleList ss {tvItemModelSubsector.TVText} --- {tvItemMWQMSiteExist.TVText}");
                                    return false;

                                }
                            }

                            try
                            {

                            }
                            catch (Exception)
                            {

                                throw;
                            }
                        }

                    }
                }
            }

            return true;

        }
        //public bool CreateSamplesQC_old()
        //{
        //    if (Cancel) return false;

        //    lblStatus.Text = "Starting ... CreateSamplesQC";
        //    Application.DoEvents();

        //    TempData.QCSubsectorAssociation qcSubsectAss = new TempData.QCSubsectorAssociation();

        //    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
        //    MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);
        //    MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
        //    MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);

        //    TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
        //    if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

        //    TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
        //    if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

        //    TVItemModel tvItemModelQC = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
        //    if (!CheckModelOK<TVItemModel>(tvItemModelQC)) return false;

        //    List<TVItemModel> tvItemModelSubsectorQCList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
        //    if (tvItemModelSubsectorQCList.Count == 0)
        //    {
        //        richTextBoxStatus.AppendText("Could not find TVItem Subsector under British Columbia\r\n");
        //        return false;
        //    }

        //    int StartQCCreateSamplesQC = int.Parse(textBoxQCCreateSamplesQC.Text);

        //    List<string> sectorList = new List<string>();
        //    List<TempData.QCSubsectorAssociation> qcSubAssList = new List<TempData.QCSubsectorAssociation>();

        //    using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
        //    {
        //        qcSubAssList = (from c in dbDT.QCSubsectorAssociations
        //                        select c).ToList<TempData.QCSubsectorAssociation>();
        //    }

        //    using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
        //    {
        //        sectorList = (from s in dbQC.geo_stations_p
        //                      where s.secteur != null
        //                      select s.secteur).Distinct().ToList();
        //    }

        //    List<string> sectorOrderedList = (from c in sectorList
        //                                      orderby c
        //                                      select c).ToList();

        //    List<PCCSM.geo_stations_p> staQCList = new List<PCCSM.geo_stations_p>();
        //    using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
        //    {

        //        staQCList = (from c in dbQC.geo_stations_p
        //                     orderby c.secteur, c.station
        //                     select c).ToList<PCCSM.geo_stations_p>();
        //    }

        //    int count = 0;
        //    int totalCount = staQCList.Count;
        //    foreach (PCCSM.geo_stations_p geoStat in staQCList)
        //    {
        //        if (Cancel) return false;

        //        count += 1;
        //        lblStatus.Text = "Doing " + count + " of " + totalCount;
        //        Application.DoEvents();

        //        textBoxQCCreateSamplesQC.Text = count.ToString();

        //        if (StartQCCreateSamplesQC > count)
        //        {
        //            continue;
        //        }

        //        if (geoStat.secteur == null || geoStat.station == null)
        //        {
        //            continue;
        //        }


        //        qcSubsectAss = (from c in qcSubAssList
        //                        where c.QCSectorText == geoStat.secteur
        //                        select c).FirstOrDefault<TempData.QCSubsectorAssociation>();

        //        //if (geoStat.secteur == "G-22.1E")
        //        //{
        //        //    continue;
        //        //}

        //        if (qcSubsectAss == null)
        //        {
        //            // should make sure the secteur is within the TempData.QCSubsectorAssociation
        //            // int sleifj = 34;
        //            continue;
        //        }

        //        TVItemModel tvItemModelSubsector = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelQC.TVItemID, qcSubsectAss.SubsectorText, TVTypeEnum.Subsector);
        //        if (!CheckModelOK<TVItemModel>(tvItemModelSubsector))
        //        {
        //            //return false;
        //            continue;
        //        }

        //        bool IsActive = true;
        //        if (geoStat.status != null)
        //        {
        //            IsActive = (geoStat.status.Substring(0, 1) == "i" ? false : true);
        //        }
        //        string PreText = "";
        //        if (geoStat.secteur.Length < qcSubsectAss.SubsectorText.Length)
        //        {
        //            PreText = "";
        //        }
        //        else
        //        {
        //            if (geoStat.secteur.StartsWith(qcSubsectAss.SubsectorText))
        //            {
        //                PreText = geoStat.secteur.Substring(qcSubsectAss.SubsectorText.Length) + "_";
        //            }
        //            else
        //            {
        //                PreText = geoStat.secteur + "_";
        //            }
        //        }

        //        if (PreText.StartsWith(".") || PreText.StartsWith("_"))
        //        {
        //            PreText = PreText.Substring(1);
        //        }

        //        string MWQMSiteTVText = PreText + "0000".Substring(0, 4 - geoStat.station.ToString().Length) + geoStat.station.ToString();

        //        // check if WQMSite already in db
        //        TVItemModel tvItemModelMWQMSiteExist = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelSubsector.TVItemID, MWQMSiteTVText, TVTypeEnum.MWQMSite);
        //        if (!CheckModelOK<TVItemModel>(tvItemModelMWQMSiteExist))
        //        {
        //            //return false;
        //            continue;
        //        }

        //        List<MWQMSampleModel> mwqmSampleModelList = mwqmSampleService.GetMWQMSampleModelListWithMWQMSiteTVItemIDDB(tvItemModelMWQMSiteExist.TVItemID);
        //        List<MWQMRunModel> mwqmRunModelList = mwqmRunService.GetMWQMRunModelListWithSubsectorTVItemIDDB(tvItemModelSubsector.TVItemID);
        //        using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
        //        {

        //            List<PCCSM.db_mesure> dbMesureList = (from s in dbQC.geo_stations_p
        //                                                  from m in dbQC.db_mesure
        //                                                  where s.id_geo_station_p == m.id_geo_station_p
        //                                                  && s.id_geo_station_p == geoStat.id_geo_station_p
        //                                                  select m).ToList<PCCSM.db_mesure>();

        //            int SampleCount = 0;
        //            int TotalSampleCount = dbMesureList.Count;
        //            List<MWQMSample> mwqmSampleListToAdd = new List<MWQMSample>();
        //            foreach (PCCSM.db_mesure dbm in dbMesureList)
        //            {
        //                SampleCount += 1;
        //                lblStatus2.Text = qcSubsectAss.SubsectorText + " --- " + dbm.station.ToString() + " --- " + SampleCount + "/" + TotalSampleCount;
        //                Application.DoEvents();

        //                // getting Runs
        //                PCCSM.db_tournee dbt = (from t in dbQC.db_tournee
        //                                        where t.ID_Tournee == dbm.id_tournee
        //                                        select t).FirstOrDefault();

        //                DateTime? SampDateTime = null;
        //                DateTime? SampStartDateTime = null;
        //                DateTime? SampEndDateTime = null;
        //                if (dbm.hre_echantillonnage != null)
        //                {
        //                    SampDateTime = (DateTime)dbm.hre_echantillonnage.Value.AddHours(1);
        //                    if (dbt.hre_fin != null)
        //                    {
        //                        SampEndDateTime = ((DateTime)dbt.date_echantillonnage).AddHours(dbt.hre_fin.Value.Hour).AddMinutes(dbt.hre_fin.Value.Minute).AddHours(1);
        //                    }
        //                    if (dbt.hre_deb != null)
        //                    {
        //                        SampStartDateTime = ((DateTime)dbt.date_echantillonnage).AddHours(dbt.hre_deb.Value.Hour).AddMinutes(dbt.hre_deb.Value.Minute).AddHours(1);
        //                    }
        //                }
        //                else
        //                {
        //                    SampDateTime = (DateTime)dbt.date_echantillonnage;
        //                    if (dbt.hre_fin != null)
        //                    {
        //                        SampEndDateTime = ((DateTime)dbt.date_echantillonnage).AddHours(dbt.hre_fin.Value.Hour).AddMinutes(dbt.hre_fin.Value.Minute).AddHours(1);
        //                        SampDateTime = SampEndDateTime;
        //                    }
        //                    if (dbt.hre_deb != null)
        //                    {
        //                        SampStartDateTime = ((DateTime)dbt.date_echantillonnage).AddHours(dbt.hre_deb.Value.Hour).AddMinutes(dbt.hre_deb.Value.Minute).AddHours(1);
        //                        SampDateTime = SampStartDateTime;
        //                    }
        //                }


        //                // doing samp

        //                double? prof = null;
        //                if (dbm.prof != null)
        //                {
        //                    prof = (float)dbm.prof;
        //                }
        //                double? Sal = null;
        //                if (dbm.sal != null)
        //                {
        //                    Sal = (float)dbm.sal;
        //                }
        //                double? Temp = null;
        //                if (dbm.temp != null)
        //                {
        //                    Temp = (float)dbm.temp;
        //                }
        //                double? PH = null;
        //                if (dbm.ph != null)
        //                {
        //                    PH = (float)dbm.ph;
        //                }


        //                string sampleNote = "--";
        //                if (!string.IsNullOrWhiteSpace(dbm.commentaire))
        //                {
        //                    sampleNote = dbm.commentaire.Trim();
        //                }
        //                MWQMSampleModel mwqmSampleModelNew = new MWQMSampleModel()
        //                {
        //                    MWQMSiteTVItemID = tvItemModelMWQMSiteExist.TVItemID,
        //                    SampleDateTime_Local = (DateTime)SampDateTime,
        //                    Depth_m = prof,
        //                    FecCol_MPN_100ml = (int)(dbm.cf == null ? 0 : dbm.cf),
        //                    Salinity_PPT = Sal,
        //                    WaterTemp_C = Temp,
        //                    PH = PH,
        //                    MWQMSampleNote = sampleNote,
        //                    SampleTypesText = ((int)SampleTypeEnum.Routine).ToString() + ",",
        //                    SampleTypeList = new List<SampleTypeEnum>() { SampleTypeEnum.Routine },
        //                };

        //                if (mwqmSampleModelNew.PH == 88.180000305175781f)
        //                {
        //                    mwqmSampleModelNew.PH = 8.8180000305175781f;
        //                }

        //                if (mwqmSampleModelNew.Salinity_PPT == 99.9000015258789)
        //                {
        //                    mwqmSampleModelNew.Salinity_PPT = 9.99000015258789;
        //                }
        //                if (mwqmSampleModelNew.Salinity_PPT == 51.799999237060547)
        //                {
        //                    mwqmSampleModelNew.Salinity_PPT = 5.1799999237060547;
        //                }
        //                if (mwqmSampleModelNew.PH == 20.600000381469727)
        //                {
        //                    mwqmSampleModelNew.PH = 2.0600000381469727;
        //                }

        //                // new code to delet later
        //                MWQMRunModel mwqmRunModel = (from c in mwqmRunModelList
        //                                             where c.DateTime_Local.Year == mwqmSampleModelNew.SampleDateTime_Local.Year
        //                                             && c.DateTime_Local.Month == mwqmSampleModelNew.SampleDateTime_Local.Month
        //                                             && c.DateTime_Local.Day == mwqmSampleModelNew.SampleDateTime_Local.Day
        //                                             && c.RunSampleType == SampleTypeEnum.Routine
        //                                             select c).FirstOrDefault();
        //                if (mwqmRunModel == null)
        //                {
        //                    //return false;
        //                    continue;
        //                }

        //                if (mwqmSampleModelNew.WaterTemp_C < 0)
        //                {
        //                    mwqmSampleModelNew.WaterTemp_C = 0;
        //                }
        //                mwqmSampleModelNew.MWQMRunTVItemID = mwqmRunModel.MWQMRunTVItemID;

        //                MWQMSample mwqmSampleNew = new MWQMSample()
        //                {
        //                    MWQMSiteTVItemID = mwqmSampleModelNew.MWQMSiteTVItemID,
        //                    MWQMRunTVItemID = mwqmSampleModelNew.MWQMRunTVItemID,
        //                    SampleDateTime_Local = mwqmSampleModelNew.SampleDateTime_Local,
        //                    Depth_m = mwqmSampleModelNew.Depth_m,
        //                    FecCol_MPN_100ml = mwqmSampleModelNew.FecCol_MPN_100ml,
        //                    Salinity_PPT = mwqmSampleModelNew.Salinity_PPT,
        //                    WaterTemp_C = mwqmSampleModelNew.WaterTemp_C,
        //                    PH = mwqmSampleModelNew.PH,
        //                    SampleTypesText = "109,",
        //                    SampleType_old = 4,
        //                    Tube_10 = null,
        //                    Tube_1_0 = null,
        //                    Tube_0_1 = null,
        //                    ProcessedBy = null,
        //                    LastUpdateDate_UTC = DateTime.UtcNow,
        //                    LastUpdateContactTVItemID = 2,
        //                };

        //                MWQMSampleLanguage mwqmSampleLanguageEnNew = new MWQMSampleLanguage()
        //                {
        //                    Language = (int)LanguageEnum.en,
        //                    MWQMSampleNote = mwqmSampleModelNew.MWQMSampleNote,
        //                    TranslationStatus = (int)TranslationStatusEnum.Translated,
        //                    LastUpdateDate_UTC = DateTime.UtcNow,
        //                    LastUpdateContactTVItemID = 2,
        //                };

        //                mwqmSampleNew.MWQMSampleLanguages.Add(mwqmSampleLanguageEnNew);

        //                MWQMSampleLanguage mwqmSampleLanguageFrNew = new MWQMSampleLanguage()
        //                {
        //                    Language = (int)LanguageEnum.fr,
        //                    MWQMSampleNote = mwqmSampleModelNew.MWQMSampleNote,
        //                    TranslationStatus = (int)TranslationStatusEnum.NotTranslated,
        //                    LastUpdateDate_UTC = DateTime.UtcNow,
        //                    LastUpdateContactTVItemID = 2,
        //                };

        //                mwqmSampleNew.MWQMSampleLanguages.Add(mwqmSampleLanguageFrNew);
        //                mwqmSampleListToAdd.Add(mwqmSampleNew);

        //                //MWQMSampleModel mwqmSampleModelRet = (from c in mwqmSampleModelList
        //                //                                      where c.MWQMSiteTVItemID == mwqmSampleModelNew.MWQMSiteTVItemID
        //                //                                      && c.SampleDateTime_Local == mwqmSampleModelNew.SampleDateTime_Local
        //                //                                      && c.FecCol_MPN_100ml == mwqmSampleModelNew.FecCol_MPN_100ml
        //                //                                      && c.Salinity_PPT == mwqmSampleModelNew.Salinity_PPT
        //                //                                      && c.WaterTemp_C == mwqmSampleModelNew.WaterTemp_C
        //                //                                      && c.SampleTypesText.Contains(mwqmSampleModelNew.SampleTypesText)
        //                //                                      select c).FirstOrDefault();

        //                //if (mwqmSampleModelRet == null)
        //                //{
        //                //    MWQMRunModel mwqmRunModel = (from c in mwqmRunModelList
        //                //                                 where c.DateTime_Local.Year == mwqmSampleModelNew.SampleDateTime_Local.Year
        //                //                                 && c.DateTime_Local.Month == mwqmSampleModelNew.SampleDateTime_Local.Month
        //                //                                 && c.DateTime_Local.Day == mwqmSampleModelNew.SampleDateTime_Local.Day
        //                //                                 select c).FirstOrDefault();
        //                //    if (mwqmRunModel == null)
        //                //    {
        //                //        //return false;
        //                //        continue;
        //                //    }

        //                //    if (mwqmSampleModelNew.WaterTemp_C < 0)
        //                //    {
        //                //        mwqmSampleModelNew.WaterTemp_C = 0;
        //                //    }
        //                //    mwqmSampleModelNew.MWQMRunTVItemID = mwqmRunModel.MWQMRunTVItemID;

        //                //    mwqmSampleModelRet = mwqmSampleService.PostAddMWQMSampleDB(mwqmSampleModelNew);
        //                //    if (!CheckModelOK<MWQMSampleModel>(mwqmSampleModelRet))
        //                //    {
        //                //        //return false;
        //                //        continue;
        //                //    }
        //                //}
        //                //else
        //                //{
        //                //    if (mwqmSampleModelRet.FecCol_MPN_100ml != mwqmSampleModelNew.FecCol_MPN_100ml
        //                //        || mwqmSampleModelRet.Salinity_PPT != mwqmSampleModelNew.Salinity_PPT
        //                //        || mwqmSampleModelRet.WaterTemp_C != mwqmSampleModelNew.WaterTemp_C
        //                //        || mwqmSampleModelRet.PH != mwqmSampleModelNew.PH
        //                //        || mwqmSampleModelRet.Depth_m != mwqmSampleModelNew.Depth_m)
        //                //    {
        //                //        mwqmSampleModelRet.FecCol_MPN_100ml = mwqmSampleModelNew.FecCol_MPN_100ml;
        //                //        mwqmSampleModelRet.Salinity_PPT = mwqmSampleModelNew.Salinity_PPT;
        //                //        mwqmSampleModelRet.WaterTemp_C = mwqmSampleModelNew.WaterTemp_C;
        //                //        mwqmSampleModelRet.PH = mwqmSampleModelNew.PH;
        //                //        mwqmSampleModelRet.Depth_m = mwqmSampleModelNew.Depth_m;

        //                //        mwqmSampleModelRet = mwqmSampleService.PostUpdateMWQMSampleDB(mwqmSampleModelNew);
        //                //        if (!CheckModelOK<MWQMSampleModel>(mwqmSampleModelRet))
        //                //        {
        //                //            //return false;
        //                //            continue;
        //                //        }
        //                //    }
        //                //}
        //            }
        //            using (CSSPDBEntities db2 = new CSSPDBEntities())
        //            {
        //                try
        //                {
        //                    db2.MWQMSamples.AddRange(mwqmSampleListToAdd);
        //                    db2.SaveChanges();
        //                }
        //                catch (Exception)
        //                {
        //                    //return false;
        //                    continue;
        //                }
        //            }

        //        }
        //    }

        //    return true;
        //}
    }
}
