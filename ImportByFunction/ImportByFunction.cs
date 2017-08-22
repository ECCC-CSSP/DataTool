using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSSPModelsDLL.Models;
using CSSPWebToolsDBDLL.Services;
using System.Security.Principal;
using CSSPWebToolsDBDLL;
using System.Xml;
using System.Data.OleDb;
using System.Transactions;
using System.Net;
using CSSPEnumsDLL.Enums;
using CSSPEnumsDLL.Services;
using System.Threading;
using TempData;
using System.Data.Entity.Spatial;

namespace ImportByFunction
{
    public partial class ImportByFunction : Form
    {
        #region Variables
        List<RegisterModel> UserList = new List<RegisterModel>();
        IPrincipal user;

        List<BackgroundWorker> bwList = new List<BackgroundWorker>();
        List<BWObj> bwObjList = new List<BWObj>();

        bool Cancel = false;

        #endregion Variables

        #region Properties
        BaseEnumService _BaseEnumService { get; set; }
        #endregion Properties

        #region Constructors
        public ImportByFunction()
        {
            InitializeComponent();
            try
            {
                user = new GenericPrincipal(new GenericIdentity("Charles.LeBlanc2@canada.ca", "Forms"), null);
            }
            catch (Exception)
            {
            }
            _BaseEnumService = new BaseEnumService(LanguageEnum.en);
        }
        #endregion Constructors

        #region Events

        private void butCreateRootAndUsersDBInfo_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateRootAndUsersDBInfo");
            ButEndFunction(CreateRootAndUsersDBInfo(), "CreateRootAndUsersDBInfo", (Button)sender);
        }
        private void butCreateCountryProvinceAndBasicDBInfo_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateCountryProvinceAndBasicDBInfo");
            ButEndFunction(CreateCountryProvinceAndBasicDBInfo(), "CreateCountryProvinceAndBasicDBInfo", (Button)sender);
        }
        private void butCancel_Click(object sender, EventArgs e)
        {
            if (Cancel)
            {
                Cancel = false;
                butCancel.Text = "Cancel";
            }
            else
            {
                Cancel = true;
                butCancel.Text = "Remove Cancel";
            }
        }
        private void butCleanDB_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CleanDB");
            ButEndFunction(CleanDB(), "CleanDB", (Button)sender);
        }
        private void butCreateClimateAll_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateClimate");
            List<string> justProvList = new List<string>() { "NB", "NFLD", "NS", "PEI", "BC", "QUE" };
            ButEndFunction(CreateClimate(justProvList), "CreateClimate", (Button)sender);
        }
        private void butCreateClimateBC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateClimate");
            List<string> justProvList = new List<string>() { "BC" };
            ButEndFunction(CreateClimate(justProvList), "CreateClimate", (Button)sender);
        }
        private void butCreateClimateNB_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateClimate");
            List<string> justProvList = new List<string>() { "NB" };
            ButEndFunction(CreateClimate(justProvList), "CreateClimate", (Button)sender);
        }
        private void butCreateClimateNL_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateClimate");
            List<string> justProvList = new List<string>() { "NFLD" };
            ButEndFunction(CreateClimate(justProvList), "CreateClimate", (Button)sender);
        }
        private void butCreateClimateNS_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateClimate");
            List<string> justProvList = new List<string>() { "NS" };
            ButEndFunction(CreateClimate(justProvList), "CreateClimate", (Button)sender);
        }
        private void butCreateClimatePE_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateClimate");
            List<string> justProvList = new List<string>() { "PEI" };
            ButEndFunction(CreateClimate(justProvList), "CreateClimate", (Button)sender);
        }
        private void butCreateClimateQC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateClimate");
            List<string> justProvList = new List<string>() { "QUE" };
            ButEndFunction(CreateClimate(justProvList), "CreateClimate", (Button)sender);
        }
        private void butCreateHydrometricAll_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateHydrometric");
            List<string> justProvList = new List<string>() { "NB", "NL", "NS", "PE", "BC", "QC" };
            ButEndFunction(CreateHydrometric(justProvList), "CreateHydrometric", (Button)sender);
        }
        private void butCreateHydrometricBC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateHydrometric");
            List<string> justProvList = new List<string>() { "BC" };
            ButEndFunction(CreateHydrometric(justProvList), "CreateHydrometric", (Button)sender);
        }
        private void butCreateHydrometricNB_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateHydrometric");
            List<string> justProvList = new List<string>() { "NB" };
            ButEndFunction(CreateHydrometric(justProvList), "CreateHydrometric", (Button)sender);
        }
        private void butCreateHydrometricNL_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateHydrometric");
            List<string> justProvList = new List<string>() { "NL" };
            ButEndFunction(CreateHydrometric(justProvList), "CreateHydrometric", (Button)sender);
        }
        private void butCreateHydrometricNS_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateHydrometric");
            List<string> justProvList = new List<string>() { "NS" };
            ButEndFunction(CreateHydrometric(justProvList), "CreateHydrometric", (Button)sender);
        }
        private void butCreateHydrometricPE_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateHydrometric");
            List<string> justProvList = new List<string>() { "PE" };
            ButEndFunction(CreateHydrometric(justProvList), "CreateHydrometric", (Button)sender);
        }
        private void butCreateHydrometricQC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateHydrometric");
            List<string> justProvList = new List<string>() { "QC" };
            ButEndFunction(CreateHydrometric(justProvList), "CreateHydrometric", (Button)sender);
        }
        private void butCreateMapInfoForLocation_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateMapInfoForLocation");
            ButEndFunction(CreateMapInfoForLocationAll(), "CreateMapInfoForLocation", (Button)sender);
        }
        private void butCreateMapInfoForLocationNB_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateMapInfoForLocation");
            ButEndFunction(CreateMapInfoForLocationNB(), "CreateMapInfoForLocation", (Button)sender);
        }
        private void butCreateMapInfoForLocationNL_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateMapInfoForLocation");
            ButEndFunction(CreateMapInfoForLocationNL(), "CreateMapInfoForLocation", (Button)sender);
        }
        private void butCreateMapInfoForLocationNS_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateMapInfoForLocation");
            ButEndFunction(CreateMapInfoForLocationNS(), "CreateMapInfoForLocation", (Button)sender);
        }
        private void butCreateMapInfoForLocationPE_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateMapInfoForLocation");
            ButEndFunction(CreateMapInfoForLocationPE(), "CreateMapInfoForLocation", (Button)sender);
        }
        private void butCreateMapInfoForLocationBC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateMapInfoForLocation");
            ButEndFunction(CreateMapInfoForLocationBC(), "CreateMapInfoForLocation", (Button)sender);
        }
        private void butCreateMapInfoForLocationQC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateMapInfoForLocation");
            ButEndFunction(CreateMapInfoForLocationQC(), "CreateMapInfoForLocation", (Button)sender);
        }
        private void butCreateSectorsAtl_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateSectorsAtl");
            ButEndFunction(CreateSectorsAtl(), "CreateSectorsAtl", (Button)sender);
        }
        private void butCreateSectorsBC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateSectorsBC");
            ButEndFunction(CreateSectorsBC(), "CreateSectorsBC", (Button)sender);
        }
        private void butCreateSectorsMapInfoAtl_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateSectorsMapInfoAtl");
            ButEndFunction(CreateSectorsMapInfoAtl(), "CreateSectorsMapInfoAtl", (Button)sender);
        }
        private void butCreateSectorsQC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "CreateSectorsQC");
            ButEndFunction(CreateSectorsQC(), "CreateSectorsQC", (Button)sender);
        }
        private void butLoadPrecipAtl_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadPrecipAtl");
            List<string> justProvList = new List<string>() { "NB", "NL", "NS", "PE" };
            ButEndFunction(CreatePrecipitationsAtl(justProvList), "LoadPrecipAtl", (Button)sender);
        }
        private void butLoadPrecipNB_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadPrecipNB");
            List<string> justProvList = new List<string>() { "NB" };
            ButEndFunction(CreatePrecipitationsAtl(justProvList), "LoadPrecipNB", (Button)sender);
        }
        private void butLoadPrecipNL_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadPrecipNL");
            List<string> justProvList = new List<string>() { "NL" };
            ButEndFunction(CreatePrecipitationsAtl(justProvList), "LoadPrecipNL", (Button)sender);
        }
        private void butLoadPrecipNS_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadPrecipNS");
            List<string> justProvList = new List<string>() { "NS" };
            ButEndFunction(CreatePrecipitationsAtl(justProvList), "LoadPrecipNS", (Button)sender);
        }
        private void butLoadPrecipPE_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadPrecipPE");
            List<string> justProvList = new List<string>() { "PE" };
            ButEndFunction(CreatePrecipitationsAtl(justProvList), "LoadPrecipPE", (Button)sender);
        }
        private void butLoadPrecipBC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadPrecipBC");
            ButEndFunction(CreatePrecipitationsBC(), "LoadPrecipBC", (Button)sender);
        }
        private void butLoadPrecipQC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadPrecipQC");
            ButEndFunction(LoadPrecipitationsQC(), "LoadPrecipQC", (Button)sender);
        }
        private void butLoadRunsAtl_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadRunsAtl");
            ButEndFunction(LoadRunsAtl(), "LoadRunsAtl", (Button)sender);
        }
        private void butLoadRunsNB_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadRunsNB");
            ButEndFunction(LoadRunsNB(), "LoadRunsNB", (Button)sender);
        }
        private void butLoadRunsNBMOU_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadRunsNBMOU");
            ButEndFunction(LoadRunsNBMOU(), "LoadRunsNBMOU", (Button)sender);
        }
        private void butLoadRunsNL_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadRunsNL");
            ButEndFunction(LoadRunsNL(), "LoadRunsNL", (Button)sender);
        }
        private void butLoadRunsNS_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadRunsNS");
            ButEndFunction(LoadRunsNS(), "LoadRunsNS", (Button)sender);
        }
        private void butLoadRunsPE_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadRunsPE");
            ButEndFunction(LoadRunsPE(), "LoadRunsPE", (Button)sender);
        }
        private void butLoadRunsBC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadRunsBC");
            ButEndFunction(CreateRunsBC(), "LoadRunsBC", (Button)sender);
        }
        private void butLoadRunsQC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadRunsQC");
            ButEndFunction(CreateRunsQC(), "LoadRunsQC", (Button)sender);
        }
        private void butLoadSamplesAtl_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSamplesAtl");
            ButEndFunction(LoadSamplesAtl(), "LoadSamplesAtl", (Button)sender);
        }
        private void butLoadSamplesNB_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSamplesNB");
            ButEndFunction(LoadSamplesNB(), "LoadSamplesNB", (Button)sender);
        }
        private void butLoadSamplesNBMOU_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSamplesNBMOU");
            ButEndFunction(LoadSamplesNBMOU(), "LoadSamplesNBMOU", (Button)sender);
        }
        private void butLoadSamplesNL_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSamplesNL");
            ButEndFunction(LoadSamplesNL(), "LoadSamplesNL", (Button)sender);
        }
        private void butLoadSamplesNS_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSamplesNS");
            ButEndFunction(LoadSamplesNS(), "LoadSamplesNS", (Button)sender);
        }
        private void butLoadSamplesPE_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSamplesPE");
            ButEndFunction(LoadSamplesPE(), "LoadSamplesPE", (Button)sender);
        }
        private void butLoadSamplesBC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSamplesBC");
            ButEndFunction(CreateSamplesBC(), "LoadSamplesBC", (Button)sender);
        }
        private void butLoadSamplesQC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSamplesQC");
            ButEndFunction(CreateSamplesQC(), "LoadSamplesQC", (Button)sender);

        }
        private void butLoadSanitaryAtl_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSanitaryAtl");
            ButEndFunction(LoadSanitaryAllAtl(), "LoadSanitaryAtl", (Button)sender);
        }
        private void butLoadSanitationNB_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSanitaryNB");
            ButEndFunction(LoadSanitaryNB(), "LoadSanitaryNB", (Button)sender);
        }
        private void butLoadSanitationNL_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSanitaryNL");
            ButEndFunction(LoadSanitaryNL(), "LoadSanitaryNL", (Button)sender);
        }
        private void butLoadSanitationNS_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSanitaryNS");
            ButEndFunction(LoadSanitaryNS(), "LoadSanitaryNS", (Button)sender);
        }
        private void butLoadSanitationPE_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSanitaryPE");
            ButEndFunction(LoadSanitaryPE(), "LoadSanitaryPE", (Button)sender);
        }
        private void butLoadSanitationBC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSanitaryBC");
            ButEndFunction(CreateSanitaryBC(), "LoadSanitaryBC", (Button)sender);
        }
        private void butLoadSanitationQC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadSanitaryQC");
            ButEndFunction(CreateSanitaryQC(), "LoadSanitaryQC", (Button)sender);
        }
        private void butLoadStationAtl_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadStationAtl");
            List<string> justProvList = new List<string>() { "NB", "NL", "NS", "PE" };
            ButEndFunction(CreateStationsAtl(justProvList), "LoadStationAtl", (Button)sender);
        }
        private void butLoadStationNB_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadStationNB");
            List<string> justProvList = new List<string>() { "NB" };
            ButEndFunction(CreateStationsAtl(justProvList), "LoadStationNB", (Button)sender);
        }
        private void butLoadStationNL_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadStationNL");
            List<string> justProvList = new List<string>() { "NL" };
            ButEndFunction(CreateStationsAtl(justProvList), "LoadStationNL", (Button)sender);
        }
        private void butLoadStationNS_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadStationNS");
            List<string> justProvList = new List<string>() { "NS" };
            ButEndFunction(CreateStationsAtl(justProvList), "LoadStationNS", (Button)sender);
        }
        private void butLoadStationPE_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadStationPE");
            List<string> justProvList = new List<string>() { "PE" };
            ButEndFunction(CreateStationsAtl(justProvList), "LoadStationPE", (Button)sender);
        }
        private void butLoadStationBC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadStationBC");
            ButEndFunction(CreateStationsBC(), "LoadStationBC", (Button)sender);
        }
        private void butLoadStationQC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadStationQC");
            ButEndFunction(CreateStationsQC(), "LoadStationQC", (Button)sender);
        }
        private void butLoadTVItemsAll_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadTVItemsAtl");
            ButEndFunction(LoadTVItemsAll(), "LoadTVItemsAtl", (Button)sender);
        }
        private void butLoadTVItemsME_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadTVItemsME");
            ButEndFunction(LoadTVItemsME(), "LoadTVItemsME", (Button)sender);
        }
        private void butLoadTVItemsNB_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadTVItemsNB");
            ButEndFunction(LoadTVItemsNB(), "LoadTVItemsNB", (Button)sender);
        }
        private void butLoadTVItemsNL_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadTVItemsNL");
            ButEndFunction(LoadTVItemsNL(), "LoadTVItemsNL", (Button)sender);
        }
        private void butLoadTVItemsNS_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadTVItemsNS");
            ButEndFunction(LoadTVItemsNS(), "LoadTVItemsNS", (Button)sender);
        }
        private void butLoadTVItemsPE_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadTVItemsPE");
            ButEndFunction(LoadTVItemsPE(), "LoadTVItemsPE", (Button)sender);
        }
        private void butLoadTVItemsBC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadTVItemsBC");
            ButEndFunction(LoadTVItemsBC(), "LoadTVItemsBC", (Button)sender);
        }
        private void butLoadTVItemsQC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "LoadTVItemsQC");
            ButEndFunction(LoadTVItemsQC(), "LoadTVItemsQC", (Button)sender);
        }
        private void butNomenclatureAtl_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "NomenclatureAtl");
            ButEndFunction(NomenclatureATL(), "NomenclatureAtl", (Button)sender);
        }
        private void butUseOfClimateSiteAtl_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "UseOfClimateSiteAtl");
            List<string> justProvList = new List<string>() { "NB", "NL", "NS", "PE" };
            ButEndFunction(CreateUseOfClimateSitesAtl(justProvList), "UseOfClimateSiteAtl", (Button)sender);
        }
        private void butUseOfClimateSiteNB_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "UseOfClimateSiteNB");
            List<string> justProvList = new List<string>() { "NB" };
            ButEndFunction(CreateUseOfClimateSitesAtl(justProvList), "UseOfClimateSiteNB", (Button)sender);
        }
        private void butbutUseOfClimateSiteNB_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "UseOfClimateSiteNL");
            List<string> justProvList = new List<string>() { "NL" };
            ButEndFunction(CreateUseOfClimateSitesAtl(justProvList), "UseOfClimateSiteNL", (Button)sender);
        }
        private void butUseOfClimateSiteNS_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "UseOfClimateSiteNS");
            List<string> justProvList = new List<string>() { "NS" };
            ButEndFunction(CreateUseOfClimateSitesAtl(justProvList), "UseOfClimateSiteNS", (Button)sender);
        }
        private void butUseOfClimateSitePE_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "UseOfClimateSitePE");
            List<string> justProvList = new List<string>() { "PE" };
            ButEndFunction(CreateUseOfClimateSitesAtl(justProvList), "UseOfClimateSitePE", (Button)sender);
        }
        private void butUseOfClimateSiteBC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "UseOfClimateSiteBC");
            ButEndFunction(CreateUseOfClimateSitesBC(), "UseOfClimateSiteBC", (Button)sender);
        }
        private void butUseOfClimateSiteQC_Click(object sender, EventArgs e)
        {
            ButClickSetup((Button)sender, "UseOfClimateSiteQC");
            ButEndFunction(CreateUseOfClimateSitesQC(), "UseOfClimateSiteQC", (Button)sender);
        }

        #endregion Events

        #region Function private
        private bool Check_RetStrOK(string retStr)
        {
            if (!string.IsNullOrWhiteSpace(retStr))
            {
                richTextBoxStatus.AppendText(retStr + "\r\n");
                return false;
            }
            return true;
        }
        private bool CheckModelOK<T>(T model) where T : LastUpdateAndContactModel
        {
            if (!string.IsNullOrWhiteSpace(model.Error))
            {
                richTextBoxStatus.AppendText(model.Error + "\r\n");
                return false;
            }
            return true;
        }
        private bool CreateRootAndUsersDBInfo()
        {
            if (!CSSPWebToolsDBisOK()) return false;
            if (!CreateRootTVItem()) return false;
            if (!CreateFirstNewUser()) return false;
            if (!CreateNewUsers()) return false;

            return true;
        }
        private bool CreateCountryProvinceAndBasicDBInfo()
        {
            if (!CreateCountryTVItem()) return false;
            if (!CreateTVItemJustRootAndProvinces()) return false;
            if (!CreateWQMLookupMPNs()) return false;
            if (!CreateMapInfoFromLocationNameLatLongXLSJustProv()) return false;

            return true;
        }
        private void ButClickSetup(Button butTemp, string FuncName)
        {
            richTextBoxStatus.Text = "";
            lblStatus.Text = FuncName + " started ...";
            lblStatus2.Text = "";
            butTemp.BackColor = Color.Orange;
            Application.DoEvents();
        }
        private void ButEndFunction(bool FuncRet, string FuncName, Button butTemp)
        {
            lblStatus.Text = FuncName + " done ...";
            if (FuncRet)
            {
                butTemp.BackColor = Color.Green;
            }
            else
            {
                butTemp.BackColor = Color.Red;
            }
            Application.DoEvents();
        }
        private bool CreateMapInfoForLocationAll()
        {
            List<string> ShortProv = new List<string>() { "NB", "NS", "NL", "PE", "BC", "QC" };
            List<string> Prov = new List<string>() { "New Brunswick", "Nova Scotia", "Newfoundland and Labrador", "Prince Edward Island", "British Columbia", "Québec" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateMapInfoFromLocationNameLatLongXLSAll(Prov[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool CreateMapInfoForLocationNB()
        {
            List<string> ShortProv = new List<string>() { "NB" };
            List<string> Prov = new List<string>() { "New Brunswick" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateMapInfoFromLocationNameLatLongXLSAll(Prov[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool CreateMapInfoForLocationNL()
        {
            List<string> ShortProv = new List<string>() { "NL" };
            List<string> Prov = new List<string>() { "Newfoundland and Labrador" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateMapInfoFromLocationNameLatLongXLSAll(Prov[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool CreateMapInfoForLocationNS()
        {
            List<string> ShortProv = new List<string>() { "NS" };
            List<string> Prov = new List<string>() { "Nova Scotia" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateMapInfoFromLocationNameLatLongXLSAll(Prov[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool CreateMapInfoForLocationPE()
        {
            List<string> ShortProv = new List<string>() { "PE" };
            List<string> Prov = new List<string>() { "Prince Edward Island" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateMapInfoFromLocationNameLatLongXLSAll(Prov[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool CreateMapInfoForLocationBC()
        {
            List<string> ShortProv = new List<string>() { "BC" };
            List<string> Prov = new List<string>() { "British Columbia" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateMapInfoFromLocationNameLatLongXLSAll(Prov[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool CreateMapInfoForLocationQC()
        {
            List<string> ShortProv = new List<string>() { "QC" };
            List<string> Prov = new List<string>() { "Québec" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateMapInfoFromLocationNameLatLongXLSAll(Prov[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool CreateSectorsAtl()
        {
            if (!CreateWQMTVSubSectorsAll())
            {
                return false;
            }

            return true;
        }
        private bool CreateSectorsMapInfoAtl()
        {
            if (!CreateMapInfosFromAreas_Sectors_SubsectorsKML())
            {
                return false;
            }
            return true;
        }
        private bool CreateSectorsBC()
        {
            if (!CreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML())
            {
                return false;
            }

            if (!CreateBCSubSectorInfo())
            {
                return false;
            }

            return true;
        }
        private bool CreateSectorsQC()
        {
            if (!CreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML())
            {
                return false;
            }

            if (!CreateQCSubSectorInfo())
            {
                return false;
            }

            return true;
        }
        private bool CreateBCSubSectorInfo()
        {
            lblStatus.Text = "Starting ... CreateBCSubSectorInfo";
            Application.DoEvents();

            if (Cancel) return false;

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMSubsectorService mwqmSubsectorService = new MWQMSubsectorService(LanguageEnum.en, user);
            MWQMSubsectorLanguageService mwqmSubsectorLanguageService = new MWQMSubsectorLanguageService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelBC = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "British Columbia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelBC)) return false;

            List<TVItemModel> tvItemModelSubSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelBC.TVItemID, TVTypeEnum.Subsector);

            int CountTotal = tvItemModelSubSectorList.Count();
            int Count = 0;
            foreach (TVItemModel tvItemModel in tvItemModelSubSectorList)
            {
                Count += 1;
                lblStatus.Text = (Count * 100 / CountTotal).ToString() + " ... CreateBCSubSectorInfo";
                Application.DoEvents();

                MWQMSubsectorModel mwqmSubsectorModel = mwqmSubsectorService.GetMWQMSubsectorModelWithMWQMSubsectorTVItemIDDB(tvItemModel.TVItemID);
                if (!string.IsNullOrWhiteSpace(mwqmSubsectorModel.Error))
                {
                    MWQMSubsectorModel mwqmSubsectorModelNew = new MWQMSubsectorModel()
                    {
                        MWQMSubsectorTVItemID = tvItemModel.TVItemID,
                        SubsectorHistoricKey = tvItemModel.TVText.Substring(0, tvItemModel.TVText.IndexOf(" ")).Trim(),
                        SubsectorDesc = "(empty)",
                    };

                    MWQMSubsectorModel mwqmSubsectorModelRet = mwqmSubsectorService.PostAddMWQMSubsectorDB(mwqmSubsectorModelNew);
                    if (!CheckModelOK<MWQMSubsectorModel>(mwqmSubsectorModelRet)) return false;

                    string TVText = "--";

                    MWQMSubsectorLanguageModel mwqmSubsectorLanguageModelRet = mwqmSubsectorLanguageService.GetMWQMSubsectorLanguageModelWithMWQMSubsectorIDAndLanguageDB(mwqmSubsectorModelRet.MWQMSubsectorID, LanguageEnum.fr);
                    if (!CheckModelOK<MWQMSubsectorLanguageModel>(mwqmSubsectorLanguageModelRet)) return false;

                    mwqmSubsectorLanguageModelRet.SubsectorDesc = TVText;

                    mwqmSubsectorLanguageModelRet = mwqmSubsectorLanguageService.PostUpdateMWQMSubsectorLanguageDB(mwqmSubsectorLanguageModelRet);
                    if (!CheckModelOK<MWQMSubsectorLanguageModel>(mwqmSubsectorLanguageModelRet)) return false;

                }
            }

            return true;
        }
        private bool CreateQCSubSectorInfo()
        {
            lblStatus.Text = "Starting ... CreateQCSubSectorInfo";
            Application.DoEvents();

            if (Cancel) return false;

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMSubsectorService mwqmSubsectorService = new MWQMSubsectorService(LanguageEnum.en, user);
            MWQMSubsectorLanguageService mwqmSubsectorLanguageService = new MWQMSubsectorLanguageService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return false;

            TVItemModel tvItemModelQC = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelQC)) return false;

            List<TVItemModel> tvItemModelSubSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);

            int TotalCount = tvItemModelSubSectorList.Count();
            int Count = 0;
            foreach (TVItemModel tvItemModel in tvItemModelSubSectorList)
            {
                Count += 1;
                lblStatus.Text = " ... CreateQCSubSectorInfo - subsector " + tvItemModel.TVText;
                Application.DoEvents();

                string subsectorName = tvItemModel.TVText.Trim();


                MWQMSubsectorModel mwqmSubsectorModel = mwqmSubsectorService.GetMWQMSubsectorModelWithMWQMSubsectorTVItemIDDB(tvItemModel.TVItemID);
                if (!string.IsNullOrWhiteSpace(mwqmSubsectorModel.Error))
                {
                    MWQMSubsectorModel mwqmSubsectorModelNew = new MWQMSubsectorModel()
                    {
                        MWQMSubsectorTVItemID = tvItemModel.TVItemID,
                        SubsectorHistoricKey = tvItemModel.TVText.Substring(0, tvItemModel.TVText.IndexOf(" ")).Trim(),
                        SubsectorDesc = "(empty)",
                    };

                    MWQMSubsectorModel mwqmSubsectorModelRet = mwqmSubsectorService.PostAddMWQMSubsectorDB(mwqmSubsectorModelNew);
                    if (!CheckModelOK<MWQMSubsectorModel>(mwqmSubsectorModelRet)) return false;

                    string TVText = "--";

                    MWQMSubsectorLanguageModel mwqmSubsectorLanguageModelRet = mwqmSubsectorLanguageService.GetMWQMSubsectorLanguageModelWithMWQMSubsectorIDAndLanguageDB(mwqmSubsectorModelRet.MWQMSubsectorID, LanguageEnum.fr);
                    if (!CheckModelOK<MWQMSubsectorLanguageModel>(mwqmSubsectorLanguageModelRet)) return false;

                    mwqmSubsectorLanguageModelRet.SubsectorDesc = TVText;

                    mwqmSubsectorLanguageModelRet = mwqmSubsectorLanguageService.PostUpdateMWQMSubsectorLanguageDB(mwqmSubsectorLanguageModelRet);
                    if (!CheckModelOK<MWQMSubsectorLanguageModel>(mwqmSubsectorLanguageModelRet)) return false;
                }
            }
            return true;
        }
        private List<List<Vect>> GetFinalPoly(List<SubSector> SubsectorList, string s)
        {
            List<List<Vect>> AllVectList = new List<List<Vect>>();

            List<Vect> VectList = new List<Vect>();

            foreach (SubSector ss in SubsectorList)
            {
                if (ss.SSName.StartsWith(s))
                {
                    for (var i = 0; i < ss.Polygon.Count - 1; i++)
                    {
                        Vect FV = new Vect()
                        {
                            Start = ss.Polygon[i],
                            End = ss.Polygon[i + 1],
                        };
                        Vect BV = new Vect()
                        {
                            Start = ss.Polygon[i + 1],
                            End = ss.Polygon[i],
                        };
                        bool exist = false;
                        foreach (Vect v in VectList)
                        {
                            if (((v.Start.Lat - 0.0001) < FV.Start.Lat) && ((v.Start.Lat + 0.0001) > FV.Start.Lat)
                                && ((v.Start.Lng - 0.0001) < FV.Start.Lng) && ((v.Start.Lng + 0.0001) > FV.Start.Lng)
                                && ((v.End.Lat - 0.0001) < FV.End.Lat) && ((v.End.Lat + 0.0001) > FV.End.Lat)
                                && ((v.End.Lng - 0.0001) < FV.End.Lng) && ((v.End.Lng + 0.0001) > FV.End.Lng))
                            {
                                VectList.Remove(v);
                                exist = true;
                                break;
                            }
                            if (((v.Start.Lat - 0.0001) < BV.Start.Lat) && ((v.Start.Lat + 0.0001) > BV.Start.Lat)
                                && ((v.Start.Lng - 0.0001) < BV.Start.Lng) && ((v.Start.Lng + 0.0001) > BV.Start.Lng)
                                && ((v.End.Lat - 0.0001) < BV.End.Lat) && ((v.End.Lat + 0.0001) > BV.End.Lat)
                                && ((v.End.Lng - 0.0001) < BV.End.Lng) && ((v.End.Lng + 0.0001) > BV.End.Lng))
                            {
                                VectList.Remove(v);
                                exist = true;
                                break;
                            }
                        }
                        if (!exist)
                        {
                            VectList.Add(FV);
                        }
                    }
                }
            }

            List<Vect> FinalPoly = new List<Vect>();
            if (VectList.Count > 0)
            {
                FinalPoly.Add(VectList[0]);
                VectList.Remove(VectList[0]);
                while (VectList.Count > 0)
                {
                    int CountVectList = VectList.Count;
                    for (var i = 0; i < VectList.Count; i++)
                    {
                        if (((FinalPoly[FinalPoly.Count - 1].End.Lat - 0.0001) < VectList[i].Start.Lat)
                            && ((FinalPoly[FinalPoly.Count - 1].End.Lat + 0.0001) > VectList[i].Start.Lat)
                            && ((FinalPoly[FinalPoly.Count - 1].End.Lng - 0.0001) < VectList[i].Start.Lng)
                            && ((FinalPoly[FinalPoly.Count - 1].End.Lng + 0.0001) > VectList[i].Start.Lng))
                        {
                            Vect v = new Vect()
                            {
                                Start = VectList[i].Start,
                                End = VectList[i].End
                            };
                            FinalPoly.Add(v);
                            VectList.Remove(VectList[i]);
                            break;
                        }
                        if (((FinalPoly[FinalPoly.Count - 1].End.Lat - 0.0001) < VectList[i].End.Lat)
                            && ((FinalPoly[FinalPoly.Count - 1].End.Lat + 0.0001) > VectList[i].End.Lat)
                            && ((FinalPoly[FinalPoly.Count - 1].End.Lng - 0.0001) < VectList[i].End.Lng)
                            && ((FinalPoly[FinalPoly.Count - 1].End.Lng + 0.0001) > VectList[i].End.Lng))
                        {
                            Vect v = new Vect()
                            {
                                Start = VectList[i].End,
                                End = VectList[i].Start
                            };
                            FinalPoly.Add(v);
                            VectList.Remove(VectList[i]);
                            break;
                        }
                    }
                    if (CountVectList == VectList.Count)
                    {
                        //richTextBoxStatus.AppendText(s + "\r\n");
                        AllVectList.Add(FinalPoly);
                        FinalPoly = new List<Vect>();
                        FinalPoly.Add(VectList[0]);
                        VectList.Remove(VectList[0]);
                    }
                    if (VectList.Count == 0)
                    {
                        if (FinalPoly.Count > 2)
                        {
                            AllVectList.Add(FinalPoly);
                        }
                        break;
                    }
                }
            }
            return AllVectList;
        }
        private int GetID(string Path)
        {
            int RetVal = -1; // will return -1 if an error occured or if there are no parent i.e. tvItem is root
            if (!string.IsNullOrWhiteSpace(Path))
            {
                if (Path.Contains(@"p"))
                {
                    RetVal = int.Parse(Path.Substring(Path.LastIndexOf(@"p") + 1));
                }
                else
                {
                    RetVal = int.Parse(Path);
                }
            }

            return RetVal;
        }
        private int GetLevel(string Path)
        {
            int RetVal = -1; // will return -1 if an error occured or if there are no parent i.e. tvItem is root
            if (!string.IsNullOrWhiteSpace(Path))
            {
                if (Path.Contains(@"p"))
                {
                    RetVal = (from c in Path
                              where c == @"p".ToCharArray()[0]
                              select c).Count();
                    RetVal -= 1;
                }
                else
                {
                    RetVal = 0;
                }
            }

            return RetVal;
        }
        private int GetParentID(string Path)
        {
            int RetVal = -1; // will return -1 if an error occured or if there are no parent i.e. tvItem is root
            if (!string.IsNullOrWhiteSpace(Path))
            {
                if (Path.Contains(@"p"))
                {
                    string ChildPath = Path.Substring(0, Path.LastIndexOf(@"p"));
                    if (ChildPath.Contains(@"p"))
                    {
                        RetVal = int.Parse(ChildPath.Substring(ChildPath.LastIndexOf(@"p") + 1));
                    }
                    else
                    {
                        if (ChildPath == "")
                        {
                            return 0;
                        }
                        RetVal = int.Parse(ChildPath);
                    }
                }
                else
                {
                    RetVal = int.Parse(Path);
                }
            }

            return RetVal;
        }
        private int GetParentLevel(string Path)
        {
            int RetVal = -1; // will return -1 if an error occured or if there are no parent i.e. tvItem is root
            if (!string.IsNullOrWhiteSpace(Path))
            {
                if (Path.Contains(@"p"))
                {
                    int count = (from c in Path
                                 where c == @"p".ToCharArray()[0]
                                 select c).Count();
                    RetVal = count - 1;
                }
            }

            return RetVal;
        }
        private string GetParentPath(string Path)
        {
            string RetVal = ""; // will return "" if an error occured or if there are no parent i.e. tvItem is root
            if (!string.IsNullOrWhiteSpace(Path))
            {
                if (Path.Contains(@"p"))
                {
                    RetVal = Path.Substring(0, Path.LastIndexOf(@"p"));
                }
                else
                {
                    RetVal = "";
                }
            }

            return RetVal;
        }
        private PolSourceObsInfoEnum GetPolSourceRisk(string CODE, string SubCODE, string Prov)
        {
            PolSourceObsInfoEnum polSourceObsInfo = new PolSourceObsInfoEnum();

            if (string.IsNullOrEmpty(CODE))
            {
                CODE = "MOD";
            }
            else if (CODE.StartsWith("-"))
            {
                CODE = "MOD";
            }
            else if (CODE.Substring(0, 3).ToUpper() == "HIG")
            {
                CODE = "HIGH";
            }
            else if (CODE.Substring(0, 3).ToUpper() == "LOW")
            {
                CODE = "LOW";
            }
            else if (CODE.Substring(0, 3).ToUpper() == "MOD")
            {
                CODE = "MOD";
            }

            polSourceObsInfo = PolSourceObsInfoEnum.RiskModerate;

            if (Prov == "AT")
            {
                switch (CODE)
                {
                    case "HIGH":
                        {
                            polSourceObsInfo = PolSourceObsInfoEnum.RiskHigh;
                        }
                        break;
                    case "LOW":
                        {
                            polSourceObsInfo = PolSourceObsInfoEnum.RiskLow;
                        }
                        break;
                }
            }
            else if (Prov == "BC")
            {
            }
            else if (Prov == "QC")
            {
            }
            else
            {
            }
            return polSourceObsInfo;
        }
        private PolSourceObsInfoEnum GetPolSourceStatus(string CODE, string SubCODE, string Prov)
        {
            PolSourceObsInfoEnum polSourceObsInfo = new PolSourceObsInfoEnum();

            polSourceObsInfo = PolSourceObsInfoEnum.StatusPotential;

            if (Prov == "AT")
            {
                switch (CODE)
                {
                    case "D":
                        {
                            polSourceObsInfo = PolSourceObsInfoEnum.StatusDefinite;
                        }
                        break;
                    case "N":
                        {
                            polSourceObsInfo = PolSourceObsInfoEnum.StatusNonPollutionSource;
                        }
                        break;
                }
            }
            else if (Prov == "BC")
            {
            }
            else if (Prov == "QC")
            {
            }
            else
            {
            }
            return polSourceObsInfo;
        }
        private List<PolSourceObsInfoEnum> GetPolSourceType(string CODE, string SubCODE, string Prov)
        {
            List<PolSourceObsInfoEnum> polSourceObsInfoList = new List<PolSourceObsInfoEnum>();

            if (Prov == "AT")
            {
                //switch (CODE.ToUpper())
                //{
                //case "AB":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Error,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "AG":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Agriculture,
                //            PolSourceObsInfoEnum.AgricultureFeedlotHobbyFarmPasture,
                //        };
                //    }
                //    break;
                //case "AQ":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Industry,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "BL":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Sewage,
                //            PolSourceObsInfoEnum.SewageHomes,
                //        };
                //    }
                //    break;
                //case "CP":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Sewage,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "CS":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Urban,
                //            PolSourceObsInfoEnum.UrbanSewageTreatmentPlant,
                //        };
                //    }
                //    break;
                //case "CU":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.SurfaceRunoff,
                //            PolSourceObsInfoEnum.SurfaceRunoffVariousDischarges,
                //        };
                //    }
                //    break;
                //case "DD":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.SurfaceRunoff,
                //            PolSourceObsInfoEnum.SurfaceRunoffVariousDischarges,
                //        };
                //    }
                //    break;
                //case "DR":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.MarineFacilities,
                //            PolSourceObsInfoEnum.MarineFacilitiesAnchoragesMarineParksWharves,
                //        };
                //    }
                //    break;
                //case "DS":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Industry,
                //            PolSourceObsInfoEnum.IndustryDump,
                //        };
                //    }
                //    break;
                //case "FP":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Industry,
                //            PolSourceObsInfoEnum.IndustryFishProcessing,
                //        };
                //    }
                //    break;
                //case "GC":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.RecreationActivities,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "GD":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Error,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "IN":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Industry,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "LE":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Industry,
                //            PolSourceObsInfoEnum.IndustryDump,
                //        };
                //    }
                //    break;
                //case "LS":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Urban,
                //            PolSourceObsInfoEnum.UrbanLiftStation,
                //        };
                //    }
                //    break;
                //case "MR":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.MarineFacilities,
                //            PolSourceObsInfoEnum.MarineFacilitiesMarina,
                //        };
                //    }
                //    break;
                //case "NP":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Industry,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "NS":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Error,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "OF":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Urban,
                //            PolSourceObsInfoEnum.UrbanOutfall,
                //        };
                //    }
                //    break;
                //case "OH":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Sewage,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "PI":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Sewage,
                //            PolSourceObsInfoEnum.SewagePitPrivyStraightPipeSepticTankField,
                //        };
                //    }
                //    break;
                //case "SI":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Forestry,
                //            PolSourceObsInfoEnum.ForestryLoggingActivity,
                //        };
                //    }
                //    break;
                //case "SS":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Urban,
                //            PolSourceObsInfoEnum.UrbanStormWater,
                //        };
                //    }
                //    break;
                //case "ST":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Sewage,
                //            PolSourceObsInfoEnum.SewagePitPrivyStraightPipeSepticTankField,
                //        };
                //    }
                //    break;
                //case "TP":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Urban,
                //            PolSourceObsInfoEnum.UrbanSewageTreatmentPlant,
                //        };
                //    }
                //    break;
                //case "WC":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.SurfaceRunoff,
                //            PolSourceObsInfoEnum.SurfaceRunoffVariousDischarges,
                //        };
                //    }
                //    break;
                //case "WL":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Wildlife,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "WS":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Error,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "WV":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.MarineFacilities,
                //            PolSourceObsInfoEnum.MarineFacilitiesAnchoragesMarineParksWharves,
                //        };
                //    }
                //    break;
                //case "XX":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Error,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //default:
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Error,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //}
            }
            else if (Prov == "BC")
            {
                //switch (CODE.ToUpper())
                //{
                //case "AG":
                //    {
                //        switch (SubCODE)
                //        {
                //            case "CR":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Agriculture,
                //                        PolSourceObsInfoEnum.AgricultureCrop,
                //                    };
                //                }
                //                break;
                //            case "FM":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Agriculture,
                //                        PolSourceObsInfoEnum.AgricultureFeedlotHobbyFarmPasture,
                //                    };
                //                }
                //                break;
                //            default:
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Agriculture,
                //                        PolSourceObsInfoEnum.Error,
                //                    };
                //                }
                //                break;
                //        }
                //    }
                //    break;
                //case "FO":
                //    {
                //        switch (SubCODE)
                //        {
                //            case "LA":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Forestry,
                //                        PolSourceObsInfoEnum.ForestryLoggingActivity,
                //                    };
                //                }
                //                break;
                //            case "LC":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Forestry,
                //                        PolSourceObsInfoEnum.ForestryLogCamp,
                //                    };
                //                }
                //                break;
                //            default:
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Forestry,
                //                        PolSourceObsInfoEnum.Error,
                //                    };
                //                }
                //                break;
                //        }
                //    }
                //    break;
                //case "IN":
                //    {
                //        switch (SubCODE)
                //        {
                //            case "FP":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Industry,
                //                        PolSourceObsInfoEnum.IndustryFishProcessing,
                //                    };
                //                }
                //                break;
                //            case "HA":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Industry,
                //                        PolSourceObsInfoEnum.IndustryFishHatchery,
                //                    };
                //                }
                //                break;
                //            case "OI":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Industry,
                //                        PolSourceObsInfoEnum.Error,
                //                    };
                //                }
                //                break;
                //            case "SV":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Industry,
                //                        PolSourceObsInfoEnum.Error,
                //                    };
                //                }
                //                break;
                //            default:
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Industry,
                //                        PolSourceObsInfoEnum.Error,
                //                    };
                //                }
                //                break;
                //        }
                //    }
                //    break;
                //case "MF":
                //    {
                //        switch (SubCODE)
                //        {
                //            case "AN":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.MarineFacilities,
                //                        PolSourceObsInfoEnum.MarineFacilitiesAnchoragesMarineParksWharves,
                //                    };
                //                }
                //                break;
                //            case "FH":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.MarineFacilities,
                //                        PolSourceObsInfoEnum.MarineFacilitiesFloathome,
                //                    };
                //                }
                //                break;
                //            case "FR":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.MarineFacilities,
                //                        PolSourceObsInfoEnum.MarineFacilitiesFishingCampsResorts,
                //                    };
                //                }
                //                break;
                //            default:
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.MarineFacilities,
                //                        PolSourceObsInfoEnum.Error,
                //                    };
                //                }
                //                break;
                //        }
                //    }
                //    break;
                //case "RE":
                //    {
                //        switch (SubCODE)
                //        {
                //            case "CA":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.RecreationActivities,
                //                        PolSourceObsInfoEnum.RecreationActivitiesCampground,
                //                    };
                //                }
                //                break;
                //            default:
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.RecreationActivities,
                //                        PolSourceObsInfoEnum.Error,
                //                    };
                //                }
                //                break;
                //        }
                //    }
                //    break;
                //case "SE":
                //    {
                //        switch (SubCODE)
                //        {
                //            case "SW":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Sewage,
                //                        PolSourceObsInfoEnum.SewagePitPrivyStraightPipeSepticTankField,
                //                    };
                //                }
                //                break;
                //            default:
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Sewage,
                //                        PolSourceObsInfoEnum.Error,
                //                    };
                //                }
                //                break;
                //        }
                //    }
                //    break;
                //case "UK":
                //    {
                //        switch (SubCODE)
                //        {
                //            case "UK":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Error,
                //                        PolSourceObsInfoEnum.Error,
                //                    };
                //                }
                //                break;
                //            default:
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Error,
                //                        PolSourceObsInfoEnum.Error,
                //                    };
                //                }
                //                break;
                //        }
                //    }
                //    break;
                //case "UR":
                //    {
                //        switch (SubCODE)
                //        {
                //            case "MU":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Urban,
                //                        PolSourceObsInfoEnum.UrbanMultipleSources,
                //                    };
                //                }
                //                break;
                //            case "UP":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Urban,
                //                        PolSourceObsInfoEnum.UrbanOutfall,
                //                    };
                //                }
                //                break;
                //            default:
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Urban,
                //                        PolSourceObsInfoEnum.Error,
                //                    };
                //                }
                //                break;
                //        }
                //    }
                //    break;
                //case "WI":
                //    {
                //        switch (SubCODE)
                //        {
                //            case "BI":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Wildlife,
                //                        PolSourceObsInfoEnum.WildlifeBirds,
                //                    };
                //                }
                //                break;
                //            case "LM":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Wildlife,
                //                        PolSourceObsInfoEnum.WildlifeLandMammals,
                //                    };
                //                }
                //                break;
                //            case "MM":
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Wildlife,
                //                        PolSourceObsInfoEnum.WildlifeMarineMammals,
                //                    };
                //                }
                //                break;
                //            default:
                //                {
                //                    polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //                    {
                //                        PolSourceObsInfoEnum.Wildlife,
                //                        PolSourceObsInfoEnum.Error,
                //                    };
                //                }
                //                break;
                //        }
                //    }
                //    break;
                //default:
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Error,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //}
            }
            else if (Prov == "QC")
            {
                //switch (CODE.ToUpper())
                //{
                //case "AB":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.RecreationActivities,
                //            PolSourceObsInfoEnum.RecreationActivitiesSwimmingArea,
                //        };
                //    }
                //    break;
                //case "AE":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Industry,
                //            PolSourceObsInfoEnum.IndustryAirport,
                //        };
                //    }
                //    break;
                //case "AF":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Agriculture,
                //            PolSourceObsInfoEnum.AgricultureFeedlotHobbyFarmPasture,
                //        };
                //    }
                //    break;
                //case "AG":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Agriculture,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "BT":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.MarineFacilities,
                //            PolSourceObsInfoEnum.MarineFacilitiesAnchoragesMarineParksWharves,
                //        };
                //    }
                //    break;
                //case "CA":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Wildlife,
                //            PolSourceObsInfoEnum.WildlifeLandMammals,
                //        };
                //    }
                //    break;
                //case "CE":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Sewage,
                //            PolSourceObsInfoEnum.SewageDrainfield,
                //        };
                //    }
                //    break;
                //case "CG":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.RecreationActivities,
                //            PolSourceObsInfoEnum.RecreationActivitiesCampground,
                //        };
                //    }
                //    break;
                //case "CT":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Industry,
                //            PolSourceObsInfoEnum.IndustryWasteTreatmentCenter,
                //        };
                //    }
                //    break;
                //case "DE":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Industry,
                //            PolSourceObsInfoEnum.IndustryDump,
                //        };
                //    }
                //    break;
                //case "EC":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Agriculture,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "EP":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Urban,
                //            PolSourceObsInfoEnum.UrbanOutfall,
                //        };
                //    }
                //    break;
                //case "ET":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.RecreationActivities,
                //            PolSourceObsInfoEnum.RecreationActivitiesTouristFacility,
                //        };
                //    }
                //    break;
                //case "FS":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Sewage,
                //            PolSourceObsInfoEnum.SewagePitPrivyStraightPipeSepticTankField,
                //        };
                //    }
                //    break;
                //case "HA":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Sewage,
                //            PolSourceObsInfoEnum.SewageHomes,
                //        };
                //    }
                //    break;
                //case "HAGR":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Sewage,
                //            PolSourceObsInfoEnum.SewageHomes,
                //        };
                //    }
                //    break;
                //case "HP":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.MarineFacilities,
                //            PolSourceObsInfoEnum.MarineFacilitiesAnchoragesMarineParksWharves,
                //        };
                //    }
                //    break;
                //case "HR":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.RecreationActivities,
                //            PolSourceObsInfoEnum.RecreationActivitiesRestStop,
                //        };
                //    }
                //    break;
                //case "ID":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Industry,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "IMM":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.MarineFacilities,
                //            PolSourceObsInfoEnum.MarineFacilitiesDisposalAtSea,
                //        };
                //    }
                //    break;
                //case "MA":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.MarineFacilities,
                //            PolSourceObsInfoEnum.MarineFacilitiesMarina,
                //        };
                //    }
                //    break;
                //case "MM":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Wildlife,
                //            PolSourceObsInfoEnum.WildlifeMarineMammals,
                //        };
                //    }
                //    break;
                //case "MO":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Sewage,
                //            PolSourceObsInfoEnum.SewageMotel,
                //        };
                //    }
                //    break;
                //case "OI":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Wildlife,
                //            PolSourceObsInfoEnum.WildlifeBirds,
                //        };
                //    }
                //    break;
                //case "OR":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Wildlife,
                //            PolSourceObsInfoEnum.WildlifeLandMammals,
                //        };
                //    }
                //    break;
                //case "OU":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Wildlife,
                //            PolSourceObsInfoEnum.WildlifeLandMammals,
                //        };
                //    }
                //    break;
                //case "PD":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Urban,
                //            PolSourceObsInfoEnum.UrbanStormWater,
                //        };
                //    }
                //    break;
                //case "PI":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Industry,
                //            PolSourceObsInfoEnum.IndustryFishHatchery,
                //        };
                //    }
                //    break;
                //case "PL":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Forestry,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //case "PM":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.MarineFacilities,
                //            PolSourceObsInfoEnum.MarineFacilitiesSeaport,
                //        };
                //    }
                //    break;
                //case "PO":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Industry,
                //            PolSourceObsInfoEnum.IndustryFishProcessing,
                //        };
                //    }
                //    break;
                //case "RC":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Sewage,
                //            PolSourceObsInfoEnum.SewageFuelTank,
                //        };
                //    }
                //    break;
                //case "RDM":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Sewage,
                //            PolSourceObsInfoEnum.SewagePitPrivyStraightPipeSepticTankField,
                //        };
                //    }
                //    break;
                //case "RE":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.SurfaceRunoff,
                //            PolSourceObsInfoEnum.SurfaceRunoffVariousDischarges,
                //        };
                //    }
                //    break;
                //case "SP":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Urban,
                //            PolSourceObsInfoEnum.UrbanLiftStation,
                //        };
                //    }
                //    break;
                //case "SPR":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Urban,
                //            PolSourceObsInfoEnum.UrbanLiftStation,
                //        };
                //    }
                //    break;
                //case "TP":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Urban,
                //            PolSourceObsInfoEnum.UrbanOutfall,
                //        };
                //    }
                //    break;
                //case "TR":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.MarineFacilities,
                //            PolSourceObsInfoEnum.MarineFacilitiesFerry,
                //        };
                //    }
                //    break;
                //case "UE":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Urban,
                //            PolSourceObsInfoEnum.UrbanSewageTreatmentPlant,
                //        };
                //    }
                //    break;
                //case "UT":
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Industry,
                //            PolSourceObsInfoEnum.IndustryFishProcessing,
                //        };
                //    }
                //    break;
                //default:
                //    {
                //        polSourceObsInfoList = new List<PolSourceObsInfoEnum>()
                //        {
                //            PolSourceObsInfoEnum.Error,
                //            PolSourceObsInfoEnum.Error,
                //        };
                //    }
                //    break;
                //}
            }
            else
            {
            }

            return polSourceObsInfoList;
        }
        private string GetTideCode(string TIDE_CODE)
        {
            switch (TIDE_CODE.ToUpper())
            {
                case "L":
                    {
                        TIDE_CODE = "LOW TIDE";
                    }
                    break;
                case "H":
                    {
                        TIDE_CODE = "HIGH TIDE";
                    }
                    break;
                case "M":
                    {
                        TIDE_CODE = "MID TIDE";
                    }
                    break;
                case "HL":
                    {
                        TIDE_CODE = "HIGH TIDE";
                    }
                    break;
                case "LL":
                    {
                        TIDE_CODE = "LOW TIDE";
                    }
                    break;
                case "ML":
                    {
                        TIDE_CODE = "MID TIDE";
                    }
                    break;
                case "R":
                    {
                        TIDE_CODE = "MID RISING";
                    }
                    break;
                case "HI":
                    {
                        TIDE_CODE = "HIGH TIDE";
                    }
                    break;
                case "F":
                    {
                        TIDE_CODE = "MID FALLING";
                    }
                    break;
                case "NT":
                    {
                        TIDE_CODE = "MID TIDE";
                    }
                    break;
                case "ME":
                    {
                        TIDE_CODE = "MID TIDE";
                    }
                    break;
                case "HE":
                    {
                        TIDE_CODE = "HIGH TIDE";
                    }
                    break;
                case "MR":
                    {
                        TIDE_CODE = "MID RISING";
                    }
                    break;
                case "MF":
                    {
                        TIDE_CODE = "MID FALLING";
                    }
                    break;
                case "LF":
                    {
                        TIDE_CODE = "LOW FALLING";
                    }
                    break;
                case "LR":
                    {
                        TIDE_CODE = "LOW RISING";
                    }
                    break;
                case "HR":
                    {
                        TIDE_CODE = "HIGH RISING";
                    }
                    break;
                case "HT":
                    {
                        TIDE_CODE = "HIGH TIDE";
                    }
                    break;
                case "HF":
                    {
                        TIDE_CODE = "HIGH FALLING";
                    }
                    break;
                case "LT":
                    {
                        TIDE_CODE = "LOW TIDE";
                    }
                    break;
                case "MT":
                    {
                        TIDE_CODE = "MID TIDE";
                    }
                    break;
                default:
                    {
                        TIDE_CODE = "MID TIDE";
                    }
                    break;
            }
            return TIDE_CODE;
        }
        private bool LoadRunsAtl()
        {
            List<string> ShortProv = new List<string>() { "NB", "NS", "NL", "PE" };
            List<string> Prov = new List<string>() { "New Brunswick", "Nova Scotia", "Newfoundland and Labrador", "Prince Edward Island" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateRunsAtl(Prov[i], ShortProv[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool LoadRunsNB()
        {
            List<string> ShortProv = new List<string>() { "NB" };
            List<string> Prov = new List<string>() { "New Brunswick" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateRunsAtl(Prov[i], ShortProv[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool LoadRunsNBMOU()
        {
            List<string> ShortProv = new List<string>() { "NB" };
            List<string> Prov = new List<string>() { "New Brunswick" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateRunsAtlMOU(Prov[i], ShortProv[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool LoadRunsNL()
        {
            List<string> ShortProv = new List<string>() { "NL" };
            List<string> Prov = new List<string>() { "Newfoundland and Labrador" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateRunsAtl(Prov[i], ShortProv[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool LoadRunsNS()
        {
            List<string> ShortProv = new List<string>() { "NS" };
            List<string> Prov = new List<string>() { "Nova Scotia" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateRunsAtl(Prov[i], ShortProv[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool LoadRunsPE()
        {
            List<string> ShortProv = new List<string>() { "PE" };
            List<string> Prov = new List<string>() { "Prince Edward Island" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateRunsAtl(Prov[i], ShortProv[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool LoadSamplesAtl()
        {
            List<string> ShortProv = new List<string>() { "NB", "NS", "NL", "PE" };
            List<string> Prov = new List<string>() { "New Brunswick", "Nova Scotia", "Newfoundland and Labrador", "Prince Edward Island" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateSamplesAtl(Prov[i], ShortProv[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool LoadSamplesNB()
        {
            List<string> ShortProv = new List<string>() { "NB" };
            List<string> Prov = new List<string>() { "New Brunswick" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateSamplesAtl(Prov[i], ShortProv[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool LoadSamplesNBMOU()
        {
            List<string> ShortProv = new List<string>() { "NB" };
            List<string> Prov = new List<string>() { "New Brunswick" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateSamplesAtlMOU(Prov[i], ShortProv[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool LoadSamplesNL()
        {
            List<string> ShortProv = new List<string>() { "NL" };
            List<string> Prov = new List<string>() { "Newfoundland and Labrador" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateSamplesAtl(Prov[i], ShortProv[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool LoadSamplesNS()
        {
            List<string> ShortProv = new List<string>() { "NS" };
            List<string> Prov = new List<string>() { "Nova Scotia" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateSamplesAtl(Prov[i], ShortProv[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool LoadSamplesPE()
        {
            List<string> ShortProv = new List<string>() { "PE" };
            List<string> Prov = new List<string>() { "Prince Edward Island" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateSamplesAtl(Prov[i], ShortProv[i]))
                {
                    return false;
                }
            }
            return true;
        }
        private bool LoadSanitaryAllAtl()
        {
            if (!CreateSanitaryAtl("New Brunswick", "NB", "Joe"))
            {
                return false;
            }

            if (!CreateSanitaryAtl("New Brunswick", "NB", "Pat"))
            {
                return false;
            }

            if (!CreateSanitaryAtl("Nova Scotia", "NS", "Dav"))
            {
                return false;
            }

            if (!CreateSanitaryAtl("Prince Edward Island", "PE", "Dav"))
            {
                return false;
            }

            List<string> ShortProv = new List<string>() { "NB", "NS", "PE" };
            List<string> Prov = new List<string>() { "New Brunswick", "Nova Scotia", "Prince Edward Island" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateSanitaryAtl(Prov[i], ShortProv[i], "Don"))
                {
                    return false;
                }
            }

            return true;
        }
        private bool LoadSanitaryNB()
        {
            if (!CreateSanitaryAtl("New Brunswick", "NB", "Joe"))
            {
                return false;
            }

            if (!CreateSanitaryAtl("New Brunswick", "NB", "Pat"))
            {
                return false;
            }

            List<string> ShortProv = new List<string>() { "NB" };
            List<string> Prov = new List<string>() { "New Brunswick" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateSanitaryAtl(Prov[i], ShortProv[i], "Don"))
                {
                    return false;
                }
            }

            return true;
        }
        private bool LoadSanitaryNL()
        {
            if (!CreateSanitaryAtl("Newfoundland and Labrador", "NL", "DavC"))
            {
                return false;
            }

            List<string> ShortProv = new List<string>() { "NL" };
            List<string> Prov = new List<string>() { "Newfoundland and Labrador" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateSanitaryAtl(Prov[i], ShortProv[i], "Don"))
                {
                    return false;
                }
            }

            return true;
        }
        private bool LoadSanitaryNS()
        {
            if (!CreateSanitaryAtl("Nova Scotia", "NS", "Dav"))
            {
                return false;
            }

            List<string> ShortProv = new List<string>() { "NS" };
            List<string> Prov = new List<string>() { "Nova Scotia" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateSanitaryAtl(Prov[i], ShortProv[i], "Don"))
                {
                    return false;
                }
            }


            return true;
        }
        private bool LoadSanitaryPE()
        {
            if (!CreateSanitaryAtl("Prince Edward Island", "PE", "Dav"))
            {
                return false;
            }

            List<string> ShortProv = new List<string>() { "PE" };
            List<string> Prov = new List<string>() { "Prince Edward Island" };

            for (int i = 0; i < ShortProv.Count; i++)
            {
                if (!CreateSanitaryAtl(Prov[i], ShortProv[i], "Don"))
                {
                    return false;
                }
            }

            return true;
        }
        private bool LoadTVItemsAll()
        {
            List<string> Prov = new List<string>() { "New Brunswick", "Nova Scotia", "Prince Edward Island", "Newfoundland and Labrador", "British Columbia", "Québec" };

            for (int i = 0; i < Prov.Count; i++)
            {
                if (!CreateTVItemsAll(Prov[i]))
                {
                    return false;
                }
            }

            return true;
        }
        private bool LoadTVItemsME()
        {
            List<string> Prov = new List<string>() { "Maine" };

            for (int i = 0; i < Prov.Count; i++)
            {
                if (!CreateTVItemsAll(Prov[i]))
                {
                    return false;
                }
            }

            return true;
        }
        private bool LoadTVItemsNB()
        {
            List<string> Prov = new List<string>() { "New Brunswick" };

            for (int i = 0; i < Prov.Count; i++)
            {
                if (!CreateTVItemsAll(Prov[i]))
                {
                    return false;
                }
            }

            return true;
        }
        private bool LoadTVItemsNL()
        {
            List<string> Prov = new List<string>() { "Newfoundland and Labrador" };

            for (int i = 0; i < Prov.Count; i++)
            {
                if (!CreateTVItemsAll(Prov[i]))
                {
                    return false;
                }
            }

            return true;
        }
        private bool LoadTVItemsNS()
        {
            List<string> Prov = new List<string>() { "Nova Scotia" };

            for (int i = 0; i < Prov.Count; i++)
            {
                if (!CreateTVItemsAll(Prov[i]))
                {
                    return false;
                }
            }

            return true;
        }
        private bool LoadTVItemsPE()
        {
            List<string> Prov = new List<string>() { "Prince Edward Island" };

            for (int i = 0; i < Prov.Count; i++)
            {
                if (!CreateTVItemsAll(Prov[i]))
                {
                    return false;
                }
            }

            return true;
        }
        private bool LoadTVItemsBC()
        {
            List<string> Prov = new List<string>() { "British Columbia" };

            for (int i = 0; i < Prov.Count; i++)
            {
                if (!CreateTVItemsAll(Prov[i]))
                {
                    return false;
                }
            }

            return true;
        }
        private bool LoadTVItemsQC()
        {
            List<string> Prov = new List<string>() { "Québec" };

            for (int i = 0; i < Prov.Count; i++)
            {
                if (!CreateTVItemsAll(Prov[i]))
                {
                    return false;
                }
            }

            return true;
        }
        private bool NomenclatureATL()
        {
            if (!CreateNomenclatureAll()) return false;

            return true;
        }
        #endregion Function private

        #region Internal Classes
        private class TableInfo
        {
            public string table_name { get; set; }
            public string column_name { get; set; }
            public string system_data_type { get; set; }
            public Int16 max_length { get; set; }
            public byte precision { get; set; }
            public byte scale { get; set; }
            public bool is_nullable { get; set; }
            public bool is_ansi_padded { get; set; }
        }
        private class SubSector
        {
            public SubSector()
            {
                Polygon = new List<Coord>();
            }
            public string Province { get; set; }
            public string Area { get; set; }
            public string Sector { get; set; }
            public string Subsector { get; set; }
            public List<Coord> Polygon { get; set; }
            public string SSName { get; set; }

        }
        //private class Coord
        //{
        //    public float Latitude { get; set; }
        //    public float Longitude { get; set; }
        //    public int Ordinal { get; set; }
        //}
        private class Vect
        {
            public Coord Start { get; set; }
            public Coord End { get; set; }
        }
        private class ProvTxt
        {
            public string Province { get; set; }
            public string ProvStr { get; set; }
            public string Prov2L { get; set; }
        }
        private class ProvCoord
        {
            public string Province { get; set; }
            public float Lat { get; set; }
            public float Lng { get; set; }
        }
        private class BCStation
        {
            public string WQMSite { get; set; }
            public float Lat { get; set; }
            public float Lng { get; set; }
            public string Desc { get; set; }
            public int TVItemID { get; set; }
        }
        private class TT
        {
            public string TTAcronym { get; set; }
            public int TVItemID { get; set; }
        }
        private class AM
        {
            public string AMAcronym { get; set; }
            public int TVItemID { get; set; }
        }
        private class Mat
        {
            public string MatAcronym { get; set; }
            public int TVItemID { get; set; }
        }
        private class Lab
        {
            public string LabAcronym { get; set; }
            public int TVItemID { get; set; }
        }
        private class SampleStatus
        {
            public string SampleStatusAcronym { get; set; }
            public int TVItemID { get; set; }
        }
        private class Obs
        {
            public string ObsTxt { get; set; }
            public int TVItemID { get; set; }
        }
        private class BCPoly
        {
            public Point[] points { get; set; }
            public int TVItemID { get; set; }
        }
        private class QCPoly
        {
            public Point[] points { get; set; }
            public int TVItemID { get; set; }
        }
        private class PolSourcCoord
        {
            public string key { get; set; }
            public float? Lat { get; set; }
            public float? Lng { get; set; }
        }
        private class SSCoord
        {
            public string key { get; set; }
            public float? Lat { get; set; }
            public float? Lng { get; set; }
        }
        private class obsTypeProv
        {
            public obsTypeProv()
            {
                obsTypeList = new List<string>();
                CountList = new List<int>();
            }
            public string Prov { get; set; }
            public List<string> obsTypeList { get; set; }
            public List<int> CountList { get; set; }
        }
        private class BCClimateSite
        {
            public string BCText { get; set; }
            public string SubSector { get; set; }
            public string ClimateSite { get; set; }
            public string ClimateID { get; set; }
            public int ClimateSiteID { get; set; }
            public int ClimateSiteTVItemID { get; set; }
            public int SurveyID { get; set; }
        }
        private class BWObj
        {
            public BWObj()
            {
                this.Status = BWStatus.NotStarted;
                this.RequiredNameList = new List<string>();
                this.StartTime = new DateTime(2050, 1, 1);
                this.EndTime = new DateTime(2050, 1, 1);
            }
            public string Name { get; set; }
            public BWStatus Status { get; set; }
            public List<string> RequiredNameList { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }
        private enum BWStatus
        {
            NotStarted,
            Started,
            Completed
        }
        private class BWDoubleStringObj
        {
            public string TVPath { get; set; }
            public string OldTVPath { get; set; }
        }
        private class BW4StringObj
        {
            public string TVPath { get; set; }
            public string TVPath2 { get; set; }
            public string OldTVPath { get; set; }
            public string NextType { get; set; }
        }
        private class OldTVItem
        {
            public int CSSPItemID { get; set; }
            public string CSSPPath { get; set; }
            public string CSSPText { get; set; }
        }
        private class BCUseOfClimateSite
        {
            public int UseOfClimateSiteID { get; set; }
            public int ClimateSiteTVItemID { get; set; }
            public int ClimateSiteID { get; set; }
            public string ClimateID { get; set; }
            public int TVItemID { get; set; }
        }
        public class TextLanguage
        {
            public string Text { get; set; }
            public string Language { get; set; }
            public TranslationStatusEnum Status { get; set; }
        }
        public class QCSectMatch
        {
            public string GoodSector { get; set; }
            public string MatchSector { get; set; }
        }
        public class QCSectObj
        {
            public QCSectObj()
            {
                SubQCSectList = new List<QCSectObj>();
            }
            public string name { get; set; }
            public string placemark { get; set; }
            public List<QCSectObj> SubQCSectList { get; set; }
        }
        public class SanitaryObsSite
        {
            public int SanitaryObsID { get; set; }
            public int SanitarySiteID { get; set; }
            public Nullable<bool> Active { get; set; }
            public Nullable<int> siteid { get; set; }
            public Nullable<System.DateTime> ObsDate { get; set; }
            public string Name_Inspector { get; set; }
            public string Type { get; set; }
            public string Status { get; set; }
            public string Risk_Assessment { get; set; }
            public string Observations { get; set; }
        }
        public class SanitarySite
        {
            public int SanitarySiteID { get; set; }
            public Nullable<int> siteid { get; set; }
            public string Locator { get; set; }
            public Nullable<int> Site { get; set; }
            public Nullable<int> Zone { get; set; }
            public Nullable<double> easting { get; set; }
            public Nullable<double> northing { get; set; }
            public string Datum { get; set; }
            public Nullable<double> latitude { get; set; }
            public Nullable<double> longitude { get; set; }
        }
        public class DocType
        {
            public string FileNameEN { get; set; }
            public string FileNameFR { get; set; }
            public string TypeEN { get; set; }
            public string Extension { get; set; }
        }
        #endregion Internal Classes

        #region Internal Enumerations
        public enum AuthLevel
        {
            Nothing,
            Read,
            Edit,
            Create,
            Delete,
            Admin,
        }
        public enum StatusEnum
        {
            Error = -1,
            Copying = 1,
            Copied = 2,
            Changing = 3,
            Changed = 4,
            AskToRun = 5,
            Running = 6,
            Completed = 7,
        }
        //public enum StatusTranslationEnum
        //{
        //    Error = -1,
        //    NotTranslated = 1,
        //    ElectronicallyTranslated = 2,
        //    Translated = 3,
        //}
        #endregion Internal Enumerations

        private void butSaveStatus_Click(object sender, EventArgs e)
        {
            StreamWriter sw = new StreamWriter(@"C:\CSSP latest code old\DataTool\ImportByFunction\Data_inputs\Status.txt");

            sw.WriteLine("textBoxCreateNomenclatureAll," + textBoxCreateNomenclatureAll.Text);
            sw.WriteLine("textBoxCreateWQMTVSubSectorsAll," + textBoxCreateWQMTVSubSectorsAll.Text);
            sw.WriteLine("textBoxSubsectorCreateMapInfosFromAreas_Sectors_SubsectorsKML," + textBoxSubsectorCreateMapInfosFromAreas_Sectors_SubsectorsKML.Text);
            sw.WriteLine("textBoxAreaCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML," + textBoxAreaCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text);
            sw.WriteLine("textBoxSectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML," + textBoxSectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text);
            sw.WriteLine("textBoxSubsectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML," + textBoxSubsectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text);
            sw.WriteLine("textBoxAreaCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML," + textBoxAreaCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text);
            sw.WriteLine("textBoxSectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML," + textBoxSectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text);
            sw.WriteLine("textBoxSubsectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML," + textBoxSubsectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text);
            sw.WriteLine("textBoxNBCreateClimateSitesAll," + textBoxNBCreateClimateSitesAll.Text);
            sw.WriteLine("textBoxNLCreateClimateSitesAll," + textBoxNLCreateClimateSitesAll.Text);
            sw.WriteLine("textBoxNSCreateClimateSitesAll," + textBoxNSCreateClimateSitesAll.Text);
            sw.WriteLine("textBoxPECreateClimateSitesAll," + textBoxPECreateClimateSitesAll.Text);
            sw.WriteLine("textBoxBCCreateClimateSitesAll," + textBoxBCCreateClimateSitesAll.Text);
            sw.WriteLine("textBoxQCCreateClimateSitesAll," + textBoxQCCreateClimateSitesAll.Text);
            sw.WriteLine("textBoxNBCreateHydrometricSitesAll," + textBoxNBCreateHydrometricSitesAll.Text);
            sw.WriteLine("textBoxNLCreateHydrometricSitesAll," + textBoxNLCreateHydrometricSitesAll.Text);
            sw.WriteLine("textBoxNSCreateHydrometricSitesAll," + textBoxNSCreateHydrometricSitesAll.Text);
            sw.WriteLine("textBoxPECreateHydrometricSitesAll," + textBoxPECreateHydrometricSitesAll.Text);
            sw.WriteLine("textBoxBCCreateHydrometricSitesAll," + textBoxBCCreateHydrometricSitesAll.Text);
            sw.WriteLine("textBoxQCCreateHydrometricSitesAll," + textBoxQCCreateHydrometricSitesAll.Text);
            sw.WriteLine("textBoxNBCreateUseOfClimateSitesAtl," + textBoxNBCreateUseOfClimateSitesAtl.Text);
            sw.WriteLine("textBoxNLCreateUseOfClimateSitesAtl," + textBoxNLCreateUseOfClimateSitesAtl.Text);
            sw.WriteLine("textBoxNSCreateUseOfClimateSitesAtl," + textBoxNSCreateUseOfClimateSitesAtl.Text);
            sw.WriteLine("textBoxPECreateUseOfClimateSitesAtl," + textBoxPECreateUseOfClimateSitesAtl.Text);
            sw.WriteLine("textBoxBCCreateUseOfClimateSitesBC," + textBoxBCCreateUseOfClimateSitesBC.Text);
            sw.WriteLine("textBoxQCCreateUseOfClimateSitesQC," + textBoxQCCreateUseOfClimateSitesQC.Text);
            sw.WriteLine("textBoxNBCreatePrecipitationsAtl," + textBoxNBCreatePrecipitationsAtl.Text);
            sw.WriteLine("textBoxNLCreatePrecipitationsAtl," + textBoxNLCreatePrecipitationsAtl.Text);
            sw.WriteLine("textBoxNSCreatePrecipitationsAtl," + textBoxNSCreatePrecipitationsAtl.Text);
            sw.WriteLine("textBoxPECreatePrecipitationsAtl," + textBoxPECreatePrecipitationsAtl.Text);
            sw.WriteLine("textBoxBCCreatePrecipitationsBC," + textBoxBCCreatePrecipitationsBC.Text);
            sw.WriteLine("textBoxQCCreatePrecipitationsQC," + textBoxQCCreatePrecipitationsQC.Text);
            sw.WriteLine("textBoxNBCreateStationsAtl," + textBoxNBCreateStationsAtl.Text);
            sw.WriteLine("textBoxNLCreateStationsAtl," + textBoxNLCreateStationsAtl.Text);
            sw.WriteLine("textBoxNSCreateStationsAtl," + textBoxNSCreateStationsAtl.Text);
            sw.WriteLine("textBoxPECreateStationsAtl," + textBoxPECreateStationsAtl.Text);
            sw.WriteLine("textBoxBCCreateStationsBC," + textBoxBCCreateStationsBC.Text);
            sw.WriteLine("textBoxQCCreateStationsQC," + textBoxQCCreateStationsQC.Text);
            sw.WriteLine("textBoxNBCreateSamplesAtl," + textBoxNBCreateSamplesAtl.Text);
            sw.WriteLine("textBoxNLCreateSamplesAtl," + textBoxNLCreateSamplesAtl.Text);
            sw.WriteLine("textBoxNSCreateSamplesAtl," + textBoxNSCreateSamplesAtl.Text);
            sw.WriteLine("textBoxPECreateSamplesAtl," + textBoxPECreateSamplesAtl.Text);
            sw.WriteLine("textBoxBCCreateSamplesBC," + textBoxBCCreateSamplesBC.Text);
            sw.WriteLine("textBoxQCCreateSamplesQC," + textBoxQCCreateSamplesQC.Text);
            sw.WriteLine("textBoxNBCreateRunsAtl," + textBoxNBCreateRunsAtl.Text);
            sw.WriteLine("textBoxNLCreateRunsAtl," + textBoxNLCreateRunsAtl.Text);
            sw.WriteLine("textBoxNSCreateRunsAtl," + textBoxNSCreateRunsAtl.Text);
            sw.WriteLine("textBoxPECreateRunsAtl," + textBoxPECreateRunsAtl.Text);
            sw.WriteLine("textBoxBCCreateRunsBC," + textBoxBCCreateRunsBC.Text);
            sw.WriteLine("textBoxQCCreateRunsQC," + textBoxQCCreateRunsQC.Text);
            sw.WriteLine("textBoxNBDonCreateSanitaryAtl," + textBoxNBDonCreateSanitaryAtl.Text);
            sw.WriteLine("textBoxNBPatCreateSanitaryAtl," + textBoxNBPatCreateSanitaryAtl.Text);
            sw.WriteLine("textBoxNBDavCreateSanitaryAtl," + textBoxNBJoeCreateSanitaryAtl.Text);
            sw.WriteLine("textBoxNLDonCreateSanitaryAtl," + textBoxNLDonCreateSanitaryAtl.Text);
            sw.WriteLine("textBoxNLDavCCreateSanitaryAtl," + textBoxNLDavCCreateSanitaryAtl.Text);
            sw.WriteLine("textBoxNSDonCreateSanitaryAtl," + textBoxNSDonCreateSanitaryAtl.Text);
            sw.WriteLine("textBoxNSDavCreateSanitaryAtl," + textBoxNSDavCreateSanitaryAtl.Text);
            sw.WriteLine("textBoxPEDonCreateSanitaryAtl," + textBoxPEDonCreateSanitaryAtl.Text);
            sw.WriteLine("textBoxPEDavCreateSanitaryAtl," + textBoxPEDavCreateSanitaryAtl.Text);
            sw.WriteLine("textBoxBCCreateSanitaryBC," + textBoxBCCreateSanitaryBC.Text);
            sw.WriteLine("textBoxQCCreateSanitaryQC," + textBoxQCCreateSanitaryQC.Text);
            sw.WriteLine("textBoxMECreateTVItemsAll," + textBoxMECreateTVItemsAll.Text);
            sw.WriteLine("textBoxNBCreateTVItemsAll," + textBoxNBCreateTVItemsAll.Text);
            sw.WriteLine("textBoxNLCreateTVItemsAll," + textBoxNLCreateTVItemsAll.Text);
            sw.WriteLine("textBoxNSCreateTVItemsAll," + textBoxNSCreateTVItemsAll.Text);
            sw.WriteLine("textBoxPECreateTVItemsAll," + textBoxPECreateTVItemsAll.Text);
            sw.WriteLine("textBoxBCCreateTVItemsAll," + textBoxBCCreateTVItemsAll.Text);
            sw.WriteLine("textBoxQCCreateTVItemsAll," + textBoxQCCreateTVItemsAll.Text);
            sw.WriteLine("textBoxNBCreateMapInfoFromLocationNameLatLongXLSAll," + textBoxNBCreateMapInfoFromLocationNameLatLongXLSAll.Text);
            sw.WriteLine("textBoxNLCreateMapInfoFromLocationNameLatLongXLSAll," + textBoxNLCreateMapInfoFromLocationNameLatLongXLSAll.Text);
            sw.WriteLine("textBoxNSCreateMapInfoFromLocationNameLatLongXLSAll," + textBoxNSCreateMapInfoFromLocationNameLatLongXLSAll.Text);
            sw.WriteLine("textBoxPECreateMapInfoFromLocationNameLatLongXLSAll," + textBoxPECreateMapInfoFromLocationNameLatLongXLSAll.Text);
            sw.WriteLine("textBoxBCCreateMapInfoFromLocationNameLatLongXLSAll," + textBoxBCCreateMapInfoFromLocationNameLatLongXLSAll.Text);
            sw.WriteLine("textBoxQCCreateMapInfoFromLocationNameLatLongXLSAll," + textBoxQCCreateMapInfoFromLocationNameLatLongXLSAll.Text);
            sw.Close();
        }

        private void ImportByFunction_Load(object sender, EventArgs e)
        {
            StreamReader sr = new StreamReader(@"C:\CSSP latest code old\DataTool\ImportByFunction\Data_inputs\Status.txt");

            while (!sr.EndOfStream)
            {
                string Line = sr.ReadLine();
                string textbox = Line.Substring(0, Line.IndexOf(","));
                string value = Line.Substring(Line.IndexOf(",") + 1);
                switch (textbox)
                {
                    case "textBoxCreateNomenclatureAll":
                        {
                            textBoxCreateNomenclatureAll.Text = value;
                        };
                        break;
                    case "textBoxCreateWQMTVSubSectorsAll":
                        {
                            textBoxCreateWQMTVSubSectorsAll.Text = value;
                        };
                        break;
                    case "textBoxSubsectorCreateMapInfosFromAreas_Sectors_SubsectorsKML":
                        {
                            textBoxSubsectorCreateMapInfosFromAreas_Sectors_SubsectorsKML.Text = value;
                        };
                        break;
                    case "textBoxAreaCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML":
                        {
                            textBoxAreaCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text = value;
                        };
                        break;
                    case "textBoxSectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML":
                        {
                            textBoxSectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text = value;
                        };
                        break;
                    case "textBoxSubsectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML":
                        {
                            textBoxSubsectorCreateBCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text = value;
                        };
                        break;
                    case "textBoxAreaCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML":
                        {
                            textBoxAreaCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text = value;
                        };
                        break;
                    case "textBoxSectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML":
                        {
                            textBoxSectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text = value;
                        };
                        break;
                    case "textBoxSubsectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML":
                        {
                            textBoxSubsectorCreateQCTVItemsFromAreasSectorsAndSubsectorFinalKML.Text = value;
                        };
                        break;
                    case "textBoxNBCreateClimateSitesAll":
                        {
                            textBoxNBCreateClimateSitesAll.Text = value;
                        };
                        break;
                    case "textBoxNLCreateClimateSitesAll":
                        {
                            textBoxNLCreateClimateSitesAll.Text = value;
                        };
                        break;
                    case "textBoxNSCreateClimateSitesAll":
                        {
                            textBoxNSCreateClimateSitesAll.Text = value;
                        };
                        break;
                    case "textBoxPECreateClimateSitesAll":
                        {
                            textBoxPECreateClimateSitesAll.Text = value;
                        };
                        break;
                    case "textBoxBCCreateClimateSitesAll":
                        {
                            textBoxBCCreateClimateSitesAll.Text = value;
                        };
                        break;
                    case "textBoxQCCreateClimateSitesAll":
                        {
                            textBoxQCCreateClimateSitesAll.Text = value;
                        };
                        break;
                    case "textBoxNBCreateHydrometricSitesAll":
                        {
                            textBoxNBCreateHydrometricSitesAll.Text = value;
                        };
                        break;
                    case "textBoxNLCreateHydrometricSitesAll":
                        {
                            textBoxNLCreateHydrometricSitesAll.Text = value;
                        };
                        break;
                    case "textBoxNSCreateHydrometricSitesAll":
                        {
                            textBoxNSCreateHydrometricSitesAll.Text = value;
                        };
                        break;
                    case "textBoxPECreateHydrometricSitesAll":
                        {
                            textBoxPECreateHydrometricSitesAll.Text = value;
                        };
                        break;
                    case "textBoxBCCreateHydrometricSitesAll":
                        {
                            textBoxBCCreateHydrometricSitesAll.Text = value;
                        };
                        break;
                    case "textBoxQCCreateHydrometricSitesAll":
                        {
                            textBoxQCCreateHydrometricSitesAll.Text = value;
                        };
                        break;
                    case "textBoxNBCreateUseOfClimateSitesAtl":
                        {
                            textBoxNBCreateUseOfClimateSitesAtl.Text = value;
                        };
                        break;
                    case "textBoxNLCreateUseOfClimateSitesAtl":
                        {
                            textBoxNLCreateUseOfClimateSitesAtl.Text = value;
                        };
                        break;
                    case "textBoxNSCreateUseOfClimateSitesAtl":
                        {
                            textBoxNSCreateUseOfClimateSitesAtl.Text = value;
                        };
                        break;
                    case "textBoxPECreateUseOfClimateSitesAtl":
                        {
                            textBoxPECreateUseOfClimateSitesAtl.Text = value;
                        };
                        break;
                    case "textBoxBCCreateUseOfClimateSitesBC":
                        {
                            textBoxBCCreateUseOfClimateSitesBC.Text = value;
                        };
                        break;
                    case "textBoxQCCreateUseOfClimateSitesQC":
                        {
                            textBoxQCCreateUseOfClimateSitesQC.Text = value;
                        };
                        break;
                    case "textBoxNBCreatePrecipitationsAtl":
                        {
                            textBoxNBCreatePrecipitationsAtl.Text = value;
                        };
                        break;
                    case "textBoxNLCreatePrecipitationsAtl":
                        {
                            textBoxNLCreatePrecipitationsAtl.Text = value;
                        };
                        break;
                    case "textBoxNSCreatePrecipitationsAtl":
                        {
                            textBoxNSCreatePrecipitationsAtl.Text = value;
                        };
                        break;
                    case "textBoxPECreatePrecipitationsAtl":
                        {
                            textBoxPECreatePrecipitationsAtl.Text = value;
                        };
                        break;
                    case "textBoxBCCreatePrecipitationsBC":
                        {
                            textBoxBCCreatePrecipitationsBC.Text = value;
                        };
                        break;
                    case "textBoxQCCreatePrecipitationsQC":
                        {
                            textBoxQCCreatePrecipitationsQC.Text = value;
                        };
                        break;
                    case "textBoxNBCreateStationsAtl":
                        {
                            textBoxNBCreateStationsAtl.Text = value;
                        };
                        break;
                    case "textBoxNLCreateStationsAtl":
                        {
                            textBoxNLCreateStationsAtl.Text = value;
                        };
                        break;
                    case "textBoxNSCreateStationsAtl":
                        {
                            textBoxNSCreateStationsAtl.Text = value;
                        };
                        break;
                    case "textBoxPECreateStationsAtl":
                        {
                            textBoxPECreateStationsAtl.Text = value;
                        };
                        break;
                    case "textBoxBCCreateStationsBC":
                        {
                            textBoxBCCreateStationsBC.Text = value;
                        };
                        break;
                    case "textBoxQCCreateStationsQC":
                        {
                            textBoxQCCreateStationsQC.Text = value;
                        };
                        break;
                    case "textBoxNBCreateSamplesAtl":
                        {
                            textBoxNBCreateSamplesAtl.Text = value;
                        };
                        break;
                    case "textBoxNLCreateSamplesAtl":
                        {
                            textBoxNLCreateSamplesAtl.Text = value;
                        };
                        break;
                    case "textBoxNSCreateSamplesAtl":
                        {
                            textBoxNSCreateSamplesAtl.Text = value;
                        };
                        break;
                    case "textBoxPECreateSamplesAtl":
                        {
                            textBoxPECreateSamplesAtl.Text = value;
                        };
                        break;
                    case "textBoxBCCreateSamplesBC":
                        {
                            textBoxBCCreateSamplesBC.Text = value;
                        };
                        break;
                    case "textBoxQCCreateSamplesQC":
                        {
                            textBoxQCCreateSamplesQC.Text = value;
                        };
                        break;
                    case "textBoxNBCreateRunsAtl":
                        {
                            textBoxNBCreateRunsAtl.Text = value;
                        };
                        break;
                    case "textBoxNLCreateRunsAtl":
                        {
                            textBoxNLCreateRunsAtl.Text = value;
                        };
                        break;
                    case "textBoxNSCreateRunsAtl":
                        {
                            textBoxNSCreateRunsAtl.Text = value;
                        };
                        break;
                    case "textBoxPECreateRunsAtl":
                        {
                            textBoxPECreateRunsAtl.Text = value;
                        };
                        break;
                    case "textBoxBCCreateRunsBC":
                        {
                            textBoxBCCreateRunsBC.Text = value;
                        };
                        break;
                    case "textBoxQCCreateRunsQC":
                        {
                            textBoxQCCreateRunsQC.Text = value;
                        };
                        break;
                    case "textBoxNBDonCreateSanitaryAtl":
                        {
                            textBoxNBDonCreateSanitaryAtl.Text = value;
                        };
                        break;
                    case "textBoxNBPatCreateSanitaryAtl":
                        {
                            textBoxNBPatCreateSanitaryAtl.Text = value;
                        };
                        break;
                    case "textBoxNBDavCreateSanitaryAtl":
                        {
                            textBoxNBJoeCreateSanitaryAtl.Text = value;
                        };
                        break;
                    case "textBoxNLDonCreateSanitaryAtl":
                        {
                            textBoxNLDonCreateSanitaryAtl.Text = value;
                        };
                        break;
                    case "textBoxNLDavCCreateSanitaryAtl":
                        {
                            textBoxNLDavCCreateSanitaryAtl.Text = value;
                        };
                        break;
                    case "textBoxNSDonCreateSanitaryAtl":
                        {
                            textBoxNSDonCreateSanitaryAtl.Text = value;
                        };
                        break;
                    case "textBoxNSDavCreateSanitaryAtl":
                        {
                            textBoxNSDavCreateSanitaryAtl.Text = value;
                        };
                        break;
                    case "textBoxPEDonCreateSanitaryAtl":
                        {
                            textBoxPEDonCreateSanitaryAtl.Text = value;
                        };
                        break;
                    case "textBoxPEDavCreateSanitaryAtl":
                        {
                            textBoxPEDavCreateSanitaryAtl.Text = value;
                        };
                        break;
                    case "textBoxBCCreateSanitaryBC":
                        {
                            textBoxBCCreateSanitaryBC.Text = value;
                        };
                        break;
                    case "textBoxQCCreateSanitaryQC":
                        {
                            textBoxQCCreateSanitaryQC.Text = value;
                        };
                        break;
                    case "textBoxMECreateTVItemsAll":
                        {
                            textBoxMECreateTVItemsAll.Text = value;
                        };
                        break;
                    case "textBoxNBCreateTVItemsAll":
                        {
                            textBoxNBCreateTVItemsAll.Text = value;
                        };
                        break;
                    case "textBoxNLCreateTVItemsAll":
                        {
                            textBoxNLCreateTVItemsAll.Text = value;
                        };
                        break;
                    case "textBoxNSCreateTVItemsAll":
                        {
                            textBoxNSCreateTVItemsAll.Text = value;
                        };
                        break;
                    case "textBoxPECreateTVItemsAll":
                        {
                            textBoxPECreateTVItemsAll.Text = value;
                        };
                        break;
                    case "textBoxBCCreateTVItemsAll":
                        {
                            textBoxBCCreateTVItemsAll.Text = value;
                        };
                        break;
                    case "textBoxQCCreateTVItemsAll":
                        {
                            textBoxQCCreateTVItemsAll.Text = value;
                        };
                        break;
                    case "textBoxNBCreateMapInfoFromLocationNameLatLongXLSAll":
                        {
                            textBoxNBCreateMapInfoFromLocationNameLatLongXLSAll.Text = value;
                        };
                        break;
                    case "textBoxNLCreateMapInfoFromLocationNameLatLongXLSAll":
                        {
                            textBoxNLCreateMapInfoFromLocationNameLatLongXLSAll.Text = value;
                        };
                        break;
                    case "textBoxNSCreateMapInfoFromLocationNameLatLongXLSAll":
                        {
                            textBoxNSCreateMapInfoFromLocationNameLatLongXLSAll.Text = value;
                        };
                        break;
                    case "textBoxPECreateMapInfoFromLocationNameLatLongXLSAll":
                        {
                            textBoxPECreateMapInfoFromLocationNameLatLongXLSAll.Text = value;
                        };
                        break;
                    case "textBoxBCCreateMapInfoFromLocationNameLatLongXLSAll":
                        {
                            textBoxBCCreateMapInfoFromLocationNameLatLongXLSAll.Text = value;
                        };
                        break;
                    case "textBoxQCCreateMapInfoFromLocationNameLatLongXLSAll":
                        {
                            textBoxQCCreateMapInfoFromLocationNameLatLongXLSAll.Text = value;
                        };
                        break;
                    default:
                        {
                        }
                        break;

                }
            }

            sr.Close();
        }

        public class LocatorCoord
        {
            public LocatorCoord()
            {
                coordList = new List<Coord>();
            }
            public string Locator { get; set; }
            public Coord coord { get; set; }
            public List<Coord> coordList { get; set; }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            //CreateFinal();
            //CreateArea();
            //CreateSector();
            //CreatePinSubsectors();
            //CreatePinSectors();
            //CreatePinAreas();
            //UpdateNLMapInfoArea();
            //UpdateNLMapInfoSector();
            //UpdateNLMapInfoSubsector();

            //CorrectWWTPOufallTVItemLink();

        }

        private void CorrectWWTPOufallTVItemLink()
        {
            TVItemLinkService tvItemLinkService = new TVItemLinkService(LanguageEnum.en, user);
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            InfrastructureService infrastructureService = new InfrastructureService(LanguageEnum.en, user);

            List<TVItemModel> tvItemModelListInfrastructure = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(1, TVTypeEnum.Infrastructure);

            int count = 0;
            int Total = tvItemModelListInfrastructure.Count;
            foreach (TVItemModel tvItemModel in tvItemModelListInfrastructure)
            {
                count += 1;
                lblStatus.Text = count + " of " + Total;
                Application.DoEvents();
                InfrastructureModel infrastructureModel = infrastructureService.GetInfrastructureModelWithInfrastructureTVItemIDDB(tvItemModel.TVItemID);

                if (string.IsNullOrWhiteSpace(infrastructureModel.Error))
                {
                    if (infrastructureModel.InfrastructureType == InfrastructureTypeEnum.WWTP)
                    {
                        TVItemLinkModel tvItemLinkModel = tvItemLinkService.GetTVItemLinkModelWithFromTVItemIDAndToTVItemIDDB(tvItemModel.TVItemID + 1, tvItemModel.TVItemID);
                        TVItemLinkModel tvItemLinkModel2 = tvItemLinkService.GetTVItemLinkModelWithFromTVItemIDAndToTVItemIDDB(tvItemModel.TVItemID + 1, tvItemModel.TVItemID);
                        if (string.IsNullOrWhiteSpace(tvItemLinkModel.Error))
                        {
                            tvItemLinkModel2.FromTVItemID = tvItemLinkModel.ToTVItemID;
                            tvItemLinkModel2.ToTVItemID = tvItemLinkModel.FromTVItemID;
                            tvItemLinkModel2.TVPath = "p" + tvItemLinkModel2.FromTVItemID + "p" + tvItemLinkModel2.ToTVItemID;

                            TVItemLinkModel tvItemLinkModel3 = tvItemLinkService.PostUpdateTVItemLinkDB(tvItemLinkModel2);
                            if (!string.IsNullOrWhiteSpace(tvItemLinkModel3.Error))
                            {
                                richTextBoxStatus.AppendText("ERROR " + tvItemLinkModel3.Error + "\r\n");
                                return;
                            }
                        }
                    }
                }
            }
        }


        private void UpdateNLMapInfoArea()
        {
            List<LocatorCoord> locatorCoordList = new List<LocatorCoord>();

            LoadNLAreasFinal(locatorCoordList);

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            TVItemModel tvItemModelNL = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "Newfoundland and Labrador", TVTypeEnum.Province);

            richTextBoxStatus.AppendText(tvItemModelNL.TVText + "\r\n");

            List<TVItemModel> tvItemModelAreaList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelNL.TVItemID, TVTypeEnum.Area);

            foreach (TVItemModel tvItemModel in tvItemModelAreaList)
            {
                richTextBoxStatus.AppendText(tvItemModel.TVText + "\r\n");
                foreach (LocatorCoord locatorCoord in locatorCoordList)
                {
                    if (tvItemModel.TVText.StartsWith(locatorCoord.Locator + " "))
                    {
                        lblStatus.Text = tvItemModel.TVText;
                        Application.DoEvents();

                        //MapInfoModel mapInfoModel = mapInfoService.PostDeleteMapInfoWithTVItemIDDB(tvItemModel.TVItemID);
                        //if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        //{
                        //    richTextBoxStatus.AppendText("ERROR: " + mapInfoModel.Error);
                        //    return;
                        //}

                        //List<Coord> coordList = new List<Coord>() 
                        //{
                        //    new Coord() { Lat = locatorCoord.coord.Lat, Lng = locatorCoord.coord.Lng },
                        //};

                        //MapInfoModel mapInfoModel = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Area, tvItemModel.TVItemID);
                        //if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        //{
                        //    richTextBoxStatus.AppendText("ERROR: " + mapInfoModel.Error);
                        //    return;
                        //}

                        //mapInfoModel = mapInfoService.CreateMapInfoObjectDB(locatorCoord.coordList, MapInfoDrawTypeEnum.Polygon, TVTypeEnum.Area, tvItemModel.TVItemID);
                        //if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        //{
                        //    richTextBoxStatus.AppendText("ERROR: " + mapInfoModel.Error);
                        //    return;
                        //}

                        break;
                    }
                }
            }

        }
        private void UpdateNLMapInfoSector()
        {
            List<LocatorCoord> locatorCoordList = new List<LocatorCoord>();

            LoadNLSectorsFinal(locatorCoordList);

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            TVItemModel tvItemModelNL = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "Newfoundland and Labrador", TVTypeEnum.Province);

            richTextBoxStatus.AppendText(tvItemModelNL.TVText + "\r\n");

            List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelNL.TVItemID, TVTypeEnum.Sector);

            foreach (TVItemModel tvItemModel in tvItemModelSectorList)
            {
                richTextBoxStatus.AppendText(tvItemModel.TVText + "\r\n");
                foreach (LocatorCoord locatorCoord in locatorCoordList)
                {
                    if (tvItemModel.TVText.StartsWith(locatorCoord.Locator + " "))
                    {
                        lblStatus.Text = tvItemModel.TVText;
                        Application.DoEvents();

                        //MapInfoModel mapInfoModel = mapInfoService.PostDeleteMapInfoWithTVItemIDDB(tvItemModel.TVItemID);
                        //if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        //{
                        //    richTextBoxStatus.AppendText("ERROR: " + mapInfoModel.Error);
                        //    return;
                        //}

                        //List<Coord> coordList = new List<Coord>() 
                        //{
                        //    new Coord() { Lat = locatorCoord.coord.Lat, Lng = locatorCoord.coord.Lng },
                        //};

                        //MapInfoModel mapInfoModel = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Sector, tvItemModel.TVItemID);
                        //if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        //{
                        //    richTextBoxStatus.AppendText("ERROR: " + mapInfoModel.Error);
                        //    return;
                        //}

                        //mapInfoModel = mapInfoService.CreateMapInfoObjectDB(locatorCoord.coordList, MapInfoDrawTypeEnum.Polygon, TVTypeEnum.Sector, tvItemModel.TVItemID);
                        //if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        //{
                        //    richTextBoxStatus.AppendText("ERROR: " + mapInfoModel.Error);
                        //    return;
                        //}

                        break;
                    }
                }
            }

        }
        private void UpdateNLMapInfoSubsector()
        {
            List<LocatorCoord> locatorCoordList = new List<LocatorCoord>();

            LoadNLSubsectorsFinal(locatorCoordList);

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            TVItemModel tvItemModelNL = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "Newfoundland and Labrador", TVTypeEnum.Province);

            richTextBoxStatus.AppendText(tvItemModelNL.TVText + "\r\n");

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelNL.TVItemID, TVTypeEnum.Subsector);

            foreach (TVItemModel tvItemModel in tvItemModelSubsectorList)
            {
                richTextBoxStatus.AppendText(tvItemModel.TVText + "\r\n");
                foreach (LocatorCoord locatorCoord in locatorCoordList)
                {
                    if (tvItemModel.TVText.StartsWith(locatorCoord.Locator + " "))
                    {
                        lblStatus.Text = tvItemModel.TVText;
                        Application.DoEvents();
                        //MapInfoModel mapInfoModel = mapInfoService.PostDeleteMapInfoWithTVItemIDDB(tvItemModel.TVItemID);
                        //if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        //{
                        //    richTextBoxStatus.AppendText("ERROR: " + mapInfoModel.Error);
                        //    return;
                        //}

                        //List<Coord> coordList = new List<Coord>() 
                        //{
                        //    new Coord() { Lat = locatorCoord.coord.Lat, Lng = locatorCoord.coord.Lng },
                        //};

                        //MapInfoModel mapInfoModel = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Subsector, tvItemModel.TVItemID);
                        //if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        //{
                        //    richTextBoxStatus.AppendText("ERROR: " + mapInfoModel.Error);
                        //    return;
                        //}

                        //mapInfoModel = mapInfoService.CreateMapInfoObjectDB(locatorCoord.coordList, MapInfoDrawTypeEnum.Polygon, TVTypeEnum.Subsector, tvItemModel.TVItemID);
                        //if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        //{
                        //    richTextBoxStatus.AppendText("ERROR: " + mapInfoModel.Error);
                        //    return;
                        //}

                        break;
                    }
                }
            }

        }

        private void CreatePinSubsectors()
        {
            List<LocatorCoord> locatorCoordList = new List<LocatorCoord>();

            LoadNLSubsectorsFinal(locatorCoordList);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine(@"<Document>");
            sb.AppendLine(@"	<name>NL Subsectors Pin Final.kml</name>");
            sb.AppendLine(@"    <Folder>");
            sb.AppendLine(@"	<name>NL Subsectors Pin Final</name>");
            foreach (LocatorCoord locatorCoord in locatorCoordList.OrderBy(c => c.Locator))
            {
                sb.AppendLine(@"	<Placemark>");
                sb.AppendLine(@"		<name>" + locatorCoord.Locator + "</name>");
                sb.AppendLine(@"		<Point>");
                sb.AppendLine(@"			<coordinates>" + locatorCoord.coord.Lng + "," + locatorCoord.coord.Lat + ",0</coordinates>");
                sb.AppendLine(@"		</Point>");
                sb.AppendLine(@"	</Placemark>");
            }
            sb.AppendLine(@"    </Folder>");
            sb.AppendLine(@"</Document>");
            sb.AppendLine(@"</kml>");

            richTextBoxStatus.Text = sb.ToString();

            richTextBoxStatus.SaveFile(@"C:\DataTool\ImportByFunction\Data_inputs\NL Subsectors Pin Final.kml", RichTextBoxStreamType.PlainText);

        }
        private void CreatePinSectors()
        {
            List<LocatorCoord> locatorCoordList = new List<LocatorCoord>();

            LoadNLSectorsFinal(locatorCoordList);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine(@"<Document>");
            sb.AppendLine(@"	<name>NL Sectors Pin Final.kml</name>");
            sb.AppendLine(@"    <Folder>");
            sb.AppendLine(@"	<name>NL Sectors Pin Final</name>");
            foreach (LocatorCoord locatorCoord in locatorCoordList.OrderBy(c => c.Locator))
            {
                sb.AppendLine(@"	<Placemark>");
                sb.AppendLine(@"		<name>" + locatorCoord.Locator + "</name>");
                sb.AppendLine(@"		<Point>");
                sb.AppendLine(@"			<coordinates>" + locatorCoord.coord.Lng + "," + locatorCoord.coord.Lat + ",0</coordinates>");
                sb.AppendLine(@"		</Point>");
                sb.AppendLine(@"	</Placemark>");
            }
            sb.AppendLine(@"    </Folder>");
            sb.AppendLine(@"</Document>");
            sb.AppendLine(@"</kml>");

            richTextBoxStatus.Text = sb.ToString();

            richTextBoxStatus.SaveFile(@"C:\DataTool\ImportByFunction\Data_inputs\NL Sectors Pin Final.kml", RichTextBoxStreamType.PlainText);

        }
        private void CreatePinAreas()
        {
            List<LocatorCoord> locatorCoordList = new List<LocatorCoord>();

            LoadNLAreasFinal(locatorCoordList);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine(@"<Document>");
            sb.AppendLine(@"	<name>NL Areas Pin Final.kml</name>");
            sb.AppendLine(@"    <Folder>");
            sb.AppendLine(@"	<name>NL Areas Pin Final</name>");
            foreach (LocatorCoord locatorCoord in locatorCoordList.OrderBy(c => c.Locator))
            {
                sb.AppendLine(@"	<Placemark>");
                sb.AppendLine(@"		<name>" + locatorCoord.Locator + "</name>");
                sb.AppendLine(@"		<Point>");
                sb.AppendLine(@"			<coordinates>" + locatorCoord.coord.Lng + "," + locatorCoord.coord.Lat + ",0</coordinates>");
                sb.AppendLine(@"		</Point>");
                sb.AppendLine(@"	</Placemark>");
            }
            sb.AppendLine(@"    </Folder>");
            sb.AppendLine(@"</Document>");
            sb.AppendLine(@"</kml>");

            richTextBoxStatus.Text = sb.ToString();

            richTextBoxStatus.SaveFile(@"C:\DataTool\ImportByFunction\Data_inputs\NL Areas Pin Final.kml", RichTextBoxStreamType.PlainText);

        }

        private void CreateSector()
        {
            List<LocatorCoord> locatorCoordSubsectorsList = new List<LocatorCoord>();
            List<LocatorCoord> locatorCoordSectorsList = new List<LocatorCoord>();
            List<LocatorCoord> locatorCoordAreasList = new List<LocatorCoord>();

            LoadNLSubsectorsFinal(locatorCoordSubsectorsList);
            LoadNLSectorsFinal(locatorCoordSectorsList);
            LoadNLAreasFinal(locatorCoordAreasList);

            List<string> SectorList = locatorCoordSubsectorsList.Select(c => c.Locator.Substring(0, 9)).Distinct().ToList();

            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            double CurrentDist = 400.0D;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine(@"<Document>");
            sb.AppendLine(@"	<name>NL Sectors Final.kml</name>");
            sb.AppendLine(@"    <Folder>");
            sb.AppendLine(@"	<name>NL Sectors Final</name>");


            foreach (string sector in SectorList)
            {
                List<Coord> coordListSubsector = new List<Coord>();
                foreach (LocatorCoord locatorCoordSubsector in locatorCoordSubsectorsList.Where(c => c.Locator.StartsWith(sector)).OrderBy(c => c.Locator))
                {
                    foreach (Coord coord in locatorCoordSubsector.coordList)
                    {
                        coordListSubsector.Add(coord);
                    }
                }

                lblStatus.Text = sector;
                Application.DoEvents();

                foreach (LocatorCoord locatorCoordSector in locatorCoordSectorsList.Where(c => c.Locator.StartsWith(sector)))
                {
                    foreach (Coord coord in locatorCoordSector.coordList)
                    {
                        foreach (Coord coordSubsector in coordListSubsector)
                        {
                            double dist = mapInfoService.CalculateDistance((double)coord.Lat * mapInfoService.d2r, (double)coord.Lng * mapInfoService.d2r, (double)coordSubsector.Lat * mapInfoService.d2r, (double)coordSubsector.Lng * mapInfoService.d2r, mapInfoService.R);
                            if (dist < CurrentDist && dist > 1)
                            {
                                coord.Lat = coordSubsector.Lat;
                                coord.Lng = coordSubsector.Lng;
                                break;
                            }
                        }
                    }

                    sb.AppendLine(@"	<Placemark>");
                    sb.AppendLine(@"		<name>" + sector + "</name>");
                    sb.AppendLine(@"		<Polygon>");
                    sb.AppendLine(@"            <outerBoundaryIs>");
                    sb.AppendLine(@"			     <LinearRing>");
                    sb.AppendLine(@"			         <coordinates>");
                    foreach (Coord coord2 in locatorCoordSector.coordList)
                    {
                        sb.AppendLine(@"" + coord2.Lng + "," + coord2.Lat + ",0 ");
                    }
                    sb.AppendLine(@"			        </coordinates>");
                    sb.AppendLine(@"			    </LinearRing>");
                    sb.AppendLine(@"            </outerBoundaryIs>");
                    sb.AppendLine(@"		</Polygon>");
                    sb.AppendLine(@"	</Placemark>");
                }


            }

            sb.AppendLine(@"    </Folder>");
            sb.AppendLine(@"</Document>");
            sb.AppendLine(@"</kml>");

            richTextBoxStatus.Text = sb.ToString();

            richTextBoxStatus.SaveFile(@"C:\DataTool\ImportByFunction\Data_inputs\NL Sectors2.kml", RichTextBoxStreamType.PlainText);

        }

        private void CreateArea()
        {
            List<LocatorCoord> locatorCoordSubsectorsList = new List<LocatorCoord>();
            List<LocatorCoord> locatorCoordSectorsList = new List<LocatorCoord>();
            List<LocatorCoord> locatorCoordAreasList = new List<LocatorCoord>();

            LoadNLSubsectorsFinal(locatorCoordSubsectorsList);
            LoadNLSectorsFinal(locatorCoordSectorsList);
            LoadNLAreasFinal(locatorCoordAreasList);

            List<string> AreaList = locatorCoordSubsectorsList.Select(c => c.Locator.Substring(0, 5)).Distinct().ToList();

            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            double CurrentDist = 400.0D;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine(@"<Document>");
            sb.AppendLine(@"	<name>NL Areas Final.kml</name>");
            sb.AppendLine(@"    <Folder>");
            sb.AppendLine(@"	<name>NL Areas Final</name>");


            foreach (string area in AreaList)
            {
                List<Coord> coordListSubsector = new List<Coord>();
                foreach (LocatorCoord locatorCoordSubsector in locatorCoordSubsectorsList.Where(c => c.Locator.StartsWith(area)).OrderBy(c => c.Locator))
                {
                    foreach (Coord coord in locatorCoordSubsector.coordList)
                    {
                        coordListSubsector.Add(coord);
                    }
                }

                lblStatus.Text = area;
                Application.DoEvents();

                foreach (LocatorCoord locatorCoordArea in locatorCoordAreasList.Where(c => c.Locator.StartsWith(area)))
                {
                    foreach (Coord coord in locatorCoordArea.coordList)
                    {
                        foreach (Coord coordSubsector in coordListSubsector)
                        {
                            double dist = mapInfoService.CalculateDistance((double)coord.Lat * mapInfoService.d2r, (double)coord.Lng * mapInfoService.d2r, (double)coordSubsector.Lat * mapInfoService.d2r, (double)coordSubsector.Lng * mapInfoService.d2r, mapInfoService.R);
                            if (dist < CurrentDist && dist > 1)
                            {
                                coord.Lat = coordSubsector.Lat;
                                coord.Lng = coordSubsector.Lng;
                                break;
                            }
                        }
                    }

                    sb.AppendLine(@"	<Placemark>");
                    sb.AppendLine(@"		<name>" + area + "</name>");
                    sb.AppendLine(@"		<Polygon>");
                    sb.AppendLine(@"            <outerBoundaryIs>");
                    sb.AppendLine(@"			     <LinearRing>");
                    sb.AppendLine(@"			         <coordinates>");
                    foreach (Coord coord2 in locatorCoordArea.coordList)
                    {
                        sb.AppendLine(@"" + coord2.Lng + "," + coord2.Lat + ",0 ");
                    }
                    sb.AppendLine(@"			        </coordinates>");
                    sb.AppendLine(@"			    </LinearRing>");
                    sb.AppendLine(@"            </outerBoundaryIs>");
                    sb.AppendLine(@"		</Polygon>");
                    sb.AppendLine(@"	</Placemark>");
                }


            }

            sb.AppendLine(@"    </Folder>");
            sb.AppendLine(@"</Document>");
            sb.AppendLine(@"</kml>");

            richTextBoxStatus.Text = sb.ToString();

            richTextBoxStatus.SaveFile(@"C:\DataTool\ImportByFunction\Data_inputs\NL Areas2.kml", RichTextBoxStreamType.PlainText);
        }

        private void LoadNLSubsectorsFinal(List<LocatorCoord> locatorCoordList)
        {
            FileInfo fi = new FileInfo(@"C:\DataTool\ImportByFunction\Data_inputs\NL Subsectors Final.kml");
            XmlDocument doc = new XmlDocument();
            doc.Load(fi.FullName);


            XmlNode StartNode = doc.ChildNodes[1].ChildNodes[0].ChildNodes[4];
            int TotalCount = StartNode.ChildNodes.Count;
            foreach (XmlNode n in StartNode.ChildNodes)
            {
                string Locator = "";
                string CoordStr = "";
                List<Coord> coordList = new List<Coord>();
                if (n.Name == "Placemark")
                {
                    foreach (XmlNode n1 in n.ChildNodes)
                    {
                        if (n1.Name == "name")
                        {
                            Locator = n1.InnerText;
                        }
                        else if (n1.Name == "Polygon")
                        {
                            foreach (XmlNode n2 in n1.ChildNodes[0].ChildNodes[0].ChildNodes)
                            {
                                if (n2.Name == "coordinates")
                                {
                                    CoordStr = n2.InnerText.Trim();

                                    string[] strArr = CoordStr.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                    int Ordinal = 0;
                                    foreach (string s in strArr)
                                    {
                                        Ordinal += 1;
                                        string[] coorArr = s.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                        coordList.Add(new Coord() { Lng = float.Parse(coorArr[0]), Lat = float.Parse(coorArr[1]), Ordinal = Ordinal });
                                    }
                                }
                            }
                        }
                    }
                    Coord coordMoy = new Coord();
                    coordMoy.Lat = (from c in coordList select c).Average(c => c.Lat);
                    coordMoy.Lng = (from c in coordList select c).Average(c => c.Lng);

                    locatorCoordList.Add(new LocatorCoord() { Locator = Locator, coord = coordMoy, coordList = coordList });
                }
                //richTextBoxStatus.AppendText(n.Name + "\r\n");
            }

        }
        private void LoadNLSectorsFinal(List<LocatorCoord> locatorCoordList)
        {
            FileInfo fi = new FileInfo(@"C:\DataTool\ImportByFunction\Data_inputs\NL Sectors Final.kml");
            XmlDocument doc = new XmlDocument();
            doc.Load(fi.FullName);


            XmlNode StartNode = doc.ChildNodes[1].ChildNodes[0].ChildNodes[4];
            int TotalCount = StartNode.ChildNodes.Count;
            foreach (XmlNode n in StartNode.ChildNodes)
            {
                string Locator = "";
                string CoordStr = "";
                List<Coord> coordList = new List<Coord>();
                if (n.Name == "Placemark")
                {
                    foreach (XmlNode n1 in n.ChildNodes)
                    {
                        if (n1.Name == "name")
                        {
                            Locator = n1.InnerText;
                        }
                        else if (n1.Name == "Polygon")
                        {
                            foreach (XmlNode n2 in n1.ChildNodes[0].ChildNodes[0].ChildNodes)
                            {
                                if (n2.Name == "coordinates")
                                {
                                    CoordStr = n2.InnerText.Trim();

                                    string[] strArr = CoordStr.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                    int Ordinal = 0;
                                    foreach (string s in strArr)
                                    {
                                        Ordinal += 1;
                                        string[] coorArr = s.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                        coordList.Add(new Coord() { Lng = float.Parse(coorArr[0]), Lat = float.Parse(coorArr[1]), Ordinal = Ordinal });
                                    }
                                }
                            }
                        }
                    }
                    Coord coordMoy = new Coord();
                    coordMoy.Lat = (from c in coordList select c).Average(c => c.Lat);
                    coordMoy.Lng = (from c in coordList select c).Average(c => c.Lng);

                    locatorCoordList.Add(new LocatorCoord() { Locator = Locator, coord = coordMoy, coordList = coordList });
                }
                //richTextBoxStatus.AppendText(n.Name + "\r\n");
            }

        }
        private void LoadNLAreasFinal(List<LocatorCoord> locatorCoordList)
        {
            FileInfo fi = new FileInfo(@"C:\DataTool\ImportByFunction\Data_inputs\NL Areas Final.kml");
            XmlDocument doc = new XmlDocument();
            doc.Load(fi.FullName);


            XmlNode StartNode = doc.ChildNodes[1].ChildNodes[0].ChildNodes[4];
            int TotalCount = StartNode.ChildNodes.Count;
            foreach (XmlNode n in StartNode.ChildNodes)
            {
                string Locator = "";
                string CoordStr = "";
                List<Coord> coordList = new List<Coord>();
                if (n.Name == "Placemark")
                {
                    foreach (XmlNode n1 in n.ChildNodes)
                    {
                        if (n1.Name == "name")
                        {
                            Locator = n1.InnerText;
                        }
                        else if (n1.Name == "Polygon")
                        {
                            foreach (XmlNode n2 in n1.ChildNodes[0].ChildNodes[0].ChildNodes)
                            {
                                if (n2.Name == "coordinates")
                                {
                                    CoordStr = n2.InnerText.Trim();

                                    string[] strArr = CoordStr.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                    int Ordinal = 0;
                                    foreach (string s in strArr)
                                    {
                                        Ordinal += 1;
                                        string[] coorArr = s.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                        coordList.Add(new Coord() { Lng = float.Parse(coorArr[0]), Lat = float.Parse(coorArr[1]), Ordinal = Ordinal });
                                    }
                                }
                            }
                        }
                    }
                    Coord coordMoy = new Coord();
                    coordMoy.Lat = (from c in coordList select c).Average(c => c.Lat);
                    coordMoy.Lng = (from c in coordList select c).Average(c => c.Lng);

                    locatorCoordList.Add(new LocatorCoord() { Locator = Locator, coord = coordMoy, coordList = coordList });
                }
                //richTextBoxStatus.AppendText(n.Name + "\r\n");
            }

        }

        private void CreateFinal()
        {
            FileInfo fi = new FileInfo(@"C:\DataTool\ImportByFunction\Data_inputs\Newfoundland subsectors.kml");
            XmlDocument doc = new XmlDocument();
            doc.Load(fi.FullName);

            List<LocatorCoord> locatorCoordList = new List<LocatorCoord>();

            XmlNode StartNode = doc.ChildNodes[1].ChildNodes[0].ChildNodes[4];
            int TotalCount = StartNode.ChildNodes.Count;
            foreach (XmlNode n in StartNode.ChildNodes)
            {
                string Locator = "";
                string CoordStr = "";
                List<Coord> coordList = new List<Coord>();
                if (n.Name == "Placemark")
                {
                    foreach (XmlNode n1 in n.ChildNodes)
                    {
                        if (n1.Name == "description")
                        {
                            string ElemText = n1.InnerText;

                            int posStart = ElemText.IndexOf("NL-");

                            Locator = ElemText.Substring(ElemText.IndexOf("NL-"), 13);

                        }
                        else if (n1.Name == "LineString")
                        {
                            foreach (XmlNode n2 in n1.ChildNodes)
                            {
                                if (n2.Name == "coordinates")
                                {
                                    CoordStr = n2.InnerText.Trim();

                                    string[] strArr = CoordStr.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                    int Ordinal = 0;
                                    foreach (string s in strArr)
                                    {
                                        Ordinal += 1;
                                        string[] coorArr = s.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                        coordList.Add(new Coord() { Lng = float.Parse(coorArr[0]), Lat = float.Parse(coorArr[1]), Ordinal = Ordinal });
                                    }
                                }
                            }
                        }
                    }
                    Coord coordMoy = new Coord();
                    coordMoy.Lat = (from c in coordList select c).Average(c => c.Lat);
                    coordMoy.Lng = (from c in coordList select c).Average(c => c.Lng);

                    MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

                    double Area = mapInfoService.CalculateAreaOfPolygon(coordList);

                    if (Area < 0)
                    {
                        List<Coord> coordListReverse = new List<Coord>();
                        for (int i = coordList.Count - 1; i > -1; i--)
                        {
                            coordListReverse.Add(coordList[i]);
                        }

                        coordList = coordListReverse;
                    }

                    locatorCoordList.Add(new LocatorCoord() { Locator = Locator, coord = coordMoy, coordList = coordList });
                }
                //richTextBoxStatus.AppendText(n.Name + "\r\n");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine(@"<Document>");
            sb.AppendLine(@"	<name>NL Subsectors_Final.kml</name>");
            sb.AppendLine(@"    <Folder>");
            sb.AppendLine(@"	    <name>NL Subsectors Final</name>");
            foreach (LocatorCoord locatorCoord in locatorCoordList.OrderBy(c => c.Locator))
            {
                sb.AppendLine(@"	<Placemark>");
                sb.AppendLine(@"		<name>" + locatorCoord.Locator + "</name>");
                sb.AppendLine(@"		<Polygon>");
                sb.AppendLine(@"            <outerBoundaryIs>");
                sb.AppendLine(@"			     <LinearRing>");
                sb.AppendLine(@"			         <coordinates>");
                for (int i = 0, count = locatorCoord.coordList.Count; i < count; i++)
                {
                    sb.Append(locatorCoord.coordList[i].Lng + "," + locatorCoord.coordList[i].Lat + ",0 ");
                }
                sb.AppendLine(@"			        </coordinates>");
                sb.AppendLine(@"			    </LinearRing>");
                sb.AppendLine(@"            </outerBoundaryIs>");
                sb.AppendLine(@"		</Polygon>");
                sb.AppendLine(@"	</Placemark>");
            }

            sb.AppendLine(@"    </Folder>");
            sb.AppendLine(@"</Document>");
            sb.AppendLine(@"</kml>");

            richTextBoxStatus.Text = sb.ToString();

            richTextBoxStatus.SaveFile(@"C:\DataTool\ImportByFunction\Data_inputs\NL Subsectors.kml", RichTextBoxStreamType.PlainText);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0, count = Enum.GetNames(typeof(TreatmentTypeEnum)).Count(); i < count; i++)
            {
                comboBox1.Items.Add(((TreatmentTypeEnum)i).ToString());
            }
        }

        private void RecalculateTVItemStat_Click(object sender, EventArgs e)
        {

            List<TVItem> tvItemList = new List<TVItem>() { new TVItem() };
            int take = 100;
            int CurrentTVItemID = int.Parse(textBoxRecalculateTVItemStatStart.Text);
            while (tvItemList.Count > 0)
            {
                TVItemStatService tvItemStatService = new TVItemStatService(LanguageEnum.en, user);

                tvItemList = (from c in tvItemStatService.db.TVItems
                              where c.TVItemID > CurrentTVItemID
                              orderby c.TVItemID
                              select c).Take(take).ToList();

                foreach (TVItem tvItem in tvItemList)
                {
                    lblStatus.Text = tvItem.TVItemID.ToString();
                    lblStatus.Refresh();
                    Application.DoEvents();

                    string retStr = tvItemStatService.SetTVItemStatForTVItemID(tvItem.TVItemID);
                    if (!string.IsNullOrWhiteSpace(retStr))
                    {
                        richTextBoxStatus.AppendText("Error: [" + retStr + "]\r\n");
                        return;
                    }

                    CurrentTVItemID = tvItem.TVItemID;
                    textBoxRecalculateTVItemStatStart.Text = CurrentTVItemID.ToString();
                }
            }
        }
        private void DoChildren(TVItemService tvItemService, PolSourceObsInfoChild polSourceObsInfoChild, string tabs)
        {
            if ((int)(polSourceObsInfoChild.PolSourceObsInfo) < 9000)
            {
                richTextBoxStatus.AppendText(tabs + "(" + polSourceObsInfoChild.PolSourceObsInfo + ") " + _BaseEnumService.GetEnumText_PolSourceObsInfoEnum(polSourceObsInfoChild.PolSourceObsInfo) + "\r\n");
            }
            tabs = tabs + "\t";
            //foreach (PolSourceObsInfoEnum polSourceObsInfo in polSourceObsInfoChild.PolSourceObsInfoChildStart)
            //{
            //    List<PolSourceObsInfoChild> polSourceObsInfoChildrenList = tvItemService.polSourceObsInfoChildList.Where(c => c.PolSourceObsInfo > polSourceObsInfo && c.PolSourceObsInfo < (polSourceObsInfo + 99)).ToList();
            //    foreach (PolSourceObsInfoChild polSourceObsInfoChildren2 in polSourceObsInfoChildrenList)
            //    {
            //        DoChildren(tvItemService, polSourceObsInfoChildren2, tabs);
            //    }
            //}
        }

        private void butPolSourceTree_Click(object sender, EventArgs e)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

            List<PolSourceObsInfoChild> polSourceObsInfoChildrenList = tvItemService.polSourceObsInfoChildList.Where(c => c.PolSourceObsInfo > PolSourceObsInfoEnum.Start && c.PolSourceObsInfo < (PolSourceObsInfoEnum.Start + 99)).ToList();
            foreach (PolSourceObsInfoChild polSourceObsInfoChildren in polSourceObsInfoChildrenList)
            {
                DoChildren(tvItemService, polSourceObsInfoChildren, "");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);
            MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);
            MWQMSiteStartEndDateService mwqmSiteStartEndDateService = new MWQMSiteStartEndDateService(LanguageEnum.en, user);
            UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);
            ClimateSiteService climateSiteService = new ClimateSiteService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                richTextBoxStatus.AppendText("Error: " + tvItemModelRoot.Error);
                return;
            }

            List<string> provinceList = new List<string>() { "New Brunswick" };

            foreach (string prov in provinceList)
            {
                TVItemModel tvItemModelProvince = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, prov, TVTypeEnum.Province);
                if (!string.IsNullOrWhiteSpace(tvItemModelProvince.Error))
                {
                    richTextBoxStatus.AppendText("Error: " + tvItemModelProvince.Error);
                    return;
                }

                List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProvince.TVItemID, TVTypeEnum.Subsector);
                if (tvItemModelSubsectorList.Count == 0)
                {
                    richTextBoxStatus.AppendText("Error: Could not find any Subsectors for province " + tvItemModelProvince.TVText);
                    return;
                }

                int count = 0;
                foreach (TVItemModel tvItemModelSubsector in tvItemModelSubsectorList)
                {
                    List<YearMinMaxDate> yearMinMaxDateList = new List<YearMinMaxDate>();

                    count += 1;
                    if (count > 5)
                        return;

                    List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSubsector.TVItemID, TVTypeEnum.MWQMSite);

                    foreach (TVItemModel tvItemModelMWQMSite in tvItemModelMWQMSiteList)
                    {
                        lblStatus.Text = tvItemModelSubsector.TVText + " --- " + " MWQMSite " + tvItemModelMWQMSite.TVText;
                        lblStatus.Refresh();
                        Application.DoEvents();

                        List<MWQMSampleModel> mwqmSampleModelList = mwqmSampleService.GetMWQMSampleModelListWithMWQMSiteTVItemIDDB(tvItemModelMWQMSite.TVItemID);
                        foreach (MWQMSampleModel mwqmSampleModel in mwqmSampleModelList.OrderBy(c => c.SampleDateTime_Local).ToList())
                        {
                            YearMinMaxDate yearMinMaxDate = (from c in yearMinMaxDateList
                                                             where c.Year == mwqmSampleModel.SampleDateTime_Local.Year
                                                             select c).FirstOrDefault();

                            if (yearMinMaxDate == null)
                            {
                                YearMinMaxDate yearMinMaxDateNew = new YearMinMaxDate()
                                {
                                    Year = mwqmSampleModel.SampleDateTime_Local.Year,
                                    MinDateTime = mwqmSampleModel.SampleDateTime_Local,
                                    MaxDateTime = mwqmSampleModel.SampleDateTime_Local,
                                };

                                yearMinMaxDateList.Add(yearMinMaxDateNew);
                            }
                            else
                            {
                                if (yearMinMaxDate.MinDateTime > mwqmSampleModel.SampleDateTime_Local)
                                {
                                    yearMinMaxDate.MinDateTime = mwqmSampleModel.SampleDateTime_Local;
                                }
                                if (yearMinMaxDate.MaxDateTime < mwqmSampleModel.SampleDateTime_Local)
                                {
                                    yearMinMaxDate.MaxDateTime = mwqmSampleModel.SampleDateTime_Local;
                                }
                            }
                        }
                    }

                    List<MapInfoPointModel> mapInfoPointListSubsector = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelSubsector.TVItemID, TVTypeEnum.Subsector, MapInfoDrawTypeEnum.Point);
                    if (mapInfoPointListSubsector.Count == 0)
                    {
                        richTextBoxStatus.AppendText("Could not find MapInfoPoint for subsector [" + tvItemModelSubsector.TVText + "]");
                        return;
                    }

                    List<UseOfSiteModel> useOfSiteModelList = useOfSiteService.GetUseOfSiteModelListWithSiteTypeAndSubsectorTVItemIDDB(SiteTypeEnum.Climate, tvItemModelSubsector.TVItemID);

                    float LookupRadius = 100000.0f; // 30 km

                    List<MapInfoModel> mapInfoModelList = new List<MapInfoModel>();
                    List<ClimateSiteModel> climateSiteModelListWithHourly = new List<ClimateSiteModel>();
                    List<ClimateSiteModel> climateSiteModelListWithDaily = new List<ClimateSiteModel>();

                    while (mapInfoModelList.Count == 0)
                    {
                        mapInfoModelList = mapInfoService.GetMapInfoModelWithinCircleWithTVTypeAndMapInfoDrawTypeDB((float)mapInfoPointListSubsector[0].Lat, (float)mapInfoPointListSubsector[0].Lng, 30000f, TVTypeEnum.ClimateSite, MapInfoDrawTypeEnum.Point);

                        foreach (MapInfoModel mapInfoModel in mapInfoModelList)
                        {
                            ClimateSiteModel climateSiteModel = climateSiteService.GetClimateSiteModelWithClimateSiteTVItemIDDB(mapInfoModel.TVItemID);
                            if (!string.IsNullOrWhiteSpace(climateSiteModel.Error))
                            {
                                richTextBoxStatus.AppendText("Could not find ClimateSiteModel [" + climateSiteModel.Error + "]");
                                return;
                            }
                            if (climateSiteModel.HourlyStartDate_Local != null)
                            {
                                climateSiteModelListWithHourly.Add(climateSiteModel);
                            }
                            if (climateSiteModel.DailyStartDate_Local != null)
                            {
                                climateSiteModelListWithDaily.Add(climateSiteModel);
                            }
                        }

                        LookupRadius += 10000;
                    }

                    foreach (YearMinMaxDate yearMinMaxDate in yearMinMaxDateList)
                    {
                        List<ClimateSiteModel> climateSiteForDailyModelListUsed = new List<ClimateSiteModel>();
                        List<ClimateSiteModel> climateSiteForHourlyModelListUsed = new List<ClimateSiteModel>();
                        foreach (ClimateSiteModel climateSiteModel in climateSiteModelListWithDaily)
                        {
                            if (climateSiteForDailyModelListUsed.Count > 2)
                                break;

                            if (climateSiteModel.DailyStartDate_Local.Value.Year <= yearMinMaxDate.Year && climateSiteModel.DailyEndDate_Local.Value.Year >= yearMinMaxDate.Year)
                            {
                                ClimateDataValueModel climateDataValueModel = GetDailyValues(climateSiteModel, yearMinMaxDate);
                                if (!string.IsNullOrWhiteSpace(climateDataValueModel.Error))
                                {
                                    richTextBoxStatus.AppendText(climateDataValueModel.Error + "\r\n");
                                    return;
                                }
                                if (climateDataValueModel.Rainfall_mm != null)
                                {
                                    climateSiteForDailyModelListUsed.Add(climateSiteModel);
                                }
                                if (climateDataValueModel.HourlyValues != null)
                                {
                                    climateSiteForHourlyModelListUsed.Add(climateSiteModel);
                                }
                            }
                        }

                        if (climateSiteForHourlyModelListUsed.Count == 0)
                        {
                            foreach (ClimateSiteModel climateSiteModel in climateSiteModelListWithHourly)
                            {
                                if (climateSiteForHourlyModelListUsed.Count > 0)
                                    break;

                                if (climateSiteModel.DailyStartDate_Local.Value.Year <= yearMinMaxDate.Year && climateSiteModel.DailyEndDate_Local.Value.Year >= yearMinMaxDate.Year)
                                {
                                    ClimateDataValueModel climateDataValueModel = GetDailyValues(climateSiteModel, yearMinMaxDate);
                                    if (!string.IsNullOrWhiteSpace(climateDataValueModel.Error))
                                    {
                                        richTextBoxStatus.AppendText(climateDataValueModel.Error + "\r\n");
                                        return;
                                    }
                                    if (climateDataValueModel.Rainfall_mm != null)
                                    {
                                        climateSiteForDailyModelListUsed.Add(climateSiteModel);
                                    }
                                    if (climateDataValueModel.HourlyValues != null)
                                    {
                                        climateSiteForHourlyModelListUsed.Add(climateSiteModel);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private ClimateDataValueModel GetDailyValues(ClimateSiteModel climateSiteModel, YearMinMaxDate yearMinMaxDate)
        {
            ClimateDataValueService climateDataValueService = new ClimateDataValueService(LanguageEnum.en, user);

            List<string> FullProvList = new List<string>() { "BRITISH COLUMBIA", "NEW BRUNSWICK", "NEWFOUNDLAND", "NOVA SCOTIA", "PRINCE EDWARD ISLAND", "QUEBEC" };
            List<string> ShortProvList = new List<string>() { "BC", "NB", "NL", "NS", "PE", "QC" };
            string httpStr = "";

            DateTime StartDate = yearMinMaxDate.MinDateTime.AddDays(-10);
            DateTime EndDate = yearMinMaxDate.MaxDateTime.AddDays(10);

            using (WebClient webClient = new WebClient())
            {
                WebProxy webProxy = new WebProxy();
                webClient.Proxy = webProxy;
                httpStr = webClient.DownloadString(new Uri("http://climate.weather.gc.ca/climateData/bulkdata_e.html?format=csv&stationID=" + climateSiteModel.ECDBID.ToString() + "&Year=" + yearMinMaxDate.Year.ToString() + "&Month=1&Day=1&timeframe=2&submit=Download+Data"));
                if (httpStr.Length > 0)
                {
                    if (httpStr.Substring(0, "\"Station Name".Length) == "\"Station Name")
                    {
                        httpStr = httpStr.Replace("\"", "").Replace("\n", "\r\n");
                    }
                }
                else
                {
                    return new ClimateDataValueModel() { Error = "HTTP did not return anything." };
                }
            }

            using (TextReader tr = new StringReader(httpStr))
            {

                int countLine = 0;
                bool HasMore = true;
                while (HasMore)
                {
                    countLine += 1;
                    string lineStr = tr.ReadLine();
                    List<string> lineValueArr = lineStr.Split(",".ToCharArray(), StringSplitOptions.None).ToList();
                    if (countLine == 1)
                    {
                        if (lineValueArr[0] != "Station Name")
                        {
                            return new ClimateDataValueModel() { Error = "Station Name not found on line [" + countLine + "]" };
                        }
                        if (lineValueArr[1] != climateSiteModel.ClimateSiteName)
                        {
                            return new ClimateDataValueModel() { Error = "Station Name not equal to [" + climateSiteModel.ClimateSiteName + "] at line [" + countLine + "]" };
                        }

                    }
                    if (countLine == 2)
                    {
                        if (lineValueArr[0] != "Province")
                        {
                            return new ClimateDataValueModel() { Error = "Province not found on line [" + countLine + "]" };
                        }
                        if (!FullProvList.Contains(lineValueArr[1]))
                        {
                            return new ClimateDataValueModel() { Error = "Province could not be found [" + lineValueArr[1] + "] at line [" + countLine + "]" };
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            if (lineValueArr[1] == FullProvList[i])
                            {
                                if (climateSiteModel.Province != ShortProvList[i])
                                {
                                    return new ClimateDataValueModel() { Error = "Province [" + ShortProvList[i] + "] not equal to [" + climateSiteModel.Province + " at line [" + countLine + "]" };
                                }
                            }
                        }
                    }
                    if (countLine == 6)
                    {
                        if (lineValueArr[0] != "Climate Identifier")
                        {
                            return new ClimateDataValueModel() { Error = "Climate Identifier not found on line [" + countLine + "]" };
                        }
                        if (lineValueArr[1].Length > 0)
                        {
                            if (lineValueArr[1] != climateSiteModel.ClimateID)
                            {
                                return new ClimateDataValueModel() { Error = "Climate Identifier not equal to [" + climateSiteModel.ClimateID + "] at line [" + countLine + "]" };
                            }
                        }
                    }
                    if (lineValueArr[0].Contains("-"))
                    {
                        if (lineValueArr[0].Substring(4, 1) == "-")
                        {
                            if (lineValueArr.Count != 27)
                            {
                                return new ClimateDataValueModel() { Error = "lineValueArr.Count not equal to 26 it is [" + lineValueArr.Count + "] at line [" + countLine + "]" };
                            }

                            int Year = int.Parse(lineValueArr[1]);
                            int Month = int.Parse(lineValueArr[2]);
                            int Day = int.Parse(lineValueArr[3]);

                            if (Year == 0 || Month == 0 || Day == 0)
                            {
                                return new ClimateDataValueModel() { Error = "Year not correct [" + Year + "-" + Month + "-" + Day + "] at line [" + countLine + "]" };
                            }

                            DateTime LineDate = new DateTime(Year, Month, Day);

                            if (StartDate > LineDate || EndDate < LineDate)
                            {
                                //continue;
                            }

                            float? MaxTemp = null;
                            if (lineValueArr[5].Length > 0)
                            {
                                MaxTemp = float.Parse(lineValueArr[5]);
                            }
                            float? MinTemp = null;
                            if (lineValueArr[7].Length > 0)
                            {
                                MinTemp = float.Parse(lineValueArr[7]);
                            }
                            float? MeanTemp = null;
                            if (lineValueArr[9].Length > 0)
                            {
                                MeanTemp = float.Parse(lineValueArr[9]);
                            }
                            float? HeatDegDays = null;
                            if (lineValueArr[11].Length > 0)
                            {
                                HeatDegDays = float.Parse(lineValueArr[11]);
                            }
                            float? CoolDegDays = null;
                            if (lineValueArr[13].Length > 0)
                            {
                                CoolDegDays = float.Parse(lineValueArr[13]);
                            }
                            float? TotalRain = null;
                            if (lineValueArr[15].Length > 0)
                            {
                                TotalRain = float.Parse(lineValueArr[15]);
                            }
                            float? TotalSnow = null;
                            if (lineValueArr[17].Length > 0)
                            {
                                TotalSnow = float.Parse(lineValueArr[17]);
                            }
                            float? TotalPrecip = null;
                            if (lineValueArr[19].Length > 0)
                            {
                                TotalPrecip = float.Parse(lineValueArr[19]);
                            }
                            float? SnowOnGround = null;
                            if (lineValueArr[21].Length > 0)
                            {
                                SnowOnGround = float.Parse(lineValueArr[21]);
                            }
                            float? DirMaxGust = null;
                            if (lineValueArr[23].Length > 0)
                            {
                                DirMaxGust = float.Parse(lineValueArr[23]);
                            }
                            float? SpdMaxGust = null;
                            if (lineValueArr[25].Length > 0)
                            {
                                if (lineValueArr[25].Substring(0, 1) == "<")
                                {
                                    SpdMaxGust = float.Parse(lineValueArr[25].Substring(1));
                                }
                                else
                                {
                                    SpdMaxGust = float.Parse(lineValueArr[25]);
                                }
                            }

                            ClimateDataValueModel climateDataValueModelNew = new ClimateDataValueModel()
                            {
                                ClimateSiteID = climateSiteModel.ClimateSiteID,
                                CoolDegDays_C = CoolDegDays,
                                DateTime_Local = LineDate,
                                DirMaxGust_0North = DirMaxGust,
                                HeatDegDays_C = HeatDegDays,
                                HourlyValues = "",
                                Keep = true,
                                MaxTemp_C = MaxTemp,
                                MinTemp_C = MinTemp,
                                Rainfall_mm = TotalRain,
                                RainfallEntered_mm = null,
                                Snow_cm = TotalSnow,
                                SnowOnGround_cm = SnowOnGround,
                                SpdMaxGust_kmh = SpdMaxGust,
                                StorageDataType = StorageDataTypeEnum.Archived,
                                TotalPrecip_mm_cm = TotalPrecip,
                            };
                            ClimateDataValueModel climateDataValueModel = climateDataValueService.GetClimateDataValueModelExitDB(climateDataValueModelNew);
                            if (!string.IsNullOrWhiteSpace(climateDataValueModel.Error))
                            {
                                // does not exist
                                if (climateSiteModel.HourlyStartDate_Local <= LineDate && climateSiteModel.HourlyEndDate_Local >= LineDate)
                                {
                                    StringBuilder hourlyValues = new StringBuilder();
                                    string retStr = GetHourlyValues(climateSiteModel, LineDate, hourlyValues);
                                    climateDataValueModelNew.HourlyValues = hourlyValues.ToString();
                                }
                                //climateDataValueModel = climateDataValueService.PostAddClimateDataValueDB(climateDataValueModelNew);
                                //if (!string.IsNullOrWhiteSpace(climateDataValueModel.Error))
                                //{
                                //    return "Could not add ClimateDataValue [" + climateDataValueModel.Error + "]";
                                //}
                            }
                            else
                            {
                                // already exist
                                climateDataValueModelNew.ClimateDataValueID = climateDataValueModel.ClimateDataValueID;
                                if (climateSiteModel.HourlyStartDate_Local <= LineDate && climateSiteModel.HourlyEndDate_Local >= LineDate)
                                {
                                    StringBuilder hourlyValues = new StringBuilder();
                                    string retStr = GetHourlyValues(climateSiteModel, LineDate, hourlyValues);
                                    climateDataValueModelNew.HourlyValues = hourlyValues.ToString();
                                }
                                climateDataValueModelNew.RainfallEntered_mm = climateDataValueModel.RainfallEntered_mm;
                                //climateDataValueModel = climateDataValueService.PostUpdateClimateDataValueDB(climateDataValueModelNew);
                                //if (!string.IsNullOrWhiteSpace(climateDataValueModel.Error))
                                //{
                                //    return "Could not update ClimateDataValue [" + climateDataValueModel.Error + "]";
                                //}
                            }

                        }
                    }
                }
            }

            return new ClimateDataValueModel();
        }

        public string GetHourlyValues(ClimateSiteModel climateSiteModel, DateTime CurrentDate, StringBuilder hourlyValues)
        {
            List<string> FullProvList = new List<string>() { "BRITISH COLUMBIA", "NEW BRUNSWICK", "NEWFOUNDLAND", "NOVA SCOTIA", "PRINCE EDWARD ISLAND", "QUEBEC" };
            List<string> ShortProvList = new List<string>() { "BC", "NB", "NL", "NS", "PE", "QC" };

            hourlyValues.Clear();

            string httpStr = "";

            hourlyValues.AppendLine("Hour,Temp,DewPoint,RelHum,WindDir,WindSpd,StnPress,Humidx,WindChill");

            using (WebClient webClient = new WebClient())
            {
                WebProxy webProxy = new WebProxy();
                webClient.Proxy = webProxy;
                httpStr = webClient.DownloadString(new Uri("http://climate.weather.gc.ca/climateData/bulkdata_e.html?format=csv&stationID=" + climateSiteModel.ECDBID.ToString() + "&Year=" + CurrentDate.Year.ToString() + "&Month=" + CurrentDate.Month.ToString() + "&Day=1&timeframe=1&submit=Download+Data"));
                if (httpStr.Length > 0)
                {
                    if (httpStr.Substring(0, "\"Station Name".Length) == "\"Station Name")
                    {
                        httpStr = httpStr.Replace("\"", "").Replace("\n", "\r\n");
                    }
                }
                else
                {
                    return "HTTP did not return anything.";
                }
            }

            using (TextReader tr = new StringReader(httpStr))
            {
                int countLine = 0;
                bool HasMore = true;
                while (HasMore)
                {
                    countLine += 1;
                    string lineStr = tr.ReadLine();
                    List<string> lineValueArr = lineStr.Split(",".ToCharArray(), StringSplitOptions.None).ToList();
                    if (countLine == 1)
                    {
                        if (lineValueArr[0] != "Station Name")
                        {
                            return "Station Name in hourly not found on line [" + countLine + "]";
                        }
                        if (lineValueArr[1] != climateSiteModel.ClimateSiteName)
                        {
                            return "Station Name not equal to [" + climateSiteModel.ClimateSiteName + "] at line [" + countLine + "]";
                        }

                    }
                    if (countLine == 2)
                    {
                        if (lineValueArr[0] != "Province")
                        {
                            return "Province not found on line [" + countLine + "]";
                        }
                        if (!FullProvList.Contains(lineValueArr[1]))
                        {
                            return "Province could not be found [" + lineValueArr[1] + "] at line [" + countLine + "]";
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            if (lineValueArr[1] == FullProvList[i])
                            {
                                if (climateSiteModel.Province != ShortProvList[i])
                                {
                                    return "Province [" + ShortProvList[i] + "] not equal to [" + climateSiteModel.Province + " at line [" + countLine + "]";
                                }
                            }
                        }
                    }
                    if (countLine == 6)
                    {
                        if (lineValueArr[0] != "Climate Identifier")
                        {
                            return "Climate Identifier not found on line [" + countLine + "]";
                        }
                        if (lineValueArr[1].Length > 0)
                        {
                            if (lineValueArr[1] != climateSiteModel.ClimateID)
                            {
                                return "Climate Identifier not equal to [" + climateSiteModel.ClimateID + "] at line [" + countLine + "]";
                            }
                        }
                    }
                    if (lineValueArr[0].Contains("-"))
                    {
                        if (lineValueArr[0].Substring(4, 1) == "-")
                        {
                            if (lineValueArr.Count != 25)
                            {
                                return "lineValueArr.Count not equal to 26 it is [" + lineValueArr.Count + "] at line [" + countLine + "]";
                            }

                            int Year = int.Parse(lineValueArr[1]);
                            int Month = int.Parse(lineValueArr[2]);
                            int Day = int.Parse(lineValueArr[3]);

                            if (Year == 0 || Month == 0 || Day == 0)
                            {
                                return "Year not correct [" + Year + "-" + Month + "-" + Day + "] at line [" + countLine + "]";
                            }


                            DateTime LineDate = new DateTime(Year, Month, Day);

                            if (CurrentDate != LineDate)
                            {
                                //continue;
                            }

                            int Hour = -1;
                            if (lineValueArr[4].Length > 2)
                            {
                                Hour = int.Parse(lineValueArr[4].Substring(0, 2));
                            }

                            float Temp_C = -9999.0f;
                            if (lineValueArr[6].Length > 0)
                            {
                                Temp_C = float.Parse(lineValueArr[6]);
                            }
                            float DewPoint_C = -9999.0f;
                            if (lineValueArr[8].Length > 0)
                            {
                                DewPoint_C = float.Parse(lineValueArr[8]);
                            }
                            float RelHum_Perc = -9999.0f;
                            if (lineValueArr[10].Length > 0)
                            {
                                RelHum_Perc = float.Parse(lineValueArr[10]);
                            }
                            float WindDir_10deg = -9999.0f;
                            if (lineValueArr[12].Length > 0)
                            {
                                WindDir_10deg = float.Parse(lineValueArr[12]);
                            }
                            float WindSpd_km_h = -9999.0f;
                            if (lineValueArr[14].Length > 0)
                            {
                                WindSpd_km_h = float.Parse(lineValueArr[14]);
                            }
                            float Visibility_km = -9999.0f;
                            if (lineValueArr[16].Length > 0)
                            {
                                Visibility_km = float.Parse(lineValueArr[16]);
                            }
                            float StnPress_kPa = -9999.0f;
                            if (lineValueArr[18].Length > 0)
                            {
                                StnPress_kPa = float.Parse(lineValueArr[18]);
                            }
                            float Humidx = -9999.0f;
                            if (lineValueArr[20].Length > 0)
                            {
                                Humidx = float.Parse(lineValueArr[20]);
                            }
                            float WindChill = -9999.0f;
                            if (lineValueArr[22].Length > 0)
                            {
                                WindChill = float.Parse(lineValueArr[22]);
                            }

                            hourlyValues.AppendLine(
                                (Hour == -1 ? "" : Hour.ToString()) + "," +
                                (Temp_C == -9999.0f ? "" : Temp_C.ToString()) + "," +
                                (DewPoint_C == -9999.0f ? "" : DewPoint_C.ToString()) + "," +
                                (RelHum_Perc == -9999.0f ? "" : RelHum_Perc.ToString()) + "," +
                                (WindDir_10deg == -9999.0f ? "" : WindDir_10deg.ToString()) + "," +
                                (WindSpd_km_h == -9999.0f ? "" : WindSpd_km_h.ToString()) + "," +
                                (Visibility_km == -9999.0f ? "" : Visibility_km.ToString()) + "," +
                                (StnPress_kPa == -9999.0f ? "" : StnPress_kPa.ToString()) + "," +
                                (Humidx == -9999.0f ? "" : Humidx.ToString()) + "," +
                                (WindChill == -9999.0f ? "" : WindChill.ToString())
                                );
                        }
                    }
                }
            }

            return "";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            FileInfo fi = new FileInfo(@"C:\Users\leblancc\Desktop\ClimateSiteNearBouctouche.kml");

            Coord coordBouctouche = new Coord() { Lat = 46.473544f, Lng = -64.714258f, Ordinal = 0 };

            // Act
            List<MapInfoModel> mapInfoModelList = mapInfoService.GetMapInfoModelWithinCircleWithTVTypeAndMapInfoDrawTypeDB(coordBouctouche.Lat, coordBouctouche.Lng, 200000f, TVTypeEnum.ClimateSite, MapInfoDrawTypeEnum.Point);

            StringBuilder sbKMZ = new StringBuilder();

            sbKMZ.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sbKMZ.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sbKMZ.AppendLine(@"<Document>");
            sbKMZ.AppendLine(@"	<name>" + fi.FullName + "</name>");

            sbKMZ.AppendLine(@"    <StyleMap id=""msn_ylw-pushpin"">");
            sbKMZ.AppendLine(@"		<Pair>");
            sbKMZ.AppendLine(@"			<key>normal</key>");
            sbKMZ.AppendLine(@"			<styleUrl>#sn_ylw-pushpin</styleUrl>");
            sbKMZ.AppendLine(@"		</Pair>");
            sbKMZ.AppendLine(@"		<Pair>");
            sbKMZ.AppendLine(@"			<key>highlight</key>");
            sbKMZ.AppendLine(@"			<styleUrl>#sh_ylw-pushpin</styleUrl>");
            sbKMZ.AppendLine(@"		</Pair>");
            sbKMZ.AppendLine(@"	</StyleMap>");
            sbKMZ.AppendLine(@"	<Style id=""sh_ylw-pushpin"">");
            sbKMZ.AppendLine(@"		<IconStyle>");
            sbKMZ.AppendLine(@"			<scale>1.2</scale>");
            sbKMZ.AppendLine(@"		</IconStyle>");
            sbKMZ.AppendLine(@"		<LineStyle>");
            sbKMZ.AppendLine(@"			<color>ff00ff00</color>");
            sbKMZ.AppendLine(@"			<width>1.5</width>");
            sbKMZ.AppendLine(@"		</LineStyle>");
            sbKMZ.AppendLine(@"		<PolyStyle>");
            sbKMZ.AppendLine(@"			<color>0000ff00</color>");
            sbKMZ.AppendLine(@"		</PolyStyle>");
            sbKMZ.AppendLine(@"	</Style>");
            sbKMZ.AppendLine(@"	<Style id=""sn_ylw-pushpin"">");
            sbKMZ.AppendLine(@"		<LineStyle>");
            sbKMZ.AppendLine(@"			<color>ff00ff00</color>");
            sbKMZ.AppendLine(@"			<width>1.5</width>");
            sbKMZ.AppendLine(@"		</LineStyle>");
            sbKMZ.AppendLine(@"		<PolyStyle>");
            sbKMZ.AppendLine(@"			<color>0000ff00</color>");
            sbKMZ.AppendLine(@"		</PolyStyle>");
            sbKMZ.AppendLine(@"	</Style>");


            sbKMZ.AppendLine(@"	<Folder>");
            sbKMZ.AppendLine(@"	<name>Climate Sites</name>");

            foreach (MapInfoModel mapInfoModel in mapInfoModelList)
            {
                TVItemModel tvItemModel = tvItemService.GetTVItemModelWithTVItemIDDB(mapInfoModel.TVItemID);
                sbKMZ.AppendLine(@"	<Placemark>");
                sbKMZ.AppendLine(@"	<name>" + tvItemModel.TVText + "</name>");
                sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
                sbKMZ.AppendLine(@"	<Point>");
                sbKMZ.AppendLine(@"		<coordinates>" + ((mapInfoModel.LngMax + mapInfoModel.LngMin) / 2).ToString() + "," + ((mapInfoModel.LatMax + mapInfoModel.LatMin) / 2).ToString() + ",0</coordinates>");
                sbKMZ.AppendLine(@"	</Point>");
                sbKMZ.AppendLine(@"	</Placemark>");
            }

            sbKMZ.AppendLine(@" </Folder>");


            sbKMZ.AppendLine(@"</Document>");
            sbKMZ.AppendLine(@"</kml>");

            StreamWriter sw = fi.CreateText();

            sw.Write(sbKMZ.ToString());

            sw.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //string FileName = @"C:\CSSP Latest Code old\DataTool\ImportByFunction\Data_inputs\NL_MWQM_Sites_Classification_2016_Final.xlsx";

            //string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=Excel 12.0";

            //OleDbConnection conn = new OleDbConnection(connectionString);

            //conn.Open();
            //OleDbDataReader reader;
            //OleDbCommand comm = new OleDbCommand("Select * from [Sheet1$];");

            //comm.Connection = conn;
            //reader = comm.ExecuteReader();

            //int CountRead = 0;
            //while (reader.Read())
            //{
            //    CountRead += 1;
            //    if (CountRead < 0)
            //        continue;

            //    Application.DoEvents();

            //    string Subsector = "";
            //    string ID = "";
            //    string MWQMSiteName = "";
            //    string Approved = "";
            //    string ConditionallyApproved = "";
            //    string Restricted = "";
            //    string ConditionallyRestricted = "";
            //    string Prohibited = "";
            //    string Unclassified = "";
            //    //string Comment = "";
            //    //string Lat = "";
            //    //string Lng = "";

            //    // Subsector
            //    if (reader.GetValue(0).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(0).ToString()))
            //    {
            //        Subsector = "";
            //    }
            //    else
            //    {
            //        Subsector = reader.GetValue(0).ToString().Trim();
            //    }

            //    // ID
            //    if (reader.GetValue(1).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(1).ToString()))
            //    {
            //        ID = "";
            //    }
            //    else
            //    {
            //        ID = reader.GetValue(1).ToString().Trim();
            //    }

            //    // MWQMSiteName
            //    if (reader.GetValue(2).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(2).ToString()))
            //    {
            //        MWQMSiteName = "";
            //    }
            //    else
            //    {
            //        MWQMSiteName = reader.GetValue(2).ToString().Trim();
            //    }

            //    // Approved
            //    if (reader.GetValue(3).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(3).ToString()))
            //    {
            //        Approved = "";
            //    }
            //    else
            //    {
            //        Approved = reader.GetValue(3).ToString().Trim();
            //    }

            //    // ConditionallyApproved
            //    if (reader.GetValue(4).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(4).ToString()))
            //    {
            //        ConditionallyApproved = "";
            //    }
            //    else
            //    {
            //        ConditionallyApproved = reader.GetValue(4).ToString().Trim();
            //    }

            //    // Restricted
            //    if (reader.GetValue(5).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(5).ToString()))
            //    {
            //        Restricted = "";
            //    }
            //    else
            //    {
            //        Restricted = reader.GetValue(5).ToString().Trim();
            //    }

            //    // ConditionalyRestricted
            //    if (reader.GetValue(6).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(6).ToString()))
            //    {
            //        ConditionallyRestricted = "";
            //    }
            //    else
            //    {
            //        ConditionallyRestricted = reader.GetValue(6).ToString().Trim();
            //    }

            //    // Prohibited
            //    if (reader.GetValue(7).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(7).ToString()))
            //    {
            //        Prohibited = "";
            //    }
            //    else
            //    {
            //        Prohibited = reader.GetValue(7).ToString().Trim();
            //    }

            //    // Unclassified
            //    if (reader.GetValue(8).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(8).ToString()))
            //    {
            //        Unclassified = "";
            //    }
            //    else
            //    {
            //        Unclassified = reader.GetValue(8).ToString().Trim();
            //    }

            //    //// Comment
            //    //if (reader.GetValue(9).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(9).ToString()))
            //    //{
            //    //    Comment = "";
            //    //}
            //    //else
            //    //{
            //    //    Comment = reader.GetValue(9).ToString().Trim();
            //    //}

            //    //// Lat
            //    //if (reader.GetValue(10).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(10).ToString()))
            //    //{
            //    //    Lat = "";
            //    //}
            //    //else
            //    //{
            //    //    Lat = reader.GetValue(10).ToString().Trim();
            //    //}

            //    //// Lng
            //    //if (reader.GetValue(11).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(11).ToString()))
            //    //{
            //    //    Lng = "";
            //    //}
            //    //else
            //    //{
            //    //    Lng = reader.GetValue(11).ToString().Trim();
            //    //}

            //    richTextBoxStatus.AppendText(CountRead + "\t");
            //    richTextBoxStatus.AppendText(Subsector + "\t");
            //    richTextBoxStatus.AppendText(ID + "\t");
            //    richTextBoxStatus.AppendText(MWQMSiteName + "\t");
            //    richTextBoxStatus.AppendText(Approved + "\t");
            //    richTextBoxStatus.AppendText(ConditionallyApproved + "\t");
            //    richTextBoxStatus.AppendText(Restricted + "\t");
            //    richTextBoxStatus.AppendText(ConditionallyRestricted + "\t");
            //    richTextBoxStatus.AppendText(Prohibited + "\t");
            //    richTextBoxStatus.AppendText(Unclassified + "\t");
            //    //richTextBoxStatus.AppendText(Comment + "\t");
            //    //richTextBoxStatus.AppendText(Lat + "\t");
            //    //richTextBoxStatus.AppendText(Lng + "\r\n");

            //    if (string.IsNullOrWhiteSpace(Subsector))
            //    {
            //        richTextBoxStatus.AppendText("Subsector is empty at line " + CountRead.ToString());
            //        break;
            //    }

            //    if (string.IsNullOrWhiteSpace(MWQMSiteName))
            //    {
            //        richTextBoxStatus.AppendText("MWQMSiteName is empty at line " + CountRead.ToString());
            //        break;
            //    }

            //    if (MWQMSiteName.Length < 4)
            //    {
            //        MWQMSiteName = "0000".Substring(0, 4 - MWQMSiteName.Length) + MWQMSiteName;
            //    }

            //    using (CSSPWebToolsDBEntities dd = new CSSPWebToolsDBEntities())
            //    {
            //        TVItem tvItemSS = (from t in dd.TVItems
            //                           from c in dd.TVItemLanguages
            //                           where t.TVItemID == c.TVItemID
            //                           && c.TVText.StartsWith(Subsector)
            //                           && c.Language == (int)LanguageEnum.en
            //                           select t).FirstOrDefault();

            //        if (tvItemSS == null)
            //        {
            //            richTextBoxStatus.AppendText("Could not find subsector " + Subsector + " at line " + CountRead.ToString());
            //            break;
            //        }

            //        if (!string.IsNullOrWhiteSpace(ID))
            //        {
            //            int TVItemID = 0;
            //            int.TryParse(ID, out TVItemID);
            //            if (TVItemID > 0)
            //            {
            //                TVItem tvItemSite = (from t in dd.TVItems
            //                                     where t.TVItemID == TVItemID
            //                                     select t).FirstOrDefault();

            //                if (tvItemSite == null)
            //                {
            //                    richTextBoxStatus.AppendText("Could not find site with ID " + ID + " at line " + CountRead.ToString());
            //                    break;
            //                }

            //                TVItemLanguage tvItemLanguageSite = (from t in dd.TVItemLanguages
            //                                                     where t.TVItemID == TVItemID
            //                                                     && t.Language == (int)LanguageEnum.en
            //                                                     select t).FirstOrDefault();

            //                if (tvItemLanguageSite == null)
            //                {
            //                    richTextBoxStatus.AppendText("Could not find sitelanguage with ID " + ID + " at line " + CountRead.ToString());
            //                    break;
            //                }

            //                if (tvItemLanguageSite.TVText != MWQMSiteName)
            //                {
            //                    richTextBoxStatus.AppendText("Subsector [" + Subsector + "] Site name in the DB " + tvItemLanguageSite.TVText + " is not the same as Spreadsheet " + MWQMSiteName + " at line " + CountRead.ToString());
            //                    break;
            //                }

            //                MWQMSite mwqmSite = (from c in dd.MWQMSites
            //                                     where c.MWQMSiteTVItemID == TVItemID
            //                                     select c).FirstOrDefault();

            //                if (mwqmSite == null)
            //                {
            //                    richTextBoxStatus.AppendText("Could not find MWQMSite in DB with TVItemID " + ID + " at line " + CountRead.ToString());
            //                    break;
            //                }

            //                bool HasClass = false;
            //                if (!string.IsNullOrWhiteSpace(Approved))
            //                {
            //                    mwqmSite.MWQMSiteLatestClassification = (int)MWQMSiteLatestClassificationEnum.Approved;
            //                    HasClass = true;
            //                }
            //                if (!string.IsNullOrWhiteSpace(ConditionallyApproved))
            //                {
            //                    mwqmSite.MWQMSiteLatestClassification = (int)MWQMSiteLatestClassificationEnum.ConditionallyApproved;
            //                    HasClass = true;
            //                }
            //                if (!string.IsNullOrWhiteSpace(Restricted))
            //                {
            //                    mwqmSite.MWQMSiteLatestClassification = (int)MWQMSiteLatestClassificationEnum.Restricted;
            //                    HasClass = true;
            //                }
            //                if (!string.IsNullOrWhiteSpace(ConditionallyRestricted))
            //                {
            //                    mwqmSite.MWQMSiteLatestClassification = (int)MWQMSiteLatestClassificationEnum.ConditionallyRestricted;
            //                    HasClass = true;
            //                }
            //                if (!string.IsNullOrWhiteSpace(Prohibited))
            //                {
            //                    mwqmSite.MWQMSiteLatestClassification = (int)MWQMSiteLatestClassificationEnum.Prohibited;
            //                    HasClass = true;
            //                }
            //                if (!string.IsNullOrWhiteSpace(Unclassified))
            //                {
            //                    mwqmSite.MWQMSiteLatestClassification = (int)MWQMSiteLatestClassificationEnum.Unclassified;
            //                    HasClass = true;
            //                }

            //                if (!HasClass)
            //                {
            //                    richTextBoxStatus.AppendText("Subsector [" + Subsector + "] Site name " + MWQMSiteName + " has no class identified at line " + CountRead.ToString());
            //                    break;
            //                }

            //                try
            //                {
            //                    dd.SaveChanges();
            //                }
            //                catch (Exception ex)
            //                {
            //                    richTextBoxStatus.AppendText("Error while saving MWQMSite " + ex.Message + (ex.InnerException == null ? "" : " Inner: " + ex.InnerException.Message) + " at line " + CountRead.ToString());
            //                    break;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            //if (!string.IsNullOrWhiteSpace(Lat) && !string.IsNullOrWhiteSpace(Lng) && !string.IsNullOrWhiteSpace(MWQMSiteName))
            //            //{
            //            //    if (tvItemSS.TVItemID > 0)
            //            //    {
            //            //        using (TransactionScope ts = new TransactionScope())
            //            //        {
            //            //            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            //            //            TVItemModel tvItemModelSite = tvItemService.PostAddChildTVItemDB(tvItemSS.TVItemID, MWQMSiteName, TVTypeEnum.MWQMSite);
            //            //            if (!string.IsNullOrWhiteSpace(tvItemModelSite.Error))
            //            //            {
            //            //                richTextBoxStatus.AppendText("Error while creating MWQMSite " + MWQMSiteName + " under subsector " + Subsector + " " + tvItemModelSite.Error + " at line " + CountRead.ToString());
            //            //                break;
            //            //            }


            //            //            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);

            //            //            MWQMSite mwqmSiteLastOrdinal = (from t in dd.TVItems
            //            //                                            from c in dd.MWQMSites
            //            //                                            where t.TVItemID == c.MWQMSiteTVItemID
            //            //                                            && t.ParentID == tvItemSS.TVItemID
            //            //                                            orderby c.Ordinal descending
            //            //                                            select c).FirstOrDefault();

            //            //            MWQMSiteModel mwqmSiteModel = new MWQMSiteModel()
            //            //            {
            //            //                MWQMSiteDescription = "Todo",
            //            //                MWQMSiteNumber = MWQMSiteName,
            //            //                MWQMSiteTVItemID = tvItemModelSite.TVItemID,
            //            //                Ordinal = mwqmSiteLastOrdinal.Ordinal + 1,
            //            //                MWQMSiteTVText = MWQMSiteName
            //            //            };

            //            //            bool HasClass = false;
            //            //            if (!string.IsNullOrWhiteSpace(Approved))
            //            //            {
            //            //                mwqmSiteModel.MWQMSiteLatestClassification = MWQMSiteLatestClassificationEnum.Approved;
            //            //                HasClass = true;
            //            //            }
            //            //            if (!string.IsNullOrWhiteSpace(ConditionallyApproved))
            //            //            {
            //            //                mwqmSiteModel.MWQMSiteLatestClassification = MWQMSiteLatestClassificationEnum.ConditionallyApproved;
            //            //                HasClass = true;
            //            //            }
            //            //            if (!string.IsNullOrWhiteSpace(Restricted))
            //            //            {
            //            //                mwqmSiteModel.MWQMSiteLatestClassification = MWQMSiteLatestClassificationEnum.Restricted;
            //            //                HasClass = true;
            //            //            }
            //            //            if (!string.IsNullOrWhiteSpace(ConditionallyRestricted))
            //            //            {
            //            //                mwqmSiteModel.MWQMSiteLatestClassification = MWQMSiteLatestClassificationEnum.ConditionallyRestricted;
            //            //                HasClass = true;
            //            //            }
            //            //            if (!string.IsNullOrWhiteSpace(Prohibited))
            //            //            {
            //            //                mwqmSiteModel.MWQMSiteLatestClassification = MWQMSiteLatestClassificationEnum.Prohibited;
            //            //                HasClass = true;
            //            //            }
            //            //            if (!string.IsNullOrWhiteSpace(Unclassified))
            //            //            {
            //            //                mwqmSiteModel.MWQMSiteLatestClassification = MWQMSiteLatestClassificationEnum.Unclassified;
            //            //                HasClass = true;
            //            //            }

            //            //            if (!HasClass)
            //            //            {
            //            //                richTextBoxStatus.AppendText("Subsector [" + Subsector + "] Site name " + MWQMSiteName + " has no class identified at line " + CountRead.ToString());
            //            //                break;
            //            //            }

            //            //            MWQMSiteModel mwqmSiteModelRet = mwqmSiteService.PostAddMWQMSiteDB(mwqmSiteModel);
            //            //            if (!string.IsNullOrWhiteSpace(mwqmSiteModelRet.Error))
            //            //            {
            //            //                richTextBoxStatus.AppendText(mwqmSiteModelRet.Error + " at line " + CountRead.ToString());
            //            //                break;
            //            //            }

            //            //            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            //            //            float LatFloat = 0.0f;
            //            //            if (!float.TryParse(Lat, out LatFloat))
            //            //            {
            //            //                richTextBoxStatus.AppendText("Could not parse Lat at line " + CountRead.ToString());
            //            //                break;
            //            //            }
            //            //            float LngFloat = 0.0f;
            //            //            if (!float.TryParse(Lng, out LngFloat))
            //            //            {
            //            //                richTextBoxStatus.AppendText("Could not parse Lng at line " + CountRead.ToString());
            //            //                break;
            //            //            }
            //            //            List<Coord> coordList = new List<Coord>()
            //            //        {
            //            //            new Coord() { Lat = LatFloat, Lng = LngFloat, Ordinal = 0 }
            //            //        };

            //            //            MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.MWQMSite, tvItemModelSite.TVItemID);
            //            //            if (!string.IsNullOrWhiteSpace(mapInfoModelRet.Error))
            //            //            {
            //            //                richTextBoxStatus.AppendText(mapInfoModelRet.Error + " at line " + CountRead.ToString());
            //            //                break;
            //            //            }
            //            //        }
            //            //    }
            //            //}
            //        }
            //    }
            //}
        }

        public class TVItemIDPinLatLng
        {
            public int TVItemID { get; set; }
            public float Lat { get; set; }
            public float Lng { get; set; }
            public string TVText { get; set; }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            //lblStatus.Text = @"C:\CSSP Latest Code old\DataTool\ImportByFunction\Data_inputs\NL_Sectors_Dec_2016_12_1_12_22.kml";
            //Application.DoEvents();

            //if (Cancel) return;

            //TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            //MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            //MapInfoPointService mapInfoPointService = new MapInfoPointService(LanguageEnum.en, user);

            //TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            //if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            //TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            //if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            //FileInfo fi = new FileInfo(lblStatus.Text);

            //if (!fi.Exists)
            //{
            //    richTextBoxStatus.AppendText("File not found [" + fi.FullName + "]\r\n");
            //    return;
            //}

            //TVItemModel tvItemModelNL = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Newfoundland and Labrador", TVTypeEnum.Province);
            //if (!CheckModelOK<TVItemModel>(tvItemModelNL)) return;

            //List<TVItemIDPinLatLng> tvItemIDPinLatLngList = new List<TVItemIDPinLatLng>();


            //List<TVItemModel> tvItemModelList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelNL.TVItemID, TVTypeEnum.Sector);

            //foreach (TVItemModel tvItemModel in tvItemModelList)
            //{
            //    List<MapInfoPointModel> mapInfoPointModelList = mapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModel.TVItemID, TVTypeEnum.Sector, MapInfoDrawTypeEnum.Point);
            //    if (mapInfoPointModelList.Count > 0)
            //    {
            //        float Lat = (float)mapInfoPointModelList[0].Lat;
            //        float Lng = (float)mapInfoPointModelList[0].Lng;
            //        TVItemIDPinLatLng tvItemIDPinLatLng = new TVItemIDPinLatLng()
            //        {
            //            Lat = Lat,
            //            Lng = Lng,
            //            TVItemID = tvItemModel.TVItemID,
            //            TVText = tvItemModel.TVText,
            //        };
            //        tvItemIDPinLatLngList.Add(tvItemIDPinLatLng);
            //    }
            //}

            //XmlDocument doc = new XmlDocument();
            //doc.Load(fi.FullName);

            //List<SubSector> PolList = new List<SubSector>();
            //string Prov = "";
            //string PolType = "";
            //string PolName = "";
            //XmlNode StartNode = doc.ChildNodes[1].ChildNodes[0];
            //int TotalCount = StartNode.ChildNodes.Count;
            //if (TotalCount != 82)
            //{
            //    richTextBoxStatus.AppendText("Total Count was not 82");
            //    return;
            //}
            //int count = 0;
            //foreach (XmlNode n in StartNode.ChildNodes)
            //{
            //    if (n.Name == "Placemark")
            //    {
            //        foreach (XmlNode n1 in n.ChildNodes)
            //        {
            //            if (n1.Name == "Polygon")
            //            {
            //                foreach (XmlNode n2 in n1.ChildNodes)
            //                {
            //                    if (n2.Name == "outerBoundaryIs")
            //                    {
            //                        foreach (XmlNode n3 in n2.ChildNodes)
            //                        {
            //                            if (n3.Name == "LinearRing")
            //                            {
            //                                foreach (XmlNode n4 in n3.ChildNodes)
            //                                {
            //                                    if (n4.Name == "coordinates")
            //                                    {
            //                                        count += 1;
            //                                        Application.DoEvents();
            //                                        List<Coord> coordList = new List<Coord>();
            //                                        string PolText = n4.InnerText.Trim().Replace("\r\n", "");

            //                                        string[] PolTextArr = PolText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            //                                        //richTextBoxStatus.AppendText(Prov + "\t" + PolType + "\t" + PolName + "\r\n" + PolText + "\r\n");
            //                                        int countElem = 0;
            //                                        foreach (string s in PolTextArr)
            //                                        {
            //                                            string[] coordVal = s.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //                                            if (coordVal.Count() != 3)
            //                                            {
            //                                                richTextBoxStatus.AppendText("coordVal.Count() should be equal to 3. It's [" + coordVal.Count() + "]\r\n");
            //                                                return;
            //                                            }
            //                                            coordList.Add(new Coord() { Lng = float.Parse(coordVal[0]), Lat = float.Parse(coordVal[1]), Ordinal = countElem });
            //                                            countElem += 1;
            //                                            //richTextBoxStatus.AppendText(s + " ");
            //                                        }
            //                                        float LatMin = coordList.Min(c => c.Lat);
            //                                        float LatMax = coordList.Max(c => c.Lat);
            //                                        float LngMin = coordList.Min(c => c.Lng);
            //                                        float LngMax = coordList.Max(c => c.Lng);
            //                                        List<TVItemIDPinLatLng> tvItemIDPinLatLngBoxList = tvItemIDPinLatLngList.Where(c => c.Lat > LatMin && c.Lat < LatMax && c.Lng > LngMin && c.Lng < LngMax).ToList();
            //                                        if (tvItemIDPinLatLngBoxList.Count == 0)
            //                                        {
            //                                            return;
            //                                        }
            //                                        foreach (TVItemIDPinLatLng tvItemIDPinLatLng in tvItemIDPinLatLngBoxList)
            //                                        {
            //                                            Application.DoEvents();

            //                                            if (mapInfoService.CoordInPolygon(coordList, new Coord() { Lat = tvItemIDPinLatLng.Lat, Lng = tvItemIDPinLatLng.Lng, Ordinal = 0 }))
            //                                            {
            //                                                List<MapInfoModel> mapInfoModelToDeleteList = mapInfoService.GetMapInfoModelListWithTVItemIDDB(tvItemIDPinLatLng.TVItemID).Where(c => c.TVType == TVTypeEnum.Sector && c.MapInfoDrawType == MapInfoDrawTypeEnum.Polygon).ToList();
            //                                                if (mapInfoModelToDeleteList.Count == 0)
            //                                                {
            //                                                    richTextBoxStatus.AppendText(tvItemIDPinLatLng.TVText + "\r\n");
            //                                                    MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Polygon, TVTypeEnum.Sector, tvItemIDPinLatLng.TVItemID);
            //                                                    if (!string.IsNullOrWhiteSpace(mapInfoModelRet.Error))
            //                                                    {
            //                                                        return;
            //                                                    }
            //                                                }
            //                                                else if (mapInfoModelToDeleteList.Count == 1)
            //                                                {
            //                                                    richTextBoxStatus.AppendText(tvItemIDPinLatLng.TVText + "\r\n");
            //                                                    MapInfoModel mapInfoModelDeleted = mapInfoService.PostDeleteMapInfoDB(mapInfoModelToDeleteList[0].MapInfoID);
            //                                                    if (!string.IsNullOrWhiteSpace(mapInfoModelDeleted.Error))
            //                                                    {
            //                                                        return;
            //                                                    }

            //                                                    MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Polygon, TVTypeEnum.Sector, tvItemIDPinLatLng.TVItemID);
            //                                                    if (!string.IsNullOrWhiteSpace(mapInfoModelRet.Error))
            //                                                    {
            //                                                        return;
            //                                                    }
            //                                                }
            //                                                else
            //                                                {
            //                                                    return;
            //                                                }
            //                                            }
            //                                        }
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }

            //            }
            //        }
            //    }
            //}

            return;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //string FileName = @"C:\CSSP Latest Code old\DataTool\ImportByFunction\Data_inputs\NS_Areas_Sectors_Subsectors_Active.xlsx";

            //string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=Excel 12.0";

            //OleDbConnection conn = new OleDbConnection(connectionString);

            //conn.Open();
            //OleDbDataReader reader;
            //OleDbCommand comm = new OleDbCommand("Select * from [Subsectors$];");

            //comm.Connection = conn;
            //reader = comm.ExecuteReader();

            //int CountRead = 0;
            //while (reader.Read())
            //{
            //    CountRead += 1;
            //    if (CountRead < 0)
            //        continue;

            //    Application.DoEvents();

            //    string ID = "";
            //    string AreaSectorSubsector = "";
            //    string Active = "";

            //    // ID
            //    if (reader.GetValue(0).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(0).ToString()))
            //    {
            //        ID = "";
            //    }
            //    else
            //    {
            //        ID = reader.GetValue(0).ToString().Trim();
            //    }

            //    // Subsector
            //    if (reader.GetValue(1).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(1).ToString()))
            //    {
            //        AreaSectorSubsector = "";
            //    }
            //    else
            //    {
            //        AreaSectorSubsector = reader.GetValue(1).ToString().Trim();
            //    }

            //    // Active
            //    if (reader.GetValue(2).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(2).ToString()))
            //    {
            //        Active = "";
            //    }
            //    else
            //    {
            //        Active = reader.GetValue(2).ToString().Trim();
            //    }

            //    richTextBoxStatus.AppendText(CountRead + "\t");
            //    richTextBoxStatus.AppendText(AreaSectorSubsector + "\t");
            //    richTextBoxStatus.AppendText(ID + "\t");
            //    richTextBoxStatus.AppendText(Active + "\t");

            //    if (string.IsNullOrWhiteSpace(AreaSectorSubsector))
            //    {
            //        richTextBoxStatus.AppendText("AreaSectorSubsector is empty at line " + CountRead.ToString());
            //        break;
            //    }

            //    if (string.IsNullOrWhiteSpace(ID))
            //    {
            //        richTextBoxStatus.AppendText("ID is empty at line " + CountRead.ToString());
            //        break;
            //    }

            //    if (string.IsNullOrWhiteSpace(Active))
            //    {
            //        richTextBoxStatus.AppendText("Active is empty at line " + CountRead.ToString());
            //        break;
            //    }

            //    using (CSSPWebToolsDBEntities dd = new CSSPWebToolsDBEntities())
            //    {
            //        int TVItemID = 0;
            //        if (!int.TryParse(ID, out TVItemID))
            //        {
            //            richTextBoxStatus.AppendText("TVItemID is not a number at line " + CountRead.ToString());
            //            break;
            //        }

            //        TVItem tvItem = (from t in dd.TVItems
            //                         where t.TVItemID == TVItemID
            //                         select t).FirstOrDefault();

            //        if (tvItem == null)
            //        {
            //            richTextBoxStatus.AppendText("Could not find TVItemID " + TVItemID + " at line " + CountRead.ToString());
            //            break;
            //        }

            //        bool IsActive = true;
            //        if (Active == "0")
            //        {
            //            IsActive = false;
            //        }

            //        tvItem.IsActive = IsActive;

            //        try
            //        {
            //            dd.SaveChanges();
            //        }
            //        catch (Exception ex)
            //        {
            //            richTextBoxStatus.AppendText("Error while saving TVItem " + ex.Message + (ex.InnerException == null ? "" : " Inner: " + ex.InnerException.Message) + " at line " + CountRead.ToString());
            //            break;
            //        }
            //    }
            //}
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities();
            //TempData.TempDataToolDBEntities dbTD = new TempData.TempDataToolDBEntities();

            //List<TempData.ASGADStation> AsgadStationList = (from c in dbTD.ASGADStations
            //                                                where c.PROV == "NL"
            //                                                orderby c.AREA, c.SECTOR, c.SUBSECTOR, c.STAT_NBR
            //                                                select c).ToList();

            //if (AsgadStationList.Count == 0)
            //{
            //    richTextBoxStatus.AppendText("Could not find ASGAD stations\r\n");
            //    return;
            //}

            //int NLID = 10; // Newfoundland and Labrador CSSPWebTools ID

            //TVItem tvItemNL = (from c in db.TVItems
            //                   where c.TVItemID == NLID
            //                   select c).FirstOrDefault();

            //if (tvItemNL == null)
            //{
            //    richTextBoxStatus.AppendText("Could not find NL TVItem \r\n");
            //    return;
            //}

            //var mwqmSiteSubsectorList = (from c in db.MWQMSites
            //                             from t in db.TVItems
            //                             from tl in db.TVItemLanguages
            //                             from t2 in db.TVItems
            //                             from tl2 in db.TVItemLanguages
            //                             where c.MWQMSiteTVItemID == t.TVItemID
            //                             && t.TVItemID == tl.TVItemID
            //                             && t2.TVItemID == t.ParentID
            //                             && tl2.TVItemID == t2.TVItemID
            //                             && t.TVPath.StartsWith(tvItemNL.TVPath)
            //                             && tl.Language == (int)LanguageEnum.en
            //                             && tl2.Language == (int)LanguageEnum.en
            //                             orderby tl2.TVText, tl.TVText
            //                             select new { tl2, tl }).ToList();

            //if (mwqmSiteSubsectorList.Count == 0)
            //{
            //    richTextBoxStatus.AppendText("Could not find CSSP MWQMSite with subsector \r\n");
            //    return;
            //}

            //richTextBoxStatus.AppendText("ASGAD Subsector\tASGAD Site\tCSSP Subsector\tCSSP Site\r\n");
            //int ASGADTotal = AsgadStationList.Count;
            //int CSSPTotal = mwqmSiteSubsectorList.Count;
            //int ASGADCount = 0;
            //int CSSPCount = 0;
            //while (true)
            //{
            //    string ASGADSite = AsgadStationList[ASGADCount].STAT_NBR.Trim().ToUpper();
            //    string CSSPSite = mwqmSiteSubsectorList[CSSPCount].tl.TVText.Trim().ToUpper();

            //    if (ASGADSite.Length != 4)
            //    {
            //        richTextBoxStatus.AppendText("ASGADSite length != 4 [" + ASGADSite + "]");
            //        break;
            //    }

            //    if (CSSPSite.Length != 4)
            //    {
            //        richTextBoxStatus.AppendText("CSSPSite length != 4 [" + CSSPSite + "]");
            //        break;
            //    }

            //    string ASGADLocator = (AsgadStationList[ASGADCount].PROV + "-" + AsgadStationList[ASGADCount].AREA + "-" + AsgadStationList[ASGADCount].SECTOR + "-" + AsgadStationList[ASGADCount].SUBSECTOR).Trim().ToUpper();
            //    string CSSPLocator = mwqmSiteSubsectorList[CSSPCount].tl2.TVText.Substring(0, mwqmSiteSubsectorList[CSSPCount].tl2.TVText.IndexOf(" ")).Trim().ToUpper();

            //    string ASGADFull = ASGADLocator + ASGADSite;
            //    string CSSPFull = CSSPLocator + CSSPSite;

            //    if (ASGADFull == CSSPFull)
            //    {
            //        richTextBoxStatus.AppendText(ASGADLocator + "\t" + ASGADSite + "\t" + CSSPLocator + "\t" + CSSPSite + "\r\n");
            //        ASGADCount += 1;
            //        CSSPCount += 1;

            //        if (ASGADCount > (ASGADTotal - 1))
            //        {
            //            ASGADCount = ASGADTotal - 1;
            //        }

            //        if (CSSPCount > (CSSPTotal - 1))
            //        {
            //            CSSPCount = CSSPTotal - 1;
            //        }
            //    }
            //    else if (String.Compare(ASGADFull, CSSPFull) < 0)
            //    {
            //        richTextBoxStatus.AppendText(ASGADLocator + "\t" + ASGADSite + "\t" + "empty" + "\t" + "empty" + "\r\n");
            //        ASGADCount += 1;

            //        if (ASGADCount > (ASGADTotal - 1))
            //        {
            //            ASGADCount = ASGADTotal - 1;
            //        }

            //    }
            //    else if (String.Compare(ASGADFull, CSSPFull) > 0)
            //    {
            //        richTextBoxStatus.AppendText("empty" + "\t" + "empty" + "\t" + CSSPLocator + "\t" + CSSPSite + "\r\n");
            //        CSSPCount += 1;

            //        if (CSSPCount > (CSSPTotal - 1))
            //        {
            //            CSSPCount = CSSPTotal - 1;
            //        }
            //    }

            //    if (ASGADCount == (ASGADTotal - 1) && CSSPCount == (CSSPTotal - 1))
            //    {
            //        break;
            //    }
            //}
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //TempData.TempDataToolDBEntities dbTD = new TempData.TempDataToolDBEntities();

            //List<TempData.ASGADStation> AsgadStationList = (from c in dbTD.ASGADStations
            //                                                where c.PROV == "NL"
            //                                                orderby c.AREA, c.SECTOR, c.SUBSECTOR, c.STAT_NBR
            //                                                select c).ToList();

            //if (AsgadStationList.Count == 0)
            //{
            //    richTextBoxStatus.AppendText("Could not find ASGAD stations\r\n");
            //    return;
            //}

            //List<TempData.ASGADSample> AsgadSampleList = (from c in dbTD.ASGADSamples
            //                                              where c.PROV == "NL"
            //                                              orderby c.AREA, c.SECTOR, c.SUBSECTOR, c.STAT_NBR
            //                                              select c).ToList();

            //if (AsgadSampleList.Count == 0)
            //{
            //    richTextBoxStatus.AppendText("Could not find ASGAD sample\r\n");
            //    return;
            //}

            //richTextBoxStatus.AppendText("ASGAD Subsector\tASGAD Station STAT_NBR\tASGAD Sample Count\r\n");
            //int ASGADTotal = AsgadStationList.Count;
            //foreach (TempData.ASGADStation asgadStation in AsgadStationList)
            //{
            //    richTextBoxStatus.Refresh();
            //    Application.DoEvents();

            //    string ASGADSite = asgadStation.STAT_NBR.Trim().ToUpper();

            //    if (ASGADSite.Length != 4)
            //    {
            //        richTextBoxStatus.AppendText("ASGADSite length != 4 [" + ASGADSite + "]");
            //        break;
            //    }

            //    string ASGADLocator = (asgadStation.PROV + "-" + asgadStation.AREA + "-" + asgadStation.SECTOR + "-" + asgadStation.SUBSECTOR).Trim().ToUpper();

            //    int count = (from c in AsgadSampleList
            //                 where c.PROV == asgadStation.PROV
            //                 && c.AREA == asgadStation.AREA
            //                 && c.SECTOR == asgadStation.SECTOR
            //                 && c.SUBSECTOR == asgadStation.SUBSECTOR
            //                 && c.STAT_NBR == ASGADSite
            //                 select c).Count();

            //    richTextBoxStatus.AppendText(ASGADLocator + "\t" + ASGADSite + "\t" + count + "\r\n");

            //}


        }

        private void button11_Click(object sender, EventArgs e)
        {
            //TempData.TempDataToolDBEntities dbTD = new TempData.TempDataToolDBEntities();

            //List<TempData.ASGADStation> AsgadStationList = (from c in dbTD.ASGADStations
            //                                                where c.PROV == "NL"
            //                                                orderby c.AREA, c.SECTOR, c.SUBSECTOR, c.STAT_NBR
            //                                                select c).ToList();

            //if (AsgadStationList.Count == 0)
            //{
            //    richTextBoxStatus.AppendText("Could not find ASGAD stations\r\n");
            //    return;
            //}

            //List<TempData.ASGADSample> AsgadSampleList = (from c in dbTD.ASGADSamples
            //                                              where c.PROV == "NL"
            //                                              orderby c.AREA, c.SECTOR, c.SUBSECTOR, c.STAT_NBR
            //                                              select c).ToList();

            //if (AsgadSampleList.Count == 0)
            //{
            //    richTextBoxStatus.AppendText("Could not find ASGAD sample\r\n");
            //    return;
            //}

            //var AsgadSampleSubsList = (from c in dbTD.ASGADSamples
            //                           where c.PROV == "NL"
            //                           orderby c.AREA, c.SECTOR, c.SUBSECTOR, c.STAT_NBR
            //                           select new { c.PROV, c.AREA, c.SECTOR, c.SUBSECTOR, c.STAT_NBR }).Distinct().ToList();

            //richTextBoxStatus.AppendText("ASGAD Station Subsector\tASGAD Station Site\tASGAD Sample Subsector\tASGAD Sample Site\tASGAD Sample Count\r\n");
            //int ASGADTotal = AsgadStationList.Count;
            //int ASGADSampleTotal = AsgadSampleSubsList.Count;
            //int ASGADCount = 0;
            //int ASGADSampleCount = 0;
            //while (true)
            //{
            //    richTextBoxStatus.Refresh();
            //    Application.DoEvents();

            //    string ASGADSite = AsgadStationList[ASGADCount].STAT_NBR.Trim().ToUpper();
            //    string ASGADSampleSite = AsgadSampleSubsList[ASGADSampleCount].STAT_NBR.Trim().ToUpper();

            //    if (ASGADSite.Length != 4)
            //    {
            //        richTextBoxStatus.AppendText("ASGADSite length != 4 [" + ASGADSite + "]");
            //        break;
            //    }

            //    if (ASGADSampleSite.Length != 4)
            //    {
            //        richTextBoxStatus.AppendText("CSSPSite length != 4 [" + ASGADSampleSite + "]");
            //        break;
            //    }

            //    string ASGADLocator = (AsgadStationList[ASGADCount].PROV + "-" + AsgadStationList[ASGADCount].AREA + "-" + AsgadStationList[ASGADCount].SECTOR + "-" + AsgadStationList[ASGADCount].SUBSECTOR).Trim().ToUpper();
            //    string ASGADSampleLocator = (AsgadSampleSubsList[ASGADSampleCount].PROV + "-" + AsgadSampleSubsList[ASGADSampleCount].AREA + "-" + AsgadSampleSubsList[ASGADSampleCount].SECTOR + "-" + AsgadSampleSubsList[ASGADSampleCount].SUBSECTOR).Trim().ToUpper();

            //    string ASGADFull = ASGADLocator + ASGADSite;
            //    string ASGADSampleFull = ASGADSampleLocator + ASGADSampleSite;

            //    if (ASGADFull == ASGADSampleFull)
            //    {
            //        int count = (from c in AsgadSampleList
            //                     where c.PROV == AsgadSampleSubsList[ASGADSampleCount].PROV
            //                     && c.AREA == AsgadSampleSubsList[ASGADSampleCount].AREA
            //                     && c.SECTOR == AsgadSampleSubsList[ASGADSampleCount].SECTOR
            //                     && c.SUBSECTOR == AsgadSampleSubsList[ASGADSampleCount].SUBSECTOR
            //                     && c.STAT_NBR == ASGADSampleSite
            //                     select c).Count();

            //        richTextBoxStatus.AppendText(ASGADLocator + "\t" + ASGADSite + "\t" + ASGADSampleLocator + "\t" + ASGADSampleSite + "\t" + count + "\r\n");
            //        ASGADCount += 1;
            //        ASGADSampleCount += 1;

            //        if (ASGADCount > (ASGADTotal - 1))
            //        {
            //            ASGADCount = ASGADTotal - 1;
            //        }

            //        if (ASGADSampleCount > (ASGADSampleTotal - 1))
            //        {
            //            ASGADSampleCount = ASGADSampleTotal - 1;
            //        }
            //    }
            //    else if (String.Compare(ASGADFull, ASGADSampleFull) < 0)
            //    {
            //        richTextBoxStatus.AppendText(ASGADLocator + "\t" + ASGADSite + "\t" + "empty" + "\t" + "empty" + "\tN/A\r\n");
            //        ASGADCount += 1;

            //        if (ASGADCount > (ASGADTotal - 1))
            //        {
            //            ASGADCount = ASGADTotal - 1;
            //        }

            //    }
            //    else if (String.Compare(ASGADFull, ASGADSampleFull) > 0)
            //    {
            //        int count = (from c in AsgadSampleList
            //                     where c.PROV == AsgadSampleSubsList[ASGADSampleCount].PROV
            //                     && c.AREA == AsgadSampleSubsList[ASGADSampleCount].AREA
            //                     && c.SECTOR == AsgadSampleSubsList[ASGADSampleCount].SECTOR
            //                     && c.SUBSECTOR == AsgadSampleSubsList[ASGADSampleCount].SUBSECTOR
            //                     && c.STAT_NBR == ASGADSampleSite
            //                     select c).Count();

            //        richTextBoxStatus.AppendText("empty" + "\t" + "empty" + "\t" + ASGADSampleLocator + "\t" + ASGADSampleSite + "\t" + count + "\r\n");
            //        ASGADSampleCount += 1;

            //        if (ASGADSampleCount > (ASGADSampleTotal - 1))
            //        {
            //            ASGADSampleCount = ASGADSampleTotal - 1;
            //        }
            //    }

            //    if (ASGADCount == (ASGADTotal - 1))
            //    {
            //        int sliejf = 34;
            //    }

            //    if (ASGADSampleCount == (ASGADSampleTotal - 1))
            //    {
            //        int sliejf = 34;
            //    }

            //    if (ASGADCount == (ASGADTotal - 1) && ASGADSampleCount == (ASGADSampleTotal - 1))
            //    {
            //        break;
            //    }
            //}

        }

        private void button12_Click(object sender, EventArgs e)
        {
            //TempData.TempDataToolDBEntities dbTD = new TempData.TempDataToolDBEntities();

            //List<TempData.ASGADRun> AsgadRunList = (from c in dbTD.ASGADRuns
            //                                        where c.PROV == "NL"
            //                                        orderby c.AREA, c.SECTOR, c.SUBSECTOR, c.SAMP_DATE
            //                                        select c).Distinct().OrderBy(c => c.AREA).ThenBy(c => c.SECTOR).ThenBy(c => c.SUBSECTOR).ThenBy(c => c.SAMP_DATE).ToList();

            //if (AsgadRunList.Count == 0)
            //{
            //    richTextBoxStatus.AppendText("Could not find ASGAD runs\r\n");
            //    return;
            //}

            //List<TempData.ASGADSample> AsgadSampleList = (from c in dbTD.ASGADSamples
            //                                              where c.PROV == "NL"
            //                                              orderby c.AREA, c.SECTOR, c.SUBSECTOR, c.SAMP_DATE
            //                                              select c).ToList();

            //if (AsgadSampleList.Count == 0)
            //{
            //    richTextBoxStatus.AppendText("Could not find ASGAD sample\r\n");
            //    return;
            //}

            //var AsgadRunDistList = (from c in AsgadRunList
            //                        where c.PROV == "NL"
            //                        orderby c.AREA, c.SECTOR, c.SUBSECTOR, c.SAMP_DATE
            //                        select new { c.PROV, c.AREA, c.SECTOR, c.SUBSECTOR, c.SAMP_DATE.Value.Year, c.SAMP_DATE, c.SAMP_DATE.Value.Month, c.SAMP_DATE.Value.Day }).Distinct().OrderBy(c => c.AREA).ThenBy(c => c.SECTOR).ThenBy(c => c.SUBSECTOR).ThenBy(c => c.SAMP_DATE).ToList();

            //var AsgadSampleSubsList = (from c in AsgadSampleList
            //                           where c.PROV == "NL"
            //                           orderby c.AREA, c.SECTOR, c.SUBSECTOR, c.SAMP_DATE
            //                           select new { c.PROV, c.AREA, c.SECTOR, c.SUBSECTOR, c.SAMP_DATE.Value.Year, c.SAMP_DATE, c.SAMP_DATE.Value.Month, c.SAMP_DATE.Value.Day }).Distinct().OrderBy(c => c.AREA).ThenBy(c => c.SECTOR).ThenBy(c => c.SUBSECTOR).ThenBy(c => c.SAMP_DATE).ToList();

            //richTextBoxStatus.AppendText("ASGAD Run Subsector\tASGAD Run Date\tASGAD Sample Subsector\tASGAD Sample Date\r\n");
            //int ASGADTotal = AsgadRunDistList.Count;
            //int ASGADSampleTotal = AsgadSampleSubsList.Count;
            //int ASGADCount = 0;
            //int ASGADSampleCount = 0;
            //while (true)
            //{
            //    richTextBoxStatus.Refresh();
            //    Application.DoEvents();

            //    DateTime runSampDate = (DateTime)AsgadRunDistList[ASGADCount].SAMP_DATE;
            //    string runSampleDateText = runSampDate.Year.ToString() + "-" +
            //        (runSampDate.Month < 10 ? "0" + runSampDate.Month.ToString() : runSampDate.Month.ToString()) + "-" +
            //        (runSampDate.Day < 10 ? "0" + runSampDate.Day.ToString() : runSampDate.Day.ToString());
            //    DateTime sampDate = new DateTime(AsgadSampleSubsList[ASGADSampleCount].Year, AsgadSampleSubsList[ASGADSampleCount].Month, AsgadSampleSubsList[ASGADSampleCount].Day);
            //    string SampleDateText = sampDate.Year.ToString() + "-" +
            //        (sampDate.Month < 10 ? "0" + sampDate.Month.ToString() : sampDate.Month.ToString()) + "-" +
            //        (sampDate.Day < 10 ? "0" + sampDate.Day.ToString() : sampDate.Day.ToString());

            //    if (runSampleDateText.Length != 10)
            //    {
            //        richTextBoxStatus.AppendText("ASGADSite length != 10 [" + runSampleDateText + "]");
            //        break;
            //    }

            //    if (SampleDateText.Length != 10)
            //    {
            //        richTextBoxStatus.AppendText("CSSPSite length != 10 [" + SampleDateText + "]");
            //        break;
            //    }

            //    string ASGADLocator = (AsgadRunDistList[ASGADCount].PROV + "-" + AsgadRunDistList[ASGADCount].AREA + "-" + AsgadRunDistList[ASGADCount].SECTOR + "-" + AsgadRunDistList[ASGADCount].SUBSECTOR).Trim().ToUpper();
            //    string ASGADSampleLocator = (AsgadSampleSubsList[ASGADSampleCount].PROV + "-" + AsgadSampleSubsList[ASGADSampleCount].AREA + "-" + AsgadSampleSubsList[ASGADSampleCount].SECTOR + "-" + AsgadSampleSubsList[ASGADSampleCount].SUBSECTOR).Trim().ToUpper();

            //    string ASGADFull = ASGADLocator + runSampleDateText;
            //    string ASGADSampleFull = ASGADSampleLocator + SampleDateText;

            //    if (ASGADFull == ASGADSampleFull)
            //    {
            //        richTextBoxStatus.AppendText(ASGADLocator + "\t" + runSampleDateText + "\t" + ASGADSampleLocator + "\t" + SampleDateText + "\r\n");
            //        ASGADCount += 1;
            //        ASGADSampleCount += 1;

            //        if (ASGADCount > (ASGADTotal - 1))
            //        {
            //            ASGADCount = ASGADTotal - 1;
            //        }

            //        if (ASGADSampleCount > (ASGADSampleTotal - 1))
            //        {
            //            ASGADSampleCount = ASGADSampleTotal - 1;
            //        }
            //    }
            //    else if (String.Compare(ASGADFull, ASGADSampleFull) < 0)
            //    {
            //        richTextBoxStatus.AppendText(ASGADLocator + "\t" + runSampleDateText + "\t" + "empty" + "\t" + "empty\r\n");
            //        ASGADCount += 1;

            //        if (ASGADCount > (ASGADTotal - 1))
            //        {
            //            ASGADCount = ASGADTotal - 1;
            //        }

            //    }
            //    else if (String.Compare(ASGADFull, ASGADSampleFull) > 0)
            //    {
            //        richTextBoxStatus.AppendText("empty" + "\t" + "empty" + "\t" + ASGADSampleLocator + "\t" + SampleDateText + "\r\n");
            //        ASGADSampleCount += 1;

            //        if (ASGADSampleCount > (ASGADSampleTotal - 1))
            //        {
            //            ASGADSampleCount = ASGADSampleTotal - 1;
            //        }
            //    }

            //    if (ASGADCount == (ASGADTotal - 1))
            //    {
            //        int sliejf = 34;
            //    }

            //    if (ASGADSampleCount == (ASGADSampleTotal - 1))
            //    {
            //        int sliejf = 34;
            //    }

            //    if (ASGADCount == (ASGADTotal - 1) && ASGADSampleCount == (ASGADSampleTotal - 1))
            //    {
            //        break;
            //    }
            //}

        }

        private void button13_Click(object sender, EventArgs e)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                richTextBoxStatus.AppendText(tvItemModelRoot.Error);
                return;
            }

            string Prov = "Newfoundland and Labrador";
            TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, Prov, TVTypeEnum.Province);
            if (!string.IsNullOrWhiteSpace(tvItemModelProv.Error))
            {
                richTextBoxStatus.AppendText(tvItemModelProv.Error);
                return;
            }

            StringBuilder sbKMZ = new StringBuilder();

            sbKMZ.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sbKMZ.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sbKMZ.AppendLine(@"<Document>");
            sbKMZ.AppendLine(@"	<name>NL CSSP Map Related Information</name>");

            sbKMZ.AppendLine(@"<Style id=""default"">");
            sbKMZ.AppendLine(@"<IconStyle>");
            sbKMZ.AppendLine(@"<scale>1.1</scale>");
            sbKMZ.AppendLine(@"<Icon>");
            sbKMZ.AppendLine(@"<href>http://maps.google.com/mapfiles/kml/pushpin/grn-pushpin.png</href>");
            sbKMZ.AppendLine(@"</Icon>");
            sbKMZ.AppendLine(@"<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sbKMZ.AppendLine(@"</IconStyle>");
            sbKMZ.AppendLine(@"<ListStyle>");
            sbKMZ.AppendLine(@"</ListStyle>");
            sbKMZ.AppendLine(@"</Style>");
            sbKMZ.AppendLine(@"<StyleMap id=""default0"">");
            sbKMZ.AppendLine(@"<Pair>");
            sbKMZ.AppendLine(@"<key>normal</key>");
            sbKMZ.AppendLine(@"<styleUrl>#default</styleUrl>");
            sbKMZ.AppendLine(@"</Pair>");
            sbKMZ.AppendLine(@"<Pair>");
            sbKMZ.AppendLine(@"<key>highlight</key>");
            sbKMZ.AppendLine(@"<styleUrl>#hl</styleUrl>");
            sbKMZ.AppendLine(@"</Pair>");
            sbKMZ.AppendLine(@"</StyleMap>");

            //sbKMZ.AppendLine(@" < StyleMap id=""msn_ylw-pushpin"">");
            //sbKMZ.AppendLine(@"		<Pair>");
            //sbKMZ.AppendLine(@"			<key>normal</key>");
            //sbKMZ.AppendLine(@"			<styleUrl>#sn_ylw-pushpin</styleUrl>");
            //sbKMZ.AppendLine(@"		</Pair>");
            //sbKMZ.AppendLine(@"		<Pair>");
            //sbKMZ.AppendLine(@"			<key>highlight</key>");
            //sbKMZ.AppendLine(@"			<styleUrl>#sh_ylw-pushpin</styleUrl>");
            //sbKMZ.AppendLine(@"		</Pair>");
            //sbKMZ.AppendLine(@"	</StyleMap>");
            //sbKMZ.AppendLine(@"	<Style id=""sh_ylw-pushpin"">");
            //sbKMZ.AppendLine(@"		<IconStyle>");
            //sbKMZ.AppendLine(@"			<scale>1.2</scale>");
            //sbKMZ.AppendLine(@"		</IconStyle>");
            //sbKMZ.AppendLine(@"		<LineStyle>");
            //sbKMZ.AppendLine(@"			<color>ff00ff00</color>");
            //sbKMZ.AppendLine(@"			<width>1.5</width>");
            //sbKMZ.AppendLine(@"		</LineStyle>");
            //sbKMZ.AppendLine(@"		<PolyStyle>");
            //sbKMZ.AppendLine(@"			<color>0000ff00</color>");
            //sbKMZ.AppendLine(@"		</PolyStyle>");
            //sbKMZ.AppendLine(@"	</Style>");
            //sbKMZ.AppendLine(@"	<Style id=""sn_ylw-pushpin"">");
            //sbKMZ.AppendLine(@"		<LineStyle>");
            //sbKMZ.AppendLine(@"			<color>ff00ff00</color>");
            //sbKMZ.AppendLine(@"			<width>1.5</width>");
            //sbKMZ.AppendLine(@"		</LineStyle>");
            //sbKMZ.AppendLine(@"		<PolyStyle>");
            //sbKMZ.AppendLine(@"			<color>0000ff00</color>");
            //sbKMZ.AppendLine(@"		</PolyStyle>");
            //sbKMZ.AppendLine(@"	</Style>");


            sbKMZ.AppendLine(@"	<Folder>");
            sbKMZ.AppendLine(@"	<name>" + tvItemModelProv.TVText + "</name>");

            #region Areas
            sbKMZ.AppendLine(@"	<Folder>");
            sbKMZ.AppendLine(@"	<name>Areas</name>");

            #region Areas Pin
            sbKMZ.AppendLine(@"	<Folder>");
            sbKMZ.AppendLine(@"	<name>Pin</name>");

            var areaInfoList = (from c in tvItemService.db.TVItems
                                from cl in tvItemService.db.TVItemLanguages
                                let ml = (from m in tvItemService.db.MapInfos
                                          where m.TVItemID == c.TVItemID
                                          && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
                                          select m).FirstOrDefault()
                                let mpl = (from mp in tvItemService.db.MapInfoPoints
                                           where ml.MapInfoID == mp.MapInfoID
                                           select mp).FirstOrDefault()
                                where c.TVItemID == cl.TVItemID
                                && c.TVPath.StartsWith(tvItemModelProv.TVPath + "p")
                                && c.TVType == (int)TVTypeEnum.Area
                                && cl.Language == (int)LanguageEnum.en
                                select new { c, ml, mpl, cl }).Take(5).ToList();



            foreach (var areaInfo in areaInfoList)
            {
                if (areaInfo.mpl != null)
                {
                    sbKMZ.AppendLine(@"	<Placemark>");
                    sbKMZ.AppendLine(@"	<name>" + areaInfo.cl.TVText + (areaInfo.c.IsActive ? " (active)" : "") + "</name>");
                    if (areaInfo.c.IsActive)
                    {
                        sbKMZ.AppendLine(@"<styleUrl>#default0</styleUrl>");
                    }
                    else
                    {
                        sbKMZ.AppendLine(@"<styleUrl>#default</styleUrl>");
                    }
                    sbKMZ.AppendLine(@"	<Point>");
                    sbKMZ.AppendLine(@"		<coordinates>" + areaInfo.mpl.Lng + "," + areaInfo.mpl.Lat + ",0</coordinates>");
                    sbKMZ.AppendLine(@"	</Point>");
                    sbKMZ.AppendLine(@"	</Placemark>");
                }
            }

            sbKMZ.AppendLine(@" </Folder>"); // Areas Pin
            #endregion Area pin

            #region Area polygon
            sbKMZ.AppendLine(@"	<Folder>");
            sbKMZ.AppendLine(@"	<name>Polygon</name>");

            var areaInfo2List = (from c in tvItemService.db.TVItems
                                 from cl in tvItemService.db.TVItemLanguages
                                 let ml = (from m in tvItemService.db.MapInfos
                                           where m.TVItemID == c.TVItemID
                                           && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon
                                           select m).FirstOrDefault()
                                 let mpl = (from mp in tvItemService.db.MapInfoPoints
                                            where ml.MapInfoID == mp.MapInfoID
                                            select mp).ToList()
                                 where c.TVItemID == cl.TVItemID
                                 && c.TVPath.StartsWith(tvItemModelProv.TVPath + "p")
                                 && c.TVType == (int)TVTypeEnum.Area
                                 && cl.Language == (int)LanguageEnum.en
                                 select new { c, ml, mpl, cl }).Take(5).ToList();



            foreach (var areaInfo2 in areaInfo2List)
            {
                if (areaInfo2.mpl != null)
                {
                    sbKMZ.AppendLine(@"	<Placemark>");
                    sbKMZ.AppendLine(@"	<name>" + areaInfo2.cl.TVText + (areaInfo2.c.IsActive ? " (active)" : "") + "</name>");
                    //sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
                    sbKMZ.AppendLine(@"	<Polygon>");
                    sbKMZ.AppendLine(@"	    <outerBoundaryIs>");
                    sbKMZ.AppendLine(@"	        <LinearRing>");
                    sbKMZ.AppendLine(@"		        <coordinates>");
                    foreach (var mp in areaInfo2.mpl)
                    {
                        sbKMZ.AppendLine(@"" + mp.Lng + "," + mp.Lat + ",0");
                    }
                    sbKMZ.AppendLine(@"		        </coordinates>");
                    sbKMZ.AppendLine(@"	        </LinearRing>");
                    sbKMZ.AppendLine(@"	    </outerBoundaryIs>");
                    sbKMZ.AppendLine(@"	</Polygon>");
                    sbKMZ.AppendLine(@"	</Placemark>");
                }
            }

            sbKMZ.AppendLine(@" </Folder>"); // Areas Pin
            #endregion Area Polygon

            sbKMZ.AppendLine(@" </Folder>"); // Areas

            #endregion Areas

            #region Sectors
            sbKMZ.AppendLine(@"	<Folder>");
            sbKMZ.AppendLine(@"	<name>Sectors</name>");

            #region Sectors Pin
            sbKMZ.AppendLine(@"	<Folder>");
            sbKMZ.AppendLine(@"	<name>Pin</name>");

            var SectorInfoList = (from c in tvItemService.db.TVItems
                                  from cl in tvItemService.db.TVItemLanguages
                                  let ml = (from m in tvItemService.db.MapInfos
                                            where m.TVItemID == c.TVItemID
                                            && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
                                            select m).FirstOrDefault()
                                  let mpl = (from mp in tvItemService.db.MapInfoPoints
                                             where ml.MapInfoID == mp.MapInfoID
                                             select mp).FirstOrDefault()
                                  where c.TVItemID == cl.TVItemID
                                  && c.TVPath.StartsWith(tvItemModelProv.TVPath + "p")
                                  && c.TVType == (int)TVTypeEnum.Sector
                                  && cl.Language == (int)LanguageEnum.en
                                  select new { c, ml, mpl, cl }).Take(5).ToList();



            foreach (var SectorInfo in SectorInfoList)
            {
                if (SectorInfo.mpl != null)
                {
                    sbKMZ.AppendLine(@"	<Placemark>");
                    sbKMZ.AppendLine(@"	<name>" + SectorInfo.cl.TVText + (SectorInfo.c.IsActive ? " (active)" : "") + "</name>");
                    //sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
                    sbKMZ.AppendLine(@"	<Point>");
                    sbKMZ.AppendLine(@"		<coordinates>" + SectorInfo.mpl.Lng + "," + SectorInfo.mpl.Lat + ",0</coordinates>");
                    sbKMZ.AppendLine(@"	</Point>");
                    sbKMZ.AppendLine(@"	</Placemark>");
                }
            }

            sbKMZ.AppendLine(@" </Folder>"); // Sectors Pin
            #endregion Sector pin

            #region Sector polygon
            sbKMZ.AppendLine(@"	<Folder>");
            sbKMZ.AppendLine(@"	<name>Polygon</name>");

            var SectorInfo2List = (from c in tvItemService.db.TVItems
                                   from cl in tvItemService.db.TVItemLanguages
                                   let ml = (from m in tvItemService.db.MapInfos
                                             where m.TVItemID == c.TVItemID
                                             && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon
                                             select m).FirstOrDefault()
                                   let mpl = (from mp in tvItemService.db.MapInfoPoints
                                              where ml.MapInfoID == mp.MapInfoID
                                              select mp).ToList()
                                   where c.TVItemID == cl.TVItemID
                                   && c.TVPath.StartsWith(tvItemModelProv.TVPath + "p")
                                   && c.TVType == (int)TVTypeEnum.Sector
                                   && cl.Language == (int)LanguageEnum.en
                                   select new { c, ml, mpl, cl }).Take(5).ToList();



            foreach (var SectorInfo2 in SectorInfo2List)
            {
                if (SectorInfo2.mpl != null)
                {
                    sbKMZ.AppendLine(@"	<Placemark>");
                    sbKMZ.AppendLine(@"	<name>" + SectorInfo2.cl.TVText + (SectorInfo2.c.IsActive ? " (active)" : "") + "</name>");
                    //sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
                    sbKMZ.AppendLine(@"	<Polygon>");
                    sbKMZ.AppendLine(@"	    <outerBoundaryIs>");
                    sbKMZ.AppendLine(@"	        <LinearRing>");
                    sbKMZ.AppendLine(@"		        <coordinates>");
                    foreach (var mp in SectorInfo2.mpl)
                    {
                        sbKMZ.AppendLine(@"" + mp.Lng + "," + mp.Lat + ",0");
                    }
                    sbKMZ.AppendLine(@"		        </coordinates>");
                    sbKMZ.AppendLine(@"	        </LinearRing>");
                    sbKMZ.AppendLine(@"	    </outerBoundaryIs>");
                    sbKMZ.AppendLine(@"	</Polygon>");
                    sbKMZ.AppendLine(@"	</Placemark>");
                }
            }

            sbKMZ.AppendLine(@" </Folder>"); // Sectors Pin
            #endregion Sector Polygon

            sbKMZ.AppendLine(@" </Folder>"); // Sectors

            #endregion Sectors

            #region Subsectors
            sbKMZ.AppendLine(@"	<Folder>");
            sbKMZ.AppendLine(@"	<name>Subsectors</name>");

            #region Subsectors Pin
            sbKMZ.AppendLine(@"	<Folder>");
            sbKMZ.AppendLine(@"	<name>Pin</name>");

            var SubsectorInfoList = (from c in tvItemService.db.TVItems
                                     from cl in tvItemService.db.TVItemLanguages
                                     let ml = (from m in tvItemService.db.MapInfos
                                               where m.TVItemID == c.TVItemID
                                               && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
                                               select m).FirstOrDefault()
                                     let mpl = (from mp in tvItemService.db.MapInfoPoints
                                                where ml.MapInfoID == mp.MapInfoID
                                                select mp).FirstOrDefault()
                                     where c.TVItemID == cl.TVItemID
                                     && c.TVPath.StartsWith(tvItemModelProv.TVPath + "p")
                                     && c.TVType == (int)TVTypeEnum.Subsector
                                     && cl.Language == (int)LanguageEnum.en
                                     select new { c, ml, mpl, cl }).Take(5).ToList();



            foreach (var SubsectorInfo in SubsectorInfoList)
            {
                if (SubsectorInfo.mpl != null)
                {
                    sbKMZ.AppendLine(@"	<Placemark>");
                    sbKMZ.AppendLine(@"	<name>" + SubsectorInfo.cl.TVText + (SubsectorInfo.c.IsActive ? " (active)" : "") + "</name>");
                    //sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
                    sbKMZ.AppendLine(@"	<Point>");
                    sbKMZ.AppendLine(@"		<coordinates>" + SubsectorInfo.mpl.Lng + "," + SubsectorInfo.mpl.Lat + ",0</coordinates>");
                    sbKMZ.AppendLine(@"	</Point>");
                    sbKMZ.AppendLine(@"	</Placemark>");
                }
            }

            sbKMZ.AppendLine(@" </Folder>"); // Subsectors Pin
            #endregion Subsector pin

            #region Subsector polygon
            sbKMZ.AppendLine(@"	<Folder>");
            sbKMZ.AppendLine(@"	<name>Polygon</name>");

            var SubsectorInfo2List = (from c in tvItemService.db.TVItems
                                      from cl in tvItemService.db.TVItemLanguages
                                      let ml = (from m in tvItemService.db.MapInfos
                                                where m.TVItemID == c.TVItemID
                                                && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon
                                                select m).FirstOrDefault()
                                      let mpl = (from mp in tvItemService.db.MapInfoPoints
                                                 where ml.MapInfoID == mp.MapInfoID
                                                 select mp).ToList()
                                      where c.TVItemID == cl.TVItemID
                                      && c.TVPath.StartsWith(tvItemModelProv.TVPath + "p")
                                      && c.TVType == (int)TVTypeEnum.Subsector
                                      && cl.Language == (int)LanguageEnum.en
                                      select new { c, ml, mpl, cl }).Take(5).ToList();



            foreach (var SubsectorInfo2 in SubsectorInfo2List)
            {
                if (SubsectorInfo2.mpl != null)
                {
                    sbKMZ.AppendLine(@"	<Placemark>");
                    sbKMZ.AppendLine(@"	<name>" + SubsectorInfo2.cl.TVText + (SubsectorInfo2.c.IsActive ? " (active)" : "") + "</name>");
                    //sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
                    sbKMZ.AppendLine(@"	<Polygon>");
                    sbKMZ.AppendLine(@"	    <outerBoundaryIs>");
                    sbKMZ.AppendLine(@"	        <LinearRing>");
                    sbKMZ.AppendLine(@"		        <coordinates>");
                    foreach (var mp in SubsectorInfo2.mpl)
                    {
                        sbKMZ.AppendLine(@"" + mp.Lng + "," + mp.Lat + ",0");
                    }
                    sbKMZ.AppendLine(@"		        </coordinates>");
                    sbKMZ.AppendLine(@"	        </LinearRing>");
                    sbKMZ.AppendLine(@"	    </outerBoundaryIs>");
                    sbKMZ.AppendLine(@"	</Polygon>");
                    sbKMZ.AppendLine(@"	</Placemark>");
                }
            }

            sbKMZ.AppendLine(@" </Folder>"); // Subsectors Pin
            #endregion Subsector Polygon

            sbKMZ.AppendLine(@" </Folder>"); // Subsectors

            #endregion Subsectors

            //#region Subsectors
            //sbKMZ.AppendLine(@"	<Folder>");
            //sbKMZ.AppendLine(@"	<name>Subsectors</name>");

            //#region Active Subsectors
            //sbKMZ.AppendLine(@"	<Folder>");
            //sbKMZ.AppendLine(@"	<name>Active</name>");

            //#region Subsectors Active Pin

            //sbKMZ.AppendLine(@"	<Folder>");
            //sbKMZ.AppendLine(@"	<name>Pins</name>");

            //var SubsectorInfoList = (from c in tvItemService.db.TVItems
            //                         from cl in tvItemService.db.TVItemLanguages
            //                         let ml = (from m in tvItemService.db.MapInfos
            //                                   where m.TVItemID == c.TVItemID
            //                                   && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                                   select m).FirstOrDefault()
            //                         let mpl = (from mp in tvItemService.db.MapInfoPoints
            //                                    where ml.MapInfoID == mp.MapInfoID
            //                                    select mp).FirstOrDefault()
            //                         where c.TVItemID == cl.TVItemID
            //                         && c.TVPath.StartsWith(tvItemModelProv.TVPath + "p")
            //                         && c.TVType == (int)TVTypeEnum.Subsector
            //                         && cl.Language == (int)LanguageEnum.en
            //                         && c.IsActive == true
            //                         select new { c, ml, mpl, cl }).Take(5).ToList();



            //foreach (var SubsectorInfo in SubsectorInfoList)
            //{
            //    if (SubsectorInfo.mpl != null)
            //    {
            //        sbKMZ.AppendLine(@"	<Placemark>");
            //        sbKMZ.AppendLine(@"	<name>" + SubsectorInfo.cl.TVText + "</name>");
            //        //sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
            //        sbKMZ.AppendLine(@"	<Point>");
            //        sbKMZ.AppendLine(@"		<coordinates>" + SubsectorInfo.mpl.Lng + "," + SubsectorInfo.mpl.Lat + ",0</coordinates>");
            //        sbKMZ.AppendLine(@"	</Point>");
            //        sbKMZ.AppendLine(@"	</Placemark>");
            //    }
            //}

            //sbKMZ.AppendLine(@" </Folder>"); // Subsectors Active Pin

            //#endregion Subsector Active Pin

            //#region Subsector Active Polygon
            //sbKMZ.AppendLine(@"	<Folder>");
            //sbKMZ.AppendLine(@"	<name>Polygon</name>");

            //var SubsectorInfo2List = (from c in tvItemService.db.TVItems
            //                          from cl in tvItemService.db.TVItemLanguages
            //                          let ml = (from m in tvItemService.db.MapInfos
            //                                    where m.TVItemID == c.TVItemID
            //                                    && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon
            //                                    select m).FirstOrDefault()
            //                          let mpl = (from mp in tvItemService.db.MapInfoPoints
            //                                     where ml.MapInfoID == mp.MapInfoID
            //                                     select mp).ToList()
            //                          where c.TVItemID == cl.TVItemID
            //                          && c.TVPath.StartsWith(tvItemModelProv.TVPath + "p")
            //                          && c.TVType == (int)TVTypeEnum.Subsector
            //                          && cl.Language == (int)LanguageEnum.en
            //                          && c.IsActive == true
            //                          select new { c, ml, mpl, cl }).Take(5).ToList();



            //foreach (var SubsectorInfo2 in SubsectorInfo2List)
            //{
            //    if (SubsectorInfo2.mpl != null)
            //    {
            //        sbKMZ.AppendLine(@"	<Placemark>");
            //        sbKMZ.AppendLine(@"	<name>" + SubsectorInfo2.cl.TVText + "</name>");
            //        //sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
            //        sbKMZ.AppendLine(@"	<Polygon>");
            //        sbKMZ.AppendLine(@"	    <outerBoundaryIs>");
            //        sbKMZ.AppendLine(@"	        <LinearRing>");
            //        sbKMZ.AppendLine(@"		        <coordinates>");
            //        foreach (var mp in SubsectorInfo2.mpl)
            //        {
            //            sbKMZ.AppendLine(@"" + mp.Lng + "," + mp.Lat + ",0");
            //        }
            //        sbKMZ.AppendLine(@"		        </coordinates>");
            //        sbKMZ.AppendLine(@"	        </LinearRing>");
            //        sbKMZ.AppendLine(@"	    </outerBoundaryIs>");
            //        sbKMZ.AppendLine(@"	</Polygon>");
            //        sbKMZ.AppendLine(@"	</Placemark>");
            //    }



            //}

            //#endregion Subsector Active Polygon

            //sbKMZ.AppendLine(@" </Folder>"); // Subsectors Active
            //#endregion Active Subsector

            //#region Inactive Subsector
            //sbKMZ.AppendLine(@"	<Folder>");
            //sbKMZ.AppendLine(@"	<name>Inactive</name>");

            //#region Inactive Subsectors

            //#region Inactive Subsectors Pin
            //////////// Subsectors inactive
            //sbKMZ.AppendLine(@"	<Folder>");
            //sbKMZ.AppendLine(@"	<name>Pin</name>");

            //SubsectorInfoList = (from c in tvItemService.db.TVItems
            //                     from cl in tvItemService.db.TVItemLanguages
            //                     let ml = (from m in tvItemService.db.MapInfos
            //                               where m.TVItemID == c.TVItemID
            //                               && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                               select m).FirstOrDefault()
            //                     let mpl = (from mp in tvItemService.db.MapInfoPoints
            //                                where ml.MapInfoID == mp.MapInfoID
            //                                select mp).FirstOrDefault()
            //                     where c.TVItemID == cl.TVItemID
            //                     && c.TVPath.StartsWith(tvItemModelProv.TVPath + "p")
            //                     && c.TVType == (int)TVTypeEnum.Subsector
            //                     && cl.Language == (int)LanguageEnum.en
            //                     && c.IsActive == false
            //                     select new { c, ml, mpl, cl }).Take(5).ToList();



            //foreach (var SubsectorInfo in SubsectorInfoList)
            //{
            //    if (SubsectorInfo.mpl != null)
            //    {
            //        sbKMZ.AppendLine(@"	<Placemark>");
            //        sbKMZ.AppendLine(@"	<name>" + SubsectorInfo.cl.TVText + "</name>");
            //        //sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
            //        sbKMZ.AppendLine(@"	<Point>");
            //        sbKMZ.AppendLine(@"		<coordinates>" + SubsectorInfo.mpl.Lng + "," + SubsectorInfo.mpl.Lat + ",0</coordinates>");
            //        sbKMZ.AppendLine(@"	</Point>");
            //        sbKMZ.AppendLine(@"	</Placemark>");
            //    }
            //}

            //sbKMZ.AppendLine(@" </Folder>"); // Subsectors Active Pin

            //#endregion  Inactive Subsectors Pin

            //#region Inactive Subsector Polygon
            //sbKMZ.AppendLine(@"	<Folder>");
            //sbKMZ.AppendLine(@"	<name>Polygon</name>");

            //SubsectorInfo2List = (from c in tvItemService.db.TVItems
            //                      from cl in tvItemService.db.TVItemLanguages
            //                      let ml = (from m in tvItemService.db.MapInfos
            //                                where m.TVItemID == c.TVItemID
            //                                && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon
            //                                select m).FirstOrDefault()
            //                      let mpl = (from mp in tvItemService.db.MapInfoPoints
            //                                 where ml.MapInfoID == mp.MapInfoID
            //                                 select mp).ToList()
            //                      where c.TVItemID == cl.TVItemID
            //                      && c.TVPath.StartsWith(tvItemModelProv.TVPath + "p")
            //                      && c.TVType == (int)TVTypeEnum.Subsector
            //                      && cl.Language == (int)LanguageEnum.en
            //                      && c.IsActive == false
            //                      select new { c, ml, mpl, cl }).Take(5).ToList();



            //foreach (var SubsectorInfo2 in SubsectorInfo2List)
            //{
            //    if (SubsectorInfo2.mpl != null)
            //    {
            //        sbKMZ.AppendLine(@"	<Placemark>");
            //        sbKMZ.AppendLine(@"	<name>" + SubsectorInfo2.cl.TVText + "</name>");
            //        //sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
            //        sbKMZ.AppendLine(@"	<Polygon>");
            //        sbKMZ.AppendLine(@"	    <outerBoundaryIs>");
            //        sbKMZ.AppendLine(@"	        <LinearRing>");
            //        sbKMZ.AppendLine(@"		        <coordinates>");
            //        foreach (var mp in SubsectorInfo2.mpl)
            //        {
            //            sbKMZ.AppendLine(@"" + mp.Lng + "," + mp.Lat + ",0");
            //        }
            //        sbKMZ.AppendLine(@"		        </coordinates>");
            //        sbKMZ.AppendLine(@"	        </LinearRing>");
            //        sbKMZ.AppendLine(@"	    </outerBoundaryIs>");
            //        sbKMZ.AppendLine(@"	</Polygon>");
            //        sbKMZ.AppendLine(@"	</Placemark>");
            //    }


            //    #region MWQM sites Active
            //    sbKMZ.AppendLine(@"	<Folder>");
            //    sbKMZ.AppendLine(@"	<name>" + SubsectorInfo2.cl.TVText + " MWQM Sites Active</name>");

            //    var mwqmsiteList = (from c in tvItemService.db.TVItems
            //                        from cl in tvItemService.db.TVItemLanguages
            //                        let ml = (from m in tvItemService.db.MapInfos
            //                                  where m.TVItemID == c.TVItemID
            //                                  && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                                  select m).FirstOrDefault()
            //                        let mpl = (from mp in tvItemService.db.MapInfoPoints
            //                                   where ml.MapInfoID == mp.MapInfoID
            //                                   select mp).FirstOrDefault()
            //                        where c.TVItemID == cl.TVItemID
            //                        && c.TVPath.StartsWith(SubsectorInfo2.c.TVPath + "p")
            //                        && c.TVType == (int)TVTypeEnum.MWQMSite
            //                        && cl.Language == (int)LanguageEnum.en
            //                        && c.IsActive == true
            //                        select new { c, ml, mpl, cl }).Take(5).ToList();



            //    foreach (var mwqmsite in mwqmsiteList)
            //    {
            //        if (mwqmsite.mpl != null)
            //        {
            //            sbKMZ.AppendLine(@"	<Placemark>");
            //            sbKMZ.AppendLine(@"	<name>" + mwqmsite.cl.TVText + "</name>");
            //            //sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
            //            sbKMZ.AppendLine(@"	<Point>");
            //            sbKMZ.AppendLine(@"		<coordinates>" + mwqmsite.mpl.Lng + "," + mwqmsite.mpl.Lat + ",0</coordinates>");
            //            sbKMZ.AppendLine(@"	</Point>");
            //            sbKMZ.AppendLine(@"	</Placemark>");
            //        }
            //    }

            //    sbKMZ.AppendLine(@" </Folder>"); // Subsectors Active Pin            #endregion MWQM sites

            //    #endregion MWQM sites Active

            //    #region MWQM sites Inactive
            //    sbKMZ.AppendLine(@"	<Folder>");
            //    sbKMZ.AppendLine(@"	<name>" + SubsectorInfo2.cl.TVText + " MWQM Sites Inactive</name>");

            //    mwqmsiteList = (from c in tvItemService.db.TVItems
            //                    from cl in tvItemService.db.TVItemLanguages
            //                    let ml = (from m in tvItemService.db.MapInfos
            //                              where m.TVItemID == c.TVItemID
            //                              && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                              select m).FirstOrDefault()
            //                    let mpl = (from mp in tvItemService.db.MapInfoPoints
            //                               where ml.MapInfoID == mp.MapInfoID
            //                               select mp).FirstOrDefault()
            //                    where c.TVItemID == cl.TVItemID
            //                    && c.TVPath.StartsWith(SubsectorInfo2.c.TVPath + "p")
            //                    && c.TVType == (int)TVTypeEnum.MWQMSite
            //                    && cl.Language == (int)LanguageEnum.en
            //                    && c.IsActive == false
            //                    select new { c, ml, mpl, cl }).Take(5).ToList();



            //    foreach (var mwqmsite in mwqmsiteList)
            //    {
            //        if (mwqmsite.mpl != null)
            //        {
            //            sbKMZ.AppendLine(@"	<Placemark>");
            //            sbKMZ.AppendLine(@"	<name>" + mwqmsite.cl.TVText + "</name>");
            //            //sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
            //            sbKMZ.AppendLine(@"	<Point>");
            //            sbKMZ.AppendLine(@"		<coordinates>" + mwqmsite.mpl.Lng + "," + mwqmsite.mpl.Lat + ",0</coordinates>");
            //            sbKMZ.AppendLine(@"	</Point>");
            //            sbKMZ.AppendLine(@"	</Placemark>");
            //        }
            //    }

            //    sbKMZ.AppendLine(@" </Folder>"); // Subsectors Active Pin            

            //    #endregion MWQM sites Inactive

            //}

            //sbKMZ.AppendLine(@" </Folder>"); // Subsectors Inactive Polygon

            //#endregion Inactive Subsector Polygon

            //#endregion Inactive Subsectors

            //sbKMZ.AppendLine(@" </Folder>"); // Subsectors Inactive
            //#endregion Inactive Subsector

            //sbKMZ.AppendLine(@" </Folder>"); // Subsectors 

            //#endregion Subsectors

            //#region Subsector MWQM Sites
            //var SubsectorInfo3List = (from c in tvItemService.db.TVItems
            //                          from cl in tvItemService.db.TVItemLanguages
            //                          where c.TVItemID == cl.TVItemID
            //                          && c.TVPath.StartsWith(tvItemModelProv.TVPath + "p")
            //                          && c.TVType == (int)TVTypeEnum.Subsector
            //                          && cl.Language == (int)LanguageEnum.en
            //                          && c.IsActive == false
            //                          select new { c, cl }).Take(5).ToList();



            //foreach (var SubsectorInfo3 in SubsectorInfo3List)
            //{
            //    sbKMZ.AppendLine(@"	<Folder>");
            //    sbKMZ.AppendLine(@"	<name>" + SubsectorInfo3.cl.TVText + " MWQM Sites</name>");

            //    #region MWQM sites Active
            //    sbKMZ.AppendLine(@"	<Folder>");
            //    sbKMZ.AppendLine(@"	<name>Active</name>");

            //    var mwqmsiteList = (from c in tvItemService.db.TVItems
            //                        from cl in tvItemService.db.TVItemLanguages
            //                        let ml = (from m in tvItemService.db.MapInfos
            //                                  where m.TVItemID == c.TVItemID
            //                                  && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                                  select m).FirstOrDefault()
            //                        let mpl = (from mp in tvItemService.db.MapInfoPoints
            //                                   where ml.MapInfoID == mp.MapInfoID
            //                                   select mp).FirstOrDefault()
            //                        where c.TVItemID == cl.TVItemID
            //                        && c.TVPath.StartsWith(SubsectorInfo3.c.TVPath + "p")
            //                        && c.TVType == (int)TVTypeEnum.MWQMSite
            //                        && cl.Language == (int)LanguageEnum.en
            //                        && c.IsActive == true
            //                        select new { c, ml, mpl, cl }).Take(5).ToList();



            //    foreach (var mwqmsite in mwqmsiteList)
            //    {
            //        if (mwqmsite.mpl != null)
            //        {
            //            sbKMZ.AppendLine(@"	<Placemark>");
            //            sbKMZ.AppendLine(@"	<name>" + mwqmsite.cl.TVText + "</name>");
            //            //sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
            //            sbKMZ.AppendLine(@"	<Point>");
            //            sbKMZ.AppendLine(@"		<coordinates>" + mwqmsite.mpl.Lng + "," + mwqmsite.mpl.Lat + ",0</coordinates>");
            //            sbKMZ.AppendLine(@"	</Point>");
            //            sbKMZ.AppendLine(@"	</Placemark>");
            //        }
            //    }

            //    sbKMZ.AppendLine(@" </Folder>"); // Subsectors Active Pin            #endregion MWQM sites

            //    #endregion MWQM sites Active

            //    #region MWQM sites Inactive
            //    sbKMZ.AppendLine(@"	<Folder>");
            //    sbKMZ.AppendLine(@"	<name>Inactive</name>");

            //    mwqmsiteList = (from c in tvItemService.db.TVItems
            //                    from cl in tvItemService.db.TVItemLanguages
            //                    let ml = (from m in tvItemService.db.MapInfos
            //                              where m.TVItemID == c.TVItemID
            //                              && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                              select m).FirstOrDefault()
            //                    let mpl = (from mp in tvItemService.db.MapInfoPoints
            //                               where ml.MapInfoID == mp.MapInfoID
            //                               select mp).FirstOrDefault()
            //                    where c.TVItemID == cl.TVItemID
            //                    && c.TVPath.StartsWith(SubsectorInfo3.c.TVPath + "p")
            //                    && c.TVType == (int)TVTypeEnum.MWQMSite
            //                    && cl.Language == (int)LanguageEnum.en
            //                    && c.IsActive == false
            //                    select new { c, ml, mpl, cl }).Take(5).ToList();



            //    foreach (var mwqmsite in mwqmsiteList)
            //    {
            //        if (mwqmsite.mpl != null)
            //        {
            //            sbKMZ.AppendLine(@"	<Placemark>");
            //            sbKMZ.AppendLine(@"	<name>" + mwqmsite.cl.TVText + "</name>");
            //            //sbKMZ.AppendLine(@"<styleUrl>#msn_ylw-pushpin</styleUrl>");
            //            sbKMZ.AppendLine(@"	<Point>");
            //            sbKMZ.AppendLine(@"		<coordinates>" + mwqmsite.mpl.Lng + "," + mwqmsite.mpl.Lat + ",0</coordinates>");
            //            sbKMZ.AppendLine(@"	</Point>");
            //            sbKMZ.AppendLine(@"	</Placemark>");
            //        }
            //    }

            //    sbKMZ.AppendLine(@" </Folder>"); // Subsectors Active Pin            

            //    #endregion MWQM sites Inactive

            //    sbKMZ.AppendLine(@" </Folder>"); // Subsectors 

            //}

            //sbKMZ.AppendLine(@" </Folder>"); // Subsectors Inactive Polygon

            //#region Inactive Subsector Polygon

            //#endregion Inactive Subsectors

            //sbKMZ.AppendLine(@" </Folder>"); // Subsectors Inactive

            //sbKMZ.AppendLine(@" </Folder>"); // Subsectors 

            //#endregion Subsectors

            sbKMZ.AppendLine(@" </Folder>");


            sbKMZ.AppendLine(@"</Document>");
            sbKMZ.AppendLine(@"</kml>");

            FileInfo fi = new FileInfo(@"C:\Users\leblancc\Desktop\NL_CSSP_Map_Related_Infomation.kml");
            StreamWriter sw = fi.CreateText();

            sw.Write(sbKMZ.ToString());

            sw.Close();

            richTextBoxStatus.Text = "done....";
        }

        private void button14_Click(object sender, EventArgs e)
        {
            //CheckSubsector("NL");
            //CheckStation("NL");
            //CheckRun("NL");
            //CheckSample("NL");
        }
        private void CheckSubsector(string ShortProv)
        {
            StringBuilder sb = new StringBuilder();
            TempData.ASGADRun OldASGADRun = new TempData.ASGADRun();

            List<string> subsectorListCSSP = new List<string>();
            List<string> subsectorListStationASGAD = new List<string>();
            List<string> subsectorListRunASGAD = new List<string>();
            List<string> subsectorListSampleASGAD = new List<string>();
            using (CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities())
            {
                var subsectors = (from c in db.TVItems
                                  from cl in db.TVItemLanguages
                                  where c.TVItemID == cl.TVItemID
                                  && cl.TVText.StartsWith("NL-")
                                  && cl.Language == (int)LanguageEnum.en
                                  && c.TVType == (int)TVTypeEnum.Subsector
                                  orderby cl.TVText
                                  select cl.TVText).ToList();

                foreach (string s in subsectors)
                {
                    subsectorListCSSP.Add(s.Substring(0, s.IndexOf(" ")).Trim());
                }
            }

            /// ------------------------------------------ Checking Subsector Stations
            /// 
            using (TempData.TempDataToolDBEntities dbDT2 = new TempData.TempDataToolDBEntities())
            {
                var subsectors = (from c in dbDT2.ASGADStations
                                  where c.PROV == ShortProv
                                  orderby c.PROV, c.AREA, c.SECTOR, c.SUBSECTOR
                                  select new { ss = (c.PROV + "-" + c.AREA + "-" + c.SECTOR + "-" + c.SUBSECTOR) }).Distinct().ToList();

                foreach (var s in subsectors)
                {
                    subsectorListStationASGAD.Add(s.ss.Trim());
                }
            }

            sb.AppendLine("Checking Subsector Stations --- empty is ok");
            sb.AppendLine("");

            foreach (string s in subsectorListStationASGAD)
            {
                if (!subsectorListCSSP.Any(c => c == s))
                {
                    sb.AppendLine(s + " NO");
                }
            }

            /// ------------------------------------------ Checking Subsector Runs
            /// 
            using (TempData.TempDataToolDBEntities dbDT2 = new TempData.TempDataToolDBEntities())
            {
                var subsectors = (from c in dbDT2.ASGADRuns
                                  where c.PROV == ShortProv
                                  orderby c.PROV, c.AREA, c.SECTOR, c.SUBSECTOR
                                  select new { ss = (c.PROV + "-" + c.AREA + "-" + c.SECTOR + "-" + c.SUBSECTOR) }).Distinct().ToList();

                foreach (var s in subsectors)
                {
                    subsectorListRunASGAD.Add(s.ss.Trim());
                }
            }

            sb.AppendLine("Checking Subsector Subsectors --- empty is ok");
            sb.AppendLine("");

            foreach (string s in subsectorListRunASGAD)
            {
                if (!subsectorListCSSP.Any(c => c == s))
                {
                    sb.AppendLine(s + " NO");
                }
            }

            /// ------------------------------------------ Checking Subsector Samples
            /// 
            using (TempData.TempDataToolDBEntities dbDT2 = new TempData.TempDataToolDBEntities())
            {
                var subsectors = (from c in dbDT2.ASGADSamples
                                  where c.PROV == ShortProv
                                  orderby c.PROV, c.AREA, c.SECTOR, c.SUBSECTOR
                                  select new { ss = (c.PROV + "-" + c.AREA + "-" + c.SECTOR + "-" + c.SUBSECTOR) }).Distinct().ToList();

                foreach (var s in subsectors)
                {
                    subsectorListSampleASGAD.Add(s.ss.Trim());
                }
            }

            sb.AppendLine("Checking Subsector Samples --- empty is ok");
            sb.AppendLine("");

            foreach (string s in subsectorListSampleASGAD)
            {
                if (!subsectorListCSSP.Any(c => c == s))
                {
                    sb.AppendLine(s + " NO");
                }
            }

            sb.AppendLine("Done ...");

            richTextBoxStatus.Text = sb.ToString();
        }
        private void CheckStation(string ShortProv)
        {
            StringBuilder sb = new StringBuilder();
            TempData.ASGADRun OldASGADRun = new TempData.ASGADRun();

            sb.AppendLine("Subsector name\tCSSP Station Count\tASGAD Station Count");
            sb.AppendLine("");

            using (TempData.TempDataToolDBEntities dbDT2 = new TempData.TempDataToolDBEntities())
            {
                using (CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities())
                {
                    var subsectors = (from c in db.TVItems
                                      from cl in db.TVItemLanguages
                                      where c.TVItemID == cl.TVItemID
                                      && cl.TVText.StartsWith("NL-")
                                      && cl.Language == (int)LanguageEnum.en
                                      && c.TVType == (int)TVTypeEnum.Subsector
                                      orderby cl.TVText
                                      select new { c.TVItemID, cl.TVText }).ToList();

                    int total = subsectors.Count;
                    int count = 0;
                    foreach (var s in subsectors)
                    {
                        count += 1;
                        lblStatus.Text = count + " of " + total;
                        Application.DoEvents();

                        string Area = s.TVText.Substring(3, 2);
                        string Sector = s.TVText.Substring(6, 3);
                        string Subsector = s.TVText.Substring(10, 3);
                        var stationsASGAD = (from c in dbDT2.ASGADStations
                                             where c.PROV == ShortProv
                                             && c.AREA == Area
                                             && c.SECTOR == Sector
                                             && c.SUBSECTOR == Subsector
                                             orderby c.STAT_NBR
                                             select c).ToList();

                        var stationsCSSP = (from c in db.TVItems
                                            from cl in db.TVItemLanguages
                                            where c.TVItemID == cl.TVItemID
                                            && c.ParentID == s.TVItemID
                                            && cl.Language == (int)LanguageEnum.en
                                            && c.TVType == (int)TVTypeEnum.MWQMSite
                                            orderby cl.TVText
                                            select cl).ToList();

                        int ASGADCount = stationsASGAD.Count;
                        int CSSPCount = stationsCSSP.Count;

                        if (CSSPCount != ASGADCount)
                        {
                            sb.AppendLine(s.TVText + "\t" + CSSPCount + "\t" + ASGADCount);

                            int maxCount = Math.Max(CSSPCount, ASGADCount);
                            for (int i = 0; i < maxCount; i++)
                            {
                                sb.AppendLine("\t" + (i < CSSPCount ? stationsCSSP[i].TVText : "") + "\t" + (i < ASGADCount ? stationsASGAD[i].STAT_NBR : ""));
                            }
                        }
                    }
                }
            }

            //sb.AppendLine("Done ...");

            richTextBoxStatus.Text = sb.ToString();
        }
        private void CheckRun(string ShortProv)
        {
            StringBuilder sb = new StringBuilder();
            TempData.ASGADRun OldASGADRun = new TempData.ASGADRun();

            using (TempData.TempDataToolDBEntities dbDT2 = new TempData.TempDataToolDBEntities())
            {
                using (CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities())
                {
                    var subsectors = (from c in db.TVItems
                                      from cl in db.TVItemLanguages
                                      where c.TVItemID == cl.TVItemID
                                      && cl.TVText.StartsWith("NL-")
                                      && cl.Language == (int)LanguageEnum.en
                                      && c.TVType == (int)TVTypeEnum.Subsector
                                      orderby cl.TVText
                                      select new { c.TVItemID, cl.TVText }).ToList();

                    int total = subsectors.Count;
                    int count = 0;
                    foreach (var s in subsectors)
                    {
                        count += 1;
                        lblStatus.Text = count + " of " + total;
                        Application.DoEvents();

                        string Area = s.TVText.Substring(3, 2);
                        string Sector = s.TVText.Substring(6, 3);
                        string Subsector = s.TVText.Substring(10, 3);
                        List<TempData.ASGADRun> runsASGAD = (from c in dbDT2.ASGADRuns
                                                             where c.PROV == ShortProv
                                                             && c.AREA == Area
                                                             && c.SECTOR == Sector
                                                             && c.SUBSECTOR == Subsector
                                                             orderby c.SAMP_DATE
                                                             select c).ToList();

                        if (runsASGAD.Count > 0)
                        {
                            TempData.ASGADRun arOld = new TempData.ASGADRun();
                            foreach (TempData.ASGADRun ar in runsASGAD)
                            {
                                if (arOld.SAMP_DATE == ar.SAMP_DATE)
                                {
                                    sb.AppendLine(s.TVText + " same date [" + ar.SAMP_DATE + "]");
                                }
                                arOld = ar;
                            }
                        }

                        //var runsCSSP = (from c in db.TVItems
                        //                    from cl in db.TVItemLanguages
                        //                    where c.TVItemID == cl.TVItemID
                        //                    && c.ParentID == s.TVItemID
                        //                    && cl.Language == (int)LanguageEnum.en
                        //                    && c.TVType == (int)TVTypeEnum.MWQMRun
                        //                    orderby cl.TVText
                        //                    select cl).ToList();

                        //int ASGADCount = runsASGAD.Count;
                        //int CSSPCount = runsCSSP.Count;

                        //if (CSSPCount != ASGADCount)
                        //{
                        //    sb.AppendLine(s.TVText + "\t" + CSSPCount + "\t" + ASGADCount);

                        //    //int maxCount = Math.Max(CSSPCount, ASGADCount);
                        //    //for (int i = 0; i < maxCount; i++)
                        //    //{
                        //    //    sb.AppendLine("\t" + (i < CSSPCount ? runsCSSP[i].TVText : "") + "\t" + (i < ASGADCount ? ((DateTime)runsASGAD[i].SAMP_DATE).ToString("yyyy MM dd") : ""));
                        //    //}
                        //}
                    }
                }
            }

            //sb.AppendLine("Done ...");

            richTextBoxStatus.Text = sb.ToString();
        }
        private void CheckSample(string ShortProv)
        {
            StringBuilder sb = new StringBuilder();
            TempData.ASGADRun OldASGADRun = new TempData.ASGADRun();

            using (TempData.TempDataToolDBEntities dbDT2 = new TempData.TempDataToolDBEntities())
            {
                using (CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities())
                {
                    var subsectors = (from c in db.TVItems
                                      from cl in db.TVItemLanguages
                                      where c.TVItemID == cl.TVItemID
                                      && cl.TVText.StartsWith("NL-")
                                      && cl.Language == (int)LanguageEnum.en
                                      && c.TVType == (int)TVTypeEnum.Subsector
                                      orderby cl.TVText
                                      select new { c.TVItemID, cl.TVText }).ToList();

                    int total = subsectors.Count;
                    int count = 0;
                    foreach (var s in subsectors)
                    {
                        count += 1;
                        lblStatus.Text = count + " of " + total;
                        Application.DoEvents();

                        string Area = s.TVText.Substring(3, 2);
                        string Sector = s.TVText.Substring(6, 3);
                        string Subsector = s.TVText.Substring(10, 3);
                        List<TempData.ASGADSample> sampleASGAD = (from c in dbDT2.ASGADSamples
                                                                  where c.PROV == ShortProv
                                                                  && c.AREA == Area
                                                                  && c.SECTOR == Sector
                                                                  && c.SUBSECTOR == Subsector
                                                                  orderby c.STAT_NBR, c.SAMP_DATE
                                                                  select c).ToList();

                        if (sampleASGAD.Count > 0)
                        {
                            TempData.ASGADSample arOld = new TempData.ASGADSample();
                            foreach (TempData.ASGADSample ar in sampleASGAD)
                            {
                                if (arOld.SAMP_DATE == ar.SAMP_DATE)
                                {
                                    //if (arOld.FEC_COL == ar.FEC_COL && arOld.SALINITY == ar.SALINITY && arOld.WATERTEMP == ar.WATERTEMP)
                                    //{
                                    //    dbDT2.ASGADSamples.Remove(ar);
                                    //}
                                    sb.AppendLine(s.TVText + " same date [" + ar.SAMP_DATE + "]");
                                }
                                arOld = ar;
                            }
                            //dbDT2.SaveChanges();
                        }
                    }
                }
            }

            richTextBoxStatus.Text = sb.ToString();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            bool Start = false;
            int SubsectorID = 785;
            int CurrentYear = 2015;
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMSubsectorService mwqmSubsectorService = new MWQMSubsectorService(LanguageEnum.en, user);
            AppTaskService appTaskService = new AppTaskService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                return;
            }

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Subsector).Where(c => c.TVText.StartsWith("NL-")).OrderBy(c => c.TVText).ToList();
            if (tvItemModelSubsectorList.Count == 0)
            {
                return;
            }


            foreach (TVItemModel tvItemModel in tvItemModelSubsectorList)
            {

                // get all years with data
                List<int> YearWithData = mwqmSubsectorService.GetMWQMSubsectorRunsYears(tvItemModel.TVItemID);


                foreach (int year in YearWithData)
                {
                    lblStatus.Text = tvItemModel.TVText + " ----- " + year;
                    lblStatus.Refresh();
                    Application.DoEvents();

                    if (SubsectorID == tvItemModel.TVItemID && CurrentYear == year)
                    {
                        Start = true;
                    }

                    if (Start)
                    {
                        AppTaskModel appTaskModel = mwqmSubsectorService.ClimateSiteGetDataForRunsOfYearDB(tvItemModel.TVItemID, year);
                        if (!string.IsNullOrWhiteSpace(appTaskModel.Error))
                        {
                            return;
                        }

                        int AppTaskID = appTaskModel.AppTaskID;
                        bool Working = true;
                        while (Working)
                        {
                            Thread.Sleep(500);
                            AppTask appTaskExist = appTaskService.GetAppTaskWithAppTaskIDDB(appTaskModel.AppTaskID);
                            if (appTaskExist == null)
                            {
                                Working = false;
                            }
                        }
                    }
                }
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            bool Start = false;
            int SubsectorID = 660;
            int CurrentYear = 1988;
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMSubsectorService mwqmSubsectorService = new MWQMSubsectorService(LanguageEnum.en, user);
            AppTaskService appTaskService = new AppTaskService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                return;
            }

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Subsector).Where(c => c.TVText.StartsWith("NB-")).OrderBy(c => c.TVText).ToList();
            if (tvItemModelSubsectorList.Count == 0)
            {
                return;
            }


            foreach (TVItemModel tvItemModel in tvItemModelSubsectorList)
            {

                // get all years with data
                List<int> YearWithData = mwqmSubsectorService.GetMWQMSubsectorRunsYears(tvItemModel.TVItemID);


                foreach (int year in YearWithData)
                {
                    lblStatus.Text = tvItemModel.TVText + " ----- " + year;
                    lblStatus.Refresh();
                    Application.DoEvents();

                    if (SubsectorID == tvItemModel.TVItemID && CurrentYear == year)
                    {
                        Start = true;
                    }

                    if (Start)
                    {
                        MWQMSubsectorModel mwqmSubsectorModel = mwqmSubsectorService.ClimateSiteSetDataToUseByAverageOrPriorityDB(tvItemModel.TVItemID, year, "Priority");
                        if (!string.IsNullOrWhiteSpace(mwqmSubsectorModel.Error))
                        {
                            //return;
                        }
                    }
                }
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
            AppTaskService appTaskService = new AppTaskService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                return;
            }

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Subsector).Where(c => c.TVText.StartsWith("NB-")).OrderBy(c => c.TVText).ToList();
            if (tvItemModelSubsectorList.Count == 0)
            {
                return;
            }

            foreach (TVItemModel tvItemModel in tvItemModelSubsectorList)
            {
                List<MWQMRunModel> mwqmRunModelList = mwqmRunService.GetMWQMRunModelListWithSubsectorTVItemIDDB(tvItemModel.TVItemID);

                foreach (MWQMRunModel mwqmRunModel in mwqmRunModelList)
                {
                    lblStatus.Text = tvItemModel.TVText + "\t" + mwqmRunModel.MWQMRunTVText + "\t";
                    lblStatus.Refresh();
                    Application.DoEvents();

                    string RainMissing = "";
                    if (mwqmRunModel.RainDay0_mm == null)
                    {
                        RainMissing = RainMissing + " RainDay0\t";
                    }
                    if (mwqmRunModel.RainDay1_mm == null)
                    {
                        RainMissing = RainMissing + " RainDay1\t";
                    }
                    if (mwqmRunModel.RainDay2_mm == null)
                    {
                        RainMissing = RainMissing + " RainDay2\t";
                    }
                    if (mwqmRunModel.RainDay3_mm == null)
                    {
                        RainMissing = RainMissing + " RainDay3\t";
                    }
                    if (mwqmRunModel.RainDay4_mm == null)
                    {
                        RainMissing = RainMissing + " RainDay4\t";
                    }
                    if (mwqmRunModel.RainDay5_mm == null)
                    {
                        RainMissing = RainMissing + " RainDay5\t";
                    }
                    if (mwqmRunModel.RainDay6_mm == null)
                    {
                        RainMissing = RainMissing + " RainDay6\t";
                    }
                    if (mwqmRunModel.RainDay7_mm == null)
                    {
                        RainMissing = RainMissing + " RainDay7\t";
                    }
                    if (mwqmRunModel.RainDay8_mm == null)
                    {
                        RainMissing = RainMissing + " RainDay8\t";
                    }
                    if (mwqmRunModel.RainDay9_mm == null)
                    {
                        RainMissing = RainMissing + " RainDay9\t";
                    }
                    if (mwqmRunModel.RainDay10_mm == null)
                    {
                        RainMissing = RainMissing + " RainDay10\t";
                    }

                    if (!string.IsNullOrWhiteSpace(RainMissing))
                    {
                        richTextBoxStatus.AppendText(lblStatus.Text + RainMissing + "\r\n");
                    }
                }
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            string provText = "Île-du-Prince-Édouard";
            StringBuilder sb = new StringBuilder();
            LanguageEnum language = LanguageEnum.fr;
            TVItemLanguageService tvItemLanguageService = new TVItemLanguageService(language, user);
            MWQMRunService mwqmRunService = new MWQMRunService(language, user);

            using (CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities())
            {
                TVItem tvItemProv = (from c in db.TVItems
                                     from cl in db.TVItemLanguages
                                     where c.TVItemID == cl.TVItemID
                                     && cl.Language == (int)language
                                     && cl.TVText == provText
                                     && c.TVType == (int)TVTypeEnum.Province
                                     select c).FirstOrDefault();

                if (tvItemProv == null)
                {
                    richTextBoxStatus.Text = "could not find TVItem [" + provText + "]";
                    return;
                }
                var tvItemSubsectorList = (from c in db.TVItems
                                           from cl in db.TVItemLanguages
                                           where c.TVItemID == cl.TVItemID
                                           && cl.Language == (int)language
                                           && c.TVPath.StartsWith(tvItemProv.TVPath + "p")
                                           && c.TVType == (int)TVTypeEnum.Subsector
                                           select new { c, cl }).ToList();

                foreach (var ss in tvItemSubsectorList)
                {
                    Application.DoEvents();

                    List<MWQMRun> runList = (from c in db.MWQMRuns
                                             where c.SubsectorTVItemID == ss.c.TVItemID
                                             orderby c.MWQMRunTVItemID
                                             select c).ToList();

                    // delete any duplicate runs with the same MWQMRunTVItemID
                    var oldRunTVItemID = 0;
                    foreach (MWQMRun r in runList)
                    {
                        lblStatus.Text = ss.cl.TVText + " --- " + r.DateTime_Local;
                        lblStatus.Refresh();
                        Application.DoEvents();

                        if (oldRunTVItemID == r.MWQMRunTVItemID)
                        {
                            sb.AppendLine("MWQMRunTVItemID is dupliate [" + r.MWQMRunTVItemID + "]");
                            db.MWQMRuns.Remove(r);
                        }
                        oldRunTVItemID = r.MWQMRunTVItemID;
                    }

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        int asefsef = 34;
                    }

                    // Rename TVItemLanguage to the right date of MWQMRun
                    var TVItemLangMWQMRun = (from c in db.TVItems
                                             from cl in db.TVItemLanguages
                                             from r in db.MWQMRuns
                                             from rl in db.MWQMRunLanguages
                                             where c.TVItemID == cl.TVItemID
                                             && c.TVItemID == r.MWQMRunTVItemID
                                             && r.MWQMRunID == rl.MWQMRunID
                                             && rl.Language == (int)language
                                             && cl.Language == (int)language
                                             && c.TVType == (int)TVTypeEnum.MWQMRun
                                             && c.TVPath.StartsWith(ss.c.TVPath + "p")
                                             select new { r }).ToList();

                    foreach (var tr in TVItemLangMWQMRun)
                    {
                        lblStatus.Text = ss.cl.TVText + " --- " + tr.r.DateTime_Local;
                        lblStatus.Refresh();
                        Application.DoEvents();

                        string TVText = "" + tr.r.DateTime_Local.Year.ToString() + " " +
                            (tr.r.DateTime_Local.Month > 9 ? tr.r.DateTime_Local.Month.ToString() : "0" + tr.r.DateTime_Local.Month.ToString()) + " " +
                            (tr.r.DateTime_Local.Day > 9 ? tr.r.DateTime_Local.Day.ToString() : "0" + tr.r.DateTime_Local.Day.ToString());

                        TVItemLanguageModel tvItemLanguageModel = tvItemLanguageService.GetTVItemLanguageModelWithTVItemIDAndLanguageDB(tr.r.MWQMRunTVItemID, language);
                        if (!string.IsNullOrWhiteSpace(tvItemLanguageModel.Error))
                        {
                            int sef = 34;
                        }

                        if (TVText != tvItemLanguageModel.TVText)
                        {
                            tvItemLanguageModel.TVText = TVText;
                            TVItemLanguageModel tvItemLanguageModelRet = tvItemLanguageService.PostUpdateTVItemLanguageDB(tvItemLanguageModel);
                            if (!string.IsNullOrWhiteSpace(tvItemLanguageModelRet.Error))
                            {
                                int sef = 34;
                            }
                            sb.AppendLine(tr.r.DateTime_Local + " to be changed to " + TVText);
                        }
                    }
                }
                sb.AppendLine("done...");
                richTextBoxStatus.Text = sb.ToString();
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            string provText = "Newfoundland and Labrador";
            StringBuilder sb = new StringBuilder();
            LanguageEnum language = LanguageEnum.en;
            TVItemLanguageService tvItemLanguageService = new TVItemLanguageService(language, user);
            MWQMRunService mwqmRunService = new MWQMRunService(language, user);

            using (CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities())
            {
                TVItem tvItemProv = (from c in db.TVItems
                                     from cl in db.TVItemLanguages
                                     where c.TVItemID == cl.TVItemID
                                     && cl.Language == (int)language
                                     && cl.TVText == provText
                                     && c.TVType == (int)TVTypeEnum.Province
                                     select c).FirstOrDefault();

                if (tvItemProv == null)
                {
                    richTextBoxStatus.Text = "could not find TVItem [" + provText + "]";
                    return;
                }

                sb.AppendLine("Subsector,Run");

                var tvItemSubsectorList = (from c in db.TVItems
                                           from cl in db.TVItemLanguages
                                           where c.TVItemID == cl.TVItemID
                                           && cl.Language == (int)language
                                           && c.TVPath.StartsWith(tvItemProv.TVPath + "p")
                                           && c.TVType == (int)TVTypeEnum.Subsector
                                           select new { c, cl }).ToList();

                foreach (var ss in tvItemSubsectorList)
                {
                    Application.DoEvents();

                    List<MWQMRun> runList = (from c in db.MWQMRuns
                                             where c.SubsectorTVItemID == ss.c.TVItemID
                                             orderby c.MWQMRunTVItemID
                                             select c).ToList();

                    foreach (MWQMRun r in runList)
                    {
                        lblStatus.Text = ss.cl.TVText + " --- " + r.DateTime_Local;
                        lblStatus.Refresh();
                        Application.DoEvents();

                        int countSample = (from c in db.MWQMSamples
                                           where c.MWQMRunTVItemID == r.MWQMRunTVItemID
                                           select c).Count();

                        if (countSample == 0)
                        {
                            string TVText = r.StartDateTime_Local.Value.Year.ToString() + " " +
                           (r.StartDateTime_Local.Value.Month > 9 ? r.StartDateTime_Local.Value.Month.ToString() : "0" + r.StartDateTime_Local.Value.Month.ToString()) + " " +
                           (r.StartDateTime_Local.Value.Day > 9 ? r.StartDateTime_Local.Value.Day.ToString() : "0" + r.StartDateTime_Local.Value.Day.ToString());

                            sb.AppendLine(ss.cl.TVText + "," + TVText);

                        }
                    }

                }
                sb.AppendLine("done...");
                richTextBoxStatus.Text = sb.ToString();
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            string provText = "New Brunswick";
            string provInit = "NB";
            StringBuilder sb = new StringBuilder();
            LanguageEnum language = LanguageEnum.en;
            MWQMSampleService sampleService = new MWQMSampleService(language, user);
            MWQMRunService mwqmRunService = new MWQMRunService(language, user);

            using (CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities())
            {
                TVItem tvItemProv = (from c in db.TVItems
                                     from cl in db.TVItemLanguages
                                     where c.TVItemID == cl.TVItemID
                                     && cl.Language == (int)language
                                     && cl.TVText == provText
                                     && c.TVType == (int)TVTypeEnum.Province
                                     select c).FirstOrDefault();

                if (tvItemProv == null)
                {
                    richTextBoxStatus.Text = "could not find TVItem [" + provText + "]";
                    return;
                }

                sb.AppendLine("Subsector\tRun\tSite\tASGAD FC\tCSSPWebTools FC");

                var tvItemSubsectorList = (from c in db.TVItems
                                           from cl in db.TVItemLanguages
                                           where c.TVItemID == cl.TVItemID
                                           && cl.Language == (int)language
                                           && c.TVPath.StartsWith(tvItemProv.TVPath + "p")
                                           && c.TVType == (int)TVTypeEnum.Subsector
                                           select new { c, cl }).ToList();

                foreach (var ss in tvItemSubsectorList)
                {
                    Application.DoEvents();

                    List<TempData.ASGADSample> asgadSampleList = new List<ASGADSample>();
                    using (TempDataToolDBEntities dbt = new TempDataToolDBEntities())
                    {
                        string Area = ss.cl.TVText.Substring(3, 2);
                        string Sector = ss.cl.TVText.Substring(6, 3);
                        string Subsector = ss.cl.TVText.Substring(10, 3);
                        asgadSampleList = (from c in dbt.ASGADSamples
                                           where c.PROV == provInit
                                           && c.AREA == Area
                                           && c.SECTOR == Sector
                                           && c.SUBSECTOR == Subsector
                                           select c).ToList();
                    }

                    List<int> mwqmSiteTVItemIDList = (from c in db.MWQMSites
                                                      from t in db.TVItems
                                                      from tl in db.TVItemLanguages
                                                      where c.MWQMSiteTVItemID == t.TVItemID
                                                      && t.TVItemID == tl.TVItemID
                                                      && t.TVType == (int)TVTypeEnum.MWQMSite
                                                      && tl.Language == (int)LanguageEnum.en
                                                      && t.TVPath.StartsWith(ss.c.TVPath + "p")
                                                      select c.MWQMSiteTVItemID).Distinct().ToList();

                    if (mwqmSiteTVItemIDList.Count > 0)
                    {
                        var sampleList = (from c in db.MWQMSamples
                                          from cl in db.TVItemLanguages
                                          from s in mwqmSiteTVItemIDList
                                          where c.MWQMSiteTVItemID == s
                                          && c.MWQMSiteTVItemID == cl.TVItemID
                                          && c.SampleTypesText.Contains("109,")
                                          && cl.Language == (int)LanguageEnum.en
                                          orderby c.SampleDateTime_Local
                                          select new { c, cl }).ToList();


                        foreach (var s in sampleList)
                        {
                            lblStatus.Text = ss.cl.TVText + " --- " + s.c.SampleDateTime_Local;
                            lblStatus.Refresh();
                            Application.DoEvents();

                            int? fcASGAD = (int?)((from c in asgadSampleList
                                                   where c.SAMP_DATE.Value.Year == s.c.SampleDateTime_Local.Year
                                                   && c.SAMP_DATE.Value.Month == s.c.SampleDateTime_Local.Month
                                                   && c.SAMP_DATE.Value.Day == s.c.SampleDateTime_Local.Day
                                                   && c.STAT_NBR == s.cl.TVText
                                                   select c.FEC_COL).FirstOrDefault());

                            if (s.c.FecCol_MPN_100ml != fcASGAD)
                            {
                                string TVText = s.c.SampleDateTime_Local.Year.ToString() + " " +
                                (s.c.SampleDateTime_Local.Month > 9 ? s.c.SampleDateTime_Local.Month.ToString() : "0" + s.c.SampleDateTime_Local.Month.ToString()) + " " +
                                (s.c.SampleDateTime_Local.Day > 9 ? s.c.SampleDateTime_Local.Day.ToString() : "0" + s.c.SampleDateTime_Local.Day.ToString());

                                sb.AppendLine(ss.cl.TVText + "\t" + TVText + "\t" + s.cl.TVText + "\t" + (fcASGAD == null ? "empty" : fcASGAD.ToString()) + "\t" + s.c.FecCol_MPN_100ml);
                            }
                        }
                    }
                }
                sb.AppendLine("done...");
                richTextBoxStatus.Text = sb.ToString();
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            string provText = "New Brunswick";
            string provInit = "NB";
            StringBuilder sb = new StringBuilder();
            LanguageEnum language = LanguageEnum.en;
            MWQMSampleService sampleService = new MWQMSampleService(language, user);
            MWQMRunService mwqmRunService = new MWQMRunService(language, user);
            TVItemService tvItemService = new TVItemService(language, user);

            sb.AppendLine("Subsector\tRun\tSite\tCSSPWebTools FC\tASGAD FC");

            using (TempDataToolDBEntities dbt = new TempDataToolDBEntities())
            {
                var asgadSSList = (from c in dbt.ASGADSamples
                                   where c.PROV == provInit
                                   orderby c.PROV, c.AREA, c.SECTOR, c.SUBSECTOR
                                   select new { ss = c.PROV + "-" + c.AREA + "-" + c.SECTOR + "-" + c.SUBSECTOR }).Distinct().ToList();


                foreach (var asgadSS in asgadSSList.OrderBy(c => c.ss))
                {
                    string Prov = asgadSS.ss.Substring(0, 2);
                    string Area = asgadSS.ss.Substring(3, 2);
                    string Sector = asgadSS.ss.Substring(6, 3);
                    string Subsector = asgadSS.ss.Substring(10, 3);
                    List<TempData.ASGADSample> asgadSampleList = (from c in dbt.ASGADSamples
                                                                  where c.PROV == Prov
                                                                  && c.AREA == Area
                                                                  && c.SECTOR == Sector
                                                                  && c.SUBSECTOR == Subsector
                                                                  orderby c.STAT_NBR, c.SAMP_DATE
                                                                  select c).ToList();

                    using (CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities())
                    {
                        TVItem tvItemProv = (from c in db.TVItems
                                             from cl in db.TVItemLanguages
                                             where c.TVItemID == cl.TVItemID
                                             && cl.Language == (int)language
                                             && cl.TVText == provText
                                             && c.TVType == (int)TVTypeEnum.Province
                                             select c).FirstOrDefault();

                        if (tvItemProv == null)
                        {
                            richTextBoxStatus.Text = "could not find TVItem [" + provText + "]\r\n";
                            return;
                        }

                        TVItemModel tvItemModelSS = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemProv.TVItemID, asgadSS.ss + " ", TVTypeEnum.Subsector);
                        if (!string.IsNullOrWhiteSpace(tvItemModelSS.Error))
                        {
                            richTextBoxStatus.Text = tvItemModelSS.Error + "\r\n";
                            return;
                        }

                        var sampleList = (from t in db.TVItems
                                          from tl in db.TVItemLanguages
                                          from s in db.MWQMSamples
                                          where t.TVItemID == tl.TVItemID
                                          && t.TVItemID == s.MWQMSiteTVItemID
                                          && t.TVType == (int)TVTypeEnum.MWQMSite
                                          && t.ParentID == tvItemModelSS.TVItemID
                                          && tl.Language == (int)LanguageEnum.en
                                          select new { tl.TVText, s }).ToList();

                        if (sampleList.Count == 0)
                        {
                            richTextBoxStatus.Text = "sampleList == 0\r\n";
                            return;
                        }

                        foreach (TempData.ASGADSample asgadSample in asgadSampleList)
                        {
                            lblStatus.Text = tvItemModelSS.TVText + " --- " + asgadSample.STAT_NBR + " --- " + asgadSample.SAMP_DATE;
                            lblStatus.Refresh();
                            Application.DoEvents();

                            int? fcASGAD = (int?)((from c in sampleList
                                                   where c.s.SampleDateTime_Local.Year == asgadSample.SAMP_DATE.Value.Year
                                                   && c.s.SampleDateTime_Local.Month == asgadSample.SAMP_DATE.Value.Month
                                                   && c.s.SampleDateTime_Local.Day == asgadSample.SAMP_DATE.Value.Day
                                                   && c.TVText == asgadSample.STAT_NBR
                                                   select c.s.FecCol_MPN_100ml).FirstOrDefault());

                            int FC = (int)((asgadSample.FEC_COL == 1.9 ? 1 : asgadSample.FEC_COL));
                            if (FC != fcASGAD)
                            {
                                string TVText = asgadSample.SAMP_DATE.Value.Year.ToString() + " " +
                                (asgadSample.SAMP_DATE.Value.Month > 9 ? asgadSample.SAMP_DATE.Value.ToString() : "0" + asgadSample.SAMP_DATE.Value.Month.ToString()) + " " +
                                (asgadSample.SAMP_DATE.Value.Day > 9 ? asgadSample.SAMP_DATE.Value.ToString() : "0" + asgadSample.SAMP_DATE.Value.ToString());

                                sb.AppendLine(tvItemModelSS.TVText + "\t" + TVText + "\t" + asgadSample.STAT_NBR + "\t" + (fcASGAD == null ? "empty" : fcASGAD.ToString()) + "\t" + asgadSample.FEC_COL);
                            }
                        }
                    }
                }
            }
            sb.AppendLine("done...");
            richTextBoxStatus.Text = sb.ToString();
        }

        private void button22_Click(object sender, EventArgs e)
        {
            using (CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities())
            {
                string provText = "Québec";
                MWQMSampleService sampleService = new MWQMSampleService(LanguageEnum.en, user);
                MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
                TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

                TVItem tvItemProv = (from c in db.TVItems
                                     from cl in db.TVItemLanguages
                                     where c.TVItemID == cl.TVItemID
                                     && cl.Language == (int)LanguageEnum.en
                                     && cl.TVText == provText
                                     && c.TVType == (int)TVTypeEnum.Province
                                     select c).FirstOrDefault();

                if (tvItemProv == null)
                {
                    richTextBoxStatus.Text = "could not find TVItem [" + provText + "]\r\n";
                    return;
                }

                List<TVItemModel> tvItemModelSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemProv.TVItemID, TVTypeEnum.Subsector);
                if (tvItemModelSSList.Count == 0)
                {
                    richTextBoxStatus.Text = "could not find Québec subsectors [" + provText + "]\r\n";
                    return;
                }

                foreach (TVItemModel tvItemModelSS in tvItemModelSSList)
                {
                    string Subsector = tvItemModelSS.TVText;

                    lblStatus.Text = Subsector;
                    Application.DoEvents();

                    List<TVItemModel> tvItemModelSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.MWQMSite);
                    if (tvItemModelSSList.Count == 0)
                    {
                        richTextBoxStatus.Text = "could not find Québec subsectors [" + provText + "]\r\n";
                        return;
                    }

                    foreach (TVItemModel tvItemModelSite in tvItemModelSiteList.OrderBy(c => c.TVText))
                    {
                        string Site = tvItemModelSite.TVText;

                        lblStatus.Text = Subsector + " --- " + Site;
                        richTextBoxStatus.AppendText(Subsector + " --- " + Site + "\r\n");
                        Application.DoEvents();

                        DeleteSite(tvItemModelSite);
                    }
                }
            }
        }

        private void DeleteSite(TVItemModel tvItemModelSite)
        {
            using (CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities())
            {
                List<MWQMSample> mwqmSampleList = (from c in db.MWQMSamples
                                                   where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
                                                   select c).ToList();

                if (mwqmSampleList.Count > 0)
                {
                    db.MWQMSamples.RemoveRange(mwqmSampleList);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        richTextBoxStatus.Text = "could not delete mwqm samples\r\n";
                        richTextBoxStatus.Text = ex.Message + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "");
                        return;
                    }
                }

                Application.DoEvents();
                List<MapInfo> mapInfoList = (from c in db.MapInfos
                                             where c.TVItemID == tvItemModelSite.TVItemID
                                             select c).ToList();

                if (mapInfoList.Count > 0)
                {
                    db.MapInfos.RemoveRange(mapInfoList);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        richTextBoxStatus.Text = "could not delete MapInfo\r\n";
                        richTextBoxStatus.Text = ex.Message + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "");
                        return;
                    }
                }

                Application.DoEvents();
                List<MWQMSiteStartEndDate> mwqmSiteStartEndDateList = (from c in db.MWQMSiteStartEndDates
                                                                       where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
                                                                       select c).ToList();

                if (mwqmSiteStartEndDateList != null)
                {
                    db.MWQMSiteStartEndDates.RemoveRange(mwqmSiteStartEndDateList);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        richTextBoxStatus.Text = "could not delete mwqmSiteStartEndDate\r\n";
                        richTextBoxStatus.Text = ex.Message + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "");
                        return;
                    }
                }

                Application.DoEvents();
                MWQMSite mwqmSite = (from c in db.MWQMSites
                                     where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
                                     select c).FirstOrDefault();

                if (mwqmSite != null)
                {
                    db.MWQMSites.Remove(mwqmSite);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        richTextBoxStatus.Text = "could not delete mwqmSite\r\n";
                        richTextBoxStatus.Text = ex.Message + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "");
                        return;
                    }
                }

                Application.DoEvents();
                TVItem tvItemSite = (from c in db.TVItems
                                     where c.TVItemID == tvItemModelSite.TVItemID
                                     select c).FirstOrDefault();

                if (tvItemSite != null)
                {
                    db.TVItems.Remove(tvItemSite);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        richTextBoxStatus.Text = "could not delete tvItemSite\r\n";
                        richTextBoxStatus.Text = ex.Message + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "");
                        return;
                    }
                }
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {
                PCCSM.geo_secteur_s sec = (from s in dbQC.geo_secteur_s
                                           where s.id_geo_secteur_s == 16
                                           select s).FirstOrDefault();

                if (sec != null)
                {
                    var geo = DbGeometry.PolygonFromBinary(sec.geometry, DbGeometry.DefaultCoordinateSystemId);
                    int sef = 34;
                }
            }

        }

        private void button24_Click(object sender, EventArgs e)
        {
            using (CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities())
            {
                var SSList = (from c in db.TVItems
                              from cl in db.TVItemLanguages
                              where c.TVItemID == cl.TVItemID
                              && c.TVType == (int)TVTypeEnum.Subsector
                              && cl.Language == (int)LanguageEnum.en
                              select new { c, cl }).ToList();

                foreach (var ss in SSList)
                {
                    List<string> SiteNameList = (from c in db.TVItems
                                                 from cl in db.TVItemLanguages
                                                 where c.TVItemID == cl.TVItemID
                                                 && c.TVType == (int)TVTypeEnum.MWQMSite
                                                 && cl.Language == (int)LanguageEnum.en
                                                 && c.TVPath.StartsWith(ss.c.TVPath + "p")
                                                 select cl.TVText).ToList();

                    for (int i = 0, count = SiteNameList.Count; i < count; i++)
                    {
                        lblStatus.Text = ss.cl.TVText + " -- " + SiteNameList[i] + " -- " + i;
                        lblStatus.Refresh();
                        Application.DoEvents();

                        if (SiteNameList[i].StartsWith("0"))
                        {
                            SiteNameList[i] = SiteNameList[i].Substring(1);
                        }

                    }

                    SiteNameList = SiteNameList.OrderBy(c => c).ToList();

                    for (int i = 0, count = SiteNameList.Count - 1; i < count; i++)
                    {
                        if (SiteNameList[i] == SiteNameList[i + 1])
                        {
                            richTextBoxStatus.AppendText(ss.cl.TVText.Substring(0, ss.cl.TVText.IndexOf(" ")) + "\t" + SiteNameList[i] + "\r\n");
                        }
                    }

                }
            }
        }

        private void button25_Click(object sender, EventArgs e)
        {
            //string FileName = @"C:\CSSP Latest Code Old\DataTool\ImportByFunction\Data_inputs\NB Station Master List WGS84.xlsx";

            //string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=Excel 12.0";

            //OleDbConnection conn = new OleDbConnection(connectionString);

            //conn.Open();
            //OleDbDataReader reader;
            //OleDbCommand comm = new OleDbCommand("Select * from [Feuil1$];");

            //comm.Connection = conn;
            //reader = comm.ExecuteReader();

            //int CountRead = 0;
            //while (reader.Read())
            //{
            //    CountRead += 1;
            //    if (CountRead < 0)
            //        continue;

            //    Application.DoEvents();

            //    string STAT_NBR = "";
            //    float Lat = 0.0f;
            //    float Lng = 0.0f;
            //    string Subsector = "";

            //    // STAT_NBR
            //    if (reader.GetValue(4).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(4).ToString()))
            //    {
            //        STAT_NBR = "";
            //    }
            //    else
            //    {
            //        STAT_NBR = reader.GetValue(4).ToString().Trim();
            //        if (STAT_NBR.Length < 4)
            //        {
            //            STAT_NBR = "0000".Substring(0, 4 - STAT_NBR.Length) + STAT_NBR;
            //        }
            //        else if (STAT_NBR.Length > 4)
            //        {
            //            STAT_NBR = STAT_NBR + " ------- Too long";
            //        }
            //        else
            //        {
            //            // nothing
            //        }
            //    }

            //    // lat_wgs
            //    if (reader.GetValue(22).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(22).ToString()))
            //    {
            //        Lat = 0.0f;
            //    }
            //    else
            //    {
            //        Lat = float.Parse(reader.GetValue(22).ToString());
            //    }

            //    // long_wgs
            //    if (reader.GetValue(23).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(23).ToString()))
            //    {
            //        Lng = 0.0f;
            //    }
            //    else
            //    {
            //        Lng = float.Parse(reader.GetValue(23).ToString());
            //    }

            //    // Subsector
            //    if (reader.GetValue(26).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(26).ToString()))
            //    {
            //        Subsector = "";
            //    }
            //    else
            //    {
            //        Subsector = reader.GetValue(26).ToString().Trim();
            //    }

            //    richTextBoxStatus.AppendText(CountRead + "\t");
            //    richTextBoxStatus.AppendText(STAT_NBR + "\t");
            //    richTextBoxStatus.AppendText(Lat + "\t");
            //    richTextBoxStatus.AppendText(Lng + "\t");
            //    richTextBoxStatus.AppendText(Subsector + "\r\n");

            //    if (string.IsNullOrWhiteSpace(STAT_NBR))
            //    {
            //        richTextBoxStatus.AppendText("STAT_NBR is empty at line " + CountRead.ToString());
            //        break;
            //    }

            //    if (Lat == 0.0f)
            //    {
            //        richTextBoxStatus.AppendText("Subsector is empty at line " + CountRead.ToString());
            //        break;
            //    }

            //    if (Lng == 0.0f)
            //    {
            //        richTextBoxStatus.AppendText("Lng is empty at line " + CountRead.ToString());
            //        break;
            //    }
            //    if (string.IsNullOrWhiteSpace(Subsector))
            //    {
            //        richTextBoxStatus.AppendText("Subsector is empty at line " + CountRead.ToString());
            //        break;
            //    }

            //    using (CSSPWebToolsDBEntities dd = new CSSPWebToolsDBEntities())
            //    {
            //        TVItem tvItemSS = (from t in dd.TVItems
            //                           from c in dd.TVItemLanguages
            //                           where t.TVItemID == c.TVItemID
            //                           && c.TVText.StartsWith(Subsector)
            //                           && c.Language == (int)LanguageEnum.en
            //                           select t).FirstOrDefault();

            //        if (tvItemSS == null)
            //        {
            //            richTextBoxStatus.AppendText("Could not find subsector " + Subsector + " at line " + CountRead.ToString());
            //            continue;
            //        }

            //        TVItem tvItemSite = (from t in dd.TVItems
            //                             from tl in dd.TVItemLanguages
            //                             where t.TVItemID == tl.TVItemID
            //                             && tl.Language == (int)LanguageEnum.en
            //                             && t.TVType == (int)TVTypeEnum.MWQMSite
            //                             && tl.TVText == STAT_NBR
            //                             && t.ParentID == tvItemSS.TVItemID
            //                             select t).FirstOrDefault();

            //        if (tvItemSite == null)
            //        {
            //            richTextBoxStatus.AppendText("Could not find site with STAT_NBR and Subsector " + STAT_NBR + " -- " + Subsector + " at line " + CountRead.ToString());
            //            continue;
            //        }

            //        MapInfoPoint mapInfoPoint = (from m in dd.MapInfos
            //                                     from mp in dd.MapInfoPoints
            //                                     where m.MapInfoID == mp.MapInfoID
            //                                     && m.TVItemID == tvItemSite.TVItemID
            //                                     && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                                     && m.TVType == (int)TVTypeEnum.MWQMSite
            //                                     select mp).FirstOrDefault();

            //        if (mapInfoPoint != null)
            //        {
            //            mapInfoPoint.Lat = Lat;
            //            mapInfoPoint.Lng = Lng;

            //            try
            //            {
            //                dd.SaveChanges();
            //                richTextBoxStatus.AppendText("Saved " + STAT_NBR + " -- " + Subsector + "\r\n");
            //            }
            //            catch (Exception ex)
            //            {
            //                richTextBoxStatus.AppendText("Error while saving MapInfoPoint " + ex.Message + (ex.InnerException == null ? "" : " Inner: " + ex.InnerException.Message) + " at line " + CountRead.ToString());
            //                continue;
            //            }
            //        }
            //        else
            //        {
            //            richTextBoxStatus.AppendText("Could not find MapInfoPoint for " + STAT_NBR + " --- " + Subsector);
            //            continue;
            //        }
            //    }
            //}

        }

        private void button26_Click(object sender, EventArgs e)
        {
            //MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            //int count = 0;
            //using (CSSPWebToolsDBEntities dd = new CSSPWebToolsDBEntities())
            //{
            //    var tvItemSSList = (from t in dd.TVItems
            //                        from tl in dd.TVItemLanguages
            //                        where t.TVItemID == tl.TVItemID
            //                        && tl.TVText.StartsWith("NB-")
            //                        && tl.Language == (int)LanguageEnum.en
            //                        && t.TVType == (int)TVTypeEnum.Subsector
            //                        select new { t, tl }).ToList();

            //    foreach (var itemSS in tvItemSSList)
            //    {
            //        List<MapInfoPoint> mapInfoPointSSPolygon = (from m in dd.MapInfos
            //                                                    from mp in dd.MapInfoPoints
            //                                                    where m.MapInfoID == mp.MapInfoID
            //                                                    && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon
            //                                                    && m.TVType == (int)TVTypeEnum.Subsector
            //                                                    && m.TVItemID == itemSS.t.TVItemID
            //                                                    select mp).ToList();

            //        if (mapInfoPointSSPolygon != null)
            //        {
            //            List<Coord> coordList = new List<Coord>();

            //            foreach (MapInfoPoint mp in mapInfoPointSSPolygon)
            //            {
            //                coordList.Add(new Coord() { Lat = (float)mp.Lat, Lng = (float)mp.Lng, Ordinal = mp.Ordinal });
            //            }

            //            var tvItemSiteList = (from t in dd.TVItems
            //                                  from tl in dd.TVItemLanguages
            //                                  where t.TVItemID == tl.TVItemID
            //                                  && t.ParentID == itemSS.t.TVItemID
            //                                  && tl.Language == (int)LanguageEnum.en
            //                                  && t.TVType == (int)TVTypeEnum.MWQMSite
            //                                  select new { t, tl }).ToList();

            //            foreach (var itemSite in tvItemSiteList)
            //            {
            //                count += 1;
            //                lblStatus.Text = count + " " + itemSS.tl.TVText + " --- " + itemSite.tl.TVText;
            //                lblStatus.Refresh();
            //                Application.DoEvents();

            //                MapInfoPoint mapInfoPointSite = (from m in dd.MapInfos
            //                                                 from mp in dd.MapInfoPoints
            //                                                 where m.MapInfoID == mp.MapInfoID
            //                                                 && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                                                 && m.TVType == (int)TVTypeEnum.MWQMSite
            //                                                 && m.TVItemID == itemSite.t.TVItemID
            //                                                 select mp).FirstOrDefault();

            //                if (mapInfoPointSite == null)
            //                {
            //                    string Subsector = itemSS.tl.TVText;
            //                    Subsector = Subsector.Substring(0, Subsector.IndexOf(" "));

            //                    richTextBoxStatus.AppendText("No map point for " + Subsector + "\t" + itemSite.tl.TVText + "\r\n");
            //                }
            //                else
            //                {
            //                    Coord SitePoint = new Coord() { Lat = (float)mapInfoPointSite.Lat, Lng = (float)mapInfoPointSite.Lng, Ordinal = 0 };

            //                    bool IsOut = !mapInfoService.CoordInPolygon(coordList, SitePoint);

            //                    if (IsOut)
            //                    {
            //                        string Subsector = itemSS.tl.TVText;
            //                        Subsector = Subsector.Substring(0, Subsector.IndexOf(" "));

            //                        richTextBoxStatus.AppendText(Subsector + "\t" + itemSite.tl.TVText + "\t");
            //                        richTextBoxStatus.AppendText(@"http://wmon01dtchlebl2/csspwebtools/en-CA/#!View/a|||" + itemSS.t.TVItemID + @"|||31010000000000000000000000001000" + "\r\n");
            //                    }
            //                }
            //            }
            //        }

            //    }
            //}
        }

        private void button27_Click(object sender, EventArgs e)
        {
            //List<PolSourceSite> polSourceSiteList = null;
            //using (CSSPWebToolsDBEntities dd = new CSSPWebToolsDBEntities())
            //{
            //    polSourceSiteList = (from c in dd.PolSourceSites
            //                         select c).ToList();
            //}

            //while (true)
            //{
            //    using (CSSPWebToolsDBEntities dd = new CSSPWebToolsDBEntities())
            //    {
            //        List<PolSourceObservation> polSourceObsList = (from c in dd.PolSourceObservations
            //                                                       where c.PolSourceSiteID == null
            //                                                       orderby c.PolSourceObservationID
            //                                                       select c).Take(200).ToList();


            //        if (polSourceObsList.Count == 0)
            //        {
            //            break;
            //        }

            //        lblStatus.Text = "Doing PolSourceObservation " + polSourceObsList[0].PolSourceObservationID;
            //        lblStatus.Refresh();
            //        Application.DoEvents();

            //        foreach (PolSourceObservation pso in polSourceObsList)
            //        {
            //            PolSourceSite pss = (from c in polSourceSiteList
            //                                 where c.PolSourceSiteTVItemID == pso.PolSourceSiteTVItemID
            //                                 select c).FirstOrDefault();

            //            if (pss == null)
            //            {
            //                int slefji = 23;
            //            }

            //            pso.PolSourceSiteID = pss.PolSourceSiteID;
            //        }
            //        try
            //        {
            //            dd.SaveChanges();
            //        }
            //        catch (Exception ex)
            //        {
            //            string err = ex.Message;
            //        }
            //    }
            //}
        }

        private void button28_Click(object sender, EventArgs e)
        {
            //string FileName = @"C:\CSSP Latest Code Old\DataTool\ImportByFunction\Data_inputs\Final NB Station Master List In WGS 84.xlsx";

            //string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=Excel 12.0";

            //OleDbConnection conn = new OleDbConnection(connectionString);

            //conn.Open();
            //OleDbDataReader reader;
            //OleDbCommand comm = new OleDbCommand("Select * from [Sheet1$];");

            //comm.Connection = conn;
            //reader = comm.ExecuteReader();

            //int CountRead = 0;
            //while (reader.Read())
            //{
            //    CountRead += 1;
            //    if (CountRead < 0)
            //        continue;

            //    Application.DoEvents();

            //    string Subsector = "";
            //    string STAT_NBR = "";
            //    float Lat = 0.0f;
            //    float Lng = 0.0f;

            //    // Subsector
            //    if (reader.GetValue(0).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(0).ToString()))
            //    {
            //        Subsector = "";
            //    }
            //    else
            //    {
            //        Subsector = reader.GetValue(0).ToString().Trim();
            //    }

            //    // STAT_NBR
            //    if (reader.GetValue(1).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(1).ToString()))
            //    {
            //        STAT_NBR = "";
            //    }
            //    else
            //    {
            //        STAT_NBR = reader.GetValue(1).ToString().Trim();
            //        if (STAT_NBR.Length < 4)
            //        {
            //            STAT_NBR = "0000".Substring(0, 4 - STAT_NBR.Length) + STAT_NBR;
            //        }
            //        else if (STAT_NBR.Length > 4)
            //        {
            //            STAT_NBR = STAT_NBR + " ------- Too long";
            //        }
            //        else
            //        {
            //            // nothing
            //        }
            //    }

            //    // lat_wgs
            //    if (reader.GetValue(2).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(2).ToString()))
            //    {
            //        Lat = 0.0f;
            //    }
            //    else
            //    {
            //        Lat = float.Parse(reader.GetValue(2).ToString());
            //    }

            //    // long_wgs
            //    if (reader.GetValue(3).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(3).ToString()))
            //    {
            //        Lng = 0.0f;
            //    }
            //    else
            //    {
            //        Lng = float.Parse(reader.GetValue(3).ToString());
            //    }

            //    richTextBoxStatus.AppendText(CountRead + "\t");
            //    richTextBoxStatus.AppendText(Subsector + "\t");
            //    richTextBoxStatus.AppendText(STAT_NBR + "\t");
            //    richTextBoxStatus.AppendText(Lat + "\t");
            //    richTextBoxStatus.AppendText(Lng + "\r\n");

            //    if (string.IsNullOrWhiteSpace(STAT_NBR))
            //    {
            //        richTextBoxStatus.AppendText("STAT_NBR is empty at line " + CountRead.ToString());
            //        break;
            //    }

            //    if (Lat == 0.0f)
            //    {
            //        richTextBoxStatus.AppendText("Subsector is empty at line " + CountRead.ToString());
            //        break;
            //    }

            //    if (Lng == 0.0f)
            //    {
            //        richTextBoxStatus.AppendText("Lng is empty at line " + CountRead.ToString());
            //        break;
            //    }
            //    if (string.IsNullOrWhiteSpace(Subsector))
            //    {
            //        richTextBoxStatus.AppendText("Subsector is empty at line " + CountRead.ToString());
            //        break;
            //    }

            //    using (CSSPWebToolsDBEntities dd = new CSSPWebToolsDBEntities())
            //    {
            //        TVItem tvItemSS = (from t in dd.TVItems
            //                           from c in dd.TVItemLanguages
            //                           where t.TVItemID == c.TVItemID
            //                           && c.TVText.StartsWith(Subsector)
            //                           && c.Language == (int)LanguageEnum.en
            //                           select t).FirstOrDefault();

            //        if (tvItemSS == null)
            //        {
            //            richTextBoxStatus.AppendText("Could not find subsector " + Subsector + " at line " + CountRead.ToString());
            //            continue;
            //        }

            //        TVItem tvItemSite = (from t in dd.TVItems
            //                             from tl in dd.TVItemLanguages
            //                             where t.TVItemID == tl.TVItemID
            //                             && tl.Language == (int)LanguageEnum.en
            //                             && t.TVType == (int)TVTypeEnum.MWQMSite
            //                             && tl.TVText == STAT_NBR
            //                             && t.ParentID == tvItemSS.TVItemID
            //                             select t).FirstOrDefault();

            //        if (tvItemSite == null)
            //        {
            //            richTextBoxStatus.AppendText("Could not find site with STAT_NBR and Subsector " + STAT_NBR + " -- " + Subsector + " at line " + CountRead.ToString());
            //            continue;
            //        }

            //        MapInfoPoint mapInfoPoint = (from m in dd.MapInfos
            //                                     from mp in dd.MapInfoPoints
            //                                     where m.MapInfoID == mp.MapInfoID
            //                                     && m.TVItemID == tvItemSite.TVItemID
            //                                     && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                                     && m.TVType == (int)TVTypeEnum.MWQMSite
            //                                     select mp).FirstOrDefault();

            //        if (mapInfoPoint != null)
            //        {
            //            mapInfoPoint.Lat = Lat;
            //            mapInfoPoint.Lng = Lng;

            //            try
            //            {
            //                dd.SaveChanges();
            //                richTextBoxStatus.AppendText("Saved " + STAT_NBR + " -- " + Subsector + "\r\n");
            //            }
            //            catch (Exception ex)
            //            {
            //                richTextBoxStatus.AppendText("Error while saving MapInfoPoint " + ex.Message + (ex.InnerException == null ? "" : " Inner: " + ex.InnerException.Message) + " at line " + CountRead.ToString());
            //                continue;
            //            }
            //        }
            //        else
            //        {
            //            richTextBoxStatus.AppendText("Could not find MapInfoPoint for " + STAT_NBR + " --- " + Subsector);
            //            continue;
            //        }
            //    }
            //}
        }

        private void button29_Click(object sender, EventArgs e)
        {
            //StringBuilder sb = new StringBuilder();
            //List<MonitorSiteKML> MonitorSiteKMLList = new List<MonitorSiteKML>();

            //string FileName = @"C:\CSSP Latest Code Old\DataTool\ImportByFunction\Data_inputs\Final NB Station Master List In WGS 84.xlsx";

            //string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=Excel 12.0";

            //OleDbConnection conn = new OleDbConnection(connectionString);

            //conn.Open();
            //OleDbDataReader reader;
            //OleDbCommand comm = new OleDbCommand("Select * from [Sheet1$];");

            //comm.Connection = conn;
            //reader = comm.ExecuteReader();

            //int CountRead = 0;
            //while (reader.Read())
            //{
            //    CountRead += 1;
            //    if (CountRead < 0)
            //        continue;

            //    Application.DoEvents();

            //    string Subsector = "";
            //    string STAT_NBR = "";
            //    float Lat = 0.0f;
            //    float Lng = 0.0f;
            //    string Monitor = "";

            //    // Subsector
            //    if (reader.GetValue(0).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(0).ToString()))
            //    {
            //        Subsector = "";
            //    }
            //    else
            //    {
            //        Subsector = reader.GetValue(0).ToString().Trim();
            //    }

            //    // STAT_NBR
            //    if (reader.GetValue(1).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(1).ToString()))
            //    {
            //        STAT_NBR = "";
            //    }
            //    else
            //    {
            //        STAT_NBR = reader.GetValue(1).ToString().Trim();
            //        if (STAT_NBR.Length < 4)
            //        {
            //            STAT_NBR = "0000".Substring(0, 4 - STAT_NBR.Length) + STAT_NBR;
            //        }
            //        else if (STAT_NBR.Length > 4)
            //        {
            //            STAT_NBR = STAT_NBR + " ------- Too long";
            //        }
            //        else
            //        {
            //            // nothing
            //        }

            //        while (true)
            //        {
            //            if (STAT_NBR.First().ToString() == "0")
            //            {
            //                STAT_NBR = STAT_NBR.Substring(1);
            //            }
            //            else
            //            {
            //                break;
            //            }
            //        }
            //    }

            //    // lat_wgs
            //    if (reader.GetValue(2).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(2).ToString()))
            //    {
            //        Lat = 0.0f;
            //    }
            //    else
            //    {
            //        Lat = float.Parse(reader.GetValue(2).ToString());
            //    }

            //    // long_wgs
            //    if (reader.GetValue(3).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(3).ToString()))
            //    {
            //        Lng = 0.0f;
            //    }
            //    else
            //    {
            //        Lng = float.Parse(reader.GetValue(3).ToString());
            //    }

            //    // Monitor
            //    if (reader.GetValue(4).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(4).ToString()))
            //    {
            //        Monitor = "";
            //    }
            //    else
            //    {
            //        Monitor = reader.GetValue(4).ToString().Trim();
            //    }

            //    //richTextBoxStatus.AppendText(CountRead + "\t");
            //    //richTextBoxStatus.AppendText(Subsector + "\t");
            //    //richTextBoxStatus.AppendText(STAT_NBR + "\t");
            //    //richTextBoxStatus.AppendText(Lat + "\t");
            //    //richTextBoxStatus.AppendText(Lng + "\t");
            //    //richTextBoxStatus.AppendText(Monitor + "\r\n");

            //    if (string.IsNullOrWhiteSpace(STAT_NBR))
            //    {
            //        richTextBoxStatus.AppendText("STAT_NBR is empty at line " + CountRead.ToString());
            //        break;
            //    }

            //    if (Lat == 0.0f)
            //    {
            //        richTextBoxStatus.AppendText("Subsector is empty at line " + CountRead.ToString());
            //        break;
            //    }

            //    if (Lng == 0.0f)
            //    {
            //        richTextBoxStatus.AppendText("Lng is empty at line " + CountRead.ToString());
            //        break;
            //    }
            //    if (string.IsNullOrWhiteSpace(Subsector))
            //    {
            //        richTextBoxStatus.AppendText("Subsector is empty at line " + CountRead.ToString());
            //        break;
            //    }
            //    if (!string.IsNullOrWhiteSpace(Monitor))
            //    {
            //        MonitorSiteKML monitorSiteKML = new MonitorSiteKML()
            //        {
            //            Subsector = Subsector,
            //            MWQMSite = STAT_NBR,
            //            Lat = Lat,
            //            Lng = Lng
            //        };

            //        MonitorSiteKMLList.Add(monitorSiteKML);
            //    }
            //}

            //conn.Close();

            //List<string> SSList = (from c in MonitorSiteKMLList
            //                       select c.Subsector).Distinct().ToList();

            //sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            //sb.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            //sb.AppendLine(@"<Document>");
            //sb.AppendLine(@"	<name>NB Monitoring Sites</name>");
            //sb.AppendLine(@"	<StyleMap id=""m_ylw-pushpin0"">");
            //sb.AppendLine(@"		<Pair>");
            //sb.AppendLine(@"			<key>normal</key>");
            //sb.AppendLine(@"			<styleUrl>#s_ylw-pushpin</styleUrl>");
            //sb.AppendLine(@"		</Pair>");
            //sb.AppendLine(@"		<Pair>");
            //sb.AppendLine(@"			<key>highlight</key>");
            //sb.AppendLine(@"			<styleUrl>#s_ylw-pushpin_hl0</styleUrl>");
            //sb.AppendLine(@"		</Pair>");
            //sb.AppendLine(@"	</StyleMap>");
            //sb.AppendLine(@"	<StyleMap id=""msn_placemark_circle"">");
            //sb.AppendLine(@"		<Pair>");
            //sb.AppendLine(@"			<key>normal</key>");
            //sb.AppendLine(@"			<styleUrl>#sn_placemark_circle</styleUrl>");
            //sb.AppendLine(@"		</Pair>");
            //sb.AppendLine(@"		<Pair>");
            //sb.AppendLine(@"			<key>highlight</key>");
            //sb.AppendLine(@"			<styleUrl>#sh_placemark_circle_highlight</styleUrl>");
            //sb.AppendLine(@"		</Pair>");
            //sb.AppendLine(@"	</StyleMap>");
            //sb.AppendLine(@"	<Style id=""sn_placemark_circle"">");
            //sb.AppendLine(@"		<IconStyle>");
            //sb.AppendLine(@"			<color>ff00ffff</color>");
            //sb.AppendLine(@"			<scale>1.2</scale>");
            //sb.AppendLine(@"			<Icon>");
            //sb.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_circle.png</href>");
            //sb.AppendLine(@"			</Icon>");
            //sb.AppendLine(@"		</IconStyle>");
            //sb.AppendLine(@"		<LabelStyle>");
            //sb.AppendLine(@"			<color>ff00ffff</color>");
            //sb.AppendLine(@"		</LabelStyle>");
            //sb.AppendLine(@"		<ListStyle>");
            //sb.AppendLine(@"		</ListStyle>");
            //sb.AppendLine(@"		<LineStyle>");
            //sb.AppendLine(@"			<color>ff00ff00</color>");
            //sb.AppendLine(@"			<width>2</width>");
            //sb.AppendLine(@"		</LineStyle>");
            //sb.AppendLine(@"		<PolyStyle>");
            //sb.AppendLine(@"			<fill>0</fill>");
            //sb.AppendLine(@"		</PolyStyle>");
            //sb.AppendLine(@"	</Style>");
            //sb.AppendLine(@"	<Style id=""sh_placemark_circle_highlight"">");
            //sb.AppendLine(@"		<IconStyle>");
            //sb.AppendLine(@"			<color>ff00ffff</color>");
            //sb.AppendLine(@"			<scale>1.2</scale>");
            //sb.AppendLine(@"			<Icon>");
            //sb.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_circle_highlight.png</href>");
            //sb.AppendLine(@"			</Icon>");
            //sb.AppendLine(@"		</IconStyle>");
            //sb.AppendLine(@"		<LabelStyle>");
            //sb.AppendLine(@"			<color>ff00ffff</color>");
            //sb.AppendLine(@"		</LabelStyle>");
            //sb.AppendLine(@"		<ListStyle>");
            //sb.AppendLine(@"		</ListStyle>");
            //sb.AppendLine(@"		<LineStyle>");
            //sb.AppendLine(@"			<color>ff00ff00</color>");
            //sb.AppendLine(@"			<width>2</width>");
            //sb.AppendLine(@"		</LineStyle>");
            //sb.AppendLine(@"		<PolyStyle>");
            //sb.AppendLine(@"			<fill>0</fill>");
            //sb.AppendLine(@"		</PolyStyle>");
            //sb.AppendLine(@"	</Style>");
            //sb.AppendLine(@"	<Style id=""s_ylw-pushpin_hl0"">");
            //sb.AppendLine(@"		<IconStyle>");
            //sb.AppendLine(@"			<color>ff00ff00</color>");
            //sb.AppendLine(@"			<scale>1.3</scale>");
            //sb.AppendLine(@"			<Icon>");
            //sb.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            //sb.AppendLine(@"			</Icon>");
            //sb.AppendLine(@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            //sb.AppendLine(@"		</IconStyle>");
            //sb.AppendLine(@"		<LabelStyle>");
            //sb.AppendLine(@"			<color>ff00ff00</color>");
            //sb.AppendLine(@"		</LabelStyle>");
            //sb.AppendLine(@"		<LineStyle>");
            //sb.AppendLine(@"			<color>ff00ff00</color>");
            //sb.AppendLine(@"			<width>2</width>");
            //sb.AppendLine(@"		</LineStyle>");
            //sb.AppendLine(@"		<PolyStyle>");
            //sb.AppendLine(@"			<fill>0</fill>");
            //sb.AppendLine(@"		</PolyStyle>");
            //sb.AppendLine(@"	</Style>");
            //sb.AppendLine(@"	<Style id=""s_ylw-pushpin"">");
            //sb.AppendLine(@"		<IconStyle>");
            //sb.AppendLine(@"			<color>ff00ff00</color>");
            //sb.AppendLine(@"			<scale>1.1</scale>");
            //sb.AppendLine(@"			<Icon>");
            //sb.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            //sb.AppendLine(@"			</Icon>");
            //sb.AppendLine(@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            //sb.AppendLine(@"		</IconStyle>");
            //sb.AppendLine(@"		<LabelStyle>");
            //sb.AppendLine(@"			<color>ff00ff00</color>");
            //sb.AppendLine(@"		</LabelStyle>");
            //sb.AppendLine(@"		<LineStyle>");
            //sb.AppendLine(@"			<color>ff00ff00</color>");
            //sb.AppendLine(@"			<width>2</width>");
            //sb.AppendLine(@"		</LineStyle>");
            //sb.AppendLine(@"		<PolyStyle>");
            //sb.AppendLine(@"			<fill>0</fill>");
            //sb.AppendLine(@"		</PolyStyle>");
            //sb.AppendLine(@" </Style>");


            //foreach (string ss in SSList.OrderBy(c => c))
            //{
            //    lblStatus.Text = ss;
            //    lblStatus.Refresh();
            //    Application.DoEvents();


            //    using (CSSPWebToolsDBEntities dd = new CSSPWebToolsDBEntities())
            //    {
            //        var tvItemSS = (from t in dd.TVItems
            //                        from c in dd.TVItemLanguages
            //                        where t.TVItemID == c.TVItemID
            //                        && c.TVText.StartsWith(ss)
            //                        && c.Language == (int)LanguageEnum.en
            //                        select new { t, c }).FirstOrDefault();

            //        if (tvItemSS == null)
            //        {
            //            richTextBoxStatus.AppendText("Could not find subsector " + ss + " at line " + CountRead.ToString());
            //            continue;
            //        }

            //        MapInfoPoint mapInfoPointSS = (from m in dd.MapInfos
            //                                       from mp in dd.MapInfoPoints
            //                                       where m.MapInfoID == mp.MapInfoID
            //                                       && m.TVItemID == tvItemSS.t.TVItemID
            //                                       && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                                       && m.TVType == (int)TVTypeEnum.Subsector
            //                                       select mp).FirstOrDefault();


            //        sb.AppendLine(@"    <Folder>");
            //        sb.AppendLine(@"    <name>" + tvItemSS.c.TVText + "</name>");
            //        sb.AppendLine(@"    <open>1</open>");

            //        sb.AppendLine(@"    <Placemark>");
            //        sb.AppendLine(@"        <name>" + tvItemSS.c.TVText + "</name>");
            //        sb.AppendLine(@"        <styleUrl>s_ylw-pushpin_hl0</styleUrl>");
            //        sb.AppendLine(@"        <Point>");
            //        sb.AppendLine(@"        <coordinates>" + mapInfoPointSS.Lng.ToString("F5") + ", " + mapInfoPointSS.Lat.ToString("F5") + ", 0 </coordinates>");
            //        sb.AppendLine(@"        </Point>");
            //        sb.AppendLine(@"    </Placemark>");

            //        List<MapInfoPoint> mapInfoPointList = (from m in dd.MapInfos
            //                                               from mp in dd.MapInfoPoints
            //                                               where m.MapInfoID == mp.MapInfoID
            //                                               && m.TVItemID == tvItemSS.t.TVItemID
            //                                               && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon
            //                                               && m.TVType == (int)TVTypeEnum.Subsector
            //                                               select mp).ToList();


            //        if (mapInfoPointList.Count == 0)
            //        {
            //            richTextBoxStatus.AppendText("Could not find MapInfoPoint for " + ss);
            //            continue;
            //        }

            //        sb.AppendLine(@"	<Placemark>");
            //        sb.AppendLine(@"		<name>Polygon</name>");
            //        sb.AppendLine(@"		<styleUrl>s_ylw-pushpin_hl0</styleUrl>");
            //        sb.AppendLine(@"		<Polygon>");
            //        sb.AppendLine(@"			<tessellate>1</tessellate>");
            //        sb.AppendLine(@"			<outerBoundaryIs>");
            //        sb.AppendLine(@"				<LinearRing>");
            //        sb.AppendLine(@"					<coordinates>");
            //        sb.AppendLine(@"						");
            //        foreach (MapInfoPoint mapInfoPoint in mapInfoPointList)
            //        {
            //            sb.AppendLine(@"" + mapInfoPoint.Lng + "," + mapInfoPoint.Lat + ",0 ");
            //        }
            //        sb.AppendLine(@"");
            //        sb.AppendLine(@"					</coordinates>");
            //        sb.AppendLine(@"				</LinearRing>");
            //        sb.AppendLine(@"			</outerBoundaryIs>");
            //        sb.AppendLine(@"		</Polygon>");
            //        sb.AppendLine(@" </Placemark>");


            //    }

            //    foreach (MonitorSiteKML monitorSiteKML in (from c in MonitorSiteKMLList
            //                                               where c.Subsector == ss
            //                                               orderby c.MWQMSite
            //                                               select c).ToList())
            //    {
            //        sb.AppendLine(@"    <Placemark>");
            //        sb.AppendLine(@"        <name>" + monitorSiteKML.MWQMSite + "</name>");
            //        sb.AppendLine(@"        <styleUrl>sn_placemark_circle</styleUrl>");
            //        sb.AppendLine(@"        <Point>");
            //        sb.AppendLine(@"        <coordinates>" + monitorSiteKML.Lng.ToString("F5") + ", " + monitorSiteKML.Lat.ToString("F5") + ", 0 </coordinates>");
            //        sb.AppendLine(@"        </Point>");
            //        sb.AppendLine(@"    </Placemark>");
            //    }

            //    sb.AppendLine(@"    </Folder>");
            //}


            //sb.AppendLine(@"</Document>");
            //sb.AppendLine(@"</kml>");

            //FileInfo fi = new FileInfo(@"C:\Users\leblancc\Desktop\NBSouthWestMonitoring.kml");
            //StreamWriter sw = fi.CreateText();
            //sw.Write(sb.ToString());
            //sw.Close();

            //lblStatus.Text = "done ...";
        }

        private void button30_Click(object sender, EventArgs e)
        {
            //StringBuilder sbKML = new StringBuilder();
            //StringBuilder sbSubsectorSites = new StringBuilder();
            //StringBuilder sbSubsectorSiteData = new StringBuilder();

            //DirectoryInfo di = new DirectoryInfo(@"C:\Users\leblancc\Desktop\NBData\");
            //if (!di.Exists)
            //{
            //    try
            //    {
            //        di.Create();
            //    }
            //    catch (Exception ex)
            //    {
            //        int sef = 34;
            //    }
            //}

            //List<string> SSList = new List<string>();
            //using (CSSPWebToolsDBEntities dd = new CSSPWebToolsDBEntities())
            //{
            //    SSList = (from c in dd.TVItems
            //              from cl in dd.TVItemLanguages
            //              where c.TVItemID == cl.TVItemID
            //              && c.TVType == (int)TVTypeEnum.Subsector
            //              && c.TVPath.StartsWith("p1p5p7p")
            //              select cl.TVText).Distinct().ToList();
            //}

            //sbKML.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            //sbKML.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            //sbKML.AppendLine(@"<Document>");
            //sbKML.AppendLine(@"	<name>NB Monitoring Sites</name>");
            //sbKML.AppendLine(@"	<StyleMap id=""m_ylw-pushpin0"">");
            //sbKML.AppendLine(@"		<Pair>");
            //sbKML.AppendLine(@"			<key>normal</key>");
            //sbKML.AppendLine(@"			<styleUrl>#s_ylw-pushpin</styleUrl>");
            //sbKML.AppendLine(@"		</Pair>");
            //sbKML.AppendLine(@"		<Pair>");
            //sbKML.AppendLine(@"			<key>highlight</key>");
            //sbKML.AppendLine(@"			<styleUrl>#s_ylw-pushpin_hl0</styleUrl>");
            //sbKML.AppendLine(@"		</Pair>");
            //sbKML.AppendLine(@"	</StyleMap>");
            //sbKML.AppendLine(@"	<StyleMap id=""msn_placemark_circle"">");
            //sbKML.AppendLine(@"		<Pair>");
            //sbKML.AppendLine(@"			<key>normal</key>");
            //sbKML.AppendLine(@"			<styleUrl>#sn_placemark_circle</styleUrl>");
            //sbKML.AppendLine(@"		</Pair>");
            //sbKML.AppendLine(@"		<Pair>");
            //sbKML.AppendLine(@"			<key>highlight</key>");
            //sbKML.AppendLine(@"			<styleUrl>#sh_placemark_circle_highlight</styleUrl>");
            //sbKML.AppendLine(@"		</Pair>");
            //sbKML.AppendLine(@"	</StyleMap>");
            //sbKML.AppendLine(@"	<Style id=""sn_placemark_circle"">");
            //sbKML.AppendLine(@"		<IconStyle>");
            //sbKML.AppendLine(@"			<color>ff00ffff</color>");
            //sbKML.AppendLine(@"			<scale>1.2</scale>");
            //sbKML.AppendLine(@"			<Icon>");
            //sbKML.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_circle.png</href>");
            //sbKML.AppendLine(@"			</Icon>");
            //sbKML.AppendLine(@"		</IconStyle>");
            //sbKML.AppendLine(@"		<LabelStyle>");
            //sbKML.AppendLine(@"			<color>ff00ffff</color>");
            //sbKML.AppendLine(@"		</LabelStyle>");
            //sbKML.AppendLine(@"		<ListStyle>");
            //sbKML.AppendLine(@"		</ListStyle>");
            //sbKML.AppendLine(@"		<LineStyle>");
            //sbKML.AppendLine(@"			<color>ff00ff00</color>");
            //sbKML.AppendLine(@"			<width>2</width>");
            //sbKML.AppendLine(@"		</LineStyle>");
            //sbKML.AppendLine(@"		<PolyStyle>");
            //sbKML.AppendLine(@"			<fill>0</fill>");
            //sbKML.AppendLine(@"		</PolyStyle>");
            //sbKML.AppendLine(@"	</Style>");
            //sbKML.AppendLine(@"	<Style id=""sh_placemark_circle_highlight"">");
            //sbKML.AppendLine(@"		<IconStyle>");
            //sbKML.AppendLine(@"			<color>ff00ffff</color>");
            //sbKML.AppendLine(@"			<scale>1.2</scale>");
            //sbKML.AppendLine(@"			<Icon>");
            //sbKML.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_circle_highlight.png</href>");
            //sbKML.AppendLine(@"			</Icon>");
            //sbKML.AppendLine(@"		</IconStyle>");
            //sbKML.AppendLine(@"		<LabelStyle>");
            //sbKML.AppendLine(@"			<color>ff00ffff</color>");
            //sbKML.AppendLine(@"		</LabelStyle>");
            //sbKML.AppendLine(@"		<ListStyle>");
            //sbKML.AppendLine(@"		</ListStyle>");
            //sbKML.AppendLine(@"		<LineStyle>");
            //sbKML.AppendLine(@"			<color>ff00ff00</color>");
            //sbKML.AppendLine(@"			<width>2</width>");
            //sbKML.AppendLine(@"		</LineStyle>");
            //sbKML.AppendLine(@"		<PolyStyle>");
            //sbKML.AppendLine(@"			<fill>0</fill>");
            //sbKML.AppendLine(@"		</PolyStyle>");
            //sbKML.AppendLine(@"	</Style>");
            //sbKML.AppendLine(@"	<Style id=""s_ylw-pushpin_hl0"">");
            //sbKML.AppendLine(@"		<IconStyle>");
            //sbKML.AppendLine(@"			<color>ff00ff00</color>");
            //sbKML.AppendLine(@"			<scale>1.3</scale>");
            //sbKML.AppendLine(@"			<Icon>");
            //sbKML.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            //sbKML.AppendLine(@"			</Icon>");
            //sbKML.AppendLine(@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            //sbKML.AppendLine(@"		</IconStyle>");
            //sbKML.AppendLine(@"		<LabelStyle>");
            //sbKML.AppendLine(@"			<color>ff00ff00</color>");
            //sbKML.AppendLine(@"		</LabelStyle>");
            //sbKML.AppendLine(@"		<LineStyle>");
            //sbKML.AppendLine(@"			<color>ff00ff00</color>");
            //sbKML.AppendLine(@"			<width>2</width>");
            //sbKML.AppendLine(@"		</LineStyle>");
            //sbKML.AppendLine(@"		<PolyStyle>");
            //sbKML.AppendLine(@"			<fill>0</fill>");
            //sbKML.AppendLine(@"		</PolyStyle>");
            //sbKML.AppendLine(@"	</Style>");
            //sbKML.AppendLine(@"	<Style id=""s_ylw-pushpin"">");
            //sbKML.AppendLine(@"		<IconStyle>");
            //sbKML.AppendLine(@"			<color>ff00ff00</color>");
            //sbKML.AppendLine(@"			<scale>1.1</scale>");
            //sbKML.AppendLine(@"			<Icon>");
            //sbKML.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            //sbKML.AppendLine(@"			</Icon>");
            //sbKML.AppendLine(@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            //sbKML.AppendLine(@"		</IconStyle>");
            //sbKML.AppendLine(@"		<LabelStyle>");
            //sbKML.AppendLine(@"			<color>ff00ff00</color>");
            //sbKML.AppendLine(@"		</LabelStyle>");
            //sbKML.AppendLine(@"		<LineStyle>");
            //sbKML.AppendLine(@"			<color>ff00ff00</color>");
            //sbKML.AppendLine(@"			<width>2</width>");
            //sbKML.AppendLine(@"		</LineStyle>");
            //sbKML.AppendLine(@"		<PolyStyle>");
            //sbKML.AppendLine(@"			<fill>0</fill>");
            //sbKML.AppendLine(@"		</PolyStyle>");
            //sbKML.AppendLine(@" </Style>");

            //sbSubsectorSites.AppendLine("Subsector,Site,Latitude,Longitude");
            //sbSubsectorSiteData.AppendLine("Subsector,Site,Date of Sample,MPN,Sal,Temp");

            //int countSSOK = 0;
            //foreach (string ss in SSList.OrderBy(c => c))
            //{
            //    string ssShort = ss.Substring(0, ss.IndexOf(" "));

            //    lblStatus.Text = ss;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    using (CSSPWebToolsDBEntities dd = new CSSPWebToolsDBEntities())
            //    {
            //        if (countSSOK > 1)
            //        {
            //            //continue;
            //        }

            //        var tvItemSS = (from t in dd.TVItems
            //                        from c in dd.TVItemLanguages
            //                        where t.TVItemID == c.TVItemID
            //                        && c.TVText == ss
            //                        && c.Language == (int)LanguageEnum.en
            //                        select new { t, c }).FirstOrDefault();

            //        if (tvItemSS == null)
            //        {
            //            richTextBoxStatus.AppendText("Could not find subsector " + ss);
            //            return;
            //        }

            //        var SubsectorSiteList = (from c in dd.TVItems
            //                                 from cl in dd.TVItemLanguages
            //                                 from s in dd.MWQMSamples
            //                                 where c.TVItemID == cl.TVItemID
            //                                 && c.TVItemID == s.MWQMSiteTVItemID
            //                                 && c.TVPath.StartsWith(tvItemSS.t.TVPath + "p")
            //                                 && c.TVType == (int)TVTypeEnum.MWQMSite
            //                                 && s.SampleDateTime_Local.Year > 1999
            //                                 && s.SampleDateTime_Local.Year < 2016
            //                                 && cl.Language == (int)LanguageEnum.en
            //                                 select new { c, cl }).Distinct().ToList();

            //        if (SubsectorSiteList.Count == 0)
            //        {
            //            continue;
            //        }

            //        countSSOK += 1;

            //        List<MapInfoPoint> mapInfoPointSSList = (from m in dd.MapInfos
            //                                                 from mp in dd.MapInfoPoints
            //                                                 where m.MapInfoID == mp.MapInfoID
            //                                                 && m.TVItemID == tvItemSS.t.TVItemID
            //                                                 && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                                                 && m.TVType == (int)TVTypeEnum.Subsector
            //                                                 select mp).ToList();


            //        sbKML.AppendLine(@"    <Folder>");
            //        sbKML.AppendLine(@"    <name>" + tvItemSS.c.TVText + "</name>");
            //        sbKML.AppendLine(@"    <open>1</open>");

            //        sbKML.AppendLine(@"    <Placemark>");
            //        sbKML.AppendLine(@"        <name>" + tvItemSS.c.TVText + "</name>");
            //        sbKML.AppendLine(@"        <styleUrl>s_ylw-pushpin_hl0</styleUrl>");
            //        sbKML.AppendLine(@"        <Point>");
            //        sbKML.AppendLine(@"        <coordinates>" + mapInfoPointSSList[0].Lng.ToString("F5") + ", " + mapInfoPointSSList[0].Lat.ToString("F5") + ", 0 </coordinates>");
            //        sbKML.AppendLine(@"        </Point>");
            //        sbKML.AppendLine(@"    </Placemark>");

            //        List<MapInfoPoint> mapInfoPointList = (from m in dd.MapInfos
            //                                               from mp in dd.MapInfoPoints
            //                                               where m.MapInfoID == mp.MapInfoID
            //                                               && m.TVItemID == tvItemSS.t.TVItemID
            //                                               && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon
            //                                               && m.TVType == (int)TVTypeEnum.Subsector
            //                                               select mp).ToList();


            //        if (mapInfoPointList.Count == 0)
            //        {
            //            richTextBoxStatus.AppendText("Could not find MapInfoPoint for " + ss);
            //            continue;
            //        }

            //        sbKML.AppendLine(@"	<Placemark>");
            //        sbKML.AppendLine(@"		<name>Polygon</name>");
            //        sbKML.AppendLine(@"		<styleUrl>s_ylw-pushpin_hl0</styleUrl>");
            //        sbKML.AppendLine(@"		<Polygon>");
            //        sbKML.AppendLine(@"			<tessellate>1</tessellate>");
            //        sbKML.AppendLine(@"			<outerBoundaryIs>");
            //        sbKML.AppendLine(@"				<LinearRing>");
            //        sbKML.AppendLine(@"					<coordinates>");
            //        sbKML.AppendLine(@"						");
            //        foreach (MapInfoPoint mapInfoPoint in mapInfoPointList)
            //        {
            //            sbKML.AppendLine(@"" + mapInfoPoint.Lng + "," + mapInfoPoint.Lat + ",0 ");
            //        }
            //        sbKML.AppendLine(@"");
            //        sbKML.AppendLine(@"					</coordinates>");
            //        sbKML.AppendLine(@"				</LinearRing>");
            //        sbKML.AppendLine(@"			</outerBoundaryIs>");
            //        sbKML.AppendLine(@"		</Polygon>");
            //        sbKML.AppendLine(@" </Placemark>");

            //        foreach (var SSData in (from c in SubsectorSiteList
            //                                orderby c.cl.TVText
            //                                select c).ToList())
            //        {
            //            lblStatus.Text = ss + " --- " + SSData.cl.TVText;
            //            lblStatus.Refresh();
            //            Application.DoEvents();

            //            List<MapInfoPoint> mapInfoPointSiteList = (from m in dd.MapInfos
            //                                                       from mp in dd.MapInfoPoints
            //                                                       where m.MapInfoID == mp.MapInfoID
            //                                                       && m.TVItemID == SSData.c.TVItemID
            //                                                       && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                                                       && m.TVType == (int)TVTypeEnum.MWQMSite
            //                                                       select mp).ToList();


            //            if (mapInfoPointSiteList.Count == 0)
            //            {
            //                richTextBoxStatus.AppendText("Could not find MapInfoPoint for " + ss);
            //                continue;
            //            }

            //            sbKML.AppendLine(@"    <Placemark>");
            //            sbKML.AppendLine(@"        <name>" + SSData.cl.TVText + "</name>");
            //            sbKML.AppendLine(@"        <styleUrl>sn_placemark_circle</styleUrl>");
            //            sbKML.AppendLine(@"        <Point>");
            //            sbKML.AppendLine(@"        <coordinates>" + mapInfoPointSiteList[0].Lng.ToString("F5") + ", " + mapInfoPointSiteList[0].Lat.ToString("F5") + ", 0 </coordinates>");
            //            sbKML.AppendLine(@"        </Point>");
            //            sbKML.AppendLine(@"    </Placemark>");

            //            sbSubsectorSites.AppendLine(ssShort + "," + SSData.cl.TVText + "," + mapInfoPointSiteList[0].Lat.ToString("F5") + ", " + mapInfoPointSiteList[0].Lng.ToString("F5"));

            //            List<MWQMSample> mwqmSampleList = (from c in dd.MWQMSamples
            //                                               where c.MWQMSiteTVItemID == SSData.c.TVItemID
            //                                               && c.SampleDateTime_Local.Year > 1999
            //                                               && c.SampleDateTime_Local.Year < 2016
            //                                               orderby c.SampleDateTime_Local
            //                                               select c).ToList();

            //            foreach (MWQMSample mwqmSample in mwqmSampleList)
            //            {
            //                lblStatus.Text = ss + " --- " + SSData.cl.TVText + " --- " + mwqmSample.FecCol_MPN_100ml;
            //                lblStatus.Refresh();
            //                Application.DoEvents();

            //                sbSubsectorSiteData.AppendLine(ssShort + "," + SSData.cl.TVText + "," + mwqmSample.SampleDateTime_Local.ToString("yyyy-MM-dd HH:mm:ss") + "," + mwqmSample.FecCol_MPN_100ml + "," + mwqmSample.Salinity_PPT + "," + mwqmSample.WaterTemp_C);
            //            }

            //        }

            //        sbKML.AppendLine(@"    </Folder>");

            //    }

            //}


            //sbKML.AppendLine(@"</Document>");
            //sbKML.AppendLine(@"</kml>");



            //FileInfo fiSiteData = new FileInfo(@"C:\Users\leblancc\Desktop\NBData\CSSPNBMonitoringData.csv");
            //StreamWriter swSiteData = fiSiteData.CreateText();
            //swSiteData.Write(sbSubsectorSiteData.ToString());
            //swSiteData.Close();

            //FileInfo fiKML = new FileInfo(@"C:\Users\leblancc\Desktop\NBData\CSSPNBMonitoringSites.kml");
            //StreamWriter swKML = fiKML.CreateText();
            //swKML.Write(sbKML.ToString());
            //swKML.Close();

            //FileInfo fiSite = new FileInfo(@"C:\Users\leblancc\Desktop\NBData\CSSPNBMonitoringSites.csv");
            //StreamWriter swSite = fiSite.CreateText();
            //swSite.Write(sbSubsectorSites.ToString());
            //swSite.Close();

            //lblStatus.Text = "done ...";
        }

        private void button31_Click(object sender, EventArgs e)
        {
            //using (CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities())
            //{
            //    TVItem tvItemGaspe = (from c in db.TVItems
            //                     where c.TVItemID == 48790 // Gaspé
            //                     select c).FirstOrDefault();

            //    if (tvItemGaspe != null)
            //    {
            //        List<TVItem> tvItemScenarioList = (from c in db.TVItems
            //                                           where c.TVPath.StartsWith(tvItemGaspe.TVPath + "p")
            //                                           && c.TVType == (int)TVTypeEnum.MikeScenario
            //                                           select c).ToList();

            //        foreach (TVItem tvitem in tvItemScenarioList)
            //        {
            //            List<TVFile> tvFileList = (from c in db.TVFiles
            //                                       from t in db.TVItems
            //                                       where c.TVFileTVItemID == t.TVItemID
            //                                       && (c.ServerFileName.EndsWith(".m21fm")
            //                                       || c.ServerFileName.EndsWith(".m3fm"))
            //                                       && t.ParentID == tvitem.TVItemID
            //                                       select c).ToList();

            //            foreach (TVFile tvFile in tvFileList)
            //            {
            //                FileInfo fi = new FileInfo(tvFile.ServerFilePath + tvFile.ServerFileName);

            //                if (fi.Exists)
            //                {
            //                    StreamReader sr = fi.OpenText();
            //                    string fileText = sr.ReadToEnd();
            //                    sr.Close();

            //                    bool found = false;
            //                    if (fileText.Contains("datum_depth = 0.6"))
            //                    {
            //                        richTextBoxStatus.AppendText("Has 0.6 --- " + fi.FullName + "\r\n");
            //                        found = true;
            //                    }
            //                    if (fileText.Contains("datum_depth = 0.7"))
            //                    {
            //                        richTextBoxStatus.AppendText("Has 0.7 --- " + fi.FullName + "\r\n");
            //                        found = true;
            //                    }

            //                    if (found)
            //                    {
            //                        try
            //                        {
            //                            File.Copy(fi.FullName, fi.FullName.Replace(".m21fm", ".m21fma").Replace(".m3fm", ".m3fma"));
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            string err = ex.Message;
            //                        }

            //                        fileText = fileText.Replace("datum_depth = 0.6", "datum_depth = 0.8");
            //                        fileText = fileText.Replace("datum_depth = 0.7", "datum_depth = 0.8");

            //                        fi = new FileInfo(tvFile.ServerFilePath + tvFile.ServerFileName);

            //                        StreamWriter sw = fi.CreateText();
            //                        sw.Write(fileText);
            //                        sw.Close();

            //                        fi = new FileInfo(tvFile.ServerFilePath + tvFile.ServerFileName.Replace(".m21fm", ".m21fma").Replace(".m3fm", ".m3fma"));

            //                        try
            //                        {
            //                            fi.Delete();
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            string err = ex.Message;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private void button32_Click(object sender, EventArgs e)
        {
            richTextBoxStatus.Text = "Subsector,Site,Date,MPN";
            using (CSSPWebToolsDBEntities db = new CSSPWebToolsDBEntities())
            {
                lblStatus.Text = "Starting";
                lblStatus.Refresh();
                Application.DoEvents();

                var tvItemSubsectorList = (from c in db.TVItems
                                           from cl in db.TVItemLanguages
                                           where c.TVItemID == cl.TVItemID
                                           && c.TVType == (int)TVTypeEnum.Subsector
                                           && cl.Language == (int)LanguageEnum.en
                                           where cl.TVText.StartsWith("PE-")
                                           orderby cl.TVText
                                           select new { c, cl }).ToList();

                foreach (var tvItemSubsector in tvItemSubsectorList)
                {
                    lblStatus.Text = tvItemSubsector.cl.TVText;
                    lblStatus.Refresh();
                    Application.DoEvents();

                    var tvItemMWQMSiteList = (from c in db.TVItems
                                              from cl in db.TVItemLanguages
                                              where c.TVItemID == cl.TVItemID
                                              && c.ParentID == tvItemSubsector.c.TVItemID
                                              && c.TVType == (int)TVTypeEnum.MWQMSite
                                              && cl.Language == (int)LanguageEnum.en
                                              orderby cl.TVText
                                              select new { c, cl }).ToList();

                    foreach (var tvItemMWQMSite in tvItemMWQMSiteList)
                    {
                        lblStatus.Text = tvItemSubsector.cl.TVText + " --- " + tvItemMWQMSite.cl.TVText;
                        lblStatus.Refresh();
                        Application.DoEvents();

                        List<MWQMSample> mwqmSampleList = (from c in db.MWQMSamples
                                                           where c.MWQMSiteTVItemID == tvItemMWQMSite.c.TVItemID
                                                           && c.FecCol_MPN_100ml == 5
                                                           && c.SampleDateTime_Local.Year > 2015
                                                           orderby c.SampleDateTime_Local
                                                           select c).ToList();

                        foreach (MWQMSample mwqmSample in mwqmSampleList)
                        {
                            //mwqmSample.FecCol_MPN_100ml = 4;
                            richTextBoxStatus.AppendText(tvItemSubsector.cl.TVText + "," + tvItemMWQMSite.cl.TVText + "," + mwqmSample.SampleDateTime_Local + "," + mwqmSample.FecCol_MPN_100ml + "\r\n");

                            //try
                            //{
                            //    db.SaveChanges();
                            //}
                            //catch (Exception ex)
                            //{
                            //    richTextBoxStatus.AppendText("Error: " + ex.Message + "\r\n");
                            //}
                        }

                    }
                }

            }

        }

        //private void button18_Click(object sender, EventArgs e)
        //{
        //    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
        //    AppTaskService appTaskService = new AppTaskService(LanguageEnum.en, user);
        //    MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
        //    MapInfoPointService mapInfoPointService = new MapInfoPointService(LanguageEnum.en, user);

        //    TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
        //    if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
        //    {
        //        return;
        //    }

        //    List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Subsector).Where(c => c.TVText.StartsWith("NS-")).OrderBy(c => c.TVText).ToList();
        //    if (tvItemModelSubsectorList.Count == 0)
        //    {
        //        return;
        //    }

        //    foreach (TVItemModel tvItemModel in tvItemModelSubsectorList)
        //    {
        //        string TVText = tvItemModel.TVText;
        //        TVText = TVText.Substring(0, TVText.IndexOf(" ")).Trim();
        //        string Prov = TVText.Substring(0, 2);
        //        string Area = TVText.Substring(3, 2);
        //        string Sector = TVText.Substring(6, 3);
        //        string Subsector = TVText.Substring(10, 3);

        //        List<TempData.ASGADStation> asgadStationList = new List<ASGADStation>();
        //        using (TempDataToolDBEntities tdDB = new TempDataToolDBEntities())
        //        {
        //            asgadStationList = (from c in tdDB.ASGADStations
        //                                where c.PROV == Prov
        //                                && c.AREA == Area
        //                                && c.SECTOR == Sector
        //                                && c.SUBSECTOR == Subsector
        //                                orderby c.STAT_NBR
        //                                select c).ToList();
        //        }

        //        List<TVItemModel> tvItemModelSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModel.TVItemID, TVTypeEnum.MWQMSite).OrderBy(c => c.TVText).ToList();

        //        foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
        //        {
        //            lblStatus.Text = TVText + " ---- " + tvItemModelSite.TVText;
        //            lblStatus.Refresh();
        //            Application.DoEvents();

        //            TempData.ASGADStation asgadStation = asgadStationList.Where(c => c.STAT_NBR == tvItemModelSite.TVText).FirstOrDefault();
        //            if(asgadStation != null)
        //            {
        //                List<MapInfoPointModel> mapInfoPointModelList = mapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelSite.TVItemID, TVTypeEnum.MWQMSite, MapInfoDrawTypeEnum.Point);
        //                if (mapInfoPointModelList.Count > 0)
        //                {
        //                    if (mapInfoPointModelList[0].Lat != asgadStation.LAT || mapInfoPointModelList[0].Lng != asgadStation.LONG)
        //                    {
        //                        mapInfoPointModelList[0].Lat = (float)asgadStation.LAT;
        //                        mapInfoPointModelList[0].Lng = (float)asgadStation.LONG;
        //                        MapInfoPointModel mapInfoPointModelRet = mapInfoPointService.PostUpdateMapInfoPointDB(mapInfoPointModelList[0]);
        //                        if (!string.IsNullOrWhiteSpace(mapInfoPointModelRet.Error))
        //                        {
        //                            richTextBoxStatus.AppendText("Change to MapInfoPointModel Error [" + tvItemModelSite.TVText + "]\r\n");
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    richTextBoxStatus.AppendText("Adding Coordinal points of [" + tvItemModelSite.TVText + "]\r\n");
        //                    List<Coord> coordList = new List<Coord>()
        //                    {
        //                        new Coord() { Lat = (float)asgadStation.LAT, Lng = (float)asgadStation.LONG, Ordinal = 0 }
        //                    };
        //                    MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.MWQMSite, tvItemModelSite.TVItemID);
        //                    if (!string.IsNullOrWhiteSpace(mapInfoModelRet.Error))
        //                    {
        //                        richTextBoxStatus.AppendText("Error while adding coordinate points of [" + tvItemModelSite.TVText + "]\r\n");
        //                        richTextBoxStatus.AppendText("Error message [" + mapInfoModelRet.Error + "]\r\n");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                richTextBoxStatus.AppendText("Not found [" + tvItemModelSite.TVText + "]\r\n");
        //            }
        //        }
        //    }
        //}
    }

    public class MonitorSiteKML
    {
        public MonitorSiteKML()
        {

        }

        public string Subsector { get; set; }
        public string MWQMSite { get; set; }
        public float Lat { get; set; }
        public float Lng { get; set; }
    }
    public class TTT
    {
        public int InfrastructureID { get; set; }
        public string TVText { get; set; }
        public TreatmentTypeEnum TreatmentType { get; set; }
        public int TreatmentTypeTVItemID { get; set; }
    }

    public class YearMinMaxDate
    {
        public int Year { get; set; }
        public DateTime MinDateTime { get; set; }
        public DateTime MaxDateTime { get; set; }
    }
}
