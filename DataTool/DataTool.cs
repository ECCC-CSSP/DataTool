using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Diagnostics;
using System.Reflection;

namespace DataTool
{
    public partial class DataTool : Form
    {
        private ClimateHydrometricDBEntities cdbe = new ClimateHydrometricDBEntities();
        private bool DownloadCompleted = false;
        List<ClimateStation> climateStationList = new List<ClimateStation>();
        ClimateStation currentClimateStation = new ClimateStation();
        List<HydrometricStation> hydrometricStationList = new List<HydrometricStation>();
        HydrometricStation currentHydrometricStation = new HydrometricStation();

        #region Constructors
        public DataTool()
        {
            InitializeComponent();
            comboBoxStationClimate.DisplayMember = "ClimateStationName";
            comboBoxStationClimate.ValueMember = "ClimateStationID";
            comboBoxStationHydro.DisplayMember = "HydroStationName";
            comboBoxStationHydro.ValueMember = "HydrometricStationID";
            webBrowserDataTool.DocumentCompleted += webBrowserDataTool_DocumentCompleted;
        }
        #endregion

        #region Events
        private void butCreateClimateStationKML_Click(object sender, EventArgs e)
        {
            CreateClimateStationKML();
        }
        private void butCreateHydrometricStationKML_Click(object sender, EventArgs e)
        {
            CreateHydrometricStationKML();
        }
        private void butGetClimateData_Click(object sender, EventArgs e)
        {
            richTextBoxDataTool.Text = "";

            if (currentClimateStation == null)
            {
                lblStatus.Text = "Error: currentClimateStation could not be set";
                return;
            }

            int NumberOfDays = 1;

            for (int i = 0; i < NumberOfDays; i++)
            {
                DateTime ForDate = new DateTime(dateTimePickerStartDateClimate.Value.Year, dateTimePickerStartDateClimate.Value.Month, dateTimePickerStartDateClimate.Value.Day + i);
                DateTime CurrentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                TimeSpan ts = ForDate - CurrentDate;
                ClimateDataStartDate climateDataStartDate = null;
                if (ts.Days >= 0)
                {
                    climateDataStartDate = GetHourClimateForecasts(currentClimateStation.ClimateStationID, ForDate);
                    if (ts.Days == 0)
                    {
                        climateDataStartDate = GetHourClimateObservations(currentClimateStation.ClimateStationID, ForDate);
                    }
                }
                else
                {
                    if (ts.Days > -1)
                    {
                        climateDataStartDate = GetHourClimateForecasts(currentClimateStation.ClimateStationID, ForDate);
                    }
                    if (ts.Days > -20)
                    {
                        climateDataStartDate = GetHourClimateObservations(currentClimateStation.ClimateStationID, ForDate);
                        if (climateDataStartDate.ClimateDataValues.Count == 0)
                        {
                            climateDataStartDate = GetHourArchivedClimateData(currentClimateStation.ClimateStationID, ForDate);
                        }
                    }
                    else
                    {
                        climateDataStartDate = GetHourArchivedClimateData(currentClimateStation.ClimateStationID, ForDate);
                    }
                }

                richTextBoxDataTool.AppendText("ClimateStationName [" + currentClimateStation.ClimateStationName + "] TCID [" + currentClimateStation.TCID + "] Date [" + climateDataStartDate.ClimateDataDate + "]\r\n\r\n");
                richTextBoxDataTool.AppendText("Hourly values: \r\n");
                richTextBoxDataTool.AppendText("DateTime observation\tTemp \tRel H\tWnd S\tWnd D\tPres \tSnow \tRain\tTot P\tDew P\tMax T\tMin T\tHeatDD\tCoolDD\tSnow G\tDir MG\tSp MG\r\n");
                foreach (ClimateDataValue cdv in climateDataStartDate.ClimateDataValues)
                {
                    //richTextBoxDataTool.AppendText(cdv.ClimateDataDateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'") + "\t");
                    richTextBoxDataTool.AppendText(cdv.ClimateDataDateTime.ToString("yyyy/MM/dd HH:mm:ss") + "\t");
                    richTextBoxDataTool.AppendText(cdv.Temperature_C == null ? "-----\t" : (((double)cdv.Temperature_C).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.RelativeHumidity_Perc == null ? "-----\t" : (((double)cdv.RelativeHumidity_Perc).ToString("F0") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.WindSpeed_kph == null ? "-----\t" : (((double)cdv.WindSpeed_kph).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.WindDirection_0North == null ? "-----\t" : (((double)cdv.WindDirection_0North).ToString("F0") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.AtmPressure_kpa == null ? "-----\t" : (((double)cdv.AtmPressure_kpa).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.Snow_cm == null ? "-----\t" : (((double)cdv.Snow_cm).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.Rainfall_mm == null ? "-----\t" : (((double)cdv.Rainfall_mm).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.TotalPrecip_mm_cm == null ? "-----\t" : (((double)cdv.TotalPrecip_mm_cm).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.DewPointTemp_C == null ? "-----\t" : (((double)cdv.DewPointTemp_C).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.MaxTemp_C == null ? "-----\t" : (((double)cdv.MaxTemp_C).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.MinTemp_C == null ? "-----\t" : (((double)cdv.MinTemp_C).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.HeatDegDays_C == null ? "-----\t" : (((double)cdv.HeatDegDays_C).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.CoolDegDays_C == null ? "-----\t" : (((double)cdv.CoolDegDays_C).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.SnowOnGround_cm == null ? "-----\t" : (((double)cdv.SnowOnGround_cm).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.DirMaxGust_0North == null ? "-----\t" : (((double)cdv.DirMaxGust_0North).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv.SpdMaxGust_kmh == null ? "-----\t" : (((double)cdv.SpdMaxGust_kmh).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText("\r\n");
                }

                climateDataStartDate = GetDayClimateData(currentClimateStation.ClimateStationID, ForDate);

                ClimateDataValue cdv2 = climateDataStartDate.ClimateDataValues.FirstOrDefault<ClimateDataValue>();

                richTextBoxDataTool.AppendText("\r\n");
                richTextBoxDataTool.AppendText("Daily values: \r\n");
                richTextBoxDataTool.AppendText("DateTime obs\tTemp \tRel H\tWnd S\tWnd D\tPres \tSnow \tRain\tTot P\tDew P\tMax T\tMin T\tHeatDD\tCoolDD\tSnow G\tDir MG\tSp MG\r\n");
                if (cdv2 != null)
                {
                    richTextBoxDataTool.AppendText(cdv2.ClimateDataDateTime.ToString("yyyy/MM/dd") + "\t");
                    richTextBoxDataTool.AppendText(cdv2.Temperature_C == null ? "-----\t" : (((double)cdv2.Temperature_C).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.RelativeHumidity_Perc == null ? "-----\t" : (((double)cdv2.RelativeHumidity_Perc).ToString("F0") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.WindSpeed_kph == null ? "-----\t" : (((double)cdv2.WindSpeed_kph).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.WindDirection_0North == null ? "-----\t" : (((double)cdv2.WindDirection_0North).ToString("F0") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.AtmPressure_kpa == null ? "-----\t" : (((double)cdv2.AtmPressure_kpa).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.Snow_cm == null ? "-----\t" : (((double)cdv2.Snow_cm).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.Rainfall_mm == null ? "-----\t" : (((double)cdv2.Rainfall_mm).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.TotalPrecip_mm_cm == null ? "-----\t" : (((double)cdv2.TotalPrecip_mm_cm).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.DewPointTemp_C == null ? "-----\t" : (((double)cdv2.DewPointTemp_C).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.MaxTemp_C == null ? "-----\t" : (((double)cdv2.MaxTemp_C).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.MinTemp_C == null ? "-----\t" : (((double)cdv2.MinTemp_C).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.HeatDegDays_C == null ? "-----\t" : (((double)cdv2.HeatDegDays_C).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.CoolDegDays_C == null ? "-----\t" : (((double)cdv2.CoolDegDays_C).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.SnowOnGround_cm == null ? "-----\t" : (((double)cdv2.SnowOnGround_cm).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.DirMaxGust_0North == null ? "-----\t" : (((double)cdv2.DirMaxGust_0North).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText(cdv2.SpdMaxGust_kmh == null ? "-----\t" : (((double)cdv2.SpdMaxGust_kmh).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText("\r\n");
                }
            }
        }
        private ClimateDataStartDate GetDayClimateData(int StationID, DateTime ForDate)
        {
            ClimateStation climateStation = (from c in cdbe.ClimateStations where c.ClimateStationID == StationID select c).FirstOrDefault<ClimateStation>();

            if (climateStation == null)
            {
                lblStatus.Text = "Could not find climate station with ClimateStationID = [" + StationID + "]";
                return new ClimateDataStartDate();
            }

            ClimateDataStartDate climateDataStartDate = (from c in cdbe.ClimateDataStartDates
                                                         where c.ClimateStationID == StationID
                                                         && c.IsArchivedData == true
                                                         && c.ClimateDataType == 1 /* hourly  --- 0 is daily */
                                                         && c.ClimateDataDate == ForDate
                                                         select c).FirstOrDefault<ClimateDataStartDate>();

            if (climateDataStartDate != null)
            {
                return climateDataStartDate;
            }

            climateDataStartDate = new ClimateDataStartDate();
            climateDataStartDate.ClimateStationID = StationID;
            climateDataStartDate.ClimateDataDate = ForDate;
            climateDataStartDate.ClimateDataType = 1; /* hourly  --- 0 is daily */
            climateDataStartDate.IsArchivedData = true;

            // daily climate page
            string url = string.Format("http://www.climate.weatheroffice.gc.ca/climateData/dailydata_e.html?StationID={0}&timeframe=1&Year={1}&Month={2}&Day={3}", climateStation.ECDBID, ForDate.Year, ForDate.Month, ForDate.Day);

            DownloadCompleted = false;
            webBrowserDataTool.Navigate(url);

            while (!DownloadCompleted)
            {
                Application.DoEvents();
            }

            if (webBrowserDataTool.Document.GetElementById("dynamicDataTable").Children[0].Children.Count != 4)
            {
                richTextBoxDataTool.Text = "No data available for this date";
                return new ClimateDataStartDate();
            }

            HtmlElement htmlTableBody = webBrowserDataTool.Document.GetElementById("dynamicDataTable").Children[0].Children[3];

            if (htmlTableBody.TagName != "TBODY")
            {
                return new ClimateDataStartDate();
            }

            if (ForDate.Day >= htmlTableBody.Children.Count)
            {
                return new ClimateDataStartDate();
            }

            HtmlElement htmlTr = htmlTableBody.Children[ForDate.Day - 1];

            ClimateDataValue cdv = new ClimateDataValue();

            int tdCount = 0;
            foreach (HtmlElement htmlTd in htmlTr.Children)
            {
                switch (tdCount)
                {
                    case 0:
                        {
                            int day = 0;
                            int.TryParse(htmlTd.InnerText.Trim().Substring(0, 2), out day);
                            if (day != ForDate.Day)
                            {
                                return new ClimateDataStartDate();
                            }
                            cdv.ClimateDataDateTime = new DateTime(ForDate.Year, ForDate.Month, ForDate.Day, day, 0, 0);
                        }
                        break;
                    case 1:
                        {
                            cdv.MaxTemp_C = GetFloat(htmlTd.InnerText);
                        }
                        break;
                    case 2:
                        {
                            cdv.MinTemp_C = GetFloat(htmlTd.InnerText);
                        }
                        break;
                    case 3:
                        {
                            cdv.Temperature_C = GetFloat(htmlTd.InnerText);
                        }
                        break;
                    case 4:
                        {
                            cdv.HeatDegDays_C = GetFloat(htmlTd.InnerText);
                        }
                        break;
                    case 5:
                        {
                            cdv.CoolDegDays_C = GetFloat(htmlTd.InnerText);
                        }
                        break;
                    case 6:
                        {
                            cdv.Rainfall_mm = GetFloat(htmlTd.InnerText);
                        }
                        break;
                    case 7:
                        {
                            cdv.Snow_cm = GetFloat(htmlTd.InnerText);
                        }
                        break;
                    case 8:
                        {
                            cdv.TotalPrecip_mm_cm = GetFloat(htmlTd.InnerText);
                        }
                        break;
                    case 9:
                        {
                            cdv.SnowOnGround_cm = GetFloat(htmlTd.InnerText);
                        }
                        break;
                    case 10:
                        {
                            cdv.DirMaxGust_0North = GetFloat(htmlTd.InnerText);
                            if (cdv.DirMaxGust_0North != null)
                            {
                                cdv.DirMaxGust_0North *= 10;
                            }
                        }
                        break;
                    case 11:
                        {
                            if (!htmlTd.InnerText.Trim().StartsWith("<"))
                            {
                                cdv.SpdMaxGust_kmh = GetFloat(htmlTd.InnerText);
                            }
                        }
                        break;
                    default:
                        break;
                }
                tdCount += 1;

            }

            climateDataStartDate.ClimateDataValues.Add(cdv);

            try
            {
                cdbe.ClimateDataStartDates.Add(climateDataStartDate);
                cdbe.SaveChanges();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error while trying to create a new ClimateDataStartDate in the DB " + ex.Message + ex.InnerException.Message;
                return new ClimateDataStartDate();
            }

            return climateDataStartDate;

        }
        private void butGetObservationsHydro_Click(object sender, EventArgs e)
        {
            if (comboBoxStationHydro.SelectedItem == null)
            {
                lblStatus.Text = "Please select a hydrometric station";
                return;
            }

            HydroStationInfo hydroStationInfo = (HydroStationInfo)comboBoxStationHydro.SelectedItem;

            int NumberOfDays = 1;

            for (int i = 0; i < NumberOfDays; i++)
            {
                DateTime ForDate = new DateTime(dateTimePickerStartDateHydro.Value.Year, dateTimePickerStartDateHydro.Value.Month, dateTimePickerStartDateHydro.Value.Day + i);
                DateTime CurrentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                TimeSpan ts = ForDate - CurrentDate;
                HydrometricDataStartDate hydrometricDataStartDate = null;
                //if (ts.Days >= 0)
                //{
                //    hydrometricDataStartDate = GetHourHydroForecasts(currentHydrometricStation.HydrometricStationID, ForDate);
                //    if (ts.Days == 0)
                //    {
                //        hydrometricDataStartDate = GetHourHydroObservations(currentHydrometricStation.HydrometricStationID, ForDate);
                //    }
                //}
                //else
                //{
                //    if (ts.Days > -1)
                //    {
                //        hydrometricDataStartDate = GetHourHydroForecasts(currentHydrometricStation.HydrometricStationID, ForDate);
                //    }
                //    if (ts.Days > -20)
                //    {
                //        hydrometricDataStartDate = GetHourHydroObservations(currentHydrometricStation.HydrometricStationID, ForDate);
                //        if (hydrometricDataStartDate.HydrometricDataValues.Count == 0)
                //        {
                //            hydrometricDataStartDate = GetHourArchivedHydroData(currentHydrometricStation.HydrometricStationID, ForDate);
                //        }
                //    }
                //    else
                //    {
                //        hydrometricDataStartDate = GetHourArchivedHydroData(currentHydrometricStation.HydrometricStationID, ForDate);
                //    }
                //}

                hydrometricDataStartDate = GetHourArchivedHydroData(currentHydrometricStation.HydrometricStationID, ForDate);
                for (int j = 0; j < 10; j++)
                {
                    richTextBoxDataTool.AppendText("\r\n");
                }
                richTextBoxDataTool.AppendText("HydrometricStationName [" + currentHydrometricStation.HydrometricStationName + "] TCID [" + currentClimateStation.TCID + "] Date [" + hydrometricDataStartDate.HydrometricDataDate + "]\r\n\r\n");
                richTextBoxDataTool.AppendText("Hourly values: \r\n");
                richTextBoxDataTool.AppendText("DateTime observation\tFlow\r\n");
                foreach (HydrometricDataValue hdv in hydrometricDataStartDate.HydrometricDataValues)
                {
                    richTextBoxDataTool.AppendText(hdv.HydrometricDataDateTime.ToString("yyyy/MM/dd HH:mm:ss") + "\t");
                    richTextBoxDataTool.AppendText(hdv.Flow_m3ps == null ? "-----\t" : (((double)hdv.Flow_m3ps).ToString("F1") + "\t"));
                    richTextBoxDataTool.AppendText("\r\n");
                    Application.DoEvents();
                }

                //hydrometricDataStartDate = GetDayHydrometricData(currentHydrometricStation.HydrometricStationID, ForDate);

                //HydrometricDataValue hdv2 = hydrometricDataStartDate.HydrometricDataValues.FirstOrDefault<HydrometricDataValue>();

                //richTextBoxDataTool.AppendText("\r\n");
                //richTextBoxDataTool.AppendText("Daily values: \r\n");
                //richTextBoxDataTool.AppendText("DateTime obs\tFlow\r\n");
                //if (hdv2 != null)
                //{
                //    richTextBoxDataTool.AppendText(hdv2.HydrometricDataDateTime.ToString("yyyy/MM/dd") + "\t");
                //    richTextBoxDataTool.AppendText(hdv2.Flow_m3ps == null ? "-----\t" : (((double)hdv2.Flow_m3ps).ToString("F1") + "\t"));
                //    richTextBoxDataTool.AppendText("\r\n");
                //}
            }














            //if (comboBoxStationHydro.SelectedItem == null)
            //{
            //    return;
            //}

            //HydroStationInfo hsi = (HydroStationInfo)comboBoxStationHydro.SelectedItem;

            //HydrometricStation hs = (from h in cdbe.HydrometricStations where h.HydrometricStationID == hsi.HydrometricStationID select h).FirstOrDefault<HydrometricStation>();

            //if (hs == null)
            //{
            //    lblStatus.Text = "Please select a Hydrometric Station\r\n";
            //    return;
            //}

            //if (hs.FedStationNumber != null)
            //{
            //    string url = @"http://www.wsc.ec.gc.ca/applications/H2O/report-eng.cfm?station=" + hs.FedStationNumber + @"&report=daily&data=flow&year=" + dateTimePickerStartDateHydro.Value.Year;
            //    richTextBoxDataTool.AppendText(url + "\r\n");
            //    DownloadCompleted = false;
            //    webBrowserDataTool.Navigate(url);

            //    while (!DownloadCompleted)
            //    {
            //        Application.DoEvents();
            //    }

            //    richTextBoxDataTool.AppendText(webBrowserDataTool.Document.Url.ToString() + "\r\n");
            //}
            //else
            //{
            //    // nothing for now
            //}
        }

        private HydrometricDataStartDate GetDayHydrometricData(int p, DateTime ForDate)
        {
            throw new NotImplementedException();
        }

        private HydrometricDataStartDate GetHourArchivedHydroData(int stationID, DateTime ForDate)
        {
            List<HydrometricDataValue> hdvRetList = null;
            // check if data is already in DB
            HydrometricDataStartDate hdsdRet = (from c in cdbe.HydrometricDataStartDates
                                                where c.HydrometricDataDate == ForDate
                                                select c).FirstOrDefault<HydrometricDataStartDate>();

            if (hdsdRet != null)
            {
                hdvRetList = (from c in cdbe.HydrometricDataValues
                              where c.HydrometricDataStartDateID == hdsdRet.HydroMetricDataStartDateID
                              orderby c.HydrometricDataDateTime
                              select c).ToList<HydrometricDataValue>();

                if (hdvRetList.Count > 0)
                {
                    hdsdRet.HydrometricDataValues = hdvRetList;
                    return hdsdRet;
                }
            }

            // make sure i'm logged on
            DownloadCompleted = false;
            string url = "http://www.wateroffice.ec.gc.ca/login_e.html";
            webBrowserDataTool.Navigate(url);
            List<HydrometricDataValue> hdvList = new List<HydrometricDataValue>();
            while (!DownloadCompleted)
            {
                Application.DoEvents();
            }

            HtmlElement htmlElementLoginForm = webBrowserDataTool.Document.GetElementById("loginform");
            if (htmlElementLoginForm == null)
            {
                lblStatus.Text = "Could not find id == loginform in url [" + url + "]";
                return new HydrometricDataStartDate();
            }
            else
            {
                HtmlElement htmlElementUserName = webBrowserDataTool.Document.GetElementById("username");
                if (htmlElementUserName == null)
                {
                    HtmlElement htmlElementLogOut = htmlElementLoginForm.Children[0].Children[0];
                    if (htmlElementLogOut == null)
                    {
                        lblStatus.Text = "Could not find id == username or logout button in url [" + url + "]";
                        return new HydrometricDataStartDate();
                    }
                }
                else
                {
                    HtmlElement htmlElementPassword = webBrowserDataTool.Document.GetElementById("password");
                    if (htmlElementPassword == null)
                    {
                        lblStatus.Text = "Could not find id == password in url [" + url + "]";
                        return new HydrometricDataStartDate();
                    }
                    else
                    {
                        htmlElementUserName.SetAttribute("value", "realtime");
                        htmlElementPassword.SetAttribute("value", "hydrometric");

                        HtmlElement htmlElementSubmit = htmlElementLoginForm.Children[0].Children[2].Children[0];
                        object TheSubmitButton = htmlElementSubmit.DomElement;

                        MethodInfo clickMethod = TheSubmitButton.GetType().GetMethod("click");
                        clickMethod.Invoke(TheSubmitButton, null);

                        // the login is refreshing to the index_e.html page, waiting for that to happen
                        while (webBrowserDataTool.Url.AbsoluteUri == url)
                        {
                            Application.DoEvents();
                        }
                    }
                }
            }

            lblStatus.Text = "Is logged in";

            if (webBrowserDataTool.Document == null)
            {
                lblStatus.Text = "Error please login";
                return new HydrometricDataStartDate();
            }

            if (webBrowserDataTool.Document.Cookie == null)
            {
                lblStatus.Text = "Error please login";
                return new HydrometricDataStartDate();
            }

            WebClient client = new WebClient();

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            client.Headers.Add("Cookie", webBrowserDataTool.Document.Cookie);

            List<int> TryCountDays = new List<int>() { 1, 10, 30, 10000 };

            foreach (int TryDays in TryCountDays)
            {
                string s;

                try
                {
                    DateTime ForDateStart = ForDate.AddDays(TryDays * (-1));
                    DateTime ForDateEnd = ForDate.AddDays(TryDays);
                    url = string.Format("http://www.wateroffice.ec.gc.ca/graph/download.php?format=tab&stn={0}&prm1=3&start={1}-{2}-{3}&end={4}-{5}-{6}&norawdis=0&norawwl=0&lang=e", currentHydrometricStation.FedStationNumber, ForDateStart.Year, string.Format("{0:00}", ForDateStart.Month), string.Format("{0:00}", ForDateStart.Day), ForDateEnd.Year, string.Format("{0:00}", ForDateEnd.Month), string.Format("{0:00}", ForDateEnd.Day));
                    Stream data = client.OpenRead(url);
                    StreamReader reader = new StreamReader(data);
                    s = reader.ReadToEnd();
                    data.Flush();
                    data.Close();
                    reader.Close();
                }
                catch (Exception)
                {
                    lblStatus.Text = "Error: Could not read url [" + url + "]";
                    return new HydrometricDataStartDate();
                }

                richTextBoxDataTool.Text = s;
                FileInfo fi = new FileInfo(@"c:\cssp\tempHydroLevel.txt");

                StreamWriter sw = fi.CreateText();
                sw.Write(s);
                sw.Flush();
                sw.Close();

                StreamReader sr = fi.OpenText();

                string LineStr = sr.ReadLine(); // line 1
                if (!LineStr.StartsWith("Real-time data - subject to revision"))
                {
                    lblStatus.Text = "Error: Real-Time data - subject to revision not found for first line";
                    return new HydrometricDataStartDate();
                }
                LineStr = sr.ReadLine(); // line 2
                LineStr = sr.ReadLine(); // line 3
                LineStr = sr.ReadLine(); // line 4

                if (LineStr.Substring(0, 7) != currentHydrometricStation.FedStationNumber)
                {
                    lblStatus.Text = "Error: [" + LineStr.Substring(0, 8) + " != " + currentHydrometricStation.FedStationNumber + "]";
                    return new HydrometricDataStartDate();
                }
                LineStr = sr.ReadLine(); // line 5
                LineStr = sr.ReadLine(); // line 6
                if (!LineStr.StartsWith("Description of parameters:"))
                {
                    lblStatus.Text = "Error: Description of parameters: not found for first line";
                    return new HydrometricDataStartDate();
                }
                LineStr = sr.ReadLine(); // line 7
                if (!LineStr.StartsWith("3\tRaw water level(m)"))
                {
                    lblStatus.Text = "Error: 3	Raw water level(m) not found for first line";
                    return new HydrometricDataStartDate();
                }
                while (!sr.EndOfStream)
                {
                    LineStr = sr.ReadLine();
                    if (LineStr.StartsWith("Date\tParameter\tValue"))
                    {
                        break;
                    }
                }


                RatingCurve rc = (from c in cdbe.RatingCurves
                                  where c.HydrometricStationID == currentHydrometricStation.HydrometricStationID
                                  select c).FirstOrDefault<RatingCurve>();

                if (rc == null)
                {
                    lblStatus.Text = "Error: Rating Curve for station [" + currentHydrometricStation.FedStationNumber + "] was not found";
                    return new HydrometricDataStartDate();
                }

                List<RatingCurveValue> rcvList = (from c in cdbe.RatingCurveValues
                                                  where c.RatingCurveID == rc.RatingCurveID
                                                  orderby c.StageValue
                                                  select c).ToList<RatingCurveValue>();

                if (rcvList.Count == 0)
                {
                    lblStatus.Text = "Error: Rating Curve Values for station [" + currentHydrometricStation.FedStationNumber + "] was not found";
                    return new HydrometricDataStartDate();
                }

                int count = 0;
                while (!sr.EndOfStream)
                {
                    Application.DoEvents();

                    count += 1;
                    LineStr = sr.ReadLine();
                    if (LineStr.Trim() == "")
                    {
                        continue;
                    }
                    string[] ValueArr = LineStr.Split("\t".ToCharArray()[0]);
                    if (ValueArr.Count() != 3)
                    {
                        lblStatus.Text = "Error: while reading the values of the rating curve";
                        sr.Close();
                        return new HydrometricDataStartDate();
                    }

                    DateTime ValueDateTime;
                    if (!DateTime.TryParse(ValueArr[0], out ValueDateTime))
                    {
                        lblStatus.Text = "Error: while trying to parse the ValueArr[0] value (TheDateTime)";
                        sr.Close();
                        return new HydrometricDataStartDate();
                    }

                    float WaterLevel;
                    if (!float.TryParse(ValueArr[2], out WaterLevel))
                    {
                        lblStatus.Text = "Error: while trying to parse the ValueArr[2] (WaterLevel)";
                        sr.Close();
                        return new HydrometricDataStartDate();
                    }

                    HydrometricDataValue hdv = new HydrometricDataValue();
                    hdv.HydrometricDataDateTime = ValueDateTime;

                    RatingCurveValue rcvBelow = (from c in rcvList
                                                 where c.StageValue <= WaterLevel
                                                 orderby c.StageValue descending
                                                 select c).FirstOrDefault<RatingCurveValue>();

                    if (rcvBelow == null)
                    {
                        lblStatus.Text = "Error: Could not find Rating Curve Value Below for water level [" + WaterLevel + "]";
                        continue;
                    }

                    RatingCurveValue rcvAbove = (from c in rcvList
                                                 where c.StageValue >= WaterLevel
                                                 orderby c.StageValue
                                                 select c).FirstOrDefault<RatingCurveValue>();

                    if (rcvAbove == null)
                    {
                        lblStatus.Text = "Error: Could not find Rating Curve Value Above for water level [" + WaterLevel + "]";
                        continue;
                    }
                    if ((rcvAbove.DischargeValue - rcvAbove.DischargeValue) == 0)
                    {
                        hdv.Flow_m3ps = rcvBelow.DischargeValue;
                    }
                    else
                    {
                        hdv.Flow_m3ps = rcvBelow.DischargeValue + ((WaterLevel - rcvBelow.StageValue) / (rcvAbove.StageValue - rcvBelow.StageValue)) * (rcvAbove.DischargeValue - rcvAbove.DischargeValue);
                    }

                    if (double.IsNaN(hdv.Flow_m3ps))
                    {
                        lblStatus.Text = "Error: hdv.Flow_m3ps is NaN. Will skip value.";
                        continue;
                    }
                    if (double.IsInfinity(hdv.Flow_m3ps))
                    {
                        lblStatus.Text = "Error: hdv.Flow_m3ps is Infinity Will skip value.";
                        continue;
                    }

                    hdvList.Add(hdv);
                }

                sr.Close();

                // before adding to the DB we should correct and filter the hdvList
                // getting the first value and date
                HydrometricDataValue hdvStart = (from c in hdvList
                                                 orderby c.HydrometricDataDateTime
                                                 select c).FirstOrDefault<HydrometricDataValue>();

                if (hdvStart == null)
                {
                    lblStatus.Text = "Error: No water level => flow parsed";
                    return new HydrometricDataStartDate();
                }

                if (hdvStart.HydrometricDataDateTime.Minute >= 30)
                {
                    hdvStart.HydrometricDataDateTime = hdvStart.HydrometricDataDateTime.AddMinutes(60 - hdvStart.HydrometricDataDateTime.Minute);
                }
                else
                {
                    hdvStart.HydrometricDataDateTime = hdvStart.HydrometricDataDateTime.AddMinutes(hdvStart.HydrometricDataDateTime.Minute * -1);
                }

                DateTime StartDate = new DateTime(hdvStart.HydrometricDataDateTime.Year, hdvStart.HydrometricDataDateTime.Month, hdvStart.HydrometricDataDateTime.Day, hdvStart.HydrometricDataDateTime.Hour, 0, 0);

                // getting the last value and date
                HydrometricDataValue hdvEnd = (from c in hdvList
                                               orderby c.HydrometricDataDateTime descending
                                               select c).FirstOrDefault<HydrometricDataValue>();

                if (hdvEnd == null)
                {
                    lblStatus.Text = "Error: No water level => flow parsed";
                    return new HydrometricDataStartDate();
                }

                if (hdvEnd.HydrometricDataDateTime.Minute >= 30)
                {
                    hdvEnd.HydrometricDataDateTime = hdvEnd.HydrometricDataDateTime.AddMinutes(60 - hdvEnd.HydrometricDataDateTime.Minute);
                }
                else
                {
                    hdvEnd.HydrometricDataDateTime = hdvEnd.HydrometricDataDateTime.AddMinutes(hdvEnd.HydrometricDataDateTime.Minute * -1);
                }

                DateTime EndDate = new DateTime(hdvEnd.HydrometricDataDateTime.Year, hdvEnd.HydrometricDataDateTime.Month, hdvEnd.HydrometricDataDateTime.Day, hdvEnd.HydrometricDataDateTime.Hour, 0, 0);

                List<HydrometricDataValue> hdvSelectedList = new List<HydrometricDataValue>();

                while (StartDate <= EndDate)
                {
                    HydrometricDataValue hdvBelow = (from c in hdvList
                                                     where c.HydrometricDataDateTime <= StartDate
                                                     orderby c.HydrometricDataDateTime descending
                                                     select c).FirstOrDefault<HydrometricDataValue>();

                    HydrometricDataValue hdvAbove = (from c in hdvList
                                                     where c.HydrometricDataDateTime >= StartDate
                                                     orderby c.HydrometricDataDateTime
                                                     select c).FirstOrDefault<HydrometricDataValue>();

                    if (hdvBelow != null && hdvAbove != null)
                    {
                        TimeSpan ts = hdvAbove.HydrometricDataDateTime - hdvBelow.HydrometricDataDateTime;

                        HydrometricDataValue hdvTemp = new HydrometricDataValue();
                        hdvTemp.HydrometricDataDateTime = StartDate;

                        if (ts.Hours < 1)
                        {
                            hdvTemp.Flow_m3ps = (hdvAbove.Flow_m3ps + hdvBelow.Flow_m3ps) / 2;
                        }
                        else
                        {
                            TimeSpan top = StartDate - hdvBelow.HydrometricDataDateTime;
                            TimeSpan bottom = hdvAbove.HydrometricDataDateTime - hdvBelow.HydrometricDataDateTime;
                            if (hdvBelow.Flow_m3ps > 0 && hdvAbove.Flow_m3ps > 0)
                            {
                                hdvTemp.Flow_m3ps = (double)hdvBelow.Flow_m3ps + (((double)hdvAbove.Flow_m3ps - (double)hdvBelow.Flow_m3ps) * (top.TotalMinutes / bottom.TotalMinutes));
                            }
                        }

                        hdvSelectedList.Add(hdvTemp);
                    }

                    StartDate = StartDate.AddHours(1);
                }

                // should contain 24 values for the date

                List<HydrometricDataValue> hdvDayList = (from c in hdvSelectedList
                                                         where c.HydrometricDataDateTime.Year == ForDate.Year
                                                         && c.HydrometricDataDateTime.Month == ForDate.Month
                                                         && c.HydrometricDataDateTime.Day == ForDate.Day
                                                         orderby c.HydrometricDataDateTime
                                                         select c).ToList<HydrometricDataValue>();

                if (hdvDayList.Count != 24)
                {
                    DateTime NowDate = DateTime.Now;
                    if (ForDate.Year == NowDate.Year && ForDate.Month == NowDate.Month && ForDate.Day == NowDate.Day)
                    {
                        // does not require 24 values since it is today and would not have 24 values
                    }
                    else
                    {
                        continue;
                    }
                }

                // saving everything in the DB
                StartDate = new DateTime(hdvSelectedList[0].HydrometricDataDateTime.Year, hdvSelectedList[0].HydrometricDataDateTime.Month, hdvSelectedList[0].HydrometricDataDateTime.Day);

                while (StartDate <= EndDate)
                {
                    bool PleaseAddInDB = true;

                    // check if already in db
                    HydrometricDataStartDate hdsdExist = (from c in cdbe.HydrometricDataStartDates
                                                          where c.HydrometricDataDate.Year == StartDate.Year
                                                          && c.HydrometricDataDate.Month == StartDate.Month
                                                          && c.HydrometricDataDate.Day == StartDate.Day
                                                          select c).FirstOrDefault<HydrometricDataStartDate>();

                    if (hdsdExist != null) // no need to add if already in DB
                    {
                        // should verify if it has 24 values
                        int CountVal = (from c in cdbe.HydrometricDataValues
                                        where c.HydrometricDataStartDateID == hdsdExist.HydroMetricDataStartDateID
                                        select c).Count();

                        if (CountVal == 24)
                        {
                            PleaseAddInDB = false;
                        }
                        else
                        {
                            cdbe.HydrometricDataStartDates.Remove(hdsdExist);
                            try
                            {
                                cdbe.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                lblStatus.Text = "Error: Could not create new HydrometricDataStartDate" + ex.Message + " inner " + ex.InnerException.Message;
                            }
                        }
                    }

                    if (PleaseAddInDB)
                    {
                        hdvDayList = (from c in hdvSelectedList
                                      where c.HydrometricDataDateTime.Year == StartDate.Year
                                      && c.HydrometricDataDateTime.Month == StartDate.Month
                                      && c.HydrometricDataDateTime.Day == StartDate.Day
                                      orderby c.HydrometricDataDateTime
                                      select c).ToList<HydrometricDataValue>();

                        if (hdvDayList.Count > 0)
                        {
                            HydrometricDataStartDate hdsd = new HydrometricDataStartDate();
                            hdsd.HydrometricStationID = stationID;
                            hdsd.HydrometricDataDate = StartDate;
                            hdsd.FromForcastDate = null;
                            hdsd.HydrometricDataType = 1; /* hourly */
                            hdsd.IsForcastData = false;
                            hdsd.IsObservationData = true;
                            hdsd.IsArchivedData = false;

                            foreach (HydrometricDataValue hdv in hdvDayList)
                            {
                                hdsd.HydrometricDataValues.Add(hdv);
                            }

                            try
                            {
                                cdbe.HydrometricDataStartDates.Add(hdsd);
                                cdbe.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                lblStatus.Text = "Error: Could not create new HydrometricDataStartDate" + ex.Message + " inner" + ex.InnerException.Message;
                                return new HydrometricDataStartDate();
                            }
                        }
                    }

                    StartDate = StartDate.AddDays(1);
                }

                break;
            }

            lblStatus.Text = "Done ...";

            // should get the values from the DB
            hdsdRet = (from c in cdbe.HydrometricDataStartDates
                       where c.HydrometricDataDate == ForDate
                       select c).FirstOrDefault<HydrometricDataStartDate>();

            if (hdsdRet == null)
            {
                return new HydrometricDataStartDate();
            }

            hdvRetList = (from c in cdbe.HydrometricDataValues
                          where c.HydrometricDataStartDateID == hdsdRet.HydroMetricDataStartDateID
                          orderby c.HydrometricDataDateTime
                          select c).ToList<HydrometricDataValue>();

            if (hdvRetList.Count > 0)
            {
                hdsdRet.HydrometricDataValues = hdvRetList;
            }

            return hdsdRet;

        }

        private HydrometricDataStartDate GetHourHydroObservations(int p, DateTime ForDate)
        {
            throw new NotImplementedException();
        }

        private HydrometricDataStartDate GetHourHydroForecasts(int p, DateTime ForDate)
        {
            throw new NotImplementedException();
        }

        private HydrometricDataStartDate GetHydroData(int p, DateTime ForDate)
        {
            throw new NotImplementedException();
        }
        private void butUpdateRatingCurves_Click(object sender, EventArgs e)
        {
            UpdateRatingCurve();
        }
        private void ClimateStationsShowingChanged(object sender, EventArgs e)
        {
            ResetClimateStationComboBox();
        }
        private void comboBoxStationClimate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxStationClimate.SelectedItem == null)
            {
                MessageBox.Show("Please select a station");
                return;
            }

            ClimateStationInfo selectedClimateStationInfo = (ClimateStationInfo)comboBoxStationClimate.SelectedItem;

            currentClimateStation = (from c in climateStationList where c.ClimateStationID == selectedClimateStationInfo.ClimateStationID select c).FirstOrDefault<ClimateStation>();

            if (currentClimateStation == null)
            {
                lblStatus.Text = "Error: currentClimateStation could not be set";
                return;
            }

            if (currentClimateStation.HourlyStartDate != null)
            {
                lblHourlyVal.Text = string.Format("{0:d} - {1:d}", currentClimateStation.HourlyStartDate, currentClimateStation.HourlyEndDate);
                DateTime NowDate = DateTime.Now;
                if ((bool)currentClimateStation.HourlyNow)
                {
                    butGetClimateData.Enabled = true;
                    lblHourlyVal.ForeColor = Color.Green;
                }
                else
                {
                    butGetClimateData.Enabled = false;
                    lblHourlyVal.ForeColor = Color.Red;
                }
            }
            else
            {
                lblHourlyVal.Text = "[Empty]";
                butGetClimateData.Enabled = false;
            }

            if (currentClimateStation.DailyStartDate != null)
            {
                lblDailyVal.Text = string.Format("{0:d} - {1:d}", currentClimateStation.DailyStartDate, currentClimateStation.DailyEndDate);
                if ((bool)currentClimateStation.DailyNow)
                {
                    lblDailyVal.ForeColor = Color.Green;
                }
                else
                {
                    lblDailyVal.ForeColor = Color.Red;
                }
            }
            else
            {
                lblDailyVal.Text = "[Empty]";
                lblDailyVal.ForeColor = Color.Red;
            }

            if (currentClimateStation.MonthlyStartDate != null)
            {
                lblMonthlyVal.Text = string.Format("{0:d} - {1:d}", currentClimateStation.MonthlyStartDate, currentClimateStation.MonthlyEndDate);
                if ((bool)currentClimateStation.MonthlyNow)
                {
                    lblMonthlyVal.ForeColor = Color.Green;
                }
                else
                {
                    lblMonthlyVal.ForeColor = Color.Red;
                }
            }
            else
            {
                lblMonthlyVal.Text = "[Empty]";
                lblMonthlyVal.ForeColor = Color.Red;
            }

        }
        private void comboBoxProvinceClimate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxProvinceClimate.SelectedItem == null)
            {
                MessageBox.Show("Please select a province.");
                return;
            }

            string selectedProv = (string)comboBoxProvinceClimate.SelectedItem;

            climateStationList = (from s in cdbe.ClimateStations
                                  where s.Province == selectedProv && s.TCID != null
                                  orderby s.ClimateStationName
                                  select s).ToList<ClimateStation>();

            ResetClimateStationComboBox();
        }
        private void comboBoxProvinceHydro_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxProvinceHydro.SelectedItem == null)
            {
                MessageBox.Show("Please select a province.");
                return;
            }

            string selectedProv = (string)comboBoxProvinceHydro.SelectedItem;

            List<HydroStationInfo> stationInfoList = (from s in cdbe.HydrometricStations where s.Province == selectedProv orderby s.HydrometricStationName select new HydroStationInfo() { HydrometricStationID = s.HydrometricStationID, HydroStationName = ((s.FedStationNumber != null ? "[" + s.FedStationNumber + "] " : " ") + (s.QuebecStationNumber != null ? "[" + s.QuebecStationNumber + "] " : " ") + (s.RealTime == true ? "RT " : "-- ") + s.HydrometricStationName).Trim() }).ToList<HydroStationInfo>();

            comboBoxStationHydro.DataSource = stationInfoList;
            comboBoxStationHydro.SelectedIndex = 0;
        }
        private void comboBoxStationHydro_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxStationHydro.SelectedItem == null)
            {
                MessageBox.Show("Please select a station");
                return;
            }

            HydroStationInfo selectedStationInfo = (HydroStationInfo)comboBoxStationHydro.SelectedItem;

            currentHydrometricStation = (from s in cdbe.HydrometricStations where s.HydrometricStationID == selectedStationInfo.HydrometricStationID orderby s.HydrometricStationName select s).FirstOrDefault<HydrometricStation>();
        }
        private void DataTool_Load(object sender, EventArgs e)
        {
            List<string> provinceList = (from s in cdbe.ClimateStations select s.Province).Distinct().OrderBy(c => c).ToList<string>();

            comboBoxProvinceClimate.DataSource = provinceList;
            comboBoxProvinceClimate.SelectedIndex = 0;

            List<string> provincListHydro = (from s in cdbe.HydrometricStations orderby s.Province select s.Province).Distinct().OrderBy(c => c).ToList<string>();

            comboBoxProvinceHydro.DataSource = provincListHydro;
            comboBoxProvinceHydro.SelectedIndex = 0;
        }
        private void webBrowserDataTool_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            DownloadCompleted = true;
        }
        #endregion Events

        #region Functions Private
        private void CreateClimateStationKML()
        {
            StringBuilder sb = new StringBuilder();
            List<ClimateStation> stationList;
            string[] Active = new string[] { "Active", "Inactive" };
            string[] Province = new string[] { "NB", "BC", "SASK", "MAN", "NU", "YT", "PEI", "NWT", "NS", "ONT", "QUE", "ALTA", "NFLD" };

            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine(@"<Document>");
            sb.AppendLine(@"	<name>Climate Stations</name>");

            foreach (string active in Active)
            {
                sb.AppendLine(@"<Folder>");
                sb.AppendLine(string.Format(@"<name>{0}</name>", active));

                foreach (string p in Province.OrderBy(s => s))
                {
                    sb.AppendLine(@"<Folder>");
                    sb.AppendLine(string.Format(@"<name>{0}</name>", p));

                    if (active == "Active")
                    {
                        stationList = (from s in cdbe.ClimateStations
                                       where s.Latitude > 20
                                       && s.Province == p
                                       && (s.HourlyEndDate.Value.Year == 2012
                                       || s.DailyEndDate.Value.Year == 2012
                                       || s.MonthlyEndDate.Value.Year == 2012)
                                       orderby s.ClimateStationName
                                       select s).ToList<ClimateStation>();
                    }
                    else
                    {
                        stationList = (from s in cdbe.ClimateStations
                                       where s.Latitude > 20
                                       && s.Province == p
                                       && (s.HourlyEndDate.Value.Year != 2012
                                       && s.DailyEndDate.Value.Year != 2012
                                       && s.MonthlyEndDate.Value.Year != 2012)
                                       orderby s.ClimateStationName
                                       select s).ToList<ClimateStation>();

                        //stationList = (from s in cdbe.ClimateStations where s.Latitude > 20 && s.Province == p orderby s.StationName select s).ToList<Station>();
                    }

                    foreach (ClimateStation s in stationList)
                    {
                        string HourlyYears = "";
                        string DailyYears = "";
                        string MonthlyYears = "";

                        if (s.HourlyStartDate.HasValue)
                        {
                            HourlyYears = "hourly [" + s.HourlyStartDate.Value.Year.ToString() + " - " + s.HourlyEndDate.Value.Year.ToString() + "]";
                        }

                        if (s.DailyStartDate.HasValue)
                        {
                            DailyYears = "daily [" + s.DailyStartDate.Value.Year.ToString() + " - " + s.DailyEndDate.Value.Year.ToString() + "]";
                        }

                        if (s.MonthlyStartDate.HasValue)
                        {
                            MonthlyYears = "monthly [" + s.MonthlyStartDate.Value.Year.ToString() + " - " + s.MonthlyEndDate.Value.Year.ToString() + "]";
                        }

                        string ClimateID = "Climate ID [" + s.ClimateID + "]";
                        string WMOID = "WMO ID [" + s.WMOID + "]";
                        string TCID = "TC ID [" + s.TCID + "]";

                        sb.AppendLine(@"	<Placemark>");
                        //sb.AppendLine(string.Format(@"		<name>{0} {1} {2} {3}</name>", s.ClimateStationName, ClimateID, WMOID, TCID));
                        sb.AppendLine(string.Format(@"		<name>{0}</name>", s.TimeOffset_hour, s.ClimateStationName));
                        //sb.AppendLine(string.Format(@"<description><![CDATA[{0} {1} {2}<br /><iframe src=""about:"" width=""500"" height=""1"" />]]></description>]]></description>", HourlyYears, DailyYears, MonthlyYears));
                        sb.AppendLine(@"		<Point>");
                        sb.AppendLine(string.Format(@"			<coordinates>{0},{1},0</coordinates>", s.Longitude, s.Latitude));
                        sb.AppendLine(@"		</Point>");
                        sb.AppendLine(@"	</Placemark>");


                    }
                    sb.AppendLine(@"</Folder>");
                }
                sb.AppendLine(@"</Folder>");
            }

            sb.AppendLine(@"</Document>");
            sb.AppendLine(@"</kml>");

            richTextBoxDataTool.Text = sb.ToString();

            richTextBoxDataTool.SaveFile(@"c:\CSSP\Climate.kml", RichTextBoxStreamType.PlainText);

        }
        private void CreateHydrometricStationKML()
        {
            StringBuilder sb = new StringBuilder();
            List<HydrometricStation> stationList;
            string[] Active = new string[] { "Active", "Inactive" };
            string[] Province = new string[] { "NB", "BC", "SK", "MB", "NU", "YT", "PE", "NT", "NS", "ON", "QC", "AB", "NL" };

            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine(@"<Document>");
            sb.AppendLine(@"	<name>Hydrometric Stations</name>");

            foreach (string active in Active)
            {
                sb.AppendLine(@"<Folder>");
                sb.AppendLine(string.Format(@"<name>{0}</name>", active));

                foreach (string p in Province.OrderBy(s => s))
                {
                    sb.AppendLine(@"<Folder>");
                    sb.AppendLine(string.Format(@"<name>{0}</name>", p));

                    if (active == "Active")
                    {
                        stationList = (from h in cdbe.HydrometricStations
                                       where h.Latitude > 20
                                       && h.Province == p
                                       && h.IsActive == true
                                       orderby h.HydrometricStationName
                                       select h).ToList<HydrometricStation>();
                    }
                    else
                    {
                        stationList = (from h in cdbe.HydrometricStations
                                       where h.Latitude > 20
                                       && h.Province == p
                                       && h.IsActive == false
                                       orderby h.HydrometricStationName
                                       select h).ToList<HydrometricStation>();

                    }

                    foreach (HydrometricStation h in stationList)
                    {
                        string DrainageArea = "";
                        string IsNatural = "";
                        string IsRHBN = "";
                        string HasRealTime = "";
                        string FedStaName = "";
                        string QCStatName = "";

                        if (h.DrainageArea.HasValue)
                        {
                            DrainageArea = h.DrainageArea.Value.ToString("F2");
                        }

                        if (h.IsNatural.HasValue)
                        {
                            if (h.IsNatural.Value == true)
                            {
                                IsNatural = "Y";
                            }
                            else
                            {
                                IsNatural = "N";
                            }
                        }

                        if (h.RHBN.HasValue)
                        {
                            if (h.RHBN.Value == true)
                            {
                                IsRHBN = "Y";
                            }
                            else
                            {
                                IsRHBN = "N";
                            }
                        }

                        if (h.RealTime.HasValue)
                        {
                            if (h.RealTime.Value == true)
                            {
                                IsNatural = "Y";
                            }
                            else
                            {
                                IsNatural = "N";
                            }
                        }

                        if (h.FedStationNumber != null)
                        {
                            if (h.FedStationNumber != "")
                            {
                                FedStaName = h.FedStationNumber.Trim();
                            }
                        }

                        if (h.QuebecStationNumber != null)
                        {
                            if (h.QuebecStationNumber != "")
                            {
                                QCStatName = h.QuebecStationNumber.Trim();
                            }
                        }

                        sb.AppendLine(@"	<Placemark>");
                        //sb.AppendLine(string.Format(@"		<name>{0} F[{1}] Q[{2}] DA[{3}] N[{4}] R[{5}] RHBN[{6}]</name>", (h.HydrometricStationName.Length > 20 ? (h.HydrometricStationName.Substring(0, 17) + "...") : h.HydrometricStationName), FedStaName, QCStatName, DrainageArea, IsNatural, HasRealTime, IsRHBN));
                        sb.AppendLine(string.Format(@"		<name>{0}</name>", h.TimeOffset_hour, (h.HydrometricStationName.Length > 20 ? (h.HydrometricStationName.Substring(0, 17) + "...") : h.HydrometricStationName)));
                        //sb.AppendLine(string.Format(@"<description><![CDATA[{0}<br />Start Date [{1:d}]<br />End Date [{2:d}]<br /><iframe src=""about:"" width=""500"" height=""1"" />]]></description>", h.HydrometricStationName, h.StartDate, h.EndDate));
                        sb.AppendLine(@"		<Point>");
                        sb.AppendLine(string.Format(@"			<coordinates>{0},{1},0</coordinates>", h.Longitude, h.Latitude));
                        sb.AppendLine(@"		</Point>");
                        sb.AppendLine(@"	</Placemark>");


                    }
                    sb.AppendLine(@"</Folder>");
                }
                sb.AppendLine(@"</Folder>");
            }

            sb.AppendLine(@"</Document>");
            sb.AppendLine(@"</kml>");

            richTextBoxDataTool.Text = sb.ToString();

            richTextBoxDataTool.SaveFile(@"c:\CSSP\Hydrometric.kml", RichTextBoxStreamType.PlainText);

        }
        private bool DoesAllDocsExist(string url, List<string> typeOfInfoList)
        {
            DownloadCompleted = false;
            webBrowserDataTool.Navigate(url);

            while (!DownloadCompleted)
            {
                Application.DoEvents();
            }

            int countMatch = 0;
            foreach (HtmlElement he in webBrowserDataTool.Document.Links)
            {
                foreach (string s in typeOfInfoList)
                {
                    if (he.InnerText.Contains(s))
                    {
                        countMatch += 1;
                        continue;
                    }
                }
            }

            if (countMatch == typeOfInfoList.Count)
            {
                return true; // all docs exist
            }

            return false;
        }
        private XDocument GetAndParseURLEndingWithBZ2(string url, string fileName)
        {
            WebClient client = new WebClient();

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            byte[] data;
            try
            {
                data = client.DownloadData(url);
            }
            catch (Exception)
            {
                return new XDocument();
            }

            FileStream fs = new FileStream(@"c:\cssp\" + fileName, FileMode.Create);

            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();

            // starting 7z.exe application
            Process process7z = new Process();
            ProcessStartInfo pInfo7z = new ProcessStartInfo();
            pInfo7z.Arguments = @" x " + @"c:\cssp\" + fileName + @" -oc:\cssp *.xml ";
            pInfo7z.WindowStyle = ProcessWindowStyle.Hidden;
            pInfo7z.UseShellExecute = true;

            process7z.StartInfo = pInfo7z;
            try
            {
                pInfo7z.FileName = @"C:\Program Files\7-Zip\7z.exe";
                process7z.Start();
            }
            catch (Exception ex)
            {
                richTextBoxDataTool.Text = ex.Message + "\r\n" + ex.InnerException.Message;
                return new XDocument();
            }

            while (!process7z.HasExited)
            {
                // waiting for the process to exit
                Application.DoEvents();

            }

            FileInfo fi = new FileInfo(@"c:\cssp\" + fileName);
            fi.Delete();

            fi = new FileInfo(@"c:\cssp\" + fileName.Replace(".bz2", ""));

            if (!fi.Exists)
            {
                lblStatus.Text = "Error: decompressing file .bz2 did not produce the xml file expected";
                return new XDocument();
            }

            StreamReader sr = fi.OpenText();

            char[] str = new char[fi.Length];
            sr.Read(str, 0, (int)fi.Length);
            sr.Close();

            fi.Delete();

            string s = new string(str);

            return XDocument.Parse(s.Replace("\0", ""));

        }
        private float? GetFloat(string floatStr)
        {
            float TempData;
            if (!float.TryParse(floatStr.Trim(), out TempData))
            {
                return null;
            }
            else
            {
                return TempData;
            }
        }
        private ClimateDataStartDate GetHourClimateForecasts(int stationID, DateTime ForDate)
        {

            List<string> typeOfInfoList = new List<string>() { "APCP-SFC", "MSLP", "RELH-SFC", "TMP-SFC", "WDIR-SFC", "WIND-SFC" };

            string url = UrlToUse(typeOfInfoList, ForDate);
            if (url == "")
            {
                lblStatus.Text = "Error: could not find all the needed docs";
                return new ClimateDataStartDate();
            }

            if (currentClimateStation == null)
            {
                lblStatus.Text = "Error: currentClimateStation could not be set";
                return new ClimateDataStartDate();
            }

            string DateStr = url.Substring(url.Length - 12, 8);
            DateTime CurrentDate = new DateTime(int.Parse(DateStr.Substring(0, 4)), int.Parse(DateStr.Substring(4, 2)), int.Parse(DateStr.Substring(6, 2)));
            string HourStr = url.Substring(url.Length - 3, 2);

            // is it already in the DB

            ClimateDataStartDate climateDataStartDateInDB = (from f in cdbe.ClimateDataStartDates
                                                             where f.ClimateStationID == stationID
                                                             && f.ClimateDataDate == ForDate
                                                             && f.IsObservationData == false
                                                             && f.IsArchivedData == false
                                                             && f.IsForcastData == true
                                                             && f.ClimateDataType == 1 /* hourly  --- 0 is daily */
                                                             select f).FirstOrDefault<ClimateDataStartDate>();

            if (climateDataStartDateInDB != null)
            {
                climateDataStartDateInDB.ClimateDataValues = (from c in cdbe.ClimateDataValues
                                                              where c.ClimateDataStartDateID == climateDataStartDateInDB.ClimateDataStartDateID
                                                              select c).ToList<ClimateDataValue>();

                return climateDataStartDateInDB;
            }

            bool IsFirst = true;
            List<ClimateDataValue> climateDataValueList = new List<ClimateDataValue>();
            foreach (string s in typeOfInfoList)
            {
                string fileName = string.Format(@"{1}{2}_GEPS-NAEFS-RAW_{3}_{0}_000-384.xml.bz2", s, DateStr, HourStr, currentClimateStation.File_desc);
                string CurrentUrl = url + s + "/raw/" + fileName;

                lblStatus.Text = CurrentUrl;
                lblStatus.Refresh();

                XDocument doc = GetAndParseURLEndingWithBZ2(CurrentUrl, fileName);

                if (doc.Root == null)
                {
                    return new ClimateDataStartDate();
                }

                int forecastCount = doc.Descendants("forecast").Count();

                if (IsFirst)
                {
                    foreach (XElement xe in doc.Descendants("forecast"))
                    {
                        string dateStr = xe.Attribute("valid_time").Value;
                        if (dateStr.Length != 10)
                        {
                            return new ClimateDataStartDate();
                        }
                        ClimateDataValue cdv = new ClimateDataValue();
                        cdv.ClimateDataDateTime = new DateTime(int.Parse(dateStr.Substring(0, 4)), int.Parse(dateStr.Substring(4, 2)), int.Parse(dateStr.Substring(6, 2)), int.Parse(dateStr.Substring(8, 2)), 0, 0);
                        climateDataValueList.Add(cdv);
                    }
                    IsFirst = false;
                }

                switch (doc.Descendants("forecast_element").First().Attribute("title_english").Value)
                {
                    case "Surface Accumulated Precipitation":
                        {
                            int count = 0;
                            foreach (XElement xe in doc.Descendants("forecast"))
                            {
                                float total = 0.0f;
                                int cc = 0;
                                foreach (XElement xeVal in xe.Elements())
                                {
                                    float TempVal;
                                    if (!float.TryParse(xeVal.Value, out TempVal))
                                    {
                                        return new ClimateDataStartDate();
                                    }
                                    total += TempVal;
                                    cc += 1;
                                }
                                climateDataValueList[count].Rainfall_mm = total / cc;
                                count += 1;
                            }
                        }
                        break;
                    case "Mean Sea level Pressure":
                        {
                            int count = 0;
                            foreach (XElement xe in doc.Descendants("forecast"))
                            {
                                float total = 0.0f;
                                int cc = 0;
                                foreach (XElement xeVal in xe.Elements())
                                {
                                    float TempVal;
                                    if (!float.TryParse(xeVal.Value, out TempVal))
                                    {
                                        return new ClimateDataStartDate();
                                    }
                                    total += TempVal;
                                    cc += 1;
                                }
                                climateDataValueList[count].AtmPressure_kpa = total / cc / 10.0f;
                                count += 1;
                            }
                        }
                        break;
                    case "Surface Relative Humidity":
                        {
                            int count = 0;
                            foreach (XElement xe in doc.Descendants("forecast"))
                            {
                                float total = 0.0f;
                                int cc = 0;
                                foreach (XElement xeVal in xe.Elements())
                                {
                                    float TempVal;
                                    if (!float.TryParse(xeVal.Value, out TempVal))
                                    {
                                        return new ClimateDataStartDate();
                                    }
                                    total += TempVal;
                                    cc += 1;
                                }
                                climateDataValueList[count].RelativeHumidity_Perc = total / cc;
                                count += 1;
                            }
                        }
                        break;
                    case "Surface Air Temperature":
                        {
                            int count = 0;
                            foreach (XElement xe in doc.Descendants("forecast"))
                            {
                                float total = 0.0f;
                                int cc = 0;
                                foreach (XElement xeVal in xe.Elements())
                                {
                                    float TempVal;
                                    if (!float.TryParse(xeVal.Value, out TempVal))
                                    {
                                        return new ClimateDataStartDate();
                                    }
                                    total += TempVal;
                                    cc += 1;
                                }
                                climateDataValueList[count].Temperature_C = total / cc;
                                count += 1;
                            }
                        }
                        break;
                    case "Surface Wind Direction":
                        {
                            int count = 0;
                            foreach (XElement xe in doc.Descendants("forecast"))
                            {
                                float TotalX = 0.0f;
                                float TotalY = 0.0f;
                                foreach (XElement xeVal in xe.Elements())
                                {
                                    float TempVal;
                                    if (!float.TryParse(xeVal.Value, out TempVal))
                                    {
                                        return new ClimateDataStartDate();
                                    }
                                    TotalX += (float)Math.Cos((double)(TempVal * Math.PI / 180));
                                    TotalY += (float)Math.Sin((double)(TempVal * Math.PI / 180));
                                }
                                float WindDir = (float)(Math.Atan2(TotalX, TotalY) * 180.0D / Math.PI);
                                climateDataValueList[count].WindDirection_0North = WindDir;
                                if (climateDataValueList[count].WindDirection_0North < 0)
                                {
                                    climateDataValueList[count].WindDirection_0North = 360 + WindDir;
                                }
                                count += 1;
                            }
                        }
                        break;
                    case "Surface Wind Speed":
                        {
                            int count = 0;
                            foreach (XElement xe in doc.Descendants("forecast"))
                            {
                                float total = 0.0f;
                                int cc = 0;
                                foreach (XElement xeVal in xe.Elements())
                                {
                                    float TempVal;
                                    if (!float.TryParse(xeVal.Value, out TempVal))
                                    {
                                        return new ClimateDataStartDate();
                                    }
                                    total += TempVal;
                                    cc += 1;
                                }
                                climateDataValueList[count].WindSpeed_kph = total / cc;
                                count += 1;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            double OldCum = 0.0D;
            double OldCumRain = 0.0D;
            double OldCumSnow = 0.0D;
            foreach (ClimateDataValue cdv in climateDataValueList)
            {
                double TempVal = (double)cdv.Rainfall_mm;

                if (cdv.Temperature_C < 0)
                {
                    cdv.Snow_cm = (double)(cdv.Rainfall_mm - OldCum) + OldCumSnow;
                    OldCumSnow = (double)cdv.Snow_cm;
                    cdv.Rainfall_mm = OldCumRain;
                }
                else
                {
                    cdv.Rainfall_mm = (double)(cdv.Rainfall_mm - OldCum) + OldCumRain;
                    OldCumRain = (double)cdv.Rainfall_mm;
                    cdv.Snow_cm = OldCumSnow;
                }

                OldCum = TempVal;

            }

            float OriginalSnow = 0.0f;
            float OriginalRain = 0.0f;

            List<ClimateDataValue> climateDataValueAll = new List<ClimateDataValue>();

            foreach (ClimateDataValue cdvOrig in climateDataValueList)
            {
                for (int i = 0; i < 6; i++)
                {
                    ClimateDataValue cdv = new ClimateDataValue();
                    cdv.ClimateDataValueID = cdvOrig.ClimateDataValueID;
                    cdv.ClimateDataStartDateID = cdvOrig.ClimateDataStartDateID;
                    cdv.ClimateDataDateTime = cdvOrig.ClimateDataDateTime.AddHours(i + (int)(currentClimateStation.TimeOffset_hour) - 6);
                    cdv.Temperature_C = cdvOrig.Temperature_C;
                    cdv.RelativeHumidity_Perc = cdvOrig.RelativeHumidity_Perc;
                    cdv.WindSpeed_kph = cdvOrig.WindSpeed_kph;
                    cdv.WindDirection_0North = cdvOrig.WindDirection_0North;
                    cdv.AtmPressure_kpa = cdvOrig.AtmPressure_kpa;
                    cdv.Snow_cm = (cdvOrig.Snow_cm - OriginalSnow) / 6;
                    cdv.Rainfall_mm = (cdvOrig.Rainfall_mm - OriginalRain) / 6;

                    climateDataValueAll.Add(cdv);
                }
                OriginalSnow = (float)cdvOrig.Snow_cm;
                OriginalRain = (float)cdvOrig.Rainfall_mm;
            }

            List<DateTime> uniqueDateList = (from c in climateDataValueAll
                                             select new DateTime(c.ClimateDataDateTime.Year, c.ClimateDataDateTime.Month, c.ClimateDataDateTime.Day)).Distinct().ToList<DateTime>();

            foreach (DateTime dt in uniqueDateList)
            {
                List<ClimateDataValue> climateDataValueForDate = (from c in climateDataValueAll
                                                                  where c.ClimateDataDateTime.Year == dt.Year
                                                                  && c.ClimateDataDateTime.Month == dt.Month
                                                                  && c.ClimateDataDateTime.Day == dt.Day
                                                                  select c).ToList<ClimateDataValue>();

                if (climateDataValueForDate.Count == 24)
                {
                    // check if already in DB and not 

                    ClimateDataStartDate climateDataStartDate = (from c in cdbe.ClimateDataStartDates
                                                                 where c.ClimateDataDate == dt
                                                                 && c.FromForcastDate == CurrentDate
                                                                 && c.IsForcastData == true
                                                                 && c.IsObservationData == false
                                                                 && c.IsArchivedData == false
                                                                 && c.ClimateDataType == 1 /* hourly  --- 0 is daily */
                                                                 && c.ClimateStationID == stationID
                                                                 select c).FirstOrDefault<ClimateDataStartDate>();

                    if (climateDataStartDate != null)
                    {
                        // already in DB
                        continue;
                    }

                    // not in DB
                    climateDataStartDate = new ClimateDataStartDate();
                    climateDataStartDate.ClimateStationID = currentClimateStation.ClimateStationID;
                    climateDataStartDate.ClimateDataDate = dt;
                    climateDataStartDate.FromForcastDate = CurrentDate;
                    climateDataStartDate.ClimateDataType = 1; // hourly  --- 0 is daily
                    climateDataStartDate.IsForcastData = true;
                    climateDataStartDate.IsObservationData = false;
                    climateDataStartDate.IsArchivedData = false;

                    foreach (ClimateDataValue cdv in climateDataValueForDate)
                    {
                        climateDataStartDate.ClimateDataValues.Add(cdv);
                    }

                    try
                    {
                        cdbe.ClimateDataStartDates.Add(climateDataStartDate);
                        cdbe.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Error: Could not save climateDataStartDate in DB" + ex.Message;
                        return new ClimateDataStartDate();
                    }
                }
            }

            // getting the newly saved forecast values

            climateDataStartDateInDB = (from f in cdbe.ClimateDataStartDates
                                        where f.ClimateStationID == stationID
                                        && f.ClimateDataDate == ForDate
                                        select f).FirstOrDefault<ClimateDataStartDate>();

            if (climateDataStartDateInDB != null)
            {
                climateDataStartDateInDB.ClimateDataValues = (from c in cdbe.ClimateDataValues
                                                              where c.ClimateDataStartDateID == climateDataStartDateInDB.ClimateDataStartDateID
                                                              select c).ToList<ClimateDataValue>();

                return climateDataStartDateInDB;
            }

            // should have return the proper ClimateDataStartDate before this point
            // return empty obj
            return new ClimateDataStartDate();

        }
        private ClimateDataStartDate GetHourArchivedClimateData(int StationID, DateTime ForDate)
        {
            ClimateStation climateStation = (from c in cdbe.ClimateStations where c.ClimateStationID == StationID select c).FirstOrDefault<ClimateStation>();

            if (climateStation == null)
            {
                lblStatus.Text = "Could not find climate station with ClimateStationID = [" + StationID + "]";
                return new ClimateDataStartDate();
            }

            ClimateDataStartDate climateDataStartDate = (from c in cdbe.ClimateDataStartDates
                                                         where c.ClimateStationID == StationID
                                                         && c.IsArchivedData == true
                                                         && c.ClimateDataType == 1 /* hourly  --- 0 is daily */
                                                         && c.ClimateDataDate == ForDate
                                                         select c).FirstOrDefault<ClimateDataStartDate>();

            if (climateDataStartDate != null)
            {
                return climateDataStartDate;
            }

            climateDataStartDate = new ClimateDataStartDate();
            climateDataStartDate.ClimateStationID = StationID;
            climateDataStartDate.ClimateDataDate = ForDate;
            climateDataStartDate.ClimateDataType = 1; /* hourly  --- 0 is daily */
            climateDataStartDate.IsArchivedData = true;

            // daily climate page
            string url = string.Format("http://www.climate.weatheroffice.gc.ca/climateData/hourlydata_e.html?StationID={0}&timeframe=1&Year={1}&Month={2}&Day={3}", climateStation.ECDBID, ForDate.Year, ForDate.Month, ForDate.Day);

            DownloadCompleted = false;
            webBrowserDataTool.Navigate(url);

            while (!DownloadCompleted)
            {
                Application.DoEvents();
            }

            if (webBrowserDataTool.Document.GetElementById("dynamicDataTable").Children[0].Children.Count != 3)
            {
                richTextBoxDataTool.Text = "No data available for this date";
                return new ClimateDataStartDate();
            }

            HtmlElement htmlTableBody = webBrowserDataTool.Document.GetElementById("dynamicDataTable").Children[0].Children[2];

            int count = 0;
            foreach (HtmlElement htmlTr in htmlTableBody.Children)
            {
                int tdCount = 0;
                ClimateDataValue cdv = new ClimateDataValue();
                foreach (HtmlElement htmlTd in htmlTr.Children)
                {
                    switch (tdCount)
                    {
                        case 0:
                            {
                                int Hour = 0;
                                int.TryParse(htmlTd.InnerText.Trim().Substring(0, 2), out Hour);
                                if (Hour != count)
                                {
                                    return new ClimateDataStartDate();
                                }
                                cdv.ClimateDataDateTime = new DateTime(ForDate.Year, ForDate.Month, ForDate.Day, Hour, 0, 0);
                                break;
                            }
                        case 1:
                            {
                                cdv.Temperature_C = GetFloat(htmlTd.InnerText);
                                break;
                            }
                        case 2:
                            {
                                // nothing for now
                                // this would be the Dew Point Temperature
                                break;
                            }
                        case 3:
                            {
                                cdv.RelativeHumidity_Perc = GetFloat(htmlTd.InnerText);
                                break;
                            }
                        case 4:
                            {
                                cdv.WindDirection_0North = GetFloat(htmlTd.InnerText);
                                if (cdv.WindDirection_0North != null)
                                {
                                    cdv.WindDirection_0North *= 10;
                                }
                                break;
                            }
                        case 5:
                            {
                                cdv.WindSpeed_kph = GetFloat(htmlTd.InnerText);
                                break;
                            }
                        case 7:
                            {
                                cdv.AtmPressure_kpa = GetFloat(htmlTd.InnerText);
                                break;
                            }
                        default:
                            break;
                    }
                    tdCount += 1;
                }

                climateDataStartDate.ClimateDataValues.Add(cdv);
                count += 1;
            }

            try
            {
                cdbe.ClimateDataStartDates.Add(climateDataStartDate);
                cdbe.SaveChanges();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error while trying to create a new ClimateDataStartDate in the DB " + ex.Message + ex.InnerException.Message;
                return new ClimateDataStartDate();
            }

            return climateDataStartDate;
        }
        private List<string> GetLastUrls(string url, int numberOfUrl)
        {
            DownloadCompleted = false;
            webBrowserDataTool.Navigate(url);

            while (!DownloadCompleted)
            {
                Application.DoEvents();
            }

            if (webBrowserDataTool.Document.Links.Count < numberOfUrl)
            {
                return new List<string>();
            }

            List<string> LastLinks = new List<string>();
            for (int i = 0; i < numberOfUrl; i++)
            {
                LastLinks.Add(webBrowserDataTool.Document.Links[webBrowserDataTool.Document.Links.Count - i - 1].GetAttribute("href"));
            }

            return LastLinks;
        }
        private ClimateDataStartDate GetHourClimateObservations(int dstationID, DateTime ForDate)
        {
            ClimateStation station = (from s in cdbe.ClimateStations
                                      where s.ClimateStationID == currentClimateStation.ClimateStationID
                                      select s).FirstOrDefault<ClimateStation>();

            if (station == null)
            {
                return new ClimateDataStartDate();
            }

            WebClient client = new WebClient();

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            ClimateDataStartDate climateDataStartDateInDB = (from o in cdbe.ClimateDataStartDates
                                                             where o.ClimateStationID == currentClimateStation.ClimateStationID
                                                             && o.ClimateDataDate == ForDate
                                                             && o.ClimateDataType == 1 /* hourly  --- 0 is daily */
                                                             && o.IsObservationData == true
                                                             && o.IsForcastData == false
                                                             && o.IsArchivedData == false
                                                             select o).FirstOrDefault<ClimateDataStartDate>();

            if (climateDataStartDateInDB != null)
            {
                ClimateDataStartDate climateDataStartDateToRet = new ClimateDataStartDate();
                climateDataStartDateToRet = climateDataStartDateInDB;
                climateDataStartDateToRet.ClimateDataValues = (from c in cdbe.ClimateDataValues
                                                               where c.ClimateDataStartDateID == climateDataStartDateInDB.ClimateDataStartDateID
                                                               orderby c.ClimateDataDateTime
                                                               select c).ToList<ClimateDataValue>();
                return climateDataStartDateToRet;
            }

            climateDataStartDateInDB = null;

            List<ClimateDataValue> climateDataValueList = new List<ClimateDataValue>();

            // need to get the day before because of zoolo time
            for (int AddDay = -1; AddDay < 1; AddDay++)
            {
                List<string> UrlList = new List<string>();

                string StartUrl = string.Format(@"http://dd.weatheroffice.ec.gc.ca/observations/swob-ml/{1}{2}{3}/C{0}/", currentClimateStation.TCID, ForDate.Year, string.Format("{0:00}", ForDate.Month), string.Format("{0:00}", ForDate.Day + AddDay));
                DownloadCompleted = false;
                webBrowserDataTool.Navigate(StartUrl);

                while (!DownloadCompleted)
                {
                    Application.DoEvents();
                }

                foreach (HtmlElement ele in webBrowserDataTool.Document.Links)
                {
                    if (ele.GetAttribute("href").Length > StartUrl.Length + 2)
                    {
                        if (ele.GetAttribute("href").Substring(StartUrl.Length).StartsWith("2"))
                        {
                            if (ele.GetAttribute("href").Substring(StartUrl.Length).Substring(13, 2) == "00")
                            {
                                UrlList.Add(ele.GetAttribute("href").Substring(StartUrl.Length));
                            }
                        }
                    }
                }

                if (UrlList.Count == 0)
                {
                    return new ClimateDataStartDate();
                }

                //if (UrlList.Count > 24)
                //{
                //    foreach (string s in UrlList)
                //    {
                //        if (s.Substring(StartUrl.Length).Substring(21, 2) == "AU")
                //        {
                //        }
                //    }

                //}
                foreach (string url in UrlList)
                {
                    lblStatus.Text = "Reading and parsing ... " + StartUrl + url;
                    lblStatus.Refresh();

                    int year = int.Parse(url.Substring(0, 4));
                    int month = int.Parse(url.Substring(5, 2));
                    int day = int.Parse(url.Substring(8, 2));
                    int hour = int.Parse(url.Substring(11, 2));
                    int min = int.Parse(url.Substring(13, 2));

                    if (min != 0)
                    {
                        continue;
                    }

                    DateTime ObsDateZ = new DateTime(year, month, day, hour, min, 0, DateTimeKind.Utc);

                    Stream data = client.OpenRead(StartUrl + url);
                    StreamReader reader = new StreamReader(data);
                    string s = reader.ReadToEnd();
                    data.Close();
                    reader.Close();

                    XDocument doc = XDocument.Parse(s);

                    DateTime date_tm;
                    string tc_id = "";
                    int? wmo_synop_id = null;
                    string clim_id = "";
                    float? rel_hum = null;
                    float? air_temp = null;
                    float? avg_air_temp_pst1hr = null;
                    float? avg_wnd_spd_10m_pst1hr = null;
                    float? avg_wnd_spd_10m_mt50_60 = null;
                    float? avg_wnd_spd_10m_mt58_60 = null;
                    float? avg_wnd_dir_10m_pst1hr = null;
                    float? avg_wnd_dir_10m_mt50_60 = null;
                    float? avg_wnd_dir_10m_mt58_60 = null;
                    float? stn_pres = null;
                    float? pcpn_amt_pst1hr = null;
                    float? pcpn_amt_pst3hrDiv3 = null;
                    float? pcpn_amt_pst6hrDiv6 = null;
                    float? pcpn_amt_pst24hrDiv24 = null;
                    float? rnfl_amt_pst1hr = null;
                    float? rnfl_amt_pst3hrDiv3 = null;
                    float? rnfl_amt_pst6hrDiv6 = null;
                    float? rnfl_amt_pst24hrDiv24 = null;
                    float? dwpt_temp = null;

                    foreach (XElement xe in doc.Descendants().First(c => c.Name.LocalName == "identification-elements").Elements())
                    {
                        switch (xe.Attribute("name").Value.ToString())
                        {
                            case "date_tm":
                                {
                                    DateTime Tempdate_tm;
                                    if (DateTime.TryParse(xe.Attribute("value").Value, out Tempdate_tm))
                                    {
                                        date_tm = Tempdate_tm.ToUniversalTime();
                                    }
                                }
                                break;
                            case "tc_id":
                                {
                                    tc_id = xe.Attribute("value").Value;
                                }
                                break;
                            case "wmo_synop_id":
                                {
                                    int Tempwmo_synop_id;
                                    if (int.TryParse(xe.Attribute("value").Value, out Tempwmo_synop_id))
                                    {
                                        wmo_synop_id = Tempwmo_synop_id;
                                    }
                                }
                                break;
                            case "clim_id":
                                {
                                    clim_id = xe.Attribute("value").Value;
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    foreach (XElement xe in doc.Descendants().First(c => c.Name.LocalName == "elements").Elements())
                    {
                        switch (xe.Attribute("name").Value.ToString())
                        {
                            case "rel_hum":
                                {
                                    float Temprel_hum;
                                    if (float.TryParse(xe.Attribute("value").Value, out Temprel_hum))
                                    {
                                        rel_hum = Temprel_hum;
                                    }
                                }
                                break;
                            case "air_temp":
                                {
                                    float Tempair_temp;
                                    if (float.TryParse(xe.Attribute("value").Value, out Tempair_temp))
                                    {
                                        air_temp = Tempair_temp;
                                    }
                                }
                                break;
                            case "avg_air_temp_pst1hr":
                                {
                                    float Tempavg_air_temp_pst1hr;
                                    if (float.TryParse(xe.Attribute("value").Value, out Tempavg_air_temp_pst1hr))
                                    {
                                        avg_air_temp_pst1hr = Tempavg_air_temp_pst1hr;
                                    }
                                }
                                break;
                            case "avg_wnd_spd_10m_pst1hr":
                                {
                                    float Tempavg_wnd_spd_10m_pst1hr;
                                    if (float.TryParse(xe.Attribute("value").Value, out Tempavg_wnd_spd_10m_pst1hr))
                                    {
                                        avg_wnd_spd_10m_pst1hr = Tempavg_wnd_spd_10m_pst1hr;
                                    }
                                }
                                break;
                            case "avg_wnd_spd_10m_mt50-60":
                                {
                                    float Tempavg_wnd_spd_10m_mt50_60;
                                    if (float.TryParse(xe.Attribute("value").Value, out Tempavg_wnd_spd_10m_mt50_60))
                                    {
                                        avg_wnd_spd_10m_mt50_60 = Tempavg_wnd_spd_10m_mt50_60;
                                    }
                                }
                                break;
                            case "avg_wnd_spd_10m_mt58-60":
                                {
                                    float Tempavg_wnd_spd_10m_mt58_60;
                                    if (float.TryParse(xe.Attribute("value").Value, out Tempavg_wnd_spd_10m_mt58_60))
                                    {
                                        avg_wnd_spd_10m_mt58_60 = Tempavg_wnd_spd_10m_mt58_60;
                                    }
                                }
                                break;
                            case "avg_wnd_dir_10m_pst1hr":
                                {
                                    float Tempavg_wnd_dir_10m_pst1hr;
                                    if (float.TryParse(xe.Attribute("value").Value, out Tempavg_wnd_dir_10m_pst1hr))
                                    {
                                        avg_wnd_dir_10m_pst1hr = Tempavg_wnd_dir_10m_pst1hr;
                                    }
                                }
                                break;
                            case "avg_wnd_dir_10m_mt50-60":
                                {
                                    float Tempavg_wnd_dir_10m_mt50_60;
                                    if (float.TryParse(xe.Attribute("value").Value, out Tempavg_wnd_dir_10m_mt50_60))
                                    {
                                        avg_wnd_dir_10m_mt50_60 = Tempavg_wnd_dir_10m_mt50_60;
                                    }
                                }
                                break;
                            case "avg_wnd_dir_10m_mt58-60":
                                {
                                    float Tempavg_wnd_dir_10m_mt58_60;
                                    if (float.TryParse(xe.Attribute("value").Value, out Tempavg_wnd_dir_10m_mt58_60))
                                    {
                                        avg_wnd_dir_10m_mt58_60 = Tempavg_wnd_dir_10m_mt58_60;
                                    }
                                }
                                break;
                            case "stn_pres":
                                {
                                    float Tempstn_pres;
                                    if (float.TryParse(xe.Attribute("value").Value, out Tempstn_pres))
                                    {
                                        stn_pres = Tempstn_pres;
                                    }
                                }
                                break;
                            case "pcpn_amt_pst1hr":
                                {
                                    float Temppcpn_amt_pst1hr;
                                    if (float.TryParse(xe.Attribute("value").Value, out Temppcpn_amt_pst1hr))
                                    {
                                        pcpn_amt_pst1hr = Temppcpn_amt_pst1hr;
                                    }
                                }
                                break;
                            case "pcpn_amt_pst3hr":
                                {
                                    float Temppcpn_amt_pst3hr;
                                    if (float.TryParse(xe.Attribute("value").Value, out Temppcpn_amt_pst3hr))
                                    {
                                        pcpn_amt_pst3hrDiv3 = Temppcpn_amt_pst3hr / 3;
                                    }
                                }
                                break;
                            case "pcpn_amt_pst6hr":
                                {
                                    float Temppcpn_amt_pst6hr;
                                    if (float.TryParse(xe.Attribute("value").Value, out Temppcpn_amt_pst6hr))
                                    {
                                        pcpn_amt_pst6hrDiv6 = Temppcpn_amt_pst6hr / 6;
                                    }
                                }
                                break;
                            case "pcpn_amt_pst24hr":
                                {
                                    float Temppcpn_amt_pst24hr;
                                    if (float.TryParse(xe.Attribute("value").Value, out Temppcpn_amt_pst24hr))
                                    {
                                        pcpn_amt_pst24hrDiv24 = Temppcpn_amt_pst24hr / 24;
                                    }
                                }
                                break;
                            case "rnfl_amt_pst1hr":
                                {
                                    float Temprnfl_amt_pst1hr;
                                    if (float.TryParse(xe.Attribute("value").Value, out Temprnfl_amt_pst1hr))
                                    {
                                        rnfl_amt_pst1hr = Temprnfl_amt_pst1hr;
                                    }
                                }
                                break;
                            case "rnfl_amt_pst3hr":
                                {
                                    float Temprnfl_amt_pst3hr;
                                    if (float.TryParse(xe.Attribute("value").Value, out Temprnfl_amt_pst3hr))
                                    {
                                        rnfl_amt_pst3hrDiv3 = Temprnfl_amt_pst3hr / 3;
                                    }
                                }
                                break;
                            case "rnfl_amt_pst6hr":
                                {
                                    float Temprnfl_amt_pst6hr;
                                    if (float.TryParse(xe.Attribute("value").Value, out Temprnfl_amt_pst6hr))
                                    {
                                        rnfl_amt_pst6hrDiv6 = Temprnfl_amt_pst6hr / 6;
                                    }
                                }
                                break;
                            case "rnfl_amt_pst24hr":
                                {
                                    float Temprnfl_amt_pst24hr;
                                    if (float.TryParse(xe.Attribute("value").Value, out Temprnfl_amt_pst24hr))
                                    {
                                        rnfl_amt_pst24hrDiv24 = Temprnfl_amt_pst24hr / 24;
                                    }
                                }
                                break;
                            case "dwpt_temp":
                                {
                                    float Tempdwpt_temp;
                                    if (float.TryParse(xe.Attribute("value").Value, out Tempdwpt_temp))
                                    {
                                        dwpt_temp = Tempdwpt_temp;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    ClimateDataValue climateDataValue = new ClimateDataValue();
                    climateDataValue.ClimateDataDateTime = ObsDateZ;
                    climateDataValue.Temperature_C = avg_air_temp_pst1hr ?? air_temp;
                    climateDataValue.DewPointTemp_C = dwpt_temp;
                    climateDataValue.RelativeHumidity_Perc = rel_hum;
                    climateDataValue.WindSpeed_kph = avg_wnd_spd_10m_pst1hr ?? avg_wnd_spd_10m_mt50_60 ?? avg_wnd_spd_10m_mt58_60;
                    climateDataValue.WindDirection_0North = avg_wnd_dir_10m_pst1hr ?? avg_wnd_dir_10m_mt50_60 ?? avg_wnd_dir_10m_mt58_60;
                    climateDataValue.AtmPressure_kpa = stn_pres / 10.0f;
                    float? totalPrecip = pcpn_amt_pst1hr ?? pcpn_amt_pst3hrDiv3 ?? pcpn_amt_pst6hrDiv6 ?? pcpn_amt_pst24hrDiv24;
                    float? rainfall = rnfl_amt_pst1hr ?? rnfl_amt_pst3hrDiv3 ?? rnfl_amt_pst6hrDiv6 ?? rnfl_amt_pst24hrDiv24;

                    if (totalPrecip != null)
                    {
                        if (climateDataValue.Temperature_C > 0)
                        {
                            if (climateDataValue.Rainfall_mm == null)
                            {
                                climateDataValue.Rainfall_mm = totalPrecip;
                                climateDataValue.Snow_cm = 0.0f;
                            }
                        }
                        else
                        {
                            if (rainfall != null)
                            {
                                climateDataValue.Rainfall_mm = rainfall;
                                climateDataValue.Snow_cm = totalPrecip - rainfall;
                            }
                            else
                            {
                                climateDataValue.Rainfall_mm = 0.0f;
                                climateDataValue.Snow_cm = totalPrecip;
                            }
                        }
                    }
                    else
                    {
                        if (climateDataValue.Temperature_C > 0)
                        {
                            if (rainfall != null)
                            {
                                climateDataValue.Rainfall_mm = rainfall;
                                climateDataValue.Snow_cm = 0.0f;
                            }
                            else
                            {
                                climateDataValue.Rainfall_mm = null;
                                climateDataValue.Snow_cm = null;
                            }
                        }
                        else
                        {
                            if (rainfall != null)
                            {
                                climateDataValue.Rainfall_mm = 0.0f;
                                climateDataValue.Snow_cm = rainfall; // not sure?? but temperature is < 0
                            }
                            else
                            {
                                climateDataValue.Rainfall_mm = null;
                                climateDataValue.Snow_cm = null;
                            }
                        }
                    }

                    climateDataValueList.Add(climateDataValue);
                }
            }

            foreach (ClimateDataValue cdv in climateDataValueList)
            {
                cdv.ClimateDataDateTime = cdv.ClimateDataDateTime.AddHours((double)currentClimateStation.TimeOffset_hour);
            }

            List<ClimateDataValue> climateDataValueForDay = (from c in climateDataValueList
                                                             where c.ClimateDataDateTime.Year == ForDate.Year
                                                             && c.ClimateDataDateTime.Month == ForDate.Month
                                                             && c.ClimateDataDateTime.Day == ForDate.Day
                                                             orderby c.ClimateDataDateTime
                                                             select c).ToList<ClimateDataValue>();


            // does it already have some information in the DB
            ClimateDataStartDate climateDataStartDateInDB2 = (from c in cdbe.ClimateDataStartDates
                                                              where c.ClimateDataDate.Year == ForDate.Year
                                                              && c.ClimateDataDate.Month == ForDate.Month
                                                              && c.ClimateDataDate.Day == ForDate.Day
                                                              select c).FirstOrDefault<ClimateDataStartDate>();

            if (climateDataStartDateInDB2 != null)
            {
                climateDataStartDateInDB2.IsObservationData = true;
                if (climateDataValueForDay.Count == 24)
                {
                    climateDataStartDateInDB2.IsForcastData = false;
                }

                try
                {
                    cdbe.SaveChanges();
                }
                catch (Exception ex)
                {
                    return new ClimateDataStartDate();
                }

                foreach (ClimateDataValue cdv in climateDataValueForDay)
                {
                    ClimateDataValue climateDataValueInDB = (from c in cdbe.ClimateDataValues
                                                             where c.ClimateDataStartDateID == climateDataStartDateInDB2.ClimateDataStartDateID
                                                             && c.ClimateDataDateTime.Hour == cdv.ClimateDataDateTime.Hour
                                                             select c).FirstOrDefault<ClimateDataValue>();

                    if (climateDataValueInDB == null)
                    {
                        climateDataStartDateInDB2.ClimateDataValues.Add(cdv);
                    }
                    else
                    {
                        cdbe.ClimateDataValues.Remove(climateDataValueInDB);
                        try
                        {
                            cdbe.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            lblStatus.Text = "Error while removing climatDataValueInDB" + ex.Message + " - " + ex.InnerException.Message;
                            return new ClimateDataStartDate();
                        }
                        climateDataStartDateInDB2.ClimateDataValues.Add(cdv);
                        try
                        {
                            cdbe.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            return new ClimateDataStartDate();
                        }
                    }
                }

            }
            else
            {
                ClimateDataStartDate climateDataStartDate = new ClimateDataStartDate();
                climateDataStartDate.ClimateStationID = station.ClimateStationID;
                climateDataStartDate.ClimateDataDate = new DateTime(ForDate.Year, ForDate.Month, ForDate.Day);
                climateDataStartDate.ClimateDataType = 1; // hourly  --- 0 is daily
                climateDataStartDate.IsObservationData = true;
                climateDataStartDate.IsArchivedData = false;
                climateDataStartDate.FromForcastDate = null;

                foreach (ClimateDataValue cdv in climateDataValueForDay)
                {
                    climateDataStartDate.ClimateDataValues.Add(cdv);
                }

                climateDataStartDate.IsForcastData = false;
                if (climateDataValueForDay.Count < 24)
                {
                    climateDataStartDate.IsForcastData = true;
                }

                cdbe.ClimateDataStartDates.Add(climateDataStartDate);

            }

            try
            {
                cdbe.SaveChanges();
            }
            catch (Exception ex)
            {
                return new ClimateDataStartDate();
            }

            ClimateDataStartDate climateDataStartDateInDB3 = (from o in cdbe.ClimateDataStartDates
                                                              where o.ClimateStationID == currentClimateStation.ClimateStationID
                                                              && o.ClimateDataDate == ForDate
                                                              && o.ClimateDataType == 1 /* hourly  --- 0 is daily */
                                                              /* && o.IsObservationData == true */
                                                              select o).FirstOrDefault<ClimateDataStartDate>();

            if (climateDataStartDateInDB3 != null)
            {
                ClimateDataStartDate climateDataStartDateToRet = new ClimateDataStartDate();
                climateDataStartDateToRet = climateDataStartDateInDB3;
                climateDataStartDateToRet.ClimateDataValues = (from c in cdbe.ClimateDataValues
                                                               where c.ClimateDataStartDateID == climateDataStartDateInDB3.ClimateDataStartDateID
                                                               orderby c.ClimateDataDateTime
                                                               select c).ToList<ClimateDataValue>();
                return climateDataStartDateToRet;
            }

            return new ClimateDataStartDate();

        }
        private void ResetClimateStationComboBox()
        {
            if (radioButtonShowOnlyObservationClimateStations.Checked)
            {
                List<ClimateStationInfo> climateStationInComboBox = (from s in climateStationList
                                                                     where s.HourlyNow == true
                                                                     orderby s.ClimateStationName
                                                                     select new ClimateStationInfo
                                                                     {
                                                                         ClimateStationID = s.ClimateStationID,
                                                                         ClimateStationName = "(" + s.TCID + ") " + (s.File_desc == null ? "  " : "F ") + (s.HourlyNow == true ? "H " : "  ") + (s.DailyNow == true ? "D " : "  ") + s.ClimateStationName
                                                                     }).ToList<ClimateStationInfo>();

                comboBoxStationClimate.DataSource = climateStationInComboBox;
                comboBoxStationClimate.SelectedIndex = 0;
            }
            else if (radioButtonShowOnlyForecastClimateStations.Checked)
            {
                List<ClimateStationInfo> climateStationInComboBox = (from s in climateStationList
                                                                     where s.File_desc != null
                                                                     orderby s.ClimateStationName
                                                                     select new ClimateStationInfo
                                                                     {
                                                                         ClimateStationID = s.ClimateStationID,
                                                                         ClimateStationName = "(" + s.TCID + ") " + (s.File_desc == null ? "  " : "F ") + (s.HourlyNow == true ? "H " : "  ") + (s.DailyNow == true ? "D " : "  ") + s.ClimateStationName
                                                                     }).ToList<ClimateStationInfo>();

                comboBoxStationClimate.DataSource = climateStationInComboBox;
                comboBoxStationClimate.SelectedIndex = 0;
            }
            else
            {
                List<ClimateStationInfo> climateStationInComboBox = (from s in climateStationList
                                                                     orderby s.ClimateStationName
                                                                     select new ClimateStationInfo
                                                                     {
                                                                         ClimateStationID = s.ClimateStationID,
                                                                         ClimateStationName = "(" + s.TCID + ") " + (s.File_desc == null ? "  " : "F ") + (s.HourlyNow == true ? "H " : "  ") + (s.DailyNow == true ? "D " : "  ") + s.ClimateStationName
                                                                     }).ToList<ClimateStationInfo>();

                comboBoxStationClimate.DataSource = climateStationInComboBox;
                comboBoxStationClimate.SelectedIndex = 0;
            }

        }
        private void UpdateRatingCurve()
        {
            // make sure i'm logged on
            DownloadCompleted = false;
            string url = "http://www.wateroffice.ec.gc.ca/login_e.html";
            webBrowserDataTool.Navigate(url);

            while (!DownloadCompleted)
            {
                Application.DoEvents();
            }

            HtmlElement htmlElementLoginForm = webBrowserDataTool.Document.GetElementById("loginform");
            if (htmlElementLoginForm == null)
            {
                lblStatus.Text = "Could not find id == loginform in url [" + url + "]";
                return;
            }
            else
            {
                HtmlElement htmlElementUserName = webBrowserDataTool.Document.GetElementById("username");
                if (htmlElementUserName == null)
                {
                    HtmlElement htmlElementLogOut = htmlElementLoginForm.Children[0].Children[0];
                    if (htmlElementLogOut == null)
                    {
                        lblStatus.Text = "Could not find id == username or logout button in url [" + url + "]";
                        return;
                    }
                }
                else
                {
                    HtmlElement htmlElementPassword = webBrowserDataTool.Document.GetElementById("password");
                    if (htmlElementPassword == null)
                    {
                        lblStatus.Text = "Could not find id == password in url [" + url + "]";
                        return;
                    }
                    else
                    {
                        htmlElementUserName.SetAttribute("value", "realtime");
                        htmlElementPassword.SetAttribute("value", "hydrometric");

                        HtmlElement htmlElementSubmit = htmlElementLoginForm.Children[0].Children[2].Children[0];
                        object TheSubmitButton = htmlElementSubmit.DomElement;

                        MethodInfo clickMethod = TheSubmitButton.GetType().GetMethod("click");
                        clickMethod.Invoke(TheSubmitButton, null);

                        // the login is refreshing to the index_e.html page, waiting for that to happen
                        while (webBrowserDataTool.Url.AbsoluteUri == url)
                        {
                            Application.DoEvents();
                        }
                    }
                }
            }

            lblStatus.Text = "Is logged in";

            if (webBrowserDataTool.Document == null)
            {
                lblStatus.Text = "Error please login";
                return;
            }

            if (webBrowserDataTool.Document.Cookie == null)
            {
                lblStatus.Text = "Error please login";
                return;
            }

            WebClient client = new WebClient();

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            client.Headers.Add("Cookie", webBrowserDataTool.Document.Cookie);

            List<string> ProvList = new List<string>() { "NB", "PE", "NS", "NL", "BC", "QC" };

            foreach (string Prov in ProvList)
            {
                List<HydrometricStation> FedStationWithRatingCurvesList = (from h in cdbe.HydrometricStations where h.RealTime == true && h.Province == Prov orderby h.HydrometricStationName select h).ToList<HydrometricStation>();

                int StatCount = 0;
                foreach (HydrometricStation hySt in FedStationWithRatingCurvesList)
                {
                    StatCount += 1;
                    lblStatus.Text = "Doing province [" + hySt.Province + "] station (" + StatCount + ")  [" + (hySt.HydrometricStationName.Length > 40 ? hySt.HydrometricStationName.Substring(0, 40) : hySt.HydrometricStationName);
                    lblStatus.Refresh();
                    string s = "";
                    try
                    {
                        Stream data = client.OpenRead("http://www.wateroffice.ec.gc.ca/rating_curve/rating_curve_get_csv.php?stn=" + hySt.FedStationNumber);
                        StreamReader reader = new StreamReader(data);
                        s = reader.ReadToEnd();
                        data.Flush();
                        data.Close();
                        reader.Close();
                    }
                    catch (Exception)
                    {
                        if (hySt.HasRatingCurve == null)
                        {
                            hySt.HasRatingCurve = false;
                            try
                            {
                                cdbe.SaveChanges();
                                continue;
                            }
                            catch (Exception)
                            {
                                lblStatus.Text = "Error: Could not update HydrometricStations";
                                return;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                    richTextBoxDataTool.Text = s;
                    FileInfo fi = new FileInfo(@"c:\cssp\tempRatingCurve.txt");

                    StreamWriter sw = fi.CreateText();
                    sw.Write(s);
                    sw.Flush();
                    sw.Close();

                    StreamReader sr = fi.OpenText();

                    string LineStr = sr.ReadLine();
                    if (!LineStr.StartsWith("Real-time Data - Subject to Revision"))
                    {
                        if (hySt.HasRatingCurve == null)
                        {
                            hySt.HasRatingCurve = false;

                            try
                            {
                                cdbe.SaveChanges();
                                sr.Close();
                                continue;
                            }
                            catch (Exception)
                            {
                                lblStatus.Text = "Error: Could not update HydrometricStations";
                                sr.Close();
                                return;
                            }
                        }
                        else
                        {
                            sr.Close();
                            continue;
                        }
                    }
                    LineStr = sr.ReadLine();
                    LineStr = sr.ReadLine();
                    LineStr = sr.ReadLine();
                    string stageDisStr = "Stage/Discharge Curve for:";
                    if (!LineStr.StartsWith(stageDisStr))
                    {
                        lblStatus.Text = "Error: line (4) should start with [" + stageDisStr + "]";
                        sr.Close();
                        return;
                    }
                    string fedStaID = LineStr.Substring(stageDisStr.Length, LineStr.IndexOf(" ", stageDisStr.Length + 4) - stageDisStr.Length).Trim();
                    if (fedStaID != hySt.FedStationNumber)
                    {
                        lblStatus.Text = "Error: line (4) does not contain the correct federal station name [" + hySt.FedStationNumber + "] it's [" + fedStaID + "]";
                        sr.Close();
                        return;
                    }
                    LineStr = sr.ReadLine();
                    string ratTableStr = "Rating Table:";
                    if (!LineStr.StartsWith(ratTableStr))
                    {
                        lblStatus.Text = "Error: line (5) should start with [" + ratTableStr + "]";
                        sr.Close();
                        return;
                    }
                    string RatingTableNumber = LineStr.Substring(ratTableStr.Length, LineStr.IndexOf("Valid") - ratTableStr.Length).Trim();
                    if (RatingTableNumber == "")
                    {
                        lblStatus.Text = "Error: line (5) could not parse the Rating Table number";
                        sr.Close();
                        return;
                    }
                    string valPeriod = "Valid Period:";
                    string to = "To";
                    int startPos = 0;
                    int endPos = 0;
                    startPos = LineStr.IndexOf(valPeriod) + valPeriod.Length;
                    endPos = LineStr.IndexOf(to);
                    DateTime StartDateOfRatingCurve;
                    if (!DateTime.TryParse(LineStr.Substring(startPos, endPos - startPos), out StartDateOfRatingCurve))
                    {
                        lblStatus.Text = "Error: line (5) could not parse the Valid Period Date";
                        sr.Close();
                        return;
                    }
                    startPos = LineStr.IndexOf(to) + valPeriod.Length;
                    DateTime EndDateOfRatingCurve;
                    if (!DateTime.TryParse(LineStr.Substring(startPos), out EndDateOfRatingCurve))
                    {
                        lblStatus.Text = "Error: line (5) could not parse the Valid Period To Date";
                        sr.Close();
                        return;
                    }
                    LineStr = sr.ReadLine(); // empty line
                    LineStr = sr.ReadLine(); // reading Description of Parameters: 
                    LineStr = sr.ReadLine(); // reading Stage,Discharge
                    LineStr = sr.ReadLine(); // empty line
                    LineStr = sr.ReadLine(); // empty line

                    RatingCurve rcExist = (from r in cdbe.RatingCurves where r.HydrometricStationID == hySt.HydrometricStationID select r).FirstOrDefault<RatingCurve>();

                    if (rcExist != null)
                    {
                        if (RatingTableNumber == rcExist.RatingCurveNumber)
                        {
                            sr.Close();
                            continue;
                        }
                    }

                    // saving rating curve
                    RatingCurve rc = new RatingCurve();
                    rc.HydrometricStationID = hySt.HydrometricStationID;
                    rc.RatingCurveNumber = RatingTableNumber;

                    try
                    {
                        cdbe.RatingCurves.Add(rc);
                        cdbe.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Error: while trying to create the RatingCurve" + ex.Message;
                        sr.Close();
                        return;
                    }

                    // reading the actual number of the rating curve 
                    int count = 0;
                    while (!sr.EndOfStream)
                    {
                        Application.DoEvents();

                        count += 1;
                        lblStatus.Text = "Doing province [" + hySt.Province + "] station [" + (hySt.HydrometricStationName.Length > 40 ? hySt.HydrometricStationName.Substring(0, 40) : hySt.HydrometricStationName) + "] count [" + count + "]";
                        lblStatus.Refresh();

                        LineStr = sr.ReadLine();
                        if (LineStr.Trim() == "")
                        {
                            continue;
                        }
                        string[] ValueArr = LineStr.Split(",".ToCharArray()[0]);
                        if (ValueArr.Count() != 2)
                        {
                            lblStatus.Text = "Error: while reading the values of the rating curve";
                            sr.Close();
                            return;
                        }

                        float Stage;
                        if (!float.TryParse(ValueArr[0], out Stage))
                        {
                            lblStatus.Text = "Error: while trying to parse the ValueArr[0] value (Stage)";
                            sr.Close();
                            return;
                        }
                        float Discharge;
                        if (!float.TryParse(ValueArr[1], out Discharge))
                        {
                            lblStatus.Text = "Error: while trying to parse the ValueArr[1] (Discharge)";
                            sr.Close();
                            return;
                        }

                        RatingCurveValue rcv = new RatingCurveValue();
                        rcv.RatingCurveID = rc.RatingCurveID;
                        rcv.StageValue = Stage;
                        rcv.DischargeValue = Discharge;

                        rc.RatingCurveValues.Add(rcv);
                    }

                    try
                    {
                        hySt.HasRatingCurve = true;
                        cdbe.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Error: while trying to create the RatingCurveValue" + ex.Message;
                        sr.Close();
                        return;
                    }

                    sr.Close();
                }
            }

            lblStatus.Text = "Done ...";

        }
        private string UrlToUse(List<string> typeOfInfoList, DateTime ForDate)
        {
            string urlRet = "";

            string StartURL = "http://dd.weatheroffice.ec.gc.ca/ensemble/naefs/xml/";
            List<string> Last4UrlList = GetLastUrls(StartURL, 4);

            if (Last4UrlList.Count != 4)
            {
                lblStatus.Text = "Error: could not get the last 4 links of the url" + StartURL;
                return "";
            }

            DateTime StartDate = ForDate.AddHours((double)currentClimateStation.TimeOffset_hour);

            foreach (string u in Last4UrlList)
            {
                foreach (string s in new List<string>() { "12/", "00/" })
                {
                    string url = u + s;
                    string DateStr = url.Substring(url.Length - 12, 8);
                    DateTime CurrentDate = new DateTime(int.Parse(DateStr.Substring(0, 4)), int.Parse(DateStr.Substring(4, 2)), int.Parse(DateStr.Substring(6, 2)), int.Parse(url.Substring(url.Length - 3, 2)), 0, 0);

                    if (StartDate >= CurrentDate)
                    {
                        // check if all the documents needed are in the first link
                        bool AllDocsExist = DoesAllDocsExist(url, typeOfInfoList);

                        if (!AllDocsExist)
                        {
                            continue;
                        }

                        urlRet = url;
                        break;
                    }
                }
                if (urlRet != "")
                {
                    break;
                }
            }

            return urlRet;
        }
        #endregion Functions Private

        #region private classes
        private enum DataParam
        {
            Surface_Accumulated_Precipitation,
            Mean_Sea_level_Pressure,
            Surface_Relative_Humidity,
            Surface_Air_Temperature,
            Surface_Wind_Direction,
            Surface_Wind_Speed,
            Not_valid
        }
        private enum DataType
        {
            Hourly,
            Daily
        }
        public class ClimateHourlyData
        {
            public DateTime? dateTimeOfObs { get; set; }
            public float? Temperature { get; set; }
            public float? DewPtTemp { get; set; }
            public float? RelHum { get; set; }
            public float? WindDir { get; set; }
            public float? WindSp { get; set; }
            public float? Pressure { get; set; }
        }
        private class ClimateStationInfo
        {
            public int ClimateStationID { get; set; }
            public string ClimateStationName { get; set; }
        }
        private class HydroStationInfo
        {
            public int HydrometricStationID { get; set; }
            public string HydroStationName { get; set; }
            public bool HasRealTime { get; set; }
        }
        #endregion private classes


        private int GetID(string Path)
        {
            int RetVal = -1; // will return -1 if an error occured or if there are no parent i.e. tvItem is root
            if (!string.IsNullOrWhiteSpace(Path))
            {
                if (Path.Contains(@"\"))
                {
                    RetVal = int.Parse(Path.Substring(Path.LastIndexOf(@"\") + 1));
                }
                else
                {
                    RetVal = int.Parse(Path);
                }
            }

            return RetVal;
        }


        private class ItemAndPath
        {
            public string city { get; set; }
            public string path { get; set; }
        }

        private class RetCSSP
        {
            public int WWTPCSSPID { get; set; }
            public int InfrasID { get; set; }
            public float LatWWTP { get; set; }
            public float LongWWTP { get; set; }
            public float LatWWTPOF { get; set; }
            public float LongWWTPOF { get; set; }
        }
    }
}
