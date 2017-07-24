namespace DataTool
{
    partial class DataTool
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.webBrowserDataTool = new System.Windows.Forms.WebBrowser();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.richTextBoxDataTool = new System.Windows.Forms.RichTextBox();
            this.groupBoxHydro = new System.Windows.Forms.GroupBox();
            this.butUpdateRatingCurves = new System.Windows.Forms.Button();
            this.lblProvinceHydro = new System.Windows.Forms.Label();
            this.butCreateHydrometricStationKML = new System.Windows.Forms.Button();
            this.butGetObservationsHydro = new System.Windows.Forms.Button();
            this.labelStartDateHydro = new System.Windows.Forms.Label();
            this.comboBoxStationHydro = new System.Windows.Forms.ComboBox();
            this.lblStationHydro = new System.Windows.Forms.Label();
            this.dateTimePickerStartDateHydro = new System.Windows.Forms.DateTimePicker();
            this.comboBoxProvinceHydro = new System.Windows.Forms.ComboBox();
            this.groupBoxClimate = new System.Windows.Forms.GroupBox();
            this.radioButtonShowOnlyForecastClimateStations = new System.Windows.Forms.RadioButton();
            this.radioButtonShowOnlyObservationClimateStations = new System.Windows.Forms.RadioButton();
            this.radioButtonShowAllClimateStations = new System.Windows.Forms.RadioButton();
            this.lblProvinceClimate = new System.Windows.Forms.Label();
            this.butCreateClimateStationKML = new System.Windows.Forms.Button();
            this.butGetClimateData = new System.Windows.Forms.Button();
            this.lblStartDateClimate = new System.Windows.Forms.Label();
            this.comboBoxStationClimate = new System.Windows.Forms.ComboBox();
            this.lblStationClimate = new System.Windows.Forms.Label();
            this.dateTimePickerStartDateClimate = new System.Windows.Forms.DateTimePicker();
            this.lblMonthly = new System.Windows.Forms.Label();
            this.comboBoxProvinceClimate = new System.Windows.Forms.ComboBox();
            this.lblDaily = new System.Windows.Forms.Label();
            this.lblHourly = new System.Windows.Forms.Label();
            this.lblMonthlyVal = new System.Windows.Forms.Label();
            this.lblHourlyVal = new System.Windows.Forms.Label();
            this.lblDailyVal = new System.Windows.Forms.Label();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBoxHydro.SuspendLayout();
            this.groupBoxClimate.SuspendLayout();
            this.panelStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // webBrowserDataTool
            // 
            this.webBrowserDataTool.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowserDataTool.Location = new System.Drawing.Point(0, 0);
            this.webBrowserDataTool.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowserDataTool.Name = "webBrowserDataTool";
            this.webBrowserDataTool.Size = new System.Drawing.Size(468, 368);
            this.webBrowserDataTool.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 231);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.richTextBoxDataTool);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.webBrowserDataTool);
            this.splitContainer1.Size = new System.Drawing.Size(1092, 372);
            this.splitContainer1.SplitterDistance = 616;
            this.splitContainer1.TabIndex = 1;
            // 
            // richTextBoxDataTool
            // 
            this.richTextBoxDataTool.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxDataTool.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxDataTool.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxDataTool.Name = "richTextBoxDataTool";
            this.richTextBoxDataTool.Size = new System.Drawing.Size(612, 368);
            this.richTextBoxDataTool.TabIndex = 0;
            this.richTextBoxDataTool.Text = "";
            this.richTextBoxDataTool.WordWrap = false;
            // 
            // groupBoxHydro
            // 
            this.groupBoxHydro.Controls.Add(this.butUpdateRatingCurves);
            this.groupBoxHydro.Controls.Add(this.lblProvinceHydro);
            this.groupBoxHydro.Controls.Add(this.butCreateHydrometricStationKML);
            this.groupBoxHydro.Controls.Add(this.butGetObservationsHydro);
            this.groupBoxHydro.Controls.Add(this.labelStartDateHydro);
            this.groupBoxHydro.Controls.Add(this.comboBoxStationHydro);
            this.groupBoxHydro.Controls.Add(this.lblStationHydro);
            this.groupBoxHydro.Controls.Add(this.dateTimePickerStartDateHydro);
            this.groupBoxHydro.Controls.Add(this.comboBoxProvinceHydro);
            this.groupBoxHydro.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxHydro.Location = new System.Drawing.Point(0, 103);
            this.groupBoxHydro.Name = "groupBoxHydro";
            this.groupBoxHydro.Size = new System.Drawing.Size(1092, 100);
            this.groupBoxHydro.TabIndex = 17;
            this.groupBoxHydro.TabStop = false;
            this.groupBoxHydro.Text = "Hydrometric";
            // 
            // butUpdateRatingCurves
            // 
            this.butUpdateRatingCurves.Enabled = false;
            this.butUpdateRatingCurves.Location = new System.Drawing.Point(845, 13);
            this.butUpdateRatingCurves.Name = "butUpdateRatingCurves";
            this.butUpdateRatingCurves.Size = new System.Drawing.Size(177, 23);
            this.butUpdateRatingCurves.TabIndex = 16;
            this.butUpdateRatingCurves.Text = "Update Rating Curves";
            this.butUpdateRatingCurves.UseVisualStyleBackColor = true;
            this.butUpdateRatingCurves.Click += new System.EventHandler(this.butUpdateRatingCurves_Click);
            // 
            // lblProvinceHydro
            // 
            this.lblProvinceHydro.AutoSize = true;
            this.lblProvinceHydro.Location = new System.Drawing.Point(19, 16);
            this.lblProvinceHydro.Name = "lblProvinceHydro";
            this.lblProvinceHydro.Size = new System.Drawing.Size(49, 13);
            this.lblProvinceHydro.TabIndex = 9;
            this.lblProvinceHydro.Text = "Province";
            // 
            // butCreateHydrometricStationKML
            // 
            this.butCreateHydrometricStationKML.Location = new System.Drawing.Point(863, 61);
            this.butCreateHydrometricStationKML.Name = "butCreateHydrometricStationKML";
            this.butCreateHydrometricStationKML.Size = new System.Drawing.Size(191, 23);
            this.butCreateHydrometricStationKML.TabIndex = 3;
            this.butCreateHydrometricStationKML.Text = "Create Hydrometric Station KML";
            this.butCreateHydrometricStationKML.UseVisualStyleBackColor = true;
            this.butCreateHydrometricStationKML.Click += new System.EventHandler(this.butCreateHydrometricStationKML_Click);
            // 
            // butGetObservationsHydro
            // 
            this.butGetObservationsHydro.Location = new System.Drawing.Point(247, 45);
            this.butGetObservationsHydro.Name = "butGetObservationsHydro";
            this.butGetObservationsHydro.Size = new System.Drawing.Size(202, 23);
            this.butGetObservationsHydro.TabIndex = 5;
            this.butGetObservationsHydro.Text = "Get Hydro Observations";
            this.butGetObservationsHydro.UseVisualStyleBackColor = true;
            this.butGetObservationsHydro.Click += new System.EventHandler(this.butGetObservationsHydro_Click);
            // 
            // labelStartDateHydro
            // 
            this.labelStartDateHydro.AutoSize = true;
            this.labelStartDateHydro.Location = new System.Drawing.Point(10, 50);
            this.labelStartDateHydro.Name = "labelStartDateHydro";
            this.labelStartDateHydro.Size = new System.Drawing.Size(58, 13);
            this.labelStartDateHydro.TabIndex = 15;
            this.labelStartDateHydro.Text = "Start Date:";
            // 
            // comboBoxStationHydro
            // 
            this.comboBoxStationHydro.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxStationHydro.FormattingEnabled = true;
            this.comboBoxStationHydro.Location = new System.Drawing.Point(209, 13);
            this.comboBoxStationHydro.Name = "comboBoxStationHydro";
            this.comboBoxStationHydro.Size = new System.Drawing.Size(556, 22);
            this.comboBoxStationHydro.TabIndex = 6;
            this.comboBoxStationHydro.SelectedIndexChanged += new System.EventHandler(this.comboBoxStationHydro_SelectedIndexChanged);
            // 
            // lblStationHydro
            // 
            this.lblStationHydro.AutoSize = true;
            this.lblStationHydro.Location = new System.Drawing.Point(160, 16);
            this.lblStationHydro.Name = "lblStationHydro";
            this.lblStationHydro.Size = new System.Drawing.Size(43, 13);
            this.lblStationHydro.TabIndex = 14;
            this.lblStationHydro.Text = "Station:";
            // 
            // dateTimePickerStartDateHydro
            // 
            this.dateTimePickerStartDateHydro.CustomFormat = "";
            this.dateTimePickerStartDateHydro.Location = new System.Drawing.Point(69, 44);
            this.dateTimePickerStartDateHydro.Name = "dateTimePickerStartDateHydro";
            this.dateTimePickerStartDateHydro.Size = new System.Drawing.Size(134, 20);
            this.dateTimePickerStartDateHydro.TabIndex = 7;
            // 
            // comboBoxProvinceHydro
            // 
            this.comboBoxProvinceHydro.FormattingEnabled = true;
            this.comboBoxProvinceHydro.Location = new System.Drawing.Point(81, 15);
            this.comboBoxProvinceHydro.Name = "comboBoxProvinceHydro";
            this.comboBoxProvinceHydro.Size = new System.Drawing.Size(68, 21);
            this.comboBoxProvinceHydro.TabIndex = 8;
            this.comboBoxProvinceHydro.SelectedIndexChanged += new System.EventHandler(this.comboBoxProvinceHydro_SelectedIndexChanged);
            // 
            // groupBoxClimate
            // 
            this.groupBoxClimate.Controls.Add(this.radioButtonShowOnlyForecastClimateStations);
            this.groupBoxClimate.Controls.Add(this.radioButtonShowOnlyObservationClimateStations);
            this.groupBoxClimate.Controls.Add(this.radioButtonShowAllClimateStations);
            this.groupBoxClimate.Controls.Add(this.lblProvinceClimate);
            this.groupBoxClimate.Controls.Add(this.butCreateClimateStationKML);
            this.groupBoxClimate.Controls.Add(this.butGetClimateData);
            this.groupBoxClimate.Controls.Add(this.lblStartDateClimate);
            this.groupBoxClimate.Controls.Add(this.comboBoxStationClimate);
            this.groupBoxClimate.Controls.Add(this.lblStationClimate);
            this.groupBoxClimate.Controls.Add(this.dateTimePickerStartDateClimate);
            this.groupBoxClimate.Controls.Add(this.lblMonthly);
            this.groupBoxClimate.Controls.Add(this.comboBoxProvinceClimate);
            this.groupBoxClimate.Controls.Add(this.lblDaily);
            this.groupBoxClimate.Controls.Add(this.lblHourly);
            this.groupBoxClimate.Controls.Add(this.lblMonthlyVal);
            this.groupBoxClimate.Controls.Add(this.lblHourlyVal);
            this.groupBoxClimate.Controls.Add(this.lblDailyVal);
            this.groupBoxClimate.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxClimate.Location = new System.Drawing.Point(0, 0);
            this.groupBoxClimate.Name = "groupBoxClimate";
            this.groupBoxClimate.Size = new System.Drawing.Size(1092, 103);
            this.groupBoxClimate.TabIndex = 16;
            this.groupBoxClimate.TabStop = false;
            this.groupBoxClimate.Text = "Climate";
            // 
            // radioButtonShowOnlyForecastClimateStations
            // 
            this.radioButtonShowOnlyForecastClimateStations.AutoSize = true;
            this.radioButtonShowOnlyForecastClimateStations.Location = new System.Drawing.Point(339, 40);
            this.radioButtonShowOnlyForecastClimateStations.Name = "radioButtonShowOnlyForecastClimateStations";
            this.radioButtonShowOnlyForecastClimateStations.Size = new System.Drawing.Size(66, 17);
            this.radioButtonShowOnlyForecastClimateStations.TabIndex = 20;
            this.radioButtonShowOnlyForecastClimateStations.Text = "Forecast";
            this.radioButtonShowOnlyForecastClimateStations.UseVisualStyleBackColor = true;
            this.radioButtonShowOnlyForecastClimateStations.CheckedChanged += new System.EventHandler(this.ClimateStationsShowingChanged);
            // 
            // radioButtonShowOnlyObservationClimateStations
            // 
            this.radioButtonShowOnlyObservationClimateStations.AutoSize = true;
            this.radioButtonShowOnlyObservationClimateStations.Location = new System.Drawing.Point(289, 40);
            this.radioButtonShowOnlyObservationClimateStations.Name = "radioButtonShowOnlyObservationClimateStations";
            this.radioButtonShowOnlyObservationClimateStations.Size = new System.Drawing.Size(44, 17);
            this.radioButtonShowOnlyObservationClimateStations.TabIndex = 20;
            this.radioButtonShowOnlyObservationClimateStations.Text = "Obs";
            this.radioButtonShowOnlyObservationClimateStations.UseVisualStyleBackColor = true;
            this.radioButtonShowOnlyObservationClimateStations.CheckedChanged += new System.EventHandler(this.ClimateStationsShowingChanged);
            // 
            // radioButtonShowAllClimateStations
            // 
            this.radioButtonShowAllClimateStations.AutoSize = true;
            this.radioButtonShowAllClimateStations.Checked = true;
            this.radioButtonShowAllClimateStations.Location = new System.Drawing.Point(247, 40);
            this.radioButtonShowAllClimateStations.Name = "radioButtonShowAllClimateStations";
            this.radioButtonShowAllClimateStations.Size = new System.Drawing.Size(36, 17);
            this.radioButtonShowAllClimateStations.TabIndex = 20;
            this.radioButtonShowAllClimateStations.TabStop = true;
            this.radioButtonShowAllClimateStations.Text = "All";
            this.radioButtonShowAllClimateStations.UseVisualStyleBackColor = true;
            this.radioButtonShowAllClimateStations.CheckedChanged += new System.EventHandler(this.ClimateStationsShowingChanged);
            // 
            // lblProvinceClimate
            // 
            this.lblProvinceClimate.AutoSize = true;
            this.lblProvinceClimate.Location = new System.Drawing.Point(19, 16);
            this.lblProvinceClimate.Name = "lblProvinceClimate";
            this.lblProvinceClimate.Size = new System.Drawing.Size(49, 13);
            this.lblProvinceClimate.TabIndex = 9;
            this.lblProvinceClimate.Text = "Province";
            // 
            // butCreateClimateStationKML
            // 
            this.butCreateClimateStationKML.Location = new System.Drawing.Point(889, 37);
            this.butCreateClimateStationKML.Name = "butCreateClimateStationKML";
            this.butCreateClimateStationKML.Size = new System.Drawing.Size(191, 23);
            this.butCreateClimateStationKML.TabIndex = 3;
            this.butCreateClimateStationKML.Text = "Create Climate Station KML";
            this.butCreateClimateStationKML.UseVisualStyleBackColor = true;
            this.butCreateClimateStationKML.Click += new System.EventHandler(this.butCreateClimateStationKML_Click);
            // 
            // butGetClimateData
            // 
            this.butGetClimateData.Location = new System.Drawing.Point(127, 75);
            this.butGetClimateData.Name = "butGetClimateData";
            this.butGetClimateData.Size = new System.Drawing.Size(302, 23);
            this.butGetClimateData.TabIndex = 5;
            this.butGetClimateData.Text = "Get Climate Data For Selected Date";
            this.butGetClimateData.UseVisualStyleBackColor = true;
            this.butGetClimateData.Click += new System.EventHandler(this.butGetClimateData_Click);
            // 
            // lblStartDateClimate
            // 
            this.lblStartDateClimate.AutoSize = true;
            this.lblStartDateClimate.Location = new System.Drawing.Point(10, 50);
            this.lblStartDateClimate.Name = "lblStartDateClimate";
            this.lblStartDateClimate.Size = new System.Drawing.Size(58, 13);
            this.lblStartDateClimate.TabIndex = 15;
            this.lblStartDateClimate.Text = "Start Date:";
            // 
            // comboBoxStationClimate
            // 
            this.comboBoxStationClimate.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxStationClimate.FormattingEnabled = true;
            this.comboBoxStationClimate.Location = new System.Drawing.Point(209, 13);
            this.comboBoxStationClimate.Name = "comboBoxStationClimate";
            this.comboBoxStationClimate.Size = new System.Drawing.Size(427, 22);
            this.comboBoxStationClimate.TabIndex = 6;
            this.comboBoxStationClimate.SelectedIndexChanged += new System.EventHandler(this.comboBoxStationClimate_SelectedIndexChanged);
            // 
            // lblStationClimate
            // 
            this.lblStationClimate.AutoSize = true;
            this.lblStationClimate.Location = new System.Drawing.Point(160, 16);
            this.lblStationClimate.Name = "lblStationClimate";
            this.lblStationClimate.Size = new System.Drawing.Size(43, 13);
            this.lblStationClimate.TabIndex = 14;
            this.lblStationClimate.Text = "Station:";
            // 
            // dateTimePickerStartDateClimate
            // 
            this.dateTimePickerStartDateClimate.CustomFormat = "";
            this.dateTimePickerStartDateClimate.Location = new System.Drawing.Point(69, 44);
            this.dateTimePickerStartDateClimate.Name = "dateTimePickerStartDateClimate";
            this.dateTimePickerStartDateClimate.Size = new System.Drawing.Size(134, 20);
            this.dateTimePickerStartDateClimate.TabIndex = 7;
            // 
            // lblMonthly
            // 
            this.lblMonthly.AutoSize = true;
            this.lblMonthly.Location = new System.Drawing.Point(458, 80);
            this.lblMonthly.Name = "lblMonthly";
            this.lblMonthly.Size = new System.Drawing.Size(44, 13);
            this.lblMonthly.TabIndex = 10;
            this.lblMonthly.Text = "Monthly";
            // 
            // comboBoxProvinceClimate
            // 
            this.comboBoxProvinceClimate.FormattingEnabled = true;
            this.comboBoxProvinceClimate.Location = new System.Drawing.Point(81, 15);
            this.comboBoxProvinceClimate.Name = "comboBoxProvinceClimate";
            this.comboBoxProvinceClimate.Size = new System.Drawing.Size(68, 21);
            this.comboBoxProvinceClimate.TabIndex = 8;
            this.comboBoxProvinceClimate.SelectedIndexChanged += new System.EventHandler(this.comboBoxProvinceClimate_SelectedIndexChanged);
            // 
            // lblDaily
            // 
            this.lblDaily.AutoSize = true;
            this.lblDaily.Location = new System.Drawing.Point(472, 62);
            this.lblDaily.Name = "lblDaily";
            this.lblDaily.Size = new System.Drawing.Size(30, 13);
            this.lblDaily.TabIndex = 10;
            this.lblDaily.Text = "Daily";
            // 
            // lblHourly
            // 
            this.lblHourly.AutoSize = true;
            this.lblHourly.Location = new System.Drawing.Point(466, 41);
            this.lblHourly.Name = "lblHourly";
            this.lblHourly.Size = new System.Drawing.Size(37, 13);
            this.lblHourly.TabIndex = 10;
            this.lblHourly.Text = "Hourly";
            // 
            // lblMonthlyVal
            // 
            this.lblMonthlyVal.AutoSize = true;
            this.lblMonthlyVal.Location = new System.Drawing.Point(518, 80);
            this.lblMonthlyVal.Name = "lblMonthlyVal";
            this.lblMonthlyVal.Size = new System.Drawing.Size(42, 13);
            this.lblMonthlyVal.TabIndex = 10;
            this.lblMonthlyVal.Text = "[Empty]";
            // 
            // lblHourlyVal
            // 
            this.lblHourlyVal.AutoSize = true;
            this.lblHourlyVal.Location = new System.Drawing.Point(519, 42);
            this.lblHourlyVal.Name = "lblHourlyVal";
            this.lblHourlyVal.Size = new System.Drawing.Size(42, 13);
            this.lblHourlyVal.TabIndex = 10;
            this.lblHourlyVal.Text = "[Empty]";
            // 
            // lblDailyVal
            // 
            this.lblDailyVal.AutoSize = true;
            this.lblDailyVal.Location = new System.Drawing.Point(518, 62);
            this.lblDailyVal.Name = "lblDailyVal";
            this.lblDailyVal.Size = new System.Drawing.Size(42, 13);
            this.lblDailyVal.TabIndex = 10;
            this.lblDailyVal.Text = "[Empty]";
            // 
            // panelStatus
            // 
            this.panelStatus.Controls.Add(this.lblStatus);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelStatus.Location = new System.Drawing.Point(0, 203);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(1092, 28);
            this.panelStatus.TabIndex = 18;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = System.Drawing.Color.Red;
            this.lblStatus.Location = new System.Drawing.Point(4, 7);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(72, 13);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "[empty status]";
            // 
            // DataTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1092, 603);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(this.groupBoxHydro);
            this.Controls.Add(this.groupBoxClimate);
            this.Name = "DataTool";
            this.Text = "Rain Data Application";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.DataTool_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBoxHydro.ResumeLayout(false);
            this.groupBoxHydro.PerformLayout();
            this.groupBoxClimate.ResumeLayout(false);
            this.groupBoxClimate.PerformLayout();
            this.panelStatus.ResumeLayout(false);
            this.panelStatus.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowserDataTool;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RichTextBox richTextBoxDataTool;
        private System.Windows.Forms.Button butCreateClimateStationKML;
        private System.Windows.Forms.Button butGetClimateData;
        private System.Windows.Forms.DateTimePicker dateTimePickerStartDateClimate;
        private System.Windows.Forms.ComboBox comboBoxStationClimate;
        private System.Windows.Forms.Label lblProvinceClimate;
        private System.Windows.Forms.ComboBox comboBoxProvinceClimate;
        private System.Windows.Forms.Label lblHourly;
        private System.Windows.Forms.Label lblMonthly;
        private System.Windows.Forms.Label lblDaily;
        private System.Windows.Forms.Label lblMonthlyVal;
        private System.Windows.Forms.Label lblDailyVal;
        private System.Windows.Forms.Label lblHourlyVal;
        private System.Windows.Forms.GroupBox groupBoxHydro;
        private System.Windows.Forms.Label lblProvinceHydro;
        private System.Windows.Forms.Button butGetObservationsHydro;
        private System.Windows.Forms.Label labelStartDateHydro;
        private System.Windows.Forms.ComboBox comboBoxStationHydro;
        private System.Windows.Forms.Label lblStationHydro;
        private System.Windows.Forms.DateTimePicker dateTimePickerStartDateHydro;
        private System.Windows.Forms.ComboBox comboBoxProvinceHydro;
        private System.Windows.Forms.GroupBox groupBoxClimate;
        private System.Windows.Forms.Label lblStartDateClimate;
        private System.Windows.Forms.Label lblStationClimate;
        private System.Windows.Forms.Button butCreateHydrometricStationKML;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button butUpdateRatingCurves;
        private System.Windows.Forms.RadioButton radioButtonShowOnlyForecastClimateStations;
        private System.Windows.Forms.RadioButton radioButtonShowOnlyObservationClimateStations;
        private System.Windows.Forms.RadioButton radioButtonShowAllClimateStations;
    }
}

