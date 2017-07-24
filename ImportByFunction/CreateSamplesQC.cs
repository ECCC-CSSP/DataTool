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
        public bool CreateSamplesQC()
        {
            if (Cancel) return false;

            lblStatus.Text = "Starting ... CreateSamplesQC";
            Application.DoEvents();

            TempData.QCSubsectorAssociation qcSubsectAss = new TempData.QCSubsectorAssociation();

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
            MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelQC = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelQC)) return false;

            List<TVItemModel> tvItemModelSubsectorQCList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorQCList.Count == 0)
            {
                richTextBoxStatus.AppendText("Could not find TVItem Subsector under British Columbia\r\n");
                return false;
            }

            int StartQCCreateSamplesQC = int.Parse(textBoxQCCreateSamplesQC.Text);

            List<string> sectorList = new List<string>();
            List<TempData.QCSubsectorAssociation> qcSubAssList = new List<TempData.QCSubsectorAssociation>();

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {
                qcSubAssList = (from c in dbDT.QCSubsectorAssociations
                                select c).ToList<TempData.QCSubsectorAssociation>();
            }

            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {
                sectorList = (from s in dbQC.geo_stations_p
                              where s.secteur != null
                              select s.secteur).Distinct().ToList();
            }

            List<string> sectorOrderedList = (from c in sectorList
                                              orderby c
                                              select c).ToList();

            List<PCCSM.geo_stations_p> staQCList = new List<PCCSM.geo_stations_p>();
            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {

                staQCList = (from c in dbQC.geo_stations_p
                             orderby c.secteur, c.station
                             select c).ToList<PCCSM.geo_stations_p>();
            }

            int count = 0;
            int totalCount = staQCList.Count;
            foreach (PCCSM.geo_stations_p geoStat in staQCList)
            {
                if (Cancel) return false;

                count += 1;
                lblStatus.Text = "Doing " + count + " of " + totalCount;
                Application.DoEvents();

                textBoxQCCreateSamplesQC.Text = count.ToString();

                if (StartQCCreateSamplesQC > count)
                {
                    continue;
                }

                if (geoStat.secteur == null || geoStat.station == null)
                {
                    continue;
                }


                qcSubsectAss = (from c in qcSubAssList
                                where c.QCSectorText == geoStat.secteur
                                select c).FirstOrDefault<TempData.QCSubsectorAssociation>();

                //if (geoStat.secteur == "G-22.1E")
                //{
                //    continue;
                //}

                if (qcSubsectAss == null)
                {
                    // should make sure the secteur is within the TempData.QCSubsectorAssociation
                    int sleifj = 34;
                    continue;
                }

                TVItemModel tvItemModelSubsector = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelQC.TVItemID, qcSubsectAss.SubsectorText, TVTypeEnum.Subsector);
                if (!CheckModelOK<TVItemModel>(tvItemModelSubsector))
                {
                    //return false;
                    continue;
                }

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

                // check if WQMSite already in db
                TVItemModel tvItemModelMWQMSiteExist = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelSubsector.TVItemID, MWQMSiteTVText, TVTypeEnum.MWQMSite);
                if (!CheckModelOK<TVItemModel>(tvItemModelMWQMSiteExist))
                {
                    //return false;
                    continue;
                }

                List<MWQMSampleModel> mwqmSampleModelList = mwqmSampleService.GetMWQMSampleModelListWithMWQMSiteTVItemIDDB(tvItemModelMWQMSiteExist.TVItemID);
                List<MWQMRunModel> mwqmRunModelList = mwqmRunService.GetMWQMRunModelListWithSubsectorTVItemIDDB(tvItemModelSubsector.TVItemID);
                using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
                {

                    List<PCCSM.db_mesure> dbMesureList = (from s in dbQC.geo_stations_p
                                                          from m in dbQC.db_mesure
                                                          where s.id_geo_station_p == m.id_geo_station_p
                                                          && s.id_geo_station_p == geoStat.id_geo_station_p
                                                          select m).ToList<PCCSM.db_mesure>();

                    int SampleCount = 0;
                    int TotalSampleCount = dbMesureList.Count;
                    List<MWQMSample> mwqmSampleListToAdd = new List<MWQMSample>();
                    foreach (PCCSM.db_mesure dbm in dbMesureList)
                    {
                        SampleCount += 1;
                        lblStatus2.Text = qcSubsectAss.SubsectorText + " --- " + dbm.station.ToString() + " --- " + SampleCount + "/" + TotalSampleCount;
                        Application.DoEvents();

                        // getting Runs
                        PCCSM.db_tournee dbt = (from t in dbQC.db_tournee
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


                        // doing samp

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
                        if (dbm.ph != null)
                        {
                            PH = (float)dbm.ph;
                        }
                        string sampleNote = "--";
                        if (!string.IsNullOrWhiteSpace(dbm.commentaire))
                        {
                            sampleNote = dbm.commentaire.Trim();
                        }
                        MWQMSampleModel mwqmSampleModelNew = new MWQMSampleModel()
                        {
                            MWQMSiteTVItemID = tvItemModelMWQMSiteExist.TVItemID,
                            SampleDateTime_Local = (DateTime)SampDateTime,
                            Depth_m = prof,
                            FecCol_MPN_100ml = (int)(dbm.cf == null ? 0 : dbm.cf),
                            Salinity_PPT = Sal,
                            WaterTemp_C = Temp,
                            PH = PH,
                            MWQMSampleNote = sampleNote,
                            SampleTypesText = ((int)SampleTypeEnum.Routine).ToString() + ",",
                            SampleTypeList = new List<SampleTypeEnum>() { SampleTypeEnum.Routine },
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
                        MWQMRunModel mwqmRunModel = (from c in mwqmRunModelList
                                                     where c.DateTime_Local.Year == mwqmSampleModelNew.SampleDateTime_Local.Year
                                                     && c.DateTime_Local.Month == mwqmSampleModelNew.SampleDateTime_Local.Month
                                                     && c.DateTime_Local.Day == mwqmSampleModelNew.SampleDateTime_Local.Day
                                                     select c).FirstOrDefault();
                        if (mwqmRunModel == null)
                        {
                            //return false;
                            continue;
                        }

                        if (mwqmSampleModelNew.WaterTemp_C < 0)
                        {
                            mwqmSampleModelNew.WaterTemp_C = 0;
                        }
                        mwqmSampleModelNew.MWQMRunTVItemID = mwqmRunModel.MWQMRunTVItemID;

                        MWQMSample mwqmSampleNew = new MWQMSample()
                        {
                            MWQMSiteTVItemID = mwqmSampleModelNew.MWQMSiteTVItemID,
                            MWQMRunTVItemID = mwqmSampleModelNew.MWQMRunTVItemID,
                            SampleDateTime_Local = mwqmSampleModelNew.SampleDateTime_Local,
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
                        mwqmSampleListToAdd.Add(mwqmSampleNew);

                        //MWQMSampleModel mwqmSampleModelRet = (from c in mwqmSampleModelList
                        //                                      where c.MWQMSiteTVItemID == mwqmSampleModelNew.MWQMSiteTVItemID
                        //                                      && c.SampleDateTime_Local == mwqmSampleModelNew.SampleDateTime_Local
                        //                                      && c.FecCol_MPN_100ml == mwqmSampleModelNew.FecCol_MPN_100ml
                        //                                      && c.Salinity_PPT == mwqmSampleModelNew.Salinity_PPT
                        //                                      && c.WaterTemp_C == mwqmSampleModelNew.WaterTemp_C
                        //                                      && c.SampleTypesText.Contains(mwqmSampleModelNew.SampleTypesText)
                        //                                      select c).FirstOrDefault();

                        //if (mwqmSampleModelRet == null)
                        //{
                        //    MWQMRunModel mwqmRunModel = (from c in mwqmRunModelList
                        //                                 where c.DateTime_Local.Year == mwqmSampleModelNew.SampleDateTime_Local.Year
                        //                                 && c.DateTime_Local.Month == mwqmSampleModelNew.SampleDateTime_Local.Month
                        //                                 && c.DateTime_Local.Day == mwqmSampleModelNew.SampleDateTime_Local.Day
                        //                                 select c).FirstOrDefault();
                        //    if (mwqmRunModel == null)
                        //    {
                        //        //return false;
                        //        continue;
                        //    }

                        //    if (mwqmSampleModelNew.WaterTemp_C < 0)
                        //    {
                        //        mwqmSampleModelNew.WaterTemp_C = 0;
                        //    }
                        //    mwqmSampleModelNew.MWQMRunTVItemID = mwqmRunModel.MWQMRunTVItemID;

                        //    mwqmSampleModelRet = mwqmSampleService.PostAddMWQMSampleDB(mwqmSampleModelNew);
                        //    if (!CheckModelOK<MWQMSampleModel>(mwqmSampleModelRet))
                        //    {
                        //        //return false;
                        //        continue;
                        //    }
                        //}
                        //else
                        //{
                        //    if (mwqmSampleModelRet.FecCol_MPN_100ml != mwqmSampleModelNew.FecCol_MPN_100ml
                        //        || mwqmSampleModelRet.Salinity_PPT != mwqmSampleModelNew.Salinity_PPT
                        //        || mwqmSampleModelRet.WaterTemp_C != mwqmSampleModelNew.WaterTemp_C
                        //        || mwqmSampleModelRet.PH != mwqmSampleModelNew.PH
                        //        || mwqmSampleModelRet.Depth_m != mwqmSampleModelNew.Depth_m)
                        //    {
                        //        mwqmSampleModelRet.FecCol_MPN_100ml = mwqmSampleModelNew.FecCol_MPN_100ml;
                        //        mwqmSampleModelRet.Salinity_PPT = mwqmSampleModelNew.Salinity_PPT;
                        //        mwqmSampleModelRet.WaterTemp_C = mwqmSampleModelNew.WaterTemp_C;
                        //        mwqmSampleModelRet.PH = mwqmSampleModelNew.PH;
                        //        mwqmSampleModelRet.Depth_m = mwqmSampleModelNew.Depth_m;

                        //        mwqmSampleModelRet = mwqmSampleService.PostUpdateMWQMSampleDB(mwqmSampleModelNew);
                        //        if (!CheckModelOK<MWQMSampleModel>(mwqmSampleModelRet))
                        //        {
                        //            //return false;
                        //            continue;
                        //        }
                        //    }
                        //}
                    }
                    using (CSSPWebToolsDBEntities db2 = new CSSPWebToolsDBEntities())
                    {
                        try
                        {
                            db2.MWQMSamples.AddRange(mwqmSampleListToAdd);
                            db2.SaveChanges();
                        }
                        catch (Exception)
                        {
                            //return false;
                            continue;
                        }
                    }

                }
            }

            return true;
        }
    }
}
