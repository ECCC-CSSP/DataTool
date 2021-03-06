﻿using CSSPEnumsDLL.Enums;
using CSSPEnumsDLL.Services;
using CSSPModelsDLL.Models;
using CSSPDBDLL;
using CSSPDBDLL.Services;
using PCCSM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Spatial;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using TempData;
using CSSPModelsDLL.Services;
using OpenQA.Selenium;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading.Tasks;
using System.Net.Http;

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

            polSourceObsInfo = PolSourceObsInfoEnum.StatusPotentialMed;

            if (Prov == "AT")
            {
                switch (CODE)
                {
                    case "D":
                        {
                            polSourceObsInfo = PolSourceObsInfoEnum.StatusDefiniteMed;
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

            sw.WriteLine("textBoxBCCreateStationsBC," + textBoxBCCreateStationsBC.Text);
            sw.WriteLine("textBoxQCCreateStationsQC," + textBoxQCCreateStationsQC.Text);
            sw.WriteLine("textBoxBCCreateSamplesBC," + textBoxBCCreateSamplesBC.Text);
            sw.WriteLine("textBoxQCCreateSamplesQC," + textBoxQCCreateSamplesQC.Text);
            sw.WriteLine("textBoxBCCreateRunsBC," + textBoxBCCreateRunsBC.Text);
            sw.WriteLine("textBoxQCCreateRunsQC," + textBoxQCCreateRunsQC.Text);
            sw.WriteLine("textBoxBCCreateSanitaryBC," + textBoxBCCreateSanitaryBC.Text);
            sw.WriteLine("textBoxQCCreateSanitaryQC," + textBoxQCCreateSanitaryQC.Text);
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

        //private void button4_Click(object sender, EventArgs e)
        //{
        //    for (int i = 0, count = Enum.GetNames(typeof(TreatmentTypeEnum)).Count(); i < count; i++)
        //    {
        //        comboBox1.Items.Add(((TreatmentTypeEnum)i).ToString());
        //    }
        //}

        private void RecalculateTVItemStat_Click(object sender, EventArgs e)
        {
            TVItemStatService tvItemStatService = new TVItemStatService(LanguageEnum.en, user);

            List<TVItem> tvItemList = new List<TVItem>() { new TVItem() };
            int take = 100;
            int CurrentTVItemID = int.Parse(textBoxRecalculateTVItemStatStart.Text);
            while (tvItemList.Count > 0)
            {
                tvItemList = (from c in tvItemStatService.db.TVItems
                              where c.TVItemID > CurrentTVItemID
                              orderby c.TVItemID
                              select c).Take(take).ToList();

                using (CSSPDBEntities dd = new CSSPDBEntities())
                {
                    foreach (TVItem tvItem in tvItemList)
                    {
                        lblStatus.Text = tvItem.TVItemID.ToString();
                        lblStatus.Refresh();
                        Application.DoEvents();

                        List<TVTypeEnum> SubTVTypeList = tvItemStatService.GetSubTVTypeForTVItemStat((TVTypeEnum)tvItem.TVType);

                        foreach (TVTypeEnum tvType in SubTVTypeList)
                        {

                            List<TVItemStat> tvItemStatList = (from c in dd.TVItemStats
                                                               where c.TVItemID == tvItem.TVItemID
                                                               select c).ToList();

                            int ChildCount = tvItemStatService.GetChildCount2(tvItem, tvType);

                            TVItemStat TVItemStatExist = tvItemStatList.Where(c => c.TVItemID == tvItem.TVItemID && c.TVType == (int)tvType).FirstOrDefault();

                            if (TVItemStatExist == null)
                            {
                                TVItemStat tvItemStat = new TVItemStat()
                                {
                                    TVItemID = tvItem.TVItemID,
                                    TVType = (int)tvType,
                                    ChildCount = ChildCount,
                                    LastUpdateDate_UTC = DateTime.Now,
                                    LastUpdateContactTVItemID = 2
                                };

                                dd.TVItemStats.Add(tvItemStat);
                            }
                            else
                            {
                                if (TVItemStatExist.ChildCount != ChildCount)
                                {
                                    TVItemStatExist.ChildCount = ChildCount;
                                }
                            }
                        }

                        CurrentTVItemID = tvItem.TVItemID;
                        textBoxRecalculateTVItemStatStart.Text = CurrentTVItemID.ToString();
                    }

                    try
                    {
                        dd.SaveChanges();
                    }
                    catch (Exception)
                    {
                        // int aaa = 234;
                    }

                }
            }
        }



        public class TVItemIDPinLatLng
        {
            public int TVItemID { get; set; }
            public float Lat { get; set; }
            public float Lng { get; set; }
            public string TVText { get; set; }
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

            //    using (CSSPDBEntities dd = new CSSPDBEntities())
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


        private void button15_Click(object sender, EventArgs e) // button climate1
        {
            string StartTextSubsector = "S-";
            string JumpBeforeSubsector = "S-";
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMSubsectorService mwqmSubsectorService = new MWQMSubsectorService(LanguageEnum.en, user);
            AppTaskService appTaskService = new AppTaskService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                return;
            }

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Subsector).Where(c => c.TVText.StartsWith(StartTextSubsector)).OrderBy(c => c.TVText).ToList();
            if (tvItemModelSubsectorList.Count == 0)
            {
                return;
            }

            bool start = false;
            foreach (TVItemModel tvItemModel in tvItemModelSubsectorList)
            {
                if (tvItemModel.TVText.StartsWith(JumpBeforeSubsector))
                {
                    start = true;
                }

                if (!start)
                {
                    continue;
                }

                lblStatus.Text = tvItemModel.TVText;
                lblStatus.Refresh();
                Application.DoEvents();

                // get subsector with missing precipitation data in Run

                List<MWQMRun> mwqmRunList = new List<MWQMRun>();
                using (CSSPDBEntities db = new CSSPDBEntities())
                {
                    mwqmRunList = (from c in db.MWQMRuns
                                   where c.SubsectorTVItemID == tvItemModel.TVItemID
                                   && (c.RainDay0_mm == null
                                   || c.RainDay1_mm == null
                                   || c.RainDay2_mm == null
                                   || c.RainDay3_mm == null
                                   || c.RainDay4_mm == null
                                   || c.RainDay5_mm == null
                                   || c.RainDay6_mm == null
                                   || c.RainDay7_mm == null
                                   || c.RainDay8_mm == null
                                   || c.RainDay9_mm == null
                                   || c.RainDay10_mm == null)
                                   select c).ToList();
                }

                // get all years with data
                List<int> YearWithData = mwqmRunList.Select(c => c.DateTime_Local.Year).Distinct().OrderBy(c => c).ToList();

                foreach (int year in YearWithData)
                {
                    lblStatus.Text = tvItemModel.TVText + " ----- " + year;
                    lblStatus.Refresh();
                    Application.DoEvents();

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
            lblStatus.Text = "done...";
        }

        private void button16_Click(object sender, EventArgs e) // button climate2
        {
            string StartTextSubsector = "S-";
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMSubsectorService mwqmSubsectorService = new MWQMSubsectorService(LanguageEnum.en, user);
            AppTaskService appTaskService = new AppTaskService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                return;
            }

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Subsector).Where(c => c.TVText.StartsWith(StartTextSubsector)).OrderBy(c => c.TVText).ToList();
            if (tvItemModelSubsectorList.Count == 0)
            {
                return;
            }


            foreach (TVItemModel tvItemModel in tvItemModelSubsectorList)
            {
                List<MWQMRun> mwqmRunList = new List<MWQMRun>();
                using (CSSPDBEntities db = new CSSPDBEntities())
                {
                    mwqmRunList = (from c in db.MWQMRuns
                                   where c.SubsectorTVItemID == tvItemModel.TVItemID
                                   && (c.RainDay0_mm == null
                                   || c.RainDay1_mm == null
                                   || c.RainDay2_mm == null
                                   || c.RainDay3_mm == null
                                   || c.RainDay4_mm == null
                                   || c.RainDay5_mm == null
                                   || c.RainDay6_mm == null
                                   || c.RainDay7_mm == null
                                   || c.RainDay8_mm == null
                                   || c.RainDay9_mm == null
                                   || c.RainDay10_mm == null)
                                   select c).ToList();
                }

                // get all years with data
                List<int> YearWithData = mwqmRunList.Select(c => c.DateTime_Local.Year).Distinct().ToList();

                foreach (int year in YearWithData)
                {
                    lblStatus.Text = tvItemModel.TVText + " ----- " + year;
                    lblStatus.Refresh();
                    Application.DoEvents();

                    MWQMSubsectorModel mwqmSubsectorModel = mwqmSubsectorService.ClimateSiteSetDataToUseByAverageOrPriorityDB(tvItemModel.TVItemID, year, "Priority");
                    if (!string.IsNullOrWhiteSpace(mwqmSubsectorModel.Error))
                    {
                        //return;
                    }
                }
            }

            lblStatus.Text = "done...";

        }

        private void button17_Click(object sender, EventArgs e) // button climate3
        {
            string StartTextSubsector = "PE-";
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
            AppTaskService appTaskService = new AppTaskService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                return;
            }

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Subsector).Where(c => c.TVText.StartsWith(StartTextSubsector)).OrderBy(c => c.TVText).ToList();
            if (tvItemModelSubsectorList.Count == 0)
            {
                return;
            }

            foreach (TVItemModel tvItemModel in tvItemModelSubsectorList) //.Where(c => c.TVText.StartsWith("NB-06-020-002")))
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

            lblStatus.Text = "done...";

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

            //    using (CSSPDBEntities dd = new CSSPDBEntities())
            //    {
            //        TVItem tvItemSS = (from t in dd.TVItems
            //                           from c in dd.TVItemLanguages
            //                           where t.TVItemID == c.TVItemID
            //                           && c.TVText.StartsWith(Subsector + " ")
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




        public class DataIn
        {
            public DataIn()
            {

            }

            public string NewName { get; set; }
            public string Province { get; set; }
            public string Subsector { get; set; }
            public string PictureView { get; set; }
            public string Comments { get; set; }
            public string FileName { get; set; }
            public double Lat { get; set; }
            public double Lng { get; set; }
            public DateTime ObservationDate { get; set; }
            public string SpreadSheetSiteID { get; set; }
            public string CSSPWebToolsSiteID { get; set; }
        }

        public class DataOutSite
        {
            public DataOutSite()
            {

            }

            public string SpreadSheetSiteID { get; set; }
            public string CSSPWebToolsSiteID { get; set; }
            public string Province { get; set; }
            public string Subsector { get; set; }
            public double Lat { get; set; }
            public double Lng { get; set; }
            public string Projection { get; set; }
            public string Zone { get; set; }
            public string CivicNbr { get; set; }
            public string SiteComments { get; set; }

        }

        public class DataOutObservation
        {
            public DataOutObservation()
            {

            }

            public string SpreadSheetSiteID { get; set; }
            public string CSSPWebToolsSiteID { get; set; }
            public string SpreadSheetObservationID { get; set; }
            public string CSSPWebToolsObservationID { get; set; }
            public DateTime ObservationDate { get; set; }

        }
        public class DataOutObservationIssue
        {
            public DataOutObservationIssue()
            {

            }

            public string SpreadSheetSiteID { get; set; }
            public string CSSPWebToolsSiteID { get; set; }
            public string SpreadSheetObservationID { get; set; }
            public string CSSPWebToolsObservationID { get; set; }
            public string IssueID { get; set; }
            public string ObservationInfo { get; set; }
            public string ObservationInfoText { get; set; }

        }

        public class SS
        {
            public SS()
            {
                SSSiteList = new List<SSS>();
            }
            public string Name { get; set; }
            public List<SSS> SSSiteList { get; set; }
        }
        public class SSS
        {
            public SSS()
            {

            }

            public FileInfo FileInfoSite { get; set; }
            public string SiteName { get; set; }
        }

        public class PolyObj
        {
            public string Classification { get; set; } = "";
            public float MinLat { get; set; } = 0.0F;
            public float MaxLat { get; set; } = 0.0F;
            public float MinLng { get; set; } = 0.0F;
            public float MaxLng { get; set; } = 0.0F;
            public List<Coord> coordList { get; set; } = new List<Coord>();
            public string Subsector { get; set; } = "";
        }

        private void GetCoordinates(List<PolyObj> polyObsList, XmlNode node)
        {
            if (node.Name == "coordinates")
            {
                PolyObj polyObj = new PolyObj();

                List<string> pointListText = node.InnerText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                int ordinal = 0;
                foreach (string pointText in pointListText)
                {
                    string pointTxt = pointText.Replace("\r\n", "");

                    if (!string.IsNullOrWhiteSpace(pointTxt))
                    {
                        List<string> valListText = pointTxt.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                        if (valListText.Count != 3)
                        {
                            richTextBoxStatus.Text = "pointText.Count != 3";
                            return;
                        }

                        float Lng = float.Parse(valListText[0]);
                        float Lat = float.Parse(valListText[1]);

                        Coord coord = new Coord() { Lat = Lat, Lng = Lng, Ordinal = ordinal };

                        polyObj.coordList.Add(coord);

                        ordinal += 1;
                    }
                }

                polyObj.MinLat = polyObj.coordList.Min(c => c.Lat) - 0.0001f;
                polyObj.MinLng = polyObj.coordList.Min(c => c.Lng) - 0.0001f;
                polyObj.MaxLat = polyObj.coordList.Max(c => c.Lat) + 0.0001f;
                polyObj.MaxLng = polyObj.coordList.Max(c => c.Lng) + 0.0001f;


                polyObsList.Add(polyObj);
            }



            foreach (XmlNode n in node.ChildNodes)
            {
                GetCoordinates(polyObsList, n);
            }

        }
        private void button41_Click(object sender, EventArgs e)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            ClassificationService classificationService = new ClassificationService(LanguageEnum.en, user);

            #region Reading the NB_Class_2012-06.KML
            List<PolyObj> polyObjList = new List<PolyObj>();

            FileInfo fi = new FileInfo(@"C:\CSSP Latest Code Old\DataTool\ImportByFunction\Data_inputs\ClassificationPolygons_NB.KML");

            if (!fi.Exists)
            {
                richTextBoxStatus.AppendText("File not found [" + fi.FullName + "]\r\n");
                return;
            }

            StreamReader sr = fi.OpenText();
            StringBuilder sb = new StringBuilder(sr.ReadToEnd());
            sb.Replace("xsi:schemaLocation", "xmlns");
            sr.Close();

            StreamWriter sw = fi.CreateText();
            sw.Write(sb.ToString());
            sw.Close();

            XmlDocument doc = new XmlDocument();
            doc.Load(fi.FullName);

            foreach (XmlNode xmlNode in doc.ChildNodes)
            {
                GetCoordinates(polyObjList, xmlNode);
            }

            #endregion Reading the NB_Class_2012-06.KML

            #region Class_Subsector_Input.kml
            List<PolyObj> polyObjList2 = new List<PolyObj>();

            FileInfo fi2 = new FileInfo(@"C:\CSSP Latest Code Old\DataTool\ImportByFunction\Data_inputs\ClassificationInputs_NB.kml");

            if (!fi2.Exists)
            {
                richTextBoxStatus.AppendText("File not found [" + fi2.FullName + "]\r\n");
                return;
            }

            XmlDocument doc2 = new XmlDocument();
            doc2.Load(fi2.FullName);

            string CurrentSubsector = "";
            string CurrentClassification = "";

            XmlNode StartNode2 = doc2.ChildNodes[1].ChildNodes[0];
            foreach (XmlNode n in StartNode2.ChildNodes)
            {
                if (Cancel) return;

                if (n.Name == "Folder")
                {
                    foreach (XmlNode n1 in n.ChildNodes)
                    {
                        if (n1.Name == "Folder")
                        {
                            CurrentSubsector = "";

                            foreach (XmlNode n22 in n1)
                            {
                                if (n22.Name == "name")
                                {
                                    CurrentSubsector = n22.InnerText;
                                }

                                if (n22.Name == "Placemark")
                                {
                                    CurrentClassification = "";

                                    foreach (XmlNode n2 in n22)
                                    {

                                        if (n2.Name == "name")
                                        {
                                            CurrentClassification = n2.InnerText;
                                        }

                                        if (n2.Name == "LineString")
                                        {
                                            PolyObj polyObj = new PolyObj();

                                            polyObj.Subsector = CurrentSubsector;
                                            polyObj.Classification = CurrentClassification.ToUpper();

                                            foreach (XmlNode n3 in n2.ChildNodes)
                                            {
                                                if (n3.Name == "coordinates")
                                                {
                                                    string coordText = n3.InnerText.Trim();

                                                    List<string> pointListText = coordText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                                                    int ordinal = 0;
                                                    foreach (string pointText in pointListText)
                                                    {
                                                        string pointTxt = pointText.Trim();

                                                        if (!string.IsNullOrWhiteSpace(pointTxt))
                                                        {
                                                            List<string> valListText = pointTxt.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                                                            if (valListText.Count != 3)
                                                            {
                                                                richTextBoxStatus.Text = "pointText.Count != 3";
                                                                return;
                                                            }

                                                            float Lng = float.Parse(valListText[0]);
                                                            float Lat = float.Parse(valListText[1]);

                                                            List<PolyObj> polyObjCloseList = (from c in polyObjList
                                                                                              where c.MinLat <= Lat
                                                                                              && c.MaxLat >= Lat
                                                                                              && c.MinLng <= Lng
                                                                                              && c.MaxLng >= Lng
                                                                                              select c).ToList();

                                                            double distMin = 10000000D;
                                                            foreach (PolyObj polyObjClose in polyObjCloseList)
                                                            {
                                                                foreach (Coord coordClose in polyObjClose.coordList)
                                                                {
                                                                    double dist = mapInfoService.CalculateDistance(Lat * mapInfoService.d2r, Lng * mapInfoService.d2r, coordClose.Lat * mapInfoService.d2r, coordClose.Lng * mapInfoService.d2r, mapInfoService.R);

                                                                    if (dist < 20)
                                                                    {
                                                                        if (distMin > dist)
                                                                        {
                                                                            Lat = coordClose.Lat;
                                                                            Lng = coordClose.Lng;
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            Coord coord = new Coord() { Lat = Lat, Lng = Lng, Ordinal = ordinal };

                                                            polyObj.coordList.Add(coord);

                                                            ordinal += 1;
                                                        }
                                                    }
                                                }
                                            }
                                            polyObjList2.Add(polyObj);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion Reading the NB_Class_2012-06.KML

            #region Uploading MapInfo to CSSPWebTools
            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                // int slefij = 34;
            }

            TVItemModel tvItemModelNB = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "New Brunswick", TVTypeEnum.Province);
            if (!string.IsNullOrWhiteSpace(tvItemModelNB.Error))
            {
                // int slefij = 34;
            }

            List<TVItemModel> tvitemModelSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelNB.TVItemID, TVTypeEnum.Subsector);

            foreach (TVItemModel tvItemModelSS in tvitemModelSSList)
            {
                lblStatus.Text = tvItemModelSS.TVText;
                lblStatus.Refresh();
                Application.DoEvents();

                string TVTextSS = tvItemModelSS.TVText.Substring(0, tvItemModelSS.TVText.IndexOf(" "));

                List<TVItemModel> tvItemModelClassificationList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.Classification);
                List<int> TVItemIDClassificationList = new List<int>();

                int Ordinal = 0;
                foreach (PolyObj polyObj in polyObjList2.Where(c => c.Subsector == TVTextSS).OrderBy(c => c.Classification))
                {
                    Ordinal += 1;
                    richTextBoxStatus.AppendText($"{TVTextSS}\t{polyObj.Classification}\r\n");

                    string TVTextClass = "";
                    TVTypeEnum tvType = TVTypeEnum.Error;
                    ClassificationTypeEnum classificationType = ClassificationTypeEnum.Error;

                    switch (polyObj.Classification)
                    {
                        case "R":
                            {
                                TVTextClass = "R " + Ordinal;
                                tvType = TVTypeEnum.Restricted;
                                classificationType = ClassificationTypeEnum.Restricted;
                            }
                            break;
                        case "P":
                            {
                                TVTextClass = "P " + Ordinal;
                                tvType = TVTypeEnum.Prohibited;
                                classificationType = ClassificationTypeEnum.Prohibited;
                            }
                            break;
                        case "A":
                            {
                                TVTextClass = "A " + Ordinal;
                                tvType = TVTypeEnum.Approved;
                                classificationType = ClassificationTypeEnum.Approved;
                            }
                            break;
                        case "CA":
                            {
                                TVTextClass = "CA " + Ordinal;
                                tvType = TVTypeEnum.ConditionallyApproved;
                                classificationType = ClassificationTypeEnum.ConditionallyApproved;
                            }
                            break;
                        case "CR":
                            {
                                TVTextClass = "CR " + Ordinal;
                                tvType = TVTypeEnum.ConditionallyRestricted;
                                classificationType = ClassificationTypeEnum.ConditionallyRestricted;
                            }
                            break;
                        default:
                            break;
                    }

                    TVItemModel tvItemModelClass = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelSS.TVItemID, TVTextClass, TVTypeEnum.Classification);
                    if (!string.IsNullOrWhiteSpace(tvItemModelClass.Error))
                    {
                        tvItemModelClass = tvItemService.PostAddChildTVItemDB(tvItemModelSS.TVItemID, TVTextClass, TVTypeEnum.Classification);
                        if (!string.IsNullOrWhiteSpace(tvItemModelClass.Error))
                        {
                            richTextBoxStatus.AppendText($"{tvItemModelClass.Error}\r\n");
                            return;
                        }
                    }

                    TVItemIDClassificationList.Add(tvItemModelClass.TVItemID);

                    bool CoordListIsDifferent = false;
                    List<MapInfoModel> mapInfoModelList = mapInfoService.GetMapInfoModelListWithTVItemIDDB(tvItemModelClass.TVItemID);
                    foreach (MapInfoModel mapInfoModel in mapInfoModelList)
                    {
                        if (mapInfoModel.TVType == tvType)
                        {
                            List<MapInfoPointModel> mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithMapInfoIDDB(mapInfoModel.MapInfoID);
                            if (mapInfoPointModelList.Count != polyObj.coordList.Count)
                            {
                                CoordListIsDifferent = true;
                            }
                            else
                            {
                                for (int i = 0, count = mapInfoPointModelList.Count; i < count; i++)
                                {
                                    if (!(mapInfoPointModelList[i].Lat == polyObj.coordList[i].Lat && mapInfoPointModelList[i].Lng == polyObj.coordList[i].Lng))
                                    {
                                        CoordListIsDifferent = true;
                                        break;
                                    }
                                }
                            }

                            if (CoordListIsDifferent)
                            {
                                MapInfoModel mapInfoModelDeletRet = mapInfoService.PostDeleteMapInfoDB(mapInfoModel.MapInfoID);
                                if (!string.IsNullOrWhiteSpace(mapInfoModelDeletRet.Error))
                                {
                                    richTextBoxStatus.AppendText($"{mapInfoModelDeletRet.Error}\r\n");
                                    return;
                                }
                            }
                        }
                    }

                    if (CoordListIsDifferent)
                    {
                        MapInfoModel mapInfoModelRet = tvItemService.CreateMapInfoObjectDB(polyObj.coordList, MapInfoDrawTypeEnum.Polyline, tvType, tvItemModelClass.TVItemID);
                        if (!string.IsNullOrWhiteSpace(mapInfoModelRet.Error))
                        {
                            richTextBoxStatus.AppendText($"{mapInfoModelRet.Error}\r\n");
                            return;
                        }
                    }

                    ClassificationModel classificationModel = classificationService.GetClassificationModelWithClassificationTVItemIDDB(tvItemModelClass.TVItemID);
                    if (!string.IsNullOrWhiteSpace(classificationModel.Error))
                    {
                        ClassificationModel classificationModelNew = new ClassificationModel();
                        classificationModelNew.ClassificationTVItemID = tvItemModelClass.TVItemID;
                        classificationModelNew.ClassificationType = classificationType;
                        classificationModelNew.ClassificationTVText = TVTextClass;
                        classificationModelNew.Ordinal = Ordinal;

                        ClassificationModel classificationModelRet = classificationService.PostAddClassificationDB(classificationModelNew);
                        if (!string.IsNullOrWhiteSpace(classificationModelRet.Error))
                        {
                            richTextBoxStatus.AppendText($"{classificationModelRet.Error}\r\n");
                            return;
                        }
                    }
                    else
                    {
                        classificationModel.ClassificationType = classificationType;
                        classificationModel.Ordinal = Ordinal;

                        ClassificationModel classificationModelRet = classificationService.PostUpdateClassificationDB(classificationModel);
                        if (!string.IsNullOrWhiteSpace(classificationModelRet.Error))
                        {
                            richTextBoxStatus.AppendText($"{classificationModelRet.Error}\r\n");
                            return;
                        }
                    }

                    foreach (Coord coord in polyObj.coordList)
                    {
                        richTextBoxStatus.AppendText($"\t{coord.Lat}\t{coord.Lng}\t{coord.Ordinal}\r\n");
                    }

                }

                foreach (TVItemModel tvItemModel in tvItemModelClassificationList)
                {
                    if (!TVItemIDClassificationList.Where(c => c == tvItemModel.TVItemID).Any())
                    {
                        ClassificationModel classificationModelToDelete = classificationService.GetClassificationModelWithClassificationTVItemIDDB(tvItemModel.TVItemID);
                        if (!string.IsNullOrWhiteSpace(classificationModelToDelete.Error))
                        {
                            richTextBoxStatus.AppendText($"{classificationModelToDelete.Error}\r\n");
                            return;
                        }

                        ClassificationModel classificationModelToDeleteRet = classificationService.PostDeleteClassificationDB(classificationModelToDelete.ClassificationID);
                        if (!string.IsNullOrWhiteSpace(classificationModelToDeleteRet.Error))
                        {
                            richTextBoxStatus.AppendText($"{classificationModelToDeleteRet.Error}\r\n");
                            return;
                        }

                        MapInfoModel mapInfoModelToDelete = mapInfoService.PostDeleteMapInfoWithTVItemIDDB(tvItemModel.TVItemID);
                        if (!string.IsNullOrWhiteSpace(mapInfoModelToDelete.Error))
                        {
                            richTextBoxStatus.AppendText($"{mapInfoModelToDelete.Error}\r\n");
                            return;
                        }

                        TVItemModel tvItemModelToDelete = tvItemService.PostDeleteTVItemWithTVItemIDDB(tvItemModel.TVItemID);
                        if (!string.IsNullOrWhiteSpace(tvItemModelToDelete.Error))
                        {
                            richTextBoxStatus.AppendText($"{tvItemModelToDelete.Error}\r\n");
                            return;
                        }

                    }
                }


            }
            #endregion Uploading MapInfo to CSSPWebTools

            return;

            //StringBuilder sb = new StringBuilder();

            //sb.AppendLine($@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            //sb.AppendLine($@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            //sb.AppendLine($@"<Document>");
            //sb.AppendLine($@"	<name>KmlFile</name>");
            //sb.AppendLine($@"	<StyleMap id=""msn_ylw-pushpin"">");
            //sb.AppendLine($@"		<Pair>");
            //sb.AppendLine($@"			<key>normal</key>");
            //sb.AppendLine($@"			<styleUrl>#sn_ylw-pushpin</styleUrl>");
            //sb.AppendLine($@"		</Pair>");
            //sb.AppendLine($@"		<Pair>");
            //sb.AppendLine($@"			<key>highlight</key>");
            //sb.AppendLine($@"			<styleUrl>#sh_ylw-pushpin</styleUrl>");
            //sb.AppendLine($@"		</Pair>");
            //sb.AppendLine($@"	</StyleMap>");
            //sb.AppendLine($@"	<Style id=""sn_ylw-pushpin"">");
            //sb.AppendLine($@"		<IconStyle>");
            //sb.AppendLine($@"			<scale>1.1</scale>");
            //sb.AppendLine($@"			<Icon>");
            //sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            //sb.AppendLine($@"			</Icon>");
            //sb.AppendLine($@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            //sb.AppendLine($@"		</IconStyle>");
            //sb.AppendLine($@"		<LineStyle>");
            //sb.AppendLine($@"			<color>ffff00ff</color>");
            //sb.AppendLine($@"			<width>2</width>");
            //sb.AppendLine($@"		</LineStyle>");
            //sb.AppendLine($@"		<PolyStyle>");
            //sb.AppendLine($@"			<color>00ffffff</color>");
            //sb.AppendLine($@"		</PolyStyle>");
            //sb.AppendLine($@"	</Style>");
            //sb.AppendLine($@"	<Style id=""sh_ylw-pushpin"">");
            //sb.AppendLine($@"		<IconStyle>");
            //sb.AppendLine($@"			<scale>1.3</scale>");
            //sb.AppendLine($@"			<Icon>");
            //sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            //sb.AppendLine($@"			</Icon>");
            //sb.AppendLine($@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            //sb.AppendLine($@"		</IconStyle>");
            //sb.AppendLine($@"		<LineStyle>");
            //sb.AppendLine($@"			<color>ff00ff00</color>");
            //sb.AppendLine($@"			<width>2</width>");
            //sb.AppendLine($@"		</LineStyle>");
            //sb.AppendLine($@"		<PolyStyle>");
            //sb.AppendLine($@"			<color>00ffffff</color>");
            //sb.AppendLine($@"		</PolyStyle>");
            //sb.AppendLine($@"	</Style>");
            //sb.AppendLine($@"	<Folder>");
            //sb.AppendLine($@"		<name>NB Subsectors</name>");
            //sb.AppendLine($@"		<open>1</open>");



            //TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            //MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            //TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            //if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            //{
            //    int slefij = 34;
            //}

            //TVItemModel tvItemModelNB = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "New Brunswick", TVTypeEnum.Province);
            //if (!string.IsNullOrWhiteSpace(tvItemModelNB.Error))
            //{
            //    int slefij = 34;
            //}

            //List<TVItemModel> tvitemModelSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelNB.TVItemID, TVTypeEnum.Subsector);

            //foreach (TVItemModel tvItemModelSS in tvitemModelSSList)
            //{
            //    lblStatus.Text = tvItemModelSS.TVText;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    string TVText = tvItemModelSS.TVText.Substring(0, tvItemModelSS.TVText.IndexOf(" "));

            //    sb.AppendLine($@"		<Folder>");
            //    sb.AppendLine($@"			<name>{TVText}</name> ");
            //    sb.AppendLine($@"			<open>1</open>");
            //    sb.AppendLine($@"			<Placemark>");
            //    sb.AppendLine($@"				<name>Subsector Polygon</name>");
            //    sb.AppendLine($@"				<styleUrl>#msn_ylw-pushpin</styleUrl>");
            //    sb.AppendLine($@"				<Polygon>");
            //    sb.AppendLine($@"					<tessellate>1</tessellate>");
            //    sb.AppendLine($@"					<outerBoundaryIs>");
            //    sb.AppendLine($@"						<LinearRing>");
            //    sb.AppendLine($@"							<coordinates>");
            //    using (CSSPDBEntities db = new CSSPDBEntities())
            //    {
            //        List<MapInfoPointModel> mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.Subsector, MapInfoDrawTypeEnum.Polygon);

            //        foreach (MapInfoPointModel mapInfoPointModel in mapInfoPointModelList)
            //        {
            //            sb.AppendLine($@"{mapInfoPointModel.Lng.ToString("F6")},{mapInfoPointModel.Lat.ToString("F6")},0 ");
            //        }
            //    }

            //    sb.AppendLine($@"							</coordinates>");
            //    sb.AppendLine($@"						</LinearRing>");
            //    sb.AppendLine($@"					</outerBoundaryIs>");
            //    sb.AppendLine($@"				</Polygon>");
            //    sb.AppendLine($@"			</Placemark>");



            //    sb.AppendLine($@"		</Folder>");
            //}

            //sb.AppendLine($@"	</Folder>");
            //sb.AppendLine($@"</Document>");
            //sb.AppendLine($@"</kml>");

            //FileInfo fi2 = new FileInfo(@"C:\Users\leblancc\Desktop\Class_Subsector.kml");

            //StreamWriter sw = fi2.CreateText();
            //sw.Write(sb.ToString());
            //sw.Close();

            //lblStatus.Text = "Done....";
        }

        private void button42_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            //sb.AppendLine($@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            //sb.AppendLine($@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            //sb.AppendLine($@"<Document>");
            //sb.AppendLine($@"	<name>Subsector_PolSourceSites_MWQMSites.kml</name>");
            //sb.AppendLine($@"	<open>0</open>");

            //// Style for Subsector
            //sb.AppendLine($@"	<StyleMap id=""msn_ylw-pushpin"">");
            //sb.AppendLine($@"		<Pair>");
            //sb.AppendLine($@"			<key>normal</key>");
            //sb.AppendLine($@"			<styleUrl>#sn_ylw-pushpin</styleUrl>");
            //sb.AppendLine($@"		</Pair>");
            //sb.AppendLine($@"		<Pair>");
            //sb.AppendLine($@"			<key>highlight</key>");
            //sb.AppendLine($@"			<styleUrl>#sh_ylw-pushpin</styleUrl>");
            //sb.AppendLine($@"		</Pair>");
            //sb.AppendLine($@"	</StyleMap>");
            //sb.AppendLine($@"	<Style id=""sn_ylw-pushpin"">");
            //sb.AppendLine($@"		<IconStyle>");
            //sb.AppendLine($@"			<scale>1.1</scale>");
            //sb.AppendLine($@"			<Icon>");
            //sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            //sb.AppendLine($@"			</Icon>");
            //sb.AppendLine($@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            //sb.AppendLine($@"		</IconStyle>");
            //sb.AppendLine($@"		<LineStyle>");
            //sb.AppendLine($@"			<color>ffff00ff</color>");
            //sb.AppendLine($@"			<width>2</width>");
            //sb.AppendLine($@"		</LineStyle>");
            //sb.AppendLine($@"		<PolyStyle>");
            //sb.AppendLine($@"			<color>00ffffff</color>");
            //sb.AppendLine($@"		</PolyStyle>");
            //sb.AppendLine($@"	</Style>");
            //sb.AppendLine($@"	<Style id=""sh_ylw-pushpin"">");
            //sb.AppendLine($@"		<IconStyle>");
            //sb.AppendLine($@"			<scale>1.3</scale>");
            //sb.AppendLine($@"			<Icon>");
            //sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            //sb.AppendLine($@"			</Icon>");
            //sb.AppendLine($@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            //sb.AppendLine($@"		</IconStyle>");
            //sb.AppendLine($@"		<LineStyle>");
            //sb.AppendLine($@"			<color>ff00ff00</color>");
            //sb.AppendLine($@"			<width>2</width>");
            //sb.AppendLine($@"		</LineStyle>");
            //sb.AppendLine($@"		<PolyStyle>");
            //sb.AppendLine($@"			<color>00ffffff</color>");
            //sb.AppendLine($@"		</PolyStyle>");
            //sb.AppendLine($@"	</Style>");

            //// Style for Pollution Source Sites
            //sb.AppendLine($@"	<Style id=""s_ylw-pushpin"">");
            //sb.AppendLine($@"		<IconStyle>");
            //sb.AppendLine($@"			<color>ffff0000</color>");
            //sb.AppendLine($@"			<scale>1.2</scale>");
            //sb.AppendLine($@"			<Icon>");
            //sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_circle.png</href>");
            //sb.AppendLine($@"			</Icon>");
            //sb.AppendLine($@"		</IconStyle>");
            //sb.AppendLine($@"		<ListStyle>");
            //sb.AppendLine($@"		</ListStyle>");
            //sb.AppendLine($@"	</Style>");
            //sb.AppendLine($@"	<StyleMap id=""m_ylw-pushpin"">");
            //sb.AppendLine($@"		<Pair>");
            //sb.AppendLine($@"			<key>normal</key>");
            //sb.AppendLine($@"			<styleUrl>#s_ylw-pushpin</styleUrl>");
            //sb.AppendLine($@"		</Pair>");
            //sb.AppendLine($@"		<Pair>");
            //sb.AppendLine($@"			<key>highlight</key>");
            //sb.AppendLine($@"			<styleUrl>#s_ylw-pushpin_hl</styleUrl>");
            //sb.AppendLine($@"		</Pair>");
            //sb.AppendLine($@"	</StyleMap>");
            //sb.AppendLine($@"	<Style id=""s_ylw-pushpin_hl"">");
            //sb.AppendLine($@"		<IconStyle>");
            //sb.AppendLine($@"			<color>ffff0000</color>");
            //sb.AppendLine($@"			<scale>1.2</scale>");
            //sb.AppendLine($@"			<Icon>");
            //sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_circle_highlight.png</href>");
            //sb.AppendLine($@"			</Icon>");
            //sb.AppendLine($@"		</IconStyle>");
            //sb.AppendLine($@"		<ListStyle>");
            //sb.AppendLine($@"		</ListStyle>");
            //sb.AppendLine($@" </Style>");

            //// Style for MWQM Sites
            //sb.AppendLine($@"	<StyleMap id=""msn_placemark_square"">");
            //sb.AppendLine($@"		<Pair>");
            //sb.AppendLine($@"			<key>normal</key>");
            //sb.AppendLine($@"			<styleUrl>#sn_placemark_square</styleUrl>");
            //sb.AppendLine($@"		</Pair>");
            //sb.AppendLine($@"		<Pair>");
            //sb.AppendLine($@"			<key>highlight</key>");
            //sb.AppendLine($@"			<styleUrl>#sh_placemark_square_highlight</styleUrl>");
            //sb.AppendLine($@"		</Pair>");
            //sb.AppendLine($@"	</StyleMap>");
            //sb.AppendLine($@"	<Style id=""sh_placemark_square_highlight"">");
            //sb.AppendLine($@"		<IconStyle>");
            //sb.AppendLine($@"			<color>ff00ff00</color>");
            //sb.AppendLine($@"			<scale>1.2</scale>");
            //sb.AppendLine($@"			<Icon>");
            //sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_square_highlight.png</href>");
            //sb.AppendLine($@"			</Icon>");
            //sb.AppendLine($@"		</IconStyle>");
            //sb.AppendLine($@"		<ListStyle>");
            //sb.AppendLine($@"		</ListStyle>");
            //sb.AppendLine($@"	</Style>");
            //sb.AppendLine($@"	<Style id=""sn_placemark_square"">");
            //sb.AppendLine($@"		<IconStyle>");
            //sb.AppendLine($@"			<color>ff00ff00</color>");
            //sb.AppendLine($@"			<scale>1.2</scale>");
            //sb.AppendLine($@"			<Icon>");
            //sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_square.png</href>");
            //sb.AppendLine($@"			</Icon>");
            //sb.AppendLine($@"		</IconStyle>");
            //sb.AppendLine($@"		<ListStyle>");
            //sb.AppendLine($@"		</ListStyle>");
            //sb.AppendLine($@" </Style>");

            //// Style for Infrastructures
            //sb.AppendLine($@"	<Style id=""sn_shaded_dot"">");
            //sb.AppendLine($@"		<IconStyle>");
            //sb.AppendLine($@"			<color>ff0000ff</color>");
            //sb.AppendLine($@"			<scale>1.2</scale>");
            //sb.AppendLine($@"			<Icon>");
            //sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/shapes/shaded_dot.png</href>");
            //sb.AppendLine($@"			</Icon>");
            //sb.AppendLine($@"		</IconStyle>");
            //sb.AppendLine($@"		<ListStyle>");
            //sb.AppendLine($@"		</ListStyle>");
            //sb.AppendLine($@"	</Style>");
            //sb.AppendLine($@"	<Style id=""sh_shaded_dot"">");
            //sb.AppendLine($@"		<IconStyle>");
            //sb.AppendLine($@"			<color>ff0000ff</color>");
            //sb.AppendLine($@"			<scale>1.2</scale>");
            //sb.AppendLine($@"			<Icon>");
            //sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/shapes/shaded_dot.png</href>");
            //sb.AppendLine($@"			</Icon>");
            //sb.AppendLine($@"		</IconStyle>");
            //sb.AppendLine($@"		<ListStyle>");
            //sb.AppendLine($@"		</ListStyle>");
            //sb.AppendLine($@"	</Style>");
            //sb.AppendLine($@"	<StyleMap id=""msn_shaded_dot"">");
            //sb.AppendLine($@"		<Pair>");
            //sb.AppendLine($@"			<key>normal</key>");
            //sb.AppendLine($@"			<styleUrl>#sn_shaded_dot</styleUrl>");
            //sb.AppendLine($@"		</Pair>");
            //sb.AppendLine($@"		<Pair>");
            //sb.AppendLine($@"			<key>highlight</key>");
            //sb.AppendLine($@"			<styleUrl>#sh_shaded_dot</styleUrl>");
            //sb.AppendLine($@"		</Pair>");
            //sb.AppendLine($@" </StyleMap>");

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            PolSourceSiteService polSourceSiteService = new PolSourceSiteService(LanguageEnum.en, user);
            PolSourceObservationService polSourceObservationService = new PolSourceObservationService(LanguageEnum.en, user);
            PolSourceObservationIssueService polSourceObservationIssueService = new PolSourceObservationIssueService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                // int slefij = 34;
            }

            TVItemModel tvItemModelNB = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "New Brunswick", TVTypeEnum.Province);
            if (!string.IsNullOrWhiteSpace(tvItemModelNB.Error))
            {
                // int slefij = 34;
            }

            List<TVItemModel> tvitemModelSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelNB.TVItemID, TVTypeEnum.Subsector);

            //foreach (TVItemModel tvItemModelSS in tvitemModelSSList)
            //{
            //    lblStatus.Text = tvItemModelSS.TVText;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    string TVText = tvItemModelSS.TVText.Substring(0, tvItemModelSS.TVText.IndexOf(" "));

            //    // ---------------------------------------------------------------
            //    // doing Subsector
            //    // ---------------------------------------------------------------
            //    sb.AppendLine($@"		<Folder>");
            //    sb.AppendLine($@"			<name>{TVText}</name> ");
            //    sb.AppendLine($@"			<Placemark>");
            //    sb.AppendLine($@"				<name>Subsector Polygon</name>");
            //    sb.AppendLine($@"				<styleUrl>#msn_ylw-pushpin</styleUrl>");
            //    sb.AppendLine($@"				<Polygon>");
            //    sb.AppendLine($@"					<outerBoundaryIs>");
            //    sb.AppendLine($@"						<LinearRing>");
            //    sb.AppendLine($@"							<coordinates>");
            //    using (CSSPDBEntities db = new CSSPDBEntities())
            //    {
            //        List<MapInfoPointModel> mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.Subsector, MapInfoDrawTypeEnum.Polygon);

            //        foreach (MapInfoPointModel mapInfoPointModel in mapInfoPointModelList)
            //        {
            //            sb.AppendLine($@"{mapInfoPointModel.Lng.ToString("F6")},{mapInfoPointModel.Lat.ToString("F6")},0 ");
            //        }
            //    }

            //    sb.AppendLine($@"							</coordinates>");
            //    sb.AppendLine($@"						</LinearRing>");
            //    sb.AppendLine($@"					</outerBoundaryIs>");
            //    sb.AppendLine($@"				</Polygon>");
            //    sb.AppendLine($@"			</Placemark>");

            //    // ---------------------------------------------------------------
            //    // doing Pollution Source Sites
            //    // ---------------------------------------------------------------
            //    sb.AppendLine($@"		    <Folder>");
            //    sb.AppendLine($@"			    <name>Pollution Source Sites</name> ");

            //    List<TVItemModel> tvitemModelPSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.PolSourceSite).Where(c => c.IsActive == true).ToList();
            //    List<PolSourceSiteModel> polSourceSiteModelList = polSourceSiteService.GetPolSourceSiteModelListWithSubsectorTVItemIDDB(tvItemModelSS.TVItemID);
            //    List<PolSourceObservationModel> polSourceObservationModelList = polSourceObservationService.GetPolSourceObservationModelListWithSubsectorTVItemIDDB(tvItemModelSS.TVItemID);
            //    List<PolSourceObservationIssueModel> polSourceObservationIssueModelList = polSourceObservationIssueService.GetPolSourceObservationIssueModelListWithSubsectorTVItemIDDB(tvItemModelSS.TVItemID);

            //    foreach (TVItemModel tvItemModel in tvitemModelPSSList)
            //    {
            //        sb.AppendLine($@"			    <Placemark>");
            //        if (tvItemModel.TVText.Contains(" "))
            //        {
            //            sb.AppendLine($@"			    	<name>P_{tvItemModel.TVText.Substring(tvItemModel.TVText.LastIndexOf(" "))}</name>");
            //        }
            //        else
            //        {
            //            sb.AppendLine($@"			    	<name>P_{tvItemModel.TVText}</name>");
            //        }
            //        sb.AppendLine($@"               <description><![CDATA[");

            //        if (polSourceObservationModelList.Count > 0)
            //        {
            //            PolSourceObservationModel polSourceObservationModel = polSourceObservationModelList.OrderByDescending(c => c.ObservationDate_Local).FirstOrDefault();

            //            if (polSourceObservationModel != null)
            //            {
            //                sb.AppendLine($@"                <h3>Last Observation</h3>");
            //                sb.AppendLine($@"                <blockquote>");
            //                sb.AppendLine($@"                <p><b>Date:</b> {((DateTime)polSourceObservationModel.ObservationDate_Local).ToString("yyyy MMMM dd")}</p>");
            //                sb.AppendLine($@"                <p><b>Observation Last Update (UTC):</b> {((DateTime)polSourceObservationModel.LastUpdateDate_UTC).ToString("yyyy MMMM dd HH:mm:ss")}</p>");
            //                sb.AppendLine($@"                <p><b>Old Written Description:</b> {polSourceObservationModel.Observation_ToBeDeleted}</p>");

            //                List<PolSourceObservationIssueModel> polSourceObsIssueModelList = polSourceObservationIssueModelList.Where(c => c.PolSourceObservationID == polSourceObservationModel.PolSourceObservationID).OrderBy(c => c.Ordinal).ToList();
            //                if (polSourceObsIssueModelList.Count > 0)
            //                {
            //                    sb.AppendLine($@"                <blockquote>");
            //                    sb.AppendLine($@"                <ol>");
            //                    foreach (PolSourceObservationIssueModel polSourceObservationIssueModel in polSourceObsIssueModelList)
            //                    {
            //                        sb.AppendLine($@"                <li>");
            //                        sb.AppendLine($@"                <p><b>Issue Last Update (UTC):</b> {((DateTime)polSourceObservationIssueModel.LastUpdateDate_UTC).ToString("yyyy MMMM dd HH:mm:ss")}</p>");

            //                        string TVTextIssue = "";

            //                        if (!string.IsNullOrWhiteSpace(polSourceObservationIssueModel.ObservationInfo.Trim()))
            //                        {
            //                            polSourceObservationIssueModel.ObservationInfo = polSourceObservationIssueModel.ObservationInfo.Trim();
            //                            List<int> PolSourceObsInfoIntList = polSourceObservationIssueModel.ObservationInfo.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(c => int.Parse(c)).ToList();

            //                            for (int i = 0, count = PolSourceObsInfoIntList.Count; i < count; i++)
            //                            {
            //                                string Temp = _BaseEnumService.GetEnumText_PolSourceObsInfoReportEnum((PolSourceObsInfoEnum)PolSourceObsInfoIntList[i]);
            //                                switch ((PolSourceObsInfoIntList[i].ToString()).Substring(0, 3))
            //                                {
            //                                    case "101":
            //                                        {
            //                                            Temp = Temp.Replace("Source", "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<strong>Source</strong>");
            //                                        }
            //                                        break;
            //                                    //case "153":
            //                                    //    {
            //                                    //        Temp = Temp.Replace("Dilution Analyses", "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<strong>Dilution Analyses</strong>");
            //                                    //    }
            //                                    //    break;
            //                                    case "250":
            //                                        {
            //                                            Temp = Temp.Replace("Pathway", "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<strong>Pathway</strong>");
            //                                        }
            //                                        break;
            //                                    case "900":
            //                                        {
            //                                            Temp = Temp.Replace("Status", "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<strong>Status</strong>");
            //                                        }
            //                                        break;
            //                                    case "910":
            //                                        {
            //                                            Temp = Temp.Replace("Risk", "<strong>Risk</strong>");
            //                                        }
            //                                        break;
            //                                    case "110":
            //                                    case "120":
            //                                    case "122":
            //                                    case "151":
            //                                    case "152":
            //                                    case "153":
            //                                    case "155":
            //                                    case "156":
            //                                    case "157":
            //                                    case "163":
            //                                    case "166":
            //                                    case "167":
            //                                    case "170":
            //                                    case "171":
            //                                    case "172":
            //                                    case "173":
            //                                    case "176":
            //                                    case "178":
            //                                    case "181":
            //                                    case "182":
            //                                    case "183":
            //                                    case "185":
            //                                    case "186":
            //                                    case "187":
            //                                    case "190":
            //                                    case "191":
            //                                    case "192":
            //                                    case "193":
            //                                    case "194":
            //                                    case "196":
            //                                    case "198":
            //                                    case "199":
            //                                    case "220":
            //                                    case "930":
            //                                        {
            //                                            Temp = @"<span>" + Temp + "</span>";
            //                                        }
            //                                        break;
            //                                    default:
            //                                        break;
            //                                }
            //                                TVTextIssue = TVTextIssue + Temp;
            //                            }
            //                            sb.AppendLine($@"                <p><b>Selected:</b> {TVTextIssue}</p>");
            //                        }
            //                        sb.AppendLine($@"                </li>");
            //                    }
            //                    sb.AppendLine($@"                </ol>");
            //                    sb.AppendLine($@"                </blockquote>");
            //                }
            //                sb.AppendLine($@"                </blockquote>");
            //            }
            //        }
            //        sb.AppendLine($@"                   ]]></description>");
            //        sb.AppendLine($@"			    	<styleUrl>#s_ylw-pushpin</styleUrl>");
            //        sb.AppendLine($@"			    	<Point>");
            //        sb.AppendLine($@"		    			<coordinates>");
            //        using (CSSPDBEntities db = new CSSPDBEntities())
            //        {
            //            List<MapInfoPointModel> mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModel.TVItemID, TVTypeEnum.PolSourceSite, MapInfoDrawTypeEnum.Point);

            //            foreach (MapInfoPointModel mapInfoPointModel in mapInfoPointModelList)
            //            {
            //                sb.AppendLine($@"{mapInfoPointModel.Lng.ToString("F6")},{mapInfoPointModel.Lat.ToString("F6")},0 ");
            //            }
            //        }

            //        sb.AppendLine($@"						</coordinates>");
            //        sb.AppendLine($@"		    		</Point>");
            //        sb.AppendLine($@"		    	</Placemark>");
            //    }
            //    sb.AppendLine($@"		    </Folder>");

            //    // ---------------------------------------------------------------
            //    // doing MWQM Sites
            //    // ---------------------------------------------------------------
            //    sb.AppendLine($@"		    <Folder>");
            //    sb.AppendLine($@"			    <name>MWQM Sites</name> ");

            //    List<TVItemModel> tvitemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.MWQMSite).Where(c => c.IsActive == true).ToList();
            //    foreach (TVItemModel tvItemModel in tvitemModelMWQMSiteList)
            //    {
            //        sb.AppendLine($@"			    <Placemark>");
            //        if (tvItemModel.TVText.Contains(" "))
            //        {
            //            sb.AppendLine($@"			    	<name>S_{tvItemModel.TVText.Substring(0, tvItemModel.TVText.IndexOf(" "))}</name>");
            //        }
            //        else
            //        {
            //            sb.AppendLine($@"			    	<name>S_{tvItemModel.TVText}</name>");
            //        }
            //        sb.AppendLine($@"			    	<styleUrl>#msn_placemark_square</styleUrl>");
            //        sb.AppendLine($@"			    	<Point>");
            //        sb.AppendLine($@"		    			<coordinates>");
            //        using (CSSPDBEntities db = new CSSPDBEntities())
            //        {
            //            List<MapInfoPointModel> mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModel.TVItemID, TVTypeEnum.MWQMSite, MapInfoDrawTypeEnum.Point);

            //            foreach (MapInfoPointModel mapInfoPointModel in mapInfoPointModelList)
            //            {
            //                sb.AppendLine($@"{mapInfoPointModel.Lng.ToString("F6")},{mapInfoPointModel.Lat.ToString("F6")},0 ");
            //            }
            //        }

            //        sb.AppendLine($@"						</coordinates>");
            //        sb.AppendLine($@"		    		</Point>");
            //        sb.AppendLine($@"		    	</Placemark>");
            //    }
            //    sb.AppendLine($@"		    </Folder>");

            //    // ---------------------------------------------------------------
            //    // doing Municipality
            //    // ---------------------------------------------------------------
            //    sb.AppendLine($@"		    <Folder>");
            //    sb.AppendLine($@"			    <name>Municipalities</name> ");

            //    List<TVItemModel> tvItemModelMuniList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.Municipality).Where(c => c.IsActive == true).ToList();
            //    foreach (TVItemModel tvItemModelMuni in tvItemModelMuniList)
            //    {
            //        sb.AppendLine($@"		        <Folder>");
            //        sb.AppendLine($@"			        <name>{tvItemModelMuni.TVText}</name> ");
            //        List<TVItemModel> tvItemModelInfraList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelMuni.TVItemID, TVTypeEnum.Infrastructure).Where(c => c.IsActive == true).ToList();
            //        foreach (TVItemModel tvItemModel in tvItemModelInfraList)
            //        {
            //            List<TVTypeEnum> tvTypeInfList = new List<TVTypeEnum>() { TVTypeEnum.WasteWaterTreatmentPlant, TVTypeEnum.LiftStation, TVTypeEnum.LineOverflow };
            //            foreach (TVTypeEnum tvTypeInf in tvTypeInfList)
            //            {
            //                List<MapInfoPointModel> mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModel.TVItemID, tvTypeInf, MapInfoDrawTypeEnum.Point);

            //                if (mapInfoPointModelList.Count > 0)
            //                {
            //                    sb.AppendLine($@"			        <Placemark>");
            //                    sb.AppendLine($@"			        	<name>I_{tvItemModel.TVText}</name>");
            //                    sb.AppendLine($@"			        	<styleUrl>#sn_shaded_dot</styleUrl>");
            //                    sb.AppendLine($@"			        	<Point>");
            //                    sb.AppendLine($@"		        			<coordinates>");
            //                    using (CSSPDBEntities db = new CSSPDBEntities())
            //                    {
            //                        foreach (MapInfoPointModel mapInfoPointModel in mapInfoPointModelList)
            //                        {
            //                            sb.AppendLine($@"{mapInfoPointModel.Lng.ToString("F6")},{mapInfoPointModel.Lat.ToString("F6")},0 ");
            //                        }
            //                    }

            //                    sb.AppendLine($@"				    		</coordinates>");
            //                    sb.AppendLine($@"		    	    	</Point>");
            //                    sb.AppendLine($@"		    	    </Placemark>");
            //                }
            //            }

            //        }
            //        sb.AppendLine($@"		      </Folder>");
            //    }
            //    sb.AppendLine($@"		    </Folder>");

            //    sb.AppendLine($@"		</Folder>");
            //}

            //sb.AppendLine($@"</Document>");
            //sb.AppendLine($@"</kml>");

            //FileInfo fi = new FileInfo(@"C:\Users\leblancc\Desktop\Subsector_PolSourceSites_MWQMSites.kml");

            //StreamWriter sw = fi.CreateText();
            //sw.Write(sb.ToString());
            //sw.Close();

            // create the document for input
            sb = new StringBuilder();

            sb.AppendLine($@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine($@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine($@"<Document>");
            sb.AppendLine($@"	<name>Links_Between_PolSourceSites_MWQMSites_Input.kml</name>");
            sb.AppendLine($@"	<open>0</open>");

            // Style for Polygon
            sb.AppendLine($@"	<StyleMap id=""msn_ylw-pushpin"">");
            sb.AppendLine($@"		<Pair>");
            sb.AppendLine($@"			<key>normal</key>");
            sb.AppendLine($@"			<styleUrl>#sn_ylw-pushpin</styleUrl>");
            sb.AppendLine($@"		</Pair>");
            sb.AppendLine($@"		<Pair>");
            sb.AppendLine($@"			<key>highlight</key>");
            sb.AppendLine($@"			<styleUrl>#sh_ylw-pushpin</styleUrl>");
            sb.AppendLine($@"		</Pair>");
            sb.AppendLine($@"	</StyleMap>");
            sb.AppendLine($@"	<Style id=""sn_ylw-pushpin"">");
            sb.AppendLine($@"		<IconStyle>");
            sb.AppendLine($@"			<scale>1.1</scale>");
            sb.AppendLine($@"			<Icon>");
            sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine($@"			</Icon>");
            sb.AppendLine($@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine($@"		</IconStyle>");
            sb.AppendLine($@"		<LineStyle>");
            sb.AppendLine($@"			<color>ffffffff</color>");
            sb.AppendLine($@"			<width>2</width>");
            sb.AppendLine($@"		</LineStyle>");
            sb.AppendLine($@"		<PolyStyle>");
            sb.AppendLine($@"			<color>00ffffff</color>");
            sb.AppendLine($@"		</PolyStyle>");
            sb.AppendLine($@"	</Style>");
            sb.AppendLine($@"	<Style id=""sh_ylw-pushpin"">");
            sb.AppendLine($@"		<IconStyle>");
            sb.AppendLine($@"			<scale>1.3</scale>");
            sb.AppendLine($@"			<Icon>");
            sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine($@"			</Icon>");
            sb.AppendLine($@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine($@"		</IconStyle>");
            sb.AppendLine($@"		<LineStyle>");
            sb.AppendLine($@"			<color>ffffffff</color>");
            sb.AppendLine($@"			<width>2</width>");
            sb.AppendLine($@"		</LineStyle>");
            sb.AppendLine($@"		<PolyStyle>");
            sb.AppendLine($@"			<color>00ffffff</color>");
            sb.AppendLine($@"		</PolyStyle>");
            sb.AppendLine($@"	</Style>");

            sb.AppendLine($@"	<Style id=""s_ylw-pushpin"">");
            sb.AppendLine($@"		<IconStyle>");
            sb.AppendLine($@"			<scale>1.1</scale>");
            sb.AppendLine($@"			<Icon>");
            sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine($@"			</Icon>");
            sb.AppendLine($@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine($@"		</IconStyle>");
            sb.AppendLine($@"		<LineStyle>");
            sb.AppendLine($@"			<color>ffff00ff</color>");
            sb.AppendLine($@"			<width>2</width>");
            sb.AppendLine($@"		</LineStyle>");
            sb.AppendLine($@"		<PolyStyle>");
            sb.AppendLine($@"			<color>00ffffff</color>");
            sb.AppendLine($@"		</PolyStyle>");
            sb.AppendLine($@"	</Style>");
            sb.AppendLine($@"	<Style id=""s_ylw-pushpin_hl"">");
            sb.AppendLine($@"		<IconStyle>");
            sb.AppendLine($@"			<scale>1.3</scale>");
            sb.AppendLine($@"			<Icon>");
            sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine($@"			</Icon>");
            sb.AppendLine($@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine($@"		</IconStyle>");
            sb.AppendLine($@"		<LineStyle>");
            sb.AppendLine($@"			<color>ffff00ff</color>");
            sb.AppendLine($@"			<width>2</width>");
            sb.AppendLine($@"		</LineStyle>");
            sb.AppendLine($@"		<PolyStyle>");
            sb.AppendLine($@"			<color>00ffffff</color>");
            sb.AppendLine($@"		</PolyStyle>");
            sb.AppendLine($@"	</Style>");
            sb.AppendLine($@"	<StyleMap id=""m_ylw-pushpin"">");
            sb.AppendLine($@"		<Pair>");
            sb.AppendLine($@"			<key>normal</key>");
            sb.AppendLine($@"			<styleUrl>#s_ylw-pushpin</styleUrl>");
            sb.AppendLine($@"		</Pair>");
            sb.AppendLine($@"		<Pair>");
            sb.AppendLine($@"			<key>highlight</key>");
            sb.AppendLine($@"			<styleUrl>#s_ylw-pushpin_hl</styleUrl>");
            sb.AppendLine($@"		</Pair>");
            sb.AppendLine($@" </StyleMap>");


            foreach (TVItemModel tvItemModelSS in tvitemModelSSList)
            {
                lblStatus.Text = tvItemModelSS.TVText;
                lblStatus.Refresh();
                Application.DoEvents();

                string TVText = tvItemModelSS.TVText.Substring(0, tvItemModelSS.TVText.IndexOf(" "));

                // ---------------------------------------------------------------
                // doing Subsector
                // ---------------------------------------------------------------
                sb.AppendLine($@"		<Folder>");
                sb.AppendLine($@"			<name>{TVText}</name> ");
                sb.AppendLine($@"		</Folder>");
            }

            sb.AppendLine($@"</Document>");
            sb.AppendLine($@"</kml>");

            FileInfo fi = new FileInfo(@"C:\Users\leblancc\Desktop\Links_Between_PolSourceSites_MWQMSites_Input.kml");

            StreamWriter sw = fi.CreateText();
            sw.Write(sb.ToString());
            sw.Close();

            lblStatus.Text = "Done....";
        }

        private void button43_Click(object sender, EventArgs e)
        {
            List<string> startWithList = new List<string>() { "101", "143", "910" };

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            PolSourceSiteService polSourceSiteService = new PolSourceSiteService(LanguageEnum.en, user);
            PolSourceObservationService polSourceObservationService = new PolSourceObservationService(LanguageEnum.en, user);
            PolSourceObservationIssueService polSourceObservationIssueService = new PolSourceObservationIssueService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                // int sliefj = 34;
            }

            //TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "Nova Scotia", TVTypeEnum.Province);
            //if (!string.IsNullOrWhiteSpace(tvItemModelProv.Error))
            //{
            //    int sliefj = 34;
            //}

            List<TVItemModel> tvItemModelSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Subsector);

            foreach (TVItemModel tvItemModelSS in tvItemModelSSList)
            {
                lblStatus.Text = tvItemModelSS.TVText;
                lblStatus.Refresh();
                Application.DoEvents();

                //if (!(tvItemModelSS.TVText.StartsWith("NB-19-010-001")))
                //{
                //    continue;
                //}

                List<PolSourceSiteModel> polSourceSiteModelList = polSourceSiteService.GetPolSourceSiteModelListWithSubsectorTVItemIDDB(tvItemModelSS.TVItemID);

                foreach (PolSourceSiteModel polsourceSiteModel in polSourceSiteModelList)
                {
                    PolSourceObservationModel polSourceObservationModel = polSourceObservationService.GetPolSourceObservationModelLatestWithPolSourceSiteIDDB(polsourceSiteModel.PolSourceSiteID);

                    List<PolSourceObservationIssueModel> polSourceObservationIssueModelList = polSourceObservationIssueService.GetPolSourceObservationIssueModelListWithPolSourceObservationIDDB(polSourceObservationModel.PolSourceObservationID);

                    if (polSourceObservationIssueModelList.Count > 0)
                    {
                        PolSourceObservationIssueModel polSourceObservationIssueModel = polSourceObservationIssueModelList.OrderBy(c => c.Ordinal).FirstOrDefault();

                        List<string> PolSourceObsInfoList = polSourceObservationIssueModel.ObservationInfo.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                        // doing the other language
                        foreach (LanguageEnum lang in new List<LanguageEnum>() { LanguageEnum.en, LanguageEnum.fr })
                        {
                            Thread.CurrentThread.CurrentCulture = new CultureInfo(lang + "-CA");
                            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang + "-CA");

                            string TVText = "";
                            for (int i = 0, count = PolSourceObsInfoList.Count; i < count; i++)
                            {
                                if (PolSourceObsInfoList[i].Trim().Length > 3)
                                {
                                    string StartTxt = PolSourceObsInfoList[i].Substring(0, 3);

                                    if (startWithList.Where(c => c.StartsWith(StartTxt)).Any())
                                    {
                                        TVText = TVText.Trim();
                                        string TempText = _BaseEnumService.GetEnumText_PolSourceObsInfoEnum((PolSourceObsInfoEnum)int.Parse(PolSourceObsInfoList[i]));
                                        if (TempText.IndexOf("|") > 0)
                                        {
                                            TempText = TempText.Substring(0, TempText.IndexOf("|")).Trim();
                                        }
                                        TVText = TVText + (TVText.Length == 0 ? "" : ", ") + TempText;
                                    }
                                }
                            }

                            //if (!(polsourceSiteModel.Site == 75))
                            //{
                            //    continue;
                            //}

                            TVText = "P00000".Substring(0, "P00000".Length - polsourceSiteModel.Site.ToString().Length) + polsourceSiteModel.Site.ToString() + " - " + TVText;

                            TVItemLanguageModel tvItemLanguageModel = new TVItemLanguageModel();
                            tvItemLanguageModel.Language = lang;

                            bool Found = true;
                            while (Found)
                            {
                                if (TVText.Contains("  ") || TVText.Contains("" + "\u00A0" + "\u00A0"))
                                {
                                    TVText = TVText.Replace("  ", " ");
                                    TVText = TVText.Replace("" + "\u00A0" + "\u00A0", "" + "\u00A0");
                                }
                                else
                                {
                                    Found = false;
                                }
                            }

                            tvItemLanguageModel.TVText = TVText;
                            tvItemLanguageModel.TVItemID = polsourceSiteModel.PolSourceSiteTVItemID;

                            TVItemLanguageModel tvItemLanguageModelExist = tvItemService._TVItemLanguageService.GetTVItemLanguageModelWithTVItemIDAndLanguageDB(polsourceSiteModel.PolSourceSiteTVItemID, lang);

                            if (tvItemLanguageModelExist.TVText != TVText)
                            {
                                TVItemLanguageModel tvItemLanguageModelRet = tvItemService._TVItemLanguageService.PostUpdateTVItemLanguageDB(tvItemLanguageModel);
                                if (!string.IsNullOrWhiteSpace(tvItemLanguageModelRet.Error))
                                {
                                    // int slefji = 34;
                                }
                            }

                            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
                            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-CA");
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int NumberOfSamples = 30;
            StringBuilder sb = new StringBuilder();
            List<CSVValues> csvValuesList = new List<CSVValues>();
            List<CSVValues> csvValuesListFull = new List<CSVValues>();

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);
            MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);

            int ProvTVItemID = 7; // NB
            string ProvText = "NB";
            //int ProvTVItemID = 8; // NS
            //string ProvText = "NS";
            //int ProvTVItemID = 9; // PE
            //string ProvText = "PE";
            //int ProvTVItemID = 10; // NL
            //string ProvText = "NL";
            //int ProvTVItemID = 11; // BC
            //string ProvText = "BC";
            //int ProvTVItemID = 12; // QC
            //string ProvText = "QC";


            TVItemModel tvItemModelProv = tvItemService.GetTVItemModelWithTVItemIDDB(ProvTVItemID);
            if (!string.IsNullOrWhiteSpace(tvItemModelProv.Error))
            {
                return;
            }

            List<TVItemModel> tvItemModelSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);

            sb.AppendLine($@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine($@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine($@"<Document>");
            sb.AppendLine($@"	<name>StatsAndStatsWithRain_{ ProvText }.kml</name>");

            List<LetterColorName> LetterColorNameList = new List<LetterColorName>()
            {
               new LetterColorName() { Letter = "F", Color = "ff7777", Name = "NoDepuration" },
               new LetterColorName() { Letter = "E", Color = "ff7777", Name = "NoDepuration" },
               new LetterColorName() { Letter = "D", Color = "ff7777", Name = "NoDepuration" },
               new LetterColorName() { Letter = "C", Color = "ff7777", Name = "NoDepuration" },
               new LetterColorName() { Letter = "B", Color = "ff7777", Name = "NoDepuration" },
               new LetterColorName() { Letter = "A", Color = "ff7777", Name = "NoDepuration" },
               new LetterColorName() { Letter = "F", Color = "0000ff", Name = "Fail" },
               new LetterColorName() { Letter = "E", Color = "0000ff", Name = "Fail" },
               new LetterColorName() { Letter = "D", Color = "0000ff", Name = "Fail" },
               new LetterColorName() { Letter = "C", Color = "0000ff", Name = "Fail" },
               new LetterColorName() { Letter = "B", Color = "0000ff", Name = "Fail" },
               new LetterColorName() { Letter = "A", Color = "0000ff", Name = "Fail" },
               new LetterColorName() { Letter = "F", Color = "00ff00", Name = "Pass" },
               new LetterColorName() { Letter = "E", Color = "00ff00", Name = "Pass" },
               new LetterColorName() { Letter = "D", Color = "00ff00", Name = "Pass" },
               new LetterColorName() { Letter = "C", Color = "00ff00", Name = "Pass" },
               new LetterColorName() { Letter = "B", Color = "00ff00", Name = "Pass" },
               new LetterColorName() { Letter = "A", Color = "00ff00", Name = "Pass" },
            };

            foreach (LetterColorName letterColorName in LetterColorNameList)
            {
                sb.AppendLine($@"	<Style id=""{ letterColorName.Name }_{ letterColorName.Letter }"">");
                sb.AppendLine($@"		<IconStyle>");
                sb.AppendLine($@"			<color>ff{ letterColorName.Color }</color>");
                sb.AppendLine($@"			<scale>1.0</scale>");
                sb.AppendLine($@"			<Icon>");
                sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/paddle/" + $"{letterColorName.Letter}.png</href>");
                sb.AppendLine($@"			</Icon>");
                sb.AppendLine($@"			<hotSpot x=""32"" y=""1"" xunits=""pixels"" yunits=""pixels""/>");
                sb.AppendLine($@"		</IconStyle>");
                sb.AppendLine($@"   </Style>");
            }
            List<double> DryList = new List<double>() { 4, 8, 12, 16 };
            List<double> WetList = new List<double>() { 12, 25, 37, 50 };

            #region Styles
            sb.AppendLine($@"	<Style id=""SS_Point"">");
            sb.AppendLine($@"		<IconStyle>");
            sb.AppendLine($@"			<color>ffffff</color>");
            sb.AppendLine($@"			<scale>1.0</scale>");
            sb.AppendLine($@"			<Icon>");
            sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine($@"			</Icon>");
            sb.AppendLine($@"			<hotSpot x=""32"" y=""1"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine($@"		</IconStyle>");
            sb.AppendLine($@"   </Style>");

            sb.AppendLine($@"	<StyleMap id=""m_ylw-pushpin"">");
            sb.AppendLine($@"		<Pair>");
            sb.AppendLine($@"			<key>normal</key>");
            sb.AppendLine($@"			<styleUrl>#s_ylw-pushpin</styleUrl>");
            sb.AppendLine($@"		</Pair>");
            sb.AppendLine($@"		<Pair>");
            sb.AppendLine($@"			<key>highlight</key>");
            sb.AppendLine($@"			<styleUrl>#s_ylw-pushpin_hl</styleUrl>");
            sb.AppendLine($@"		</Pair>");
            sb.AppendLine($@"	</StyleMap>");
            sb.AppendLine($@"	<Style id=""s_ylw-pushpin_hl"">");
            sb.AppendLine($@"		<IconStyle>");
            sb.AppendLine($@"			<scale>1.3</scale>");
            sb.AppendLine($@"			<Icon>");
            sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine($@"			</Icon>");
            sb.AppendLine($@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine($@"		</IconStyle>");
            sb.AppendLine($@"		<LineStyle>");
            sb.AppendLine($@"			<color>ff00ff00</color>");
            sb.AppendLine($@"			<width>2</width>");
            sb.AppendLine($@"		</LineStyle>");
            sb.AppendLine($@"		<PolyStyle>");
            sb.AppendLine($@"			<color>0000ff00</color>");
            sb.AppendLine($@"		</PolyStyle>");
            sb.AppendLine($@"	</Style>");
            sb.AppendLine($@"	<Style id=""s_ylw-pushpin"">");
            sb.AppendLine($@"		<IconStyle>");
            sb.AppendLine($@"			<scale>1.1</scale>");
            sb.AppendLine($@"			<Icon>");
            sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine($@"			</Icon>");
            sb.AppendLine($@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine($@"		</IconStyle>");
            sb.AppendLine($@"		<LineStyle>");
            sb.AppendLine($@"			<color>ff00ff00</color>");
            sb.AppendLine($@"			<width>2</width>");
            sb.AppendLine($@"		</LineStyle>");
            sb.AppendLine($@"		<PolyStyle>");
            sb.AppendLine($@"			<color>0000ff00</color>");
            sb.AppendLine($@"		</PolyStyle>");
            sb.AppendLine($@" </Style>");
            #endregion Styles

            #region Subsectors names
            // -------------------------------------------------------------------------------------------------------------
            // Start Subsectors names
            // -------------------------------------------------------------------------------------------------------------
            sb.AppendLine($@"	<Folder>");
            sb.AppendLine($@"	<name>Subsectors names</name>");
            sb.AppendLine($@"	<open>0</open>");

            foreach (TVItemModel tvItemModelSS in tvItemModelSSList)
            {
                lblStatus.Text = tvItemModelSS.TVText;
                lblStatus.Refresh();
                Application.DoEvents();

                //if (tvItemModelSS.TVItemID != 635)
                //{
                //    continue; // just doing Bouctouche for now
                //}

                List<MapInfo> mapInfoList = new List<MapInfo>();
                List<MapInfoPoint> mapInfoPointList = new List<MapInfoPoint>();

                using (CSSPDBEntities db2 = new CSSPDBEntities())
                {
                    mapInfoList = (from c in db2.MapInfos
                                   where c.TVItemID == tvItemModelSS.TVItemID
                                   select c).ToList();

                    List<int> mapInfoIDList = mapInfoList.Select(c => c.MapInfoID).Distinct().ToList();

                    mapInfoPointList = (from c in db2.MapInfoPoints
                                        from mid in mapInfoIDList
                                        where c.MapInfoID == mid
                                        select c).ToList();
                }

                sb.AppendLine($@"	    <Folder>");
                sb.AppendLine($@"	    <name>{ tvItemModelSS.TVText }</name>");

                //sb.AppendLine($@"	<Placemark>");
                //sb.AppendLine($@"		<name>{ tvItemModelSS.TVText }</name>");
                //sb.AppendLine($@"		<styleUrl>#m_ylw-pushpin</styleUrl>");
                //sb.AppendLine($@"		<Polygon>");
                //sb.AppendLine($@"			<tessellate>1</tessellate>");
                //sb.AppendLine($@"			<outerBoundaryIs>");
                //sb.AppendLine($@"				<LinearRing>");
                //sb.AppendLine($@"					<coordinates>");
                //sb.Append($@"						");
                //List<MapInfoPoint> mapInfoPointListPoly = (from mi in mapInfoList
                //                                           from mip in mapInfoPointList
                //                                           where mi.MapInfoID == mip.MapInfoID
                //                                           && mi.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon
                //                                           orderby mip.Ordinal
                //                                           select mip).ToList();


                //foreach (MapInfoPoint mapInfoPoint in mapInfoPointListPoly)
                //{
                //    sb.Append($@"{ mapInfoPoint.Lng },{ mapInfoPoint.Lat },0 ");
                //}
                //sb.AppendLine($@"");
                //sb.AppendLine($@"					</coordinates>");
                //sb.AppendLine($@"				</LinearRing>");
                //sb.AppendLine($@"			</outerBoundaryIs>");
                //sb.AppendLine($@"		</Polygon>");
                //sb.AppendLine($@"   </Placemark>");


                List<MapInfoPoint> mapInfoPointListPoint = (from mi in mapInfoList
                                                            from mip in mapInfoPointList
                                                            where mi.MapInfoID == mip.MapInfoID
                                                            && mi.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
                                                            orderby mip.Ordinal
                                                            select mip).ToList();

                if (mapInfoPointListPoint.Count > 0)
                {
                    sb.AppendLine($@"           <Placemark>");
                    sb.AppendLine($@"	        	<name>{ tvItemModelSS.TVText }</name>");
                    sb.AppendLine($@"	        	<styleUrl>#m_ylw-pushpin</styleUrl>");
                    sb.AppendLine($@"	        	<Point>");
                    sb.AppendLine($@"	        		<coordinates>{ mapInfoPointListPoint[0].Lng },{ mapInfoPointListPoint[0].Lat },0</coordinates>");
                    sb.AppendLine($@"	        	</Point>");
                    sb.AppendLine($@"	        </Placemark>");

                }
                sb.AppendLine($@"	    </Folder>");
            }

            sb.AppendLine($@"	</Folder>");
            // -------------------------------------------------------------------------------------------------------------
            // End Subsectors names
            // -------------------------------------------------------------------------------------------------------------
            #endregion Subsectors names

            #region Subsectors polygon
            // -------------------------------------------------------------------------------------------------------------
            // Start Subsectors polygon
            // -------------------------------------------------------------------------------------------------------------
            sb.AppendLine($@"	<Folder>");
            sb.AppendLine($@"	<name>Subsectors polygon</name>");
            sb.AppendLine($@"	<open>0</open>");

            foreach (TVItemModel tvItemModelSS in tvItemModelSSList)
            {
                lblStatus.Text = tvItemModelSS.TVText;
                lblStatus.Refresh();
                Application.DoEvents();

                //if (tvItemModelSS.TVItemID != 635)
                //{
                //    continue; // just doing Bouctouche for now
                //}

                List<MapInfo> mapInfoList = new List<MapInfo>();
                List<MapInfoPoint> mapInfoPointList = new List<MapInfoPoint>();

                using (CSSPDBEntities db2 = new CSSPDBEntities())
                {
                    mapInfoList = (from c in db2.MapInfos
                                   where c.TVItemID == tvItemModelSS.TVItemID
                                   select c).ToList();

                    List<int> mapInfoIDList = mapInfoList.Select(c => c.MapInfoID).Distinct().ToList();

                    mapInfoPointList = (from c in db2.MapInfoPoints
                                        from mid in mapInfoIDList
                                        where c.MapInfoID == mid
                                        select c).ToList();
                }

                sb.AppendLine($@"	    <Folder>");
                sb.AppendLine($@"	    <name>{ tvItemModelSS.TVText }</name>");

                sb.AppendLine($@"	<Placemark>");
                sb.AppendLine($@"		<name>{ tvItemModelSS.TVText }</name>");
                sb.AppendLine($@"		<styleUrl>#m_ylw-pushpin</styleUrl>");
                sb.AppendLine($@"		<Polygon>");
                sb.AppendLine($@"			<tessellate>1</tessellate>");
                sb.AppendLine($@"			<outerBoundaryIs>");
                sb.AppendLine($@"				<LinearRing>");
                sb.AppendLine($@"					<coordinates>");
                sb.Append($@"						");
                List<MapInfoPoint> mapInfoPointListPoly = (from mi in mapInfoList
                                                           from mip in mapInfoPointList
                                                           where mi.MapInfoID == mip.MapInfoID
                                                           && mi.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon
                                                           orderby mip.Ordinal
                                                           select mip).ToList();


                foreach (MapInfoPoint mapInfoPoint in mapInfoPointListPoly)
                {
                    sb.Append($@"{ mapInfoPoint.Lng },{ mapInfoPoint.Lat },0 ");
                }
                sb.AppendLine($@"");
                sb.AppendLine($@"					</coordinates>");
                sb.AppendLine($@"				</LinearRing>");
                sb.AppendLine($@"			</outerBoundaryIs>");
                sb.AppendLine($@"		</Polygon>");
                sb.AppendLine($@"   </Placemark>");


                //List<MapInfoPoint> mapInfoPointListPoint = (from mi in mapInfoList
                //                                            from mip in mapInfoPointList
                //                                            where mi.MapInfoID == mip.MapInfoID
                //                                            && mi.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
                //                                            orderby mip.Ordinal
                //                                            select mip).ToList();

                //if (mapInfoPointListPoint.Count > 0)
                //{
                //    sb.AppendLine($@"           <Placemark>");
                //    sb.AppendLine($@"	        	<name>{ tvItemModelSS.TVText }</name>");
                //    sb.AppendLine($@"	        	<styleUrl>#m_ylw-pushpin</styleUrl>");
                //    sb.AppendLine($@"	        	<Point>");
                //    sb.AppendLine($@"	        		<coordinates>{ mapInfoPointListPoint[0].Lng },{ mapInfoPointListPoint[0].Lat },0</coordinates>");
                //    sb.AppendLine($@"	        	</Point>");
                //    sb.AppendLine($@"	        </Placemark>");

                //}
                sb.AppendLine($@"	    </Folder>");
            }

            sb.AppendLine($@"	</Folder>");
            // -------------------------------------------------------------------------------------------------------------
            // End Subsectors polygon
            // -------------------------------------------------------------------------------------------------------------
            #endregion Subsectors polygon

            #region Sites
            // -------------------------------------------------------------------------------------------------------------
            // Start Sites
            // -------------------------------------------------------------------------------------------------------------
            NumberOfSamples = 30;
            sb.AppendLine($@"	<Folder>");
            sb.AppendLine($@"	<name>Sites</name>");
            sb.AppendLine($@"	<open>0</open>");

            DoAllSites(tvItemModelSSList, tvItemService, sb);

            sb.AppendLine($@"	</Folder>");

            // -------------------------------------------------------------------------------------------------------------
            // End Sites
            // -------------------------------------------------------------------------------------------------------------
            #endregion Sites

            #region All-All-All 30 runs
            // -------------------------------------------------------------------------------------------------------------
            // Start All-All-All 30 samples
            // -------------------------------------------------------------------------------------------------------------
            NumberOfSamples = 30;
            sb.AppendLine($@"	<Folder>");
            sb.AppendLine($@"	<name>All-All-All ({ NumberOfSamples })</name>");

            DoAllSubsectorStats(StatType.Run30, tvItemModelSSList, tvItemService, NumberOfSamples, DryList, WetList, csvValuesList, csvValuesListFull, sb);

            sb.AppendLine($@"	</Folder>");

            // -------------------------------------------------------------------------------------------------------------
            // End All-All-All 30 samples
            // -------------------------------------------------------------------------------------------------------------
            #endregion All-All-All 30 runs

            #region Dry-All-All (4,8,12,16)mm
            // -------------------------------------------------------------------------------------------------------------
            // Start Dry-All-All (4,8,12,16)mm
            // -------------------------------------------------------------------------------------------------------------

            sb.AppendLine($@"	<Folder>");
            sb.AppendLine($@"	<name>Dry-All-All (4,8,12,16)mm</name>");

            DoAllSubsectorStats(StatType.Dry, tvItemModelSSList, tvItemService, NumberOfSamples, DryList, WetList, csvValuesList, csvValuesListFull, sb);

            sb.AppendLine($@"	</Folder>");

            // -------------------------------------------------------------------------------------------------------------
            // End Dry-All-All (4,8,12,16)mm
            // -------------------------------------------------------------------------------------------------------------
            #endregion Dry-All-All (4,8,12,16)mm

            #region Wet-All-All (12,25,37,50)mm
            // -------------------------------------------------------------------------------------------------------------
            // Start Wet-All-All (12,25,37,50)mm
            // -------------------------------------------------------------------------------------------------------------


            sb.AppendLine($@"	<Folder>");
            sb.AppendLine($@"	<name>Wet-All-All (12,25,37,50)mm</name>");

            DoAllSubsectorStats(StatType.Wet, tvItemModelSSList, tvItemService, NumberOfSamples, DryList, WetList, csvValuesList, csvValuesListFull, sb);

            sb.AppendLine($@"	</Folder>");

            // -------------------------------------------------------------------------------------------------------------
            // End Wet-All-All (12,25,37,50)mm
            // -------------------------------------------------------------------------------------------------------------
            #endregion Wet-All-All (12,25,37,50)mm

            sb.AppendLine($@"</Document>");
            sb.AppendLine($@"</kml>");

            FileInfo fi = new FileInfo($@"C:\Users\leblancc\Desktop\StatsWithRain_{ ProvText }.kml");

            StreamWriter sw = fi.CreateText();
            sw.Write(sb.ToString());
            sw.Close();

            WriteCSVFile(ProvText, csvValuesList);

            WriteFullCSVFile(ProvText, csvValuesListFull);

            lblStatus.Text = "Done...";
            lblStatus.Refresh();
            Application.DoEvents();
        }

        private void WriteCSVFile(string ProvText, List<CSVValues> csvValuesList)
        {
            StringBuilder sbCSV = new StringBuilder();

            sbCSV.AppendLine("Subsector,Site,StartYear,EndYear,StatType,Class,Letter,NumbSamples,P90,GM,Med,PercOver43,PercOver260,ValueList Dry(4_8_12_16) Wet(12_25_37_50)");

            foreach (CSVValues csvValues in csvValuesList.OrderBy(c => c.Subsector).ThenBy(c => c.Site))
            {
                string P90Str = (csvValues.P90 < 0 ? "" : csvValues.P90.ToString().Replace(",", "."));
                string GMStr = (csvValues.GM < 0 ? "" : csvValues.GM.ToString().Replace(",", "."));
                string MedStr = (csvValues.Med < 0 ? "" : csvValues.Med.ToString().Replace(",", "."));
                string PercOver43Str = (csvValues.PercOver43 < 0 ? "" : csvValues.PercOver43.ToString().Replace(",", "."));
                string PercOver260Str = (csvValues.PercOver260 < 0 ? "" : csvValues.PercOver260.ToString().Replace(",", "."));

                sbCSV.AppendLine($"{ csvValues.Subsector.Replace(",", "_") },{ csvValues.Site.Replace(",", "_") },{ csvValues.StartYear },{ csvValues.EndYear },{ csvValues.statType.ToString() },{ csvValues.ClassStr },{ csvValues.Letter },{ csvValues.NumbSamples },{ P90Str },{ GMStr },{ MedStr },{ PercOver43Str },{ PercOver260Str },{ csvValues.ValueList }");
            }

            FileInfo fi = new FileInfo($@"C:\Users\leblancc\Desktop\StatsWithRain_{ ProvText }.csv");

            StreamWriter sw = fi.CreateText();
            sw.Write(sbCSV.ToString());
            sw.Close();

        }

        private void WriteFullCSVFile(string ProvText, List<CSVValues> csvValuesListFull)
        {
            StringBuilder sbCSV = new StringBuilder();

            sbCSV.AppendLine("Subsector,Site,StartYear,EndYear,StatType,Class,Letter,NumbSamples,P90,GM,Med,PercOver43,PercOver260,ValueList Dry(4_8_12_16) Wet(12_25_37_50)");

            foreach (CSVValues csvValues in csvValuesListFull.OrderBy(c => c.Subsector).ThenBy(c => c.Site))
            {
                string P90Str = (csvValues.P90 < 0 ? "" : csvValues.P90.ToString().Replace(",", "."));
                string GMStr = (csvValues.GM < 0 ? "" : csvValues.GM.ToString().Replace(",", "."));
                string MedStr = (csvValues.Med < 0 ? "" : csvValues.Med.ToString().Replace(",", "."));
                string PercOver43Str = (csvValues.PercOver43 < 0 ? "" : csvValues.PercOver43.ToString().Replace(",", "."));
                string PercOver260Str = (csvValues.PercOver260 < 0 ? "" : csvValues.PercOver260.ToString().Replace(",", "."));

                sbCSV.AppendLine($"{ csvValues.Subsector.Replace(",", "_") },{ csvValues.Site.Replace(",", "_") },{ csvValues.StartYear },{ csvValues.EndYear },{ csvValues.statType.ToString() },{ csvValues.ClassStr },{ csvValues.Letter },{ csvValues.NumbSamples },{ P90Str },{ GMStr },{ MedStr },{ PercOver43Str },{ PercOver260Str },{ csvValues.ValueList }");
            }

            FileInfo fi = new FileInfo($@"C:\Users\leblancc\Desktop\StatsWithRain_Full_{ ProvText }.csv");

            StreamWriter sw = fi.CreateText();
            sw.Write(sbCSV.ToString());
            sw.Close();

        }

        private enum StatType
        {
            Run30 = 1,
            Dry = 2,
            Wet = 3,
        }

        private class CSVValues
        {
            public string Subsector { get; set; }
            public string Site { get; set; }
            public int StartYear { get; set; }
            public int EndYear { get; set; }
            public StatType statType { get; set; }
            public string ClassStr { get; set; }
            public string Letter { get; set; }
            public int NumbSamples { get; set; }
            public int P90 { get; set; }
            public int GM { get; set; }
            public int Med { get; set; }
            public int PercOver43 { get; set; }
            public int PercOver260 { get; set; }
            public string ValueList { get; set; }
        }

        private class RainDays
        {
            public DateTime RunDate { get; set; }
            public double R1 { get; set; }
            public double R2 { get; set; }
            public double R3 { get; set; }
            public double R4 { get; set; }
            public double R5 { get; set; }
            public double R6 { get; set; }
            public double R7 { get; set; }
            public double R8 { get; set; }
            public double R9 { get; set; }
            public double R10 { get; set; }
        }

        private void DoAllSites(List<TVItemModel> tvItemModelSSList, TVItemService tvItemService, StringBuilder sb)
        {
            foreach (TVItemModel tvItemModelSS in tvItemModelSSList)
            {
                lblStatus.Text = tvItemModelSS.TVText;
                lblStatus.Refresh();
                Application.DoEvents();

                string TVText = tvItemModelSS.TVText;
                if (TVText.Contains(" "))
                {
                    TVText = TVText.Substring(0, TVText.IndexOf(" "));
                }

                //if (tvItemModelSS.TVItemID != 635)
                //{
                //    continue; // just doing Bouctouche for now
                //}

                List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.MWQMSite).Where(c => c.IsActive == true).ToList();
                List<MapInfo> mapInfoList = new List<MapInfo>();
                List<MapInfoPoint> mapInfoPointList = new List<MapInfoPoint>();

                using (CSSPDBEntities db2 = new CSSPDBEntities())
                {
                    List<int> TVItemMWQMSiteList = tvItemModelMWQMSiteList.Select(c => c.TVItemID).Distinct().ToList();

                    mapInfoList = (from c in db2.MapInfos
                                   from tid in TVItemMWQMSiteList
                                   where c.TVItemID == tid
                                   && c.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
                                   select c).ToList();

                    List<int> mapInfoIDList = mapInfoList.Select(c => c.MapInfoID).Distinct().ToList();

                    mapInfoPointList = (from c in db2.MapInfoPoints
                                        from mid in mapInfoIDList
                                        where c.MapInfoID == mid
                                        select c).ToList();


                }

                sb.AppendLine($@"	    <Folder>");
                sb.AppendLine($@"	    <name>{ tvItemModelSS.TVText }</name>");

                foreach (TVItemModel tvItemModelMWQMSite in tvItemModelMWQMSiteList)
                {

                    if (tvItemModelMWQMSite != null)
                    {
                        MapInfoPoint mapInfoPoint = (from mi in mapInfoList
                                                     from mip in mapInfoPointList
                                                     where mi.MapInfoID == mip.MapInfoID
                                                     && mi.TVItemID == tvItemModelMWQMSite.TVItemID
                                                     select mip).FirstOrDefault();

                        if (mapInfoPoint != null)
                        {
                            sb.AppendLine($@"           <Placemark>");
                            sb.AppendLine($@"	        	<name>{ tvItemModelMWQMSite.TVText }</name>");
                            sb.AppendLine($@"	        	<Point>");
                            sb.AppendLine($@"	        		<coordinates>{ mapInfoPoint.Lng },{ mapInfoPoint.Lat },0</coordinates>");
                            sb.AppendLine($@"	        	</Point>");
                            sb.AppendLine($@"	        </Placemark>");
                        }
                    }
                }

                sb.AppendLine($@"	    </Folder>");
            }
        }
        private void DoAllSubsectorStats(StatType statType, List<TVItemModel> tvItemModelSSList, TVItemService tvItemService, int NumberOfSamples, List<double> DryList, List<double> WetList, List<CSVValues> csvValuesList, List<CSVValues> csvValuesListFull, StringBuilder sb)
        {
            foreach (TVItemModel tvItemModelSS in tvItemModelSSList)
            {
                lblStatus.Text = tvItemModelSS.TVText;
                lblStatus.Refresh();
                Application.DoEvents();

                string TVText = tvItemModelSS.TVText;
                if (TVText.Contains(" "))
                {
                    TVText = TVText.Substring(0, TVText.IndexOf(" "));
                }

                //if (tvItemModelSS.TVItemID != 635)
                //{
                //    continue; // just doing Bouctouche for now
                //}

                List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.MWQMSite).Where(c => c.IsActive == true).ToList();
                List<MapInfo> mapInfoList = new List<MapInfo>();
                List<MapInfoPoint> mapInfoPointList = new List<MapInfoPoint>();
                List<MWQMSample> mwqmSampleListAll = new List<MWQMSample>();
                List<MWQMSample> mwqmSampleListStat = new List<MWQMSample>();
                List<TVItemModel> tvItemModelMWQMRunList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.MWQMRun).Where(c => c.IsActive == true).ToList();
                List<RainDays> RainList = new List<RainDays>();

                using (CSSPDBEntities db2 = new CSSPDBEntities())
                {
                    List<int> TVItemMWQMSiteList = tvItemModelMWQMSiteList.Select(c => c.TVItemID).Distinct().ToList();
                    List<int> TVItemMWQMRunList = tvItemModelMWQMRunList.Select(c => c.TVItemID).Distinct().ToList();

                    List<MWQMRun> mwqmRunList = (from c in db2.MWQMRuns
                                                 from rid in TVItemMWQMRunList
                                                 where c.MWQMRunTVItemID == rid
                                                 && c.RunSampleType == (int)SampleTypeEnum.Routine
                                                 select c).ToList();

                    List<int> TVItemMWQMRunRoutineList = mwqmRunList.Select(c => c.MWQMRunTVItemID).Distinct().ToList();

                    mwqmSampleListAll = (from c in db2.MWQMSamples
                                         from tid in TVItemMWQMSiteList
                                         from rid in TVItemMWQMRunRoutineList
                                         where c.MWQMSiteTVItemID == tid
                                         && c.MWQMRunTVItemID == rid
                                         select c).ToList();

                    mapInfoList = (from c in db2.MapInfos
                                   from tid in TVItemMWQMSiteList
                                   where c.TVItemID == tid
                                   && c.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
                                   select c).ToList();

                    List<int> mapInfoIDList = mapInfoList.Select(c => c.MapInfoID).Distinct().ToList();

                    mapInfoPointList = (from c in db2.MapInfoPoints
                                        from mid in mapInfoIDList
                                        where c.MapInfoID == mid
                                        select c).ToList();

                    if (statType == StatType.Run30)
                    {
                        var mwqmSampleListStatAndRain = (from c in mwqmSampleListAll
                                                         from r in mwqmRunList
                                                         let R1 = r.RainDay1_mm
                                                         let R2 = R1 + r.RainDay2_mm
                                                         let R3 = R2 + r.RainDay3_mm
                                                         let R4 = R3 + r.RainDay4_mm
                                                         let R5 = R4 + r.RainDay5_mm
                                                         let R6 = R5 + r.RainDay6_mm
                                                         let R7 = R6 + r.RainDay7_mm
                                                         let R8 = R7 + r.RainDay8_mm
                                                         let R9 = R8 + r.RainDay9_mm
                                                         let R10 = R9 + r.RainDay10_mm
                                                         where c.MWQMRunTVItemID == r.MWQMRunTVItemID
                                                         select new { c, R1, R2, R3, R4, R5, R6, R7, R8, R9, R10 }).ToList();

                        foreach (var sampleStatAndRain in mwqmSampleListStatAndRain)
                        {
                            mwqmSampleListStat.Add(sampleStatAndRain.c);
                            RainList.Add(new RainDays() { RunDate = sampleStatAndRain.c.SampleDateTime_Local, R1 = (double)(sampleStatAndRain.R1 ?? -1), R2 = (double)(sampleStatAndRain.R2 ?? -1), R3 = (double)(sampleStatAndRain.R3 ?? -1), R4 = (double)(sampleStatAndRain.R4 ?? -1), R5 = (double)(sampleStatAndRain.R5 ?? -1), R6 = (double)(sampleStatAndRain.R6 ?? -1), R7 = (double)(sampleStatAndRain.R7 ?? -1), R8 = (double)(sampleStatAndRain.R8 ?? -1), R9 = (double)(sampleStatAndRain.R9 ?? -1), R10 = (double)(sampleStatAndRain.R10 ?? -1) });
                        }
                    }
                    else if (statType == StatType.Dry)
                    {
                        var mwqmSampleListStatAndRain = (from c in mwqmSampleListAll
                                                         from r in mwqmRunList
                                                         let R1 = r.RainDay1_mm
                                                         let R2 = R1 + r.RainDay2_mm
                                                         let R3 = R2 + r.RainDay3_mm
                                                         let R4 = R3 + r.RainDay4_mm
                                                         let R5 = R4 + r.RainDay5_mm
                                                         let R6 = R5 + r.RainDay6_mm
                                                         let R7 = R6 + r.RainDay7_mm
                                                         let R8 = R7 + r.RainDay8_mm
                                                         let R9 = R8 + r.RainDay9_mm
                                                         let R10 = R9 + r.RainDay10_mm
                                                         where c.MWQMRunTVItemID == r.MWQMRunTVItemID
                                                         && (R1 <= WetList[0]
                                                         || R2 <= WetList[1]
                                                         || R3 <= WetList[2]
                                                         || R4 <= WetList[3])
                                                         select new { c, R1, R2, R3, R4, R5, R6, R7, R8, R9, R10 }).ToList();

                        foreach (var sampleStatAndRain in mwqmSampleListStatAndRain)
                        {
                            mwqmSampleListStat.Add(sampleStatAndRain.c);
                            RainList.Add(new RainDays() { RunDate = sampleStatAndRain.c.SampleDateTime_Local, R1 = (double)(sampleStatAndRain.R1 ?? -1), R2 = (double)(sampleStatAndRain.R2 ?? -1), R3 = (double)(sampleStatAndRain.R3 ?? -1), R4 = (double)(sampleStatAndRain.R4 ?? -1), R5 = (double)(sampleStatAndRain.R5 ?? -1), R6 = (double)(sampleStatAndRain.R6 ?? -1), R7 = (double)(sampleStatAndRain.R7 ?? -1), R8 = (double)(sampleStatAndRain.R8 ?? -1), R9 = (double)(sampleStatAndRain.R9 ?? -1), R10 = (double)(sampleStatAndRain.R10 ?? -1) });
                        }
                    }
                    else if (statType == StatType.Wet)
                    {
                        var mwqmSampleListStatAndRain = (from c in mwqmSampleListAll
                                                         from r in mwqmRunList
                                                         let R1 = r.RainDay1_mm
                                                         let R2 = R1 + r.RainDay2_mm
                                                         let R3 = R2 + r.RainDay3_mm
                                                         let R4 = R3 + r.RainDay4_mm
                                                         let R5 = R4 + r.RainDay5_mm
                                                         let R6 = R5 + r.RainDay6_mm
                                                         let R7 = R6 + r.RainDay7_mm
                                                         let R8 = R7 + r.RainDay8_mm
                                                         let R9 = R8 + r.RainDay9_mm
                                                         let R10 = R9 + r.RainDay10_mm
                                                         where c.MWQMRunTVItemID == r.MWQMRunTVItemID
                                                         && (R1 >= WetList[0]
                                                         || R2 >= WetList[1]
                                                         || R3 >= WetList[2]
                                                         || R4 >= WetList[3])
                                                         select new { c, R1, R2, R3, R4, R5, R6, R7, R8, R9, R10 }).ToList();

                        foreach (var sampleStatAndRain in mwqmSampleListStatAndRain)
                        {
                            mwqmSampleListStat.Add(sampleStatAndRain.c);
                            RainList.Add(new RainDays() { RunDate = sampleStatAndRain.c.SampleDateTime_Local, R1 = (double)(sampleStatAndRain.R1 ?? -1), R2 = (double)(sampleStatAndRain.R2 ?? -1), R3 = (double)(sampleStatAndRain.R3 ?? -1), R4 = (double)(sampleStatAndRain.R4 ?? -1), R5 = (double)(sampleStatAndRain.R5 ?? -1), R6 = (double)(sampleStatAndRain.R6 ?? -1), R7 = (double)(sampleStatAndRain.R7 ?? -1), R8 = (double)(sampleStatAndRain.R8 ?? -1), R9 = (double)(sampleStatAndRain.R9 ?? -1), R10 = (double)(sampleStatAndRain.R10 ?? -1) });
                        }
                    }
                    else
                    {
                        mwqmSampleListStat = new List<MWQMSample>();
                    }

                }

                sb.AppendLine($@"	    <Folder>");
                sb.AppendLine($@"	    <name>{ tvItemModelSS.TVText }</name>");

                foreach (TVItemModel tvItemModelMWQMSite in tvItemModelMWQMSiteList)
                {

                    if (tvItemModelMWQMSite != null)
                    {
                        List<double> mwqmSampleFCList = (from c in mwqmSampleListStat
                                                         where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                         orderby c.SampleDateTime_Local descending
                                                         select (c.FecCol_MPN_100ml < 2 ? 1.9D : (double)c.FecCol_MPN_100ml)
                                                        ).Take(NumberOfSamples).ToList<double>();

                        List<MWQMSample> mwqmSampleList = (from c in mwqmSampleListStat
                                                           where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                           orderby c.SampleDateTime_Local descending
                                                           select c).Take(NumberOfSamples).ToList<MWQMSample>();


                        if (mwqmSampleList.Count >= 3)
                        {

                            double P90 = tvItemService.GetP90(mwqmSampleFCList);
                            double GeoMean = tvItemService.GeometricMean(mwqmSampleFCList);
                            double Median = tvItemService.GetMedian(mwqmSampleFCList);
                            double PercOver43 = ((((double)mwqmSampleList.Where(c => c.FecCol_MPN_100ml > 43).Count()) / (double)mwqmSampleList.Count()) * 100.0D);
                            double PercOver260 = ((((double)mwqmSampleList.Where(c => c.FecCol_MPN_100ml > 260).Count()) / (double)mwqmSampleList.Count()) * 100.0D);
                            int MinYear = mwqmSampleList.Select(c => c.SampleDateTime_Local).Min().Year;
                            int MaxYear = mwqmSampleList.Select(c => c.SampleDateTime_Local).Max().Year;

                            int P90Int = (int)Math.Round((double)P90, 0);
                            int GeoMeanInt = (int)Math.Round((double)GeoMean, 0);
                            int MedianInt = (int)Math.Round((double)Median, 0);
                            int PercOver43Int = (int)Math.Round((double)PercOver43, 0);
                            int PercOver260Int = (int)Math.Round((double)PercOver260, 0);

                            LetterColorName letterColorName = new LetterColorName();
                            string ClassStr = "";
                            if ((GeoMeanInt > 88) || (MedianInt > 88) || (P90Int > 260) || (PercOver260Int > 10))
                            {
                                ClassStr = "NoDep";
                                if ((GeoMeanInt > 181) || (MedianInt > 181) || (P90Int > 460) || (PercOver260Int > 18))
                                {
                                    letterColorName = new LetterColorName() { Letter = "F", Color = "8888ff", Name = "NoDepuration" };
                                }
                                else if ((GeoMeanInt > 163) || (MedianInt > 163) || (P90Int > 420) || (PercOver260Int > 17))
                                {
                                    letterColorName = new LetterColorName() { Letter = "E", Color = "9999ff", Name = "NoDepuration" };
                                }
                                else if ((GeoMeanInt > 144) || (MedianInt > 144) || (P90Int > 380) || (PercOver260Int > 15))
                                {
                                    letterColorName = new LetterColorName() { Letter = "D", Color = "aaaaff", Name = "NoDepuration" };
                                }
                                else if ((GeoMeanInt > 125) || (MedianInt > 125) || (P90Int > 340) || (PercOver260Int > 13))
                                {
                                    letterColorName = new LetterColorName() { Letter = "C", Color = "bbbbff", Name = "NoDepuration" };
                                }
                                else if ((GeoMeanInt > 107) || (MedianInt > 107) || (P90Int > 300) || (PercOver260Int > 12))
                                {
                                    letterColorName = new LetterColorName() { Letter = "B", Color = "ccccff", Name = "NoDepuration" };
                                }
                                else
                                {
                                    letterColorName = new LetterColorName() { Letter = "A", Color = "ddddff", Name = "NoDepuration" };
                                }
                            }
                            else if ((GeoMeanInt > 14) || (MedianInt > 14) || (P90Int > 43) || (PercOver43Int > 10))
                            {
                                ClassStr = "Fail";
                                if ((GeoMeanInt > 76) || (MedianInt > 76) || (P90Int > 224) || (PercOver43Int > 27))
                                {
                                    letterColorName = new LetterColorName() { Letter = "F", Color = "aa0000", Name = "Fail" };
                                }
                                else if ((GeoMeanInt > 63) || (MedianInt > 63) || (P90Int > 188) || (PercOver43Int > 23))
                                {
                                    letterColorName = new LetterColorName() { Letter = "E", Color = "cc0000", Name = "Fail" };
                                }
                                else if ((GeoMeanInt > 51) || (MedianInt > 51) || (P90Int > 152) || (PercOver43Int > 20))
                                {
                                    letterColorName = new LetterColorName() { Letter = "D", Color = "ff1111", Name = "Fail" };
                                }
                                else if ((GeoMeanInt > 39) || (MedianInt > 39) || (P90Int > 115) || (PercOver43Int > 17))
                                {
                                    letterColorName = new LetterColorName() { Letter = "C", Color = "ff4444", Name = "Fail" };
                                }
                                else if ((GeoMeanInt > 26) || (MedianInt > 26) || (P90Int > 79) || (PercOver43Int > 13))
                                {
                                    letterColorName = new LetterColorName() { Letter = "B", Color = "ff9999", Name = "Fail" };
                                }
                                else
                                {
                                    letterColorName = new LetterColorName() { Letter = "A", Color = "ffcccc", Name = "Fail" };
                                }
                            }
                            else
                            {
                                ClassStr = "Pass";
                                if ((GeoMeanInt > 12) || (MedianInt > 12) || (P90Int > 36) || (PercOver43Int > 8))
                                {
                                    letterColorName = new LetterColorName() { Letter = "F", Color = "ccffcc", Name = "Pass" };
                                }
                                else if ((GeoMeanInt > 9) || (MedianInt > 9) || (P90Int > 29) || (PercOver43Int > 7))
                                {
                                    letterColorName = new LetterColorName() { Letter = "E", Color = "99ff99", Name = "Pass" };
                                }
                                else if ((GeoMeanInt > 7) || (MedianInt > 7) || (P90Int > 22) || (PercOver43Int > 5))
                                {
                                    letterColorName = new LetterColorName() { Letter = "D", Color = "44ff44", Name = "Pass" };
                                }
                                else if ((GeoMeanInt > 5) || (MedianInt > 5) || (P90Int > 14) || (PercOver43Int > 3))
                                {
                                    letterColorName = new LetterColorName() { Letter = "C", Color = "11ff11", Name = "Pass" };
                                }
                                else if ((GeoMeanInt > 2) || (MedianInt > 2) || (P90Int > 7) || (PercOver43Int > 2))
                                {
                                    letterColorName = new LetterColorName() { Letter = "B", Color = "00bb00", Name = "Pass" };
                                }
                                else
                                {
                                    letterColorName = new LetterColorName() { Letter = "A", Color = "009900", Name = "Pass" };
                                }
                            }

                            MapInfoPoint mapInfoPoint = (from mi in mapInfoList
                                                         from mip in mapInfoPointList
                                                         where mi.MapInfoID == mip.MapInfoID
                                                         && mi.TVItemID == tvItemModelMWQMSite.TVItemID
                                                         select mip).FirstOrDefault();

                            if (mapInfoPoint != null)
                            {
                                sb.AppendLine($@"           <Placemark>");
                                sb.AppendLine($@"	        	<name></name>");
                                sb.AppendLine($@"	        	<styleUrl>#{ letterColorName.Name }_{ letterColorName.Letter }</styleUrl>");
                                sb.AppendLine($@"	        	<Point>");
                                if (statType == StatType.Dry)
                                {
                                    sb.AppendLine($@"	        		<coordinates>{ mapInfoPoint.Lng - 0.001D },{ mapInfoPoint.Lat - 0.001D },0</coordinates>");
                                }
                                else if (statType == StatType.Wet)
                                {
                                    sb.AppendLine($@"	        		<coordinates>{ mapInfoPoint.Lng + 0.001D },{ mapInfoPoint.Lat + 0.001D },0</coordinates>");
                                }
                                else
                                {
                                    sb.AppendLine($@"	        		<coordinates>{ mapInfoPoint.Lng },{ mapInfoPoint.Lat },0</coordinates>");
                                }
                                sb.AppendLine($@"	        	</Point>");
                                sb.AppendLine($@"	        </Placemark>");
                            }

                            StringBuilder sbValueList = new StringBuilder();
                            StringBuilder sbValueListFull = new StringBuilder();
                            foreach (MWQMSample mwqmSample in mwqmSampleList)
                            {
                                if (statType == StatType.Wet)
                                {
                                    StringBuilder sbRain = new StringBuilder();
                                    StringBuilder sbRainFull = new StringBuilder();
                                    RainDays rainDays = (from c in RainList
                                                         where c.RunDate == mwqmSample.SampleDateTime_Local
                                                         select c).FirstOrDefault();

                                    if (rainDays != null)
                                    {
                                        string R1Str = Math.Round(rainDays.R1, 0).ToString("F0") + "R";
                                        string R2Str = Math.Round(rainDays.R2, 0).ToString("F0") + "R";
                                        string R3Str = Math.Round(rainDays.R3, 0).ToString("F0") + "R";
                                        string R4Str = Math.Round(rainDays.R4, 0).ToString("F0") + "R";
                                        string R5Str = Math.Round(rainDays.R5, 0).ToString("F0") + "R";
                                        string R6Str = Math.Round(rainDays.R6, 0).ToString("F0") + "R";
                                        string R7Str = Math.Round(rainDays.R7, 0).ToString("F0") + "R";
                                        string R8Str = Math.Round(rainDays.R8, 0).ToString("F0") + "R";
                                        string R9Str = Math.Round(rainDays.R9, 0).ToString("F0") + "R";
                                        string R10Str = Math.Round(rainDays.R10, 0).ToString("F0") + "R";

                                        sbRain.Append($"{ R1Str },{ R2Str },{ R3Str },{ R4Str },");
                                        sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },{ sbRain.ToString() },");


                                        sbRainFull.Append($"{ R1Str },{ R2Str },{ R3Str },{ R4Str },{ R5Str },{ R6Str },{ R7Str },{ R8Str },{ R9Str },{ R10Str },");
                                        sbValueListFull.Append($"{ mwqmSample.FecCol_MPN_100ml },{ sbRainFull.ToString() },");
                                    }
                                    else
                                    {
                                        sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                    }
                                }
                                else if (statType == StatType.Dry)
                                {
                                    StringBuilder sbRainFull = new StringBuilder();
                                    RainDays rainDays = (from c in RainList
                                                         where c.RunDate == mwqmSample.SampleDateTime_Local
                                                         select c).FirstOrDefault();

                                    if (rainDays != null)
                                    {
                                        string R1Str = Math.Round(rainDays.R1, 0).ToString("F0") + "R";
                                        string R2Str = Math.Round(rainDays.R2, 0).ToString("F0") + "R";
                                        string R3Str = Math.Round(rainDays.R3, 0).ToString("F0") + "R";
                                        string R4Str = Math.Round(rainDays.R4, 0).ToString("F0") + "R";
                                        string R5Str = Math.Round(rainDays.R5, 0).ToString("F0") + "R";
                                        string R6Str = Math.Round(rainDays.R6, 0).ToString("F0") + "R";
                                        string R7Str = Math.Round(rainDays.R7, 0).ToString("F0") + "R";
                                        string R8Str = Math.Round(rainDays.R8, 0).ToString("F0") + "R";
                                        string R9Str = Math.Round(rainDays.R9, 0).ToString("F0") + "R";
                                        string R10Str = Math.Round(rainDays.R10, 0).ToString("F0") + "R";

                                        sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },");

                                        sbRainFull.Append($"{ R1Str },{ R2Str },{ R3Str },{ R4Str },{ R5Str },{ R6Str },{ R7Str },{ R8Str },{ R9Str },{ R10Str },");
                                        sbValueListFull.Append($"{ mwqmSample.FecCol_MPN_100ml },{ sbRainFull.ToString() },");
                                    }
                                    else
                                    {
                                        sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                        sbValueListFull.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                    }
                                }
                                else if (statType == StatType.Run30)
                                {
                                    StringBuilder sbRainFull = new StringBuilder();
                                    RainDays rainDays = (from c in RainList
                                                         where c.RunDate == mwqmSample.SampleDateTime_Local
                                                         select c).FirstOrDefault();

                                    if (rainDays != null)
                                    {
                                        string R1Str = Math.Round(rainDays.R1, 0).ToString("F0") + "R";
                                        string R2Str = Math.Round(rainDays.R2, 0).ToString("F0") + "R";
                                        string R3Str = Math.Round(rainDays.R3, 0).ToString("F0") + "R";
                                        string R4Str = Math.Round(rainDays.R4, 0).ToString("F0") + "R";
                                        string R5Str = Math.Round(rainDays.R5, 0).ToString("F0") + "R";
                                        string R6Str = Math.Round(rainDays.R6, 0).ToString("F0") + "R";
                                        string R7Str = Math.Round(rainDays.R7, 0).ToString("F0") + "R";
                                        string R8Str = Math.Round(rainDays.R8, 0).ToString("F0") + "R";
                                        string R9Str = Math.Round(rainDays.R9, 0).ToString("F0") + "R";
                                        string R10Str = Math.Round(rainDays.R10, 0).ToString("F0") + "R";

                                        sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },");

                                        sbRainFull.Append($"{ R1Str },{ R2Str },{ R3Str },{ R4Str },{ R5Str },{ R6Str },{ R7Str },{ R8Str },{ R9Str },{ R10Str },");
                                        sbValueListFull.Append($"{ mwqmSample.FecCol_MPN_100ml },{ sbRainFull.ToString() },");
                                    }
                                    else
                                    {
                                        sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                        sbValueListFull.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                    }
                                }
                                else
                                {

                                    sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                    sbValueListFull.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                }
                            }
                            CSVValues csvValues = new CSVValues()
                            {
                                Subsector = TVText,
                                Site = tvItemModelMWQMSite.TVText,
                                StartYear = mwqmSampleList[0].SampleDateTime_Local.Year,
                                EndYear = mwqmSampleList[mwqmSampleList.Count - 1].SampleDateTime_Local.Year,
                                statType = statType,
                                ClassStr = ClassStr,
                                Letter = letterColorName.Letter,
                                NumbSamples = mwqmSampleList.Count,
                                P90 = P90Int,
                                GM = GeoMeanInt,
                                Med = MedianInt,
                                PercOver43 = PercOver43Int,
                                PercOver260 = PercOver260Int,
                                ValueList = sbValueList.ToString(),
                            };

                            csvValuesList.Add(csvValues);

                            CSVValues csvValuesFull = new CSVValues()
                            {
                                Subsector = TVText,
                                Site = tvItemModelMWQMSite.TVText,
                                StartYear = mwqmSampleList[0].SampleDateTime_Local.Year,
                                EndYear = mwqmSampleList[mwqmSampleList.Count - 1].SampleDateTime_Local.Year,
                                statType = statType,
                                ClassStr = ClassStr,
                                Letter = letterColorName.Letter,
                                NumbSamples = mwqmSampleList.Count,
                                P90 = P90Int,
                                GM = GeoMeanInt,
                                Med = MedianInt,
                                PercOver43 = PercOver43Int,
                                PercOver260 = PercOver260Int,
                                ValueList = sbValueListFull.ToString(),
                            };

                            csvValuesListFull.Add(csvValuesFull);
                        }
                        else
                        {
                            StringBuilder sbValueList = new StringBuilder();
                            StringBuilder sbValueListFull = new StringBuilder();
                            foreach (MWQMSample mwqmSample in mwqmSampleList)
                            {
                                if (statType == StatType.Wet)
                                {
                                    StringBuilder sbRain = new StringBuilder();
                                    StringBuilder sbRainFull = new StringBuilder();
                                    RainDays rainDays = (from c in RainList
                                                         where c.RunDate == mwqmSample.SampleDateTime_Local
                                                         select c).FirstOrDefault();

                                    if (rainDays != null)
                                    {
                                        string R1Str = Math.Round(rainDays.R1, 0).ToString("F0") + "R";
                                        string R2Str = Math.Round(rainDays.R2, 0).ToString("F0") + "R";
                                        string R3Str = Math.Round(rainDays.R3, 0).ToString("F0") + "R";
                                        string R4Str = Math.Round(rainDays.R4, 0).ToString("F0") + "R";
                                        string R5Str = Math.Round(rainDays.R5, 0).ToString("F0") + "R";
                                        string R6Str = Math.Round(rainDays.R6, 0).ToString("F0") + "R";
                                        string R7Str = Math.Round(rainDays.R7, 0).ToString("F0") + "R";
                                        string R8Str = Math.Round(rainDays.R8, 0).ToString("F0") + "R";
                                        string R9Str = Math.Round(rainDays.R9, 0).ToString("F0") + "R";
                                        string R10Str = Math.Round(rainDays.R10, 0).ToString("F0") + "R";

                                        sbRain.Append($"{ R1Str },{ R2Str },{ R3Str },{ R4Str },");
                                        sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },{ sbRain.ToString() },");


                                        sbRainFull.Append($"{ R1Str },{ R2Str },{ R3Str },{ R4Str },{ R5Str },{ R6Str },{ R7Str },{ R8Str },{ R9Str },{ R10Str },");
                                        sbValueListFull.Append($"{ mwqmSample.FecCol_MPN_100ml },{ sbRainFull.ToString() },");
                                    }
                                    else
                                    {
                                        sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                    }
                                }
                                else if (statType == StatType.Dry)
                                {
                                    StringBuilder sbRainFull = new StringBuilder();
                                    RainDays rainDays = (from c in RainList
                                                         where c.RunDate == mwqmSample.SampleDateTime_Local
                                                         select c).FirstOrDefault();

                                    if (rainDays != null)
                                    {
                                        string R1Str = Math.Round(rainDays.R1, 0).ToString("F0") + "R";
                                        string R2Str = Math.Round(rainDays.R2, 0).ToString("F0") + "R";
                                        string R3Str = Math.Round(rainDays.R3, 0).ToString("F0") + "R";
                                        string R4Str = Math.Round(rainDays.R4, 0).ToString("F0") + "R";
                                        string R5Str = Math.Round(rainDays.R5, 0).ToString("F0") + "R";
                                        string R6Str = Math.Round(rainDays.R6, 0).ToString("F0") + "R";
                                        string R7Str = Math.Round(rainDays.R7, 0).ToString("F0") + "R";
                                        string R8Str = Math.Round(rainDays.R8, 0).ToString("F0") + "R";
                                        string R9Str = Math.Round(rainDays.R9, 0).ToString("F0") + "R";
                                        string R10Str = Math.Round(rainDays.R10, 0).ToString("F0") + "R";

                                        sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },");

                                        sbRainFull.Append($"{ R1Str },{ R2Str },{ R3Str },{ R4Str },{ R5Str },{ R6Str },{ R7Str },{ R8Str },{ R9Str },{ R10Str },");
                                        sbValueListFull.Append($"{ mwqmSample.FecCol_MPN_100ml },{ sbRainFull.ToString() },");
                                    }
                                    else
                                    {
                                        sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                        sbValueListFull.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                    }
                                }
                                else if (statType == StatType.Run30)
                                {
                                    StringBuilder sbRainFull = new StringBuilder();
                                    RainDays rainDays = (from c in RainList
                                                         where c.RunDate == mwqmSample.SampleDateTime_Local
                                                         select c).FirstOrDefault();

                                    if (rainDays != null)
                                    {
                                        string R1Str = Math.Round(rainDays.R1, 0).ToString("F0") + "R";
                                        string R2Str = Math.Round(rainDays.R2, 0).ToString("F0") + "R";
                                        string R3Str = Math.Round(rainDays.R3, 0).ToString("F0") + "R";
                                        string R4Str = Math.Round(rainDays.R4, 0).ToString("F0") + "R";
                                        string R5Str = Math.Round(rainDays.R5, 0).ToString("F0") + "R";
                                        string R6Str = Math.Round(rainDays.R6, 0).ToString("F0") + "R";
                                        string R7Str = Math.Round(rainDays.R7, 0).ToString("F0") + "R";
                                        string R8Str = Math.Round(rainDays.R8, 0).ToString("F0") + "R";
                                        string R9Str = Math.Round(rainDays.R9, 0).ToString("F0") + "R";
                                        string R10Str = Math.Round(rainDays.R10, 0).ToString("F0") + "R";

                                        sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },");

                                        sbRainFull.Append($"{ R1Str },{ R2Str },{ R3Str },{ R4Str },{ R5Str },{ R6Str },{ R7Str },{ R8Str },{ R9Str },{ R10Str },");
                                        sbValueListFull.Append($"{ mwqmSample.FecCol_MPN_100ml },{ sbRainFull.ToString() },");
                                    }
                                    else
                                    {
                                        sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                        sbValueListFull.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                    }
                                }
                                else
                                {

                                    sbValueList.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                    sbValueListFull.Append($"{ mwqmSample.FecCol_MPN_100ml },");
                                }
                            }
                            CSVValues csvValues = new CSVValues()
                            {
                                Subsector = TVText,
                                Site = tvItemModelMWQMSite.TVText,
                                StartYear = -1,
                                EndYear = -1,
                                statType = statType,
                                ClassStr = "",
                                Letter = "",
                                NumbSamples = mwqmSampleList.Count,
                                P90 = -1,
                                GM = -1,
                                Med = -1,
                                PercOver43 = -1,
                                PercOver260 = -1,
                                ValueList = sbValueList.ToString(),
                            };

                            csvValuesList.Add(csvValues);

                            CSVValues csvValuesFull = new CSVValues()
                            {
                                Subsector = TVText,
                                Site = tvItemModelMWQMSite.TVText,
                                StartYear = -1,
                                EndYear = -1,
                                statType = statType,
                                ClassStr = "",
                                Letter = "",
                                NumbSamples = mwqmSampleList.Count,
                                P90 = -1,
                                GM = -1,
                                Med = -1,
                                PercOver43 = -1,
                                PercOver260 = -1,
                                ValueList = sbValueListFull.ToString(),
                            };

                            csvValuesListFull.Add(csvValuesFull);
                        }
                    }
                }

                sb.AppendLine($@"	    </Folder>");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            return;

            //LanguageEnum lang = LanguageEnum.en;
            //BaseModelService _BaseModelService = new BaseModelService(lang);
            //List<string> startWithList = new List<string>() { "101", "143", "910" };
            //List<PolSourceObsInfoChild> polSourceObsInfoChildList = new List<PolSourceObsInfoChild>();
            //TVItemService tvItemService2 = new TVItemService(lang, user);

            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-CA");

            //_BaseModelService.FillPolSourceObsInfoChild(polSourceObsInfoChildList);

            //int ProvTVItemID = 5; // NB

            //TVItemModel tvItemModelCountry = tvItemService2.GetTVItemModelWithTVItemIDDB(ProvTVItemID);
            //if (!string.IsNullOrWhiteSpace(tvItemModelCountry.Error))
            //{
            //    return;
            //}

            //List<TVItemModel> tvItemModelSSList = tvItemService2.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelCountry.TVItemID, TVTypeEnum.Subsector);

            //foreach (TVItemModel tvItemModelSS in tvItemModelSSList) //.Where(c => c.TVText.CompareTo("NS-16-010-001") >= 0))
            //{
            //    TVItemService tvItemService = new TVItemService(lang, user);
            //    PolSourceSiteService polSourceSiteService = new PolSourceSiteService(lang, user);
            //    PolSourceObservationService polSourceObservationService = new PolSourceObservationService(lang, user);
            //    PolSourceObservationIssueService polSourceObservationIssueService = new PolSourceObservationIssueService(lang, user);

            //    lblStatus.Text = tvItemModelSS.TVText;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    List<PolSourceSiteModel> polSourceSiteModelList = polSourceSiteService.GetPolSourceSiteModelListWithSubsectorTVItemIDDB(tvItemModelSS.TVItemID);

            //    foreach (PolSourceSiteModel polSourceSiteModel in polSourceSiteModelList)
            //    {
            //        PolSourceObservationModel polSourceObservationModel = polSourceObservationService.GetPolSourceObservationModelLatestWithPolSourceSiteIDDB(polSourceSiteModel.PolSourceSiteID);

            //        List<PolSourceObservationIssueModel> polSourceObservationIssueModelList = polSourceObservationIssueService.GetPolSourceObservationIssueModelListWithPolSourceObservationIDDB(polSourceObservationModel.PolSourceObservationID);

            //        if (polSourceObservationIssueModelList.Count > 0)
            //        {
            //            PolSourceObservationIssueModel polSourceObservationIssueModel = polSourceObservationIssueModelList.OrderBy(c => c.Ordinal).FirstOrDefault();

            //            if (polSourceObservationIssueModel == null)
            //            {
            //            }
            //            else
            //            {
            //                List<string> polSourceObsInfoList = polSourceObservationIssueModel.ObservationInfo.Replace(" ", "").Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            //                List<int> polSourceObsInfoIntList = polSourceObsInfoList.Select(c => int.Parse(c)).ToList();

            //                string TVText = "";
            //                for (int i = 0, count = polSourceObsInfoList.Count; i < count; i++)
            //                {
            //                    string StartTxt = polSourceObsInfoList[i].Substring(0, 3);

            //                    if (startWithList.Where(c => c.StartsWith(StartTxt)).Any())
            //                    {
            //                        TVText = TVText.Trim();
            //                        string TempText = _BaseEnumService.GetEnumText_PolSourceObsInfoEnum((PolSourceObsInfoEnum)int.Parse(polSourceObsInfoList[i]));
            //                        if (TempText.IndexOf("|") > 0)
            //                        {
            //                            TempText = TempText.Substring(0, TempText.IndexOf("|")).Trim();
            //                        }
            //                        TVText = TVText + (TVText.Length == 0 ? "" : ", ") + TempText;
            //                    }
            //                }

            //                bool WellFormed = IssueWellFormed(polSourceObsInfoIntList, polSourceObsInfoChildList);
            //                bool Completed = IssueCompleted(polSourceObsInfoIntList, polSourceObsInfoChildList);

            //                string NC = (WellFormed == false || Completed == false ? " (NC) - " : "");
            //                if (lang == LanguageEnum.fr)
            //                {
            //                    NC = NC.Replace("NC", "PC");
            //                }

            //                TVText = "P00000".Substring(0, "P00000".Length - polSourceSiteModel.Site.ToString().Length) + polSourceSiteModel.Site.ToString() + " - " + NC + TVText;

            //                TVItemLanguageModel tvItemLanguageModel = new TVItemLanguageModel();
            //                tvItemLanguageModel.Language = lang;

            //                bool Found = true;
            //                while (Found)
            //                {
            //                    if (TVText.Contains("  "))
            //                    {
            //                        TVText = TVText.Replace("  ", " ");
            //                    }
            //                    else
            //                    {
            //                        Found = false;
            //                    }
            //                }

            //                tvItemLanguageModel.TVText = TVText;
            //                tvItemLanguageModel.TVItemID = polSourceSiteModel.PolSourceSiteTVItemID;

            //                TVItemLanguageModel tvItemLanguageModelRet = tvItemService._TVItemLanguageService.PostUpdateTVItemLanguageDB(tvItemLanguageModel);
            //                if (!string.IsNullOrWhiteSpace(tvItemLanguageModelRet.Error))
            //                {
            //                    richTextBoxStatus.AppendText(tvItemLanguageModelRet.Error + "\r\n");
            //                    return;
            //                }
            //            }
            //        }

            //    }

            //}

            //lblStatus.Text = "done...";
        }

        public bool IssueWellFormed(List<int> polSourceObsInfoIntList, List<PolSourceObsInfoChild> polSourceObsInfoChildList)
        {
            int ChildStart = 0;
            for (int i = 0, count = polSourceObsInfoIntList.Count - 2; i < count; i++)
            {
                if (ChildStart != 0)
                {
                    string obsEnum3Char = polSourceObsInfoIntList[i].ToString().Substring(0, 3);
                    string ChildStart3Char = ChildStart.ToString().Substring(0, 3);
                    if (obsEnum3Char != ChildStart3Char)
                    {
                        return false;
                    }
                }

                PolSourceObsInfoChild polSourceObsInfoChild = polSourceObsInfoChildList.Where(c => c.PolSourceObsInfo == ((PolSourceObsInfoEnum)polSourceObsInfoIntList[i])).FirstOrDefault<PolSourceObsInfoChild>();
                if (polSourceObsInfoChild != null)
                {
                    ChildStart = ((int)polSourceObsInfoChild.PolSourceObsInfoChildStart);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public bool IssueCompleted(List<int> polSourceObsInfoIntList, List<PolSourceObsInfoChild> polSourceObsInfoChildList)
        {

            if (polSourceObsInfoIntList.Count > 0)
            {
                int obsEnumIntLast = polSourceObsInfoIntList[polSourceObsInfoIntList.Count - 1];

                PolSourceObsInfoChild polSourceObsInfoChild = polSourceObsInfoChildList.Where(c => c.PolSourceObsInfo == ((PolSourceObsInfoEnum)obsEnumIntLast)).FirstOrDefault<PolSourceObsInfoChild>();
                if (polSourceObsInfoChild == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ////StringBuilder sb = new StringBuilder();
            //TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            //TVItemLinkService tvItemLinkService = new TVItemLinkService(LanguageEnum.en, user);

            //TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            //if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            //{
            //    return;
            //}

            //string ProvinceName = "Québec";
            //List<TVItemModel> tvItemModelProvinceList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Province);
            //if (tvItemModelProvinceList.Count == 0)
            //{
            //    return;
            //}

            //TVItemModel tvItemModelProvince = new TVItemModel();

            //foreach (TVItemModel tvItemModel in tvItemModelProvinceList)
            //{
            //    if (tvItemModel.TVText == ProvinceName)
            //    {
            //        tvItemModelProvince = tvItemModel;
            //        break;
            //    }
            //}

            //List<TVItemModel> tvItemModelMunicipalitiesList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProvince.TVItemID, TVTypeEnum.Municipality);

            //foreach (TVItemModel tvItemModelMuni in tvItemModelMunicipalitiesList)
            //{
            //    //if (tvItemModelMuni.TVText == "Bouctouche")
            //    //{
            //        lblStatus.Text = $"Doing - { tvItemModelMuni.TVText }";
            //        lblStatus.Refresh();
            //        Application.DoEvents();

            //        string OldPath = tvItemModelMuni.TVPath;
            //        string NewPath = OldPath.Replace(OldPath, tvItemModelProvince.TVPath + "p" + tvItemModelMuni.TVItemID);
            //        if (OldPath == NewPath)
            //        {
            //            continue;
            //        }
            //        int OldLevel = tvItemModelMuni.TVLevel;
            //        int NewLevel = tvItemModelProvince.TVLevel + 1;
            //        int DeltaLevel = OldLevel - NewLevel;

            //        List<TVItemModel> tvItemModelParents = tvItemService.GetParentsTVItemModelList(tvItemModelMuni.TVPath);

            //        TVItemModel tvItemModelSubsector = new TVItemModel();

            //        foreach (TVItemModel tvItemModelss in tvItemModelParents)
            //        {
            //            if (tvItemModelss.TVType == TVTypeEnum.Subsector)
            //            {
            //                tvItemModelSubsector = tvItemModelss;
            //                break;
            //            }
            //        }

            //        TVItemLinkModel tvItemLinkModelMuniSS = tvItemLinkService.GetTVItemLinkModelWithFromTVItemIDAndToTVItemIDDB(tvItemModelSubsector.TVItemID, tvItemModelMuni.TVItemID);
            //        if (!string.IsNullOrWhiteSpace(tvItemLinkModelMuniSS.Error))
            //        {
            //            TVItemLinkModel tvItemLinkModelNew = new TVItemLinkModel()
            //            {
            //                FromTVItemID = tvItemModelSubsector.TVItemID,
            //                ToTVItemID = tvItemModelMuni.TVItemID,
            //                FromTVType = tvItemModelSubsector.TVType,
            //                ToTVType = tvItemModelMuni.TVType,
            //                FromTVText = tvItemModelSubsector.TVText,
            //                ToTVText = tvItemModelMuni.TVText,
            //                Ordinal = 0,
            //                ParentTVItemLinkID = null,
            //                StartDateTime_Local = DateTime.Now,
            //                EndDateTime_Local = DateTime.Now.AddDays(1),
            //                TVLevel = tvItemModelProvince.TVLevel + 1,
            //                TVPath = NewPath
            //            };

            //            TVItemLinkModel tvItemLinkModelRet = tvItemLinkService.PostAddTVItemLinkDB(tvItemLinkModelNew);
            //            if (!string.IsNullOrWhiteSpace(tvItemLinkModelRet.Error))
            //            {
            //                richTextBoxStatus.AppendText(tvItemLinkModelRet.Error + "\r\n");
            //                //return;
            //            }

            //            //sb.AppendLine($"New Link \t{ tvItemLinkModelNew.FromTVItemID }\t{ tvItemLinkModelNew.ToTVItemID }\t{ tvItemLinkModelNew.FromTVType }\t{ tvItemLinkModelNew.ToTVType }\t{ tvItemLinkModelNew.FromTVText }\t{ tvItemLinkModelNew.ToTVText }\t");
            //        }


            //        //sb.AppendLine($"{ OldPath }\t{ NewPath }\t{ OldLevel }\t{ NewLevel }");

            //        using (CSSPDBEntities db2 = new CSSPDBEntities())
            //        {
            //            List<TVItem> tvItemList = (from c in db2.TVItems
            //                                       where c.TVPath.StartsWith(OldPath)
            //                                       orderby c.TVLevel
            //                                       select c).ToList();

            //            foreach (TVItem tvItem in tvItemList)
            //            {
            //                lblStatus.Text = $"Doing - { tvItemModelMuni.TVText } ---- { tvItem.TVItemID } --- { tvItem.TVType }";
            //                lblStatus.Refresh();
            //                Application.DoEvents();

            //                string NewPath2 = "";
            //                if (tvItem.TVItemID == tvItemModelMuni.TVItemID)
            //                {
            //                    NewPath2 = NewPath;
            //                }
            //                else
            //                {
            //                    NewPath2 = tvItem.TVPath.Replace(OldPath, NewPath);
            //                }

            //                int NewLevel2 = tvItem.TVLevel - DeltaLevel;
            //                //sb.AppendLine($"{ tvItem.TVPath }\t{ NewPath2 }\t{ tvItem.TVLevel }\t{ NewLevel2 }");
            //                tvItem.TVPath = NewPath2;
            //                tvItem.TVLevel = NewLevel2;

            //                if (tvItemModelMuni.TVItemID == tvItem.TVItemID)
            //                {
            //                    //sb.AppendLine($"{ tvItem.ParentID }\t{ tvItemModelProvince.TVItemID }");
            //                    tvItem.ParentID = tvItemModelProvince.TVItemID;
            //                }
            //            }
            //            //sb.AppendLine();
            //            //sb.AppendLine();

            //            try
            //            {
            //                lblStatus.Text = $"Doing - { tvItemModelMuni.TVText } ---- Saving all changes";
            //                lblStatus.Refresh();
            //                Application.DoEvents();
            //                db2.SaveChanges();
            //            }
            //            catch (Exception ex)
            //            {
            //                string Inner = ex.InnerException != null ? ex.InnerException.Message : "";
            //                //sb.AppendLine($"ERROR: { ex.Message } --- InnerException: { Inner }");
            //                richTextBoxStatus.AppendText($"ERROR: { ex.Message } --- InnerException: { Inner }" + "\r\n");
            //                return;
            //            }
            //        }
            //    //}
            //}

            //lblStatus.Text = $"Done...";
            //lblStatus.Refresh();
            //Application.DoEvents();

            ////richTextBoxStatus.AppendText(sb.ToString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            //MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            //MapInfoPointService mapInfoPointService = new MapInfoPointService(LanguageEnum.en, user);
            //TideSiteService tideSiteService = new TideSiteService(LanguageEnum.en, user);

            //TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            //if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            //{
            //    return;
            //}

            //string ProvinceName = "British Columbia";
            //string ProvinceInit = "BC";
            //List<TVItemModel> tvItemModelProvinceList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Province);
            //if (tvItemModelProvinceList.Count == 0)
            //{
            //    return;
            //}

            //TVItemModel tvItemModelProvince = new TVItemModel();

            //foreach (TVItemModel tvItemModel in tvItemModelProvinceList)
            //{
            //    if (tvItemModel.TVText == ProvinceName)
            //    {
            //        tvItemModelProvince = tvItemModel;
            //        break;
            //    }
            //}

            //List<TideLocation> TideLocationList = new List<TideLocation>();
            //using (CSSPDBEntities db2 = new CSSPDBEntities())
            //{

            //    TideLocationList = (from c in db2.TideLocations
            //                        where c.Prov == ProvinceInit
            //                        select c).ToList();
            //}

            //foreach (TideLocation tideLocation in TideLocationList)
            //{
            //    lblStatus.Text = tideLocation.Name;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    TVItemModel tvItemModelExist = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelProvince.TVItemID, tideLocation.Name, TVTypeEnum.TideSite);
            //    if (!string.IsNullOrWhiteSpace(tvItemModelExist.Error))
            //    {
            //        tvItemModelExist = tvItemService.PostAddChildTVItemDB(tvItemModelProvince.TVItemID, tideLocation.Name, TVTypeEnum.TideSite);
            //        if (!string.IsNullOrWhiteSpace(tvItemModelExist.Error))
            //        {
            //            richTextBoxStatus.AppendText($"Could not create of get the TVItem with TVText = { tideLocation.Name }");
            //            return;
            //        }
            //    }

            //    List<MapInfoPointModel> mapInfoPointModelList = mapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelExist.TVItemID, TVTypeEnum.TideSite, MapInfoDrawTypeEnum.Point);
            //    if (mapInfoPointModelList.Count == 0)
            //    {
            //        List<Coord> coordList = new List<Coord>()
            //                    {
            //                        new Coord() { Lat = (float)tideLocation.Lat, Lng = (float)tideLocation.Lng, Ordinal = 0 }
            //                    };
            //        MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.TideSite, tvItemModelExist.TVItemID);
            //        if (!string.IsNullOrWhiteSpace(mapInfoModelRet.Error))
            //        {
            //            richTextBoxStatus.AppendText($"Could not create MapInfo TVItem with TVText = { tideLocation.Name }");
            //            return;
            //        }
            //    }

            //    TideSiteModel tideSiteModel = new TideSiteModel()
            //    {
            //        Province = tideLocation.Prov,
            //        sid = tideLocation.sid,
            //        Zone = tideLocation.Zone,
            //        TideSiteTVItemID = tvItemModelExist.TVItemID,
            //        TideSiteName = tideLocation.Name,
            //    };

            //    TideSiteModel tideSiteModelExist = tideSiteService.GetTideSiteModelExistDB(tideSiteModel);
            //    if (!string.IsNullOrWhiteSpace(tideSiteModelExist.Error))
            //    {
            //        TideSiteModel tideSiteModelRet = tideSiteService.PostAddTideSiteDB(tideSiteModel);
            //        if (!string.IsNullOrWhiteSpace(tideSiteModelRet.Error))
            //        {
            //            richTextBoxStatus.AppendText($"Could not create TideSite TVItem with TVText = { tideLocation.Name }");
            //            return;
            //        }
            //    }
            //}

            //lblStatus.Text = "Done...";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

            //TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            //if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            //{
            //    return;
            //}

            //string ProvinceName = "New Brunswick";
            //List<TVItemModel> tvItemModelProvinceList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Province);
            //if (tvItemModelProvinceList.Count == 0)
            //{
            //    return;
            //}

            //TVItemModel tvItemModelProvince = new TVItemModel();

            //foreach (TVItemModel tvItemModel in tvItemModelProvinceList)
            //{
            //    if (tvItemModel.TVText == ProvinceName)
            //    {
            //        tvItemModelProvince = tvItemModel;
            //        break;
            //    }
            //}

            //List<TVItemModel> tvItemModelMunicipalityList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProvince.TVItemID, TVTypeEnum.Municipality);
            //List<TVItemModel> tvItemModelInfrastructureList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProvince.TVItemID, TVTypeEnum.Infrastructure);

            //foreach (TVItemModel tvItemModelMuni in tvItemModelMunicipalityList)
            //{
            //    lblStatus.Text = $"{ tvItemModelMuni.TVText }";
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    if (!tvItemModelInfrastructureList.Where(c => c.TVPath.StartsWith(tvItemModelMuni.TVPath + "p")).Any())
            //    {
            //        // municipality does not have infrastructure so make it inactive
            //        tvItemModelMuni.IsActive = false;

            //        TVItemModel tvItemModelRet = tvItemService.PostUpdateTVItemDB(tvItemModelMuni);
            //        if (!string.IsNullOrWhiteSpace(tvItemModelRet.Error))
            //        {


            //            richTextBoxStatus.AppendText($"Could not change { tvItemModelMuni.TVText }\r\n");
            //            return;
            //        }

            //    }
            //}

            //lblStatus.Text = "Done...";

        }

        private void button6_Click(object sender, EventArgs e)
        {
            richTextBoxStatus.AppendText(Math.Atan2(1, 1.1) * 180 / Math.PI + "\r\n");
            richTextBoxStatus.AppendText(Math.Atan2(-1, 1.1) * 180 / Math.PI + "\r\n");
            richTextBoxStatus.AppendText(Math.Atan2(-1, -1.1) * 180 / Math.PI + "\r\n");
            richTextBoxStatus.AppendText(Math.Atan2(1, -1.1) * 180 / Math.PI + "\r\n");

        }

        private void button7_Click(object sender, EventArgs e)
        {
            for (int year = 2007; year < 2019; year++)
            {
                for (int month = 1; month < 13; month++)
                {
                    string url = "http://climate.weather.gc.ca/climate_data/bulk_data_e.html?format=csv&stationID=43383&Year=YYYYY&Month=MMMMM&timeframe=1&submit=Download+Data";

                    url = url.Replace("YYYYY", year.ToString()).Replace("MMMMM", month.ToString());

                    using (WebClient webClient = new WebClient())
                    {
                        lblStatus.Text = "Year " + year + " Month + " + month;
                        lblStatus.Refresh();
                        Application.DoEvents();

                        WebProxy webProxy = new WebProxy();
                        webClient.Proxy = webProxy;
                        string ret = webClient.DownloadString(new Uri(url));
                        if (ret.Length > 0)
                        {
                            FileInfo fi = new FileInfo(@"C:\___Sam\StStephen_" + year + "_" + (month < 10 ? ("0" + month.ToString()) : month.ToString()) + ".csv");
                            StreamWriter sw = fi.CreateText();
                            sw.Write(ret);
                            sw.Close();
                        }
                        else
                        {
                            richTextBoxStatus.AppendText("Error: could not load [" + url + "]\r\n");
                        }
                    }

                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            FileInfo fi = new FileInfo(@"C:\Users\leblancc\Desktop\MunicipalityWWTPAndLS_NB.kml");

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                lblStatus.Text = "Could not find Root";
                return;
            }

            string prov = "New Brunswick";

            TVItemModel tvItemModelNB = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, prov, TVTypeEnum.Province);
            if (!string.IsNullOrWhiteSpace(tvItemModelNB.Error))
            {
                lblStatus.Text = $"Could not find Province {textBoxProvinceName.Text}";
                return;
            }

            List<TVItemModel> tvItemModelMuniList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelNB.TVItemID, TVTypeEnum.Municipality);

            sb.AppendLine($@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine($@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine($@"<Document>");
            sb.AppendLine($@"	<name>NB Municipalities WWTP and LS</name>");
            sb.AppendLine($@"	<Style id=""sh_placemark_circle_highlight"">");
            sb.AppendLine($@"		<IconStyle>");
            sb.AppendLine($@"			<scale>0.945455</scale>");
            sb.AppendLine($@"			<Icon>");
            sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_circle_highlight.png</href>");
            sb.AppendLine($@"			</Icon>");
            sb.AppendLine($@"		</IconStyle>");
            sb.AppendLine($@"		<LabelStyle>");
            sb.AppendLine($@"			<scale>0.8</scale>");
            sb.AppendLine($@"		</LabelStyle>");
            sb.AppendLine($@"		<BalloonStyle>");
            sb.AppendLine($@"		</BalloonStyle>");
            sb.AppendLine($@"		<ListStyle>");
            sb.AppendLine($@"		</ListStyle>");
            sb.AppendLine($@"	</Style>");
            sb.AppendLine($@"	<Style id=""sn_placemark_square"">");
            sb.AppendLine($@"		<IconStyle>");
            sb.AppendLine($@"			<scale>0.8</scale>");
            sb.AppendLine($@"			<Icon>");
            sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_square.png</href>");
            sb.AppendLine($@"			</Icon>");
            sb.AppendLine($@"		</IconStyle>");
            sb.AppendLine($@"		<LabelStyle>");
            sb.AppendLine($@"			<scale>0.8</scale>");
            sb.AppendLine($@"		</LabelStyle>");
            sb.AppendLine($@"		<BalloonStyle>");
            sb.AppendLine($@"		</BalloonStyle>");
            sb.AppendLine($@"		<ListStyle>");
            sb.AppendLine($@"		</ListStyle>");
            sb.AppendLine($@"	</Style>");
            sb.AppendLine($@"	<Style id=""sn_placemark_circle"">");
            sb.AppendLine($@"		<IconStyle>");
            sb.AppendLine($@"			<scale>0.8</scale>");
            sb.AppendLine($@"			<Icon>");
            sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_circle.png</href>");
            sb.AppendLine($@"			</Icon>");
            sb.AppendLine($@"		</IconStyle>");
            sb.AppendLine($@"		<LabelStyle>");
            sb.AppendLine($@"			<scale>0.8</scale>");
            sb.AppendLine($@"		</LabelStyle>");
            sb.AppendLine($@"		<BalloonStyle>");
            sb.AppendLine($@"		</BalloonStyle>");
            sb.AppendLine($@"		<ListStyle>");
            sb.AppendLine($@"		</ListStyle>");
            sb.AppendLine($@"	</Style>");
            sb.AppendLine($@"	<StyleMap id=""msn_placemark_circle"">");
            sb.AppendLine($@"		<Pair>");
            sb.AppendLine($@"			<key>normal</key>");
            sb.AppendLine($@"			<styleUrl>#sn_placemark_circle</styleUrl>");
            sb.AppendLine($@"		</Pair>");
            sb.AppendLine($@"		<Pair>");
            sb.AppendLine($@"			<key>highlight</key>");
            sb.AppendLine($@"			<styleUrl>#sh_placemark_circle_highlight</styleUrl>");
            sb.AppendLine($@"		</Pair>");
            sb.AppendLine($@"	</StyleMap>");
            sb.AppendLine($@"	<StyleMap id=""msn_placemark_square"">");
            sb.AppendLine($@"		<Pair>");
            sb.AppendLine($@"			<key>normal</key>");
            sb.AppendLine($@"			<styleUrl>#sn_placemark_square</styleUrl>");
            sb.AppendLine($@"		</Pair>");
            sb.AppendLine($@"		<Pair>");
            sb.AppendLine($@"			<key>highlight</key>");
            sb.AppendLine($@"			<styleUrl>#sh_placemark_square_highlight</styleUrl>");
            sb.AppendLine($@"		</Pair>");
            sb.AppendLine($@"	</StyleMap>");
            sb.AppendLine($@"	<Style id=""sh_placemark_square_highlight"">");
            sb.AppendLine($@"		<IconStyle>");
            sb.AppendLine($@"			<scale>0.945455</scale>");
            sb.AppendLine($@"			<Icon>");
            sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_square_highlight.png</href>");
            sb.AppendLine($@"			</Icon>");
            sb.AppendLine($@"		</IconStyle>");
            sb.AppendLine($@"		<LabelStyle>");
            sb.AppendLine($@"			<scale>0.8</scale>");
            sb.AppendLine($@"		</LabelStyle>");
            sb.AppendLine($@"		<BalloonStyle>");
            sb.AppendLine($@"		</BalloonStyle>");
            sb.AppendLine($@"		<ListStyle>");
            sb.AppendLine($@"		</ListStyle>");
            sb.AppendLine($@"	</Style>");

            using (CSSPDBEntities db2 = new CSSPDBEntities())
            {
                var infMapInfoList = (from c in db2.TVItems
                                      from t in db2.TVItemLanguages
                                      from mi in db2.MapInfos
                                      from mip in db2.MapInfoPoints
                                      from inf in db2.Infrastructures
                                      where c.TVItemID == t.TVItemID
                                      && t.Language == (int)LanguageEnum.en
                                      && c.TVItemID == inf.InfrastructureTVItemID
                                      && c.TVType == (int)TVTypeEnum.Infrastructure
                                      && c.TVItemID == mi.TVItemID
                                      && mi.MapInfoID == mip.MapInfoID
                                      && mi.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
                                      && c.TVPath.StartsWith(tvItemModelNB.TVPath + "p")
                                      && mi.TVType != (int)TVTypeEnum.Outfall
                                      select new { c, t, mi, mip, inf }).ToList();

                foreach (TVItemModel tvItemModel in tvItemModelMuniList)
                {


                    var InfMapInfoList2 = (from c in infMapInfoList
                                           where c.c.ParentID == tvItemModel.TVItemID
                                           select c).ToList();

                    if (InfMapInfoList2.Count > 0)
                    {
                        sb.AppendLine($@"	<Folder>");
                        sb.AppendLine($@"		<name>{ tvItemModel.TVText }</name>");


                        foreach (var a in InfMapInfoList2)
                        {
                            if (a.mip.Lat > 0 && a.mip.Lng < 0)
                            {
                                sb.AppendLine($@"		<Placemark>");
                                sb.AppendLine($@"			<name>{a.t.TVText}</name>");
                                if (a.inf.InfrastructureType == (int)InfrastructureTypeEnum.WWTP)
                                {
                                    sb.AppendLine($@"			<styleUrl>#msn_placemark_circle</styleUrl>");
                                }
                                else if (a.inf.InfrastructureType == (int)InfrastructureTypeEnum.LiftStation)
                                {
                                    sb.AppendLine($@"			<styleUrl>#msn_placemark_square</styleUrl>");
                                }
                                else if (a.inf.InfrastructureType == (int)InfrastructureTypeEnum.LineOverflow)
                                {
                                    sb.AppendLine($@"			<styleUrl>#msn_placemark_square</styleUrl>");
                                }
                                sb.AppendLine($@"			<Point>");
                                sb.AppendLine($@"				<coordinates>{a.mip.Lng},{a.mip.Lat},0</coordinates>");
                                sb.AppendLine($@"			</Point>");
                                sb.AppendLine($@"		</Placemark>");
                            }
                        }
                        sb.AppendLine($@"	</Folder>");
                    }

                }
            }

            sb.AppendLine($@"</Document>");
            sb.AppendLine($@"</kml>");

            StreamWriter sw = fi.CreateText();
            sw.Write(sb.ToString());
            sw.Close();

            lblStatus.Text = "done...";
        }

        private void butCalculateMWQMSiteVariability_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\t\tEvery Year\t\t\t\t\t\t\t\t\t\tEvery 2 Year Starting in 2018\t\t\t\t\t\t\t\t\t\tEvery 2 Year Starting in 2017\t\t\t\t\t\t\t\t\t\tEvery 2 Sample Starting with First\t\t\t\t\t\t\t\t\t\tEvery 2 Sample Starting with Second\t\t\t\t\t\t\t\t\t");
            sb.AppendLine("Subsector\tSite\t#Samples\tEnd Year\tGMean\tMed\tP90\t%>43\tLetter\tColor\tDataText\t\t#Samples\tEnd Year\tGMean\tMed\tP90\t%>43\tLetter\tColor\tDataText\t\t#Samples\tEnd Year\tGMean\tMed\tP90\t%>43\tLetter\tColor\tDataText\t\t#Samples\tEnd Year\tGMean\tMed\tP90\t%>43\tLetter\tColor\tDataText\t\t#Samples\tEnd Year\tGMean\tMed\tP90\t%>43\tLetter\tColor\tDataText\t");

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                lblStatus.Text = "Could not find Root";
                return;
            }

            TVItemModel tvItemModelNB = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, textBoxProvinceName.Text, TVTypeEnum.Province);
            if (!string.IsNullOrWhiteSpace(tvItemModelNB.Error))
            {
                lblStatus.Text = $"Could not find Province {textBoxProvinceName.Text}";
                return;
            }

            List<TVItemModel> tvitemModelSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelNB.TVItemID, TVTypeEnum.Subsector);

            foreach (TVItemModel tvItemModelSS in tvitemModelSSList)
            {
                lblStatus.Text = tvItemModelSS.TVText;
                lblStatus.Refresh();
                Application.DoEvents();

                if (!tvItemModelSS.TVText.StartsWith("NB-06-020-002"))
                {
                    //continue;
                }

                List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.MWQMSite).Where(c => c.IsActive == true).ToList();
                List<MWQMSample> mwqmSampleListAll = new List<MWQMSample>();
                List<MWQMSample> mwqmSampleListStatEveryYear = new List<MWQMSample>();
                List<MWQMSample> mwqmSampleListStatEvery2Year = new List<MWQMSample>();
                List<MWQMSample> mwqmSampleListStatEvery3Year = new List<MWQMSample>();
                List<MWQMSample> mwqmSampleListStatEvery2Samples = new List<MWQMSample>();
                List<MWQMSample> mwqmSampleListStatEvery3Samples = new List<MWQMSample>();


                using (CSSPDBEntities db2 = new CSSPDBEntities())
                {
                    List<int> TVItemMWQMSiteList = tvItemModelMWQMSiteList.Select(c => c.TVItemID).Distinct().ToList();

                    mwqmSampleListAll = (from c in db2.MWQMSamples
                                         from tid in TVItemMWQMSiteList
                                         orderby c.SampleDateTime_Local descending
                                         where c.MWQMSiteTVItemID == tid
                                         && c.SampleTypesText.Contains("109,")
                                         select c).ToList();


                    mwqmSampleListStatEveryYear = (from c in mwqmSampleListAll
                                                   select c).ToList();

                    if (mwqmSampleListAll.Count == 0)
                    {
                        continue;
                    }

                    foreach (TVItemModel tvItemModelMWQMSite in tvItemModelMWQMSiteList)
                    {
                        string EveryYear = "";
                        string Every2YearStartingIn2018 = "";
                        string Every2YearStartingIn2017 = "";
                        string Every2SampleStartingWithFirst = "";
                        string Every2SampleStartingWithSecond = "";

                        if (tvItemModelMWQMSite != null)
                        {
                            #region EveryYear
                            // ---------------------------------------------------------------------------------------
                            // -------------------------------------  every year -------------------------------------
                            // ---------------------------------------------------------------------------------------


                            List<MWQMSample> mwqmSampleListFull = (from c in mwqmSampleListStatEveryYear
                                                                   where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                                   orderby c.SampleDateTime_Local descending
                                                                   select c).ToList<MWQMSample>();

                            List<MWQMSample> mwqmSampleList = new List<MWQMSample>();

                            if (mwqmSampleListFull.Count == 0)
                            {
                                continue;
                            }
                            List<int> YearWithData = mwqmSampleListFull.Select(c => c.SampleDateTime_Local.Year).Distinct().OrderByDescending(c => c).ToList();

                            int StartYear = YearWithData[0];
                            int ToYear = 0;
                            int oldYear = YearWithData[0];
                            foreach (int year in YearWithData)
                            {
                                if (year < oldYear)
                                {
                                    if (year != (oldYear - 1))
                                    {
                                        ToYear = oldYear;
                                    }
                                    else
                                    {
                                        oldYear = year;
                                    }
                                }
                            }

                            int CurrentYear = StartYear;
                            for (int i = 0, count = mwqmSampleListFull.Count(); i < count; i++)
                            {
                                if (mwqmSampleListFull[i].SampleDateTime_Local.Year < ToYear)
                                {
                                    break;
                                }
                                mwqmSampleList.Add(mwqmSampleListFull[i]);
                            }

                            bool DoSite = mwqmSampleList.Count >= 10 ? true : false;

                            if (mwqmSampleList.Count >= 10)
                            {
                                List<double> mwqmSampleFCList = (from c in mwqmSampleList
                                                                 where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                                 orderby c.SampleDateTime_Local descending
                                                                 select (c.FecCol_MPN_100ml < 2 ? 1.9D : (double)c.FecCol_MPN_100ml)).ToList<double>();

                                double P90 = tvItemService.GetP90(mwqmSampleFCList);
                                double GeoMean = tvItemService.GeometricMean(mwqmSampleFCList);
                                double Median = tvItemService.GetMedian(mwqmSampleFCList);
                                double PercOver43 = ((((double)mwqmSampleList.Where(c => c.FecCol_MPN_100ml > 43).Count()) / (double)mwqmSampleList.Count()) * 100.0D);
                                double PercOver260 = ((((double)mwqmSampleList.Where(c => c.FecCol_MPN_100ml > 260).Count()) / (double)mwqmSampleList.Count()) * 100.0D);
                                int MinYear = mwqmSampleList.Select(c => c.SampleDateTime_Local).Min().Year;

                                int P90Int = (int)Math.Round((double)P90, 0);
                                int GeoMeanInt = (int)Math.Round((double)GeoMean, 0);
                                int MedianInt = (int)Math.Round((double)Median, 0);
                                int PercOver43Int = (int)Math.Round((double)PercOver43, 0);
                                int PercOver260Int = (int)Math.Round((double)PercOver260, 0);

                                LetterColorName letterColorName = GetLetterColorName(P90Int, GeoMeanInt, MedianInt, PercOver43Int, PercOver260Int);

                                string Subsector = tvItemModelSS.TVText.Substring(0, tvItemModelSS.TVText.IndexOf(" "));
                                string Site = tvItemModelMWQMSite.TVText;
                                string SamplesCount = mwqmSampleList.Count.ToString();
                                string EndYear = MinYear.ToString();
                                string GMeanText = GeoMean.ToString("F1");
                                string MedText = Median.ToString("F1");
                                string P90Text = P90.ToString("F1");
                                string PerOver43Text = PercOver43.ToString("F1");
                                string DataText = String.Join("|", mwqmSampleList.Select(c => c.FecCol_MPN_100ml));
                                EveryYear = $"{ Subsector }\t{Site}\t{SamplesCount}\t{EndYear}\t{GMeanText}\t{MedText}\t{P90Text}\t{PerOver43Text}\t{letterColorName.Letter}\t{letterColorName.Name}\t{DataText}\t";
                            }
                            #endregion EveryYear

                            #region Every2YearStartingIn2018
                            // ---------------------------------------------------------------------------------------
                            // ------------------------------------ every 2 years Starting in 2018 -------------------
                            // ---------------------------------------------------------------------------------------

                            mwqmSampleListFull = (from c in mwqmSampleListStatEveryYear
                                                  where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                  orderby c.SampleDateTime_Local descending
                                                  select c).ToList<MWQMSample>();

                            mwqmSampleList = new List<MWQMSample>();

                            if (mwqmSampleListFull.Count >= 10 && DoSite && mwqmSampleListFull.Count > 0)
                            {
                                YearWithData = mwqmSampleListFull.Select(c => c.SampleDateTime_Local.Year).Distinct().OrderByDescending(c => c).ToList();

                                StartYear = YearWithData[0]; // StartYear == 2018 because data is [2018, 2017...]
                                CurrentYear = StartYear;
                                for (int i = 0, count = mwqmSampleListFull.Count(); i < count; i++)
                                {
                                    if (mwqmSampleListFull[i].SampleDateTime_Local.Year == CurrentYear)
                                    {
                                        mwqmSampleList.Add(mwqmSampleListFull[i]);
                                    }
                                    else
                                    {
                                        if (mwqmSampleListFull[i].SampleDateTime_Local.Year == CurrentYear - 1)
                                        {
                                            if (CurrentYear <= ToYear)
                                            {
                                                break;
                                            }
                                            CurrentYear = CurrentYear - 2;
                                        }
                                    }
                                }

                                if (mwqmSampleList.Count >= 10 && DoSite)
                                {
                                    List<double> mwqmSampleFCList = (from c in mwqmSampleList
                                                                     where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                                     orderby c.SampleDateTime_Local descending
                                                                     select (c.FecCol_MPN_100ml < 2 ? 1.9D : (double)c.FecCol_MPN_100ml)).ToList<double>();

                                    double P90 = tvItemService.GetP90(mwqmSampleFCList);
                                    double GeoMean = tvItemService.GeometricMean(mwqmSampleFCList);
                                    double Median = tvItemService.GetMedian(mwqmSampleFCList);
                                    double PercOver43 = ((((double)mwqmSampleList.Where(c => c.FecCol_MPN_100ml > 43).Count()) / (double)mwqmSampleList.Count()) * 100.0D);
                                    double PercOver260 = ((((double)mwqmSampleList.Where(c => c.FecCol_MPN_100ml > 260).Count()) / (double)mwqmSampleList.Count()) * 100.0D);
                                    int MinYear = mwqmSampleList.Select(c => c.SampleDateTime_Local).Min().Year;

                                    int P90Int = (int)Math.Round((double)P90, 0);
                                    int GeoMeanInt = (int)Math.Round((double)GeoMean, 0);
                                    int MedianInt = (int)Math.Round((double)Median, 0);
                                    int PercOver43Int = (int)Math.Round((double)PercOver43, 0);
                                    int PercOver260Int = (int)Math.Round((double)PercOver260, 0);

                                    LetterColorName letterColorName = GetLetterColorName(P90Int, GeoMeanInt, MedianInt, PercOver43Int, PercOver260Int);

                                    string SamplesCount = mwqmSampleList.Count.ToString();
                                    string EndYear = MinYear.ToString();
                                    string GMeanText = GeoMean.ToString("F1");
                                    string MedText = Median.ToString("F1");
                                    string P90Text = P90.ToString("F1");
                                    string PerOver43Text = PercOver43.ToString("F1");
                                    string DataText = String.Join("|", mwqmSampleList.Select(c => c.FecCol_MPN_100ml));
                                    Every2YearStartingIn2018 = $"\t{SamplesCount}\t{EndYear}\t{GMeanText}\t{MedText}\t{P90Text}\t{PerOver43Text}\t{letterColorName.Letter}\t{letterColorName.Name}\t{DataText}\t";
                                }
                                else
                                {
                                    string SamplesCount = mwqmSampleList.Count.ToString();
                                    Every2YearStartingIn2018 = $"\t{SamplesCount}\t\t\t\t\t\t\t\t\t";
                                }
                            }
                            else
                            {
                                string SamplesCount = mwqmSampleList.Count.ToString();
                                Every2YearStartingIn2018 = $"\t{SamplesCount}\t\t\t\t\t\t\t\t\t";
                            }
                            #endregion Every2YearStartingIn2018

                            #region Every2YearStartingIn2017
                            // ---------------------------------------------------------------------------------------
                            // ------------------------------------ every 2 years Starting in 2017 -------------------
                            // ---------------------------------------------------------------------------------------

                            mwqmSampleListFull = (from c in mwqmSampleListStatEveryYear
                                                  where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                  orderby c.SampleDateTime_Local descending
                                                  select c).ToList<MWQMSample>();

                            mwqmSampleList = new List<MWQMSample>();

                            if (mwqmSampleListFull.Count >= 10 && DoSite)
                            {
                                YearWithData = mwqmSampleListFull.Select(c => c.SampleDateTime_Local.Year).Distinct().OrderByDescending(c => c).ToList();

                                StartYear = YearWithData[1]; // StartYear == 2017 because data is [2018, 2017...]
                                CurrentYear = StartYear;
                                for (int i = 0, count = mwqmSampleListFull.Count(); i < count; i++)
                                {
                                    if (mwqmSampleListFull[i].SampleDateTime_Local.Year == CurrentYear)
                                    {
                                        mwqmSampleList.Add(mwqmSampleListFull[i]);
                                    }
                                    else
                                    {
                                        if (mwqmSampleListFull[i].SampleDateTime_Local.Year == CurrentYear - 1)
                                        {
                                            if (CurrentYear <= ToYear)
                                            {
                                                break;
                                            }
                                            CurrentYear = CurrentYear - 2;
                                        }
                                    }
                                }

                                if (mwqmSampleList.Count >= 10 && DoSite)
                                {
                                    List<double> mwqmSampleFCList = (from c in mwqmSampleList
                                                                     where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                                     orderby c.SampleDateTime_Local descending
                                                                     select (c.FecCol_MPN_100ml < 2 ? 1.9D : (double)c.FecCol_MPN_100ml)).ToList<double>();

                                    double P90 = tvItemService.GetP90(mwqmSampleFCList);
                                    double GeoMean = tvItemService.GeometricMean(mwqmSampleFCList);
                                    double Median = tvItemService.GetMedian(mwqmSampleFCList);
                                    double PercOver43 = ((((double)mwqmSampleList.Where(c => c.FecCol_MPN_100ml > 43).Count()) / (double)mwqmSampleList.Count()) * 100.0D);
                                    double PercOver260 = ((((double)mwqmSampleList.Where(c => c.FecCol_MPN_100ml > 260).Count()) / (double)mwqmSampleList.Count()) * 100.0D);
                                    int MinYear = mwqmSampleList.Select(c => c.SampleDateTime_Local).Min().Year;

                                    int P90Int = (int)Math.Round((double)P90, 0);
                                    int GeoMeanInt = (int)Math.Round((double)GeoMean, 0);
                                    int MedianInt = (int)Math.Round((double)Median, 0);
                                    int PercOver43Int = (int)Math.Round((double)PercOver43, 0);
                                    int PercOver260Int = (int)Math.Round((double)PercOver260, 0);

                                    LetterColorName letterColorName = GetLetterColorName(P90Int, GeoMeanInt, MedianInt, PercOver43Int, PercOver260Int);

                                    string SamplesCount = mwqmSampleList.Count.ToString();
                                    string EndYear = MinYear.ToString();
                                    string GMeanText = GeoMean.ToString("F1");
                                    string MedText = Median.ToString("F1");
                                    string P90Text = P90.ToString("F1");
                                    string PerOver43Text = PercOver43.ToString("F1");
                                    string DataText = String.Join("|", mwqmSampleList.Select(c => c.FecCol_MPN_100ml));
                                    Every2YearStartingIn2017 = $"\t{SamplesCount}\t{EndYear}\t{GMeanText}\t{MedText}\t{P90Text}\t{PerOver43Text}\t{letterColorName.Letter}\t{letterColorName.Name}\t{DataText}\t";
                                }
                                else
                                {
                                    string SamplesCount = mwqmSampleList.Count.ToString();
                                    Every2YearStartingIn2018 = $"\t{SamplesCount}\t\t\t\t\t\t\t\t\t";
                                }
                            }
                            else
                            {
                                string SamplesCount = mwqmSampleList.Count.ToString();
                                Every2YearStartingIn2018 = $"\t{SamplesCount}\t\t\t\t\t\t\t\t\t";
                            }

                            #endregion Every2YearStartingIn2017

                            #region Every2SampleStartingWithFirst
                            // ---------------------------------------------------------------------------------------
                            // ------------------------------------ every 2 years Starting in 2017 -------------------
                            // ---------------------------------------------------------------------------------------

                            mwqmSampleListFull = (from c in mwqmSampleListStatEveryYear
                                                  where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                  orderby c.SampleDateTime_Local descending
                                                  select c).ToList<MWQMSample>();

                            mwqmSampleList = new List<MWQMSample>();

                            if (mwqmSampleListFull.Count >= 10 && DoSite)
                            {
                                for (int i = 0, count = mwqmSampleListFull.Count(); i < count; i++)
                                {
                                    if (mwqmSampleListFull[i].SampleDateTime_Local.Year >= ToYear)
                                    {
                                        mwqmSampleList.Add(mwqmSampleListFull[i]);
                                        i += 1;
                                    }
                                }

                                if (mwqmSampleList.Count >= 10 && DoSite)
                                {
                                    List<double> mwqmSampleFCList = (from c in mwqmSampleList
                                                                     where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                                     orderby c.SampleDateTime_Local descending
                                                                     select (c.FecCol_MPN_100ml < 2 ? 1.9D : (double)c.FecCol_MPN_100ml)).ToList<double>();

                                    double P90 = tvItemService.GetP90(mwqmSampleFCList);
                                    double GeoMean = tvItemService.GeometricMean(mwqmSampleFCList);
                                    double Median = tvItemService.GetMedian(mwqmSampleFCList);
                                    double PercOver43 = ((((double)mwqmSampleList.Where(c => c.FecCol_MPN_100ml > 43).Count()) / (double)mwqmSampleList.Count()) * 100.0D);
                                    double PercOver260 = ((((double)mwqmSampleList.Where(c => c.FecCol_MPN_100ml > 260).Count()) / (double)mwqmSampleList.Count()) * 100.0D);
                                    int MinYear = mwqmSampleList.Select(c => c.SampleDateTime_Local).Min().Year;

                                    int P90Int = (int)Math.Round((double)P90, 0);
                                    int GeoMeanInt = (int)Math.Round((double)GeoMean, 0);
                                    int MedianInt = (int)Math.Round((double)Median, 0);
                                    int PercOver43Int = (int)Math.Round((double)PercOver43, 0);
                                    int PercOver260Int = (int)Math.Round((double)PercOver260, 0);

                                    LetterColorName letterColorName = GetLetterColorName(P90Int, GeoMeanInt, MedianInt, PercOver43Int, PercOver260Int);

                                    string SamplesCount = mwqmSampleList.Count.ToString();
                                    string EndYear = MinYear.ToString();
                                    string GMeanText = GeoMean.ToString("F1");
                                    string MedText = Median.ToString("F1");
                                    string P90Text = P90.ToString("F1");
                                    string PerOver43Text = PercOver43.ToString("F1");
                                    string DataText = String.Join("|", mwqmSampleList.Select(c => c.FecCol_MPN_100ml));
                                    Every2SampleStartingWithFirst = $"\t{SamplesCount}\t{EndYear}\t{GMeanText}\t{MedText}\t{P90Text}\t{PerOver43Text}\t{letterColorName.Letter}\t{letterColorName.Name}\t{DataText}\t";
                                }
                                else
                                {
                                    string SamplesCount = mwqmSampleList.Count.ToString();
                                    Every2SampleStartingWithFirst = $"\t{SamplesCount}\t\t\t\t\t\t\t\t\t";
                                }
                            }
                            else
                            {
                                string SamplesCount = mwqmSampleList.Count.ToString();
                                Every2SampleStartingWithFirst = $"\t{SamplesCount}\t\t\t\t\t\t\t\t\t";
                            }

                            #endregion Every2SampleStartingWithFirst

                            #region Every2SampleStartingWithSecond
                            // ---------------------------------------------------------------------------------------
                            // ------------------------------------ every 2 years Starting in 2017 -------------------
                            // ---------------------------------------------------------------------------------------

                            mwqmSampleListFull = (from c in mwqmSampleListStatEveryYear
                                                  where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                  orderby c.SampleDateTime_Local descending
                                                  select c).ToList<MWQMSample>();

                            mwqmSampleList = new List<MWQMSample>();

                            if (mwqmSampleListFull.Count >= 10 && DoSite)
                            {
                                for (int i = 1, count = mwqmSampleListFull.Count(); i < count; i++)
                                {
                                    if (mwqmSampleListFull[i].SampleDateTime_Local.Year >= ToYear)
                                    {
                                        mwqmSampleList.Add(mwqmSampleListFull[i]);
                                        i += 1;
                                    }
                                }

                                if (mwqmSampleList.Count >= 10 && DoSite)
                                {
                                    List<double> mwqmSampleFCList = (from c in mwqmSampleList
                                                                     where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                                     orderby c.SampleDateTime_Local descending
                                                                     select (c.FecCol_MPN_100ml < 2 ? 1.9D : (double)c.FecCol_MPN_100ml)).ToList<double>();

                                    double P90 = tvItemService.GetP90(mwqmSampleFCList);
                                    double GeoMean = tvItemService.GeometricMean(mwqmSampleFCList);
                                    double Median = tvItemService.GetMedian(mwqmSampleFCList);
                                    double PercOver43 = ((((double)mwqmSampleList.Where(c => c.FecCol_MPN_100ml > 43).Count()) / (double)mwqmSampleList.Count()) * 100.0D);
                                    double PercOver260 = ((((double)mwqmSampleList.Where(c => c.FecCol_MPN_100ml > 260).Count()) / (double)mwqmSampleList.Count()) * 100.0D);
                                    int MinYear = mwqmSampleList.Select(c => c.SampleDateTime_Local).Min().Year;

                                    int P90Int = (int)Math.Round((double)P90, 0);
                                    int GeoMeanInt = (int)Math.Round((double)GeoMean, 0);
                                    int MedianInt = (int)Math.Round((double)Median, 0);
                                    int PercOver43Int = (int)Math.Round((double)PercOver43, 0);
                                    int PercOver260Int = (int)Math.Round((double)PercOver260, 0);

                                    LetterColorName letterColorName = GetLetterColorName(P90Int, GeoMeanInt, MedianInt, PercOver43Int, PercOver260Int);

                                    string SamplesCount = mwqmSampleList.Count.ToString();
                                    string EndYear = MinYear.ToString();
                                    string GMeanText = GeoMean.ToString("F1");
                                    string MedText = Median.ToString("F1");
                                    string P90Text = P90.ToString("F1");
                                    string PerOver43Text = PercOver43.ToString("F1");
                                    string DataText = String.Join("|", mwqmSampleList.Select(c => c.FecCol_MPN_100ml));
                                    Every2SampleStartingWithSecond = $"\t{SamplesCount}\t{EndYear}\t{GMeanText}\t{MedText}\t{P90Text}\t{PerOver43Text}\t{letterColorName.Letter}\t{letterColorName.Name}\t{DataText}\t";
                                }
                                else
                                {
                                    string SamplesCount = mwqmSampleList.Count.ToString();
                                    Every2SampleStartingWithSecond = $"\t{SamplesCount}\t\t\t\t\t\t\t\t\t";
                                }
                            }
                            else
                            {
                                string SamplesCount = mwqmSampleList.Count.ToString();
                                Every2SampleStartingWithSecond = $"\t{SamplesCount}\t\t\t\t\t\t\t\t\t";
                            }

                            #endregion Every2SampleStartingWithSecond
                        }

                        sb.AppendLine($"{EveryYear}{Every2YearStartingIn2018}{Every2YearStartingIn2017}{Every2SampleStartingWithFirst}{Every2SampleStartingWithSecond}");
                    }
                }
            }

            richTextBoxStatus.Text = sb.ToString();
        }
        private LetterColorName GetLetterColorName(int P90Int, int GeoMeanInt, int MedianInt, int PercOver43Int, int PercOver260Int)
        {
            LetterColorName letterColorName = new LetterColorName();
            if ((GeoMeanInt > 88) || (MedianInt > 88) || (P90Int > 260) || (PercOver260Int > 10))
            {
                if ((GeoMeanInt > 181) || (MedianInt > 181) || (P90Int > 460) || (PercOver260Int > 18))
                {
                    letterColorName = new LetterColorName() { Letter = "F", Color = "8888ff", Name = "NoDepuration" };
                }
                else if ((GeoMeanInt > 163) || (MedianInt > 163) || (P90Int > 420) || (PercOver260Int > 17))
                {
                    letterColorName = new LetterColorName() { Letter = "E", Color = "9999ff", Name = "NoDepuration" };
                }
                else if ((GeoMeanInt > 144) || (MedianInt > 144) || (P90Int > 380) || (PercOver260Int > 15))
                {
                    letterColorName = new LetterColorName() { Letter = "D", Color = "aaaaff", Name = "NoDepuration" };
                }
                else if ((GeoMeanInt > 125) || (MedianInt > 125) || (P90Int > 340) || (PercOver260Int > 13))
                {
                    letterColorName = new LetterColorName() { Letter = "C", Color = "bbbbff", Name = "NoDepuration" };
                }
                else if ((GeoMeanInt > 107) || (MedianInt > 107) || (P90Int > 300) || (PercOver260Int > 12))
                {
                    letterColorName = new LetterColorName() { Letter = "B", Color = "ccccff", Name = "NoDepuration" };
                }
                else
                {
                    letterColorName = new LetterColorName() { Letter = "A", Color = "ddddff", Name = "NoDepuration" };
                }
            }
            else if ((GeoMeanInt > 14) || (MedianInt > 14) || (P90Int > 43) || (PercOver43Int > 10))
            {
                if ((GeoMeanInt > 76) || (MedianInt > 76) || (P90Int > 224) || (PercOver43Int > 27))
                {
                    letterColorName = new LetterColorName() { Letter = "F", Color = "aa0000", Name = "Fail" };
                }
                else if ((GeoMeanInt > 63) || (MedianInt > 63) || (P90Int > 188) || (PercOver43Int > 23))
                {
                    letterColorName = new LetterColorName() { Letter = "E", Color = "cc0000", Name = "Fail" };
                }
                else if ((GeoMeanInt > 51) || (MedianInt > 51) || (P90Int > 152) || (PercOver43Int > 20))
                {
                    letterColorName = new LetterColorName() { Letter = "D", Color = "ff1111", Name = "Fail" };
                }
                else if ((GeoMeanInt > 39) || (MedianInt > 39) || (P90Int > 115) || (PercOver43Int > 17))
                {
                    letterColorName = new LetterColorName() { Letter = "C", Color = "ff4444", Name = "Fail" };
                }
                else if ((GeoMeanInt > 26) || (MedianInt > 26) || (P90Int > 79) || (PercOver43Int > 13))
                {
                    letterColorName = new LetterColorName() { Letter = "B", Color = "ff9999", Name = "Fail" };
                }
                else
                {
                    letterColorName = new LetterColorName() { Letter = "A", Color = "ffcccc", Name = "Fail" };
                }
            }
            else
            {
                if ((GeoMeanInt > 12) || (MedianInt > 12) || (P90Int > 36) || (PercOver43Int > 8))
                {
                    letterColorName = new LetterColorName() { Letter = "F", Color = "ccffcc", Name = "Pass" };
                }
                else if ((GeoMeanInt > 9) || (MedianInt > 9) || (P90Int > 29) || (PercOver43Int > 7))
                {
                    letterColorName = new LetterColorName() { Letter = "E", Color = "99ff99", Name = "Pass" };
                }
                else if ((GeoMeanInt > 7) || (MedianInt > 7) || (P90Int > 22) || (PercOver43Int > 5))
                {
                    letterColorName = new LetterColorName() { Letter = "D", Color = "44ff44", Name = "Pass" };
                }
                else if ((GeoMeanInt > 5) || (MedianInt > 5) || (P90Int > 14) || (PercOver43Int > 3))
                {
                    letterColorName = new LetterColorName() { Letter = "C", Color = "11ff11", Name = "Pass" };
                }
                else if ((GeoMeanInt > 2) || (MedianInt > 2) || (P90Int > 7) || (PercOver43Int > 2))
                {
                    letterColorName = new LetterColorName() { Letter = "B", Color = "00bb00", Name = "Pass" };
                }
                else
                {
                    letterColorName = new LetterColorName() { Letter = "A", Color = "009900", Name = "Pass" };
                }
            }

            return letterColorName;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            DateTime StartDate = new DateTime(2017, 1, 1);
            DateTime EndDate = new DateTime(2017, 12, 31);
            string routineText = "109,";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("2018 Statistics");
            sb.AppendLine("Locator\tSubsector Name\tRuns #\tStations #\tMin Date\tMax Date\tLat\tLong");

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                lblStatus.Text = "Could not find Root";
                return;
            }

            List<string> ProvNameList = new List<string>()
            {
                "Nova Scotia",
                "New Brunswick",
                "Newfoundland and Labrador",
                "Prince Edward Island"
            };

            foreach (string provName in ProvNameList)
            {
                TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, provName, TVTypeEnum.Province);
                if (!string.IsNullOrWhiteSpace(tvItemModelProv.Error))
                {
                    lblStatus.Text = $"Could not find Province {textBoxProvinceName.Text}";
                    return;
                }

                List<TVItemModel> tvitemModelSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);

                foreach (TVItemModel tvItemModelSS in tvitemModelSSList)
                {
                    lblStatus.Text = tvItemModelSS.TVText;
                    lblStatus.Refresh();
                    Application.DoEvents();

                    string Locator = tvItemModelSS.TVText;
                    string SubsectorName = Locator.Substring(Locator.IndexOf(" ")).Trim();
                    Locator = Locator.Substring(0, Locator.IndexOf(" ")).Trim();

                    List<MWQMRun> mwqmRunList = new List<MWQMRun>();
                    List<MWQMSample> mwqmSampleList = new List<MWQMSample>();
                    List<MWQMSite> mwqmSiteList = new List<MWQMSite>();
                    List<MapInfo> mapInfoList = new List<MapInfo>();
                    List<MapInfoPoint> mapInfoPointList = new List<MapInfoPoint>();

                    using (CSSPDBEntities db2 = new CSSPDBEntities())
                    {
                        var aaa = (from c in db2.MWQMSamples
                                   from r in db2.MWQMRuns
                                   from t in db2.TVItems
                                   from s in db2.MWQMSites
                                   from t2 in db2.TVItems
                                   from mi in db2.MapInfos
                                   from mip in db2.MapInfoPoints
                                   where r.MWQMRunTVItemID == t.TVItemID
                                   && s.MWQMSiteTVItemID == t2.TVItemID
                                   && c.MWQMRunTVItemID == r.MWQMRunTVItemID
                                   && c.MWQMSiteTVItemID == s.MWQMSiteTVItemID
                                   && mi.TVItemID == t2.TVItemID
                                   && mi.MapInfoID == mip.MapInfoID
                                   && mi.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
                                   && t.TVType == (int)TVTypeEnum.MWQMRun
                                   && t.TVPath.StartsWith(tvItemModelSS.TVPath + "p")
                                   && t2.TVType == (int)TVTypeEnum.MWQMSite
                                   && t2.TVPath.StartsWith(tvItemModelSS.TVPath + "p")
                                   && c.SampleTypesText.Contains(routineText)
                                   && c.SampleDateTime_Local >= StartDate
                                   && c.SampleDateTime_Local <= EndDate
                                   select new { c, r, s, mi, mip }).ToList();

                        if (aaa.Count > 0)
                        {
                            mwqmRunList = (from c in aaa
                                           select c.r).Distinct().ToList();

                            mwqmSiteList = (from c in aaa
                                            select c.s).Distinct().ToList();

                            mwqmSampleList = (from c in aaa
                                              select c.c).Distinct().ToList();

                            mapInfoList = (from c in aaa
                                           select c.mi).Distinct().ToList();

                            mapInfoPointList = (from c in aaa
                                                select c.mip).Distinct().ToList();


                            MWQMRun mwqmRunMinDate = (from c in mwqmRunList
                                                      orderby c.DateTime_Local
                                                      select c).FirstOrDefault();

                            string mwqmRunMinDateText = "";

                            if (mwqmRunMinDate != null)
                            {
                                mwqmRunMinDateText = ((MWQMRun)mwqmRunMinDate).DateTime_Local.ToString("yyyy-MM-dd");
                            }

                            MWQMRun mwqmRunMaxDate = (from c in mwqmRunList
                                                      orderby c.DateTime_Local descending
                                                      select c).FirstOrDefault();

                            string mwqmRunMaxDateText = "";

                            if (mwqmRunMaxDate != null)
                            {
                                mwqmRunMaxDateText = ((MWQMRun)mwqmRunMaxDate).DateTime_Local.ToString("yyyy-MM-dd");
                            }

                            int RunNumb = mwqmRunList.Count();
                            int SiteNumb = mwqmSiteList.Count();

                            var LatLongList = (from s in mwqmSiteList
                                               from mi in mapInfoList
                                               from mip in mapInfoPointList
                                               where s.MWQMSiteTVItemID == mi.TVItemID
                                               && mi.MapInfoID == mip.MapInfoID
                                               select new { mip.Lat, mip.Lng }).ToList();

                            double Lat = (from c in LatLongList
                                          select c.Lat).Average();

                            double Lng = (from c in LatLongList
                                          select c.Lng).Average();

                            sb.AppendLine($"{Locator}\t{SubsectorName}\t{RunNumb.ToString()}\t{SiteNumb.ToString()}\t{mwqmRunMinDateText}\t{mwqmRunMaxDateText}\t{Lat}\t{Lng}");
                        }
                        else
                        {
                            sb.AppendLine($"{Locator}\t{SubsectorName}");
                        }

                    }
                }
            }

            richTextBoxStatus.Text = sb.ToString();
        }

        private void Button11_Click(object sender, EventArgs e)
        {
            //MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            //TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            //ClimateSiteService climateSiteService = new ClimateSiteService(LanguageEnum.en, user);

            //FileInfo fi = new FileInfo(@"C:\Users\leblancc\Desktop\cocorahs.json");
            //StreamReader sr = fi.OpenText();
            //string ret = sr.ReadToEnd();
            //sr.Close();

            //List<CoCoRaHSSite> CoCoRaHSSiteList = new List<CoCoRaHSSite>();
            //try
            //{
            //    var CoCoRaHSSiteValue = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, CoCoRaHSSite[]>>(ret);

            //    CoCoRaHSSiteList = CoCoRaHSSiteValue.First().Value.ToList();
            //}
            //catch (Exception ex)
            //{
            //    int slejf = 34;
            //}

            //string Prov = "";

            //TVItemModel tvItemModelNB = tvItemService.GetTVItemModelWithTVItemIDDB(7); // New Brunswick
            //if (!string.IsNullOrWhiteSpace(tvItemModelNB.Error))
            //{
            //    richTextBoxStatus.Text = "Error could not find TVItemID = 7";
            //    return;
            //}
            //TVItemModel tvItemModelNS = tvItemService.GetTVItemModelWithTVItemIDDB(8); // Nova Scotia
            //if (!string.IsNullOrWhiteSpace(tvItemModelNB.Error))
            //{
            //    richTextBoxStatus.Text = "Error could not find TVItemID = 8";
            //    return;
            //}
            //TVItemModel tvItemModelNL = tvItemService.GetTVItemModelWithTVItemIDDB(10); // Newfoundland and Labrador
            //if (!string.IsNullOrWhiteSpace(tvItemModelNB.Error))
            //{
            //    richTextBoxStatus.Text = "Error could not find TVItemID = 10";
            //    return;
            //}
            //TVItemModel tvItemModelPE = tvItemService.GetTVItemModelWithTVItemIDDB(9); // Prince Edward Island
            //if (!string.IsNullOrWhiteSpace(tvItemModelNB.Error))
            //{
            //    richTextBoxStatus.Text = "Error could not find TVItemID = 9";
            //    return;
            //}
            //TVItemModel tvItemModelToUse = new TVItemModel();

            //foreach (CoCoRaHSSite site in CoCoRaHSSiteList)
            //{
            //    lblStatus.Text = site.st_num;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    if (site.st_num.StartsWith("CAN-NB"))
            //    {
            //        tvItemModelToUse = tvItemModelNB;
            //        Prov = "NB";
            //    }
            //    else if (site.st_num.StartsWith("CAN-NS"))
            //    {
            //        tvItemModelToUse = tvItemModelNS;
            //        Prov = "NS";
            //    }
            //    else if (site.st_num.StartsWith("CAN-NL"))
            //    {
            //        tvItemModelToUse = tvItemModelNL;
            //        Prov = "NL";
            //    }
            //    else if (site.st_num.StartsWith("CAN-PE"))
            //    {
            //        tvItemModelToUse = tvItemModelPE;
            //        Prov = "PE";
            //    }
            //    else
            //    {
            //        continue;
            //    }

            //    TVItemModel tvItemModelSite = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelToUse.TVItemID, "CoCoRaHS " + site.st_name, TVTypeEnum.ClimateSite);
            //    if (!string.IsNullOrWhiteSpace(tvItemModelSite.Error))
            //    {
            //        tvItemModelSite = tvItemService.PostAddChildTVItemDB(tvItemModelToUse.TVItemID, "CoCoRaHS " + site.st_name, TVTypeEnum.ClimateSite);
            //        if (!string.IsNullOrWhiteSpace(tvItemModelSite.Error))
            //        {
            //            richTextBoxStatus.Text = "Error could create TVItem = " + "CoCoRaHS " + site.st_name;
            //            return;
            //        }
            //    }

            //    List<Coord> coordList = new List<Coord>()
            //    {
            //        new Coord() { Lat = site.lat, Lng = site.lng, Ordinal = 0 }
            //    };

            //    List<MapInfoModel> mapInfoModelList = mapInfoService.GetMapInfoModelListWithTVItemIDDB(tvItemModelSite.TVItemID);

            //    bool MapInfoExist = false;
            //    if (mapInfoModelList.Count > 0)
            //    {
            //        foreach (MapInfoModel mapInfoModel in mapInfoModelList)
            //        {
            //            if (mapInfoModel.MapInfoDrawType == MapInfoDrawTypeEnum.Point && mapInfoModel.TVType == TVTypeEnum.ClimateSite)
            //            {
            //                MapInfoExist = true;
            //            }
            //        }
            //    }

            //    if (!MapInfoExist)
            //    {
            //        MapInfoModel mapInfoModel = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.ClimateSite, tvItemModelSite.TVItemID);
            //        if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
            //        {
            //            richTextBoxStatus.Text = "Error could create MapInfo = " + "CoCoRaHS " + site.st_name;
            //            return;
            //        }
            //    }

            //    ClimateSiteModel climateSiteModelNew = new ClimateSiteModel()
            //    {
            //        ClimateSiteTVItemID = tvItemModelSite.TVItemID,
            //        ClimateID = site.st_num,
            //        ClimateSiteName = site.st_name,
            //        IsCoCoRaHS = true,
            //        Province = Prov,
            //    };

            //    ClimateSiteModel climateSiteModel = climateSiteService.GetClimateSiteModelExistDB(climateSiteModelNew);
            //    if (!string.IsNullOrWhiteSpace(climateSiteModel.Error))
            //    {
            //        climateSiteModel = climateSiteService.PostAddClimateSiteDB(climateSiteModelNew);
            //        if (!string.IsNullOrWhiteSpace(climateSiteModel.Error))
            //        {
            //            richTextBoxStatus.Text = "Error could create ClimateSite = " + "CoCoRaHS " + site.st_name;
            //            return;
            //        }
            //    }

            //}

        }

        private void ButCheckBCSubsectors_Click(object sender, EventArgs e)
        {
            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            TVItemModel tvItemModelProv = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "British Columbia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            List<TVItemModel> tvItemModelSubsectorList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelProv.TVText + "\r\n");
                return;
            }

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {

                List<string> BCSubsectorListTemp = (from c in tvItemModelSubsectorList
                                                    select c.TVText).Distinct().ToList();


                foreach (TVItemModel tvItemModelSubsector in tvItemModelSubsectorList.OrderByDescending(c => c.TVText))
                {
                    lblStatus.Text = "Doing " + tvItemModelSubsector.TVText;
                    lblStatus.Refresh();
                    Application.DoEvents();

                    string subsector = tvItemModelSubsector.TVText;
                    if (tvItemModelSubsector.TVText.Contains(" "))
                    {
                        subsector = tvItemModelSubsector.TVText.Substring(0, tvItemModelSubsector.TVText.IndexOf(" "));
                    }

                    var BCSubsectorList = (from c in dbDT.BCMarineSampleStations
                                           from s in dbDT.BCMarineSamples
                                           where c.SS_STATION == s.SR_STATION_CODE
                                           && c.SS_SHELLFI == subsector
                                           orderby s.SR_READING_DATE
                                           select new { s.SR_READING_DATE, s.SR_SURVEY, s.SR_ANALYSIS_TYPE, s.SR_SAMPLE_AGENCY, s.SR_SAMPLE_TYPE }).Distinct().ToList();


                    foreach (var BCSubsector in BCSubsectorList)
                    {
                        lblStatus.Text = "Doing " + tvItemModelSubsector.TVText + " --- " + BCSubsector.SR_READING_DATE;
                        lblStatus.Refresh();
                        Application.DoEvents();

                        SampleTypeEnum sampleType = SampleTypeEnum.Routine;

                        if (BCSubsector.SR_SAMPLE_TYPE == "S")
                        {
                            sampleType = SampleTypeEnum.Sediment;
                        }

                        if (BCSubsector.SR_SAMPLE_TYPE == "B")
                        {
                            sampleType = SampleTypeEnum.Bivalve;
                        }

                        DateTime DayOfSample = (DateTime)(BCSubsector.SR_READING_DATE);

                        MWQMRunModel mwqmRunModelNew = new MWQMRunModel()
                        {
                            SubsectorTVItemID = tvItemModelSubsector.TVItemID,
                            DateTime_Local = DayOfSample,
                            StartDateTime_Local = DayOfSample,
                            EndDateTime_Local = DayOfSample,
                            RunSampleType = sampleType,
                            RunNumber = 1,
                        };

                        MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
                        TideSiteService tideSiteService = new TideSiteService(LanguageEnum.en, user);
                        UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);
                        TideDataValueService tideDataValueService = new TideDataValueService(LanguageEnum.en, user);

                        MWQMRunModel mwqmRunModelExist = mwqmRunService.GetMWQMRunModelExistDB(mwqmRunModelNew);
                        if (!string.IsNullOrWhiteSpace(mwqmRunModelExist.Error))
                        {
                            string TVTextRun = DayOfSample.Year.ToString() + " " +
                                (DayOfSample.Month < 10 ? "0" : "") + DayOfSample.Month.ToString() + " " +
                                (DayOfSample.Day < 10 ? "0" : "") + DayOfSample.Day.ToString();

                            TVItemModel tvItemModel = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelSubsector.TVItemID, TVTextRun, TVTypeEnum.MWQMRun);
                            if (!string.IsNullOrWhiteSpace(tvItemModel.Error))
                            {
                                tvItemModel = tvItemService.PostAddChildTVItemDB(tvItemModelSubsector.TVItemID, TVTextRun, TVTypeEnum.MWQMRun);
                                if (!string.IsNullOrWhiteSpace(tvItemModel.Error))
                                {
                                    richTextBoxStatus.AppendText(tvItemModel.Error + "\r\n");
                                    return;
                                }
                            }


                            mwqmRunModelNew.SubsectorTVItemID = tvItemModelSubsector.TVItemID;
                            mwqmRunModelNew.MWQMRunTVItemID = tvItemModel.TVItemID;
                            mwqmRunModelNew.DateTime_Local = DayOfSample;
                            mwqmRunModelNew.StartDateTime_Local = null;
                            mwqmRunModelNew.EndDateTime_Local = null;
                            mwqmRunModelNew.RunSampleType = sampleType;
                            mwqmRunModelNew.RunNumber = 1;

                            string Comments = null;

                            TempData.BCSurvey bcSurvey = new TempData.BCSurvey();

                            using (TempData.TempDataToolDBEntities dbDT2 = new TempData.TempDataToolDBEntities())
                            {
                                bcSurvey = (from c in dbDT2.BCSurveys
                                            where c.S_ID_NUMBER == BCSubsector.SR_SURVEY
                                            select c).FirstOrDefault<TempData.BCSurvey>();
                            }

                            if (bcSurvey != null)
                            {
                                mwqmRunModelNew.StartDateTime_Local = bcSurvey.S_START_DATE;
                                mwqmRunModelNew.EndDateTime_Local = bcSurvey.S_END_DATE;
                                if (mwqmRunModelNew.StartDateTime_Local > mwqmRunModelNew.EndDateTime_Local)
                                {
                                    mwqmRunModelNew.EndDateTime_Local = mwqmRunModelNew.StartDateTime_Local;
                                }
                                Comments = bcSurvey.S_DESCRIPTION + "\r\n" + bcSurvey.S_COMMENT;
                            }

                            string TextEN = "--";
                            if (!string.IsNullOrWhiteSpace(Comments))
                            {
                                TextEN = Comments.Trim();
                            }

                            mwqmRunModelNew.RunComment = TextEN;
                            mwqmRunModelNew.RunWeatherComment = TextEN;
                            mwqmRunModelNew.LabReceivedDateTime_Local = null;
                            mwqmRunModelNew.TemperatureControl1_C = null;
                            mwqmRunModelNew.TemperatureControl2_C = null;
                            mwqmRunModelNew.SeaStateAtStart_BeaufortScale = null;
                            mwqmRunModelNew.SeaStateAtEnd_BeaufortScale = null;
                            mwqmRunModelNew.WaterLevelAtBrook_m = null;
                            mwqmRunModelNew.WaveHightAtStart_m = null;
                            mwqmRunModelNew.WaveHightAtEnd_m = null;
                            mwqmRunModelNew.SampleCrewInitials = null;
                            mwqmRunModelNew.AnalyzeMethod = null;
                            mwqmRunModelNew.SampleMatrix = null;
                            mwqmRunModelNew.Laboratory = null;
                            mwqmRunModelNew.SampleStatus = null;
                            mwqmRunModelNew.LabSampleApprovalContactTVItemID = null;
                            mwqmRunModelNew.LabAnalyzeBath1IncubationStartDateTime_Local = null;
                            mwqmRunModelNew.LabRunSampleApprovalDateTime_Local = null;
                            mwqmRunModelNew.Tide_Start = null;
                            mwqmRunModelNew.Tide_End = null;

                            switch (BCSubsector.SR_ANALYSIS_TYPE)
                            {
                                case "MF":
                                    {
                                        mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.MF;
                                    }
                                    break;
                                case "MPN":
                                    {
                                        mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.MPN;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            switch (BCSubsector.SR_SAMPLE_TYPE)
                            {
                                case "W":
                                    {
                                        mwqmRunModelNew.SampleMatrix = SampleMatrixEnum.W;
                                    }
                                    break;
                                case "S":
                                    {
                                        mwqmRunModelNew.SampleMatrix = SampleMatrixEnum.S;
                                    }
                                    break;
                                case "B":
                                    {
                                        mwqmRunModelNew.SampleMatrix = SampleMatrixEnum.B;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            switch (BCSubsector.SR_SAMPLE_AGENCY)
                            {
                                case 0:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_0;
                                    }
                                    break;
                                case 1:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_1;
                                    }
                                    break;
                                case 2:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_2;
                                    }
                                    break;
                                case 3:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_3;
                                    }
                                    break;
                                case 4:
                                    {
                                        mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_4;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            MWQMRunModel mwqmRunModelRet = mwqmRunService.PostAddMWQMRunDB(mwqmRunModelNew);
                            if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return;
                        }
                        else
                        {
                            bool ShouldUpdate2 = false;

                            if (mwqmRunModelExist.RunSampleType != sampleType)
                            {
                                mwqmRunModelExist.RunSampleType = sampleType;
                                ShouldUpdate2 = true;
                            }

                            mwqmRunModelExist.RunNumber = 1;

                            string Comments = null;

                            TempData.BCSurvey bcSurvey = new TempData.BCSurvey();

                            using (TempData.TempDataToolDBEntities dbDT2 = new TempData.TempDataToolDBEntities())
                            {
                                bcSurvey = (from c in dbDT2.BCSurveys
                                            where c.S_ID_NUMBER == BCSubsector.SR_SURVEY
                                            select c).FirstOrDefault<TempData.BCSurvey>();
                            }

                            if (bcSurvey != null)
                            {
                                if (mwqmRunModelExist.StartDateTime_Local != bcSurvey.S_START_DATE || mwqmRunModelExist.EndDateTime_Local != bcSurvey.S_END_DATE)
                                {
                                    mwqmRunModelExist.StartDateTime_Local = bcSurvey.S_START_DATE;
                                    mwqmRunModelExist.EndDateTime_Local = bcSurvey.S_END_DATE;
                                    if (mwqmRunModelExist.StartDateTime_Local > mwqmRunModelExist.EndDateTime_Local)
                                    {
                                        mwqmRunModelExist.EndDateTime_Local = mwqmRunModelExist.StartDateTime_Local;
                                    }

                                    ShouldUpdate2 = true;
                                }
                                Comments = bcSurvey.S_DESCRIPTION + "\r\n" + bcSurvey.S_COMMENT;
                            }

                            string TextEN = "--";
                            if (!string.IsNullOrWhiteSpace(Comments))
                            {
                                TextEN = Comments.Trim();
                            }

                            if (mwqmRunModelExist.RunComment != TextEN || mwqmRunModelExist.RunWeatherComment != TextEN)
                            {
                                mwqmRunModelExist.RunComment = TextEN;
                                mwqmRunModelExist.RunWeatherComment = TextEN;
                                ShouldUpdate2 = true;
                            }

                            mwqmRunModelExist.LabReceivedDateTime_Local = null;
                            mwqmRunModelExist.TemperatureControl1_C = null;
                            mwqmRunModelExist.TemperatureControl2_C = null;
                            mwqmRunModelExist.SeaStateAtStart_BeaufortScale = null;
                            mwqmRunModelExist.SeaStateAtEnd_BeaufortScale = null;
                            mwqmRunModelExist.WaterLevelAtBrook_m = null;
                            mwqmRunModelExist.WaveHightAtStart_m = null;
                            mwqmRunModelExist.WaveHightAtEnd_m = null;
                            mwqmRunModelExist.SampleCrewInitials = null;
                            mwqmRunModelExist.AnalyzeMethod = null;
                            mwqmRunModelExist.SampleMatrix = null;
                            mwqmRunModelExist.Laboratory = null;
                            mwqmRunModelExist.SampleStatus = null;
                            mwqmRunModelExist.LabSampleApprovalContactTVItemID = null;
                            mwqmRunModelExist.LabAnalyzeBath1IncubationStartDateTime_Local = null;
                            mwqmRunModelExist.LabRunSampleApprovalDateTime_Local = null;
                            mwqmRunModelExist.Tide_Start = null;
                            mwqmRunModelExist.Tide_End = null;

                            switch (BCSubsector.SR_ANALYSIS_TYPE)
                            {
                                case "MF":
                                    {
                                        if (mwqmRunModelExist.AnalyzeMethod != AnalyzeMethodEnum.MF)
                                        {
                                            mwqmRunModelExist.AnalyzeMethod = AnalyzeMethodEnum.MF;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case "MPN":
                                    {
                                        if (mwqmRunModelExist.AnalyzeMethod != AnalyzeMethodEnum.MPN)
                                        {
                                            mwqmRunModelExist.AnalyzeMethod = AnalyzeMethodEnum.MPN;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            switch (BCSubsector.SR_SAMPLE_TYPE)
                            {
                                case "W":
                                    {
                                        if (mwqmRunModelExist.SampleMatrix != SampleMatrixEnum.W)
                                        {
                                            mwqmRunModelExist.SampleMatrix = SampleMatrixEnum.W;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case "S":
                                    {
                                        if (mwqmRunModelExist.SampleMatrix != SampleMatrixEnum.S)
                                        {
                                            mwqmRunModelExist.SampleMatrix = SampleMatrixEnum.S;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case "B":
                                    {
                                        if (mwqmRunModelExist.SampleMatrix != SampleMatrixEnum.B)
                                        {
                                            mwqmRunModelExist.SampleMatrix = SampleMatrixEnum.B;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            switch (BCSubsector.SR_SAMPLE_AGENCY)
                            {
                                case 0:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_0)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_0;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case 1:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_1)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_1;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case 2:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_2)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_2;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case 3:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_3)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_3;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                case 4:
                                    {
                                        if (mwqmRunModelExist.Laboratory != LaboratoryEnum.ZZ_4)
                                        {
                                            mwqmRunModelExist.Laboratory = LaboratoryEnum.ZZ_4;
                                            ShouldUpdate2 = true;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            if (ShouldUpdate2)
                            {
                                MWQMRunModel mwqmRunModelRet = mwqmRunService.PostUpdateMWQMRunDB(mwqmRunModelExist);
                                if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return;
                            }
                        }
                    }
                }
            }


        }

        private void ButCheckBCSamples_Click(object sender, EventArgs e)
        {
            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);
            MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            TVItemModel tvItemModelProv = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "British Columbia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            List<TVItemModel> tvItemModelSubsectorList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelProv.TVText + "\r\n");
                return;
            }

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {

                List<string> BCSubsectorListTemp = (from c in tvItemModelSubsectorList
                                                    select c.TVText).Distinct().ToList();


                bool Jump = true;

                foreach (TVItemModel tvItemModelSubsector in tvItemModelSubsectorList)
                {
                    lblStatus.Text = "Doing " + tvItemModelSubsector.TVText;
                    lblStatus.Refresh();
                    Application.DoEvents();

                    string subsector = tvItemModelSubsector.TVText;
                    if (tvItemModelSubsector.TVText.Contains(" "))
                    {
                        subsector = tvItemModelSubsector.TVText.Substring(0, tvItemModelSubsector.TVText.IndexOf(" "));
                    }

                    if (subsector == textBoxStartSubsectorName.Text)
                    {
                        Jump = false;
                    }

                    if (Jump)
                    {
                        continue;
                    }

                    List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSubsector.TVItemID, TVTypeEnum.MWQMSite);
                    //List<TVItemModel> tvItemModelMWQMRun = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSubsector.TVItemID, TVTypeEnum.MWQMRun);

                    List<MWQMRunModel> BCRunModelList = mwqmRunService.GetMWQMRunModelListWithSubsectorTVItemIDDB(tvItemModelSubsector.TVItemID);
                    if (BCRunModelList.Count == 0)
                        continue;

                    List<MWQMSample> mwqmSampleList = new List<MWQMSample>();
                    using (CSSPDBEntities dd = new CSSPDBEntities())
                    {
                        mwqmSampleList = (from c in dd.MWQMSamples
                                          from ts in dd.TVItems
                                          from tr in dd.TVItems
                                          where c.MWQMSiteTVItemID == ts.TVItemID
                                          && c.MWQMRunTVItemID == tr.TVItemID
                                          && ts.ParentID == tvItemModelSubsector.TVItemID
                                          && tr.ParentID == tvItemModelSubsector.TVItemID
                                          select c).Distinct().ToList();



                        var BCSubsectorList = (from c in dbDT.BCMarineSampleStations
                                               from s in dbDT.BCMarineSamples
                                               where c.SS_STATION == s.SR_STATION_CODE
                                               && c.SS_SHELLFI == subsector
                                               orderby s.SR_STATION_CODE, s.SR_READING_DATE
                                               select new { s.SR_TEMPERATURE, s.SR_SALINITY, s.SR_SAMPLE_DEPTH, s.SR_FECAL_COLIFORM_IND, s.SR_STATION_CODE, s.SR_FECAL_COLIFORM, s.SR_READING_DATE, s.SR_READING_TIME, s.SR_OBS, s.Pub, s.SR_SAMPLE_TYPE }).ToList();


                        foreach (var BCSubsector in BCSubsectorList)
                        {
                            if (Cancel) return;

                            lblStatus.Text = "Doing " + tvItemModelSubsector.TVText + " --- " + BCSubsector.SR_STATION_CODE + " --- " + BCSubsector.SR_READING_DATE;
                            lblStatus.Refresh();
                            Application.DoEvents();

                            SampleTypeEnum sampleType = SampleTypeEnum.Routine;

                            if (BCSubsector.SR_SAMPLE_TYPE == "S")
                            {
                                sampleType = SampleTypeEnum.Sediment;
                            }

                            if (BCSubsector.SR_SAMPLE_TYPE == "B")
                            {
                                sampleType = SampleTypeEnum.Bivalve;
                            }

                            DateTime DayOfSample = (DateTime)(BCSubsector.SR_READING_DATE);

                            string SampleTime = BCSubsector.SR_READING_TIME;

                            if (SampleTime == null || SampleTime == "0")
                            {
                                SampleTime = "0000";
                            }

                            string hourText = SampleTime.Substring(0, 2);
                            string minText = SampleTime.Substring(2, 2);

                            int hour = int.Parse(hourText);
                            int min = int.Parse(minText);

                            DateTime SampleDate = new DateTime(DayOfSample.Year, DayOfSample.Month, DayOfSample.Day, hour, min, 0);

                            int FecCol = 0;
                            if (BCSubsector.SR_FECAL_COLIFORM_IND == "<" && BCSubsector.SR_FECAL_COLIFORM == 2)
                            {
                                FecCol = 1;
                            }
                            else
                            {
                                if (BCSubsector.SR_FECAL_COLIFORM == null)
                                {
                                    continue;
                                }

                                FecCol = (int)BCSubsector.SR_FECAL_COLIFORM;
                            }
                            if (FecCol == 0)
                            {
                                FecCol = 1;
                            }

                            double? Depth = BCSubsector.SR_SAMPLE_DEPTH;
                            double? Salinity = BCSubsector.SR_SALINITY;
                            double? Temperature = BCSubsector.SR_TEMPERATURE;

                            bool UseForOpenData = BCSubsector.Pub == "y" ? true : false;

                            MWQMRunModel mwqmRunModel = (from c in BCRunModelList
                                                         where c.SubsectorTVItemID == tvItemModelSubsector.TVItemID
                                                         && c.DateTime_Local.Year == SampleDate.Year
                                                         && c.DateTime_Local.Month == SampleDate.Month
                                                         && c.DateTime_Local.Day == SampleDate.Day
                                                         && c.RunSampleType == sampleType
                                                         select c).FirstOrDefault();

                            if (mwqmRunModel == null)
                            {
                                richTextBoxStatus.AppendText("Could not find run for date [" + SampleDate.ToString("yyyy MMM dd") + "] for subsector [" + tvItemModelSubsector.TVText + "]\r\n");
                                continue;
                                //return false;
                            }

                            TVItemModel tvItemModelMWQMSite = tvItemModelMWQMSiteList.Where(c => c.TVText == BCSubsector.SR_STATION_CODE).FirstOrDefault();
                            if (tvItemModelMWQMSite == null)
                            {
                                richTextBoxStatus.AppendText("Could not find MWQMSite [" + BCSubsector.SR_STATION_CODE + "]");
                                return;
                            }

                            string SampleTypeText = ((int)sampleType).ToString() + ",";

                            MWQMSample SampleExist = (from c in mwqmSampleList
                                                      where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                      && c.MWQMRunTVItemID == mwqmRunModel.MWQMRunTVItemID
                                                      && c.SampleDateTime_Local.Year == SampleDate.Year
                                                      && c.SampleDateTime_Local.Month == SampleDate.Month
                                                      && c.SampleDateTime_Local.Day == SampleDate.Day
                                                      && c.FecCol_MPN_100ml == FecCol
                                                      && c.Depth_m == Depth
                                                      && c.Salinity_PPT == Salinity
                                                      && c.WaterTemp_C == Temperature
                                                      && c.TimeText == SampleTime
                                                      && c.SampleTypesText.Contains(SampleTypeText)
                                                      select c).FirstOrDefault();

                            if (SampleExist == null)
                            {
                                MWQMSample mwqmSampleNew = new MWQMSample()
                                {
                                    MWQMSiteTVItemID = tvItemModelMWQMSite.TVItemID,
                                    MWQMRunTVItemID = mwqmRunModel.MWQMRunTVItemID,
                                    SampleDateTime_Local = SampleDate,
                                    TimeText = SampleTime,
                                    Depth_m = Depth,
                                    FecCol_MPN_100ml = FecCol,
                                    Salinity_PPT = Salinity,
                                    WaterTemp_C = Temperature,
                                    PH = null,
                                    SampleTypesText = SampleTypeText,
                                    SampleType_old = null,
                                    Tube_10 = null,
                                    Tube_1_0 = null,
                                    Tube_0_1 = null,
                                    ProcessedBy = null,
                                    UseForOpenData = UseForOpenData,
                                    LastUpdateDate_UTC = DateTime.UtcNow,
                                    LastUpdateContactTVItemID = 2,
                                };

                                using (CSSPDBEntities db2 = new CSSPDBEntities())
                                {
                                    try
                                    {
                                        db2.MWQMSamples.Add(mwqmSampleNew);
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText("Could not Add mwqmSampleNew\r\n");
                                        return;

                                    }

                                    MWQMSampleLanguage mwqmSampleLanguageNewEN = new MWQMSampleLanguage()
                                    {
                                        MWQMSampleID = mwqmSampleNew.MWQMSampleID,
                                        Language = (int)LanguageEnum.en,
                                        MWQMSampleNote = (string.IsNullOrWhiteSpace(BCSubsector.SR_OBS) == true ? "--" : BCSubsector.SR_OBS.Trim()),
                                        TranslationStatus = (int)TranslationStatusEnum.Translated,
                                        LastUpdateDate_UTC = DateTime.UtcNow,
                                        LastUpdateContactTVItemID = 2,
                                    };

                                    MWQMSampleLanguage mwqmSampleLanguageNewFR = new MWQMSampleLanguage()
                                    {
                                        MWQMSampleID = mwqmSampleNew.MWQMSampleID,
                                        Language = (int)LanguageEnum.fr,
                                        MWQMSampleNote = (string.IsNullOrWhiteSpace(BCSubsector.SR_OBS) == true ? "--" : BCSubsector.SR_OBS.Trim()),
                                        TranslationStatus = (int)TranslationStatusEnum.NotTranslated,
                                        LastUpdateDate_UTC = DateTime.UtcNow,
                                        LastUpdateContactTVItemID = 2,
                                    };

                                    try
                                    {
                                        db2.MWQMSampleLanguages.Add(mwqmSampleLanguageNewEN);
                                        db2.MWQMSampleLanguages.Add(mwqmSampleLanguageNewFR);
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText("Could not Add mwqmSampleLanguageNewEN and FR\r\n");
                                        return;

                                    }
                                }
                            }
                            else
                            {
                                bool ShouldUpdate = false;

                                //  MWQMSiteTVItemID, MWQMRunTVItemID, SampleDateTime_Local should already be ok from the query

                                if (SampleExist.SampleDateTime_Local != SampleDate)
                                {
                                    SampleExist.SampleDateTime_Local = SampleDate;
                                    ShouldUpdate = true;
                                }
                                if (SampleExist.TimeText != SampleTime)
                                {
                                    SampleExist.TimeText = SampleTime;
                                    ShouldUpdate = true;
                                }
                                if (SampleExist.Depth_m != Depth)
                                {
                                    SampleExist.Salinity_PPT = Depth;
                                    ShouldUpdate = true;
                                }
                                if (SampleExist.Salinity_PPT != Salinity)
                                {
                                    SampleExist.Salinity_PPT = Salinity;
                                    ShouldUpdate = true;
                                }
                                if (SampleExist.WaterTemp_C != Temperature)
                                {
                                    SampleExist.WaterTemp_C = Temperature;
                                    ShouldUpdate = true;
                                }
                                if (SampleExist.SampleTypesText != SampleTypeText)
                                {
                                    SampleExist.SampleTypesText = SampleTypeText;
                                    ShouldUpdate = true;
                                }
                                if (SampleExist.UseForOpenData != UseForOpenData)
                                {
                                    SampleExist.UseForOpenData = UseForOpenData;
                                }
                                SampleExist.LastUpdateDate_UTC = DateTime.UtcNow;
                                SampleExist.LastUpdateContactTVItemID = 2;

                                if (ShouldUpdate)
                                {
                                    try
                                    {
                                        dd.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText("Could not update SampleExist\r\n");
                                        return;

                                    }
                                }

                                if (ShouldUpdate)
                                {
                                    MWQMSampleLanguage mwqmSampleLanguageEN = (from c in dd.MWQMSampleLanguages
                                                                               where c.Language == (int)LanguageEnum.en
                                                                               && c.MWQMSampleID == SampleExist.MWQMSampleID
                                                                               select c).FirstOrDefault();

                                    if (mwqmSampleLanguageEN != null)
                                    {
                                        string MWQMSampleNote = (string.IsNullOrWhiteSpace(BCSubsector.SR_OBS) == true ? "--" : BCSubsector.SR_OBS.Trim());
                                        if (mwqmSampleLanguageEN.MWQMSampleNote != MWQMSampleNote)
                                        {
                                            mwqmSampleLanguageEN.MWQMSampleNote = (string.IsNullOrWhiteSpace(BCSubsector.SR_OBS) == true ? "--" : BCSubsector.SR_OBS.Trim());

                                            try
                                            {
                                                dd.SaveChanges();
                                            }
                                            catch (Exception)
                                            {
                                                richTextBoxStatus.AppendText("Could not update mwqmSampleLanguageEN\r\n");
                                                return;

                                            }
                                        }

                                    }
                                    else
                                    {
                                        MWQMSampleLanguage mwqmSampleLanguageNewEN = new MWQMSampleLanguage()
                                        {
                                            MWQMSampleID = SampleExist.MWQMSampleID,
                                            Language = (int)LanguageEnum.en,
                                            MWQMSampleNote = (string.IsNullOrWhiteSpace(BCSubsector.SR_OBS) == true ? "--" : BCSubsector.SR_OBS.Trim()),
                                            TranslationStatus = (int)TranslationStatusEnum.Translated,
                                            LastUpdateDate_UTC = DateTime.UtcNow,
                                            LastUpdateContactTVItemID = 2,
                                        };

                                        try
                                        {
                                            dd.MWQMSampleLanguages.Add(mwqmSampleLanguageNewEN);
                                            dd.SaveChanges();
                                        }
                                        catch (Exception)
                                        {
                                            richTextBoxStatus.AppendText("Could not Add mwqmSampleLanguageNewEN and FR\r\n");
                                            return;

                                        }
                                    }

                                    MWQMSampleLanguage mwqmSampleLanguageFR = (from c in dd.MWQMSampleLanguages
                                                                               where c.Language == (int)LanguageEnum.fr
                                                                               && c.MWQMSampleID == SampleExist.MWQMSampleID
                                                                               select c).FirstOrDefault();

                                    if (mwqmSampleLanguageFR != null)
                                    {
                                        string MWQMSampleNote = (string.IsNullOrWhiteSpace(BCSubsector.SR_OBS) == true ? "--" : BCSubsector.SR_OBS.Trim());
                                        if (mwqmSampleLanguageFR.MWQMSampleNote != MWQMSampleNote)
                                        {
                                            mwqmSampleLanguageFR.MWQMSampleNote = (string.IsNullOrWhiteSpace(BCSubsector.SR_OBS) == true ? "--" : BCSubsector.SR_OBS.Trim());

                                            try
                                            {
                                                dd.SaveChanges();
                                            }
                                            catch (Exception)
                                            {
                                                richTextBoxStatus.AppendText("Could not update mwqmSampleLanguageFR\r\n");
                                                return;

                                            }
                                        }

                                    }
                                    else
                                    {
                                        MWQMSampleLanguage mwqmSampleLanguageNewFR = new MWQMSampleLanguage()
                                        {
                                            MWQMSampleID = SampleExist.MWQMSampleID,
                                            Language = (int)LanguageEnum.fr,
                                            MWQMSampleNote = (string.IsNullOrWhiteSpace(BCSubsector.SR_OBS) == true ? "--" : BCSubsector.SR_OBS.Trim()),
                                            TranslationStatus = (int)TranslationStatusEnum.NotTranslated,
                                            LastUpdateDate_UTC = DateTime.UtcNow,
                                            LastUpdateContactTVItemID = 2,
                                        };

                                        try
                                        {
                                            dd.MWQMSampleLanguages.Add(mwqmSampleLanguageNewFR);
                                            dd.SaveChanges();
                                        }
                                        catch (Exception)
                                        {
                                            richTextBoxStatus.AppendText("Could not Add mwqmSampleLanguageNewEN and FR\r\n");
                                            return;

                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }

        }

        private void ButViewBCSubsectorNames_Click(object sender, EventArgs e)
        {

        }

        private void Button12_Click(object sender, EventArgs e)
        {
            TVItemService tvItemServiceR = new TVItemService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);
            MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemServiceR.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            TVItemModel tvItemModelCanada = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            TVItemModel tvItemModelProv = tvItemServiceR.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "British Columbia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            List<TVItemModel> tvItemModelSubsectorList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelProv.TVText + "\r\n");
                return;
            }

            List<TVItemModel> tvItemModelMWQMSiteList = tvItemServiceR.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.MWQMSite);
            if (tvItemModelMWQMSiteList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem MWQMSite for " + tvItemModelProv.TVText + "\r\n");
                return;
            }

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

            using (TempData.TempDataToolDBEntities dbDT = new TempData.TempDataToolDBEntities())
            {

                List<BCMarineSample> BCSampleListAll = (from c in dbDT.BCMarineSamples
                                                        select c).ToList();

                foreach (TVItemModel tvItemModelSubsector in tvItemModelSubsectorList)
                {
                    using (CSSPDBEntities db2 = new CSSPDBEntities())
                    {
                        lblStatus.Text = "Doing " + tvItemModelSubsector.TVText;
                        lblStatus.Refresh();
                        Application.DoEvents();

                        string subsector = tvItemModelSubsector.TVText;
                        if (tvItemModelSubsector.TVText.Contains(" "))
                        {
                            subsector = tvItemModelSubsector.TVText.Substring(0, tvItemModelSubsector.TVText.IndexOf(" "));
                        }

                        if (subsector.EndsWith("F"))
                        {
                            continue;
                        }

                        List<TVItemModel> tvItemModelSiteList = (from c in tvItemModelMWQMSiteList
                                                                 where c.ParentID == tvItemModelSubsector.TVItemID
                                                                 && c.TVText.EndsWith("F") == false
                                                                 select c).ToList();

                        foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
                        {
                            if (Cancel) return;

                            List<BCMarineSample> BCSampleList = (from c in BCSampleListAll
                                                                 where c.SR_STATION_CODE == tvItemModelSite.TVText
                                                                 orderby c.SR_READING_DATE, c.SR_READING_TIME, c.SR_FECAL_COLIFORM,
                                                                 c.SR_FECAL_COLIFORM_IND, c.SR_SAMPLE_DEPTH, c.SR_SALINITY, c.SR_TEMPERATURE, c.SR_SAMPLE_TYPE
                                                                 select c).ToList();

                            foreach (BCMarineSample bcMarineSample in BCSampleList)
                            {
                                if (bcMarineSample.SR_FECAL_COLIFORM == 0)
                                {
                                    bcMarineSample.SR_FECAL_COLIFORM = 1;
                                }
                                if (bcMarineSample.SR_FECAL_COLIFORM == 2 && bcMarineSample.SR_FECAL_COLIFORM_IND == "<")
                                {
                                    bcMarineSample.SR_FECAL_COLIFORM = 1;
                                }
                            }

                            lblStatus.Text = $"Doing { tvItemModelSubsector.TVText } --- { tvItemModelSite.TVText }";
                            lblStatus.Refresh();
                            Application.DoEvents();


                            List<MWQMSample> mwqmSiteMWQMSampleList = (from c in db2.MWQMSamples
                                                                       where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
                                                                       orderby c.SampleDateTime_Local, c.FecCol_MPN_100ml, c.SampleTypesText, c.Depth_m, c.TimeText
                                                                       select c).ToList();

                            List<MWQMSample> mwqmSampleToDeleteList = new List<MWQMSample>();


                            int Count = BCSampleList.Count - 1;
                            if (BCSampleList.Count > 1)
                            {
                                for (int i = 0; i < Count; i++)
                                {
                                    if (BCSampleList[i].SR_READING_DATE == BCSampleList[i + 1].SR_READING_DATE
                                        && BCSampleList[i].SR_READING_TIME == BCSampleList[i + 1].SR_READING_TIME
                                        && BCSampleList[i].SR_FECAL_COLIFORM == BCSampleList[i + 1].SR_FECAL_COLIFORM
                                        && BCSampleList[i].SR_FECAL_COLIFORM_IND == BCSampleList[i + 1].SR_FECAL_COLIFORM_IND
                                        && BCSampleList[i].SR_SAMPLE_DEPTH == BCSampleList[i + 1].SR_SAMPLE_DEPTH
                                        && BCSampleList[i].SR_SALINITY == BCSampleList[i + 1].SR_SALINITY
                                        && BCSampleList[i].SR_TEMPERATURE == BCSampleList[i + 1].SR_TEMPERATURE
                                        && BCSampleList[i].SR_SAMPLE_TYPE == BCSampleList[i + 1].SR_SAMPLE_TYPE
                                        )
                                    {
                                        richTextBoxStatus.AppendText($"{ subsector }\t{BCSampleList[i].SR_STATION_CODE}\t{((DateTime)BCSampleList[i].SR_READING_DATE).ToString("yyyy-MMMM-dd")}\t{BCSampleList[i].SR_READING_TIME}\t{BCSampleList[i].SR_FECAL_COLIFORM}\t{BCSampleList[i].SR_SAMPLE_TYPE}\t{BCSampleList[i].SR_SAMPLE_DEPTH}\r\n");
                                    }
                                }
                            }



                            //int Count = mwqmSiteMWQMSampleList.Count - 1;
                            //if (mwqmSiteMWQMSampleList.Count > 1)
                            //{
                            //    for (int i = 0; i < Count; i++)
                            //    {
                            //        if (string.IsNullOrWhiteSpace(mwqmSiteMWQMSampleList[i].TimeText))
                            //        {
                            //            mwqmSampleToDeleteList.Add(mwqmSiteMWQMSampleList[i]);
                            //            richTextBoxStatus.AppendText($"{ subsector } --- {tvItemModelSite.TVText} --- {mwqmSiteMWQMSampleList[i].SampleDateTime_Local} --- {mwqmSiteMWQMSampleList[i].FecCol_MPN_100ml}\r\n");
                            //        }
                            //    }
                            //}




                            //int Count = mwqmSiteMWQMSampleList.Count;
                            //for (int i = 0; i < Count; i++)
                            //{
                            //    List<BCMarineSample> BCSubsectorList = (from c in BCSampleList
                            //                                            where c.SR_READING_DATE.Value.Year == mwqmSiteMWQMSampleList[i].SampleDateTime_Local.Year
                            //                                            && c.SR_READING_DATE.Value.Month == mwqmSiteMWQMSampleList[i].SampleDateTime_Local.Month
                            //                                            && c.SR_READING_DATE.Value.Day == mwqmSiteMWQMSampleList[i].SampleDateTime_Local.Day
                            //                                            && c.SR_FECAL_COLIFORM == mwqmSiteMWQMSampleList[i].FecCol_MPN_100ml
                            //                                            select c).ToList();

                            //    bool exist = false;

                            //    foreach (BCMarineSample bcMarineSample in BCSubsectorList)
                            //    {
                            //        bool Exact = true;

                            //        SampleTypeEnum sampleType = SampleTypeEnum.Routine;

                            //        if (bcMarineSample.SR_SAMPLE_TYPE == "S")
                            //        {
                            //            sampleType = SampleTypeEnum.Sediment;
                            //        }

                            //        if (bcMarineSample.SR_SAMPLE_TYPE == "B")
                            //        {
                            //            sampleType = SampleTypeEnum.Bivalve;
                            //        }

                            //        string SampleTypeText = ((int)sampleType).ToString() + ",";

                            //        if (mwqmSiteMWQMSampleList[i].SampleTypesText != SampleTypeText)
                            //        {
                            //            Exact = false;
                            //        }

                            //        DateTime DayOfSample = (DateTime)(bcMarineSample.SR_READING_DATE);

                            //        string SampleTime = bcMarineSample.SR_READING_TIME;

                            //        if (SampleTime == null || SampleTime == "0")
                            //        {
                            //            SampleTime = "0000";
                            //        }

                            //        string hourText = SampleTime.Substring(0, 2);
                            //        string minText = SampleTime.Substring(2, 2);

                            //        int hour = int.Parse(hourText);
                            //        int min = int.Parse(minText);

                            //        DateTime SampleDate = new DateTime(DayOfSample.Year, DayOfSample.Month, DayOfSample.Day, hour, min, 0);

                            //        if (mwqmSiteMWQMSampleList[i].SampleDateTime_Local != SampleDate)
                            //        {
                            //            Exact = false;
                            //        }

                            //        int FecCol = 0;
                            //        if (bcMarineSample.SR_FECAL_COLIFORM_IND == "<" && bcMarineSample.SR_FECAL_COLIFORM == 2)
                            //        {
                            //            FecCol = 1;
                            //        }
                            //        else
                            //        {
                            //            if (bcMarineSample.SR_FECAL_COLIFORM == null)
                            //            {
                            //                continue;
                            //            }

                            //            FecCol = (int)bcMarineSample.SR_FECAL_COLIFORM;
                            //        }
                            //        if (FecCol == 0)
                            //        {
                            //            FecCol = 1;
                            //        }

                            //        if (mwqmSiteMWQMSampleList[i].FecCol_MPN_100ml != FecCol)
                            //        {
                            //            Exact = false;
                            //        }

                            //        if (mwqmSiteMWQMSampleList[i].Depth_m != bcMarineSample.SR_SAMPLE_DEPTH)
                            //        {
                            //            Exact = false;
                            //        }

                            //        if (mwqmSiteMWQMSampleList[i].Salinity_PPT != bcMarineSample.SR_SALINITY)
                            //        {
                            //            Exact = false;
                            //        }

                            //        if (mwqmSiteMWQMSampleList[i].WaterTemp_C != bcMarineSample.SR_TEMPERATURE)
                            //        {
                            //            Exact = false;
                            //        }

                            //        if (Exact)
                            //        {
                            //            exist = true;
                            //            break;
                            //        }
                            //    }

                            //    if (!exist)
                            //    {
                            //        mwqmSampleToDeleteList.Add(mwqmSiteMWQMSampleList[i]);
                            //        richTextBoxStatus.AppendText($"{ subsector } --- {tvItemModelSite.TVText} --- {mwqmSiteMWQMSampleList[i].SampleDateTime_Local} --- {mwqmSiteMWQMSampleList[i].FecCol_MPN_100ml}\r\n");
                            //    }

                            //}





                            //int Count = mwqmSiteMWQMSampleList.Count - 1;
                            //if (mwqmSiteMWQMSampleList.Count > 1)
                            //{
                            //    for (int i = 0; i < Count; i++)
                            //    {
                            //        if (mwqmSiteMWQMSampleList[i].SampleDateTime_Local.Year == mwqmSiteMWQMSampleList[i + 1].SampleDateTime_Local.Year
                            //            && mwqmSiteMWQMSampleList[i].SampleDateTime_Local.Month == mwqmSiteMWQMSampleList[i + 1].SampleDateTime_Local.Month
                            //            && mwqmSiteMWQMSampleList[i].SampleDateTime_Local.Day == mwqmSiteMWQMSampleList[i + 1].SampleDateTime_Local.Day
                            //            && mwqmSiteMWQMSampleList[i].FecCol_MPN_100ml == mwqmSiteMWQMSampleList[i + 1].FecCol_MPN_100ml
                            //            && mwqmSiteMWQMSampleList[i].Depth_m == mwqmSiteMWQMSampleList[i + 1].Depth_m
                            //            && mwqmSiteMWQMSampleList[i].TimeText == mwqmSiteMWQMSampleList[i + 1].TimeText
                            //            && mwqmSiteMWQMSampleList[i].SampleTypesText == mwqmSiteMWQMSampleList[i + 1].SampleTypesText
                            //            )
                            //        {
                            //            mwqmSampleToDeleteList.Add(mwqmSiteMWQMSampleList[i + 1]);
                            //            richTextBoxStatus.AppendText($"{ subsector } --- {tvItemModelSite.TVText} --- {mwqmSiteMWQMSampleList[i + 1].SampleDateTime_Local} --- {mwqmSiteMWQMSampleList[i + 1].FecCol_MPN_100ml}\r\n");
                            //        }
                            //    }
                            //}




                            //try
                            //{
                            //    foreach (MWQMSample mwqmSample in mwqmSampleToDeleteList)
                            //    {
                            //        db2.MWQMSamples.Remove(mwqmSample);
                            //    }

                            //    db2.SaveChanges();
                            //}
                            //catch (Exception)
                            //{
                            //    int sleifj = 34;
                            //}


                            //lblStatus.Text = $"Doing { tvItemModelSubsector.TVText } [{BCSampleList.Count}] --- { tvItemModelSite.TVText } [{mwqmSiteMWQMSampleList.Count}]";
                            //lblStatus.Refresh();
                            //Application.DoEvents();

                            //if (BCSampleList.Count != mwqmSiteMWQMSampleList.Count)
                            //{
                            //    richTextBoxStatus.AppendText($"{ subsector } --- {tvItemModelSite.TVText} --- BC [{BCSampleList.Count}] CSSP [{mwqmSiteMWQMSampleList.Count}]\r\n");
                            //}
                        }
                    }
                }
            }
        }

        private void Button13_Click(object sender, EventArgs e)
        {
            DateTime date_local = DateTime.Now;

            List<string> zoneTextList = new List<string>()
            {
                "Newfoundland Standard Time",
                "Atlantic Standard Time",
                "Eastern Standard Time",
                "Pacific Standard Time"
            };

            List<DateTime> dateTimeList = new List<DateTime>()
            {
                DateTime.Now,
                DateTime.Now.AddMonths(1).AddHours(8),
                DateTime.Now.AddMonths(2).AddHours(8),
                DateTime.Now.AddMonths(3).AddHours(8),
                DateTime.Now.AddMonths(4).AddHours(8),
                DateTime.Now.AddMonths(5).AddHours(8),
                DateTime.Now.AddMonths(6).AddHours(8),
                DateTime.Now.AddMonths(7).AddHours(8),
                DateTime.Now.AddMonths(8).AddHours(8),
                DateTime.Now.AddMonths(9).AddHours(8),
                DateTime.Now.AddMonths(10).AddHours(8),
                DateTime.Now.AddMonths(11).AddHours(8),
                DateTime.Now.AddMonths(12).AddHours(8),
            };


            foreach (string zoneText in zoneTextList)
            {
                TimeZoneInfo tst = TimeZoneInfo.FindSystemTimeZoneById(zoneText);

                foreach (DateTime dt in dateTimeList)
                {
                    bool IDST = tst.IsDaylightSavingTime(dt);
                    DateTime dateUTC = dt.ToUniversalTime();
                    TimeSpan ts = tst.GetUtcOffset(dt);
                    richTextBoxStatus.AppendText($"{zoneText} --- {dt.ToString("yyyy-MMMM-ddThh:mm:ss")} -- IsDaylightSavingTime [{IDST}] --- UTC [{dateUTC.ToString("O")}] --- offset [{ts.Hours}.{ts.Minutes}] \r\n");
                }
            }
        }

        private void Button14_Click(object sender, EventArgs e)
        {
            DateTime dateTime = DateTime.Parse(textBox1.Text);

            richTextBoxStatus.Text = "";
            richTextBoxStatus.AppendText($"Date read [{dateTime}]\r\n");

            richTextBoxStatus.AppendText($"UTC date [{dateTime.ToUniversalTime()}]");

            richTextBoxStatus.AppendText($"output format [{dateTime.ToString(textBox2.Text)}]");
        }

        private string RemoveAllSectorNotInQCDB(List<StationQC> stationList, TVItemService tvItemService, TVItemModel tvItemModelQC)
        {
            // -----------------------------------------------------------------
            // ------ remove all sectors and under info that are not in QC DB --
            // -----------------------------------------------------------------

            List<string> AreaList = (from c in stationList
                                     select c.secteur).Distinct().ToList();

            List<string> SectorList = new List<string>();
            foreach (string s in AreaList)
            {
                if (s.Length > 3)
                {
                    string sector = s.Substring(0, 4);

                    if (!SectorList.Contains(sector))
                    {
                        SectorList.Add(sector);
                        //richTextBoxStatus.AppendText($"{sector}\r\n");
                    }
                }
                else
                {
                    SectorList.Add(s);
                }
            }

            List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Sector);

            foreach (TVItemModel tvItemModel in tvItemModelSectorList)
            {
                string sector = tvItemModel.TVText;
                if (sector.Contains(" "))
                {
                    sector = sector.Substring(0, sector.IndexOf(" "));
                }

                lblStatus.Text = sector;
                lblStatus.Refresh();
                Application.DoEvents();

                if (!SectorList.Contains(sector))
                {
                    richTextBoxStatus.AppendText($"{sector}\r\n");
                    List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModel.TVItemID, TVTypeEnum.Subsector);

                    foreach (TVItemModel tvItemModelss in tvItemModelSubsectorList)
                    {
                        List<TVItemModel> tvItemModelSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelss.TVItemID, TVTypeEnum.MWQMSite);
                        List<TVItemModel> tvItemModelRunList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelss.TVItemID, TVTypeEnum.MWQMRun);
                        List<TVItemModel> tvItemModelPSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelss.TVItemID, TVTypeEnum.PolSourceSite);

                        using (CSSPDBEntities db2 = new CSSPDBEntities())
                        {
                            // Deleting TVItemStats of MWQMSites
                            foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
                            {
                                List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
                                                                   where c.TVItemID == tvItemModelSite.TVItemID
                                                                   select c).ToList();

                                if (tvItemStatList.Count > 0)
                                {
                                    db2.TVItemStats.RemoveRange(tvItemStatList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMSites");
                                    }
                                }
                            }

                            // Deleting TVItemStats of MWQMRuns
                            foreach (TVItemModel tvItemModelRun in tvItemModelRunList)
                            {
                                List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
                                                                   where c.TVItemID == tvItemModelRun.TVItemID
                                                                   select c).ToList();

                                if (tvItemStatList.Count > 0)
                                {
                                    db2.TVItemStats.RemoveRange(tvItemStatList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception )
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMRuns");
                                    }
                                }
                            }

                            // Deleting TVItemStats of PSS
                            foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
                            {
                                List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
                                                                   where c.TVItemID == tvItemModelPSS.TVItemID
                                                                   select c).ToList();

                                if (tvItemStatList.Count > 0)
                                {
                                    db2.TVItemStats.RemoveRange(tvItemStatList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMSites");
                                    }
                                }
                            }

                            // Deleting MWQMSamples
                            foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
                            {
                                List<MWQMSample> mwqmSampleList = (from c in db2.MWQMSamples
                                                                   where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
                                                                   select c).ToList();

                                if (mwqmSampleList.Count > 0)
                                {
                                    db2.MWQMSamples.RemoveRange(mwqmSampleList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete Sample");
                                    }
                                }
                            }


                            // Deleting MapInfos for MWQMSites
                            foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
                            {
                                List<MapInfo> MapInfoList = (from c in db2.MapInfos
                                                             where c.TVItemID == tvItemModelSite.TVItemID
                                                             select c).ToList();


                                if (MapInfoList.Count > 0)
                                {
                                    db2.MapInfos.RemoveRange(MapInfoList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete MapInfos for MWQMSites");
                                    }
                                }
                            }

                            // Deleting MapInfos for PSS
                            foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
                            {
                                List<MapInfo> MapInfoList = (from c in db2.MapInfos
                                                             where c.TVItemID == tvItemModelPSS.TVItemID
                                                             select c).ToList();


                                if (MapInfoList.Count > 0)
                                {
                                    db2.MapInfos.RemoveRange(MapInfoList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete MapInfos for PSS");
                                    }
                                }
                            }

                            // Deleting MWQMSites
                            foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
                            {
                                List<MWQMSite> mwqmSiteList = (from c in db2.MWQMSites
                                                               where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
                                                               select c).ToList();

                                if (mwqmSiteList.Count > 0)
                                {
                                    db2.MWQMSites.RemoveRange(mwqmSiteList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
                                    }
                                }
                            }

                            // Deleting TVItems for MWQMSites
                            foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
                            {
                                List<TVItem> TVItemList = (from c in db2.TVItems
                                                           where c.TVItemID == tvItemModelSite.TVItemID
                                                           select c).ToList();

                                if (TVItemList.Count > 0)
                                {
                                    db2.TVItems.RemoveRange(TVItemList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
                                    }
                                }
                            }

                            // Deleting PSS
                            foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
                            {
                                List<PolSourceSite> polSourceSiteList = (from c in db2.PolSourceSites
                                                                         where c.PolSourceSiteTVItemID == tvItemModelPSS.TVItemID
                                                                         select c).ToList();

                                if (polSourceSiteList.Count > 0)
                                {
                                    db2.PolSourceSites.RemoveRange(polSourceSiteList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItems for PSS");
                                    }
                                }
                            }

                            // Deleting TVItems for PSS
                            foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
                            {
                                List<TVItem> TVItemList = (from c in db2.TVItems
                                                           where c.TVItemID == tvItemModelPSS.TVItemID
                                                           select c).ToList();

                                if (TVItemList.Count > 0)
                                {
                                    db2.TVItems.RemoveRange(TVItemList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItems for PSS");
                                    }
                                }
                            }

                            // Deleting MWQMRuns
                            foreach (TVItemModel tvItemModelRun in tvItemModelRunList)
                            {
                                List<MWQMRun> mwqmRunList = (from c in db2.MWQMRuns
                                                             where c.MWQMRunTVItemID == tvItemModelRun.TVItemID
                                                             select c).ToList();

                                if (mwqmRunList.Count > 0)
                                {
                                    db2.MWQMRuns.RemoveRange(mwqmRunList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
                                    }
                                }
                            }

                            // Deleting TVItems for MWQMRuns
                            foreach (TVItemModel tvItemModelRun in tvItemModelRunList)
                            {
                                List<TVItem> TVItemList = (from c in db2.TVItems
                                                           where c.TVItemID == tvItemModelRun.TVItemID
                                                           select c).ToList();

                                if (TVItemList.Count > 0)
                                {
                                    db2.TVItems.RemoveRange(TVItemList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMRuns");
                                    }
                                }
                            }

                            // Deleting TVItemStats of TVItemModelss
                            List<TVItemStat> tvItemStatList3 = (from c in db2.TVItemStats
                                                                where c.TVItemID == tvItemModelss.TVItemID
                                                                select c).ToList();

                            if (tvItemStatList3.Count > 0)
                            {
                                db2.TVItemStats.RemoveRange(tvItemStatList3);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete TVItemStats of TVItemModelss");
                                }
                            }

                            // Deleting MapInfos of TVItemModelss
                            List<MapInfo> MapInfoList3 = (from c in db2.MapInfos
                                                          where c.TVItemID == tvItemModelss.TVItemID
                                                          select c).ToList();


                            if (MapInfoList3.Count > 0)
                            {
                                db2.MapInfos.RemoveRange(MapInfoList3);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete MapInfos of TVItemModelss");
                                }
                            }

                            // Deleting MWQMSubsectors for TVItemModelss
                            List<MWQMSubsector> mwqmSubsectorList = (from c in db2.MWQMSubsectors
                                                                     where c.MWQMSubsectorTVItemID == tvItemModelss.TVItemID
                                                                     select c).ToList();

                            if (mwqmSubsectorList.Count > 0)
                            {
                                db2.MWQMSubsectors.RemoveRange(mwqmSubsectorList);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete MWQMSubsectors for TVItemModelss");
                                }
                            }

                            // Deleting UseOfSites for TVItemModelss
                            List<UseOfSite> UseOfSiteList = (from c in db2.UseOfSites
                                                             where c.SubsectorTVItemID == tvItemModelss.TVItemID
                                                             select c).ToList();

                            if (UseOfSiteList.Count > 0)
                            {
                                db2.UseOfSites.RemoveRange(UseOfSiteList);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete UseOfSites for TVItemModelss");
                                }
                            }


                            // Deleting TVItems for TVItemModelss
                            List<TVItem> TVItemList3 = (from c in db2.TVItems
                                                        where c.TVItemID == tvItemModelss.TVItemID
                                                        select c).ToList();

                            if (TVItemList3.Count > 0)
                            {
                                db2.TVItems.RemoveRange(TVItemList3);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete TVItems for TVItemModelss");
                                }
                            }
                        }
                    }

                    // now we should be able to remove the sector
                    using (CSSPDBEntities db2 = new CSSPDBEntities())
                    {
                        // Deleting TVItemStats of tvItemModel
                        List<TVItemStat> tvItemStatList2 = (from c in db2.TVItemStats
                                                            where c.TVItemID == tvItemModel.TVItemID
                                                            select c).ToList();

                        if (tvItemStatList2.Count > 0)
                        {
                            db2.TVItemStats.RemoveRange(tvItemStatList2);

                            try
                            {
                                db2.SaveChanges();
                            }
                            catch (Exception)
                            {
                                richTextBoxStatus.AppendText($"Could not delete TVItemStats of tvItemModel");
                            }
                        }

                        // Deleting MapInfos of tvItemModel
                        List<MapInfo> MapInfoList2 = (from c in db2.MapInfos
                                                      where c.TVItemID == tvItemModel.TVItemID
                                                      select c).ToList();


                        if (MapInfoList2.Count > 0)
                        {
                            db2.MapInfos.RemoveRange(MapInfoList2);

                            try
                            {
                                db2.SaveChanges();
                            }
                            catch (Exception)
                            {
                                richTextBoxStatus.AppendText($"Could not delete MapInfos of tvItemModel");
                            }
                        }
                        // Deleting TVItems for tvItemModel
                        List<TVItem> TVItemList2 = (from c in db2.TVItems
                                                    where c.TVItemID == tvItemModel.TVItemID
                                                    select c).ToList();

                        if (TVItemList2.Count > 0)
                        {
                            db2.TVItems.RemoveRange(TVItemList2);

                            try
                            {
                                db2.SaveChanges();
                            }
                            catch (Exception)
                            {
                                richTextBoxStatus.AppendText($"Could not delete TVItems for tvItemModel");
                            }
                        }
                    }
                }
            }
            lblStatus.Text = "done...";


            return "";
        }

        private string AddAllNewSectorFoundInQCDB(List<StationQC> stationList, TVItemService tvItemService, TVItemModel tvItemModelQC)
        {
            /// -----------------------------------------------------------------
            /// ------ Add all new sectors found in QC DB --
            /// -----------------------------------------------------------------

            List<TVItemModel> tvItemModelAreaList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Area);
            List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Sector);


            List<string> SubsectorListQC = (from c in stationList
                                            select c.secteur).Distinct().ToList();

            List<string> SectorList = new List<string>();
            foreach (string s in SubsectorListQC)
            {
                if (s.Length > 3)
                {
                    string sector = s.Substring(0, 4);

                    if (!SectorList.Contains(sector))
                    {
                        SectorList.Add(sector);
                    }
                }
                else
                {
                    SectorList.Add(s);
                }
            }

            foreach (TVItemModel tvItemModel in tvItemModelSectorList)
            {
                string sector = tvItemModel.TVText;
                if (sector.Contains(" "))
                {
                    sector = sector.Substring(0, sector.IndexOf(" "));
                }

                if (!SectorList.Contains(sector))
                {
                    richTextBoxStatus.AppendText($"{sector} does not exist\t should delete it\r\n");
                }
            }

            foreach (string sector in SectorList)
            {
                lblStatus.Text = sector;
                lblStatus.Refresh();
                Application.DoEvents();

                if (!tvItemModelSectorList.Where(c => c.TVText.StartsWith(sector + " ")).Any())
                {
                    richTextBoxStatus.AppendText($"{sector} does not exist\t should create it\r\n");

                    // add tvitems and tvitemlanguages under the proper Area
                    // 

                    string AreaText = sector.Substring(0, 1);

                    TVItemModel tvItemModelArea = tvItemModelAreaList.Where(c => c.TVText.Contains(AreaText + " ")).FirstOrDefault();

                    if (tvItemModelArea != null)
                    {
                        TVItemModel tvItemModelSector = tvItemService.PostAddChildTVItemDB(tvItemModelArea.TVItemID, sector + " (vide)", TVTypeEnum.Sector);
                        if (!string.IsNullOrWhiteSpace(tvItemModelSector.Error))
                        {
                            richTextBoxStatus.AppendText($"{tvItemModelSector.Error}\r\n");
                            return tvItemModelSector.Error;
                        }

                        var geo_stations_pList2 = (from c in stationList
                                                   where c.secteur.StartsWith(sector)
                                                   select new { c.x, c.y }).ToList();

                        float MinLat = (float)((from c in geo_stations_pList2
                                                select c.y).Min());

                        float MaxLat = (float)((from c in geo_stations_pList2
                                                select c.y).Max());

                        float MinLng = (float)((from c in geo_stations_pList2
                                                select c.x).Min());

                        float MaxLng = (float)((from c in geo_stations_pList2
                                                select c.x).Max());

                        List<Coord> coordList = new List<Coord>()
                            {
                                new Coord() { Lat = (MaxLat + MinLat)/2.0f, Lng = (MaxLng + MinLng)/2.0f, Ordinal = 0 },
                            };

                        MapInfoModel mapInfoModel = tvItemService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Sector, tvItemModelSector.TVItemID);
                        if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        {
                            richTextBoxStatus.AppendText($"{mapInfoModel.Error}\r\n");
                            return mapInfoModel.Error;
                        }

                        coordList = new List<Coord>()
                            {
                                new Coord() { Lat = MinLat, Lng = MinLng, Ordinal = 0 },
                                new Coord() { Lat = MinLat, Lng = MaxLng, Ordinal = 1 },
                                new Coord() { Lat = MaxLat, Lng = MaxLng, Ordinal = 2 },
                                new Coord() { Lat = MaxLat, Lng = MinLng, Ordinal = 3 },
                                new Coord() { Lat = MinLat, Lng = MinLng, Ordinal = 4 },
                            };

                        mapInfoModel = tvItemService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Polygon, TVTypeEnum.Sector, tvItemModelSector.TVItemID);
                        if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        {
                            richTextBoxStatus.AppendText($"{mapInfoModel.Error}\r\n");
                            return mapInfoModel.Error;
                        }
                    }
                }
            }

            lblStatus.Text = "done...";

            return "";
        }

        private string RemoveAllSubsectorNotInQCDB(List<StationQC> stationList, TVItemService tvItemService, TVItemModel tvItemModelQC)
        {
            /// -----------------------------------------------------------------
            /// ------ Remove all subsectors and under info that are not in QC DB --
            /// -----------------------------------------------------------------

            List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Sector);
            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);


            List<string> SubsectorListQC = (from c in stationList
                                            select c.secteur).Distinct().ToList();

            List<string> SubsectorList = new List<string>();
            foreach (string s in SubsectorListQC)
            {
                if (s.Length > 3)
                {
                    if (!SubsectorList.Contains(s))
                    {
                        SubsectorList.Add(s);
                    }
                }
                else
                {
                    SubsectorList.Add(s);
                }
            }


            foreach (TVItemModel tvItemModel in tvItemModelSubsectorList)
            {
                string subsector = tvItemModel.TVText;
                if (subsector.Contains(" "))
                {
                    subsector = subsector.Substring(0, subsector.IndexOf(" "));
                }

                lblStatus.Text = subsector;
                lblStatus.Refresh();
                Application.DoEvents();

                if (!SubsectorList.Contains(subsector))
                {
                    richTextBoxStatus.AppendText($"{subsector} does not exist\t should delete it\r\n");

                    if (!SubsectorList.Contains(subsector))
                    {
                        richTextBoxStatus.AppendText($"{subsector}\r\n");
                        List<TVItemModel> tvItemModelSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModel.TVItemID, TVTypeEnum.MWQMSite);
                        List<TVItemModel> tvItemModelRunList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModel.TVItemID, TVTypeEnum.MWQMRun);
                        List<TVItemModel> tvItemModelPSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModel.TVItemID, TVTypeEnum.PolSourceSite);

                        using (CSSPDBEntities db2 = new CSSPDBEntities())
                        {
                            // Deleting TVItemStats of MWQMSites
                            foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
                            {
                                List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
                                                                   where c.TVItemID == tvItemModelSite.TVItemID
                                                                   select c).ToList();

                                if (tvItemStatList.Count > 0)
                                {
                                    db2.TVItemStats.RemoveRange(tvItemStatList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMSites");
                                        return $"Could not delete TVItemStats of MWQMSites";
                                    }
                                }
                            }

                            // Deleting TVItemStats of MWQMRuns
                            foreach (TVItemModel tvItemModelRun in tvItemModelRunList)
                            {
                                List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
                                                                   where c.TVItemID == tvItemModelRun.TVItemID
                                                                   select c).ToList();

                                if (tvItemStatList.Count > 0)
                                {
                                    db2.TVItemStats.RemoveRange(tvItemStatList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMRuns");
                                        return $"Could not delete TVItemStats of MWQMRuns";
                                    }
                                }
                            }

                            // Deleting TVItemStats of PSS
                            foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
                            {
                                List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
                                                                   where c.TVItemID == tvItemModelPSS.TVItemID
                                                                   select c).ToList();

                                if (tvItemStatList.Count > 0)
                                {
                                    db2.TVItemStats.RemoveRange(tvItemStatList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMSites");
                                        return $"Could not delete TVItemStats of MWQMSites";
                                    }
                                }
                            }

                            // Deleting MWQMSamples
                            foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
                            {
                                lblStatus.Text = subsector + " -- " + tvItemModelSite.TVText + " samples delete";
                                lblStatus.Refresh();
                                Application.DoEvents();

                                List<MWQMSample> mwqmSampleList = (from c in db2.MWQMSamples
                                                                   where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
                                                                   select c).ToList();

                                if (mwqmSampleList.Count > 0)
                                {
                                    db2.MWQMSamples.RemoveRange(mwqmSampleList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete Sample");
                                        return $"Could not delete Sample";
                                    }
                                }
                            }


                            // Deleting MapInfos for MWQMSites
                            foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
                            {
                                List<MapInfo> MapInfoList = (from c in db2.MapInfos
                                                             where c.TVItemID == tvItemModelSite.TVItemID
                                                             select c).ToList();


                                if (MapInfoList.Count > 0)
                                {
                                    db2.MapInfos.RemoveRange(MapInfoList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete MapInfos for MWQMSites");
                                        return $"Could not delete MapInfos for MWQMSites";
                                    }
                                }
                            }

                            // Deleting MapInfos for PSS
                            foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
                            {
                                List<MapInfo> MapInfoList = (from c in db2.MapInfos
                                                             where c.TVItemID == tvItemModelPSS.TVItemID
                                                             select c).ToList();


                                if (MapInfoList.Count > 0)
                                {
                                    db2.MapInfos.RemoveRange(MapInfoList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete MapInfos for PSS");
                                        return $"Could not delete MapInfos for PSS";
                                    }
                                }
                            }

                            // Deleting MWQMSites
                            foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
                            {
                                List<MWQMSite> mwqmSiteList = (from c in db2.MWQMSites
                                                               where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
                                                               select c).ToList();

                                if (mwqmSiteList.Count > 0)
                                {
                                    db2.MWQMSites.RemoveRange(mwqmSiteList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
                                        return $"Could not delete TVItems for MWQMSites";
                                    }
                                }
                            }

                            // Deleting TVItems for MWQMSites
                            foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
                            {
                                List<TVItem> TVItemList = (from c in db2.TVItems
                                                           where c.TVItemID == tvItemModelSite.TVItemID
                                                           select c).ToList();

                                if (TVItemList.Count > 0)
                                {
                                    db2.TVItems.RemoveRange(TVItemList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
                                        return $"Could not delete TVItems for MWQMSites";
                                    }
                                }
                            }

                            // Deleting PSS
                            foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
                            {
                                List<PolSourceSite> polSourceSiteList = (from c in db2.PolSourceSites
                                                                         where c.PolSourceSiteTVItemID == tvItemModelPSS.TVItemID
                                                                         select c).ToList();

                                if (polSourceSiteList.Count > 0)
                                {
                                    db2.PolSourceSites.RemoveRange(polSourceSiteList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItems for PSS");
                                        return $"Could not delete TVItems for PSS";
                                    }
                                }
                            }

                            // Deleting TVItems for PSS
                            foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
                            {
                                List<TVItem> TVItemList = (from c in db2.TVItems
                                                           where c.TVItemID == tvItemModelPSS.TVItemID
                                                           select c).ToList();

                                if (TVItemList.Count > 0)
                                {
                                    db2.TVItems.RemoveRange(TVItemList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItems for PSS");
                                        return $"Could not delete TVItems for PSS";
                                    }
                                }
                            }

                            // Deleting MWQMRuns
                            foreach (TVItemModel tvItemModelRun in tvItemModelRunList)
                            {
                                List<MWQMRun> mwqmRunList = (from c in db2.MWQMRuns
                                                             where c.MWQMRunTVItemID == tvItemModelRun.TVItemID
                                                             select c).ToList();

                                if (mwqmRunList.Count > 0)
                                {
                                    db2.MWQMRuns.RemoveRange(mwqmRunList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
                                        return $"Could not delete TVItems for MWQMSites";
                                    }
                                }
                            }

                            // Deleting TVItems for MWQMRuns
                            foreach (TVItemModel tvItemModelRun in tvItemModelRunList)
                            {
                                List<TVItem> TVItemList = (from c in db2.TVItems
                                                           where c.TVItemID == tvItemModelRun.TVItemID
                                                           select c).ToList();

                                if (TVItemList.Count > 0)
                                {
                                    db2.TVItems.RemoveRange(TVItemList);

                                    try
                                    {
                                        db2.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMRuns");
                                        return $"Could not delete TVItems for MWQMRuns";
                                    }
                                }
                            }

                            // Deleting TVItemStats of TVItemModel
                            List<TVItemStat> tvItemStatList3 = (from c in db2.TVItemStats
                                                                where c.TVItemID == tvItemModel.TVItemID
                                                                select c).ToList();

                            if (tvItemStatList3.Count > 0)
                            {
                                db2.TVItemStats.RemoveRange(tvItemStatList3);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete TVItemStats of TVItemModel");
                                    return $"Could not delete TVItemStats of TVItemModel";
                                }
                            }

                            // Deleting MapInfos of TVItemModel
                            List<MapInfo> MapInfoList3 = (from c in db2.MapInfos
                                                          where c.TVItemID == tvItemModel.TVItemID
                                                          select c).ToList();


                            if (MapInfoList3.Count > 0)
                            {
                                db2.MapInfos.RemoveRange(MapInfoList3);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete MapInfos of TVItemModel");
                                    return $"Could not delete MapInfos of TVItemModel";
                                }
                            }

                            // Deleting MWQMSubsectors for TVItemModel
                            List<MWQMSubsector> mwqmSubsectorList = (from c in db2.MWQMSubsectors
                                                                     where c.MWQMSubsectorTVItemID == tvItemModel.TVItemID
                                                                     select c).ToList();

                            if (mwqmSubsectorList.Count > 0)
                            {
                                db2.MWQMSubsectors.RemoveRange(mwqmSubsectorList);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete MWQMSubsectors for TVItemModel");
                                    return $"Could not delete MWQMSubsectors for TVItemModel";
                                }
                            }

                            // Deleting UseOfSites for TVItemModel
                            List<UseOfSite> UseOfSiteList = (from c in db2.UseOfSites
                                                             where c.SubsectorTVItemID == tvItemModel.TVItemID
                                                             select c).ToList();

                            if (UseOfSiteList.Count > 0)
                            {
                                db2.UseOfSites.RemoveRange(UseOfSiteList);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete UseOfSites for TVItemModelss");
                                    return $"Could not delete UseOfSites for TVItemModelss";
                                }
                            }


                            // Deleting TVItems for TVItemModel
                            List<TVItem> TVItemList3 = (from c in db2.TVItems
                                                        where c.TVItemID == tvItemModel.TVItemID
                                                        select c).ToList();

                            if (TVItemList3.Count > 0)
                            {
                                db2.TVItems.RemoveRange(TVItemList3);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete TVItems for TVItemModel");
                                    return $"Could not delete TVItems for TVItemModel";
                                }
                            }
                        }
                    }
                }
            }

            lblStatus.Text = "done...";

            return "";
        }

        private string AddAllNewSubsectorFoundInQCDB(List<StationQC> stationList, TVItemService tvItemService, TVItemModel tvItemModelQC)
        {
            /// -----------------------------------------------------------------
            /// ------ Add all new subsectors found in QC DB and not in CSSPDB --
            /// -----------------------------------------------------------------

            List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Sector);
            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);


            List<string> SubsectorListQC = (from c in stationList
                                            select c.secteur).Distinct().ToList();

            List<string> SubsectorList = new List<string>();
            foreach (string s in SubsectorListQC)
            {
                if (s.Length > 3)
                {
                    if (!SubsectorList.Contains(s))
                    {
                        SubsectorList.Add(s);
                    }
                }
                else
                {
                    SubsectorList.Add(s);
                }
            }

            foreach (string subsector in SubsectorList)
            {
                lblStatus.Text = subsector;
                lblStatus.Refresh();
                Application.DoEvents();

                if (!tvItemModelSubsectorList.Where(c => c.TVText.StartsWith(subsector + " ")).Any())
                {
                    richTextBoxStatus.AppendText($"{subsector} does not exist\t creating it\r\n");

                    string SectorText = "";
                    if (subsector == "S")
                    {
                        SectorText = subsector.Substring(0, 1);
                    }
                    else
                    {
                        SectorText = subsector.Substring(0, 4);
                    }

                    TVItemModel tvItemModelSector = tvItemModelSectorList.Where(c => c.TVText.Contains(SectorText + " ")).FirstOrDefault();

                    if (tvItemModelSector == null)
                    {
                        richTextBoxStatus.AppendText($"{SectorText} could not be found\r\n");
                        return $"{SectorText} could not be found";
                    }

                    TVItemModel tvItemModelSubsector = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelSector.TVItemID, subsector + " ", TVTypeEnum.Subsector);
                    if (!string.IsNullOrWhiteSpace(tvItemModelSubsector.Error))
                    {
                        TVItemModel tvItemModelSubsectorRet = tvItemService.PostAddChildTVItemDB(tvItemModelSector.TVItemID, subsector + " (vide)", TVTypeEnum.Subsector);
                        if (!string.IsNullOrWhiteSpace(tvItemModelSubsectorRet.Error))
                        {
                            richTextBoxStatus.AppendText($"{tvItemModelSubsectorRet.Error}\r\n");
                            return $"{tvItemModelSubsectorRet.Error}";
                        }

                        var geo_stations_pList2 = (from c in stationList
                                                   where c.secteur.StartsWith(subsector)
                                                   select c).ToList();

                        float MinLat = (float)((from c in geo_stations_pList2
                                                select c.y).Min());

                        float MaxLat = (float)((from c in geo_stations_pList2
                                                select c.y).Max());

                        float MinLng = (float)((from c in geo_stations_pList2
                                                select c.x).Min());

                        float MaxLng = (float)((from c in geo_stations_pList2
                                                select c.x).Max());

                        List<Coord> coordList = new List<Coord>()
                            {
                                new Coord() { Lat = (MaxLat + MinLat)/2.0f, Lng = (MaxLng + MinLng)/2.0f, Ordinal = 0 },
                            };

                        MapInfoModel mapInfoModel = tvItemService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Subsector, tvItemModelSubsectorRet.TVItemID);
                        if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        {
                            richTextBoxStatus.AppendText($"{mapInfoModel.Error}\r\n");
                            return $"{mapInfoModel.Error}";
                        }

                        coordList = new List<Coord>()
                            {
                                new Coord() { Lat = MinLat, Lng = MinLng, Ordinal = 0 },
                                new Coord() { Lat = MinLat, Lng = MaxLng, Ordinal = 1 },
                                new Coord() { Lat = MaxLat, Lng = MaxLng, Ordinal = 2 },
                                new Coord() { Lat = MaxLat, Lng = MinLng, Ordinal = 3 },
                                new Coord() { Lat = MinLat, Lng = MinLng, Ordinal = 4 },
                            };

                        mapInfoModel = tvItemService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Polygon, TVTypeEnum.Subsector, tvItemModelSubsectorRet.TVItemID);
                        if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        {
                            richTextBoxStatus.AppendText($"{mapInfoModel.Error}\r\n");
                            return $"{mapInfoModel.Error}";
                        }
                    }

                }
            }

            lblStatus.Text = "done...";

            return "";
        }

        private string AddAllNewSitesFoundInQCDB(List<StationQC> stationList, TVItemService tvItemService, TVItemModel tvItemModelQC, MWQMSiteService mwqmSiteService)
        {
            /// -----------------------------------------------------------------
            /// ------ Add all new MWQMSites found in QC DB and not in CSSPDB --
            /// -----------------------------------------------------------------

            List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Sector);
            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);


            List<string> SubsectorListQC = (from c in stationList
                                            select c.secteur).Distinct().ToList();

            List<string> SubsectorList = new List<string>();
            foreach (string s in SubsectorListQC)
            {
                if (s.Length > 3)
                {
                    if (!SubsectorList.Contains(s))
                    {
                        SubsectorList.Add(s);
                    }
                }
                else
                {
                    SubsectorList.Add(s);
                }
            }

            foreach (string subsector in SubsectorList)
            {
                lblStatus.Text = subsector;
                lblStatus.Refresh();
                Application.DoEvents();

                TVItemModel tvItemModelSubsector = tvItemModelSubsectorList.Where(c => c.TVText.StartsWith(subsector + " ")).FirstOrDefault();
                if (tvItemModelSubsector == null)
                {
                    richTextBoxStatus.AppendText($"{subsector} could not be found\r\n");
                    return $"{subsector} could not be found";
                }

                List<StationQC> QCSites = (from c in stationList
                                           where c.secteur == subsector
                                           orderby c.station
                                           select c).ToList();

                List<MWQMSiteModel> mwqmSiteModelList = mwqmSiteService.GetMWQMSiteModelListWithSubsectorTVItemIDDB(tvItemModelSubsector.TVItemID);

                int ordinal = 0;
                foreach (var qcsite in QCSites)
                {
                    lblStatus.Text = subsector + " -- " + ((int)qcsite.station).ToString();
                    lblStatus.Refresh();
                    Application.DoEvents();

                    string MWQMSiteTVText = "0000".Substring(0, 4 - qcsite.station.ToString().Length) + qcsite.station.ToString();

                    TVItemModel tvItemModelSite = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelSubsector.TVItemID, MWQMSiteTVText, TVTypeEnum.MWQMSite);
                    if (!string.IsNullOrWhiteSpace(tvItemModelSite.Error))
                    {

                        tvItemModelSite = tvItemService.PostAddChildTVItemDB(tvItemModelSubsector.TVItemID, MWQMSiteTVText, TVTypeEnum.MWQMSite);
                        if (!string.IsNullOrWhiteSpace(tvItemModelSite.Error))
                        {
                            richTextBoxStatus.AppendText($"{tvItemModelSite.Error}\r\n");
                            return $"{tvItemModelSite.Error}";
                        }

                        List<Coord> coordList = new List<Coord>()
                        {
                            new Coord() { Lat = (float)(qcsite.y), Lng = (float)(qcsite.x), Ordinal = 0 },
                        };

                        MapInfoModel mapInfoModel = tvItemService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.MWQMSite, tvItemModelSite.TVItemID);
                        if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        {
                            richTextBoxStatus.AppendText($"{mapInfoModel.Error}\r\n");
                            return $"{mapInfoModel.Error}";
                        }
                    }

                    if (string.IsNullOrWhiteSpace(tvItemModelSite.Error))
                    {
                        MWQMSiteModel mwqmSiteModelExist = (from c in mwqmSiteModelList
                                                            where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
                                                            select c).FirstOrDefault();

                        if (mwqmSiteModelExist == null)
                        {
                            MWQMSiteModel mwqmSiteModelNew = new MWQMSiteModel()
                            {
                                MWQMSiteTVItemID = tvItemModelSite.TVItemID,
                                MWQMSiteDescription = "--",
                                MWQMSiteLatestClassification = 0,
                                MWQMSiteNumber = tvItemModelSite.TVText,
                                MWQMSiteTVText = tvItemModelSite.TVText,
                                Ordinal = ordinal,
                            };

                            MWQMSiteModel mwqmSiteModelRet = mwqmSiteService.PostAddMWQMSiteDB(mwqmSiteModelNew);
                            if (!string.IsNullOrWhiteSpace(mwqmSiteModelRet.Error))
                            {
                                richTextBoxStatus.AppendText($"{mwqmSiteModelRet.Error}\r\n");
                                return $"{mwqmSiteModelRet.Error}";
                            }
                            ordinal += 1;
                        }
                        else
                        {
                            bool ShouldUpdate = false;
                            if (mwqmSiteModelExist.MWQMSiteDescription != "--")
                            {
                                mwqmSiteModelExist.MWQMSiteDescription = "--";
                                ShouldUpdate = true;
                            }
                            if (mwqmSiteModelExist.MWQMSiteLatestClassification != 0)
                            {
                                mwqmSiteModelExist.MWQMSiteLatestClassification = 0;
                                ShouldUpdate = true;
                            }
                            if (mwqmSiteModelExist.MWQMSiteNumber != tvItemModelSite.TVText)
                            {
                                mwqmSiteModelExist.MWQMSiteNumber = tvItemModelSite.TVText;
                                ShouldUpdate = true;
                            }
                            if (mwqmSiteModelExist.Ordinal != ordinal)
                            {
                                mwqmSiteModelExist.Ordinal = ordinal;
                                ShouldUpdate = true;
                            }

                            if (ShouldUpdate)
                            {
                                MWQMSiteModel mwqmSiteModelRet = mwqmSiteService.PostUpdateMWQMSiteDB(mwqmSiteModelExist);
                                if (!string.IsNullOrWhiteSpace(mwqmSiteModelRet.Error))
                                {
                                    richTextBoxStatus.AppendText($"{mwqmSiteModelRet.Error}\r\n");
                                    return $"{mwqmSiteModelRet.Error}";
                                }
                            }
                            ordinal += 1;
                        }
                    }
                }

            }

            lblStatus.Text = "done...";

            return "";
        }

        private string RemoveAllMWQMSitesThatNoLongerExistInQCDB(List<StationQC> stationList, TVItemService tvItemService, TVItemModel tvItemModelQC, MWQMSiteService mwqmSiteService)
        {
            /// -----------------------------------------------------------------
            /// ------ Remove all MWQMSites from CSSPDB that no longer exist in QC DB --
            /// -----------------------------------------------------------------

            List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Sector);
            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);

            List<string> SubsectorListQC = (from c in stationList
                                            select c.secteur).Distinct().ToList();

            List<string> SubsectorList = new List<string>();
            foreach (string s in SubsectorListQC)
            {
                if (s.Length > 3)
                {
                    if (!SubsectorList.Contains(s))
                    {
                        SubsectorList.Add(s);
                    }
                }
                else
                {
                    SubsectorList.Add(s);
                }
            }

            foreach (string subsector in SubsectorList)
            {
                lblStatus.Text = subsector;
                lblStatus.Refresh();
                Application.DoEvents();

                TVItemModel tvItemModelSubsector = tvItemModelSubsectorList.Where(c => c.TVText.StartsWith(subsector + " ")).FirstOrDefault();
                if (tvItemModelSubsector == null)
                {
                    richTextBoxStatus.AppendText($"{subsector} could not be found\r\n");
                    return $"{subsector} could not be found";
                }

                List<StationQC> QCSites = (from c in stationList
                                           where c.secteur == subsector
                                           orderby c.station
                                           select c).ToList();

                List<string> siteList = new List<string>();

                foreach (var qcsite in QCSites)
                {
                    string MWQMSiteTVText = "0000".Substring(0, 4 - qcsite.station.ToString().Length) + qcsite.station.ToString();

                    siteList.Add(MWQMSiteTVText);
                }

                List<MWQMSiteModel> mwqmSiteModelList = mwqmSiteService.GetMWQMSiteModelListWithSubsectorTVItemIDDB(tvItemModelSubsector.TVItemID);
                List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSubsector.TVItemID, TVTypeEnum.MWQMSite);

                foreach (TVItemModel tvItemModel in tvItemModelMWQMSiteList)
                {
                    lblStatus.Text = subsector + " -- " + tvItemModel.TVText;
                    lblStatus.Refresh();
                    Application.DoEvents();

                    string siteExist = (from c in siteList
                                        where c == tvItemModel.TVText
                                        select c).FirstOrDefault();

                    if (siteExist == null)
                    {
                        richTextBoxStatus.AppendText($"{subsector} site {tvItemModel.TVText} does not exist in QC DB\r\n");

                        using (CSSPDBEntities db2 = new CSSPDBEntities())
                        {
                            // Deleting TVItemStats of MWQMSite
                            List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
                                                               where c.TVItemID == tvItemModel.TVItemID
                                                               select c).ToList();

                            if (tvItemStatList.Count > 0)
                            {
                                db2.TVItemStats.RemoveRange(tvItemStatList);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMSites");
                                    return $"Could not delete TVItemStats of MWQMSites";
                                }
                            }

                            // Might have to delete runs that would not be attached to any MWQMSample


                            // Deleting MWQMSamples associated to the MWQMSite
                            lblStatus.Text = subsector + " -- " + tvItemModel.TVText + " samples delete";
                            lblStatus.Refresh();
                            Application.DoEvents();

                            List<MWQMSample> mwqmSampleList = (from c in db2.MWQMSamples
                                                               where c.MWQMSiteTVItemID == tvItemModel.TVItemID
                                                               select c).ToList();

                            if (mwqmSampleList.Count > 0)
                            {
                                db2.MWQMSamples.RemoveRange(mwqmSampleList);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete Sample");
                                    return $"Could not delete Sample";
                                }
                            }


                            // Deleting MapInfos for MWQMSite
                            List<MapInfo> MapInfoList = (from c in db2.MapInfos
                                                         where c.TVItemID == tvItemModel.TVItemID
                                                         select c).ToList();


                            if (MapInfoList.Count > 0)
                            {
                                db2.MapInfos.RemoveRange(MapInfoList);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete MapInfos for MWQMSites");
                                    return $"Could not delete MapInfos for MWQMSites";
                                }
                            }

                            // Deleting MWQMSite
                            List<MWQMSite> mwqmSiteList = (from c in db2.MWQMSites
                                                           where c.MWQMSiteTVItemID == tvItemModel.TVItemID
                                                           select c).ToList();

                            if (mwqmSiteList.Count > 0)
                            {
                                db2.MWQMSites.RemoveRange(mwqmSiteList);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
                                    return $"Could not delete TVItems for MWQMSites";
                                }
                            }

                            // Deleting TVItems for MWQMSite
                            List<TVItem> TVItemList = (from c in db2.TVItems
                                                       where c.TVItemID == tvItemModel.TVItemID
                                                       select c).ToList();

                            if (TVItemList.Count > 0)
                            {
                                db2.TVItems.RemoveRange(TVItemList);

                                try
                                {
                                    db2.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
                                    return $"Could not delete TVItems for MWQMSites";
                                }
                            }
                        }
                    }
                }
            }

            lblStatus.Text = "done...";

            return "";
        }
        private string AddAllNewRunsFoundInQCDB(List<StationQC> stationList, List<SampleQC> sampleList, List<PCCSM.db_tournee> tourneeList, TVItemService tvItemService, TVItemModel tvItemModelQC, MWQMSiteService mwqmSiteService, MWQMRunService mwqmRunService)
        {
            /// -----------------------------------------------------------------
            /// ------Add all new Runs found in QC DB and not in CSSPDB--
            /// -----------------------------------------------------------------

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelQC.TVText + "\r\n");
                return "Error: could not find TVItem Subsector for " + tvItemModelQC.TVText;
            }


            List<string> subsectorList = (from s in stationList
                                          orderby s.secteur
                                          select s.secteur).Distinct().ToList();

            List<TVItemModel> tvItemMWQMSiteAll = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.MWQMSite);

            List<TVItemModel> tvItemMWQMRunAll = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.MWQMRun);

            int Count = 0;
            int TotalCount = subsectorList.Count();
            foreach (string subsector in subsectorList)
            {
                if (Cancel)
                {
                    richTextBoxStatus.AppendText($"Pressed Cancel\r\n");
                    return $"Pressed Cancel";
                }

                Count += 1;
                lblStatus.Text = (Count * 100 / TotalCount).ToString() + " ... CreateRunsQC for " + subsector;
                lblStatus2.Text = Count + " of " + TotalCount;
                Application.DoEvents();

                TVItemModel tvItemModelSubsector = (from c in tvItemModelSubsectorList
                                                    where c.TVText.StartsWith(subsector + " ")
                                                    select c).FirstOrDefault();

                if (tvItemModelSubsector == null)
                {
                    richTextBoxStatus.AppendText($"could not find tvItemmodelSubsector [{subsector}]\r\n");
                    return $"could not find tvItemmodelSubsector [{subsector}]";
                }

                List<MWQMRunModel> mwqmRunModelAll = mwqmRunService.GetMWQMRunModelListWithSubsectorTVItemIDDB(tvItemModelSubsector.TVItemID);

                List<StationQC> staQCList = (from c in stationList
                                             where c.secteur == subsector
                                             select c).ToList();

                int countSta = 0;
                int totalCountsta = staQCList.Count;
                foreach (StationQC geoStat in staQCList)
                {
                    if (Cancel)
                    {
                        richTextBoxStatus.AppendText($"Pressed Cancel\r\n");
                        return $"Pressed Cancel";
                    }


                    countSta += 1;
                    lblStatus2.Text = countSta + " of " + totalCountsta + " ... CreateRunsQC for " + subsector;
                    Application.DoEvents();

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
                        richTextBoxStatus.AppendText($"could not find MWQMSite [{MWQMSiteTVText} under subsector {subsector}]\r\n");
                        return $"could not find MWQMSite [{MWQMSiteTVText} under subsector {subsector}]";
                    }

                    List<SampleQC> dbMesureList = (from m in sampleList
                                                   where m.id_geo_station_p == geoStat.id_geo_station_p
                                                   select m).ToList();


                    foreach (var dbm in dbMesureList)
                    {

                        Application.DoEvents();

                        // getting Runs
                        PCCSM.db_tournee dbt = (from t in tourneeList
                                                where t.ID_Tournee == dbm.id_tournee
                                                select t).FirstOrDefault();

                        DateTime? SampDateTime = null;
                        DateTime? SampStartDateTime = null;
                        DateTime? SampEndDateTime = null;
                        if (dbm.hre_echantillonnage != null)
                        {
                            SampDateTime = ((DateTime)dbm.hre_echantillonnage.Value).AddHours(1);
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

                        SampleTypeEnum runSampleType = SampleTypeEnum.Routine;

                        switch (geoStat.type_station)
                        {
                            case "Eu":
                                {
                                    runSampleType = SampleTypeEnum.Infrastructure;
                                }
                                break;
                            case "EE":
                                {
                                    runSampleType = SampleTypeEnum.Study;
                                }
                                break;
                            default:
                                // everything else is already set to SampleTypeEnum.Routine
                                break;
                        }

                        MWQMRunModel mwqmRunModelNew = new MWQMRunModel()
                        {
                            SubsectorTVItemID = tvItemModelSubsector.TVItemID,
                            DateTime_Local = DateRun,
                            RunSampleType = runSampleType,
                            RunNumber = 1,
                        };

                        MWQMRunModel wqmRunModelExist = (from c in mwqmRunModelAll
                                                         where c.DateTime_Local == DateRun
                                                         && c.RunSampleType == runSampleType
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
                                if (runSampleType == SampleTypeEnum.Study || runSampleType == SampleTypeEnum.Infrastructure)
                                {
                                    TVTextRun = TVTextRun + " (" + runSampleType.ToString() + ")";
                                }

                                richTextBoxStatus.AppendText($"{tvItemModelSubsector.TVText} --- { TVTextRun } adding TVText\r\n");
                                tvItemModelRunRet = tvItemService.PostAddChildTVItemDB(tvItemModelSubsector.TVItemID, TVTextRun, TVTypeEnum.MWQMRun);
                                if (!string.IsNullOrWhiteSpace(tvItemModelRunRet.Error))
                                {
                                    richTextBoxStatus.AppendText($"could not add TVItem for Runs [{TVTextRun} under subsector {subsector}]\r\n");
                                    return $"could not add TVItem for Runs [{TVTextRun} under subsector {subsector}]";
                                }
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
                                if (!string.IsNullOrWhiteSpace(mwqmRunModelRet.Error))
                                {
                                    richTextBoxStatus.AppendText($"could not add Runs [{TVTextRun} under subsector {subsector}]\r\n");
                                    return $"could not add Runs [{TVTextRun} under subsector {subsector}]";
                                }

                                mwqmRunModelAll.Add(mwqmRunModelRet);
                            }
                        }
                    }
                }
            }

            lblStatus.Text = "done...";
            lblStatus.Refresh();
            Application.DoEvents();

            return "";
        }

        private string AddAllNewSampleFoundInQCDB(List<StationQC> stationList, List<SampleQC> sampleList, List<PCCSM.db_tournee> tourneeList, TVItemService tvItemService, TVItemModel tvItemModelQC, MWQMSiteService mwqmSiteService, MWQMRunService mwqmRunService)
        {
            /// -----------------------------------------------------------------
            /// Add all new MWQMSample found in QC DB and not in CSSPDB 
            /// while removing dupliates of orther sample not belonging there
            /// -----------------------------------------------------------------

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelQC.TVText + "\r\n");
                return "Error: could not find TVItem Subsector for " + tvItemModelQC.TVText;
            }


            List<string> subsectorList = (from s in stationList
                                          orderby s.secteur
                                          select s.secteur).Distinct().ToList();

            List<TVItemModel> tvItemMWQMSiteAll = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.MWQMSite);

            List<TVItemModel> tvItemMWQMRunAll = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.MWQMRun);

            foreach (string subsector in subsectorList)
            {
                lblStatus.Text = subsector;
                lblStatus.Refresh();
                Application.DoEvents();

                if (Cancel)
                {
                    richTextBoxStatus.AppendText($"Pressed Cancel\r\n");
                    return $"Pressed Cancel";
                }

                TVItemModel tvItemModelSubsector = (from c in tvItemModelSubsectorList
                                                    where c.TVText.StartsWith(subsector + " ")
                                                    select c).FirstOrDefault();

                if (tvItemModelSubsector == null)
                {
                    richTextBoxStatus.AppendText($"could not find tvItemmodelSubsector [{subsector}]\r\n");
                    return $"could not find tvItemmodelSubsector [{subsector}]";
                }

                List<MWQMRunModel> mwqmRunModelAll = mwqmRunService.GetMWQMRunModelListWithSubsectorTVItemIDDB(tvItemModelSubsector.TVItemID);

                List<StationQC> staQCList = (from c in stationList
                                             where c.secteur == subsector
                                             select c).ToList();

                foreach (var geoStat in staQCList)
                {
                    if (Cancel)
                    {
                        richTextBoxStatus.AppendText($"Pressed Cancel\r\n");
                        return $"Pressed Cancel";
                    }

                    lblStatus.Text = subsector + " -- " + geoStat.station;
                    lblStatus.Refresh();
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
                        richTextBoxStatus.AppendText($"could not find MWQMSite [{MWQMSiteTVText} under subsector {subsector}]\r\n");
                        return $"could not find MWQMSite [{MWQMSiteTVText} under subsector {subsector}]";
                    }

                    List<SampleQC> dbMesureList = (from m in sampleList
                                                   where m.id_geo_station_p == geoStat.id_geo_station_p
                                                   && m.cf != null
                                                   select m).ToList();

                    List<MWQMSample> mwqmSampleCSSPList = new List<MWQMSample>();
                    using (CSSPDBEntities db2 = new CSSPDBEntities())
                    {
                        mwqmSampleCSSPList = (from c in db2.MWQMSamples
                                              where c.MWQMSiteTVItemID == tvItemMWQMSiteExist.TVItemID
                                              select c).ToList();
                    }

                    List<MWQMSample> mwqmSampleListToAdd = new List<MWQMSample>();
                    List<MWQMSample> mwqmSampleListToUpdate = new List<MWQMSample>();

                    List<MWQMSample> mwqmSampleListExistInQCDB = new List<MWQMSample>();
                    foreach (var dbm in dbMesureList)
                    {
                        Application.DoEvents();

                        // getting Runs
                        PCCSM.db_tournee dbt = (from t in tourneeList
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
                                                          && (c.RunSampleType == SampleTypeEnum.Routine
                                                          || c.RunSampleType == SampleTypeEnum.Infrastructure)
                                                          && c.RunNumber == 1
                                                          select c).FirstOrDefault();

                        if (mwqmRunModelExist == null)
                        {
                            richTextBoxStatus.AppendText($"could not find Run [{DateRun} under subsector {subsector}]\r\n");
                            return $"could not find Run [{DateRun} under subsector {subsector}]";
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
                        if (dbm.ph != null)
                        {
                            PH = (float)dbm.ph;
                        }

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

                        //if (mwqmSampleModelNew.PH >= 14.0f)
                        //{
                        //    mwqmSampleModelNew.PH = null;
                        //}
                        //if (mwqmSampleModelNew.Salinity_PPT > 40.0f)
                        //{
                        //    mwqmSampleModelNew.Salinity_PPT = null;
                        //}
                        //if (mwqmSampleModelNew.WaterTemp_C > 40.0f)
                        //{
                        //    mwqmSampleModelNew.WaterTemp_C = null;
                        //}

                        // new code to delet later
                        mwqmRunModelExist = (from c in mwqmRunModelAll
                                             where c.SubsectorTVItemID == tvItemModelSubsector.TVItemID
                                             && c.DateTime_Local == DateRun
                                             && (c.RunSampleType == SampleTypeEnum.Routine
                                             || c.RunSampleType == SampleTypeEnum.Infrastructure)
                                             && c.RunNumber == 1
                                             select c).FirstOrDefault();



                        if (mwqmRunModelExist == null)
                        {
                            richTextBoxStatus.AppendText($"Could not find MWQMRunModel ss {tvItemModelSubsector.TVText} --- {DateRun.ToString("yyyy MM dd")}");
                            return $"Could not find MWQMRunModel ss {tvItemModelSubsector.TVText} --- {DateRun.ToString("yyyy MM dd")}";
                        }

                        string SampleTypeText = mwqmRunModelExist.RunSampleType == SampleTypeEnum.Routine ? "109," : "102,";

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
                            SampleTypesText = SampleTypeText,
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
                                                      && c.SampleTypesText == SampleTypeText
                                                      select c).FirstOrDefault();

                        if (mwqmSampleExist == null)
                        {
                            mwqmSampleListToAdd.Add(mwqmSampleNew);
                        }
                        else
                        {
                            bool changed = false;
                            if (mwqmSampleExist.UseForOpenData != UseForOpenData)
                            {
                                mwqmSampleExist.UseForOpenData = UseForOpenData;
                                changed = true;
                            }
                            if (mwqmSampleExist.FecCol_MPN_100ml != dbm.cf)
                            {
                                mwqmSampleExist.FecCol_MPN_100ml = (int)dbm.cf;
                                changed = true;
                            }
                            if (mwqmSampleExist.Salinity_PPT != Sal)
                            {
                                mwqmSampleExist.Salinity_PPT = Sal;
                                changed = true;
                            }
                            if (mwqmSampleExist.WaterTemp_C != Temp)
                            {
                                mwqmSampleExist.WaterTemp_C = Temp;
                                changed = true;
                            }
                            if (mwqmSampleExist.Depth_m != prof)
                            {
                                mwqmSampleExist.Depth_m = prof;
                                changed = true;
                            }

                            if (changed)
                            {
                                mwqmSampleListToUpdate.Add(mwqmSampleExist);
                            }

                            mwqmSampleListExistInQCDB.Add(mwqmSampleExist);
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

                                foreach (MWQMSample mwqmSample in mwqmSampleListToAdd)
                                {
                                    mwqmSampleListExistInQCDB.Add(mwqmSample);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            richTextBoxStatus.AppendText($"Could not add MWQMSampleList ss {tvItemModelSubsector.TVText} --- {tvItemMWQMSiteExist.TVText}");
                            return $"Could not add MWQMSampleList ss {tvItemModelSubsector.TVText} --- {tvItemMWQMSiteExist.TVText}";
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
                                    mwqmSample.FecCol_MPN_100ml = mwqmSampleChanged.FecCol_MPN_100ml;
                                    mwqmSample.Salinity_PPT = mwqmSampleChanged.Salinity_PPT;
                                    mwqmSample.WaterTemp_C = mwqmSampleChanged.WaterTemp_C;
                                }
                            }

                            try
                            {
                                db2.SaveChanges();
                            }
                            catch (Exception)
                            {
                                richTextBoxStatus.AppendText($"Could not change MWQMSampleList ss {tvItemModelSubsector.TVText} --- {tvItemMWQMSiteExist.TVText}");
                                return $"Could not change MWQMSampleList ss {tvItemModelSubsector.TVText} --- {tvItemMWQMSiteExist.TVText}";
                            }
                        }

                        //mwqmSampleCSSPList = (from c in db2.MWQMSamples
                        //                      where c.MWQMSiteTVItemID == tvItemMWQMSiteExist.TVItemID
                        //                      select c).ToList();


                        //List<MWQMSample> mwqmSampleCSSPListToDelete = new List<MWQMSample>();

                        //foreach (MWQMSample mwqmSample in mwqmSampleCSSPList)
                        //{
                        //    if (!mwqmSampleListExistInQCDB.Where(c => c.MWQMSampleID == mwqmSample.MWQMSampleID).Any())
                        //    {
                        //        mwqmSampleCSSPListToDelete.Add(mwqmSample);
                        //    }
                        //}

                        //try
                        //{
                        //    db2.MWQMSamples.RemoveRange(mwqmSampleCSSPListToDelete);
                        //    db2.SaveChanges();
                        //}
                        //catch (Exception ex)
                        //{
                        //    richTextBoxStatus.AppendText($"Could not delete the mwqmSampleCSSPListToDelete for ss {tvItemModelSubsector.TVText} and site {tvItemMWQMSiteExist.TVText}");
                        //    return $"Could not delete the mwqmSampleCSSPListToDelete for ss {tvItemModelSubsector.TVText} and site {tvItemMWQMSiteExist.TVText}";
                        //}
                    }

                }
            }

            lblStatus.Text = "done...";
            lblStatus.Refresh();
            Application.DoEvents();

            return "";
        }

        private string RemoveSamplesFoundFoundInQCDB(List<StationQC> stationList, List<SampleQC> sampleList, List<PCCSM.db_tournee> tourneeList, TVItemService tvItemService, TVItemModel tvItemModelQC, MWQMSiteService mwqmSiteService, MWQMRunService mwqmRunService)
        {
            /// -----------------------------------------------------------------
            /// Add all new MWQMSample found in QC DB and not in CSSPDB 
            /// while removing dupliates of orther sample not belonging there
            /// -----------------------------------------------------------------

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelQC.TVText + "\r\n");
                return "Error: could not find TVItem Subsector for " + tvItemModelQC.TVText;
            }


            List<string> subsectorList = (from s in stationList
                                          orderby s.secteur
                                          select s.secteur).Distinct().ToList();

            List<TVItemModel> tvItemMWQMSiteAll = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.MWQMSite);

            List<TVItemModel> tvItemMWQMRunAll = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.MWQMRun);

            foreach (TVItemModel tvItemModelSS in tvItemModelSubsectorList)
            {
                string subsector = tvItemModelSS.TVText;
                if (subsector.Contains(" "))
                {
                    subsector = subsector.Substring(0, subsector.IndexOf(" "));
                }

                lblStatus.Text = subsector;
                lblStatus.Refresh();
                Application.DoEvents();

                if (Cancel)
                {
                    richTextBoxStatus.AppendText($"Pressed Cancel\r\n");
                    return $"Pressed Cancel";
                }

                List<MWQMRunModel> mwqmRunModelAll = mwqmRunService.GetMWQMRunModelListWithSubsectorTVItemIDDB(tvItemModelSS.TVItemID);

                List<int> staNameQCList = (from c in stationList
                                           where c.secteur == subsector
                                           select c.station).Distinct().ToList();

                foreach (int site in staNameQCList)
                {
                    if (Cancel)
                    {
                        richTextBoxStatus.AppendText($"Pressed Cancel\r\n");
                        return $"Pressed Cancel";
                    }

                    lblStatus.Text = subsector + " -- " + site.ToString();
                    lblStatus.Refresh();
                    Application.DoEvents();

                    string MWQMSiteTVText = "0000".Substring(0, 4 - site.ToString().Length) + site.ToString();

                    TVItemModel tvItemMWQMSiteExist = (from c in tvItemMWQMSiteAll
                                                       where c.ParentID == tvItemModelSS.TVItemID
                                                       && c.TVText == MWQMSiteTVText
                                                       && c.TVType == TVTypeEnum.MWQMSite
                                                       select c).FirstOrDefault();

                    if (tvItemMWQMSiteExist == null)
                    {
                        richTextBoxStatus.AppendText($"could not find MWQMSite [{MWQMSiteTVText} under subsector {subsector}]\r\n");
                        return $"could not find MWQMSite [{MWQMSiteTVText} under subsector {subsector}]";
                    }

                    List<StationQC> dbStationList = (from c in stationList
                                                     where c.secteur == subsector
                                                     && c.station == site
                                                     select c).ToList();


                    List<SampleQC> dbMesureList = new List<SampleQC>();
                    foreach (StationQC stationQC in dbStationList)
                    {
                        List<SampleQC> dbMesureList2 = (from c in sampleList
                                                        where c.id_geo_station_p == stationQC.id_geo_station_p
                                                        && c.cf != null
                                                        select c).ToList();

                        dbMesureList.AddRange(dbMesureList2);

                    }

                    List<MWQMSample> mwqmSampleCSSPList = new List<MWQMSample>();
                    using (CSSPDBEntities db2 = new CSSPDBEntities())
                    {
                        mwqmSampleCSSPList = (from c in db2.MWQMSamples
                                              where c.MWQMSiteTVItemID == tvItemMWQMSiteExist.TVItemID
                                              select c).ToList();
                    }

                    List<MWQMSample> mwqmSampleQCList = new List<MWQMSample>();
                    foreach (SampleQC sampleQC in dbMesureList)
                    {
                        Application.DoEvents();

                        // getting Runs
                        PCCSM.db_tournee dbt = (from t in tourneeList
                                                where t.ID_Tournee == sampleQC.id_tournee
                                                select t).FirstOrDefault();

                        DateTime? SampDateTime = null;
                        DateTime? SampStartDateTime = null;
                        DateTime? SampEndDateTime = null;
                        if (sampleQC.hre_echantillonnage != null)
                        {
                            SampDateTime = (DateTime)sampleQC.hre_echantillonnage.Value.AddHours(1);
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

                        MWQMRunModel mwqmRunModelExist = (from c in mwqmRunModelAll
                                                          where c.SubsectorTVItemID == tvItemModelSS.TVItemID
                                                          && c.DateTime_Local == DateRun
                                                          && (c.RunSampleType == SampleTypeEnum.Routine
                                                          || c.RunSampleType == SampleTypeEnum.Infrastructure)
                                                          && c.RunNumber == 1
                                                          select c).FirstOrDefault();

                        if (mwqmRunModelExist == null)
                        {
                            richTextBoxStatus.AppendText($"could not find Run [{DateRun} under subsector {subsector}]\r\n");
                            return $"could not find Run [{DateRun} under subsector {subsector}]";
                        }


                        // doing MWQMSample

                        double? prof = null;
                        if (sampleQC.prof != null)
                        {
                            prof = (float)sampleQC.prof;
                        }
                        double? Sal = null;
                        if (sampleQC.sal != null)
                        {
                            Sal = (float)sampleQC.sal;
                        }
                        double? Temp = null;
                        if (sampleQC.temp != null)
                        {
                            Temp = (float)sampleQC.temp;
                        }
                        double? PH = null;
                        if (sampleQC.ph != null)
                        {
                            PH = (float)sampleQC.ph;
                        }

                        bool UseForOpenData = true;
                        if (sampleQC.diffusable == null)
                        {
                            UseForOpenData = false;
                        }
                        else
                        {
                            if (!(bool)sampleQC.diffusable)
                            {
                                UseForOpenData = false;
                            }
                        }

                        string sampleNote = "--";
                        if (!string.IsNullOrWhiteSpace(sampleQC.commentaire))
                        {
                            sampleNote = sampleQC.commentaire.Trim();
                        }
                        MWQMSampleModel mwqmSampleModelNew = new MWQMSampleModel()
                        {
                            MWQMSiteTVItemID = tvItemMWQMSiteExist.TVItemID,
                            SampleDateTime_Local = (DateTime)SampDateTime,
                            Depth_m = prof,
                            FecCol_MPN_100ml = (int)(sampleQC.cf == null ? 0 : sampleQC.cf),
                            Salinity_PPT = Sal,
                            WaterTemp_C = Temp,
                            PH = PH,
                            MWQMSampleNote = sampleNote,
                            SampleTypesText = ((int)SampleTypeEnum.Routine).ToString() + ",",
                            SampleTypeList = new List<SampleTypeEnum>() { SampleTypeEnum.Routine },
                            UseForOpenData = UseForOpenData,
                        };

                        if (mwqmSampleModelNew.PH >= 14.0f)
                        {
                            mwqmSampleModelNew.PH = null;
                        }
                        if (mwqmSampleModelNew.Salinity_PPT > 40.0f)
                        {
                            mwqmSampleModelNew.Salinity_PPT = null;
                        }
                        if (mwqmSampleModelNew.WaterTemp_C > 40.0f)
                        {
                            mwqmSampleModelNew.WaterTemp_C = null;
                        }

                        // new code to delet later
                        mwqmRunModelExist = (from c in mwqmRunModelAll
                                             where c.SubsectorTVItemID == tvItemModelSS.TVItemID
                                             && c.DateTime_Local == DateRun
                                             && (c.RunSampleType == SampleTypeEnum.Routine
                                             || c.RunSampleType == SampleTypeEnum.Infrastructure)
                                             && c.RunNumber == 1
                                             select c).FirstOrDefault();



                        if (mwqmRunModelExist == null)
                        {
                            richTextBoxStatus.AppendText($"Could not find MWQMRunModel ss {tvItemModelSS.TVText} --- {DateRun.ToString("yyyy MM dd")}");
                            return $"Could not find MWQMRunModel ss {tvItemModelSS.TVText} --- {DateRun.ToString("yyyy MM dd")}";
                        }

                        string SampleTypeText = mwqmRunModelExist.RunSampleType == SampleTypeEnum.Routine ? "109," : "102,";

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
                            SampleTypesText = SampleTypeText,
                            SampleType_old = 4,
                            Tube_10 = null,
                            Tube_1_0 = null,
                            Tube_0_1 = null,
                            ProcessedBy = null,
                            UseForOpenData = UseForOpenData,
                            LastUpdateDate_UTC = DateTime.UtcNow,
                            LastUpdateContactTVItemID = 2,
                        };


                        MWQMSample mwqmSampleExist = (from c in mwqmSampleCSSPList
                                                      where c.MWQMSiteTVItemID == tvItemMWQMSiteExist.TVItemID
                                                      && c.MWQMRunTVItemID == mwqmRunModelExist.MWQMRunTVItemID
                                                      && c.SampleDateTime_Local == (DateTime)SampDateTime
                                                      && c.Depth_m == mwqmSampleModelNew.Depth_m
                                                      && c.FecCol_MPN_100ml == mwqmSampleModelNew.FecCol_MPN_100ml
                                                      && c.Salinity_PPT == mwqmSampleModelNew.Salinity_PPT
                                                      && c.WaterTemp_C == mwqmSampleModelNew.WaterTemp_C
                                                      && c.SampleTypesText == SampleTypeText
                                                      select c).FirstOrDefault();

                        if (mwqmSampleExist == null)
                        {
                            richTextBoxStatus.AppendText($"{ subsector } site { site } MWQMSiteTVItemID { tvItemMWQMSiteExist.TVItemID } MWQMRunTVItemID { mwqmRunModelExist.MWQMRunTVItemID }  not found\r\n");
                        }
                        else
                        {
                            if (Math.Abs(mwqmSampleModelNew.FecCol_MPN_100ml - mwqmSampleExist.FecCol_MPN_100ml) > 1)
                            {
                                richTextBoxStatus.AppendText($"{ subsector } site { site } FC CSSP { mwqmSampleExist.FecCol_MPN_100ml } FC QC { mwqmSampleModelNew.FecCol_MPN_100ml }  not equal\r\n");
                            }
                            if (mwqmSampleModelNew.PH != null && mwqmSampleExist.PH != null)
                            {
                                if (Math.Abs((double)mwqmSampleModelNew.PH - (double)mwqmSampleExist.PH) > 0.1D)
                                {
                                    richTextBoxStatus.AppendText($"{ subsector } site { site } FC CSSP { mwqmSampleExist.PH } FC QC { mwqmSampleModelNew.PH }  not equal\r\n");
                                }
                            }
                            if (mwqmSampleModelNew.Salinity_PPT != null && mwqmSampleExist.Salinity_PPT != null)
                            {
                                if (Math.Abs((double)mwqmSampleModelNew.Salinity_PPT - (double)mwqmSampleExist.Salinity_PPT) > 0.1D)
                                {
                                    richTextBoxStatus.AppendText($"{ subsector } site { site } FC CSSP { mwqmSampleExist.Salinity_PPT } FC QC { mwqmSampleModelNew.Salinity_PPT }  not equal\r\n");
                                }
                            }
                            if (mwqmSampleModelNew.WaterTemp_C != null && mwqmSampleExist.WaterTemp_C != null)
                            {
                                if (Math.Abs((double)mwqmSampleModelNew.WaterTemp_C - (double)mwqmSampleExist.WaterTemp_C) > 0.1D)
                                {
                                    richTextBoxStatus.AppendText($"{ subsector } site { site } FC CSSP { mwqmSampleExist.WaterTemp_C } FC QC { mwqmSampleModelNew.WaterTemp_C }  not equal\r\n");
                                }
                            }
                            if (mwqmSampleModelNew.Depth_m != null && mwqmSampleExist.Depth_m != null)
                            {
                                if (Math.Abs((double)mwqmSampleModelNew.Depth_m - (double)mwqmSampleExist.Depth_m) > 0.1D)
                                {
                                    richTextBoxStatus.AppendText($"{ subsector } site { site } FC CSSP { mwqmSampleExist.Depth_m } FC QC { mwqmSampleModelNew.Depth_m }  not equal\r\n");
                                }
                            }
                        }
                    }
                }
            }

            lblStatus.Text = "done...";
            lblStatus.Refresh();
            Application.DoEvents();

            return "";
        }

        private string AddAllNewMWQMSubsectorTextAndTideInfoFoundInQCDB(TVItemService tvItemService, TVItemModel tvItemModelQC, MapInfoService mapInfoService, MWQMSubsectorService mwqmSubsectorService)
        {
            /// -----------------------------------------------------------------
            /// ------ Add all new MWQMSubsectors with proper tide info --
            /// -----------------------------------------------------------------

            List<TideLocation> tideLocationList = new List<TideLocation>();
            using (CSSPDBEntities db2 = new CSSPDBEntities())
            {
                tideLocationList = (from c in db2.TideLocations
                                    select c).ToList();
            }

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);

            foreach (TVItemModel tvItemModelSS in tvItemModelSubsectorList)
            {
                string subsector = tvItemModelSS.TVText;
                if (subsector.Contains(" "))
                {
                    subsector = subsector.Substring(0, subsector.IndexOf(" "));
                }

                lblStatus.Text = subsector;
                lblStatus.Refresh();
                Application.DoEvents();


                MWQMSubsectorModel mwqmSubsectorModel = mwqmSubsectorService.GetMWQMSubsectorModelWithMWQMSubsectorTVItemIDDB(tvItemModelSS.TVItemID);


                if (!string.IsNullOrWhiteSpace(mwqmSubsectorModel.Error) || string.IsNullOrWhiteSpace(mwqmSubsectorModel.TideLocationSIDText))
                {
                    string TideLocationSIDText = "";
                    List<MapInfoPointModel> mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.Subsector, MapInfoDrawTypeEnum.Point);
                    if (mapInfoPointModelList.Count > 0)
                    {
                        double Lat = mapInfoPointModelList[0].Lat;
                        double Lng = mapInfoPointModelList[0].Lng;

                        var tideLocation3List = (from c in tideLocationList
                                                 let d = ((c.Lat - Lat) * (c.Lat - Lat)) + ((c.Lng - Lng) * (c.Lng - Lng))
                                                 orderby d
                                                 select new { c, d }).Take(3).ToList();

                        if (tideLocation3List.Count != 3)
                        {
                            richTextBoxStatus.AppendText($"tideLocatiion3List count is not equal to 3\r\n");
                            return $"tideLocatiion3List count is not equal to 3";
                        }

                        foreach (var tideLocation3 in tideLocation3List)
                        {
                            TideLocationSIDText = TideLocationSIDText + tideLocation3.c.sid + ",";
                        }

                        TideLocationSIDText = TideLocationSIDText.Substring(0, TideLocationSIDText.Length - 1);

                    }

                    if (string.IsNullOrWhiteSpace(mwqmSubsectorModel.Error) && string.IsNullOrWhiteSpace(mwqmSubsectorModel.TideLocationSIDText))
                    {
                        mwqmSubsectorModel.TideLocationSIDText = TideLocationSIDText;

                        MWQMSubsectorModel mwqmSubsectormodelRet = mwqmSubsectorService.PostUpdateMWQMSubsectorDB(mwqmSubsectorModel);
                        if (!string.IsNullOrWhiteSpace(mwqmSubsectormodelRet.Error))
                        {
                            richTextBoxStatus.AppendText($"{mwqmSubsectormodelRet.Error}\r\n");
                            return $"{mwqmSubsectormodelRet.Error}";
                        }
                    }
                    else
                    {
                        MWQMSubsectorModel mwqmSubsectorModelNew = new MWQMSubsectorModel()
                        {
                            MWQMSubsectorTVItemID = tvItemModelSS.TVItemID,
                            MWQMSubsectorTVText = subsector,
                            SubsectorDesc = "Todo",
                            SubsectorHistoricKey = subsector,
                            TideLocationSIDText = TideLocationSIDText
                        };
                        MWQMSubsectorModel mwqmSubsectormodelRet = mwqmSubsectorService.PostAddMWQMSubsectorDB(mwqmSubsectorModelNew);
                        if (!string.IsNullOrWhiteSpace(mwqmSubsectormodelRet.Error))
                        {
                            richTextBoxStatus.AppendText($"{mwqmSubsectormodelRet.Error}\r\n");
                            return $"{mwqmSubsectormodelRet.Error}";
                        }
                    }
                }

                lblStatus.Text = "done...";

            }

            return "";

        }

        private string DeleteAllMWQMSubsectorThatShouldNotExist(TVItemService tvItemService, TVItemModel tvItemModelRoot, MWQMSubsectorService mwqmSubsectorService)
        {
            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Subsector);
            List<MWQMSubsectorModel> mwqmSubsectorModelList = mwqmSubsectorService.GetAllMWQMSubsectorModelDB();

            foreach (MWQMSubsectorModel mwqmSubsectorModel in mwqmSubsectorModelList)
            {
                lblStatus.Text = mwqmSubsectorModel.SubsectorHistoricKey;
                lblStatus.Refresh();
                Application.DoEvents();

                if (!tvItemModelSubsectorList.Where(c => c.TVItemID == mwqmSubsectorModel.MWQMSubsectorTVItemID).Any())
                {
                    richTextBoxStatus.AppendText($"{mwqmSubsectorModel.SubsectorHistoricKey} should be removed\r\n");

                    MWQMSubsectorModel mwqmSubsectorModelRet = mwqmSubsectorService.PostDeleteMWQMSubsectorDB(mwqmSubsectorModel.MWQMSubsectorTVItemID);
                    if (!string.IsNullOrWhiteSpace(mwqmSubsectorModelRet.Error))
                    {
                        richTextBoxStatus.AppendText($"ERROR {mwqmSubsectorModelRet.Error}\r\n");
                    }
                }
            }

            lblStatus.Text = "done...";

            return "";
        }

        private string GiveProperNameToSubsectorFoundInQCDB(List<StationQC> stationList, List<SectorQC> subsectorNameList, TVItemService tvItemService, TVItemModel tvItemModelQC, TVItemLanguageService tvItemLanguageService)
        {
            /// -----------------------------------------------------------------
            /// ------ Give proper name for subsector from QC DB --
            /// -----------------------------------------------------------------

            //List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
            //if (tvItemModelSubsectorList.Count == 0)
            //{
            //    richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelQC.TVText + "\r\n");
            //    return "Error: could not find TVItem Subsector for " + tvItemModelQC.TVText;
            //}

            //foreach (string subsector in subsectorNameList.Select(c => c.secteur).OrderBy(c => c).Distinct().ToList())
            //{

            //    lblStatus.Text = subsector;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    if (Cancel)
            //    {
            //        richTextBoxStatus.AppendText($"Pressed Cancel\r\n");
            //        return $"Pressed Cancel";
            //    }

            //    var nomList = subsectorNameList.Where(c => c.secteur == subsector).FirstOrDefault();

            //    if (nomList != null)
            //    {
            //        string nomFR = nomList.secteur_nom;
            //        string nomEN = nomList.secteur_nom_a;

            //        TVItemModel tvItemModelSubsector = (from c in tvItemModelSubsectorList
            //                                            where c.TVText.StartsWith(subsector + " ")
            //                                            select c).FirstOrDefault();

            //        if (tvItemModelSubsector == null)
            //        {
            //            continue;
            //        }

            //        if (!string.IsNullOrWhiteSpace(nomFR))
            //        {
            //            TVItemLanguageModel tvItemLanguageModelFR = tvItemLanguageService.GetTVItemLanguageModelWithTVItemIDAndLanguageDB(tvItemModelSubsector.TVItemID, LanguageEnum.fr);
            //            if (!string.IsNullOrWhiteSpace(tvItemLanguageModelFR.Error))
            //            {
            //                richTextBoxStatus.AppendText($"could not find tvItemLanguageModel (FR) for [{subsector}]\r\n");
            //                return $"could not find tvItemLanguageModel (FR) for [{subsector}]";
            //            }

            //            string TVTextFR = subsector + " (" + nomFR + ")";

            //            if (tvItemLanguageModelFR.TVText != TVTextFR)
            //            {
            //                tvItemLanguageModelFR.TVText = TVTextFR;

            //                TVItemLanguageModel tvItemLanguageModelFRRet = tvItemLanguageService.PostUpdateTVItemLanguageDB(tvItemLanguageModelFR);
            //                if (!string.IsNullOrWhiteSpace(tvItemLanguageModelFRRet.Error))
            //                {
            //                    richTextBoxStatus.AppendText($"could not update tvItemLanguageModel (FR) for [{subsector}]\r\n");
            //                    return $"could not update tvItemLanguageModel (FR) for [{subsector}]";
            //                }
            //            }

            //            TVItemLanguageModel tvItemLanguageModelEN = tvItemLanguageService.GetTVItemLanguageModelWithTVItemIDAndLanguageDB(tvItemModelSubsector.TVItemID, LanguageEnum.en);
            //            if (!string.IsNullOrWhiteSpace(tvItemLanguageModelEN.Error))
            //            {
            //                richTextBoxStatus.AppendText($"could not find tvItemLanguageModel (EN) for [{subsector}]\r\n");
            //                return $"could not find tvItemLanguageModel (EN) for [{subsector}]";
            //            }

            //            string TVTextEN = subsector + " (" + nomFR + ")";
            //            if (!string.IsNullOrWhiteSpace(nomEN))
            //            {
            //                TVTextEN = subsector + " (" + nomEN + ")";
            //            }

            //            if (tvItemLanguageModelEN.TVText != TVTextEN)
            //            {
            //                tvItemLanguageModelEN.TVText = TVTextEN;

            //                TVItemLanguageModel tvItemLanguageModelENRet = tvItemLanguageService.PostUpdateTVItemLanguageDB(tvItemLanguageModelEN);
            //                if (!string.IsNullOrWhiteSpace(tvItemLanguageModelENRet.Error))
            //                {
            //                    richTextBoxStatus.AppendText($"could not update tvItemLanguageModel (EN) for [{subsector}]\r\n");
            //                    return $"could not update tvItemLanguageModel(EN) for [{ subsector}]";
            //                }
            //            }
            //        }

            //    }


            //}

            lblStatus.Text = "done...";

            return "";
        }

        private string CreateKMLFileWithSubsectorIDAndTideLocation(TVItemService tvItemService, TVItemModel tvItemModelRoot, MapInfoService mapInfoService, MWQMSubsectorService mwqmSubsectorService)
        {
            /// -----------------------------------------------------------------
            /// ------ Create KML file with all the subsector ID and TideLocation selected --
            /// -----------------------------------------------------------------

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Subsector);
            List<MWQMSubsectorModel> mwqmSubsectorModelList = mwqmSubsectorService.GetAllMWQMSubsectorModelDB();

            List<TideLocation> tideLocationList = new List<TideLocation>();
            using (CSSPDBEntities db2 = new CSSPDBEntities())
            {
                tideLocationList = (from c in db2.TideLocations
                                    select c).ToList();
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine($@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine($@"<Document>");
            sb.AppendLine($@"	<name>KmlFile</name>");
            sb.AppendLine($@"	<Style id=""s_ylw-pushpin_hl"">");
            sb.AppendLine($@"		<IconStyle>");
            sb.AppendLine($@"			<scale>1.3</scale>");
            sb.AppendLine($@"			<Icon>");
            sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine($@"			</Icon>");
            sb.AppendLine($@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine($@"		</IconStyle>");
            sb.AppendLine($@"	</Style>");
            sb.AppendLine($@"	<StyleMap id=""m_ylw-pushpin"">");
            sb.AppendLine($@"		<Pair>");
            sb.AppendLine($@"			<key>normal</key>");
            sb.AppendLine($@"			<styleUrl>#s_ylw-pushpin</styleUrl>");
            sb.AppendLine($@"		</Pair>");
            sb.AppendLine($@"		<Pair>");
            sb.AppendLine($@"			<key>highlight</key>");
            sb.AppendLine($@"			<styleUrl>#s_ylw-pushpin_hl</styleUrl>");
            sb.AppendLine($@"		</Pair>");
            sb.AppendLine($@"	</StyleMap>");
            sb.AppendLine($@"	<Style id=""s_ylw-pushpin"">");
            sb.AppendLine($@"		<IconStyle>");
            sb.AppendLine($@"			<scale>1.1</scale>");
            sb.AppendLine($@"			<Icon>");
            sb.AppendLine($@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine($@"			</Icon>");
            sb.AppendLine($@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine($@"		</IconStyle>");
            sb.AppendLine($@"	</Style>");




            foreach (TVItemModel tvItemModelSS in tvItemModelSubsectorList)
            {
                string subsector = tvItemModelSS.TVText;
                if (subsector.Contains(" "))
                {
                    subsector = subsector.Substring(0, subsector.IndexOf(" "));
                }

                lblStatus.Text = subsector;
                lblStatus.Refresh();
                Application.DoEvents();

                sb.AppendLine($@"	<Folder>");
                sb.AppendLine($@"		<name>{subsector}</name>");

                MWQMSubsectorModel mwqmSubsectorModel = (from c in mwqmSubsectorModelList
                                                         where c.MWQMSubsectorTVItemID == tvItemModelSS.TVItemID
                                                         select c).FirstOrDefault();

                if (mwqmSubsectorModel == null)
                {
                    richTextBoxStatus.AppendText($"Could not find mwqmSubsectorModel\r\n");
                    return $"Could not find mwqmSubsectorModel";
                }
                List<MapInfoPointModel> mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.Subsector, MapInfoDrawTypeEnum.Point);
                if (mapInfoPointModelList.Count == 0)
                {
                    richTextBoxStatus.AppendText($"Could not find mapInfoPointModelList\r\n");
                    return $"Could not find mapInfoPointModelList";
                }

                sb.AppendLine($@"	    <Placemark>");
                sb.AppendLine($@"	    	<name>{subsector}</name>");
                sb.AppendLine($@"	    	<styleUrl>#m_ylw-pushpin</styleUrl>");
                sb.AppendLine($@"	    	<Point>");
                sb.AppendLine($@"	    		<coordinates>{mapInfoPointModelList[0].Lng},{mapInfoPointModelList[0].Lat},0</coordinates>");
                sb.AppendLine($@"	    	</Point>");
                sb.AppendLine($@"	    </Placemark>");

                List<int> sidList = new List<int>();

                List<string> sidTextList = mwqmSubsectorModel.TideLocationSIDText.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                if (sidTextList.Count == 0)
                {
                    richTextBoxStatus.AppendText($"Could not find sidTextList\r\n");
                    return $"Could not find sidTextList";
                }

                foreach (string s in sidTextList)
                {
                    if (int.TryParse(s, out int sid))
                    {
                        sidList.Add(sid);
                    }
                }

                List<TideLocation> tideLocation3List = (from c in tideLocationList
                                                        from s in sidList
                                                        where c.sid == s
                                                        select c).ToList();

                foreach (TideLocation tideLocation3 in tideLocation3List)
                {
                    sb.AppendLine($@"	    <Placemark>");
                    sb.AppendLine($@"	    	<name>{tideLocation3.sid}</name>");
                    sb.AppendLine($@"	    	<styleUrl>#m_ylw-pushpin</styleUrl>");
                    sb.AppendLine($@"	    	<Point>");
                    sb.AppendLine($@"	    		<coordinates>{tideLocation3.Lng},{tideLocation3.Lat},0</coordinates>");
                    sb.AppendLine($@"	    	</Point>");
                    sb.AppendLine($@"	    </Placemark>");
                }

                sb.AppendLine($@"	</Folder>");
            }

            sb.AppendLine($@"</Document>");
            sb.AppendLine($@"</kml>");

            FileInfo fi = new FileInfo(@"C:\Users\leblancc\Desktop\testingtidelocation.kml");
            StreamWriter sw = fi.CreateText();
            sw.Write(sb.ToString());
            sw.Close();

            lblStatus.Text = "done...";

            return "";
        }

        private void ButImportQCWQMonitoringToCSSPDB_Click(object sender, EventArgs e)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            TVItemLanguageService tvItemLanguageService = new TVItemLanguageService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
            MWQMSubsectorService mwqmSubsectorService = new MWQMSubsectorService(LanguageEnum.en, user);
            MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            TVItemModel tvItemModelQC = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelQC)) return;

            List<TVItemModel> tvItemModelSubsectorQCList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorQCList.Count == 0)
            {
                richTextBoxStatus.AppendText("Could not find TVItem Subsector under British Columbia\r\n");
                return;
            }

            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {
                List<StationQC> stationList = (from s in dbQC.geo_stations_p
                                               where s.secteur != null
                                               && s.x != null
                                               && s.y != null
                                               orderby s.secteur
                                               select new StationQC
                                               {
                                                   secteur = s.secteur,
                                                   id_geo_station_p = s.id_geo_station_p,
                                                   station = (int)s.station,
                                                   type_station = s.type_station,
                                                   x = (double)s.x,
                                                   y = (double)s.y,
                                               }).ToList();

                List<SampleQC> sampleList = (from m in dbQC.db_mesure
                                             where m != null
                                             select new SampleQC
                                             {
                                                 id_geo_station_p = m.id_geo_station_p,
                                                 id_tournee = m.id_tournee,
                                                 cf = m.cf,
                                                 hre_echantillonnage = m.hre_echantillonnage,
                                                 prof = (double?)m.prof,
                                                 sal = (double?)m.sal,
                                                 temp = (double?)m.temp,
                                                 ph = (double?)m.ph,
                                                 diffusable = m.diffusable,
                                                 commentaire = m.commentaire,
                                             }).ToList();

                //List<SectorQC> subsectorNameList = (from s in dbQC.geo_secteur_s
                //                                    where s.secteur != null
                //                                    orderby s.secteur
                //                                    select new SectorQC
                //                                    {
                //                                        secteur = s.secteur,
                //                                        secteur_nom = s.secteur_nom,
                //                                        secteur_nom_a = s.secteur_nom_a
                //                                    }).Distinct().ToList();


                List<PCCSM.db_tournee> tourneeList = (from m in dbQC.db_tournee
                                                      select m).ToList();

                //List<string> subsectorList = (from c in stationList
                //                              select c.secteur).Distinct().OrderBy(c => c).ToList();

                //lblStatus2.Text = "1 - Starting RemoveAllSectorNotInQCDB";
                //lblStatus2.Refresh();
                //Application.DoEvents();

                //string retStr = RemoveAllSectorNotInQCDB(stationList, tvItemService, tvItemModelQC);
                //if (!string.IsNullOrWhiteSpace(retStr))
                //{
                //    return;
                //}

                //lblStatus2.Text = "2 - Starting AddAllNewSectorFoundInQCDB";
                //lblStatus2.Refresh();
                //Application.DoEvents();

                //retStr = AddAllNewSectorFoundInQCDB(stationList, tvItemService, tvItemModelQC);
                //if (!string.IsNullOrWhiteSpace(retStr))
                //{
                //    return;
                //}

                //lblStatus2.Text = "3 - Starting RemoveAllSubsectorNotInQCDB";
                //lblStatus2.Refresh();
                //Application.DoEvents();

                //retStr = RemoveAllSubsectorNotInQCDB(stationList, tvItemService, tvItemModelQC);
                //if (!string.IsNullOrWhiteSpace(retStr))
                //{
                //    return;
                //}

                //lblStatus2.Text = "4 - Starting AddAllNewSubsectorFoundInQCDB";
                //lblStatus2.Refresh();
                //Application.DoEvents();

                //retStr = AddAllNewSubsectorFoundInQCDB(stationList, tvItemService, tvItemModelQC);
                //if (!string.IsNullOrWhiteSpace(retStr))
                //{
                //    return;
                //}

                //lblStatus2.Text = "5 - Starting AddAllNewSitesFoundInQCDB";
                //lblStatus2.Refresh();
                //Application.DoEvents();

                //string retStr = AddAllNewSitesFoundInQCDB(stationList, tvItemService, tvItemModelQC, mwqmSiteService);
                //if (!string.IsNullOrWhiteSpace(retStr))
                //{
                //    return;
                //}

                //lblStatus2.Text = "5a - Starting RemoveAllSitesNotInQCDB";
                //lblStatus2.Refresh();
                //Application.DoEvents();

                //string retStr = RemoveAllMWQMSitesThatNoLongerExistInQCDB(stationList, tvItemService, tvItemModelQC, mwqmSiteService);
                //if (!string.IsNullOrWhiteSpace(retStr))
                //{
                //    return;
                //}

                //lblStatus2.Text = "6 - Starting AddAllNewRunsFoundInQCDB";
                //lblStatus2.Refresh();
                //Application.DoEvents();

                //retStr = AddAllNewRunsFoundInQCDB(stationList, sampleList, tourneeList, tvItemService, tvItemModelQC, mwqmSiteService, mwqmRunService);
                //if (!string.IsNullOrWhiteSpace(retStr))
                //{
                //    return;
                //}

                lblStatus2.Text = "7 - Starting AddAllNewSampleFoundInQCDB";
                lblStatus2.Refresh();
                Application.DoEvents();

                string retStr = AddAllNewSampleFoundInQCDB(stationList, sampleList, tourneeList, tvItemService, tvItemModelQC, mwqmSiteService, mwqmRunService);
                if (!string.IsNullOrWhiteSpace(retStr))
                {
                    return;
                }

                //lblStatus2.Text = "7 - Starting RemoveSamplesFoundFoundInQCDB";
                //lblStatus2.Refresh();
                //Application.DoEvents();

                //retStr = RemoveSamplesFoundFoundInQCDB(stationList, sampleList, tourneeList, tvItemService, tvItemModelQC, mwqmSiteService, mwqmRunService);
                //if (!string.IsNullOrWhiteSpace(retStr))
                //{
                //    return;
                //}

                //lblStatus2.Text = "8 - Starting AddAllNewMWQMSubsectorTextAndTideInfoFoundInQCDB";
                //lblStatus2.Refresh();
                //Application.DoEvents();

                //retStr = AddAllNewMWQMSubsectorTextAndTideInfoFoundInQCDB(tvItemService, tvItemModelQC, mapInfoService, mwqmSubsectorService);
                //if (!string.IsNullOrWhiteSpace(retStr))
                //{
                //    return;
                //}

                //lblStatus2.Text = "9 - Starting CreateKMLFileWithSubsectorIDAndTideLocation";
                //lblStatus2.Refresh();
                //Application.DoEvents();

                //retStr = CreateKMLFileWithSubsectorIDAndTideLocation(tvItemService, tvItemModelRoot, mapInfoService, mwqmSubsectorService);
                //if (!string.IsNullOrWhiteSpace(retStr))
                //{
                //    return;
                //}

                //lblStatus2.Text = "10 - Starting DeleteAllMWQMSubsectorThatShouldNotExist";
                //lblStatus2.Refresh();
                //Application.DoEvents();

                //retStr = DeleteAllMWQMSubsectorThatShouldNotExist(tvItemService, tvItemModelRoot, mwqmSubsectorService);
                //if (!string.IsNullOrWhiteSpace(retStr))
                //{
                //    return;
                //}

                //lblStatus2.Text = "11 - Starting GiveProperNameToSubsectorFoundInQCDB";
                //lblStatus2.Refresh();
                //Application.DoEvents();

                //retStr = GiveProperNameToSubsectorFoundInQCDB(stationList, subsectorNameList, tvItemService, tvItemModelQC, tvItemLanguageService);
                //if (!string.IsNullOrWhiteSpace(retStr))
                //{
                //    return;
                //}

            }
        }

        public class SubsectorAndSite
        {
            public SubsectorAndSite()
            {

            }

            public string Subsector { get; set; }
            public string MWQMSite { get; set; }

            public int Count { get; set; }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);
            MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);


            richTextBoxStatus.AppendText("Locator\tSubsector name\t2000\t2001\t2002\t2003\t2004\t2005\t2006\t2007\t2008\t2009\t2010\t2011\t2012\t2013\t2014\t2015\t2016\t2017\t2018\t2019 YTD\r\n");
            using (CSSPDBEntities db2 = new CSSPDBEntities())
            {

                foreach (int TVItemID in new List<int>() { 7, 10, 8, 9 })
                {
                    TVItemModel tvItemModelProv = tvItemService.GetTVItemModelWithTVItemIDDB(TVItemID);
                    if (!string.IsNullOrWhiteSpace(tvItemModelProv.Error))
                    {
                        richTextBoxStatus.Text = "Error could not find TVItemID = 7";
                        return;
                    }

                    List<TVItemModel> tvItemModelList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(TVItemID, TVTypeEnum.Subsector);

                    foreach (TVItemModel tvItemModel in tvItemModelList)
                    {
                        string subsector = tvItemModel.TVText;
                        string Desc = "";
                        if (subsector.Contains(" "))
                        {
                            int pos = subsector.IndexOf(" ");
                            subsector = subsector.Substring(0, pos);

                            Desc = tvItemModel.TVText.Substring(pos).Trim();

                            if (Desc.StartsWith("("))
                            {
                                Desc = Desc.Substring(1);
                            }

                            if (Desc.EndsWith(")"))
                            {
                                Desc = Desc.Substring(0, Desc.Length - 1);
                            }
                        }

                        lblStatus.Text = subsector;
                        lblStatus.Refresh();
                        Application.DoEvents();

                        //List<MWQMRunModel> MWQMRunModelList = mwqmRunService.GetMWQMRunModelListWithSubsectorTVItemIDDB(tvItemModel.TVItemID);
                        //List<MWQMSiteModel> MWQMSiteModelList = mwqmSiteService.GetMWQMSiteModelListWithSubsectorTVItemIDDB(tvItemModel.TVItemID);
                        //List<MWQMSampleModel> MWQMSampleModelList = mwqmSampleService.GetMWQMSampleModelListWithSubsectorTVItemIDDB(tvItemModel.TVItemID);

                        StringBuilder sb = new StringBuilder();
                        bool HasBiggerThan0 = false;

                        for (int i = 2000; i < 2020; i++)
                        {


                            //// doing run
                            //int Count = (from c in db2.MWQMRuns
                            //             where c.SubsectorTVItemID == tvItemModel.TVItemID
                            //             && c.RunSampleType == (int)SampleTypeEnum.Routine
                            //             && c.DateTime_Local.Year == i
                            //             select c).Count();

                            //sb.Append($"{Count}\t");

                            //if (Count > 0)
                            //{
                            //    HasBiggerThan0 = true;
                            //}


                            //// doing site
                            //int Count = (from c in db2.MWQMSites
                            //             from s in db2.MWQMSamples
                            //             from t in db2.TVItems
                            //             where c.MWQMSiteTVItemID == t.TVItemID
                            //             && c.MWQMSiteTVItemID == s.MWQMSiteTVItemID
                            //             && s.SampleDateTime_Local.Year == i
                            //             && s.SampleTypesText.Contains("109,")
                            //             && t.ParentID == tvItemModel.TVItemID
                            //             select c).Distinct().Count();

                            //sb.Append($"{Count}\t");

                            //if (Count > 0)
                            //{
                            //    HasBiggerThan0 = true;
                            //}


                            // doing sample
                            int Count = (from c in db2.MWQMSites
                                         from s in db2.MWQMSamples
                                         from t in db2.TVItems
                                         where c.MWQMSiteTVItemID == t.TVItemID
                                         && c.MWQMSiteTVItemID == s.MWQMSiteTVItemID
                                         && s.SampleDateTime_Local.Year == i
                                         && s.SampleTypesText.Contains("109,")
                                         && t.ParentID == tvItemModel.TVItemID
                                         select s).Distinct().Count();

                            sb.Append($"{Count}\t");

                            if (Count > 0)
                            {
                                HasBiggerThan0 = true;
                            }


                        }
                        if (HasBiggerThan0)
                        {
                            richTextBoxStatus.AppendText($"{subsector}\t{Desc}\t{sb.ToString()}\r\n");
                        }
                    }
                }
            }



        }

        private void button19_Click(object sender, EventArgs e)
        {
            //TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            //TVItemLanguageService tvItemLanguageService = new TVItemLanguageService(LanguageEnum.en, user);
            //MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            //MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);
            //MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
            //MWQMSubsectorService mwqmSubsectorService = new MWQMSubsectorService(LanguageEnum.en, user);

            //TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            //if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            //TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            //if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            //TVItemModel tvItemModelNS = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Nova Scotia", TVTypeEnum.Province);
            //if (!CheckModelOK<TVItemModel>(tvItemModelNS)) return;

            //List<TVItemModel> tvItemModelSubsectorNSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelNS.TVItemID, TVTypeEnum.Subsector);
            //if (tvItemModelSubsectorNSList.Count == 0)
            //{
            //    richTextBoxStatus.AppendText("Could not find TVItem Subsector under Nova Scotia\r\n");
            //    return;
            //}

            //List<TVItemModel> tvItemModelNSSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelNS.TVItemID, TVTypeEnum.MWQMSite);
            //if (tvItemModelNSSiteList.Count == 0)
            //{
            //    richTextBoxStatus.AppendText("Could not find TVItem Subsector under Nova Scotia\r\n");
            //    return;
            //}

            //List<TVItemModel> tvItemModelNSSiteInActiveList = new List<TVItemModel>();
            //using (CSSPDBEntities db2 = new CSSPDBEntities())
            //{
            //    var SiteAndInfoList = (from c in db2.MWQMSites
            //                           from t in db2.TVItems
            //                           from m in db2.MapInfos
            //                           where c.MWQMSiteTVItemID == t.TVItemID
            //                           && t.TVItemID == m.TVItemID
            //                           && t.TVPath.StartsWith(tvItemModelNS.TVPath + "p")
            //                           && m.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                           && m.TVType == (int)TVTypeEnum.MWQMSite
            //                           && t.TVType == (int)TVTypeEnum.MWQMSite
            //                           select new { c, m }).ToList();

            //    string FileName = @"C:\CSSP Latest Code old\DataTool\ImportByFunction\Data_inputs\NS_MWQM_Sites_Classification_Final_2019.xlsx";

            //    string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=Excel 12.0";

            //    OleDbConnection conn = new OleDbConnection(connectionString);

            //    conn.Open();
            //    OleDbDataReader reader;
            //    OleDbCommand comm = new OleDbCommand("Select * from [NewSiteClassification$];");

            //    comm.Connection = conn;
            //    reader = comm.ExecuteReader();

            //    int CountRead = 0;
            //    while (reader.Read())
            //    {
            //        CountRead += 1;
            //        if (CountRead < 0)
            //            continue;

            //        Application.DoEvents();

            //        string FID = "";
            //        string LOCATOR = "";
            //        string STAT_NBR = "";
            //        string Class_Type_E = "";
            //        string Approved = "";
            //        string Conditionally_Approved = "";
            //        string Restricted = "";
            //        string Conditionally_Restricted = "";
            //        string Prohibited = "";
            //        string Unclassified = "";
            //        string LATITUDE_WGS84 = "";
            //        string LONGITUDE_WGS84 = "";

            //        int index = 0;
            //        // FID
            //        if (reader.GetValue(index).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(index).ToString()))
            //        {
            //            FID = "";
            //        }
            //        else
            //        {
            //            FID = reader.GetValue(index).ToString().Trim();
            //        }

            //        index = 1;
            //        // LOCATOR
            //        if (reader.GetValue(index).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(index).ToString()))
            //        {
            //            LOCATOR = "";
            //        }
            //        else
            //        {
            //            LOCATOR = reader.GetValue(index).ToString().Trim();
            //        }

            //        index = 2;
            //        // STAT_NBR
            //        if (reader.GetValue(index).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(index).ToString()))
            //        {
            //            STAT_NBR = "";
            //        }
            //        else
            //        {
            //            STAT_NBR = reader.GetValue(index).ToString().Trim();
            //        }

            //        index = 3;
            //        // Class_Type_E
            //        if (reader.GetValue(index).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(index).ToString()))
            //        {
            //            Class_Type_E = "";
            //        }
            //        else
            //        {
            //            Class_Type_E = reader.GetValue(index).ToString().Trim();
            //        }

            //        index = 4;
            //        // Approved
            //        if (reader.GetValue(index).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(index).ToString()))
            //        {
            //            Approved = "";
            //        }
            //        else
            //        {
            //            Approved = reader.GetValue(index).ToString().Trim();
            //        }

            //        index = 5;
            //        // Conditionally_Approved
            //        if (reader.GetValue(index).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(index).ToString()))
            //        {
            //            Conditionally_Approved = "";
            //        }
            //        else
            //        {
            //            Conditionally_Approved = reader.GetValue(index).ToString().Trim();
            //        }

            //        index = 6;
            //        // Restricted
            //        if (reader.GetValue(index).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(index).ToString()))
            //        {
            //            Restricted = "";
            //        }
            //        else
            //        {
            //            Restricted = reader.GetValue(index).ToString().Trim();
            //        }

            //        index = 7;
            //        // Conditionally_Restricted
            //        if (reader.GetValue(index).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(index).ToString()))
            //        {
            //            Conditionally_Restricted = "";
            //        }
            //        else
            //        {
            //            Conditionally_Restricted = reader.GetValue(index).ToString().Trim();
            //        }

            //        index = 8;
            //        // Prohibited
            //        if (reader.GetValue(index).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(index).ToString()))
            //        {
            //            Prohibited = "";
            //        }
            //        else
            //        {
            //            Prohibited = reader.GetValue(index).ToString().Trim();
            //        }

            //        index = 9;
            //        // Unclassified
            //        if (reader.GetValue(index).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(index).ToString()))
            //        {
            //            Unclassified = "";
            //        }
            //        else
            //        {
            //            Unclassified = reader.GetValue(index).ToString().Trim();
            //        }

            //        index = 10;
            //        // LATITUDE_WGS84
            //        if (reader.GetValue(index).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(index).ToString()))
            //        {
            //            LATITUDE_WGS84 = "";
            //        }
            //        else
            //        {
            //            LATITUDE_WGS84 = reader.GetValue(index).ToString().Trim();
            //        }

            //        index = 11;
            //        // LONGITUDE_WGS84
            //        if (reader.GetValue(index).GetType() == typeof(DBNull) || string.IsNullOrEmpty(reader.GetValue(index).ToString()))
            //        {
            //            LONGITUDE_WGS84 = "";
            //        }
            //        else
            //        {
            //            LONGITUDE_WGS84 = reader.GetValue(index).ToString().Trim();
            //        }

            //        TVItemModel tvItemModelSS = (from c in tvItemModelSubsectorNSList
            //                                     where c.TVText.StartsWith(LOCATOR + " ")
            //                                     select c).FirstOrDefault();

            //        if (tvItemModelSS == null)
            //        {
            //            richTextBoxStatus.AppendText($"{LOCATOR} could not be found in TVItemModelNSList");
            //            return;
            //        }

            //        string TVText = "0000".Substring(0, (4 - STAT_NBR.Length)) + STAT_NBR.ToString();

            //        TVItemModel tvItemModelSite = (from c in tvItemModelNSSiteList
            //                                       where c.ParentID == tvItemModelSS.TVItemID
            //                                       && c.TVText == TVText
            //                                       select c).FirstOrDefault();

            //        if (tvItemModelSite == null)
            //        {
            //            richTextBoxStatus.AppendText($"{STAT_NBR} could not be found in tvItemModelNSSiteList");
            //            return;
            //        }

            //        tvItemModelNSSiteInActiveList.Add(tvItemModelSite);

            //        var siteAndInfo = (from c in SiteAndInfoList
            //                           where c.m.TVItemID == tvItemModelSite.TVItemID
            //                           select c).FirstOrDefault();


            //        if (siteAndInfo == null)
            //        {
            //            richTextBoxStatus.AppendText($"Could not find siteAndInfo within SiteAndInfoList with c.m.TVItemID = [{tvItemModelSite.TVItemID}]");
            //            return;
            //        }

            //        if (tvItemModelSite.IsActive != true)
            //        {
            //            tvItemModelSite.IsActive = true;

            //            TVItemModel tvItemModelRet = tvItemService.PostUpdateTVItemDB(tvItemModelSite);
            //            if (!string.IsNullOrWhiteSpace(tvItemModelRet.Error))
            //            {
            //                richTextBoxStatus.AppendText($"Could not update tvItemModel with TVItemID = [{tvItemModelSite.TVItemID}]");
            //                return;
            //            }
            //        }

            //        lblStatus.Text = tvItemModelSS.TVText + " --- " + tvItemModelSite.TVText;
            //        lblStatus.Refresh();
            //        Application.DoEvents();

            //        using (CSSPDBEntities db3 = new CSSPDBEntities())
            //        {
            //            List<MapInfoPoint> mapInfoPointList = (from c in db3.MapInfoPoints
            //                                                   where c.MapInfoID == siteAndInfo.m.MapInfoID
            //                                                   select c).ToList();

            //            if (mapInfoPointList.Count > 0)
            //            {
            //                if (!float.TryParse(LATITUDE_WGS84, out float lat))
            //                {
            //                    richTextBoxStatus.AppendText($"Could not read Lat for TVItemID = [{tvItemModelSite.TVItemID}]");
            //                    return;
            //                }
            //                if (!float.TryParse(LONGITUDE_WGS84, out float lng))
            //                {
            //                    richTextBoxStatus.AppendText($"Could not read Lng for TVItemID = [{tvItemModelSite.TVItemID}]");
            //                    return;
            //                }

            //                if (mapInfoPointList[0].Lat != lat || mapInfoPointList[0].Lng != lng)
            //                {
            //                    mapInfoPointList[0].Lat = lat;
            //                    mapInfoPointList[0].Lng = lng;

            //                    try
            //                    {
            //                        db3.SaveChanges();
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        richTextBoxStatus.AppendText($"Could not read Lng for TVItemID = [{tvItemModelSite.TVItemID}]");
            //                        return;
            //                    }
            //                }

            //            }
            //        }

            //        using (CSSPDBEntities db3 = new CSSPDBEntities())
            //        {
            //            MWQMSite mwqmSite = (from c in db3.MWQMSites
            //                                 where c.MWQMSiteTVItemID == siteAndInfo.c.MWQMSiteTVItemID
            //                                 select c).FirstOrDefault();

            //            if (mwqmSite == null)
            //            {
            //                richTextBoxStatus.AppendText($"Could not find MWQMSite where MWQMSiteTVItemID = [{siteAndInfo.c.MWQMSiteTVItemID}]");
            //                return;
            //            }

            //            MWQMSiteLatestClassificationEnum mwqmSiteLatestClassification = MWQMSiteLatestClassificationEnum.Error;

            //            switch (Class_Type_E)
            //            {
            //                case "Approved":
            //                    {
            //                        if (Approved != "1")
            //                        {
            //                            richTextBoxStatus.AppendText($"Class_Type_E does not match Approved = [{siteAndInfo.c.MWQMSiteTVItemID}]");
            //                            return;
            //                        }
            //                        mwqmSiteLatestClassification = MWQMSiteLatestClassificationEnum.Approved;
            //                    }
            //                    break;
            //                case "Restricted":
            //                    {
            //                        if (Restricted != "1")
            //                        {
            //                            richTextBoxStatus.AppendText($"Class_Type_E does not match Restricted = [{siteAndInfo.c.MWQMSiteTVItemID}]");
            //                            return;
            //                        }
            //                        mwqmSiteLatestClassification = MWQMSiteLatestClassificationEnum.Restricted;
            //                    }
            //                    break;
            //                case "Conditionally Restricted":
            //                    {
            //                        if (Conditionally_Restricted != "1")
            //                        {
            //                            richTextBoxStatus.AppendText($"Class_Type_E does not match Conditionaly Restricted = [{siteAndInfo.c.MWQMSiteTVItemID}]");
            //                            return;
            //                        }
            //                        mwqmSiteLatestClassification = MWQMSiteLatestClassificationEnum.ConditionallyRestricted;
            //                    }
            //                    break;
            //                case "Conditionally Approved":
            //                    {
            //                        if (Conditionally_Approved != "1")
            //                        {
            //                            richTextBoxStatus.AppendText($"Class_Type_E does not match Conditionaly Approved = [{siteAndInfo.c.MWQMSiteTVItemID}]");
            //                            return;
            //                        }
            //                        mwqmSiteLatestClassification = MWQMSiteLatestClassificationEnum.ConditionallyApproved;
            //                    }
            //                    break;
            //                case "Prohibited":
            //                    {
            //                        if (Prohibited != "1")
            //                        {
            //                            richTextBoxStatus.AppendText($"Class_Type_E does not match Prohibited = [{siteAndInfo.c.MWQMSiteTVItemID}]");
            //                            return;
            //                        }
            //                        mwqmSiteLatestClassification = MWQMSiteLatestClassificationEnum.Prohibited;
            //                    }
            //                    break;
            //                case "Unclassified":
            //                    {
            //                        if (Unclassified != "1")
            //                        {
            //                            richTextBoxStatus.AppendText($"Class_Type_E does not match Unclassified = [{siteAndInfo.c.MWQMSiteTVItemID}]");
            //                            return;
            //                        }
            //                        mwqmSiteLatestClassification = MWQMSiteLatestClassificationEnum.Unclassified;
            //                    }
            //                    break;
            //                default:
            //                    {
            //                        richTextBoxStatus.AppendText($"Classified not found = [{Class_Type_E}]");
            //                        return;
            //                    }
            //            }

            //            if ((int)mwqmSiteLatestClassification != mwqmSite.MWQMSiteLatestClassification)
            //            {
            //                try
            //                {
            //                    db3.SaveChanges();
            //                }
            //                catch (Exception ex)
            //                {
            //                    richTextBoxStatus.AppendText($"Could not read Lng for MWQMSite = [{tvItemModelSite.TVItemID}]");
            //                    return;
            //                }
            //            }
            //        }
            //    }

            //    foreach (TVItemModel tvItemModel in tvItemModelNSSiteList)
            //    {
            //        if (!tvItemModelNSSiteInActiveList.Where(c => c.TVItemID == tvItemModel.TVItemID).Any())
            //        {
            //            if (tvItemModel.IsActive != false)
            //            {
            //                tvItemModel.IsActive = false;

            //                richTextBoxStatus.AppendText($"{tvItemModel.TVText} was set to inactive\r\n");
            //                TVItemModel tvItemModelRet = tvItemService.PostUpdateTVItemDB(tvItemModel);
            //                if (!string.IsNullOrWhiteSpace(tvItemModelRet.Error))
            //                {
            //                    richTextBoxStatus.AppendText($"Could not update tvItemModel with TVItemID = [{tvItemModel.TVItemID}]");
            //                    return;
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private void button20_Click(object sender, EventArgs e)
        {
            #region Remove Washington and Maine state CoCoRaHS site that are more than 100 km from NB and BC Subsectors

            //List<string> SubsectorNameList = new List<string>()
            //{
            //    "NB-18-010-001",
            //    "VI01"
            //};

            //TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            //MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            //MapInfoPointService mapInfoPointService = new MapInfoPointService(LanguageEnum.en, user);


            //TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            //if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            //{
            //    richTextBoxStatus.AppendText($"Error: {tvItemModelRoot.Error}\r\n");
            //    return;
            //}

            //foreach (string SubsectorName in SubsectorNameList)
            //{
            //    string StartWith = "ME-";

            //    if (SubsectorName == "VI01")
            //    {
            //        StartWith = "WA-";
            //    }

            //    TVItemModel tvItemModelSS = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, SubsectorName + " ", TVTypeEnum.Subsector);
            //    if (!string.IsNullOrWhiteSpace(tvItemModelSS.Error))
            //    {
            //        richTextBoxStatus.AppendText($"Error: {tvItemModelSS.Error}\r\n");
            //        return;
            //    }

            //    List<MapInfoPointModel> mapInfoPointModelList = mapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.Subsector, MapInfoDrawTypeEnum.Point);
            //    if (mapInfoPointModelList.Count == 0)
            //    {
            //        richTextBoxStatus.AppendText($"Error: mapInfoPointModelList count should not be 0\r\n");
            //        return;
            //    }

            //    double ssLat = mapInfoPointModelList[0].Lat;
            //    double ssLng = mapInfoPointModelList[0].Lng;

            //    using (CoCoRaHSModel.CoCoRaHSEntities db = new CoCoRaHSModel.CoCoRaHSEntities())
            //    {

            //        List<CoCoRaHSModel.CoCoRaHSSite> cocorahsSiteList = (from c in db.CoCoRaHSSites
            //                                                             where c.StationNumber.StartsWith(StartWith)
            //                                                             orderby c.StationNumber
            //                                                             select c).ToList();

            //        int count = 0;
            //        foreach (CoCoRaHSModel.CoCoRaHSSite cocorahsSite in cocorahsSiteList)
            //        {
            //            count += 1;

            //            lblStatus.Text = cocorahsSite.StationNumber + " --- " + count;
            //            lblStatus.Refresh();
            //            Application.DoEvents();

            //            double dist = mapInfoService.CalculateDistance(ssLat * mapInfoService.d2r, ssLng * mapInfoService.d2r, cocorahsSite.Latitude * mapInfoService.d2r,
            //                cocorahsSite.Longitude * mapInfoService.d2r, mapInfoService.R);

            //            if (dist > 100000)
            //            {
            //                richTextBoxStatus.AppendText($"Should delete {cocorahsSite.StationNumber} --- {count}\r\n");

            //                db.CoCoRaHSSites.Remove(cocorahsSite);

            //                try
            //                {
            //                    db.SaveChanges();
            //                }
            //                catch (Exception ex)
            //                {
            //                    richTextBoxStatus.AppendText($"Error: {ex.Message}\r\n");
            //                    return;
            //                }
            //            }
            //        }
            //    }
            //}

            //using (CoCoRaHSModel.CoCoRaHSEntities db = new CoCoRaHSModel.CoCoRaHSEntities())
            //{

            //    List<string> cocorahsStationNumberExistList = (from c in db.CoCoRaHSSites
            //                                                   orderby c.StationNumber
            //                                                   select c.StationNumber).Distinct().ToList();

            //    List<string> cocorahsStationNumberList = (from c in db.CoCoRaHSValues
            //                                              orderby c.StationNumber
            //                                              select c.StationNumber).Distinct().ToList();

            //    int count = 0;
            //    foreach (string StationNumber in cocorahsStationNumberList)
            //    {
            //        count += 1;

            //        lblStatus.Text = "Deleting CoCoRaHSValues with " + StationNumber + " --- " + count;
            //        lblStatus.Refresh();
            //        Application.DoEvents();

            //        if (!cocorahsStationNumberExistList.Contains(StationNumber))
            //        {
            //            using (CoCoRaHSModel.CoCoRaHSEntities db2 = new CoCoRaHSModel.CoCoRaHSEntities())
            //            {
            //                List<CoCoRaHSModel.CoCoRaHSValue> cocorahsValueList = (from c in db2.CoCoRaHSValues
            //                                                                       where c.StationNumber == StationNumber
            //                                                                       select c).ToList();

            //                try
            //                {
            //                    db2.CoCoRaHSValues.RemoveRange(cocorahsValueList);
            //                    db2.SaveChanges();
            //                }
            //                catch (Exception)
            //                {
            //                    richTextBoxStatus.AppendText($"Error: Deleting CoCoRaHSValues with StationNumber {StationNumber}\r\n");
            //                    return;

            //                }
            //            }
            //        }
            //    }

            //}


            //using (CoCoRaHSModel.CoCoRaHSEntities db = new CoCoRaHSModel.CoCoRaHSEntities())
            //{

            //    List<CoCoRaHSModel.CoCoRaHSSite> cocorahsSiteList = (from c in db.CoCoRaHSSites
            //                                                         orderby c.StationNumber
            //                                                         select c).ToList();

            //    int count = 0;
            //    foreach (CoCoRaHSModel.CoCoRaHSSite coCoRaHSSite in cocorahsSiteList)
            //    {
            //        count += 1;

            //        lblStatus.Text = coCoRaHSSite.StationNumber + " --- " + count;
            //        lblStatus.Refresh();
            //        Application.DoEvents();

            //        using (CoCoRaHSModel.CoCoRaHSEntities db2 = new CoCoRaHSModel.CoCoRaHSEntities())
            //        {

            //            List<CoCoRaHSModel.CoCoRaHSValue> cocorahsValueList = (from c in db2.CoCoRaHSValues
            //                                                                   where c.StationNumber == coCoRaHSSite.StationNumber
            //                                                                   select c).ToList();

            //            foreach (CoCoRaHSModel.CoCoRaHSValue coCoRaHSValue in cocorahsValueList)
            //            {
            //                coCoRaHSValue.CoCoRaHSSiteID = coCoRaHSSite.CoCoRaHSSiteID;
            //            }

            //            try
            //            {
            //                db2.SaveChanges();
            //            }
            //            catch (Exception ex)
            //            {
            //                richTextBoxStatus.AppendText($"Error: {ex.Message}\r\n");
            //                return;
            //            }

            //        }

            //    }


            //}


            //using (CoCoRaHSModel.CoCoRaHSEntities db = new CoCoRaHSModel.CoCoRaHSEntities())
            //{
            //    TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            //    ClimateSiteService climateSiteService = new ClimateSiteService(LanguageEnum.en, user);
            //    MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            //    MapInfoPointService mapInfoPointService = new MapInfoPointService(LanguageEnum.en, user);

            //    TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            //    if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            //    {
            //        richTextBoxStatus.AppendText($"Error: {tvItemModelRoot.Error}\r\n");
            //        return;
            //    }

            //    TVItemModel tvItemModelNB = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "New Brunswick", TVTypeEnum.Province);
            //    if (!string.IsNullOrWhiteSpace(tvItemModelNB.Error))
            //    {
            //        richTextBoxStatus.AppendText($"Error: {tvItemModelNB.Error}\r\n");
            //        return;
            //    }

            //    TVItemModel tvItemModelNL = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "Newfoundland and Labrador", TVTypeEnum.Province);
            //    if (!string.IsNullOrWhiteSpace(tvItemModelNL.Error))
            //    {
            //        richTextBoxStatus.AppendText($"Error: {tvItemModelNL.Error}\r\n");
            //        return;
            //    }

            //    TVItemModel tvItemModelNS = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "Nova Scotia", TVTypeEnum.Province);
            //    if (!string.IsNullOrWhiteSpace(tvItemModelNS.Error))
            //    {
            //        richTextBoxStatus.AppendText($"Error: {tvItemModelNS.Error}\r\n");
            //        return;
            //    }

            //    TVItemModel tvItemModelPE = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "Prince Edward Island", TVTypeEnum.Province);
            //    if (!string.IsNullOrWhiteSpace(tvItemModelPE.Error))
            //    {
            //        richTextBoxStatus.AppendText($"Error: {tvItemModelPE.Error}\r\n");
            //        return;
            //    }

            //    TVItemModel tvItemModelQC = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "Québec", TVTypeEnum.Province);
            //    if (!string.IsNullOrWhiteSpace(tvItemModelQC.Error))
            //    {
            //        richTextBoxStatus.AppendText($"Error: {tvItemModelQC.Error}\r\n");
            //        return;
            //    }

            //    TVItemModel tvItemModelBC = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "British Columbia", TVTypeEnum.Province);
            //    if (!string.IsNullOrWhiteSpace(tvItemModelBC.Error))
            //    {
            //        richTextBoxStatus.AppendText($"Error: {tvItemModelBC.Error}\r\n");
            //        return;
            //    }

            //    TVItemModel tvItemModelME = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "Maine", TVTypeEnum.Province);
            //    if (!string.IsNullOrWhiteSpace(tvItemModelME.Error))
            //    {
            //        richTextBoxStatus.AppendText($"Error: {tvItemModelME.Error}\r\n");
            //        return;
            //    }

            //    TVItemModel tvItemModelWA = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, "Washington", TVTypeEnum.Province);
            //    if (!string.IsNullOrWhiteSpace(tvItemModelWA.Error))
            //    {
            //        richTextBoxStatus.AppendText($"Error: {tvItemModelWA.Error}\r\n");
            //        return;
            //    }

            //    TVItemModel tvItemModelProvince = null;

            //    List<ClimateSite> climateSiteList = new List<ClimateSite>();
            //    using (CSSPDBEntities db2 = new CSSPDBEntities())
            //    {
            //        climateSiteList = (from c in db2.ClimateSites
            //                           select c).ToList();
            //    }

            //    List<CoCoRaHSModel.CoCoRaHSSite> cocorahsSiteList = (from c in db.CoCoRaHSSites
            //                                                         orderby c.StationNumber
            //                                                         select c).ToList();

            //    int count = 0;
            //    foreach (CoCoRaHSModel.CoCoRaHSSite coCoRaHSSite in cocorahsSiteList)
            //    {
            //        count += 1;

            //        lblStatus.Text = coCoRaHSSite.StationNumber + " --- " + count;
            //        lblStatus.Refresh();
            //        Application.DoEvents();

            //        using (CoCoRaHSModel.CoCoRaHSEntities db2 = new CoCoRaHSModel.CoCoRaHSEntities())
            //        {

            //            List<CoCoRaHSModel.CoCoRaHSValue> cocorahsValueList = (from c in db2.CoCoRaHSValues
            //                                                                   where c.CoCoRaHSSiteID == coCoRaHSSite.CoCoRaHSSiteID
            //                                                                   select c).ToList();

            //            DateTime FirstDate = (from c in cocorahsValueList
            //                                  orderby c.ObservationDateAndTime ascending
            //                                  select c.ObservationDateAndTime).FirstOrDefault();

            //            DateTime LastDate = (from c in cocorahsValueList
            //                                 orderby c.ObservationDateAndTime descending
            //                                 select c.ObservationDateAndTime).FirstOrDefault();

            //            bool DailyNow = false;
            //            if (LastDate.Date > new DateTime(2019, 10, 10))
            //            {
            //                DailyNow = true;
            //            }

            //            ClimateSite climateSite = (from c in climateSiteList
            //                                       where c.ClimateID == coCoRaHSSite.StationNumber
            //                                       select c).FirstOrDefault();

            //            if (climateSite == null)
            //            {
            //                string Prov = "";
            //                if (coCoRaHSSite.StationNumber.StartsWith("CAN-"))
            //                {
            //                    if (coCoRaHSSite.StationNumber.StartsWith("CAN-NB"))
            //                    {
            //                        Prov = "NB";
            //                        tvItemModelProvince = tvItemModelNB;
            //                    }
            //                    else if (coCoRaHSSite.StationNumber.StartsWith("CAN-NL"))
            //                    {
            //                        Prov = "NL";
            //                        tvItemModelProvince = tvItemModelNL;
            //                    }
            //                    else if (coCoRaHSSite.StationNumber.StartsWith("CAN-NS"))
            //                    {
            //                        Prov = "NS";
            //                        tvItemModelProvince = tvItemModelNS;
            //                    }
            //                    else if (coCoRaHSSite.StationNumber.StartsWith("CAN-PE"))
            //                    {
            //                        Prov = "PE";
            //                        tvItemModelProvince = tvItemModelPE;
            //                    }
            //                    else if (coCoRaHSSite.StationNumber.StartsWith("CAN-QC"))
            //                    {
            //                        Prov = "QC";
            //                        tvItemModelProvince = tvItemModelQC;
            //                    }
            //                    else if (coCoRaHSSite.StationNumber.StartsWith("CAN-BC"))
            //                    {
            //                        Prov = "BC";
            //                        tvItemModelProvince = tvItemModelBC;
            //                    }
            //                    else
            //                    {
            //                        richTextBoxStatus.AppendText($"Error: CoCoRaHSSite.StationNumber does not start with [CAN-NB, CAN-NL, CAN-NS, CAN-PE, CAN-NB, CAN-NB, CAN-NB, ]\r\n");
            //                        return;
            //                    }
            //                }
            //                else if (coCoRaHSSite.StationNumber.StartsWith("WA-"))
            //                {
            //                    Prov = "WA";
            //                    tvItemModelProvince = tvItemModelWA;
            //                }
            //                else if (coCoRaHSSite.StationNumber.StartsWith("ME-"))
            //                {
            //                    Prov = "ME";
            //                    tvItemModelProvince = tvItemModelME;
            //                }
            //                else
            //                {
            //                    richTextBoxStatus.AppendText($"Error: CoCoRaHSSite.StationNumber does not start with [CAN-, WA, ME]\r\n");
            //                    return;
            //                }

            //                string TVText = "CoCoRaHS " + coCoRaHSSite.StationName + "(" + coCoRaHSSite.StationNumber + ")";

            //                TVItemModel tvItemModelClimateSite = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelProvince.TVItemID, TVText, TVTypeEnum.ClimateSite);
            //                if (!string.IsNullOrEmpty(tvItemModelClimateSite.Error)) // climate site does not exist
            //                {
            //                    tvItemModelClimateSite = tvItemService.PostAddChildTVItemDB(tvItemModelProvince.TVItemID, TVText, TVTypeEnum.ClimateSite);
            //                    if (!string.IsNullOrEmpty(tvItemModelClimateSite.Error)) // climate site does not exist
            //                    {
            //                        richTextBoxStatus.AppendText($"Error: Could not create new TVItemModelClimateSite with TVItemID [{tvItemModelProvince.TVItemID}] and TVText [{TVText}]\r\n");
            //                        return;
            //                    }
            //                }

            //                MapInfoModel mapInfoModel = new MapInfoModel();

            //                List<MapInfoModel> mapInfoModelList = mapInfoService.GetMapInfoModelListWithTVItemIDDB(tvItemModelClimateSite.TVItemID);
            //                mapInfoModel = (from c in mapInfoModelList
            //                                where c.MapInfoDrawType == MapInfoDrawTypeEnum.Point
            //                                && c.TVType == TVTypeEnum.ClimateSite
            //                                select c).FirstOrDefault();

            //                if (mapInfoModel == null)
            //                {

            //                    List<Coord> coordList = new List<Coord>()
            //                    {
            //                        new Coord() { Lat = (float)coCoRaHSSite.Latitude, Lng = (float)coCoRaHSSite.Longitude, Ordinal = 0 },
            //                    };

            //                    mapInfoModel = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.ClimateSite, tvItemModelClimateSite.TVItemID);
            //                    if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
            //                    {
            //                        richTextBoxStatus.AppendText($"Error: Could not create new MapInfo with StationName [{coCoRaHSSite.StationName}] and StationNumber [{coCoRaHSSite.StationNumber}]\r\n");
            //                        return;
            //                    }
            //                }

            //                ClimateSiteModel climateSiteModelNew = new ClimateSiteModel()
            //                {
            //                    ClimateSiteTVItemID = tvItemModelClimateSite.TVItemID,
            //                    ClimateSiteName = coCoRaHSSite.StationName,
            //                    Province = Prov,
            //                    ClimateID = coCoRaHSSite.StationNumber,
            //                    DailyStartDate_Local = FirstDate,
            //                    DailyEndDate_Local = LastDate,
            //                    DailyNow = DailyNow,
            //                };

            //                ClimateSiteModel climateSiteModelExist = climateSiteService.GetClimateSiteModelExistDB(climateSiteModelNew);
            //                if (!string.IsNullOrWhiteSpace(climateSiteModelExist.Error))
            //                {
            //                    ClimateSiteModel climateSiteModelRet = climateSiteService.PostAddClimateSiteDB(climateSiteModelNew);
            //                    if (!string.IsNullOrWhiteSpace(climateSiteModelRet.Error))
            //                    {
            //                        richTextBoxStatus.AppendText($"Error: Could not create new ClimateSite with StationName [{coCoRaHSSite.StationName}] and StationNumber [{coCoRaHSSite.StationNumber}]\r\n");
            //                        return;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion Remove Washington and Maine state CoCoRaHS site that are more than 100 km from NB and BC Subsectors

            #region Loading all CoCoRaHS values in CoCoRaHS database
            //List<string> FirstCharList = new List<string>()
            //{
            //    "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
            //};

            //string dir = @"C:\Users\leblancc\Desktop\CoCoRaHS\";

            //DirectoryInfo di = new DirectoryInfo(dir);

            //List<FileInfo> fileInfoList = di.GetFiles().Where(c => c.Name.EndsWith(".csv")).ToList();

            //foreach (FileInfo fi in fileInfoList)
            //{
            //    List<CoCoRaHSModel.CoCoRaHSValue> CoCoRaHSValueList = new List<CoCoRaHSModel.CoCoRaHSValue>();

            //    if (!fi.Exists)
            //    {
            //        richTextBoxStatus.AppendText($"[{fi.FullName}] does not exist\r\n");
            //        return;
            //    }

            //    lblStatus.Text = fi.FullName;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    TextReader tr = fi.OpenText();

            //    string LineTxt = tr.ReadLine(); // this will remove the first row
            //    LineTxt = tr.ReadLine();
            //    int countLine = 0;
            //    while (!string.IsNullOrWhiteSpace(LineTxt))
            //    {
            //        if (countLine % 1000 == 0)
            //        {
            //            lblStatus.Text = countLine + " --- " + fi.FullName;
            //            lblStatus.Refresh();
            //            Application.DoEvents();

            //            using (CoCoRaHSModel.CoCoRaHSEntities db = new CoCoRaHSModel.CoCoRaHSEntities())
            //            {
            //                try
            //                {
            //                    db.CoCoRaHSValues.AddRange(CoCoRaHSValueList);
            //                    db.SaveChanges();
            //                }
            //                catch (Exception ex)
            //                {
            //                    richTextBoxStatus.AppendText($"Error: {ex.Message}\r\n");
            //                    return;
            //                }
            //            }

            //            CoCoRaHSValueList = new List<CoCoRaHSModel.CoCoRaHSValue>();
            //        }

            //        countLine += 1;
            //        List<string> valList = LineTxt.Split(",".ToCharArray(), StringSplitOptions.None).Select(c => c.Trim()).ToList();

            //        if (valList.Count != 13)
            //        {
            //            richTextBoxStatus.AppendText($"LineTxt {countLine} of document [{fi.FullName}] does not have 13 elements\r\n");
            //            return;
            //        }

            //        if (!DateTime.TryParse(valList[0], out DateTime ObservationDateAndTime))
            //        {
            //            richTextBoxStatus.AppendText($"Could not parse LineTxt {countLine} of document [{fi.FullName}] element [0] to DateTime\r\n");
            //            return;
            //        }

            //        if (!int.TryParse(valList[1].Substring(0, 2), out int Hour))
            //        {
            //            richTextBoxStatus.AppendText($"Could not parse LineTxt {countLine} of document [{fi.FullName}] element [1] first two characters to int\r\n");
            //            return;
            //        }

            //        if (!int.TryParse(valList[1].Substring(3, 2), out int Min))
            //        {
            //            richTextBoxStatus.AppendText($"Could not parse LineTxt {countLine} of document [{fi.FullName}] element [1] characters 3 to 5 to int\r\n");
            //            return;
            //        }

            //        string AmPm = valList[1].Substring(6, 2);
            //        if (AmPm == "PM")
            //        {
            //            Hour += 12;
            //        }
            //        else
            //        {
            //            if (AmPm != "AM")
            //            {
            //                richTextBoxStatus.AppendText($"Could not parse LineTxt {countLine} of document [{fi.FullName}] element [1] to proper AM PM\r\n");
            //                return;
            //            }
            //        }

            //        ObservationDateAndTime = ObservationDateAndTime.AddHours(Hour);

            //        ObservationDateAndTime = ObservationDateAndTime.AddMinutes(Min);

            //        string StationNumber = valList[3];
            //        string StationName = valList[4];

            //        if (!double.TryParse(valList[5], out double Latitude))
            //        {
            //            richTextBoxStatus.AppendText($"Could not parse LineTxt {countLine} of document [{fi.FullName}] element [5] (Latitude) to double\r\n");
            //            return;
            //        }

            //        if (!double.TryParse(valList[6], out double Longitude))
            //        {
            //            richTextBoxStatus.AppendText($"Could not parse LineTxt {countLine} of document [{fi.FullName}] element [6] (Longitude) to double\r\n");
            //            return;
            //        }

            //        // getting TotalPrecipAmt
            //        double? TotalPrecipAmt = null;
            //        double? TotalPrecipAmtInInches = null;
            //        string FirstChar = valList[7].Substring(0, 1);
            //        if (FirstCharList.Contains(FirstChar))
            //        {
            //            // should be a number
            //            TotalPrecipAmtInInches = double.Parse(valList[7]);
            //        }
            //        else
            //        {
            //            if (FirstChar == "T")
            //            {
            //                TotalPrecipAmtInInches = 0;
            //            }
            //        }

            //        TotalPrecipAmt = TotalPrecipAmtInInches * 25.4;

            //        // getting NewSnowDepth
            //        double? NewSnowDepth = null;
            //        double? NewSnowDepthInInches = null;
            //        FirstChar = valList[8].Substring(0, 1);
            //        if (FirstCharList.Contains(FirstChar))
            //        {
            //            // should be a number
            //            NewSnowDepthInInches = double.Parse(valList[8]);
            //            NewSnowDepth = NewSnowDepthInInches * 25.4;
            //        }
            //        else
            //        {
            //            if (FirstChar == "N")
            //            {
            //                NewSnowDepthInInches = null;
            //            }
            //        }

            //        // getting NewSnowSWE
            //        double? NewSnowSWE = null;
            //        double? NewSnowSWEInInches = null;
            //        FirstChar = valList[9].Substring(0, 1);
            //        if (FirstCharList.Contains(FirstChar))
            //        {
            //            // should be a number
            //            NewSnowSWEInInches = double.Parse(valList[9]);
            //            NewSnowSWE = NewSnowSWEInInches * 25.4;
            //        }
            //        else
            //        {
            //            if (FirstChar == "N")
            //            {
            //                NewSnowSWEInInches = null;
            //            }
            //        }

            //        // getting TotalSnowDepth
            //        double? TotalSnowDepth = null;
            //        double? TotalSnowDepthInInches = null;
            //        FirstChar = valList[10].Substring(0, 1);
            //        if (FirstCharList.Contains(FirstChar))
            //        {
            //            // should be a number
            //            TotalSnowDepthInInches = double.Parse(valList[10]);
            //            TotalSnowDepth = TotalSnowDepthInInches * 25.4;
            //        }
            //        else
            //        {
            //            if (FirstChar == "N")
            //            {
            //                TotalSnowDepthInInches = null;
            //            }
            //        }

            //        // getting TotalSnowSWE
            //        double? TotalSnowSWE = null;
            //        double? TotalSnowSWEInInches = null;
            //        FirstChar = valList[11].Substring(0, 1);
            //        if (FirstCharList.Contains(FirstChar))
            //        {
            //            // should be a number
            //            TotalSnowSWEInInches = double.Parse(valList[11]);
            //            TotalSnowSWE = TotalSnowSWEInInches * 25.4;
            //        }
            //        else
            //        {
            //            if (FirstChar == "N")
            //            {
            //                TotalSnowSWEInInches = null;
            //            }
            //        }

            //        CoCoRaHSModel.CoCoRaHSValue cocorahsValue = new CoCoRaHSModel.CoCoRaHSValue()
            //        {
            //             ObservationDateAndTime = ObservationDateAndTime,
            //             StationNumber = StationNumber,
            //             StationName = StationName,
            //             Latitude = Latitude,
            //             Longitude = Longitude,
            //             TotalPrecipAmt = TotalPrecipAmt,
            //             NewSnowDepth = NewSnowDepth,
            //             NewSnowSWE = NewSnowSWE,
            //             TotalSnowDepth = TotalSnowDepth,
            //             TotalSnowSWE = TotalSnowSWE
            //        };

            //        CoCoRaHSValueList.Add(cocorahsValue);

            //        LineTxt = tr.ReadLine();

            //    }

            //    tr.Close();

            //    using (CoCoRaHSModel.CoCoRaHSEntities db = new CoCoRaHSModel.CoCoRaHSEntities())
            //    {
            //        try
            //        {
            //            db.CoCoRaHSValues.AddRange(CoCoRaHSValueList);
            //            db.SaveChanges();
            //        }
            //        catch (Exception ex)
            //        {
            //            richTextBoxStatus.AppendText($"Error: {ex.Message}\r\n");
            //            return;
            //        }
            //    }

            //}

            #endregion Loading all CoCoRaHS values in CoCoRaHS database

            #region Temp Loading CoCoRaHS site and value in CoCoRaHS DB and CSSPDB

            //string dir = @"C:\Users\leblancc\Desktop\CoCoRaHS\";

            //DirectoryInfo di = new DirectoryInfo(dir);

            //List<FileInfo> fileInfoList = di.GetFiles().Where(c => c.Name.EndsWith(".csv")).ToList();

            //foreach (FileInfo fi in fileInfoList)
            //{
            //    if (!fi.Exists)
            //    {
            //        richTextBoxStatus.AppendText($"[{fi.FullName}] does not exist\r\n");
            //        return;
            //    }

            //    lblStatus.Text = fi.FullName;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    TextReader tr = fi.OpenText();

            //    string cocoPrecipData = tr.ReadToEnd();

            //    if (fi.FullName.Contains("_ME_"))
            //    {
            //        ParseCoCoRaHSExportData(cocoPrecipData, "ME");
            //    }
            //    else if (fi.FullName.Contains("_WA_"))
            //    {
            //        ParseCoCoRaHSExportData(cocoPrecipData, "WA");
            //    }
            //    else
            //    {
            //        ParseCoCoRaHSExportData(cocoPrecipData, "CAN");
            //    }
            //}


            #endregion Temp Loading CoCoRaHS site and value in CoCoRaHS DB and CSSPDB

            lblStatus.Text = "done...";

            #region Creating Xlsx file with subsectors, runs, climate site and climate values
            //StringBuilder sb = new StringBuilder();

            //TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            //MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
            //UseOfSiteService useOfSiteService = new UseOfSiteService(LanguageEnum.en, user);
            //ClimateSiteService climateSiteService = new ClimateSiteService(LanguageEnum.en, user);
            //ClimateDataValueService climateDataValueService = new ClimateDataValueService(LanguageEnum.en, user);
            //MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            //TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            //if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            //{
            //    richTextBoxStatus.AppendText($"Could not find TVItemModelRoot");
            //    return;
            //}

            //string ProvInit = "";
            //using (CSSPDBEntities db2 = new CSSPDBEntities())
            //{
            //    var ClimateSiteAndLocationList = (from c in db2.ClimateSites
            //                                      from mi in db2.MapInfos
            //                                      from mip in db2.MapInfoPoints
            //                                      where c.ClimateSiteTVItemID == mi.TVItemID
            //                                      && mi.MapInfoID == mip.MapInfoID
            //                                      && mi.TVType == (int)TVTypeEnum.ClimateSite
            //                                      && mi.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                                      select new { c, mi, mip }).ToList();

            //    var SubsectorAndLocationList = (from c in db2.TVItems
            //                                    from mi in db2.MapInfos
            //                                    from mip in db2.MapInfoPoints
            //                                    where c.TVItemID == mi.TVItemID
            //                                    && mi.MapInfoID == mip.MapInfoID
            //                                    && mi.TVType == (int)TVTypeEnum.Subsector
            //                                    && mi.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
            //                                    && c.TVType == (int)TVTypeEnum.Subsector
            //                                    && c.TVPath.StartsWith(tvItemModelRoot.TVPath + "p")
            //                                    select new { c, mi, mip }).ToList();

            //    List<string> ProvList = new List<string>()
            //    {
            //        //"New Brunswick",
            //        //"Newfoundland and Labrador",
            //        //"Nova Scotia",
            //        "Prince Edward Island"
            //    };

            //    List<string> ProvInitList = new List<string>()
            //    {
            //        //"NB",
            //        //"NL",
            //        //"NS",
            //        "PE"
            //    };

            //    sb.AppendLine($"Subsector,Year,Month,Day,Climate_Site_Name,ClimateID,Dist_km,D0,DM1,DM2,DM3,DM4,DM5,DM6,DM7,DM8,DM9,DM10");

            //    for (int p = 0; p < ProvList.Count; p++)
            //    {
            //        ProvInit = ProvInitList[p];

            //        TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, ProvList[p], TVTypeEnum.Province);
            //        if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            //        {
            //            richTextBoxStatus.AppendText($"Could not find TVItemModelRoot");
            //            return;
            //        }

            //        List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);

            //        foreach (TVItemModel tvItemModelss in tvItemModelSubsectorList.OrderBy(c => c.TVText))
            //        {
            //            string subsector = tvItemModelss.TVText;
            //            if (subsector.Contains(" "))
            //            {
            //                subsector = subsector.Substring(0, subsector.IndexOf(" "));
            //            }

            //            lblStatus.Text = subsector;
            //            lblStatus.Refresh();
            //            Application.DoEvents();

            //            List<UseOfSiteModel> useOfSiteModelList = useOfSiteService.GetUseOfSiteModelListWithTVTypeAndSubsectorTVItemIDDB(TVTypeEnum.ClimateSite, tvItemModelss.TVItemID);

            //            List<ClimateSiteModel> climateSiteModelList = new List<ClimateSiteModel>();

            //            foreach (UseOfSiteModel useOfSiteModel in useOfSiteModelList)
            //            {
            //                ClimateSiteModel climateSiteModel = climateSiteService.GetClimateSiteModelWithClimateSiteTVItemIDDB(useOfSiteModel.SiteTVItemID);
            //                if (string.IsNullOrWhiteSpace(climateSiteModel.Error))
            //                {
            //                    climateSiteModelList.Add(climateSiteModel);
            //                }
            //            }
            //            List<MWQMRunModel> mwqmRunModelList = mwqmRunService.GetMWQMRunModelListWithSubsectorTVItemIDDB(tvItemModelss.TVItemID);

            //            List<ClimateDataValueModel> climateDataValueModelList = new List<ClimateDataValueModel>();

            //            foreach (ClimateSiteModel climateSiteModel in climateSiteModelList)
            //            {
            //                List<ClimateDataValueModel> climateDataValueModelList2 = climateDataValueService.GetClimateDataValueModelWithClimateSiteIDDB(climateSiteModel.ClimateSiteID);

            //                climateDataValueModelList.AddRange(climateDataValueModelList2);
            //            }

            //            foreach (MWQMRunModel mwqmRunModel in mwqmRunModelList.OrderByDescending(c => c.MWQMRunTVText))
            //            {
            //                string run = mwqmRunModel.MWQMRunTVText;
            //                string year = run.Substring(0, 4);
            //                string month = run.Substring(5, 2);
            //                string day = run.Substring(8, 2);

            //                lblStatus.Text = subsector + " --- " + run;
            //                lblStatus.Refresh();
            //                Application.DoEvents();

            //                DateTime StartDate = mwqmRunModel.DateTime_Local;
            //                DateTime EndDate = mwqmRunModel.DateTime_Local.AddDays(-10);

            //                string RD0 = mwqmRunModel.RainDay0_mm == null ? "" : ((double)mwqmRunModel.RainDay0_mm).ToString("F1");
            //                string RD1 = mwqmRunModel.RainDay1_mm == null ? "" : ((double)mwqmRunModel.RainDay1_mm).ToString("F1");
            //                string RD2 = mwqmRunModel.RainDay2_mm == null ? "" : ((double)mwqmRunModel.RainDay2_mm).ToString("F1");
            //                string RD3 = mwqmRunModel.RainDay3_mm == null ? "" : ((double)mwqmRunModel.RainDay3_mm).ToString("F1");
            //                string RD4 = mwqmRunModel.RainDay4_mm == null ? "" : ((double)mwqmRunModel.RainDay4_mm).ToString("F1");
            //                string RD5 = mwqmRunModel.RainDay5_mm == null ? "" : ((double)mwqmRunModel.RainDay5_mm).ToString("F1");
            //                string RD6 = mwqmRunModel.RainDay6_mm == null ? "" : ((double)mwqmRunModel.RainDay6_mm).ToString("F1");
            //                string RD7 = mwqmRunModel.RainDay7_mm == null ? "" : ((double)mwqmRunModel.RainDay7_mm).ToString("F1");
            //                string RD8 = mwqmRunModel.RainDay8_mm == null ? "" : ((double)mwqmRunModel.RainDay8_mm).ToString("F1");
            //                string RD9 = mwqmRunModel.RainDay9_mm == null ? "" : ((double)mwqmRunModel.RainDay9_mm).ToString("F1");
            //                string RD10 = mwqmRunModel.RainDay10_mm == null ? "" : ((double)mwqmRunModel.RainDay10_mm).ToString("F1");

            //                sb.AppendLine($"{subsector},{year},{month},{day},-------,----------,------,{RD0},{RD1},{RD2},{RD3},{RD4},{RD5},{RD6},{RD7},{RD8},{RD9},{RD10}");

            //                foreach (ClimateSiteModel climateSiteModel in climateSiteModelList)
            //                {
            //                    string climateSiteName = climateSiteModel.ClimateSiteName;
            //                    string climateID = climateSiteModel.ClimateID;


            //                    List<ClimateDataValueModel> climateDataValueModelsList = (from c in climateDataValueModelList
            //                                                                              where c.ClimateSiteID == climateSiteModel.ClimateSiteID
            //                                                                              && c.DateTime_Local <= StartDate
            //                                                                              && c.DateTime_Local >= EndDate
            //                                                                              orderby c.DateTime_Local descending
            //                                                                              select c).ToList();

            //                    bool hasValue = false;
            //                    StringBuilder DataValues = new StringBuilder();
            //                    for (int i = 0; i < climateDataValueModelsList.Count; i++)
            //                    {
            //                        if (climateDataValueModelsList[i].TotalPrecip_mm_cm == null)
            //                        {
            //                            DataValues.Append(",");
            //                        }
            //                        else
            //                        {
            //                            hasValue = true;
            //                            DataValues.Append("," + ((double)(climateDataValueModelsList[i].TotalPrecip_mm_cm)).ToString("F1"));
            //                        }
            //                    }

            //                    if (hasValue)
            //                    {
            //                        var climateCoord = (from r in ClimateSiteAndLocationList
            //                                            where r.c.ClimateID == climateID
            //                                            select new { r.mip.Lat, r.mip.Lng }).FirstOrDefault();

            //                        var subsectorCoord = (from r in SubsectorAndLocationList
            //                                              where r.c.TVItemID == tvItemModelss.TVItemID
            //                                              select new { r.mip.Lat, r.mip.Lng }).FirstOrDefault();

            //                        string distText = "";
            //                        if (climateCoord != null && subsectorCoord != null)
            //                        {
            //                            double dist = mapInfoService.CalculateDistance(climateCoord.Lat * mapInfoService.d2r, climateCoord.Lng * mapInfoService.d2r, subsectorCoord.Lat * mapInfoService.d2r, subsectorCoord.Lng * mapInfoService.d2r, mapInfoService.R);

            //                            dist = dist / 1000.0D;

            //                            distText = dist.ToString("F0");
            //                        }

            //                        // DataValues already has the starting comma
            //                        sb.AppendLine($"{subsector},{year},{month},{day},{climateSiteName},{climateID},{distText}{DataValues}");
            //                    }
            //                }
            //            }
            //        }

            //    }

            //    FileInfo fi = new FileInfo($@"C:\Users\leblancc\Desktop\{ProvInit}_ClimateData.csv");

            //    StreamWriter sw = fi.CreateText();
            //    sw.WriteLine(sb.ToString());
            //    sw.Close();

            //    richTextBoxStatus.Text = sb.ToString();

            //}
            #endregion Creating Xlsx file with subsectors, runs, climate site and climate values
        }

        private void butCompareQCDBAndCSSPDB_Click(object sender, EventArgs e)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            TVItemLanguageService tvItemLanguageService = new TVItemLanguageService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);
            MWQMSubsectorService mwqmSubsectorService = new MWQMSubsectorService(LanguageEnum.en, user);
            MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            TVItemModel tvItemModelQC = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelQC)) return;

            List<TVItemModel> tvItemModelSubsectorQCList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorQCList.Count == 0)
            {
                richTextBoxStatus.AppendText("Could not find TVItem Subsector under Quebec\r\n");
                return;
            }
            List<TVItemModel> tvItemModelSiteQCList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.MWQMSite);
            if (tvItemModelSiteQCList.Count == 0)
            {
                richTextBoxStatus.AppendText("Could not find TVItem MWQMSite under Quebec\r\n");
                return;
            }
            List<TVItemModel> tvItemModelRunQCList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.MWQMRun);
            if (tvItemModelSiteQCList.Count == 0)
            {
                richTextBoxStatus.AppendText("Could not find TVItem MWQMSite under Quebec\r\n");
                return;
            }

            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {
                List<StationQC> stationList = (from s in dbQC.geo_stations_p
                                               where s.secteur != null
                                               && s.x != null
                                               && s.y != null
                                               orderby s.secteur
                                               select new StationQC
                                               {
                                                   secteur = s.secteur,
                                                   id_geo_station_p = s.id_geo_station_p,
                                                   station = (int)s.station,
                                                   type_station = s.type_station,
                                                   x = (double)s.x,
                                                   y = (double)s.y,
                                               }).ToList();

                List<SampleQC> sampleList = (from m in dbQC.db_mesure
                                             select new SampleQC
                                             {
                                                 id_geo_station_p = m.id_geo_station_p,
                                                 id_tournee = m.id_tournee,
                                                 cf = m.cf,
                                                 hre_echantillonnage = m.hre_echantillonnage,
                                                 prof = (double?)m.prof,
                                                 sal = (double?)m.sal,
                                                 temp = (double?)m.temp,
                                                 ph = (double?)m.ph,
                                                 diffusable = m.diffusable,
                                                 commentaire = m.commentaire,
                                             }).ToList();

                //List<SectorQC> subsectorNameList = (from s in dbQC.geo_secteur_s
                //                                    where s.secteur != null
                //                                    orderby s.secteur
                //                                    select new SectorQC
                //                                    {
                //                                        secteur = s.secteur,
                //                                        secteur_nom = s.secteur_nom,
                //                                        secteur_nom_a = s.secteur_nom_a
                //                                    }).Distinct().ToList();


                List<PCCSM.db_tournee> tourneeList = (from m in dbQC.db_tournee
                                                      select m).ToList();


                List<string> subsectorQCList = (from c in stationList
                                                select c.secteur).Distinct().ToList();

                List<string> subsectorCSSPList = new List<string>();

                List<SubsectorAndSiteList> subsectorAndSiteListQC = new List<SubsectorAndSiteList>();

                foreach (string subsector in subsectorQCList)
                {
                    List<int> SiteList = (from c in stationList
                                          where c.secteur == subsector
                                          select c.station).Distinct().ToList();

                    List<string> SiteTextList = new List<string>();

                    foreach (int site in SiteList)
                    {
                        string siteText = site.ToString();
                        while (siteText.Length < 4)
                        {
                            siteText = "0" + siteText;
                        }
                        SiteTextList.Add(siteText);
                    }

                    subsectorAndSiteListQC.Add(new SubsectorAndSiteList() { Subsector = subsector, SiteList = SiteTextList });
                }

                List<SubsectorAndSiteList> subsectorAndSiteListCSSP = new List<SubsectorAndSiteList>();

                foreach (TVItemModel tvItemModelSubsector in tvItemModelSubsectorQCList)
                {
                    string subsector = tvItemModelSubsector.TVText;
                    if (subsector.Contains(" "))
                    {
                        subsector = subsector.Substring(0, subsector.IndexOf(" "));
                    }

                    subsectorCSSPList.Add(subsector);

                    List<string> siteList = (from c in tvItemModelSiteQCList
                                             where c.TVPath.StartsWith(tvItemModelSubsector.TVPath + "p")
                                             select c.TVText).ToList();

                    subsectorAndSiteListCSSP.Add(new SubsectorAndSiteList() { Subsector = subsector, SiteList = siteList });
                }

                // checking that All Subsector and Site in CSSP DB exist in QC DB
                foreach (SubsectorAndSiteList subsectorAndSiteList in subsectorAndSiteListCSSP)
                {
                    lblStatus.Text = subsectorAndSiteList.Subsector;
                    lblStatus.Refresh();
                    Application.DoEvents();

                    SubsectorAndSiteList subsectorAndSiteListExist = (from c in subsectorAndSiteListQC
                                                                      where c.Subsector == subsectorAndSiteList.Subsector
                                                                      select c).FirstOrDefault();

                    if (subsectorAndSiteListExist == null)
                    {
                        richTextBoxStatus.AppendText($"{subsectorAndSiteListExist.Subsector} does not exist in QCDB\r\n");
                    }

                    foreach (string site in subsectorAndSiteList.SiteList)
                    {
                        string siteExist = (from c in subsectorAndSiteListExist.SiteList
                                            where c == site
                                            select c).FirstOrDefault();

                        if (siteExist == null)
                        {
                            richTextBoxStatus.AppendText($"{subsectorAndSiteListExist.Subsector} site {site} does not exist in QCDB\r\n");
                        }
                    }

                }

                // checking that All Subsector and Site in QC DB exist in CSSP DB
                foreach (SubsectorAndSiteList subsectorAndSiteList in subsectorAndSiteListQC)
                {
                    lblStatus.Text = subsectorAndSiteList.Subsector;
                    lblStatus.Refresh();
                    Application.DoEvents();

                    SubsectorAndSiteList subsectorAndSiteListExist = (from c in subsectorAndSiteListCSSP
                                                                      where c.Subsector == subsectorAndSiteList.Subsector
                                                                      select c).FirstOrDefault();

                    if (subsectorAndSiteListExist == null)
                    {
                        richTextBoxStatus.AppendText($"{subsectorAndSiteListExist.Subsector} does not exist in QCDB\r\n");
                    }

                    foreach (string site in subsectorAndSiteList.SiteList)
                    {
                        string siteExist = (from c in subsectorAndSiteListExist.SiteList
                                            where c == site
                                            select c).FirstOrDefault();

                        if (siteExist == null)
                        {
                            richTextBoxStatus.AppendText($"{subsectorAndSiteListExist.Subsector} site {site} does not exist in QCDB\r\n");
                        }
                    }

                }

                // checking runs number and dates as well as samples
                foreach (TVItemModel tvItemModelSS in tvItemModelSubsectorQCList)
                {
                    List<MWQMSampleModel> mwqmSampleModelListSS = mwqmSampleService.GetMWQMSampleModelListWithSubsectorTVItemIDDB(tvItemModelSS.TVItemID);

                    string subsector = tvItemModelSS.TVText;
                    if (subsector.Contains(" "))
                    {
                        subsector = subsector.Substring(0, subsector.IndexOf(" "));
                    }

                    lblStatus.Text = tvItemModelSS.TVText;
                    lblStatus.Refresh();
                    Application.DoEvents();


                    // Checking runs
                    List<TVItemModel> runTVTextList = (from c in tvItemModelRunQCList
                                                       where c.TVPath.StartsWith(tvItemModelSS.TVPath + "p")
                                                       select c).ToList();

                    List<StationQC> stationQCList = (from c in stationList
                                                     where c.secteur == subsector
                                                     select c).ToList();

                    List<SampleQC> sampleQCList = (from c in sampleList
                                                   from s in stationQCList
                                                   where c.id_geo_station_p == s.id_geo_station_p
                                                   select c).ToList();

                    //List<PCCSM.db_tournee> runQCList = (from c in tourneeList
                    //                                    from st in stationQCList
                    //                                    from sa in sampleQCList
                    //                                    where sa.id_tournee == c.ID_Tournee
                    //                                    && sa.id_geo_station_p == st.id_geo_station_p
                    //                                    && c.date_echantillonnage != null
                    //                                    select c).Distinct().ToList();


                    //if (runTVTextList.Count != runQCList.Count)
                    //{
                    //    richTextBoxStatus.AppendText($"{subsector} number of runs CSSP {runTVTextList.Count} QC {runQCList.Count}\r\n");
                    //}

                    //foreach (TVItemModel tvItemModel in runTVTextList)
                    //{
                    //    int year = int.Parse(tvItemModel.TVText.Substring(0, 4));
                    //    int month = int.Parse(tvItemModel.TVText.Substring(5, 2));
                    //    int day = int.Parse(tvItemModel.TVText.Substring(8, 2));
                    //    DateTime dateCSSP = new DateTime(year, month, day);

                    //    if (!runQCList.Where(c => c.date_echantillonnage == dateCSSP).Any())
                    //    {
                    //        richTextBoxStatus.AppendText($"{subsector} runs date CSSP {dateCSSP.ToString("yyyy MM dd")} does not exist in QC DB\r\n");
                    //    }
                    //}

                    // Checking samples
                    List<MWQMSiteModel> mwqmSiteModelList = mwqmSiteService.GetMWQMSiteModelListWithSubsectorTVItemIDDB(tvItemModelSS.TVItemID);
                    foreach (MWQMSiteModel mwqmSiteModel in mwqmSiteModelList)
                    {
                        lblStatus.Text = tvItemModelSS.TVText + " --- " + mwqmSiteModel.MWQMSiteTVText;
                        lblStatus.Refresh();
                        Application.DoEvents();

                        int site = int.Parse(mwqmSiteModel.MWQMSiteTVText);

                        StationQC stationQC = (from c in stationQCList
                                               where c.station == site
                                               select c).FirstOrDefault();

                        if (stationQC == null)
                        {
                            richTextBoxStatus.AppendText($"{subsector} site {site} not found in stationQCList\r\n");
                        }
                        else
                        {
                            List<MWQMSampleModel> mwqmSampleModelList = (from c in mwqmSampleModelListSS
                                                                         where c.MWQMSiteTVItemID == mwqmSiteModel.MWQMSiteTVItemID
                                                                         select c).ToList();

                            List<SampleQC> sampleQCList2 = (from c in sampleList
                                                            where c.id_geo_station_p == stationQC.id_geo_station_p
                                                            select c).ToList();


                            if (mwqmSampleModelList.Count != sampleQCList2.Count)
                            {
                                richTextBoxStatus.AppendText($"{subsector} number of sample for site {site} CSSP {mwqmSampleModelList.Count} QC {sampleQCList2.Count}\r\n");
                            }

                            //for (int i = 0; i < sampleQCList2.Count; i++)
                            //{
                            //    int year = ((DateTime)sampleQCList2[i].r.date_echantillonnage).Year;
                            //    int month = ((DateTime)sampleQCList2[i].r.date_echantillonnage).Month;
                            //    int day = ((DateTime)sampleQCList2[i].r.date_echantillonnage).Day;

                            //    MWQMSampleModel mwqmSampleModel = (from c in mwqmSampleModelList
                            //                                       where c.SampleDateTime_Local.Year == year
                            //                                       && c.SampleDateTime_Local.Month == month
                            //                                       && c.SampleDateTime_Local.Day == day
                            //                                       select c).FirstOrDefault();

                            //    if (mwqmSampleModel == null)
                            //    {
                            //        richTextBoxStatus.AppendText($"{subsector} site {site} date {((DateTime)sampleQCList2[i].r.date_echantillonnage).ToString("yyyy MM dd")} not found in CSSP DB\r\n");
                            //    }
                            //    else
                            //    {
                            //        if (Math.Abs(mwqmSampleModel.FecCol_MPN_100ml - ((int)sampleQCList2[i].c.cf)) > 1)
                            //        {
                            //            richTextBoxStatus.AppendText($"{subsector} site {site} CF CSSP {mwqmSampleModel.FecCol_MPN_100ml} CF QC {sampleQCList2[i].c.cf}\r\n");
                            //        }
                            //        if (mwqmSampleModel.Salinity_PPT != null)
                            //        {
                            //            if (Math.Abs(((double)mwqmSampleModel.Salinity_PPT) - ((double)sampleQCList2[i].c.sal)) > 0.01)
                            //            {
                            //                richTextBoxStatus.AppendText($"{subsector} site {site} CF CSSP {mwqmSampleModel.Salinity_PPT} CF QC {sampleQCList2[i].c.sal}\r\n");
                            //            }
                            //        }
                            //        if (mwqmSampleModel.WaterTemp_C != null)
                            //        {
                            //            if (Math.Abs(((double)mwqmSampleModel.WaterTemp_C) - ((double)sampleQCList2[i].c.temp)) > 0.01)
                            //            {
                            //                richTextBoxStatus.AppendText($"{subsector} site {site} CF CSSP {mwqmSampleModel.WaterTemp_C} CF QC {sampleQCList2[i].c.temp}\r\n");
                            //            }
                            //        }
                            //    }
                            //}
                        }
                    }
                }
            }

            lblStatus.Text = "done ...";
            lblStatus.Refresh();
            Application.DoEvents();

        }

        private void butUpdateClimateSites_Click(object sender, EventArgs e)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            TVItemLanguageService tvItemLanguageService = new TVItemLanguageService(LanguageEnum.en, user);
            ClimateSiteService climateSiteService = new ClimateSiteService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                richTextBoxStatus.AppendText($"ERROR: {tvItemModelRoot.Error}");
                return;
            }

            List<TVItemModel> tvItemModelClimateSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.ClimateSite);
            List<ClimateSite> climateSiteList = new List<ClimateSite>();
            using (CSSPDBEntities db2 = new CSSPDBEntities())
            {
                climateSiteList = (from c in db2.ClimateSites
                                   select c).ToList();
            }

            #region Verify number of climate site used by subsector
            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Subsector);
            foreach (TVItemModel tvItemModel in tvItemModelSubsectorList)
            {
                string ProvInit = "";
                if (tvItemModel.TVPath.Contains("p11p"))
                {
                    ProvInit = "BC";
                }
                if (tvItemModel.TVPath.Contains("p7p"))
                {
                    ProvInit = "NB";
                }
                if (tvItemModel.TVPath.Contains("p10p"))
                {
                    ProvInit = "NL";
                }
                if (tvItemModel.TVPath.Contains("p8p"))
                {
                    ProvInit = "NS";
                }
                if (tvItemModel.TVPath.Contains("p9p"))
                {
                    ProvInit = "PE";
                }
                if (tvItemModel.TVPath.Contains("p12p"))
                {
                    ProvInit = "QC";
                }
                string TVText = tvItemModel.TVText;
                if (TVText.Contains(" "))
                {
                    TVText = TVText.Substring(0, TVText.IndexOf(" "));
                }

                lblStatus.Text = TVText;
                lblStatus.Refresh();
                Application.DoEvents();


                using (CSSPDBEntities db2 = new CSSPDBEntities())
                {

                    var useClimateList = (from c in db2.UseOfSites
                                          from s in db2.ClimateSites
                                          where c.SiteTVItemID == s.ClimateSiteTVItemID
                                          && c.SubsectorTVItemID == tvItemModel.TVItemID
                                          && c.TVType == (int)TVTypeEnum.ClimateSite
                                          select new { c, s }).ToList();

                    richTextBoxStatus.AppendText($"{tvItemModel.TVItemID}\t{TVText}\t{ProvInit}");
                    foreach (var useClimate in useClimateList)
                    {
                        richTextBoxStatus.AppendText($"\t{useClimate.s.ClimateSiteName} ({useClimate.s.ClimateID})");
                    }
                    richTextBoxStatus.AppendText("\r\n");
                }
            }
            #endregion Verify number of climate site used by subsector

            #region Verifying TVItemS (ClimateSite) and ClimateSite match

            //foreach (ClimateSite climateSite in climateSiteList)
            //{
            //    string TVText = climateSite.ClimateSiteName + " (" + climateSite.ClimateID + ")";

            //    if (climateSite.IsCoCoRaHS != null && climateSite.IsCoCoRaHS == true)
            //    {
            //        TVText = "CoCoRaHS " + TVText;
            //    }

            //    lblStatus.Text = TVText;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    TVItemModel tvItemModel = (from c in tvItemModelClimateSiteList
            //                               where c.TVText == TVText
            //                               select c).FirstOrDefault();

            //    if (tvItemModel == null)
            //    {
            //        richTextBoxStatus.AppendText($"{TVText} not found in CSSPDB\r\n");
            //    }
            //}

            //foreach (TVItemModel tvItemModel in tvItemModelClimateSiteList)
            //{
            //    string TVText = tvItemModel.TVText;

            //    string TVText2 = tvItemModel.TVText;
            //    if (!TVText2.Contains(" ("))
            //    {
            //        TVText2 = TVText2.Replace("(", " (");
            //    }


            //    lblStatus.Text = TVText;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    ClimateSite climateSite = (from c in climateSiteList
            //                               let tvText1 = c.ClimateSiteName + " (" + c.ClimateID + ")"
            //                               let tvText2 = "CoCoRaHS " + tvText1
            //                               where tvText1 == TVText
            //                               || tvText2 == TVText
            //                               select c).FirstOrDefault();

            //    if (climateSite == null)
            //    {
            //        richTextBoxStatus.AppendText($"{TVText} not found in ClimateSites\r\n");

            //        using (CSSPDBEntities db2 = new CSSPDBEntities())
            //        {
            //            bool found = false;
            //            ClimateDataValue climateDataValue = (from c in db2.ClimateDataValues
            //                                                 from cs in db2.ClimateSites
            //                                                 from t in db2.TVItems
            //                                                 where cs.ClimateSiteTVItemID == t.TVItemID
            //                                                 && cs.ClimateSiteID == c.ClimateSiteID
            //                                                 && t.TVItemID == tvItemModel.TVItemID
            //                                                 select c).FirstOrDefault();

            //            if (climateDataValue != null)
            //            {
            //                richTextBoxStatus.AppendText($"\tClimateDataValues found\r\n");
            //                found = true;
            //            }

            //            UseOfSite useOfSite = (from c in db2.UseOfSites
            //                                   where c.SiteTVItemID == tvItemModel.TVItemID
            //                                   select c).FirstOrDefault();

            //            if (useOfSite != null)
            //            {
            //                richTextBoxStatus.AppendText($"\tUseOfSites found\r\n");
            //                found = true;
            //            }

            //            if (!found)
            //            {
            //                MapInfo mapInfo = (from c in db2.MapInfos
            //                                   where c.TVItemID == tvItemModel.TVItemID
            //                                   select c).FirstOrDefault();

            //                try
            //                {
            //                    db2.MapInfos.Remove(mapInfo);
            //                    db2.SaveChanges();
            //                }
            //                catch (Exception ex)
            //                {
            //                    richTextBoxStatus.AppendText($"ERRRRR: {ex.Message}");
            //                }

            //                TVItem tvItem = (from c in db2.TVItems
            //                                 where c.TVItemID == tvItemModel.TVItemID
            //                                 select c).FirstOrDefault();

            //                try
            //                {
            //                    db2.TVItems.Remove(tvItem);
            //                    db2.SaveChanges();
            //                }
            //                catch (Exception ex)
            //                {
            //                    richTextBoxStatus.AppendText($"ERRRRR: {ex.Message}");
            //                }
            //            }
            //        }
            //    }
            //}

            //using (CSSPDBEntities db2 = new CSSPDBEntities())
            //{
            //    foreach (TVItemModel tvItemModel in tvItemModelClimateSiteList)
            //    {
            //        lblStatus.Text = tvItemModel.TVText;
            //        lblStatus.Refresh();
            //        Application.DoEvents();

            //        MapInfo mapInfo = (from c in db2.MapInfos
            //                           where c.TVItemID == tvItemModel.TVItemID
            //                           select c).FirstOrDefault();

            //        if (mapInfo == null)
            //        {
            //            richTextBoxStatus.AppendText($"{tvItemModel.TVText} does not have MapInfo");
            //        }
            //    }
            //}
            #endregion  Verifying TVItemS (ClimateSite) and ClimateSite match


            #region Getting all Climate Sites Info from https://climate.weather.gc.ca/....
            //HttpClient httpClient = new HttpClient();

            //List<string> provList = new List<string>()
            //{
            //    "NB", "NL", "NS", "PE", "QC", "BC"
            //};
            //foreach (string prov in provList)
            //{
            //    for (int i = 0; i < 10000; i = i + 100)
            //    {
            //        Task<string> taskRes = httpClient.GetStringAsync("https://climate.weather.gc.ca/historical_data/search_historic_data_stations_e.html?searchType=stnProv&timeframe=1&" +
            //            "lstProvince=" + prov + "&optLimit=yearRange&StartYear=1840&EndYear=2019&Year=2019&Month=12&Day=11&selRowPerPage=100" +
            //            "&txtCentralLatMin=0&txtCentralLatSec=0&txtCentralLongMin=0&txtCentralLongSec=0&startRow=" + i.ToString() + "");

            //        string res = taskRes.Result.ToString();

            //        if (res.Contains("/climate_data/interform_e.html"))
            //        {
            //            res = res.Replace("<link", "<NOlink");
            //            res = res.Replace("<script", "<NOscript");

            //            FileInfo fi = new FileInfo($@"C:\CSSP Latest Code Old\DataTool\ImportByFunction\Assets\ClimateMetaData\{prov}_{i}.html");
            //            StreamWriter sw = fi.CreateText();
            //            sw.WriteLine(res);
            //            sw.Close();
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //}
            #endregion Getting all Climate Sites Info from https://climate.weather.gc.ca/....

            #region Parsing all assets files C:\CSSP Latest Code Old\DataTool\ImportByFunction\Assets\ClimateMetaData
            //StringBuilder sb = new StringBuilder();
            //using (IWebDriver driver = new ChromeDriver())
            //{
            //    List<string> provList = new List<string>()
            //    {
            //        "NB", "NL", "NS", "PE", "QC", "BC"
            //    };

            //    foreach (string prov in provList)
            //    {
            //        for (int i = 0; i < 10000; i += 100)
            //        {
            //            string fileName = @"C:\CSSP Latest Code Old\DataTool\ImportByFunction\Assets\ClimateMetaData\" + prov + "_" + i.ToString() + ".html";
            //            FileInfo fi = new FileInfo(fileName);
            //            if (fi.Exists)
            //            {
            //                driver.Navigate().GoToUrl(fileName);

            //                IReadOnlyCollection<IWebElement> webElements = driver.FindElements(By.TagName("form"));
            //                int count = 0;
            //                foreach (IWebElement webElement in webElements)
            //                {
            //                    if (webElement.GetAttribute("action").Contains("/climate_data/interform_e.html"))
            //                    {
            //                        if (!webElement.GetAttribute("id").EndsWith("sm"))
            //                        {
            //                            count++;
            //                            //sb.AppendLine($"{webElement.TagName} --- {count} -- {webElement.GetAttribute("id")}");
            //                            string hlyRange = webElement.FindElement(By.CssSelector("input[name='hlyRange']")).GetAttribute("value");
            //                            hlyRange = hlyRange.Trim();
            //                            DateTime? hlyStartDate = null;
            //                            DateTime? hlyEndDate = null;
            //                            string ret = SetupDate(hlyRange, ref hlyStartDate, ref hlyEndDate);
            //                            if (!string.IsNullOrEmpty(ret))
            //                            {
            //                                richTextBoxStatus.AppendText(ret);
            //                                return;
            //                            }

            //                            string dlyRange = webElement.FindElement(By.CssSelector("input[name='dlyRange']")).GetAttribute("value");
            //                            dlyRange = dlyRange.Trim();
            //                            DateTime? dlyStartDate = null;
            //                            DateTime? dlyEndDate = null;
            //                            ret = SetupDate(dlyRange, ref dlyStartDate, ref dlyEndDate);
            //                            if (!string.IsNullOrEmpty(ret))
            //                            {
            //                                richTextBoxStatus.AppendText(ret);
            //                                return;
            //                            }

            //                            string mlyRange = webElement.FindElement(By.CssSelector("input[name='mlyRange']")).GetAttribute("value");
            //                            mlyRange = mlyRange.Trim();
            //                            DateTime? mlyStartDate = null;
            //                            DateTime? mlyEndDate = null;
            //                            ret = SetupDate(mlyRange, ref mlyStartDate, ref mlyEndDate);
            //                            if (!string.IsNullOrEmpty(ret))
            //                            {
            //                                richTextBoxStatus.AppendText(ret);
            //                                return;
            //                            }

            //                            string ECDBIDText = webElement.FindElement(By.CssSelector("input[name='StationID']")).GetAttribute("value");
            //                            if (!int.TryParse(ECDBIDText, out int ECDBID))
            //                            {
            //                                richTextBoxStatus.AppendText($"Could not parse {ECDBIDText} to an int\r\n");
            //                                return;
            //                            }
            //                            string Prov = webElement.FindElement(By.CssSelector("input[name='Prov']")).GetAttribute("value");
            //                            string ClimateSiteName = webElement.FindElements(By.CssSelector("div")).First().GetAttribute("innerHTML");

            //                            lblStatus.Text = Prov + " --- " + ClimateSiteName;
            //                            lblStatus.Refresh();
            //                            Application.DoEvents();

            //                            string hStartDate = "";
            //                            string hEndDate = "";
            //                            if (hlyStartDate != null)
            //                            {
            //                                hStartDate = ((DateTime)hlyStartDate).ToString("yyyy MM dd");
            //                                hEndDate = ((DateTime)hlyEndDate).ToString("yyyy MM dd");
            //                            }
            //                            string dStartDate = "";
            //                            string dEndDate = "";
            //                            if (dlyStartDate != null)
            //                            {
            //                                dStartDate = ((DateTime)dlyStartDate).ToString("yyyy MM dd");
            //                                dEndDate = ((DateTime)dlyEndDate).ToString("yyyy MM dd");
            //                            }

            //                            string mStartDate = "";
            //                            string mEndDate = "";
            //                            if (mlyStartDate != null)
            //                            {
            //                                mStartDate = ((DateTime)mlyStartDate).ToString("yyyy MM dd");
            //                                mEndDate = ((DateTime)mlyEndDate).ToString("yyyy MM dd");
            //                            }

            //                            using (CSSPDBEntities db2 = new CSSPDBEntities())
            //                            {
            //                                ClimateSite climateSite = (from c in db2.ClimateSites
            //                                                           where c.ECDBID == ECDBID
            //                                                           select c).FirstOrDefault();

            //                                if (climateSite == null)
            //                                {
            //                                    sb.AppendLine($"{ECDBID}\t{Prov}\t{ClimateSiteName} does not exist in CSSPDB");

            //                                    string url = "";
            //                                    string year = "";
            //                                    string month = "";
            //                                    string day = "";
            //                                    if (dlyStartDate != null)
            //                                    {
            //                                        year = ((DateTime)dlyStartDate).Year.ToString();
            //                                        month = ((DateTime)dlyStartDate).Month.ToString();
            //                                        day = ((DateTime)dlyStartDate).Day.ToString();

            //                                        url = "https://climate.weather.gc.ca/climate_data/daily_data_e.html?" +
            //                                            "StationID=" + ECDBID.ToString() + "&Prov=" + prov +
            //                                            "&Month=" + ((DateTime)dlyStartDate).Month + "&Day=" + ((DateTime)dlyStartDate).Day +
            //                                            "&lstProvince=" + prov + "&timeframe=2&Year=" + ((DateTime)dlyStartDate).Year + "";
            //                                    }
            //                                    else if (hlyStartDate != null)
            //                                    {
            //                                        year = ((DateTime)hlyStartDate).Year.ToString();
            //                                        month = ((DateTime)hlyStartDate).Month.ToString();
            //                                        day = ((DateTime)hlyStartDate).Day.ToString();

            //                                        url = "https://climate.weather.gc.ca/climate_data/daily_data_e.html?" +
            //                                            "StationID=" + ECDBID.ToString() + "&Prov=" + prov +
            //                                            "&Month=" + ((DateTime)hlyStartDate).Month + "&Day=" + ((DateTime)hlyStartDate).Day +
            //                                            "&lstProvince=" + prov + "&timeframe=2&Year=" + ((DateTime)hlyStartDate).Year + "";
            //                                    }
            //                                    else if (mlyStartDate != null)
            //                                    {
            //                                        year = ((DateTime)mlyStartDate).Year.ToString();
            //                                        month = ((DateTime)mlyStartDate).Month.ToString();
            //                                        day = ((DateTime)mlyStartDate).Day.ToString();

            //                                        url = "https://climate.weather.gc.ca/climate_data/daily_data_e.html?" +
            //                                            "StationID=" + ECDBID.ToString() + "&Prov=" + prov +
            //                                            "&Month=" + ((DateTime)mlyStartDate).Month + "&Day=" + ((DateTime)mlyStartDate).Day +
            //                                            "&lstProvince=" + prov + "&timeframe=2&Year=" + ((DateTime)mlyStartDate).Year + "";
            //                                    }
            //                                    else
            //                                    {
            //                                    }

            //                                    if (url != "")
            //                                    {
            //                                        //HttpClient httpClient = new HttpClient();

            //                                        //Task<string> taskRes = httpClient.GetStringAsync(url);

            //                                        //string res = taskRes.Result.ToString();

            //                                        //if (!res.Contains($@"id=""climateid"""))
            //                                        //{
            //                                        //    sb.AppendLine($"ERROR: Could not find id of climateid within document {url}\r\n");
            //                                        //    return;
            //                                        //}

            //                                        //res = res.Replace("<link", "<NOlink");
            //                                        //res = res.Replace("<script", "<NOscript");

            //                                        //FileInfo fi2 = new FileInfo($@"C:\CSSP Latest Code Old\DataTool\ImportByFunction\Assets\ClimateMetaData\ClimateSites\climatesite_{ECDBID.ToString()}_{year}_{month}_{day}.html");

            //                                        //StreamWriter sw = fi2.CreateText();
            //                                        //sw.WriteLine(res);
            //                                        //sw.Close();
            //                                    }

            //                                    //sb.AppendLine($"\t{hStartDate}|{hEndDate}\t{dStartDate}|{dEndDate}\t{mStartDate}|{mEndDate}\t{ECDBID}\t{Prov}\t{ClimateSiteName}");
            //                                }

            //                                if (climateSite != null)
            //                                {
            //                                    bool DateChanged = false;
            //                                    if (!(climateSite.DailyStartDate_Local == dlyStartDate && climateSite.DailyEndDate_Local == dlyEndDate))
            //                                    {
            //                                        if (dlyEndDate != null && climateSite.DailyEndDate_Local != ((DateTime)dlyEndDate).AddYears(10))
            //                                        {
            //                                            sb.AppendLine($"{climateSite.ClimateSiteName} ({climateSite.ClimateID}) dlyStartDate not equal {climateSite.DailyStartDate_Local} {dlyStartDate}");
            //                                            DateChanged = true;
            //                                        }
            //                                    }
            //                                    if (!(climateSite.HourlyStartDate_Local == hlyStartDate && climateSite.HourlyEndDate_Local == hlyEndDate))
            //                                    {
            //                                        if (hlyEndDate != null && climateSite.HourlyEndDate_Local != ((DateTime)hlyEndDate).AddYears(10))
            //                                        {
            //                                            sb.AppendLine($"{climateSite.ClimateSiteName} ({climateSite.ClimateID}) hlyStartDate not equal {climateSite.HourlyStartDate_Local} {hlyStartDate}");
            //                                            DateChanged = true;
            //                                        }
            //                                    }
            //                                    if (!(climateSite.MonthlyStartDate_Local == mlyStartDate && climateSite.MonthlyEndDate_Local == mlyEndDate))
            //                                    {
            //                                        if (mlyEndDate != null && climateSite.MonthlyEndDate_Local != ((DateTime)mlyEndDate).AddYears(10))
            //                                        {
            //                                            sb.AppendLine($"{climateSite.ClimateSiteName} ({climateSite.ClimateID}) mlyStartDate not equal {climateSite.MonthlyStartDate_Local} {mlyStartDate}");
            //                                            DateChanged = true;
            //                                        }
            //                                    }

            //                                    if (DateChanged)
            //                                    {
            //                                        climateSite.HourlyStartDate_Local = hlyStartDate;
            //                                        if (hlyEndDate != null && ((DateTime)hlyEndDate).Year == 2019)
            //                                        {
            //                                            climateSite.HourlyEndDate_Local = ((DateTime)hlyEndDate).AddYears(10);
            //                                            climateSite.HourlyNow = true;
            //                                        }
            //                                        else
            //                                        {
            //                                            climateSite.HourlyEndDate_Local = hlyEndDate;
            //                                            climateSite.HourlyNow = null;
            //                                        }
            //                                        climateSite.DailyStartDate_Local = dlyStartDate;
            //                                        if (dlyEndDate != null && ((DateTime)dlyEndDate).Year == 2019)
            //                                        {
            //                                            climateSite.DailyEndDate_Local = ((DateTime)dlyEndDate).AddYears(10);
            //                                            climateSite.DailyNow = true;
            //                                        }
            //                                        else
            //                                        {
            //                                            climateSite.DailyEndDate_Local = dlyEndDate;
            //                                            climateSite.DailyNow = null;
            //                                        }
            //                                        climateSite.MonthlyStartDate_Local = mlyStartDate;
            //                                        if (mlyEndDate != null && ((DateTime)mlyEndDate).Year == 2019)
            //                                        {
            //                                            climateSite.MonthlyEndDate_Local = ((DateTime)mlyEndDate).AddYears(10);
            //                                            climateSite.MonthlyNow = true;
            //                                        }
            //                                        else
            //                                        {
            //                                            climateSite.MonthlyEndDate_Local = mlyEndDate;
            //                                            climateSite.MonthlyNow = null;
            //                                        }

            //                                        try
            //                                        {
            //                                            db2.SaveChanges();
            //                                        }
            //                                        catch (Exception ex)
            //                                        {
            //                                            sb.AppendLine($"\t{climateSite.ClimateSiteName} ({climateSite.ClimateID}) could not change {ex.Message}");
            //                                        }
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //            // break; // just doing one document
            //        }
            //        //break; // just doing one province
            //    }

            //}

            //richTextBoxStatus.Text = sb.ToString();
            #endregion Parsing all assets files C:\CSSP Latest Code Old\DataTool\ImportByFunction\Assets\ClimateMetaData

            #region Parsing ClimateSites.csv and adding missing sites to TVItems and ClimateSites tables
            //// Name,Province,ClimateID,WMOID,TCID,Lat,Lng,Elev,hlyStartYear,hlyEndYear,dlyStartYear,dlyEndYear,mlyStartYear,mlyEndYear
            //// ACTIVE PASS,BC,1010066,,,48.87,-123.28,4,,,1984,1996,1984,1996

            //List<string> ProvList = new List<string>()
            //{
            //    "British Columbia", "Québec", "New Brunswick", "Newfoundland and Labrador", "Nova Scotia", "Prince Edward Island"
            //};
            //List<string> ProvInit = new List<string>()
            //{
            //    "BC", "QC", "NB", "NL", "NS", "PE"
            //};
            //List<float> ProvTimeOffset = new List<float>()
            //{
            //    -8.0f, -5.0f, -4.0f, -3.5f, -4.0f, -4.0f
            //};
            //List<TVItemModel> tvItemModelProvList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.Province);

            //TVItemModel tvItemModelProv = new TVItemModel();
            //float TimeOffset = -8.0f;

            //FileInfo fi = new FileInfo(@"C:\CSSP Latest Code Old\DataTool\ImportByFunction\Assets\ClimateSites.csv");
            //if (!fi.Exists)
            //{
            //    richTextBoxStatus.AppendText($"ERROR: Could not find file [{fi.FullName}]\r\n");
            //    return;
            //}

            //using (StreamReader sr = fi.OpenText())
            //{
            //    string lineStr = sr.ReadLine(); // reading first line of csv file
            //    List<string> paramList = lineStr.Split(",".ToCharArray(), StringSplitOptions.None).ToList();
            //    if (paramList.Count != 14)
            //    {
            //        richTextBoxStatus.AppendText($"ERROR: paramList does not have 14 items\r\n");
            //        sr.Close();
            //        return;
            //    }
            //    List<string> itemList = new List<string>()
            //    {
            //        "Name","Province","ClimateID","WMOID","TCID","Lat","Lng","Elev","hlyStartYear","hlyEndYear","dlyStartYear","dlyEndYear","mlyStartYear","mlyEndYear"
            //    };

            //    for (int i = 0; i < paramList.Count; i++)
            //    {
            //        if (paramList[i] != itemList[i])
            //        {
            //            richTextBoxStatus.AppendText($"ERROR: paramList[{i}] = {paramList[i]} is not equal to itemList[{i}] = {itemList[i]}\r\n");
            //            sr.Close();
            //            return;
            //        }
            //    }
            //    bool ok = true;
            //    int count = 0;
            //    while (ok)
            //    {
            //        lineStr = sr.ReadLine();
            //        if (string.IsNullOrEmpty(lineStr))
            //        {
            //            break;
            //        }

            //        List<string> valueList = lineStr.Split(",".ToCharArray(), StringSplitOptions.None).ToList();
            //        if (valueList.Count != 14)
            //        {
            //            richTextBoxStatus.AppendText($"ERROR: valueList does not have 14 items\r\n");
            //            richTextBoxStatus.AppendText($"{lineStr}\r\n");
            //            sr.Close();
            //            return;
            //        }

            //        string ClimateSiteName = valueList[0];

            //        count += 1;

            //        if (count % 20 == 0)
            //        {
            //            lblStatus.Text = count.ToString() + " " +ClimateSiteName;
            //            lblStatus.Refresh();
            //            Application.DoEvents();
            //        }

            //        string Province = valueList[1];
            //        string ClimateID = valueList[2];
            //        int? WMOID = null;
            //        if (!string.IsNullOrEmpty(valueList[3].Trim()))
            //        {
            //            if (int.TryParse(valueList[3], out int tempWMOID))
            //            {
            //                WMOID = tempWMOID;
            //            };
            //        }
            //        string TCID = valueList[4];
            //        if (!float.TryParse(valueList[5], out float Lat))
            //        {
            //            richTextBoxStatus.AppendText($"ERROR: valueList could not parse Lat [{valueList[5]}]\r\n");
            //            sr.Close();
            //            return;
            //        }
            //        if (!float.TryParse(valueList[6], out float Lng))
            //        {
            //            richTextBoxStatus.AppendText($"ERROR: valueList could not parse Lng [{valueList[6]}]\r\n");
            //            sr.Close();
            //            return;
            //        }

            //        float? Elevation_m = null;
            //        if (!string.IsNullOrEmpty(valueList[7]))
            //        {
            //            if (!float.TryParse(valueList[7], out float tempElevation_m))
            //            {
            //                richTextBoxStatus.AppendText($"ERROR: valueList could not parse Elev [{valueList[7]}]\r\n");
            //                sr.Close();
            //                return;
            //            }
            //            Elevation_m = tempElevation_m;
            //        }

            //        // hourly start and end year
            //        int? hylStartYear = null;
            //        if (!string.IsNullOrEmpty(valueList[8]))
            //        {
            //            if (!int.TryParse(valueList[8], out int temphylStartYear))
            //            {
            //                richTextBoxStatus.AppendText($"ERROR: valueList could not parse Elev [{valueList[8]}]\r\n");
            //                sr.Close();
            //                return;
            //            }
            //            hylStartYear = temphylStartYear;
            //        }

            //        int? hylEndYear = null;
            //        if (!string.IsNullOrEmpty(valueList[9]))
            //        {
            //            if (!int.TryParse(valueList[9], out int temphylEndYear))
            //            {
            //                richTextBoxStatus.AppendText($"ERROR: valueList could not parse Elev [{valueList[9]}]\r\n");
            //                sr.Close();
            //                return;
            //            }
            //            hylEndYear = temphylEndYear;
            //        }


            //        // hourly start and end year
            //        int? dylStartYear = null;
            //        if (!string.IsNullOrEmpty(valueList[10]))
            //        {
            //            if (!int.TryParse(valueList[10], out int tempdylStartYear))
            //            {
            //                richTextBoxStatus.AppendText($"ERROR: valueList could not parse Elev [{valueList[10]}]\r\n");
            //                sr.Close();
            //                return;
            //            }
            //            dylStartYear = tempdylStartYear;
            //        }

            //        int? dylEndYear = null;
            //        if (!string.IsNullOrEmpty(valueList[11]))
            //        {
            //            if (!int.TryParse(valueList[11], out int tempdylEndYear))
            //            {
            //                richTextBoxStatus.AppendText($"ERROR: valueList could not parse Elev [{valueList[11]}]\r\n");
            //                sr.Close();
            //                return;
            //            }
            //            dylEndYear = tempdylEndYear;
            //        }

            //        // hourly start and end year
            //        int? mylStartYear = null;
            //        if (!string.IsNullOrEmpty(valueList[12]))
            //        {
            //            if (!int.TryParse(valueList[12], out int tempmylStartYear))
            //            {
            //                richTextBoxStatus.AppendText($"ERROR: valueList could not parse Elev [{valueList[12]}]\r\n");
            //                sr.Close();
            //                return;
            //            }
            //            mylStartYear = tempmylStartYear;
            //        }

            //        int? mylEndYear = null;
            //        if (!string.IsNullOrEmpty(valueList[13]))
            //        {
            //            if (!int.TryParse(valueList[13], out int tempmylEndYear))
            //            {
            //                richTextBoxStatus.AppendText($"ERROR: valueList could not parse Elev [{valueList[13]}]\r\n");
            //                sr.Close();
            //                return;
            //            }
            //            mylEndYear = tempmylEndYear;
            //        }

            //        #region Add ClimateSite if does not exist
            //        //ClimateSite climateSiteExist = (from c in climateSiteList
            //        //                                where c.ClimateID == ClimateID
            //        //                                select c).FirstOrDefault();

            //        //if (climateSiteExist == null)
            //        //{
            //        //    richTextBoxStatus.AppendText($"{ClimateSiteName} with ClimateID [{ClimateID}] does not exist\r\n");

            //        //    for (int i = 0; i < ProvInit.Count; i++)
            //        //    {
            //        //        if (Province == ProvInit[i])
            //        //        {
            //        //            tvItemModelProv = (from c in tvItemModelProvList
            //        //                               where c.TVText == ProvList[i]
            //        //                               select c).FirstOrDefault();

            //        //            if (tvItemModelProv == null)
            //        //            {
            //        //                richTextBoxStatus.AppendText($"ERROR: Could not find TVItemModel for province {Province}\r\n");
            //        //                sr.Close();
            //        //                return;
            //        //            }

            //        //            TimeOffset = ProvTimeOffset[i];
            //        //            break;
            //        //        }
            //        //    }

            //        //    string TVText = (ClimateSiteName + " (" + ClimateID + ")").Replace(",", "_");

            //        //    TVItemModel tvItemModelClimateSite = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelProv.TVItemID, TVText, TVTypeEnum.ClimateSite);
            //        //    if (!string.IsNullOrWhiteSpace(tvItemModelClimateSite.Error))
            //        //    {
            //        //        tvItemModelClimateSite = tvItemService.PostAddChildTVItemDB(tvItemModelProv.TVItemID, TVText, TVTypeEnum.ClimateSite);
            //        //        if (!string.IsNullOrWhiteSpace(tvItemModelClimateSite.Error))
            //        //        {
            //        //            richTextBoxStatus.AppendText($"ERROR: {tvItemModelClimateSite.Error}\r\n");
            //        //            sr.Close();
            //        //            return;
            //        //        }
            //        //    }

            //        //    ClimateSiteModel climateSiteModelNew = new ClimateSiteModel()
            //        //    {
            //        //        ClimateSiteTVItemID = tvItemModelClimateSite.TVItemID,
            //        //        ECDBID = null,
            //        //        ClimateSiteName = ClimateSiteName,
            //        //        Province = Province,
            //        //        Elevation_m = Elevation_m,
            //        //        ClimateID = ClimateID,
            //        //        WMOID = WMOID,
            //        //        TCID = TCID,
            //        //        IsQuebecSite = null,
            //        //        IsCoCoRaHS = null,
            //        //        TimeOffset_hour = TimeOffset,
            //        //        HourlyStartDate_Local = null,
            //        //        HourlyEndDate_Local = null,
            //        //        HourlyNow = null,
            //        //        DailyStartDate_Local = null,
            //        //        DailyEndDate_Local = null,
            //        //        DailyNow = null,
            //        //        MonthlyStartDate_Local = null,
            //        //        MonthlyEndDate_Local = null,
            //        //        MonthlyNow = null
            //        //    };

            //        //    ClimateSiteModel climateSiteModelExist = climateSiteService.GetClimateSiteModelExistDB(climateSiteModelNew);
            //        //    if (!string.IsNullOrWhiteSpace(climateSiteModelExist.Error))
            //        //    {
            //        //        ClimateSiteModel climateSiteModelRet = climateSiteService.PostAddClimateSiteDB(climateSiteModelNew);
            //        //        if (!string.IsNullOrWhiteSpace(climateSiteModelRet.Error))
            //        //        {
            //        //            richTextBoxStatus.AppendText($"ERROR: {climateSiteModelRet.Error}\r\n");
            //        //            sr.Close();
            //        //            return;
            //        //        }
            //        //    }


            //        //    //sr.Close();
            //        //    //return;
            //        //}

            //        #endregion Add ClimateSite if does not exist

            //        #region Verify all info related to ClimateSite
            //        //ClimateSite climateSiteExist = (from c in climateSiteList
            //        //                                where c.ClimateID == ClimateID
            //        //                                select c).FirstOrDefault();

            //        //if (climateSiteExist == null)
            //        //{
            //        //    richTextBoxStatus.AppendText($"{ClimateSiteName} with ClimateID [{ClimateID}] does not exist\r\n");
            //        //    return;
            //        //}
            //        //else
            //        //{
            //        //    for (int i = 0; i < ProvInit.Count; i++)
            //        //    {
            //        //        if (Province == ProvInit[i])
            //        //        {
            //        //            tvItemModelProv = (from c in tvItemModelProvList
            //        //                               where c.TVText == ProvList[i]
            //        //                               select c).FirstOrDefault();

            //        //            if (tvItemModelProv == null)
            //        //            {
            //        //                richTextBoxStatus.AppendText($"ERROR: Could not find TVItemModel for province {Province}\r\n");
            //        //                sr.Close();
            //        //                return;
            //        //            }

            //        //            TimeOffset = ProvTimeOffset[i];
            //        //            break;
            //        //        }
            //        //    }

            //        //    string TVText = (ClimateSiteName + " (" + ClimateID + ")").Replace(",", "_");

            //        //    TVItemModel tvItemModelClimateSite = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelProv.TVItemID, TVText, TVTypeEnum.ClimateSite);
            //        //    if (!string.IsNullOrWhiteSpace(tvItemModelClimateSite.Error))
            //        //    {
            //        //        richTextBoxStatus.AppendText($"ERROR: {tvItemModelClimateSite.Error}\r\n");
            //        //        continue;
            //        //    }

            //        //    if (climateSiteExist.ClimateSiteTVItemID != tvItemModelClimateSite.TVItemID)
            //        //    {
            //        //        richTextBoxStatus.AppendText($"{TVText}\tClimateSiteTVItemID: current {climateSiteExist.ClimateSiteTVItemID} new {tvItemModelClimateSite.TVItemID}\r\n");
            //        //    }
            //        //    if (climateSiteExist.ClimateSiteName != ClimateSiteName)
            //        //    {
            //        //        richTextBoxStatus.AppendText($"{TVText}\tClimateSiteName: current {climateSiteExist.ClimateSiteName} new {ClimateSiteName}\r\n");
            //        //    }
            //        //    if (climateSiteExist.Province != Province)
            //        //    {
            //        //        richTextBoxStatus.AppendText($"{TVText}\tProvince: current {climateSiteExist.Province} new {Province}\r\n");
            //        //    }
            //        //    if (Elevation_m != null && climateSiteExist.Elevation_m != null)
            //        //    {
            //        //        if (Math.Abs(((float)climateSiteExist.Elevation_m) - (float)Elevation_m) > 0.1f)
            //        //        {
            //        //            richTextBoxStatus.AppendText($"{TVText}\tElevation_m: current {climateSiteExist.Elevation_m} new {Elevation_m}\r\n");
            //        //        }
            //        //    }
            //        //    if (WMOID != null && climateSiteExist.WMOID != null)
            //        //    {
            //        //        if (Math.Abs(((int)climateSiteExist.WMOID) - (int)WMOID) > 0.1f)
            //        //        {
            //        //            richTextBoxStatus.AppendText($"{TVText}\tWMOID: current {climateSiteExist.WMOID} new {WMOID}\r\n");
            //        //        }
            //        //    }
            //        //    if (TCID != null && climateSiteExist.TCID != null)
            //        //    {
            //        //        if (climateSiteExist.TCID != TCID)
            //        //        {
            //        //            richTextBoxStatus.AppendText($"{TVText}\tTCID: current {climateSiteExist.TCID} new {TCID}\r\n");
            //        //        }
            //        //    }

            //        //    List<MapInfoModel> mapInfoModelList = mapInfoService.GetMapInfoModelListWithTVItemIDDB(tvItemModelClimateSite.TVItemID);

            //        //    MapInfoModel mapInfoModelClimateSite = new MapInfoModel();
            //        //    foreach (MapInfoModel mapInfoModel in mapInfoModelList)
            //        //    {
            //        //        if (mapInfoModel.TVType == TVTypeEnum.ClimateSite)
            //        //        {
            //        //            if (mapInfoModel.MapInfoDrawType == MapInfoDrawTypeEnum.Point)
            //        //            {
            //        //                mapInfoModelClimateSite = mapInfoModel;
            //        //                break;
            //        //            }
            //        //        }
            //        //    }

            //        //    List<MapInfoPointModel> mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelClimateSite.TVItemID, TVTypeEnum.ClimateSite, MapInfoDrawTypeEnum.Point);

            //        //    if (mapInfoPointModelList.Count == 0)
            //        //    {
            //        //        richTextBoxStatus.AppendText($"{TVText}\tMapInfo not found for TVItemID {tvItemModelClimateSite.TVItemID}\r\n");

            //        //        List<Coord> coordList = new List<Coord>()
            //        //        {
            //        //            new Coord() { Lat = Lat, Lng = Lng, Ordinal = 0 }
            //        //        };

            //        //        MapInfoModel mapInfoModelRet = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.ClimateSite, tvItemModelClimateSite.TVItemID);
            //        //        if (!string.IsNullOrWhiteSpace(mapInfoModelRet.Error))
            //        //        {
            //        //            richTextBoxStatus.AppendText($"ERROR: Cound not create MapInfo for {TVText}\r\n");
            //        //            continue;
            //        //        }
            //        //    }
            //        //    else
            //        //    {
            //        //        if (Math.Abs(Lat - mapInfoPointModelList[0].Lat) > 0.1f || Math.Abs(Lng - mapInfoPointModelList[0].Lng) > 0.1f)
            //        //        {
            //        //            richTextBoxStatus.AppendText($"{TVText}\tMapInfoPoint: current {mapInfoPointModelList[0].Lat} new {Lng}  current {mapInfoPointModelList[0].Lat} new {Lng}\r\n");

            //        //            mapInfoPointModelList[0].Lat = Lat;
            //        //            mapInfoPointModelList[0].Lng = Lng;

            //        //            MapInfoPointModel mapInfoPointModelRet = mapInfoService._MapInfoPointService.PostUpdateMapInfoPointDB(mapInfoPointModelList[0]);
            //        //            if (!string.IsNullOrWhiteSpace(mapInfoPointModelRet.Error))
            //        //            {
            //        //                richTextBoxStatus.AppendText($"ERROR: Cound not change MapInfoPoint for {TVText}\r\n");
            //        //                continue;
            //        //            }
            //        //        }
            //        //    }

            //        //    //sr.Close();
            //        //    //return;
            //        //}
            //        #endregion Verify all info related to ClimateSite

            //        if (sr.EndOfStream)
            //        {
            //            ok = false;
            //        }
            //    }
            //    sr.Close();
            //}

            #endregion Parsing ClimateSites.csv and adding missing sites to TVItems and ClimateSites tables

            #region parsing file under the directory C:\CSSP Latest Code Old\DataTool\ImportByFunction\Assets\ClimateMetaData\ClimateSites
            //DirectoryInfo di = new DirectoryInfo(@"C:\CSSP Latest Code Old\DataTool\ImportByFunction\Assets\ClimateMetaData\ClimateSites\");
            //List<FileInfo> fiList = di.GetFiles().Where(c => c.Extension == ".html").ToList();
            //string searchText = @"aria-labelledby=""climateid"">";
            //foreach (FileInfo fi in fiList)
            //{
            //    lblStatus.Text = fi.Name;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    StreamReader sr = fi.OpenText();
            //    string fileText = sr.ReadToEnd();
            //    sr.Close();

            //    string ECDBIDText = fi.Name;
            //    ECDBIDText = ECDBIDText.Replace("climatesite_", "");
            //    ECDBIDText = ECDBIDText.Substring(0, ECDBIDText.IndexOf("_"));

            //    if (!int.TryParse(ECDBIDText, out int ECDBID))
            //    {
            //        richTextBoxStatus.AppendText($"ERROR: could not parse ECDBIDText [{ECDBIDText}]");
            //    }

            //    int pos = fileText.IndexOf(searchText) + searchText.Length;
            //    string ClimateID = fileText.Substring(pos, 20).Trim();
            //    if (ClimateID.Contains("<"))
            //    {
            //        ClimateID = ClimateID.Substring(0, ClimateID.IndexOf("<"));
            //    }

            //    richTextBoxStatus.AppendText($"{ECDBID} --- {ClimateID}\r\n");

            //    using(CSSPDBEntities db2 = new CSSPDBEntities())
            //    {
            //        ClimateSite climateSite = (from c in db2.ClimateSites
            //                                   where c.ClimateID == ClimateID
            //                                   select c).FirstOrDefault();

            //        if (climateSite == null)
            //        {
            //            richTextBoxStatus.AppendText($"\tCould not find in CSSPDB\r\n");
            //        }
            //        else
            //        {
            //            climateSite.ECDBID = ECDBID;
            //            try
            //            {
            //                db2.SaveChanges();
            //            }
            //            catch (Exception ex)
            //            {
            //                richTextBoxStatus.AppendText($"ERROR: {ex.Message}\r\n");
            //            }
            //        }
            //    }
            //}
            #endregion  parsing file under the directory C:\CSSP Latest Code Old\DataTool\ImportByFunction\Assets\ClimateMetaData\ClimateSites
            lblStatus.Text = "done...";
        }

        private string SetupDate(string DateString, ref DateTime? StartDate, ref DateTime? EndDate)
        {
            if (DateString.StartsWith("|"))
            {
                StartDate = null;
                EndDate = null;
            }
            else
            { //Example: "1962-01-01|1962-11-30"
                string errMessage = $"ERROR: Could not parse hlyRange {DateString}";

                if (!int.TryParse(DateString.Substring(0, 4), out int yearStart))
                {
                    return errMessage;
                }
                if (!int.TryParse(DateString.Substring(5, 2), out int monthStart))
                {
                    return errMessage;
                }
                if (!int.TryParse(DateString.Substring(8, 2), out int dayStart))
                {
                    return errMessage;
                }
                if (!int.TryParse(DateString.Substring(11, 4), out int yearEnd))
                {
                    return errMessage;
                }
                if (!int.TryParse(DateString.Substring(16, 2), out int monthEnd))
                {
                    return errMessage;
                }
                if (!int.TryParse(DateString.Substring(19, 2), out int dayEnd))
                {
                    return errMessage;
                }
                StartDate = new DateTime(yearStart, monthStart, dayStart);
                EndDate = new DateTime(yearEnd, monthEnd, dayEnd);
            }

            return "";
        }

        private void butSetClassForMWQMSites_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrWhiteSpace(tvItemModelRoot.Error))
            {
                richTextBoxStatus.AppendText($"{tvItemModelRoot.Error}");
                return;
            }

            List<MWQMSite> mwqmSiteList = new List<MWQMSite>();
            List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelRoot.TVItemID, TVTypeEnum.MWQMSite);

            using (CSSPDBEntities db3 = new CSSPDBEntities())
            {
                mwqmSiteList = (from c in db3.MWQMSites
                                select c).ToList();

                FileInfo fi = new FileInfo(@"C:\CSSP Latest Code Old\DataTool\ImportByFunction\Assets\NationalClassification\NationalClassification2019.kml");

                if (!fi.Exists)
                {
                    richTextBoxStatus.AppendText($"could not find [{fi.FullName}]");
                    return;
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(fi.FullName);

                XmlNodeList xmlNodePlacmarkList = doc.GetElementsByTagName("Placemark");
                int cc = 0;
                foreach (XmlNode n1 in xmlNodePlacmarkList)
                {
                    cc++;

                    if (cc > 1) //50880)
                    {
                        break;
                    }

                    sb.AppendLine($"{n1.Name} -- {n1.Attributes["id"].Value}");

                    lblStatus.Text = n1.Attributes["id"].Value;
                    lblStatus.Refresh();
                    Application.DoEvents();

                    string classEN = "";
                    List<Coord> coordList = new List<Coord>();
                    foreach (XmlNode n2 in n1.ChildNodes)
                    {
                        if (n2.Name == "description")
                        {
                            string descText = n2.InnerText;
                            int posStart = descText.IndexOf("<td>class_en</td>") + "<td>class_en</td>".Length;
                            int posEnd = descText.IndexOf("</tr>", posStart);
                            descText = descText.Substring(posStart, posEnd - posStart);
                            descText = descText.Replace("<td>", "");
                            descText = descText.Replace("</td>", "");
                            descText = descText.Trim();

                            classEN = descText;
                        }

                        if (n2.Name == "MultiGeometry")
                        {
                            foreach (XmlNode n3 in n2.ChildNodes)
                            {
                                if (n3.Name == "Polygon")
                                {
                                    foreach (XmlNode n4 in n3.ChildNodes)
                                    {
                                        if (n4.Name == "outerBoundaryIs")
                                        {
                                            foreach (XmlNode n5 in n4.ChildNodes)
                                            {
                                                if (n5.Name == "LinearRing")
                                                {
                                                    foreach (XmlNode n6 in n5.ChildNodes)
                                                    {
                                                        if (n6.Name == "coordinates")
                                                        {
                                                            string coordText = n6.InnerText.Trim();
                                                            List<string> pointTextList = coordText.Split(" ".ToCharArray(), StringSplitOptions.None).ToList();
                                                            int count = 0;
                                                            foreach (string s in pointTextList)
                                                            {
                                                                List<string> pointText = s.Split(",".ToCharArray(), StringSplitOptions.None).ToList();

                                                                if (pointText.Count != 3)
                                                                {
                                                                    richTextBoxStatus.AppendText($"{pointText.Count} should be 3");
                                                                    return;
                                                                }

                                                                float Lng = float.Parse(pointText[0]);
                                                                float Lat = float.Parse(pointText[1]);

                                                                coordList.Add(new Coord() { Lat = Lat, Lng = Lng, Ordinal = count });
                                                                count++;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            float MinLat = (from c in coordList
                                            select c.Lat).Min();
                            float MaxLat = (from c in coordList
                                            select c.Lat).Max();

                            float MinLng = (from c in coordList
                                            select c.Lng).Min();
                            float MaxLng = (from c in coordList
                                            select c.Lng).Max();

                            using (CSSPDBEntities db2 = new CSSPDBEntities())
                            {
                                var mapInfoMWQMSiteList = (from mi in db2.MapInfos
                                                           from mip in db2.MapInfoPoints
                                                           where mi.MapInfoID == mip.MapInfoID
                                                           && mi.LatMin >= MinLat
                                                           && mi.LatMax <= MaxLat
                                                           && mi.LngMin >= MinLng
                                                           && mi.LngMax <= MaxLng
                                                           && mi.TVType == (int)TVTypeEnum.MWQMSite
                                                           && mi.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
                                                           select new { mi, mip }).ToList();

                                foreach (var miMWQMSite in mapInfoMWQMSiteList)
                                {
                                    if (miMWQMSite.mip != null)
                                    {
                                        Coord coord = new Coord() { Lat = ((float)miMWQMSite.mip.Lat), Lng = ((float)miMWQMSite.mip.Lng), Ordinal = 0 };
                                        if (mapInfoService.CoordInPolygon(coordList, coord))
                                        {
                                            MWQMSite mwqmSite = (from c in mwqmSiteList
                                                                 where c.MWQMSiteTVItemID == miMWQMSite.mi.TVItemID
                                                                 select c).FirstOrDefault();

                                            TVItemModel tvItemModelMWQMSite = (from c in tvItemModelMWQMSiteList
                                                                               where c.TVItemID == miMWQMSite.mi.TVItemID
                                                                               select c).FirstOrDefault();

                                            if (mwqmSite != null && tvItemModelMWQMSite != null)
                                            {
                                                MWQMSiteLatestClassificationEnum latestClass = MWQMSiteLatestClassificationEnum.Error;
                                                switch (classEN)
                                                {
                                                    case "Approved":
                                                        {
                                                            latestClass = MWQMSiteLatestClassificationEnum.Approved;
                                                        }
                                                        break;
                                                    case "Prohibited":
                                                        {
                                                            latestClass = MWQMSiteLatestClassificationEnum.Prohibited;
                                                        }
                                                        break;
                                                    case "Conditionally Approved":
                                                        {
                                                            latestClass = MWQMSiteLatestClassificationEnum.ConditionallyApproved;
                                                        }
                                                        break;
                                                    case "Conditionally Restricted":
                                                        {
                                                            latestClass = MWQMSiteLatestClassificationEnum.ConditionallyRestricted;
                                                        }
                                                        break;
                                                    case "Restricted":
                                                        {
                                                            latestClass = MWQMSiteLatestClassificationEnum.Restricted;
                                                        }
                                                        break;
                                                    case "Unclassified":
                                                        {
                                                            latestClass = MWQMSiteLatestClassificationEnum.Unclassified;
                                                        }
                                                        break;
                                                    default:
                                                        {
                                                            richTextBoxStatus.AppendText($"{classEN} not recognized");
                                                            return;
                                                        }
                                                }

                                                mwqmSite.MWQMSiteLatestClassification = (int)latestClass;

                                                sb.AppendLine($"\t{miMWQMSite.mi.TVItemID} --- {mwqmSite.MWQMSiteTVItemID} --- {tvItemModelMWQMSite.TVText} --- {((MWQMSiteLatestClassificationEnum)mwqmSite.MWQMSiteLatestClassification).ToString()} --- {latestClass.ToString()}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                try
                {
                    db3.SaveChanges();
                }
                catch (Exception ex)
                {
                    richTextBoxStatus.AppendText($"{ex.Message}\r\n");
                    return;
                }

                lblStatus.Text = "done...";

                richTextBoxStatus.AppendText(sb.ToString());
            }

        }

        private void button21_Click(object sender, EventArgs e)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "British Columbia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelProv.TVText + "\r\n");
                return;
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine(@"<Document id=""Shellfish_Harvest_Area_Classification_in_British_Columbia___Classification_des_zones_de_récolte_des_mollusques_en_Colombie-Britannique"">");
            sb.AppendLine(@"	<name>ClassificationPolygons_BC.kml</name>");
            sb.AppendLine(@"	<visibility>0</visibility>");
            sb.AppendLine(@"	<open>1</open>");
            sb.AppendLine(@"	<Snippet maxLines=""0""></Snippet>");
            sb.AppendLine(@"	<Style id=""PolyStyle053"">");
            sb.AppendLine(@"		<LabelStyle>");
            sb.AppendLine(@"			<color>00000000</color>");
            sb.AppendLine(@"			<scale>0</scale>");
            sb.AppendLine(@"		</LabelStyle>");
            sb.AppendLine(@"		<LineStyle>");
            sb.AppendLine(@"			<color>ff242424</color>");
            sb.AppendLine(@"			<width>0.4</width>");
            sb.AppendLine(@"		</LineStyle>");
            sb.AppendLine(@"		<PolyStyle>");
            sb.AppendLine(@"			<color>ff4f4f4f</color>");
            sb.AppendLine(@"		</PolyStyle>");
            sb.AppendLine(@"	</Style>");
            sb.AppendLine(@"	<Style id=""PolyStyle050"">");
            sb.AppendLine(@"		<LabelStyle>");
            sb.AppendLine(@"			<color>00000000</color>");
            sb.AppendLine(@"			<scale>0</scale>");
            sb.AppendLine(@"		</LabelStyle>");
            sb.AppendLine(@"		<LineStyle>");
            sb.AppendLine(@"			<color>ff395f73</color>");
            sb.AppendLine(@"			<width>0.4</width>");
            sb.AppendLine(@"		</LineStyle>");
            sb.AppendLine(@"		<PolyStyle>");
            sb.AppendLine(@"			<color>ff7fd3ff</color>");
            sb.AppendLine(@"		</PolyStyle>");
            sb.AppendLine(@"	</Style>");
            sb.AppendLine(@"	<Style id=""PolyStyle052"">");
            sb.AppendLine(@"		<LabelStyle>");
            sb.AppendLine(@"			<color>00000000</color>");
            sb.AppendLine(@"			<scale>0</scale>");
            sb.AppendLine(@"		</LabelStyle>");
            sb.AppendLine(@"		<LineStyle>");
            sb.AppendLine(@"			<color>ff393973</color>");
            sb.AppendLine(@"			<width>0.4</width>");
            sb.AppendLine(@"		</LineStyle>");
            sb.AppendLine(@"		<PolyStyle>");
            sb.AppendLine(@"			<color>ff7b7bf5</color>");
            sb.AppendLine(@"		</PolyStyle>");
            sb.AppendLine(@"	</Style>");
            sb.AppendLine(@"	<Style id=""PolyStyle00"">");
            sb.AppendLine(@"		<LabelStyle>");
            sb.AppendLine(@"			<color>00000000</color>");
            sb.AppendLine(@"			<scale>0</scale>");
            sb.AppendLine(@"		</LabelStyle>");
            sb.AppendLine(@"		<LineStyle>");
            sb.AppendLine(@"			<color>ff347373</color>");
            sb.AppendLine(@"			<width>0.4</width>");
            sb.AppendLine(@"		</LineStyle>");
            sb.AppendLine(@"		<PolyStyle>");
            sb.AppendLine(@"			<color>ff6ff5f5</color>");
            sb.AppendLine(@"		</PolyStyle>");
            sb.AppendLine(@"	</Style>");
            sb.AppendLine(@"	<Style id=""PolyStyle02"">");
            sb.AppendLine(@"		<LabelStyle>");
            sb.AppendLine(@"			<color>00000000</color>");
            sb.AppendLine(@"			<scale>0</scale>");
            sb.AppendLine(@"		</LabelStyle>");
            sb.AppendLine(@"		<LineStyle>");
            sb.AppendLine(@"			<color>ff347348</color>");
            sb.AppendLine(@"			<width>0.4</width>");
            sb.AppendLine(@"		</LineStyle>");
            sb.AppendLine(@"		<PolyStyle>");
            sb.AppendLine(@"			<color>ff74ffa3</color>");
            sb.AppendLine(@"		</PolyStyle>");
            sb.AppendLine(@"	</Style>");



            List<SubsectorPolClass> subsectorPolClassList = new List<SubsectorPolClass>();

            XmlDocument doc = new XmlDocument();
            doc.Load(@"C:\Users\leblancc\Desktop\ClassificationPolygons_BC.kml");
            foreach (XmlNode n1 in doc.DocumentElement.ChildNodes[0].ChildNodes)
            {
                if (n1.Name == "Folder")
                {
                    foreach (XmlNode n2 in n1.ChildNodes)
                    {
                        if (n2.Name == "Placemark")
                        {
                            SubsectorPolClass subsectorPolClass = new SubsectorPolClass();
                            List<Coord> coordList = new List<Coord>();

                            foreach (XmlNode n3 in n2.ChildNodes)
                            {
                                if (n3.Name == "description")
                                {
                                    string desc = n3.InnerText;

                                    int startPos = desc.IndexOf("<td>class_code</td>") + "<td>class_code</td>".Length;
                                    int endPos = desc.IndexOf("</td>", startPos + 1);

                                    string classCode = desc.Substring(startPos, endPos - startPos);

                                    classCode = classCode.Replace("<td>", "").Trim();

                                    subsectorPolClass.ClassCode = classCode;
                                }
                                if (n3.Name == "styleUrl")
                                {
                                    string style = n3.InnerText;

                                    subsectorPolClass.StyleURL = style;
                                }
                                if (n3.Name == "MultiGeometry")
                                {
                                    foreach (XmlNode n4 in n3.ChildNodes)
                                    {
                                        if (n4.Name == "Polygon")
                                        {
                                            foreach (XmlNode n5 in n4.ChildNodes)
                                            {
                                                if (n5.Name == "outerBoundaryIs")
                                                {
                                                    foreach (XmlNode n6 in n5.ChildNodes)
                                                    {
                                                        if (n6.Name == "LinearRing")
                                                        {
                                                            foreach (XmlNode n7 in n6.ChildNodes)
                                                            {
                                                                if (n7.Name == "coordinates")
                                                                {

                                                                    string coordText = n7.InnerText.Trim();

                                                                    List<string> pointTextList = coordText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                                                                    int ordinal = 0;
                                                                    foreach (string s in pointTextList)
                                                                    {
                                                                        List<string> pointsText = s.Split(",".ToCharArray(), StringSplitOptions.None).ToList();

                                                                        if (pointsText.Count != 3)
                                                                        {
                                                                            richTextBoxStatus.AppendText($"pointsText.Count [{pointsText.Count}] != 3 { pointsText[0] }");
                                                                            return;
                                                                        }

                                                                        if (!float.TryParse(pointsText[0], out float Lng))
                                                                        {
                                                                            richTextBoxStatus.AppendText($"Could not parse Lng { pointsText[0] } to a float number");
                                                                            return;
                                                                        }
                                                                        if (!float.TryParse(pointsText[1], out float Lat))
                                                                        {
                                                                            richTextBoxStatus.AppendText($"Could not parse Lat { pointsText[1] } to a float number");
                                                                            return;
                                                                        }

                                                                        coordList.Add(new Coord() { Lat = Lat, Lng = Lng, Ordinal = ordinal });

                                                                        ordinal++;

                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    subsectorPolClass.CoordList = coordList;

                                    float LatCentroid = (from c in coordList
                                                         select c.Lat).Average();

                                    float LngCentroid = (from c in coordList
                                                         select c.Lng).Average();

                                    subsectorPolClass.Centroid = new Coord() { Lat = LatCentroid, Lng = LngCentroid, Ordinal = 0 };
                                }
                            }

                            subsectorPolClassList.Add(subsectorPolClass);
                        }
                    }
                }
            }



            foreach (TVItemModel tvItemModelSS in tvItemModelSubsectorList)
            {
                string subsector = tvItemModelSS.TVText;
                if (subsector.Contains(" "))
                {
                    subsector = subsector.Substring(0, subsector.IndexOf(" "));
                }

                sb.AppendLine(@"	<Folder id=""FeatureLayer0"">");
                sb.AppendLine($@"		<name>{ subsector }</name>");
                sb.AppendLine(@"		<visibility>0</visibility>");
                sb.AppendLine(@"		<open>1</open>");
                sb.AppendLine(@"		<Snippet maxLines=""0""></Snippet>");

                lblStatus.Text = "Doing " + tvItemModelSS.TVText;
                lblStatus.Refresh();
                Application.DoEvents();

                List<MapInfoPointModel> mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.Subsector, MapInfoDrawTypeEnum.Polygon);

                List<Coord> coordList = (from c in mapInfoPointModelList
                                         orderby c.Ordinal
                                         select new Coord
                                         {
                                             Lat = (float)c.Lat,
                                             Lng = (float)c.Lng,
                                             Ordinal = c.Ordinal
                                         }).ToList();

                List<SubsectorPolClass> subsectorPolClassList2 = (from c in subsectorPolClassList
                                                                  where c.Subsector == null
                                                                  select c).ToList();

                foreach (SubsectorPolClass subsectorPolClass in subsectorPolClassList2)
                {
                    bool InSubsector = mapInfoService.CoordInPolygon(coordList, subsectorPolClass.Centroid);
                    if (InSubsector)
                    {
                        sb.AppendLine(@"		<Placemark id=""ID_00000"">");
                        sb.AppendLine($@"			<name>{ subsectorPolClass.ClassCode }</name>");
                        sb.AppendLine(@"			<visibility>0</visibility>");
                        sb.AppendLine(@"			<Snippet maxLines=""0""></Snippet>");
                        sb.AppendLine($@"			<styleUrl>{ subsectorPolClass.StyleURL }</styleUrl>");
                        sb.AppendLine(@"			<MultiGeometry>");
                        sb.AppendLine(@"				<Polygon>");
                        sb.AppendLine(@"					<outerBoundaryIs>");
                        sb.AppendLine(@"						<LinearRing>");
                        sb.AppendLine(@"							<coordinates>");
                        foreach (Coord coord in subsectorPolClass.CoordList)
                        {
                            sb.Append($"{ coord.Lng },{ coord.Lat },0 ");
                        }
                        sb.AppendLine(@"							</coordinates>");
                        sb.AppendLine(@"						</LinearRing>");
                        sb.AppendLine(@"					</outerBoundaryIs>");
                        sb.AppendLine(@"				</Polygon>");
                        sb.AppendLine(@"			</MultiGeometry>");
                        sb.AppendLine(@"		</Placemark>");

                        richTextBoxStatus.AppendText($"{ tvItemModelSS.TVText } --- {subsectorPolClass.ClassCode} --- { subsectorPolClass.Centroid.Lat } { subsectorPolClass.Centroid.Lng }\r\n");
                    }
                }

                sb.AppendLine(@"	</Folder>");
            }


            sb.AppendLine(@"</Document>");
            sb.AppendLine(@"</kml>");

            FileInfo fi = new FileInfo(@"C:\Users\leblancc\Desktop\ClassificationPolygons_BC2.kml");

            StreamWriter sw = fi.CreateText();
            sw.WriteLine(sb.ToString());
            sw.Close();

            lblStatus.Text = "Done...";

        }

        private void button22_Click(object sender, EventArgs e)
        {
            return;

            //TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            //MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            //TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            //if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            //TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            //if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            //TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            //if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            //List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            //if (tvItemModelSubsectorList.Count == 0)
            //{
            //    richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelProv.TVText + "\r\n");
            //    return;
            //}

            //List<SubsectorPolClass> subsectorPolClassList = new List<SubsectorPolClass>();

            //XmlDocument doc = new XmlDocument();
            //doc.Load(@"C:\Users\leblancc\Desktop\ClassificationPolygons_QC.kml");
            //foreach (XmlNode n1 in doc.DocumentElement.ChildNodes[0].ChildNodes)
            //{
            //    if (n1.Name == "Folder")
            //    {
            //        foreach (XmlNode n2 in n1.ChildNodes)
            //        {
            //            if (n2.Name == "Placemark")
            //            {
            //                SubsectorPolClass subsectorPolClass = new SubsectorPolClass();
            //                List<Coord> coordList = new List<Coord>();

            //                foreach (XmlNode n3 in n2.ChildNodes)
            //                {
            //                    if (n3.Name == "description")
            //                    {
            //                        string desc = n3.InnerText;

            //                        int startPos = desc.IndexOf("<td>Class_Code</td>") + "<td>Class_Code</td>".Length;
            //                        int endPos = desc.IndexOf("</td>", startPos + 1);

            //                        string classCode = desc.Substring(startPos, endPos - startPos);

            //                        classCode = classCode.Replace("<td>", "").Trim();

            //                        subsectorPolClass.ClassCode = classCode;

            //                        int startPos2 = desc.IndexOf("<td>Sector</td>") + "<td>Sector</td>".Length;
            //                        int endPos2 = desc.IndexOf("</td>", startPos2 + 1);

            //                        string subsector = desc.Substring(startPos2, endPos2 - startPos2);

            //                        subsector = subsector.Replace("<td>", "").Trim();

            //                        subsectorPolClass.Subsector = subsector;
            //                    }
            //                    if (n3.Name == "styleUrl")
            //                    {
            //                        string style = n3.InnerText;

            //                        subsectorPolClass.StyleURL = style;
            //                    }
            //                    if (n3.Name == "MultiGeometry")
            //                    {
            //                        foreach (XmlNode n4 in n3.ChildNodes)
            //                        {
            //                            if (n4.Name == "Polygon")
            //                            {
            //                                foreach (XmlNode n5 in n4.ChildNodes)
            //                                {
            //                                    if (n5.Name == "outerBoundaryIs")
            //                                    {
            //                                        foreach (XmlNode n6 in n5.ChildNodes)
            //                                        {
            //                                            if (n6.Name == "LinearRing")
            //                                            {
            //                                                foreach (XmlNode n7 in n6.ChildNodes)
            //                                                {
            //                                                    if (n7.Name == "coordinates")
            //                                                    {

            //                                                        string coordText = n7.InnerText.Trim();

            //                                                        List<string> pointTextList = coordText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            //                                                        int ordinal = 0;
            //                                                        foreach (string s in pointTextList)
            //                                                        {
            //                                                            List<string> pointsText = s.Split(",".ToCharArray(), StringSplitOptions.None).ToList();

            //                                                            if (pointsText.Count != 3)
            //                                                            {
            //                                                                richTextBoxStatus.AppendText($"pointsText.Count [{pointsText.Count}] != 3 { pointsText[0] }\r\n");
            //                                                                return;
            //                                                            }

            //                                                            if (!float.TryParse(pointsText[0], out float Lng))
            //                                                            {
            //                                                                richTextBoxStatus.AppendText($"Could not parse Lng { pointsText[0] } to a float number\r\n");
            //                                                                return;
            //                                                            }
            //                                                            if (!float.TryParse(pointsText[1], out float Lat))
            //                                                            {
            //                                                                richTextBoxStatus.AppendText($"Could not parse Lat { pointsText[1] } to a float number\r\n");
            //                                                                return;
            //                                                            }

            //                                                            coordList.Add(new Coord() { Lat = Lat, Lng = Lng, Ordinal = ordinal });

            //                                                            ordinal++;

            //                                                        }
            //                                                    }
            //                                                }
            //                                            }
            //                                        }
            //                                    }
            //                                }
            //                            }
            //                        }

            //                        subsectorPolClass.CoordList = coordList;

            //                        float LatCentroid = (from c in coordList
            //                                             select c.Lat).Average();

            //                        float LngCentroid = (from c in coordList
            //                                             select c.Lng).Average();

            //                        subsectorPolClass.Centroid = new Coord() { Lat = LatCentroid, Lng = LngCentroid, Ordinal = 0 };
            //                    }
            //                }

            //                subsectorPolClassList.Add(subsectorPolClass);
            //            }
            //        }
            //    }
            //}

            ////bool start = false;
            //foreach (TVItemModel tvItemModelSS in tvItemModelSubsectorList)
            //{
            //    string subsector = tvItemModelSS.TVText;
            //    if (subsector.Contains(" "))
            //    {
            //        subsector = subsector.Substring(0, subsector.IndexOf(" "));
            //    }

            //    lblStatus.Text = "Doing " + subsector;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    //if (subsector == "N-15.1.2")
            //    //{
            //    //    start = true;
            //    //}

            //    //if (!start)
            //    //{
            //    //    continue;
            //    //}

            //    List<MapInfoModel> mapInfoModelList = mapInfoService.GetMapInfoModelListWithTVItemIDDB(tvItemModelSS.TVItemID);

            //    foreach (MapInfoModel mapInfoModel in mapInfoModelList)
            //    {
            //        if (mapInfoModel.MapInfoDrawType == MapInfoDrawTypeEnum.Polygon)
            //        {
            //            SubsectorPolClass subsectorPolClass = (from c in subsectorPolClassList
            //                                                   where c.Subsector == subsector
            //                                                   select c).FirstOrDefault();

            //            if (subsectorPolClass == null)
            //            {
            //                richTextBoxStatus.AppendText($"Could not find subsector in classification polygons: { subsector }\r\n");
            //                continue;
            //            }

            //            StringBuilder sb = new StringBuilder();

            //            foreach (Coord coord in subsectorPolClass.CoordList)
            //            {
            //                sb.Append($"{ coord.Lat }s{ coord.Lng }p");
            //            }

            //            string LatLngListText = sb.ToString();

            //            MapInfoModel mapInfoModelret = mapInfoService.PostSavePolyDB(LatLngListText, mapInfoModel.MapInfoID);
            //            if (!string.IsNullOrWhiteSpace(mapInfoModelret.Error))
            //            {
            //                richTextBoxStatus.AppendText($"ERROR: { mapInfoModelret.Error }\r\n");
            //                return;
            //            }
            //        }
            //    }
            //}

            //lblStatus.Text = "done...";
        }

        private void button23_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            List<TVItemModel> tvItemModelAreaList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Area);
            if (tvItemModelAreaList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Area for " + tvItemModelProv.TVText + "\r\n");
                return;
            }

            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine(@"<Document>");
            sb.AppendLine(@"	<name>QC_Info.kml</name>");
            sb.AppendLine(@"	<Style id=""sn_ylw-pushpin"">");
            sb.AppendLine(@"		<IconStyle>");
            sb.AppendLine(@"			<scale>1.1</scale>");
            sb.AppendLine(@"			<Icon>");
            sb.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine(@"			</Icon>");
            sb.AppendLine(@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine(@"		</IconStyle>");
            sb.AppendLine(@"		<BalloonStyle>");
            sb.AppendLine(@"		</BalloonStyle>");
            sb.AppendLine(@"		<LineStyle>");
            sb.AppendLine(@"			<color>ff00ff00</color>");
            sb.AppendLine(@"			<width>2</width>");
            sb.AppendLine(@"		</LineStyle>");
            sb.AppendLine(@"		<PolyStyle>");
            sb.AppendLine(@"			<color>00ffffff</color>");
            sb.AppendLine(@"		</PolyStyle>");
            sb.AppendLine(@"	</Style>");
            sb.AppendLine(@"	<Style id=""sn_ylw-pushpin0"">");
            sb.AppendLine(@"		<IconStyle>");
            sb.AppendLine(@"			<scale>1.1</scale>");
            sb.AppendLine(@"			<Icon>");
            sb.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine(@"			</Icon>");
            sb.AppendLine(@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine(@"		</IconStyle>");
            sb.AppendLine(@"		<BalloonStyle>");
            sb.AppendLine(@"		</BalloonStyle>");
            sb.AppendLine(@"		<LineStyle>");
            sb.AppendLine(@"			<color>ffff00ff</color>");
            sb.AppendLine(@"			<width>2</width>");
            sb.AppendLine(@"		</LineStyle>");
            sb.AppendLine(@"		<PolyStyle>");
            sb.AppendLine(@"			<color>00ffffff</color>");
            sb.AppendLine(@"		</PolyStyle>");
            sb.AppendLine(@"	</Style>");
            sb.AppendLine(@"	<Style id=""sh_ylw-pushpin"">");
            sb.AppendLine(@"		<IconStyle>");
            sb.AppendLine(@"			<scale>1.3</scale>");
            sb.AppendLine(@"			<Icon>");
            sb.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine(@"			</Icon>");
            sb.AppendLine(@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine(@"		</IconStyle>");
            sb.AppendLine(@"		<BalloonStyle>");
            sb.AppendLine(@"		</BalloonStyle>");
            sb.AppendLine(@"		<LineStyle>");
            sb.AppendLine(@"			<color>ffff00ff</color>");
            sb.AppendLine(@"			<width>2</width>");
            sb.AppendLine(@"		</LineStyle>");
            sb.AppendLine(@"		<PolyStyle>");
            sb.AppendLine(@"			<color>00ffffff</color>");
            sb.AppendLine(@"		</PolyStyle>");
            sb.AppendLine(@"	</Style>");
            sb.AppendLine(@"	<StyleMap id=""msn_ylw-pushpin"">");
            sb.AppendLine(@"		<Pair>");
            sb.AppendLine(@"			<key>normal</key>");
            sb.AppendLine(@"			<styleUrl>#sn_ylw-pushpin</styleUrl>");
            sb.AppendLine(@"		</Pair>");
            sb.AppendLine(@"		<Pair>");
            sb.AppendLine(@"			<key>highlight</key>");
            sb.AppendLine(@"			<styleUrl>#sh_ylw-pushpin0</styleUrl>");
            sb.AppendLine(@"		</Pair>");
            sb.AppendLine(@"	</StyleMap>");
            sb.AppendLine(@"	<Style id=""sh_ylw-pushpin0"">");
            sb.AppendLine(@"		<IconStyle>");
            sb.AppendLine(@"			<scale>1.3</scale>");
            sb.AppendLine(@"			<Icon>");
            sb.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine(@"			</Icon>");
            sb.AppendLine(@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine(@"		</IconStyle>");
            sb.AppendLine(@"		<BalloonStyle>");
            sb.AppendLine(@"		</BalloonStyle>");
            sb.AppendLine(@"		<LineStyle>");
            sb.AppendLine(@"			<color>ff00ff00</color>");
            sb.AppendLine(@"			<width>2</width>");
            sb.AppendLine(@"		</LineStyle>");
            sb.AppendLine(@"		<PolyStyle>");
            sb.AppendLine(@"			<color>00ffffff</color>");
            sb.AppendLine(@"		</PolyStyle>");
            sb.AppendLine(@"	</Style>");
            sb.AppendLine(@"	<StyleMap id=""msn_ylw-pushpin0"">");
            sb.AppendLine(@"		<Pair>");
            sb.AppendLine(@"			<key>normal</key>");
            sb.AppendLine(@"			<styleUrl>#sn_ylw-pushpin1</styleUrl>");
            sb.AppendLine(@"		</Pair>");
            sb.AppendLine(@"		<Pair>");
            sb.AppendLine(@"			<key>highlight</key>");
            sb.AppendLine(@"			<styleUrl>#sh_ylw-pushpin1</styleUrl>");
            sb.AppendLine(@"		</Pair>");
            sb.AppendLine(@"	</StyleMap>");
            sb.AppendLine(@"	<Style id=""sh_ylw-pushpin1"">");
            sb.AppendLine(@"		<IconStyle>");
            sb.AppendLine(@"			<scale>1.3</scale>");
            sb.AppendLine(@"			<Icon>");
            sb.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine(@"			</Icon>");
            sb.AppendLine(@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine(@"		</IconStyle>");
            sb.AppendLine(@"		<BalloonStyle>");
            sb.AppendLine(@"		</BalloonStyle>");
            sb.AppendLine(@"		<LineStyle>");
            sb.AppendLine(@"			<color>ff0000ff</color>");
            sb.AppendLine(@"			<width>2</width>");
            sb.AppendLine(@"		</LineStyle>");
            sb.AppendLine(@"		<PolyStyle>");
            sb.AppendLine(@"			<color>00ffffff</color>");
            sb.AppendLine(@"		</PolyStyle>");
            sb.AppendLine(@"	</Style>");
            sb.AppendLine(@"	<Style id=""sn_ylw-pushpin1"">");
            sb.AppendLine(@"		<IconStyle>");
            sb.AppendLine(@"			<scale>1.1</scale>");
            sb.AppendLine(@"			<Icon>");
            sb.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
            sb.AppendLine(@"			</Icon>");
            sb.AppendLine(@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
            sb.AppendLine(@"		</IconStyle>");
            sb.AppendLine(@"		<BalloonStyle>");
            sb.AppendLine(@"		</BalloonStyle>");
            sb.AppendLine(@"		<LineStyle>");
            sb.AppendLine(@"			<color>ff0000ff</color>");
            sb.AppendLine(@"			<width>2</width>");
            sb.AppendLine(@"		</LineStyle>");
            sb.AppendLine(@"		<PolyStyle>");
            sb.AppendLine(@"			<color>00ffffff</color>");
            sb.AppendLine(@"		</PolyStyle>");
            sb.AppendLine(@"	</Style>");
            sb.AppendLine(@"	<StyleMap id=""msn_ylw-pushpin1"">");
            sb.AppendLine(@"		<Pair>");
            sb.AppendLine(@"			<key>normal</key>");
            sb.AppendLine(@"			<styleUrl>#sn_ylw-pushpin0</styleUrl>");
            sb.AppendLine(@"		</Pair>");
            sb.AppendLine(@"		<Pair>");
            sb.AppendLine(@"			<key>highlight</key>");
            sb.AppendLine(@"			<styleUrl>#sh_ylw-pushpin</styleUrl>");
            sb.AppendLine(@"		</Pair>");
            sb.AppendLine(@"	</StyleMap>");
            sb.AppendLine(@"	<Style id=""sh_placemark_circle_highlight"">");
            sb.AppendLine(@"		<IconStyle>");
            sb.AppendLine(@"			<scale>1.2</scale>");
            sb.AppendLine(@"			<Icon>");
            sb.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_circle_highlight.png</href>");
            sb.AppendLine(@"			</Icon>");
            sb.AppendLine(@"		</IconStyle>");
            sb.AppendLine(@"		<BalloonStyle>");
            sb.AppendLine(@"		</BalloonStyle>");
            sb.AppendLine(@"		<ListStyle>");
            sb.AppendLine(@"		</ListStyle>");
            sb.AppendLine(@"	</Style>");
            sb.AppendLine(@"	<StyleMap id=""msn_placemark_circle"">");
            sb.AppendLine(@"		<Pair>");
            sb.AppendLine(@"			<key>normal</key>");
            sb.AppendLine(@"			<styleUrl>#sn_placemark_circle</styleUrl>");
            sb.AppendLine(@"		</Pair>");
            sb.AppendLine(@"		<Pair>");
            sb.AppendLine(@"			<key>highlight</key>");
            sb.AppendLine(@"			<styleUrl>#sh_placemark_circle_highlight</styleUrl>");
            sb.AppendLine(@"		</Pair>");
            sb.AppendLine(@"	</StyleMap>");
            sb.AppendLine(@"	<Style id=""sn_placemark_circle"">");
            sb.AppendLine(@"		<IconStyle>");
            sb.AppendLine(@"			<scale>0.8</scale>");
            sb.AppendLine(@"			<Icon>");
            sb.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_circle.png</href>");
            sb.AppendLine(@"			</Icon>");
            sb.AppendLine(@"		</IconStyle>");
            sb.AppendLine(@"		<LabelStyle>");
            sb.AppendLine(@"		    <scale>0.8</scale>");
            sb.AppendLine(@"		</LabelStyle>");
            sb.AppendLine(@"		<BalloonStyle>");
            sb.AppendLine(@"		</BalloonStyle>");
            sb.AppendLine(@"		<ListStyle>");
            sb.AppendLine(@"		</ListStyle>");
            sb.AppendLine(@"    </Style>");


            List<MapInfoPointModel> mapInfoPointModelList = new List<MapInfoPointModel>();

            foreach (TVItemModel tvItemModelArea in tvItemModelAreaList)
            {
                string area = tvItemModelArea.TVText;
                if (area.Contains(" "))
                {
                    area = area.Substring(0, area.IndexOf(" "));
                }

                mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelArea.TVItemID, TVTypeEnum.Area, MapInfoDrawTypeEnum.Polygon);

                sb.AppendLine(@"	<Folder>");
                sb.AppendLine($@"		<name>{ area }</name>");
                sb.AppendLine(@"		<Placemark>");
                sb.AppendLine($@"			<name>{ area }</name>");
                sb.AppendLine(@"			<styleUrl>#msn_ylw-pushpin1</styleUrl>");
                sb.AppendLine(@"			<Polygon>");
                sb.AppendLine(@"				<outerBoundaryIs>");
                sb.AppendLine(@"					<LinearRing>");
                sb.AppendLine(@"						<coordinates>");
                foreach (MapInfoPointModel mapInfoPointModel in mapInfoPointModelList)
                {
                    sb.Append($"{ mapInfoPointModel.Lng },{ mapInfoPointModel.Lat },0 ");
                }
                sb.AppendLine(@"						</coordinates>");
                sb.AppendLine(@"					</LinearRing>");
                sb.AppendLine(@"				</outerBoundaryIs>");
                sb.AppendLine(@"			</Polygon>");
                sb.AppendLine(@"		</Placemark>");

                List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelArea.TVItemID, TVTypeEnum.Sector);
                if (tvItemModelSectorList.Count == 0)
                {
                    richTextBoxStatus.AppendText("Error: could not find TVItem Sector for " + tvItemModelArea.TVText + "\r\n");
                    sb.AppendLine(@"	</Folder>");
                    continue;
                }

                foreach (TVItemModel tvItemModelSec in tvItemModelSectorList)
                {
                    string sector = tvItemModelSec.TVText;
                    if (sector.Contains(" "))
                    {
                        sector = sector.Substring(0, sector.IndexOf(" "));
                    }

                    mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelSec.TVItemID, TVTypeEnum.Sector, MapInfoDrawTypeEnum.Polygon);

                    sb.AppendLine(@"		<Folder>");
                    sb.AppendLine($@"			<name>{ sector }</name>");
                    sb.AppendLine(@"			<Placemark>");
                    sb.AppendLine($@"				<name>{ sector }</name>");
                    sb.AppendLine(@"				<styleUrl>#msn_ylw-pushpin</styleUrl>");
                    sb.AppendLine(@"				<Polygon>");
                    sb.AppendLine(@"					<outerBoundaryIs>");
                    sb.AppendLine(@"						<LinearRing>");
                    sb.AppendLine(@"							<coordinates>");
                    foreach (MapInfoPointModel mapInfoPointModel in mapInfoPointModelList)
                    {
                        sb.Append($"{ mapInfoPointModel.Lng },{ mapInfoPointModel.Lat },0 ");
                    }
                    sb.AppendLine(@"							</coordinates>");
                    sb.AppendLine(@"						</LinearRing>");
                    sb.AppendLine(@"					</outerBoundaryIs>");
                    sb.AppendLine(@"				</Polygon>");
                    sb.AppendLine(@"			</Placemark>");

                    List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSec.TVItemID, TVTypeEnum.Subsector);
                    if (tvItemModelSubsectorList.Count == 0)
                    {
                        richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelSec.TVText + "\r\n");
                        sb.AppendLine(@"	    </Folder>");
                        continue;
                    }

                    foreach (TVItemModel tvItemModelSS in tvItemModelSubsectorList)
                    {
                        string subsector = tvItemModelSS.TVText;
                        if (subsector.Contains(" "))
                        {
                            subsector = subsector.Substring(0, subsector.IndexOf(" "));
                        }

                        lblStatus.Text = $"Doing { area } --- { sector } --- { subsector }";
                        lblStatus.Refresh();
                        Application.DoEvents();

                        mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.Subsector, MapInfoDrawTypeEnum.Polygon);

                        sb.AppendLine(@"			<Folder>");
                        sb.AppendLine($@"				<name>{ subsector }</name>");
                        sb.AppendLine(@"				<Placemark>");
                        sb.AppendLine($@"					<name>{ subsector }</name>");
                        sb.AppendLine(@"					<styleUrl>#msn_ylw-pushpin0</styleUrl>");
                        sb.AppendLine(@"					<Polygon>");
                        sb.AppendLine(@"						<outerBoundaryIs>");
                        sb.AppendLine(@"							<LinearRing>");
                        sb.AppendLine(@"								<coordinates>");
                        foreach (MapInfoPointModel mapInfoPointModel in mapInfoPointModelList)
                        {
                            sb.Append($"{ mapInfoPointModel.Lng },{ mapInfoPointModel.Lat },0 ");
                        }
                        sb.AppendLine(@"								</coordinates>");
                        sb.AppendLine(@"							</LinearRing>");
                        sb.AppendLine(@"						</outerBoundaryIs>");
                        sb.AppendLine(@"					</Polygon>");
                        sb.AppendLine(@"				</Placemark>");

                        sb.AppendLine(@"		    	<Folder>");
                        sb.AppendLine($@"		    		<name>{ subsector } MWQM Sites</name>");

                        List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.MWQMSite);

                        foreach (TVItemModel tvItemModelMWQMSite in tvItemModelMWQMSiteList)
                        {
                            mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelMWQMSite.TVItemID, TVTypeEnum.MWQMSite, MapInfoDrawTypeEnum.Point);

                            if (mapInfoPointModelList.Count > 0)
                            {
                                sb.AppendLine(@"		    		<Placemark>");
                                sb.AppendLine($@"		    			<name>{ tvItemModelMWQMSite.TVText }</name>");
                                sb.AppendLine(@"		    			<styleUrl>#msn_placemark_circle</styleUrl>");
                                sb.AppendLine(@"		    			<Point>");
                                sb.AppendLine($@"		    				<coordinates>{ mapInfoPointModelList[0].Lng },{ mapInfoPointModelList[0].Lat },0</coordinates>");
                                sb.AppendLine(@"		    			</Point>");
                                sb.AppendLine(@"		    		</Placemark>");
                            }
                        }

                        sb.AppendLine(@"		    	</Folder>");
                        sb.AppendLine(@"			</Folder>");
                    }
                    sb.AppendLine(@"		</Folder>");
                }
                sb.AppendLine(@"	</Folder>");
            }

            sb.AppendLine(@"</Document>");
            sb.AppendLine(@"</kml>");


            FileInfo fi = new FileInfo(@"C:\Users\leblancc\Desktop\QC_Info.kml");

            StreamWriter sw = fi.CreateText();
            sw.WriteLine(sb.ToString());
            sw.Close();

            lblStatus.Text = "Done...";

        }

        private void button24_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Area for " + tvItemModelProv.TVText + "\r\n");
                return;
            }
            List<MapInfoPointModel> mapInfoPointModelList = new List<MapInfoPointModel>();

            sb.AppendLine($"MWQM sites found outside the subsector polygon (i.e. QC classification)");
            sb.AppendLine($"Subsector\tMWQMSite\tLat\tLng");

            foreach (TVItemModel tvItemModelSS in tvItemModelSubsectorList)
            {
                string subsector = tvItemModelSS.TVText;
                if (subsector.Contains(" "))
                {
                    subsector = subsector.Substring(0, subsector.IndexOf(" "));
                }

                lblStatus.Text = $"Doing { subsector }";
                lblStatus.Refresh();
                Application.DoEvents();

                mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.Subsector, MapInfoDrawTypeEnum.Polygon);

                if (mapInfoPointModelList.Count > 0)
                {
                    List<Coord> coordList = new List<Coord>();
                    int Ordinal = 0;
                    foreach (MapInfoPointModel mapInfoPointModel in mapInfoPointModelList)
                    {
                        coordList.Add(new Coord() { Lat = (float)mapInfoPointModel.Lat, Lng = (float)mapInfoPointModel.Lng, Ordinal = Ordinal });
                        Ordinal++;
                    }

                    List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelSS.TVItemID, TVTypeEnum.MWQMSite);

                    foreach (TVItemModel tvItemModelMWQMSite in tvItemModelMWQMSiteList)
                    {
                        if (subsector == "A-14.3.5E" && tvItemModelMWQMSite.TVText == "0074")
                        {
                            //int slijfe = 34;
                        }

                        mapInfoPointModelList = mapInfoService._MapInfoPointService.GetMapInfoPointModelListWithTVItemIDAndTVTypeAndMapInfoDrawTypeDB(tvItemModelMWQMSite.TVItemID, TVTypeEnum.MWQMSite, MapInfoDrawTypeEnum.Point);

                        if (mapInfoPointModelList.Count > 0)
                        {
                            Coord coord = new Coord() { Lat = (float)mapInfoPointModelList[0].Lat, Lng = (float)mapInfoPointModelList[0].Lng, Ordinal = 0 };
                            bool InPoly = mapInfoService.CoordInPolygon(coordList, coord);

                            if (!InPoly)
                            {
                                sb.AppendLine($"{ subsector }\t{ tvItemModelMWQMSite.TVText }\t{ coord.Lat.ToString("F6") }\t{ coord.Lng.ToString("F6") }");
                            }
                        }
                    }
                }
            }

            richTextBoxStatus.Text = sb.ToString();

            lblStatus.Text = "Done...";
        }

        private void button26_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(@"C:\Users\leblancc\Desktop\QC_InfoF.kml");

            if (!fi.Exists)
            {
                richTextBoxStatus.Text = $"Could not find file [{fi.FullName}]";
                return;
            }

            StringBuilder sb = new StringBuilder();

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Québec", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            List<TVItemModel> tvItemModelAreaList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Area);
            if (tvItemModelAreaList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Area for " + tvItemModelProv.TVText + "\r\n");
                return;
            }

            List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Sector);
            if (tvItemModelSectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Sector for " + tvItemModelProv.TVText + "\r\n");
                return;
            }

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelProv.TVText + "\r\n");
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(fi.FullName);

            string CurrentArea = "";
            string CurrentSector = "";
            string CurrentSubsector = "";

            XmlNode StartNode = doc.ChildNodes[1].ChildNodes[0];
            foreach (XmlNode n in StartNode.ChildNodes)
            {
                if (n.Name == "Folder")
                {
                    foreach (XmlNode n2 in n.ChildNodes)
                    {
                        if (n2.Name == "name")
                        {
                            CurrentArea = n2.InnerText;
                            lblStatus.Text = CurrentArea;
                            lblStatus.Refresh();
                            Application.DoEvents();
                        }
                        //else if (n2.Name == "Placemark") // Area
                        //{
                        //    foreach(XmlNode p1 in n2.ChildNodes)
                        //    {
                        //        if (p1.Name == "name")
                        //        {
                        //            string tempArea = p1.InnerText;
                        //            if (tempArea != CurrentArea)
                        //            {
                        //                richTextBoxStatus.AppendText("ERROR: tempArea != CurrentArea\r\n");
                        //                return;
                        //            }
                        //        }
                        //        else if (p1.Name == "Polygon") // Area polygon
                        //        {
                        //            List<Coord> coordList = new List<Coord>();
                        //            GetCoordList(coordList, p1);

                        //            TVItemModel tvItemModelArea = tvItemModelAreaList.Where(c => c.TVText.StartsWith(CurrentArea + " ")).FirstOrDefault();
                        //            if (!string.IsNullOrWhiteSpace(tvItemModelArea.Error))
                        //            {
                        //                richTextBoxStatus.AppendText($"{tvItemModelArea.Error}\r\n");
                        //            }

                        //            List<MapInfo> mapInfoPolyList = mapInfoService.GetMapInfoListWithTVItemIDDB(tvItemModelArea.TVItemID).Where(c => c.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon).ToList();

                        //            if (mapInfoPolyList.Count > 1)
                        //            {
                        //                for (int i = 0; i < mapInfoPolyList.Count - 1; i++)
                        //                {
                        //                    MapInfoModel mapInfoModel = mapInfoService.PostDeleteMapInfoDB(mapInfoPolyList[i].MapInfoID);
                        //                    if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                        //                    {
                        //                        richTextBoxStatus.AppendText($"{mapInfoModel.Error}\r\n");
                        //                        return;
                        //                    }
                        //                }
                        //            }

                        //            mapInfoPolyList = mapInfoService.GetMapInfoListWithTVItemIDDB(tvItemModelArea.TVItemID).Where(c => c.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon).ToList();
                        //            foreach (MapInfo mapInfo in mapInfoPolyList)
                        //            {
                        //                if (coordList.Count > 0)
                        //                {
                        //                    StringBuilder sbLL = new StringBuilder();
                        //                    foreach (Coord coord in coordList)
                        //                    {
                        //                        sbLL.Append($"{coord.Lat}s{coord.Lng}p");
                        //                    }
                        //                    sbLL.Append($"{coordList[0].Lat}s{coordList[0].Lng}p");

                        //                    MapInfoModel mapInfoModelRet = mapInfoService.PostSavePolyDB(sbLL.ToString(), mapInfo.MapInfoID);
                        //                    if (!string.IsNullOrWhiteSpace(mapInfoModelRet.Error))
                        //                    {
                        //                        richTextBoxStatus.AppendText($"{mapInfoModelRet.Error}\r\n");
                        //                        return;
                        //                    }
                        //                }
                        //            }

                        //        }
                        //    }
                        //}
                        else if (n2.Name == "Folder")
                        {
                            foreach (XmlNode n3 in n2.ChildNodes)
                            {
                                if (n3.Name == "name")
                                {
                                    CurrentSector = n3.InnerText;
                                    lblStatus.Text = CurrentArea + " - " + CurrentSector;
                                    lblStatus.Refresh();
                                    Application.DoEvents();

                                }
                                //else if (n3.Name == "Placemark") // Sector
                                //{
                                //    foreach (XmlNode p1 in n3.ChildNodes)
                                //    {
                                //        if (p1.Name == "name")
                                //        {
                                //            string tempSector = p1.InnerText;
                                //            if (tempSector != CurrentSector)
                                //            {
                                //                richTextBoxStatus.AppendText("ERROR: tempSector != CurrentSector\r\n");
                                //                return;
                                //            }
                                //        }
                                //        else if (p1.Name == "Polygon") // Sector polygon
                                //        {
                                //            List<Coord> coordList = new List<Coord>();
                                //            GetCoordList(coordList, p1);

                                //            TVItemModel tvItemModelSector = tvItemModelSectorList.Where(c => c.TVText.StartsWith(CurrentSector + " ")).FirstOrDefault();
                                //            if (!string.IsNullOrWhiteSpace(tvItemModelSector.Error))
                                //            {
                                //                richTextBoxStatus.AppendText($"{tvItemModelSector.Error}\r\n");
                                //            }

                                //            List<MapInfo> mapInfoPolyList = mapInfoService.GetMapInfoListWithTVItemIDDB(tvItemModelSector.TVItemID).Where(c => c.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon).ToList();

                                //            if (mapInfoPolyList.Count > 1)
                                //            {
                                //                for (int i = 0; i < mapInfoPolyList.Count - 1; i++)
                                //                {
                                //                    MapInfoModel mapInfoModel = mapInfoService.PostDeleteMapInfoDB(mapInfoPolyList[i].MapInfoID);
                                //                    if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                                //                    {
                                //                        richTextBoxStatus.AppendText($"{mapInfoModel.Error}\r\n");
                                //                        return;
                                //                    }
                                //                }
                                //            }

                                //            mapInfoPolyList = mapInfoService.GetMapInfoListWithTVItemIDDB(tvItemModelSector.TVItemID).Where(c => c.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon).ToList();
                                //            foreach (MapInfo mapInfo in mapInfoPolyList)
                                //            {
                                //                if (coordList.Count > 0)
                                //                {
                                //                    StringBuilder sbLL = new StringBuilder();
                                //                    foreach (Coord coord in coordList)
                                //                    {
                                //                        sbLL.Append($"{coord.Lat}s{coord.Lng}p");
                                //                    }
                                //                    sbLL.Append($"{coordList[0].Lat}s{coordList[0].Lng}p");

                                //                    MapInfoModel mapInfoModelRet = mapInfoService.PostSavePolyDB(sbLL.ToString(), mapInfo.MapInfoID);
                                //                    if (!string.IsNullOrWhiteSpace(mapInfoModelRet.Error))
                                //                    {
                                //                        richTextBoxStatus.AppendText($"{mapInfoModelRet.Error}\r\n");
                                //                        return;
                                //                    }
                                //                }
                                //            }

                                //        }
                                //    }
                                //}
                                else if (n3.Name == "Folder")
                                {
                                    foreach (XmlNode n4 in n3.ChildNodes)
                                    {
                                        if (n4.Name == "name")
                                        {
                                            CurrentSubsector = n4.InnerText;
                                            lblStatus.Text = CurrentArea + " - " + CurrentSector + " - " + CurrentSubsector;
                                            lblStatus.Refresh();
                                            Application.DoEvents();
                                            richTextBoxStatus.AppendText($"{CurrentArea} - {CurrentSector} - {CurrentSubsector}\r\n");
                                        }
                                        else if (n4.Name == "Placemark") // Subsector
                                        {
                                            foreach (XmlNode p1 in n4.ChildNodes)
                                            {
                                                if (p1.Name == "name")
                                                {
                                                    string tempSubsector = p1.InnerText;
                                                    if (tempSubsector != CurrentSubsector)
                                                    {
                                                        richTextBoxStatus.AppendText("ERROR: tempSubsector != CurrentSubsector\r\n");
                                                        return;
                                                    }
                                                }
                                                else if (p1.Name == "Polygon") // Subsector polygon
                                                {
                                                    List<Coord> coordList = new List<Coord>();
                                                    GetCoordList(coordList, p1);

                                                    TVItemModel tvItemModelSubsector = tvItemModelSubsectorList.Where(c => c.TVText.StartsWith(CurrentSubsector + " ")).FirstOrDefault();
                                                    if (!string.IsNullOrWhiteSpace(tvItemModelSubsector.Error))
                                                    {
                                                        richTextBoxStatus.AppendText($"{tvItemModelSubsector.Error}\r\n");
                                                    }

                                                    List<MapInfo> mapInfoPolyList = mapInfoService.GetMapInfoListWithTVItemIDDB(tvItemModelSubsector.TVItemID).Where(c => c.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon).ToList();

                                                    if (mapInfoPolyList.Count > 1)
                                                    {
                                                        for (int i = 0; i < mapInfoPolyList.Count - 1; i++)
                                                        {
                                                            MapInfoModel mapInfoModel = mapInfoService.PostDeleteMapInfoDB(mapInfoPolyList[i].MapInfoID);
                                                            if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
                                                            {
                                                                richTextBoxStatus.AppendText($"{mapInfoModel.Error}\r\n");
                                                                return;
                                                            }
                                                        }
                                                    }

                                                    mapInfoPolyList = mapInfoService.GetMapInfoListWithTVItemIDDB(tvItemModelSubsector.TVItemID).Where(c => c.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Polygon).ToList();
                                                    foreach (MapInfo mapInfo in mapInfoPolyList)
                                                    {
                                                        if (coordList.Count > 0)
                                                        {
                                                            StringBuilder sbLL = new StringBuilder();
                                                            foreach (Coord coord in coordList)
                                                            {
                                                                sbLL.Append($"{coord.Lat}s{coord.Lng}p");
                                                            }
                                                            sbLL.Append($"{coordList[0].Lat}s{coordList[0].Lng}p");

                                                            MapInfoModel mapInfoModelRet = mapInfoService.PostSavePolyDB(sbLL.ToString(), mapInfo.MapInfoID);
                                                            if (!string.IsNullOrWhiteSpace(mapInfoModelRet.Error))
                                                            {
                                                                richTextBoxStatus.AppendText($"{mapInfoModelRet.Error}\r\n");
                                                                return;
                                                            }
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }


            }

            lblStatus.Text = "done...";
            lblStatus.Refresh();
            Application.DoEvents();
        }

        private void GetCoordList(List<Coord> coordList, XmlNode node)
        {
            if (node.Name == "coordinates")
            {
                List<string> pointListText = node.InnerText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                int ordinal = 0;
                foreach (string pointText in pointListText)
                {
                    string pointTxt = pointText.Replace("\r\n", "");

                    if (!string.IsNullOrWhiteSpace(pointTxt))
                    {
                        List<string> valListText = pointTxt.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                        if (valListText.Count != 3)
                        {
                            richTextBoxStatus.Text = "pointText.Count != 3";
                            return;
                        }

                        float Lng = float.Parse(valListText[0]);
                        float Lat = float.Parse(valListText[1]);

                        Coord coord = new Coord() { Lat = Lat, Lng = Lng, Ordinal = ordinal };

                        coordList.Add(coord);

                        ordinal += 1;
                    }
                }

                return;
            }

            foreach (XmlNode n in node.ChildNodes)
            {
                GetCoordList(coordList, n);
            }

        }

        private void button27_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Nova Scotia", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelProv.TVText + "\r\n");
                return;
            }

            List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.MWQMSite);
            if (tvItemModelMWQMSiteList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem MWQMSite for " + tvItemModelProv.TVText + "\r\n");
                return;
            }

            sb.AppendLine($"Subsector\tMWQMSite\tSiteLink\tDate\tRunLink\tSal\tKeep\tAllSalValueForSiteOrderByDate");

            foreach (TVItemModel tvItemModelSS in tvItemModelSubsectorList)
            {
                string subsector = tvItemModelSS.TVText;
                if (subsector.Contains(" "))
                {
                    subsector = subsector.Substring(0, subsector.IndexOf(" "));
                }

                lblStatus.Text = subsector;
                lblStatus.Refresh();
                Application.DoEvents();

                foreach (TVItemModel tvItemModelMWQMSite in tvItemModelMWQMSiteList.Where(c => c.ParentID == tvItemModelSS.TVItemID))
                {
                    using (CSSPDBEntities db2 = new CSSPDBEntities())
                    {
                        List<MWQMSample> mwqmSampleList = (from c in db2.MWQMSamples
                                                           where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                           && c.Salinity_PPT == 0
                                                           select c).ToList();

                        List<MWQMSample> mwqmSampleList2 = (from c in db2.MWQMSamples
                                                            where c.MWQMSiteTVItemID == tvItemModelMWQMSite.TVItemID
                                                            orderby c.SampleDateTime_Local
                                                            select c).ToList();

                        StringBuilder sbSalValues = new StringBuilder();
                        foreach (MWQMSample mwqmSample in mwqmSampleList2)
                        {
                            string salText = mwqmSample.Salinity_PPT == null ? "" : ((float)mwqmSample.Salinity_PPT).ToString("F1");
                            sbSalValues.Append($"{salText}|");
                        }


                        foreach (MWQMSample mwqmSample in mwqmSampleList)
                        {
                            sb.AppendLine($"{subsector}\t{tvItemModelMWQMSite.TVText}\t{tvItemModelMWQMSite.TVItemID}\t{mwqmSample.SampleDateTime_Local.ToString("yyyy MM dd")}\t{mwqmSample.MWQMRunTVItemID}\t{mwqmSample.Salinity_PPT}\tY\t{sbSalValues.ToString()}");
                        }

                    }

                }
            }

            richTextBoxStatus.Text = sb.ToString();

            lblStatus.Text = "Done...";
            lblStatus.Refresh();
            Application.DoEvents();

        }

        private void button28_Click(object sender, EventArgs e)
        {
            return;

            //StringBuilder sb = new StringBuilder();

            //TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            //InfrastructureService infrastructureService = new InfrastructureService(LanguageEnum.en, user);
            //TVItemLinkService tvItemLinkService = new TVItemLinkService(LanguageEnum.en, user);
            //AddressService addressService = new AddressService(LanguageEnum.en, user);

            //TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            //if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            //TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            //if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            //TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Prince Edward Island", TVTypeEnum.Province);
            //if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            //List<TVItemModel> tvItemModelMuniList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Municipality);


            //// doing contact
            //sb.AppendLine($"Province\tMunicipality\tContact\tUnder Correct Prov");

            //foreach (TVItemModel tvItemModelMuni in tvItemModelMuniList)
            //{
            //    lblStatus.Text = tvItemModelMuni.TVText;
            //    lblStatus.Refresh();
            //    Application.DoEvents();

            //    List<TVItemLinkModel> tvItemLinkModelList = tvItemLinkService.GetTVItemLinkModelListWithFromTVItemIDDB(tvItemModelMuni.TVItemID);

            //    foreach (TVItemLinkModel tvItemLinkModel in tvItemLinkModelList)
            //    {
            //        if (tvItemLinkModel.ToTVType == TVTypeEnum.Contact)
            //        {
            //            TVItemModel tvItemModelContact = tvItemService.GetTVItemModelWithTVItemIDDB(tvItemLinkModel.ToTVItemID);
            //            if (!string.IsNullOrWhiteSpace(tvItemModelContact.Error))
            //            {
            //                sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelContact.Error}");
            //            }
            //            else
            //            {
            //                sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelContact.TVText}\t");

            //                List<TVItemLinkModel> tvItemLinkModelList2 = tvItemLinkService.GetTVItemLinkModelListWithFromTVItemIDDB(tvItemLinkModel.ToTVItemID);
            //                foreach (TVItemLinkModel tvItemLinkModel2 in tvItemLinkModelList2)
            //                {
            //                    if (tvItemLinkModel2.ToTVType == TVTypeEnum.Address)
            //                    {
            //                        TVItemModel tvItemModelAddress = tvItemService.GetTVItemModelWithTVItemIDDB(tvItemLinkModel2.ToTVItemID);
            //                        if (!string.IsNullOrWhiteSpace(tvItemModelAddress.Error))
            //                        {
            //                            sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelAddress.Error}");
            //                        }
            //                        else
            //                        {
            //                            sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelContact.TVText}\t{tvItemModelAddress.TVText}");


            //                            AddressModel addressModel = addressService.GetAddressModelWithAddressTVItemIDDB((int)tvItemModelAddress.TVItemID);
            //                            if (!string.IsNullOrWhiteSpace(addressModel.Error))
            //                            {
            //                                sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelContact.TVText}\t{tvItemModelAddress.TVText}\tContactAddressTVItemID exist but can't find address");
            //                            }
            //                            else
            //                            {
            //                                if (addressModel.ProvinceTVItemID == tvItemModelProv.TVItemID)
            //                                {
            //                                    sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelContact.TVText}\t{tvItemModelAddress.TVText}\tOK");
            //                                }
            //                                else
            //                                {
            //                                    sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelContact.TVText}\t{tvItemModelAddress.TVText}\tNot OK");

            //                                    string TVTextMuni = addressModel.MunicipalityTVText;

            //                                    TVItemModel tvItemModelMuniOK = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelProv.TVItemID, TVTextMuni, TVTypeEnum.Municipality);
            //                                    if (!string.IsNullOrWhiteSpace(tvItemModelMuniOK.Error))
            //                                    {
            //                                        sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelContact.TVText}\t{tvItemModelAddress.TVText}\tCould not find muni [{TVTextMuni}]");
            //                                    }
            //                                    else
            //                                    {
            //                                        //addressModel.ProvinceTVItemID = tvItemModelProv.TVItemID;
            //                                        //addressModel.MunicipalityTVItemID = tvItemModelMuniOK.TVItemID;

            //                                        //AddressModel addressModelRet = addressService.PostUpdateAddressDB(addressModel);
            //                                        //if (!string.IsNullOrWhiteSpace(addressModelRet.Error))
            //                                        //{
            //                                        sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelContact.TVText}\t{tvItemModelAddress.TVText}\tCould not update new address");
            //                                        //}
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
            //// doing infrastructure
            ////sb.AppendLine($"Province\tMunicipality\tInfrastructure\tUnder Correct Prov");

            ////foreach (TVItemModel tvItemModelMuni in tvItemModelMuniList)
            ////{
            ////    lblStatus.Text = tvItemModelMuni.TVText;
            ////    lblStatus.Refresh();
            ////    Application.DoEvents();

            ////    List<TVItemModel> tvItemModelInfList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelMuni.TVItemID, TVTypeEnum.Infrastructure);

            ////    foreach (TVItemModel tvItemModelInf in tvItemModelInfList)
            ////    {
            ////        InfrastructureModel infrastructureModel = infrastructureService.GetInfrastructureModelWithInfrastructureTVItemIDDB(tvItemModelInf.TVItemID);
            ////        if (!string.IsNullOrWhiteSpace(infrastructureModel.Error))
            ////        {
            ////            sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelInf.TVText}\tERROR");
            ////        }
            ////        else
            ////        {
            ////            if (infrastructureModel.CivicAddressTVItemID == null)
            ////            {
            ////                sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelInf.TVText}\tNo Address");
            ////            }
            ////            else
            ////            {
            ////                AddressModel addressModel = addressService.GetAddressModelWithAddressTVItemIDDB((int)infrastructureModel.CivicAddressTVItemID);
            ////                if (!string.IsNullOrWhiteSpace(addressModel.Error))
            ////                {
            ////                    sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelInf.TVText}\tCivicAddressTVItemID exist but can't find address");
            ////                }
            ////                else
            ////                {
            ////                    if (addressModel.ProvinceTVItemID == tvItemModelProv.TVItemID)
            ////                    {
            ////                        sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelInf.TVText}\tOK");
            ////                    }
            ////                    else
            ////                    {
            ////                        sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelInf.TVText}\tNot OK");

            ////                        string TVTextMuni = addressModel.MunicipalityTVText;

            ////                        TVItemModel tvItemModelMuniOK = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelProv.TVItemID, TVTextMuni, TVTypeEnum.Municipality);
            ////                        if (!string.IsNullOrWhiteSpace(tvItemModelMuniOK.Error))
            ////                        {
            ////                            sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelInf.TVText}\tCould not find muni [{TVTextMuni}]");
            ////                        }
            ////                        else
            ////                        {
            ////                            //addressModel.ProvinceTVItemID = tvItemModelProv.TVItemID;
            ////                            //addressModel.MunicipalityTVItemID = tvItemModelMuniOK.TVItemID;

            ////                            //AddressModel addressModelRet = addressService.PostUpdateAddressDB(addressModel);
            ////                            //if (!string.IsNullOrWhiteSpace(addressModelRet.Error))
            ////                            //{
            ////                                sb.AppendLine($"{tvItemModelProv.TVText}\t{tvItemModelMuni.TVText}\t{tvItemModelInf.TVText}\tCould not update new address");
            ////                            //}
            ////                        }

            ////                    }
            ////                }
            ////            }
            ////        }
            ////    }
            ////}

            //richTextBoxStatus.Text = sb.ToString();

            //lblStatus.Text = "Done...";
            //lblStatus.Refresh();
            //Application.DoEvents();

        }

        private void button29_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "New Brunswick", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            List<TVItemModel> tvItemModelSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);

            List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.MWQMSite);

            //List<TVItemModel> tvItemModelMWQMRunList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.MWQMRun);

            FileInfo fi = new FileInfo(@"C:\Users\leblancc\Desktop\NB_Sample_Temp_To_NULL.txt");
            StreamReader sr = fi.OpenText();

            string line = sr.ReadLine();
            int count = 0;
            while (!sr.EndOfStream)
            {
                count++;
                line = sr.ReadLine();

                lblStatus.Text = count + " ----- " + line;
                lblStatus.Refresh();
                Application.DoEvents();

                List<string> strList = line.Split("\t".ToCharArray(), StringSplitOptions.None).ToList();

                if (strList.Count != 7)
                {
                    richTextBoxStatus.AppendText($"Error: Line [{count}]. Does not have 7 items");
                    sr.Close();
                    return;
                }

                string subsector = strList[0];
                string site = "0000".Substring(0, "0000".Length - strList[1].Length) + strList[1];
                DateTime date = new DateTime(int.Parse(strList[2].Substring(0, 4)), int.Parse(strList[2].Substring(5, 2)), int.Parse(strList[2].Substring(8, 2)));
                int SiteTVItemID = int.Parse(strList[6]);

                TVItemModel tvItemModelSS = tvItemModelSSList.Where(c => c.TVText.StartsWith(subsector)).FirstOrDefault();

                if (tvItemModelSS == null)
                {
                    richTextBoxStatus.AppendText($"Error: Line [{count}]. Could not find subsector [{subsector}]");
                    sr.Close();
                    return;
                }

                TVItemModel tvItemModelMWQMSite = (from c in tvItemModelMWQMSiteList
                                                   where c.TVItemID == SiteTVItemID
                                                   select c).FirstOrDefault();

                if (tvItemModelMWQMSite == null)
                {
                    richTextBoxStatus.AppendText($"Error: Line [{count}]. Could not find site [{site}] under subsector [{subsector}]");
                    sr.Close();
                    return;
                }

                List<MWQMSampleModel> mwqmSampleModelList = mwqmSampleService.GetMWQMSampleModelListWithMWQMSiteTVItemIDDB(tvItemModelMWQMSite.TVItemID);

                MWQMSampleModel mwqmSampleModel = (from c in mwqmSampleModelList
                                                   where c.SampleDateTime_Local.Year == date.Year
                                                   && c.SampleDateTime_Local.Month == date.Month
                                                   && c.SampleDateTime_Local.Day == date.Day
                                                   && c.WaterTemp_C == 0
                                                   select c).FirstOrDefault();

                if (mwqmSampleModel != null)
                {
                    mwqmSampleModel.WaterTemp_C = null;

                    MWQMSampleModel mwqmSampleModelRet = mwqmSampleService.PostUpdateMWQMSampleDB(mwqmSampleModel);
                    if (!string.IsNullOrWhiteSpace(mwqmSampleModelRet.Error))
                    {
                        richTextBoxStatus.AppendText($"Error: Line [{count}]. Could not Update temp to null for site [{site}] and date [{date.ToString("yyyy MM dd")}] under subsector [{subsector}]");
                        sr.Close();
                        return;
                    }

                }
            }

            sr.Close();
        }

        private void button30_Click(object sender, EventArgs e)
        {
            return;

            StringBuilder sb = new StringBuilder();

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return;

            TVItemModel tvItemModelCanada = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelRoot.TVItemID, "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelCanada)) return;

            //TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "New Brunswick", TVTypeEnum.Province);
            //if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            //TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Newfoundland and Labrador", TVTypeEnum.Province);
            //if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            //TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Nova Scotia", TVTypeEnum.Province);
            //if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelCanada.TVItemID, "Prince Edward Island", TVTypeEnum.Province);
            if (!CheckModelOK<TVItemModel>(tvItemModelProv)) return;

            List<TVItemModel> tvItemModelSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.Subsector);

            //List<TVItemModel> tvItemModelMWQMSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.MWQMSite);

            //List<TVItemModel> tvItemModelMWQMRunList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelProv.TVItemID, TVTypeEnum.MWQMRun);

            //sb.AppendLine($"\t\t\t\tActive Sites\t");
            //sb.AppendLine($"Locator\tSubsector Name\tLast Run Date\tRouting Last Run Date\tCount Approved\tCount Conditionally Approved\tCount Restricted\tCount Conditionally Restricted\tProhibited\tUnclassified or unknown\ttotal");

            foreach (TVItemModel tvItemModel in tvItemModelSSList)
            {
                string locator = tvItemModel.TVText;
                string name = "";
                if (locator.Contains(" "))
                {
                    name = locator.Substring(locator.IndexOf(" ")).Trim();
                    locator = locator.Substring(0, locator.IndexOf(" ")).Trim();
                }

                lblStatus.Text = locator;
                lblStatus.Refresh();
                Application.DoEvents();

                using (CSSPDBEntities db2 = new CSSPDBEntities())
                {
                    MWQMRun mwqmRun = (from c in db2.MWQMRuns
                                       from t in db2.TVItems
                                       where c.MWQMRunTVItemID == t.TVItemID
                                       && t.TVType == (int)TVTypeEnum.MWQMRun
                                       && t.ParentID == tvItemModel.TVItemID
                                       orderby c.DateTime_Local descending
                                       select c).FirstOrDefault();

                    MWQMRun mwqmRunRoutine = (from c in db2.MWQMRuns
                                              from t in db2.TVItems
                                              where c.MWQMRunTVItemID == t.TVItemID
                                              && t.TVType == (int)TVTypeEnum.MWQMRun
                                              && t.ParentID == tvItemModel.TVItemID
                                              && c.RunSampleType == (int)SampleTypeEnum.Routine
                                              orderby c.DateTime_Local descending
                                              select c).FirstOrDefault();

                    List<MWQMSite> mwqmSiteList = (from c in db2.MWQMSites
                                                   from t in db2.TVItems
                                                   where c.MWQMSiteTVItemID == t.TVItemID
                                                   && t.TVType == (int)TVTypeEnum.MWQMSite
                                                   && t.ParentID == tvItemModel.TVItemID
                                                   && t.IsActive == true
                                                   select c).ToList();

                    if (mwqmRun != null)
                    {
                        string dateStr = mwqmRun.DateTime_Local.ToString("yyyy MM dd");
                        string dateRoutineStr = mwqmRunRoutine.DateTime_Local.ToString("yyyy MM dd");
                        if (mwqmSiteList.Count > 0)
                        {
                            int countA = (from a in mwqmSiteList
                                                 where a.MWQMSiteLatestClassification == (int)ClassificationTypeEnum.Approved
                                                 select a).Count();

                            int countCA = (from a in mwqmSiteList
                                                 where a.MWQMSiteLatestClassification == (int)ClassificationTypeEnum.ConditionallyApproved
                                                 select a).Count();

                            int countR = (from a in mwqmSiteList
                                                 where a.MWQMSiteLatestClassification == (int)ClassificationTypeEnum.Restricted
                                                 select a).Count();

                            int countCR = (from a in mwqmSiteList
                                                 where a.MWQMSiteLatestClassification == (int)ClassificationTypeEnum.ConditionallyRestricted
                                                 select a).Count();

                            int countP = (from a in mwqmSiteList
                                                 where a.MWQMSiteLatestClassification == (int)ClassificationTypeEnum.Prohibited
                                                 select a).Count();

                            int countU = (from a in mwqmSiteList
                                          where a.MWQMSiteLatestClassification == (int)ClassificationTypeEnum.Error
                                          select a).Count();


                            sb.AppendLine($"{locator}\t{name}\t{dateStr}\t{dateRoutineStr}\t{countA}\t{countCA}\t{countR}\t{countCR}\t{countP}\t{countU}\t{mwqmSiteList.Count}");
                        }
                        else
                        {
                            sb.AppendLine($"{locator}\t{name}\t{dateStr}\t{dateRoutineStr}");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"{locator}\t{name}");
                    }
                }
            }

            lblStatus.Text = "done...";

            richTextBoxStatus.Text = sb.ToString();
        }


<<<<<<< HEAD
=======
        private void button37_Click(object sender,  EventArgs e)
        {
            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

            string ProvInit = "";
            List<string> ProvInitList = new List<string>()
            {
               "NL" //"NB", "PE", "BC", "NL", "NS", "QC",
            };
            List<string> ProvList = new List<string>()
            {
                  "Newfoundland and Labrador" //"New Brunswick", "Prince Edward Island", "British Columbia", "Newfoundland and Labrador", "Nova Scotia", "Québec",
            };

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!string.IsNullOrEmpty(tvItemModelRoot.Error))
            {
                return;
            }

            foreach (string prov in ProvList)
            {

                TVItemModel tvItemModelProv = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelRoot.TVItemID, prov, TVTypeEnum.Province);
                for (int i = 0, countProv = ProvList.Count; i < countProv; i++)
                {
                    if (ProvList[i] == tvItemModelProv.TVText)
                    {
                        ProvInit = ProvInitList[i];
                        break;
                    }
                }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Province\tMunicipality\tWWPTorLS\tInfrastructure_Name\tLatitude\tLongitude\tOutfall_Lat\tOutfall_Lng");

                using (CSSPDBEntities db = new CSSPDBEntities())
                {
                    var tvItemMuniList = (from t in db.TVItems
                                          from tl in db.TVItemLanguages
                                          where t.TVItemID == tl.TVItemID
                                          && tl.Language == (int)LanguageEnum.en
                                          && t.TVPath.StartsWith(tvItemModelProv.TVPath + "p")
                                          && t.TVType == (int)TVTypeEnum.Municipality
                                          orderby tl.TVText
                                          select new { t, tl }).ToList();

                    foreach(var tvItemMuni in tvItemMuniList)
                    {
                        var InfList = (from t in db.TVItems
                                       from tl in db.TVItemLanguages
                                       from inf in db.Infrastructures
                                       where t.TVItemID == tl.TVItemID
                                       && t.TVItemID == inf.InfrastructureTVItemID
                                       && t.TVPath.StartsWith(tvItemMuni.t.TVPath + "p")
                                       && t.TVType == (int)TVTypeEnum.Infrastructure
                                       && tl.Language == (int)LanguageEnum.en
                                       select new { t, tl, inf }).ToList();

                        foreach(var infrastructure in InfList)
                        {
                            var MapInfoList = (from mi in db.MapInfos
                                               from mip in db.MapInfoPoints
                                               where mi.MapInfoID == mip.MapInfoID
                                               && mi.MapInfoDrawType == (int)MapInfoDrawTypeEnum.Point
                                               && mi.TVItemID == infrastructure.t.TVItemID
                                               select new { mi, mip }).ToList();

                            string provInit = ProvInit;
                            string municipality = tvItemMuni.tl.TVText;
                            string wwtpOrLS = "";
                            string infName = "";
                            string latitude = "";
                            string longitude = "";
                            string outfall_lat = "";
                            string outfall_lng = "";

                            infName = infrastructure.tl.TVText;

                            if (infrastructure.inf.InfrastructureType == (int)InfrastructureTypeEnum.WWTP)
                            {
                                wwtpOrLS = "WWTP";

                                var mapInfo = MapInfoList.Where(c => c.mi.TVType == (int)TVTypeEnum.WasteWaterTreatmentPlant).FirstOrDefault();
                                if (mapInfo != null)
                                {
                                    latitude = mapInfo.mip.Lat.ToString();
                                    longitude = mapInfo.mip.Lng.ToString();
                                }

                                var mapInfoOutfall = MapInfoList.Where(c => c.mi.TVType == (int)TVTypeEnum.Outfall).FirstOrDefault();
                                if (mapInfoOutfall != null)
                                {
                                    outfall_lat = mapInfoOutfall.mip.Lat.ToString();
                                    outfall_lng = mapInfoOutfall.mip.Lng.ToString();
                                }
                            }
                            else if (infrastructure.inf.InfrastructureType == (int)InfrastructureTypeEnum.LiftStation)
                            {
                                wwtpOrLS = "LS";

                                var mapInfo = MapInfoList.Where(c => c.mi.TVType == (int)TVTypeEnum.LiftStation).FirstOrDefault();
                                if (mapInfo != null)
                                {
                                    latitude = mapInfo.mip.Lat.ToString();
                                    longitude = mapInfo.mip.Lng.ToString();
                                }

                                var mapInfoOutfall = MapInfoList.Where(c => c.mi.TVType == (int)TVTypeEnum.Outfall).FirstOrDefault();
                                if (mapInfoOutfall != null)
                                {
                                    outfall_lat = mapInfoOutfall.mip.Lat.ToString();
                                    outfall_lng = mapInfoOutfall.mip.Lng.ToString();
                                }
                            }
                            else if (infrastructure.inf.InfrastructureType == (int)InfrastructureTypeEnum.LineOverflow)
                            {
                                wwtpOrLS = "Line Overflow";

                                var mapInfo = MapInfoList.Where(c => c.mi.TVType == (int)TVTypeEnum.LineOverflow).FirstOrDefault();
                                if (mapInfo != null)
                                {
                                    latitude = mapInfo.mip.Lat.ToString();
                                    longitude = mapInfo.mip.Lng.ToString();
                                }

                                var mapInfoOutfall = MapInfoList.Where(c => c.mi.TVType == (int)TVTypeEnum.Outfall).FirstOrDefault();
                                if (mapInfoOutfall != null)
                                {
                                    outfall_lat = mapInfoOutfall.mip.Lat.ToString();
                                    outfall_lng = mapInfoOutfall.mip.Lng.ToString();
                                }
                            }
                            else
                            {
                                continue;
                            }

                            sb.AppendLine($"{provInit}\t{municipality}\t{wwtpOrLS}\t{infName}\t{latitude}\t{longitude}\t{outfall_lat}\t{outfall_lng}");

                            richTextBoxStatus.Text = sb.ToString();
                        }
                    }
                }

                lblStatus.Text = "done ... " + ProvInit;
            }

        }

>>>>>>> 32a91c17a976d96ff3aee9c71b6f9c2e0a2ad180
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

    public class SiteLatLngUse
    {
        public string Site { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public bool UseForOpenData { get; set; }
    }

    public class LetterColorName
    {
        public string Letter { get; set; }
        public string Color { get; set; }
        public string Name { get; set; }
    }


    public class StationQC
    {
        public string secteur { get; set; }
        public int id_geo_station_p { get; set; }
        public int station { get; set; }
        public string type_station { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public string status { get; set; }
    }

    public class SampleQC
    {
        public int id_geo_station_p { get; set; }
        public int id_tournee { get; set; }
        public int? cf { get; set; }
        public DateTime? hre_echantillonnage { get; set; }
        public double? prof { get; set; }
        public double? sal { get; set; }
        public double? temp { get; set; }
        public double? ph { get; set; }
        public bool? diffusable { get; set; }
        public string commentaire { get; set; }

    }

    public class SectorQC
    {
        public string secteur { get; set; }
        public string secteur_nom { get; set; }
        public string secteur_nom_a { get; set; }
    }

    public class SubsectorAndSiteList
    {
        public SubsectorAndSiteList()
        {
            SiteList = new List<string>();
        }

        public string Subsector { get; set; }
        public List<string> SiteList { get; set; }
    }

    public class SubsectorPolClass
    {
        public SubsectorPolClass()
        {
            CoordList = new List<Coord>();
        }

        public string Subsector { get; set; }
        public string StyleURL { get; set; }
        public string ClassCode { get; set; }
        public List<Coord> CoordList { get; set; }
        public Coord Centroid { get; set; }
    }

}
