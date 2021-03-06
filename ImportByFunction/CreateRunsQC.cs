﻿using CSSPDBDLL;
using CSSPModelsDLL.Models;
using CSSPDBDLL.Services;
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

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CreateRunsQC()
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

            int StartQCCreateRunsQC = int.Parse(textBoxQCCreateRunsQC.Text);

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

                textBoxQCCreateRunsQC.Text = Count.ToString();

                if (StartQCCreateRunsQC > Count)
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
                        string MWQMSiteTVText = "0000".Substring(0, 4 - geoStat.station.ToString().Length) + geoStat.station.ToString();

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

                            DateTime DateRun = new DateTime(((DateTime)SampDateTime).Year, ((DateTime)SampDateTime).Month, ((DateTime)SampDateTime).Day);

                            MWQMRunModel mwqmRunModelNew = new MWQMRunModel()
                            {
                                SubsectorTVItemID = tvItemModelSubsector.TVItemID,
                                DateTime_Local = DateRun,
                                RunSampleType = SampleTypeEnum.Routine,
                                RunNumber = 1,
                            };

                            MWQMRunModel wqmRunModelExist = (from c in mwqmRunModelAll
                                                             where c.DateTime_Local == DateRun
                                                             && c.RunSampleType == SampleTypeEnum.Routine
                                                             && c.RunNumber == 1
                                                             select c).FirstOrDefault();

                            if (wqmRunModelExist == null)
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
                                if (dbt.mer_etat_fin == null)
                                {
                                    mwqmRunModelNew.SeaStateAtEnd_BeaufortScale = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.SeaStateAtEnd_BeaufortScale = (BeaufortScaleEnum)dbt.mer_etat_fin;
                                }
                                if (dbt.mer_etat == null)
                                {
                                    mwqmRunModelNew.SeaStateAtStart_BeaufortScale = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.SeaStateAtStart_BeaufortScale = (BeaufortScaleEnum)dbt.mer_etat;
                                }
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
                                if (dbt.vague_hauteur_fin == null)
                                {
                                    mwqmRunModelNew.WaveHightAtEnd_m = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.WaveHightAtEnd_m = dbt.vague_hauteur_fin;
                                }
                                if (dbt.vague_hauteur == null)
                                {
                                    mwqmRunModelNew.WaveHightAtStart_m = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.WaveHightAtStart_m = dbt.vague_hauteur;
                                }

                                string TextEN = "--";
                                if (!string.IsNullOrWhiteSpace(dbt.commentaire))
                                {
                                    TextEN = dbt.commentaire.Trim();
                                }

                                if (dbt.precipit == null)
                                {
                                    mwqmRunModelNew.RainDay1_mm = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.RainDay1_mm = dbt.precipit;
                                }
                                if (dbt.precipit_3jant == null)
                                {
                                    mwqmRunModelNew.RainDay3_mm = null;
                                }
                                else
                                {
                                    mwqmRunModelNew.RainDay3_mm = dbt.precipit_3jant;
                                }


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
                        }
                    }
                }
            }

            return true;

        }
    }
}
