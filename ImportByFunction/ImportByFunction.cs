using CSSPEnumsDLL.Enums;
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
            string StartTextSubsector = "NS-";
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
                if (tvItemModel.TVText.StartsWith("NS-15-010-002"))
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
        }

        private void button16_Click(object sender, EventArgs e) // button climate2
        {
            string StartTextSubsector = "NS-";
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
        }

        private void button17_Click(object sender, EventArgs e) // button climate3
        {
            string StartTextSubsector = "NS-";
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
            DirectoryInfo di = new DirectoryInfo(@"C:\___Sam");

            if (di.Exists)
            {
                List<FileInfo> fiList = di.GetFiles().ToList();

                //int count = 0;
                foreach (FileInfo fi in fiList)
                {
                    lblStatus.Text = fi.FullName;
                    lblStatus.Refresh();
                    Application.DoEvents();


                    StreamReader sr = fi.OpenText();
                    //if (count == 0)
                    //{
                    //    string ret = sr.ReadToEnd();
                    //    sb.AppendLine(ret);
                    //    count += 1;
                    //}
                    //else
                    //{
                    bool skip = true;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (!skip)
                        {
                            if (string.IsNullOrWhiteSpace(line.Trim()))
                            {
                                break;
                            }
                            sb.AppendLine(line);
                        }

                        if (line.StartsWith(@"""Date/Time") && skip == true)
                        {
                            skip = false;
                        }

                    }
                    //}
                    sr.Close();
                }
            }

            StreamWriter sw = new StreamWriter(@"C:\___Sam\_StStephenAll.csv");
            sw.Write(sb.ToString());
            sw.Close();
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
                                    catch (Exception ex)
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
                                    catch (Exception ex)
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
                                    catch (Exception ex)
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
                                            catch (Exception ex)
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
                                        catch (Exception ex)
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
                                            catch (Exception ex)
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
                                        catch (Exception ex)
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

        private void ButCompareQCAndCSSP_Click(object sender, EventArgs e)
        {
            //TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            //MWQMSampleService mwqmSampleService = new MWQMSampleService(LanguageEnum.en, user);

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



            //// calculate the total QC samples and total QC samples with UseForOpenData

            //int TotalCount = 0;
            //int TotalCount2 = 0;

            //using (CSSPDBEntities db2 = new CSSPDBEntities())
            //{
            //    foreach (TVItemModel tvItemModel in tvItemModelSubsectorList)
            //    {
            //        lblStatus.Text = tvItemModel.TVText;
            //        lblStatus.Refresh();
            //        Application.DoEvents();

            //        int count = (from s in db2.MWQMSamples
            //                     from c in db2.MWQMSites
            //                     from t in db2.TVItems
            //                     where c.MWQMSiteTVItemID == t.TVItemID
            //                     && c.MWQMSiteTVItemID == s.MWQMSiteTVItemID
            //                     && t.ParentID == tvItemModel.TVItemID
            //                     && t.TVType == (int)TVTypeEnum.MWQMSite
            //                     select s).Count();

            //        int count2 = (from s in db2.MWQMSamples
            //                      from c in db2.MWQMSites
            //                      from t in db2.TVItems
            //                      where c.MWQMSiteTVItemID == t.TVItemID
            //                      && c.MWQMSiteTVItemID == s.MWQMSiteTVItemID
            //                      && t.ParentID == tvItemModel.TVItemID
            //                      && t.TVType == (int)TVTypeEnum.MWQMSite
            //                      && s.UseForOpenData == true
            //                      select s).Count();

            //        richTextBoxStatus.AppendText($"{tvItemModel.TVText} -- [{count}] -- [{count2}]\r\n");
            //        TotalCount += count;
            //        TotalCount2 += count2;
            //    }

            //    richTextBoxStatus.AppendText($"Total Count -- [{TotalCount}] -- [{TotalCount2}]\r\n");
            //}


            // deleting the MWQMSite from CSSPWebTools that should not exist

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            TVItemLanguageService tvItemLanguageService = new TVItemLanguageService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);
            MWQMSiteService mwqmSiteService = new MWQMSiteService(LanguageEnum.en, user);
            MWQMRunService mwqmRunService = new MWQMRunService(LanguageEnum.en, user);

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

            #region remove all sectors and under info that are not in QC DB
            /// -----------------------------------------------------------------
            /// ------ remove all sectors and under info that are not in QC DB --
            /// -----------------------------------------------------------------
            //using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            //{
            //    var geo_stations_pList = (from c in dbQC.geo_stations_p
            //                              where c.secteur != null
            //                              && c.secteur.StartsWith("M") == false
            //                              orderby c.secteur, c.station
            //                              select new { c.id_geo_station_p, c.station, c.secteur }).ToList();

            //    List<string> AreaList = (from c in geo_stations_pList
            //                             select c.secteur).Distinct().ToList();

            //    List<string> SectorList = new List<string>();
            //    foreach (string s in AreaList)
            //    {
            //        if (s.Length > 3)
            //        {
            //            string sector = s.Substring(0, 4);

            //            if (!SectorList.Contains(sector))
            //            {
            //                SectorList.Add(sector);
            //                //richTextBoxStatus.AppendText($"{sector}\r\n");
            //            }
            //        }
            //    }

            //    List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Sector);

            //    foreach (TVItemModel tvItemModel in tvItemModelSectorList)
            //    {
            //        string sector = tvItemModel.TVText;
            //        if (sector.Contains(" "))
            //        {
            //            sector = sector.Substring(0, sector.IndexOf(" "));
            //        }

            //        if (!SectorList.Contains(sector))
            //        {
            //            int lsefjlij = 423;
            //            if (sector != "MS")
            //            {
            //                richTextBoxStatus.AppendText($"{sector}\r\n");
            //                List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModel.TVItemID, TVTypeEnum.Subsector);

            //                foreach (TVItemModel tvItemModelss in tvItemModelSubsectorList)
            //                {
            //                    List<TVItemModel> tvItemModelSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelss.TVItemID, TVTypeEnum.MWQMSite);
            //                    List<TVItemModel> tvItemModelRunList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelss.TVItemID, TVTypeEnum.MWQMRun);
            //                    List<TVItemModel> tvItemModelPSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelss.TVItemID, TVTypeEnum.PolSourceSite);

            //                    using (CSSPDBEntities db2 = new CSSPDBEntities())
            //                    {
            //                        // Deleting TVItemStats of MWQMSites
            //                        foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
            //                        {
            //                            List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
            //                                                               where c.TVItemID == tvItemModelSite.TVItemID
            //                                                               select c).ToList();

            //                            if (tvItemStatList.Count > 0)
            //                            {
            //                                db2.TVItemStats.RemoveRange(tvItemStatList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMSites");
            //                                }
            //                            }
            //                        }

            //                        // Deleting TVItemStats of MWQMRuns
            //                        foreach (TVItemModel tvItemModelRun in tvItemModelRunList)
            //                        {
            //                            List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
            //                                                               where c.TVItemID == tvItemModelRun.TVItemID
            //                                                               select c).ToList();

            //                            if (tvItemStatList.Count > 0)
            //                            {
            //                                db2.TVItemStats.RemoveRange(tvItemStatList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMRuns");
            //                                }
            //                            }
            //                        }

            //                        // Deleting TVItemStats of PSS
            //                        foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
            //                        {
            //                            List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
            //                                                               where c.TVItemID == tvItemModelPSS.TVItemID
            //                                                               select c).ToList();

            //                            if (tvItemStatList.Count > 0)
            //                            {
            //                                db2.TVItemStats.RemoveRange(tvItemStatList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMSites");
            //                                }
            //                            }
            //                        }

            //                        // Deleting MWQMSamples
            //                        foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
            //                        {
            //                            List<MWQMSample> mwqmSampleList = (from c in db2.MWQMSamples
            //                                                               where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
            //                                                               select c).ToList();

            //                            if (mwqmSampleList.Count > 0)
            //                            {
            //                                db2.MWQMSamples.RemoveRange(mwqmSampleList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete Sample");
            //                                }
            //                            }
            //                        }


            //                        // Deleting MapInfos for MWQMSites
            //                        foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
            //                        {
            //                            List<MapInfo> MapInfoList = (from c in db2.MapInfos
            //                                                         where c.TVItemID == tvItemModelSite.TVItemID
            //                                                         select c).ToList();


            //                            if (MapInfoList.Count > 0)
            //                            {
            //                                db2.MapInfos.RemoveRange(MapInfoList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete MapInfos for MWQMSites");
            //                                }
            //                            }
            //                        }

            //                        // Deleting MapInfos for PSS
            //                        foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
            //                        {
            //                            List<MapInfo> MapInfoList = (from c in db2.MapInfos
            //                                                         where c.TVItemID == tvItemModelPSS.TVItemID
            //                                                         select c).ToList();


            //                            if (MapInfoList.Count > 0)
            //                            {
            //                                db2.MapInfos.RemoveRange(MapInfoList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete MapInfos for PSS");
            //                                }
            //                            }
            //                        }

            //                        // Deleting MWQMSites
            //                        foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
            //                        {
            //                            List<MWQMSite> mwqmSiteList = (from c in db2.MWQMSites
            //                                                           where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
            //                                                           select c).ToList();

            //                            if (mwqmSiteList.Count > 0)
            //                            {
            //                                db2.MWQMSites.RemoveRange(mwqmSiteList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
            //                                }
            //                            }
            //                        }

            //                        // Deleting TVItems for MWQMSites
            //                        foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
            //                        {
            //                            List<TVItem> TVItemList = (from c in db2.TVItems
            //                                                       where c.TVItemID == tvItemModelSite.TVItemID
            //                                                       select c).ToList();

            //                            if (TVItemList.Count > 0)
            //                            {
            //                                db2.TVItems.RemoveRange(TVItemList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
            //                                }
            //                            }
            //                        }

            //                        // Deleting PSS
            //                        foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
            //                        {
            //                            List<PolSourceSite> polSourceSiteList = (from c in db2.PolSourceSites
            //                                                                     where c.PolSourceSiteTVItemID == tvItemModelPSS.TVItemID
            //                                                                     select c).ToList();

            //                            if (polSourceSiteList.Count > 0)
            //                            {
            //                                db2.PolSourceSites.RemoveRange(polSourceSiteList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItems for PSS");
            //                                }
            //                            }
            //                        }

            //                        // Deleting TVItems for PSS
            //                        foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
            //                        {
            //                            List<TVItem> TVItemList = (from c in db2.TVItems
            //                                                       where c.TVItemID == tvItemModelPSS.TVItemID
            //                                                       select c).ToList();

            //                            if (TVItemList.Count > 0)
            //                            {
            //                                db2.TVItems.RemoveRange(TVItemList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItems for PSS");
            //                                }
            //                            }
            //                        }

            //                        // Deleting MWQMRuns
            //                        foreach (TVItemModel tvItemModelRun in tvItemModelRunList)
            //                        {
            //                            List<MWQMRun> mwqmRunList = (from c in db2.MWQMRuns
            //                                                         where c.MWQMRunTVItemID == tvItemModelRun.TVItemID
            //                                                         select c).ToList();

            //                            if (mwqmRunList.Count > 0)
            //                            {
            //                                db2.MWQMRuns.RemoveRange(mwqmRunList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
            //                                }
            //                            }
            //                        }

            //                        // Deleting TVItems for MWQMRuns
            //                        foreach (TVItemModel tvItemModelRun in tvItemModelRunList)
            //                        {
            //                            List<TVItem> TVItemList = (from c in db2.TVItems
            //                                                       where c.TVItemID == tvItemModelRun.TVItemID
            //                                                       select c).ToList();

            //                            if (TVItemList.Count > 0)
            //                            {
            //                                db2.TVItems.RemoveRange(TVItemList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMRuns");
            //                                }
            //                            }
            //                        }

            //                        // Deleting TVItemStats of TVItemModelss
            //                        List<TVItemStat> tvItemStatList3 = (from c in db2.TVItemStats
            //                                                            where c.TVItemID == tvItemModelss.TVItemID
            //                                                            select c).ToList();

            //                        if (tvItemStatList3.Count > 0)
            //                        {
            //                            db2.TVItemStats.RemoveRange(tvItemStatList3);

            //                            try
            //                            {
            //                                db2.SaveChanges();
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                richTextBoxStatus.AppendText($"Could not delete TVItemStats of TVItemModelss");
            //                            }
            //                        }

            //                        // Deleting MapInfos of TVItemModelss
            //                        List<MapInfo> MapInfoList3 = (from c in db2.MapInfos
            //                                                      where c.TVItemID == tvItemModelss.TVItemID
            //                                                      select c).ToList();


            //                        if (MapInfoList3.Count > 0)
            //                        {
            //                            db2.MapInfos.RemoveRange(MapInfoList3);

            //                            try
            //                            {
            //                                db2.SaveChanges();
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                richTextBoxStatus.AppendText($"Could not delete MapInfos of TVItemModelss");
            //                            }
            //                        }

            //                        // Deleting MWQMSubsectors for TVItemModelss
            //                        List<MWQMSubsector> mwqmSubsectorList = (from c in db2.MWQMSubsectors
            //                                                                 where c.MWQMSubsectorTVItemID == tvItemModelss.TVItemID
            //                                                                 select c).ToList();

            //                        if (mwqmSubsectorList.Count > 0)
            //                        {
            //                            db2.MWQMSubsectors.RemoveRange(mwqmSubsectorList);

            //                            try
            //                            {
            //                                db2.SaveChanges();
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                richTextBoxStatus.AppendText($"Could not delete MWQMSubsectors for TVItemModelss");
            //                            }
            //                        }

            //                        // Deleting UseOfSites for TVItemModelss
            //                        List<UseOfSite> UseOfSiteList = (from c in db2.UseOfSites
            //                                                                 where c.SubsectorTVItemID == tvItemModelss.TVItemID
            //                                                                 select c).ToList();

            //                        if (UseOfSiteList.Count > 0)
            //                        {
            //                            db2.UseOfSites.RemoveRange(UseOfSiteList);

            //                            try
            //                            {
            //                                db2.SaveChanges();
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                richTextBoxStatus.AppendText($"Could not delete UseOfSites for TVItemModelss");
            //                            }
            //                        }


            //                        // Deleting TVItems for TVItemModelss
            //                        List<TVItem> TVItemList3 = (from c in db2.TVItems
            //                                                    where c.TVItemID == tvItemModelss.TVItemID
            //                                                    select c).ToList();

            //                        if (TVItemList3.Count > 0)
            //                        {
            //                            db2.TVItems.RemoveRange(TVItemList3);

            //                            try
            //                            {
            //                                db2.SaveChanges();
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                richTextBoxStatus.AppendText($"Could not delete TVItems for TVItemModelss");
            //                            }
            //                        }
            //                    }
            //                }

            //                // now we should be able to remove the sector
            //                using (CSSPDBEntities db2 = new CSSPDBEntities())
            //                {
            //                    // Deleting TVItemStats of tvItemModel
            //                    List<TVItemStat> tvItemStatList2 = (from c in db2.TVItemStats
            //                                                        where c.TVItemID == tvItemModel.TVItemID
            //                                                        select c).ToList();

            //                    if (tvItemStatList2.Count > 0)
            //                    {
            //                        db2.TVItemStats.RemoveRange(tvItemStatList2);

            //                        try
            //                        {
            //                            db2.SaveChanges();
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            richTextBoxStatus.AppendText($"Could not delete TVItemStats of tvItemModel");
            //                        }
            //                    }

            //                    // Deleting MapInfos of tvItemModel
            //                    List<MapInfo> MapInfoList2 = (from c in db2.MapInfos
            //                                                  where c.TVItemID == tvItemModel.TVItemID
            //                                                  select c).ToList();


            //                    if (MapInfoList2.Count > 0)
            //                    {
            //                        db2.MapInfos.RemoveRange(MapInfoList2);

            //                        try
            //                        {
            //                            db2.SaveChanges();
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            richTextBoxStatus.AppendText($"Could not delete MapInfos of tvItemModel");
            //                        }
            //                    }
            //                    // Deleting TVItems for tvItemModel
            //                    List<TVItem> TVItemList2 = (from c in db2.TVItems
            //                                                where c.TVItemID == tvItemModel.TVItemID
            //                                                select c).ToList();

            //                    if (TVItemList2.Count > 0)
            //                    {
            //                        db2.TVItems.RemoveRange(TVItemList2);

            //                        try
            //                        {
            //                            db2.SaveChanges();
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            richTextBoxStatus.AppendText($"Could not delete TVItems for tvItemModel");
            //                        }
            //                    }
            //                }

            //            }
            //        }
            //    }
            //    lblStatus.Text = "done...";
            //}

            #endregion remove all sectors and under info that are not in QC DB

            #region Add all new sectors and under info that are not in QC DB
            ///// -----------------------------------------------------------------
            ///// ------ Add all new sectors and under info that are not in QC DB --
            ///// -----------------------------------------------------------------

            //List<TVItemModel> tvItemModelAreaList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Area);
            //List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Sector);

            //using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            //{
            //    var geo_stations_pList = (from c in dbQC.geo_stations_p
            //                              where c.secteur != null
            //                              && c.secteur.StartsWith("M") == false
            //                              orderby c.secteur, c.station
            //                              select new { c.id_geo_station_p, c.station, c.secteur }).ToList();

            //    List<string> SubsectorListQC = (from c in geo_stations_pList
            //                                    select c.secteur).Distinct().ToList();

            //    List<string> SectorList = new List<string>();
            //    foreach (string s in SubsectorListQC)
            //    {
            //        if (s.Length > 3)
            //        {
            //            string sector = s.Substring(0, 4);

            //            if (!SectorList.Contains(sector))
            //            {
            //                SectorList.Add(sector);
            //            }
            //        }
            //    }


            //    richTextBoxStatus.AppendText($"------------------- TVItemModel Sector ------------\r\n");
            //    foreach (TVItemModel tvItemModel in tvItemModelSectorList)
            //    {
            //        string sector = tvItemModel.TVText;
            //        if (sector.Contains(" "))
            //        {
            //            sector = sector.Substring(0, sector.IndexOf(" "));
            //        }

            //        if (!SectorList.Contains(sector))
            //        {
            //            richTextBoxStatus.AppendText($"{sector} does not exist\t should delete it\r\n");
            //        }
            //    }

            //    richTextBoxStatus.AppendText($"------------------- QC Sector ------------\r\n");
            //    foreach (string sector in SectorList)
            //    {
            //        if (!tvItemModelSectorList.Where(c => c.TVText.StartsWith(sector + " ")).Any())
            //        {
            //            richTextBoxStatus.AppendText($"{sector} does not exist\t should create it\r\n");

            //            // add tvitems and tvitemlanguages under the proper Area
            //            // 

            //            string AreaText = sector.Substring(0, 1);

            //            TVItemModel tvItemModelArea = tvItemModelAreaList.Where(c => c.TVText.Contains(AreaText + " ")).FirstOrDefault();

            //            if (tvItemModelArea != null)
            //            {
            //                TVItemModel tvItemModelSector = tvItemService.PostAddChildTVItemDB(tvItemModelArea.TVItemID, sector + " (vide)", TVTypeEnum.Sector);
            //                if (!string.IsNullOrWhiteSpace(tvItemModelSector.Error))
            //                {
            //                    richTextBoxStatus.AppendText($"{tvItemModelSector.Error}\r\n");
            //                    return;
            //                }

            //                var geo_stations_pList2 = (from c in dbQC.geo_stations_p
            //                                           where c.secteur != null
            //                                           && c.secteur.StartsWith("M") == false
            //                                           && c.secteur.StartsWith(sector)
            //                                           && c.x != null
            //                                           && c.y != null
            //                                           select new { c.x, c.y }).ToList();

            //                float MinLat = (float)((from c in geo_stations_pList2
            //                                        select c.y).Min());

            //                float MaxLat = (float)((from c in geo_stations_pList2
            //                                        select c.y).Max());

            //                float MinLng = (float)((from c in geo_stations_pList2
            //                                        select c.x).Min());

            //                float MaxLng = (float)((from c in geo_stations_pList2
            //                                        select c.x).Max());

            //                List<Coord> coordList = new List<Coord>()
            //                {
            //                    new Coord() { Lat = (MaxLat + MinLat)/2.0f, Lng = (MaxLng + MinLng)/2.0f, Ordinal = 0 },
            //                };

            //                MapInfoModel mapInfoModel = tvItemService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Sector, tvItemModelSector.TVItemID);
            //                if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
            //                {
            //                    richTextBoxStatus.AppendText($"{mapInfoModel.Error}\r\n");
            //                    return;
            //                }

            //                coordList = new List<Coord>()
            //                {
            //                    new Coord() { Lat = MinLat, Lng = MinLng, Ordinal = 0 },
            //                    new Coord() { Lat = MinLat, Lng = MaxLng, Ordinal = 1 },
            //                    new Coord() { Lat = MaxLat, Lng = MaxLng, Ordinal = 2 },
            //                    new Coord() { Lat = MaxLat, Lng = MinLng, Ordinal = 3 },
            //                    new Coord() { Lat = MinLat, Lng = MinLng, Ordinal = 4 },
            //                };

            //                mapInfoModel = tvItemService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Polygon, TVTypeEnum.Sector, tvItemModelSector.TVItemID);
            //                if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
            //                {
            //                    richTextBoxStatus.AppendText($"{mapInfoModel.Error}\r\n");
            //                    return;
            //                }
            //            }
            //        }
            //    }

            //    lblStatus.Text = "done...";
            #endregion Add all new sectors and under info that are not in QC DB

            #region Remove all subsectors and under info that are not in QC DB
            ///// -----------------------------------------------------------------
            ///// ------ Remove all subsectors and under info that are not in QC DB --
            ///// -----------------------------------------------------------------

            //List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Sector);
            //List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);

            //using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            //{
            //    var geo_stations_pList = (from c in dbQC.geo_stations_p
            //                              where c.secteur != null
            //                              && c.secteur.StartsWith("M") == false
            //                              orderby c.secteur, c.station
            //                              select new { c.id_geo_station_p, c.station, c.secteur }).ToList();

            //    List<string> SubsectorListQC = (from c in geo_stations_pList
            //                                    select c.secteur).Distinct().ToList();

            //    List<string> SubsectorList = new List<string>();
            //    foreach (string s in SubsectorListQC)
            //    {
            //        if (s.Length > 3)
            //        {
            //            if (!SubsectorList.Contains(s))
            //            {
            //                SubsectorList.Add(s);
            //            }
            //        }
            //    }


            //    richTextBoxStatus.AppendText($"------------------- TVItemModel Subsector ------------\r\n");
            //    foreach (TVItemModel tvItemModel in tvItemModelSubsectorList)
            //    {
            //        string subsector = tvItemModel.TVText;
            //        if (subsector.Contains(" "))
            //        {
            //            subsector = subsector.Substring(0, subsector.IndexOf(" "));
            //        }

            //        lblStatus.Text = subsector;
            //        lblStatus.Refresh();
            //        Application.DoEvents();

            //        if (!SubsectorList.Contains(subsector))
            //        {
            //            richTextBoxStatus.AppendText($"{subsector} does not exist\t should delete it\r\n");

            //            if (!SubsectorList.Contains(subsector))
            //            {
            //                if (!subsector.StartsWith("MS"))
            //                {
            //                    richTextBoxStatus.AppendText($"{subsector}\r\n");
            //                    List<TVItemModel> tvItemModelSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModel.TVItemID, TVTypeEnum.MWQMSite);
            //                    List<TVItemModel> tvItemModelRunList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModel.TVItemID, TVTypeEnum.MWQMRun);
            //                    List<TVItemModel> tvItemModelPSSList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModel.TVItemID, TVTypeEnum.PolSourceSite);

            //                    using (CSSPDBEntities db2 = new CSSPDBEntities())
            //                    {
            //                        // Deleting TVItemStats of MWQMSites
            //                        foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
            //                        {
            //                            List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
            //                                                               where c.TVItemID == tvItemModelSite.TVItemID
            //                                                               select c).ToList();

            //                            if (tvItemStatList.Count > 0)
            //                            {
            //                                db2.TVItemStats.RemoveRange(tvItemStatList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMSites");
            //                                    return;
            //                                }
            //                            }
            //                        }

            //                        // Deleting TVItemStats of MWQMRuns
            //                        foreach (TVItemModel tvItemModelRun in tvItemModelRunList)
            //                        {
            //                            List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
            //                                                               where c.TVItemID == tvItemModelRun.TVItemID
            //                                                               select c).ToList();

            //                            if (tvItemStatList.Count > 0)
            //                            {
            //                                db2.TVItemStats.RemoveRange(tvItemStatList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMRuns");
            //                                    return;
            //                                }
            //                            }
            //                        }

            //                        // Deleting TVItemStats of PSS
            //                        foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
            //                        {
            //                            List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
            //                                                               where c.TVItemID == tvItemModelPSS.TVItemID
            //                                                               select c).ToList();

            //                            if (tvItemStatList.Count > 0)
            //                            {
            //                                db2.TVItemStats.RemoveRange(tvItemStatList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMSites");
            //                                    return;
            //                                }
            //                            }
            //                        }

            //                        // Deleting MWQMSamples
            //                        foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
            //                        {
            //                            lblStatus.Text = subsector + " -- " + tvItemModelSite.TVText + " samples delete";
            //                            lblStatus.Refresh();
            //                            Application.DoEvents();

            //                            List<MWQMSample> mwqmSampleList = (from c in db2.MWQMSamples
            //                                                               where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
            //                                                               select c).ToList();

            //                            if (mwqmSampleList.Count > 0)
            //                            {
            //                                db2.MWQMSamples.RemoveRange(mwqmSampleList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete Sample");
            //                                    return;
            //                                }
            //                            }
            //                        }


            //                        // Deleting MapInfos for MWQMSites
            //                        foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
            //                        {
            //                            List<MapInfo> MapInfoList = (from c in db2.MapInfos
            //                                                         where c.TVItemID == tvItemModelSite.TVItemID
            //                                                         select c).ToList();


            //                            if (MapInfoList.Count > 0)
            //                            {
            //                                db2.MapInfos.RemoveRange(MapInfoList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete MapInfos for MWQMSites");
            //                                    return;
            //                                }
            //                            }
            //                        }

            //                        // Deleting MapInfos for PSS
            //                        foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
            //                        {
            //                            List<MapInfo> MapInfoList = (from c in db2.MapInfos
            //                                                         where c.TVItemID == tvItemModelPSS.TVItemID
            //                                                         select c).ToList();


            //                            if (MapInfoList.Count > 0)
            //                            {
            //                                db2.MapInfos.RemoveRange(MapInfoList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete MapInfos for PSS");
            //                                    return;
            //                                }
            //                            }
            //                        }

            //                        // Deleting MWQMSites
            //                        foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
            //                        {
            //                            List<MWQMSite> mwqmSiteList = (from c in db2.MWQMSites
            //                                                           where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
            //                                                           select c).ToList();

            //                            if (mwqmSiteList.Count > 0)
            //                            {
            //                                db2.MWQMSites.RemoveRange(mwqmSiteList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
            //                                    return;
            //                                }
            //                            }
            //                        }

            //                        // Deleting TVItems for MWQMSites
            //                        foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
            //                        {
            //                            List<TVItem> TVItemList = (from c in db2.TVItems
            //                                                       where c.TVItemID == tvItemModelSite.TVItemID
            //                                                       select c).ToList();

            //                            if (TVItemList.Count > 0)
            //                            {
            //                                db2.TVItems.RemoveRange(TVItemList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
            //                                    return;
            //                                }
            //                            }
            //                        }

            //                        // Deleting PSS
            //                        foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
            //                        {
            //                            List<PolSourceSite> polSourceSiteList = (from c in db2.PolSourceSites
            //                                                                     where c.PolSourceSiteTVItemID == tvItemModelPSS.TVItemID
            //                                                                     select c).ToList();

            //                            if (polSourceSiteList.Count > 0)
            //                            {
            //                                db2.PolSourceSites.RemoveRange(polSourceSiteList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItems for PSS");
            //                                    return;
            //                                }
            //                            }
            //                        }

            //                        // Deleting TVItems for PSS
            //                        foreach (TVItemModel tvItemModelPSS in tvItemModelPSSList)
            //                        {
            //                            List<TVItem> TVItemList = (from c in db2.TVItems
            //                                                       where c.TVItemID == tvItemModelPSS.TVItemID
            //                                                       select c).ToList();

            //                            if (TVItemList.Count > 0)
            //                            {
            //                                db2.TVItems.RemoveRange(TVItemList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItems for PSS");
            //                                    return;
            //                                }
            //                            }
            //                        }

            //                        // Deleting MWQMRuns
            //                        foreach (TVItemModel tvItemModelRun in tvItemModelRunList)
            //                        {
            //                            List<MWQMRun> mwqmRunList = (from c in db2.MWQMRuns
            //                                                         where c.MWQMRunTVItemID == tvItemModelRun.TVItemID
            //                                                         select c).ToList();

            //                            if (mwqmRunList.Count > 0)
            //                            {
            //                                db2.MWQMRuns.RemoveRange(mwqmRunList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
            //                                    return;
            //                                }
            //                            }
            //                        }

            //                        // Deleting TVItems for MWQMRuns
            //                        foreach (TVItemModel tvItemModelRun in tvItemModelRunList)
            //                        {
            //                            List<TVItem> TVItemList = (from c in db2.TVItems
            //                                                       where c.TVItemID == tvItemModelRun.TVItemID
            //                                                       select c).ToList();

            //                            if (TVItemList.Count > 0)
            //                            {
            //                                db2.TVItems.RemoveRange(TVItemList);

            //                                try
            //                                {
            //                                    db2.SaveChanges();
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMRuns");
            //                                    return;
            //                                }
            //                            }
            //                        }

            //                        // Deleting TVItemStats of TVItemModel
            //                        List<TVItemStat> tvItemStatList3 = (from c in db2.TVItemStats
            //                                                            where c.TVItemID == tvItemModel.TVItemID
            //                                                            select c).ToList();

            //                        if (tvItemStatList3.Count > 0)
            //                        {
            //                            db2.TVItemStats.RemoveRange(tvItemStatList3);

            //                            try
            //                            {
            //                                db2.SaveChanges();
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                richTextBoxStatus.AppendText($"Could not delete TVItemStats of TVItemModel");
            //                                return;
            //                            }
            //                        }

            //                        // Deleting MapInfos of TVItemModel
            //                        List<MapInfo> MapInfoList3 = (from c in db2.MapInfos
            //                                                      where c.TVItemID == tvItemModel.TVItemID
            //                                                      select c).ToList();


            //                        if (MapInfoList3.Count > 0)
            //                        {
            //                            db2.MapInfos.RemoveRange(MapInfoList3);

            //                            try
            //                            {
            //                                db2.SaveChanges();
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                richTextBoxStatus.AppendText($"Could not delete MapInfos of TVItemModel");
            //                                return;
            //                            }
            //                        }

            //                        // Deleting MWQMSubsectors for TVItemModel
            //                        List<MWQMSubsector> mwqmSubsectorList = (from c in db2.MWQMSubsectors
            //                                                                 where c.MWQMSubsectorTVItemID == tvItemModel.TVItemID
            //                                                                 select c).ToList();

            //                        if (mwqmSubsectorList.Count > 0)
            //                        {
            //                            db2.MWQMSubsectors.RemoveRange(mwqmSubsectorList);

            //                            try
            //                            {
            //                                db2.SaveChanges();
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                richTextBoxStatus.AppendText($"Could not delete MWQMSubsectors for TVItemModel");
            //                                return;
            //                            }
            //                        }

            //                        // Deleting UseOfSites for TVItemModel
            //                        List<UseOfSite> UseOfSiteList = (from c in db2.UseOfSites
            //                                                         where c.SubsectorTVItemID == tvItemModel.TVItemID
            //                                                         select c).ToList();

            //                        if (UseOfSiteList.Count > 0)
            //                        {
            //                            db2.UseOfSites.RemoveRange(UseOfSiteList);

            //                            try
            //                            {
            //                                db2.SaveChanges();
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                richTextBoxStatus.AppendText($"Could not delete UseOfSites for TVItemModelss");
            //                                return;
            //                            }
            //                        }


            //                        // Deleting TVItems for TVItemModel
            //                        List<TVItem> TVItemList3 = (from c in db2.TVItems
            //                                                    where c.TVItemID == tvItemModel.TVItemID
            //                                                    select c).ToList();

            //                        if (TVItemList3.Count > 0)
            //                        {
            //                            db2.TVItems.RemoveRange(TVItemList3);

            //                            try
            //                            {
            //                                db2.SaveChanges();
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                richTextBoxStatus.AppendText($"Could not delete TVItems for TVItemModel");
            //                                return;
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    lblStatus.Text = "done...";
            //}
            #endregion Remove all subsectors and under info that are not in QC DB

            #region Add all new subsectors that are in QC DB and not in CSSPDB
            ///// -----------------------------------------------------------------
            ///// ------ Add all new subsectors that are in QC DB and not in CSSPDB --
            ///// -----------------------------------------------------------------

            //List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Sector);
            //List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);

            //using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            //{
            //    var geo_stations_pList = (from c in dbQC.geo_stations_p
            //                              where c.secteur != null
            //                              && c.secteur.StartsWith("M") == false
            //                              orderby c.secteur, c.station
            //                              select new { c.id_geo_station_p, c.station, c.secteur }).ToList();

            //    List<string> SubsectorListQC = (from c in geo_stations_pList
            //                                    select c.secteur).Distinct().ToList();

            //    List<string> SubsectorList = new List<string>();
            //    foreach (string s in SubsectorListQC)
            //    {
            //        if (s.Length > 3)
            //        {
            //            if (!SubsectorList.Contains(s))
            //            {
            //                SubsectorList.Add(s);
            //            }
            //        }
            //    }

            //    richTextBoxStatus.AppendText($"------------------- QC Subsector ------------\r\n");
            //    foreach (string subsector in SubsectorList)
            //    {
            //        lblStatus.Text = subsector;
            //        lblStatus.Refresh();
            //        Application.DoEvents();

            //        if (!tvItemModelSubsectorList.Where(c => c.TVText.StartsWith(subsector + " ")).Any())
            //        {
            //            richTextBoxStatus.AppendText($"{subsector} does not exist\t creating it\r\n");

            //            string SectorText = subsector.Substring(0, 4);

            //            TVItemModel tvItemModelSector = tvItemModelSectorList.Where(c => c.TVText.Contains(SectorText + " ")).FirstOrDefault();

            //            if (tvItemModelSector == null)
            //            {
            //                richTextBoxStatus.AppendText($"{SectorText} could not be found\r\n");
            //                return;
            //            }

            //            TVItemModel tvItemModelSubsector = tvItemService.GetChildTVItemModelWithTVItemIDAndTVTextStartWithAndTVTypeDB(tvItemModelSector.TVItemID, subsector + " ", TVTypeEnum.Subsector);
            //            if (!string.IsNullOrWhiteSpace(tvItemModelSubsector.Error))
            //            {
            //                TVItemModel tvItemModelSubsectorRet = tvItemService.PostAddChildTVItemDB(tvItemModelSector.TVItemID, subsector + " (vide)", TVTypeEnum.Subsector);
            //                if (!string.IsNullOrWhiteSpace(tvItemModelSubsectorRet.Error))
            //                {
            //                    richTextBoxStatus.AppendText($"{tvItemModelSubsectorRet.Error}\r\n");
            //                    return;
            //                }

            //                var geo_stations_pList2 = (from c in dbQC.geo_stations_p
            //                                           where c.secteur != null
            //                                           && c.secteur.StartsWith("M") == false
            //                                           && c.secteur.StartsWith(subsector)
            //                                           && c.x != null
            //                                           && c.y != null
            //                                           select new { c.x, c.y }).ToList();

            //                float MinLat = (float)((from c in geo_stations_pList2
            //                                        select c.y).Min());

            //                float MaxLat = (float)((from c in geo_stations_pList2
            //                                        select c.y).Max());

            //                float MinLng = (float)((from c in geo_stations_pList2
            //                                        select c.x).Min());

            //                float MaxLng = (float)((from c in geo_stations_pList2
            //                                        select c.x).Max());

            //                List<Coord> coordList = new List<Coord>()
            //                {
            //                    new Coord() { Lat = (MaxLat + MinLat)/2.0f, Lng = (MaxLng + MinLng)/2.0f, Ordinal = 0 },
            //                };

            //                MapInfoModel mapInfoModel = tvItemService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Subsector, tvItemModelSubsectorRet.TVItemID);
            //                if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
            //                {
            //                    richTextBoxStatus.AppendText($"{mapInfoModel.Error}\r\n");
            //                    return;
            //                }

            //                coordList = new List<Coord>()
            //                {
            //                    new Coord() { Lat = MinLat, Lng = MinLng, Ordinal = 0 },
            //                    new Coord() { Lat = MinLat, Lng = MaxLng, Ordinal = 1 },
            //                    new Coord() { Lat = MaxLat, Lng = MaxLng, Ordinal = 2 },
            //                    new Coord() { Lat = MaxLat, Lng = MinLng, Ordinal = 3 },
            //                    new Coord() { Lat = MinLat, Lng = MinLng, Ordinal = 4 },
            //                };

            //                mapInfoModel = tvItemService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Polygon, TVTypeEnum.Subsector, tvItemModelSubsectorRet.TVItemID);
            //                if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
            //                {
            //                    richTextBoxStatus.AppendText($"{mapInfoModel.Error}\r\n");
            //                    return;
            //                }
            //            }

            //        }
            //    }

            //    lblStatus.Text = "done...";
            //}
            #endregion Add all new subsectors that are in QC DB and not in CSSPDB

            #region Add all new MWQMSites that are in QC DB and not in CSSPDB
            ///// -----------------------------------------------------------------
            ///// ------ Add all new MWQMSites that are in QC DB and not in CSSPDB --
            ///// -----------------------------------------------------------------

            //List<TVItemModel> tvItemModelSectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Sector);
            //List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);

            //using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            //{
            //    var geo_stations_pList = (from c in dbQC.geo_stations_p
            //                              where c.secteur != null
            //                              && c.secteur.StartsWith("M") == false
            //                              orderby c.secteur, c.station
            //                              select new { c.id_geo_station_p, c.station, c.secteur, c.x, c.y }).ToList();

            //    List<string> SubsectorListQC = (from c in geo_stations_pList
            //                                    select c.secteur).Distinct().ToList();

            //    List<string> SubsectorList = new List<string>();
            //    foreach (string s in SubsectorListQC)
            //    {
            //        if (s.Length > 3)
            //        {
            //            if (!SubsectorList.Contains(s))
            //            {
            //                SubsectorList.Add(s);
            //            }
            //        }
            //    }

            //    richTextBoxStatus.AppendText($"------------------- QC Subsector ------------\r\n");
            //    foreach (string subsector in SubsectorList)
            //    {
            //        lblStatus.Text = subsector;
            //        lblStatus.Refresh();
            //        Application.DoEvents();

            //        TVItemModel tvItemModelSubsector = tvItemModelSubsectorList.Where(c => c.TVText.StartsWith(subsector + " ")).FirstOrDefault();
            //        if (tvItemModelSubsector == null)
            //        {
            //            richTextBoxStatus.AppendText($"{subsector} could not be found\r\n");
            //            return;
            //        }

            //        var QCSites = (from c in geo_stations_pList
            //                       where c.secteur == subsector
            //                       select c).ToList();

            //        foreach (var qcsite in QCSites)
            //        {
            //            lblStatus.Text = subsector + " -- " + ((int)qcsite.station).ToString();
            //            lblStatus.Refresh();
            //            Application.DoEvents();

            //            string MWQMSiteTVText = "0000".Substring(0, 4 - qcsite.station.ToString().Length) + qcsite.station.ToString();

            //            TVItemModel tvItemModelSite = tvItemService.GetChildTVItemModelWithParentIDAndTVTextAndTVTypeDB(tvItemModelSubsector.TVItemID, MWQMSiteTVText, TVTypeEnum.MWQMSite);
            //            if (!string.IsNullOrWhiteSpace(tvItemModelSite.Error))
            //            {

            //                TVItemModel tvItemModelSiteRet = tvItemService.PostAddChildTVItemDB(tvItemModelSubsector.TVItemID, MWQMSiteTVText, TVTypeEnum.MWQMSite);
            //                if (!string.IsNullOrWhiteSpace(tvItemModelSiteRet.Error))
            //                {
            //                    richTextBoxStatus.AppendText($"{tvItemModelSiteRet.Error}\r\n");
            //                    return;
            //                }

            //                List<Coord> coordList = new List<Coord>()
            //                {
            //                    new Coord() { Lat = (float)(qcsite.y), Lng = (float)(qcsite.x), Ordinal = 0 },
            //                };

            //                MapInfoModel mapInfoModel = tvItemService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.MWQMSite, tvItemModelSiteRet.TVItemID);
            //                if (!string.IsNullOrWhiteSpace(mapInfoModel.Error))
            //                {
            //                    richTextBoxStatus.AppendText($"{mapInfoModel.Error}\r\n");
            //                    return;
            //                }
            //            }
            //        }

            //    }

            //    lblStatus.Text = "done...";
            //}
            #endregion Add all new MWQMSites that are in QC DB and not in CSSPDB

            #region Add all new Runs that are in QC DB and not in CSSPDB
            ///// -----------------------------------------------------------------
            ///// ------ Add all new Runs that are in QC DB and not in CSSPDB --
            ///// -----------------------------------------------------------------

            //List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
            //if (tvItemModelSubsectorList.Count == 0)
            //{
            //    richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelQC.TVText + "\r\n");
            //    return;
            //}

            //using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            //{

            //    var staQCListAll = (from c in dbQC.geo_stations_p
            //                        where (c.x != null && c.y != null)
            //                        && c.secteur != null
            //                        orderby c.secteur
            //                        select new { c.id_geo_station_p, c.status, c.station, c.secteur, c.x, c.y }).ToList();

            //    var subsectorList = (from s in dbQC.geo_stations_p
            //                         where s.secteur != null
            //                         orderby s.secteur
            //                         select s.secteur).Distinct().ToList();

            //    List<TVItemModel> tvItemMWQMSiteAll = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.MWQMSite);

            //    List<TVItemModel> tvItemMWQMRunAll = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.MWQMRun);

            //    var dbMesureListAll = (from m in dbQC.db_mesure
            //                           select new { m.id_geo_station_p, m.id_tournee, m.hre_echantillonnage }).ToList();

            //    List<PCCSM.db_tournee> dbtAll = (from t in dbQC.db_tournee
            //                                     select t).ToList();

            //    int Count = 0;
            //    int TotalCount = subsectorList.Count();
            //    foreach (string subsector in subsectorList)
            //    {
            //        if (subsector == "S")
            //        {
            //            continue;
            //        }

            //        if (subsector.StartsWith("MS"))
            //        {
            //            continue;
            //        }

            //        if (Cancel)
            //        {
            //            richTextBoxStatus.AppendText($"Pressed Cancel\r\n");
            //            return;
            //        }

            //        Count += 1;
            //        lblStatus.Text = (Count * 100 / TotalCount).ToString() + " ... CreateRunsQC for " + subsector;
            //        lblStatus2.Text = Count + " of " + TotalCount;
            //        Application.DoEvents();

            //        TVItemModel tvItemModelSubsector = (from c in tvItemModelSubsectorList
            //                                            where c.TVText.StartsWith(subsector)
            //                                            select c).FirstOrDefault();

            //        if (tvItemModelSubsector == null)
            //        {
            //            richTextBoxStatus.AppendText($"could not find tvItemmodelSubsector [{subsector}]\r\n");
            //            return;
            //        }

            //        List<MWQMRunModel> mwqmRunModelAll = mwqmRunService.GetMWQMRunModelListWithSubsectorTVItemIDDB(tvItemModelSubsector.TVItemID);

            //        var staQCList = (from c in staQCListAll
            //                         where c.secteur == subsector
            //                         select c).ToList();

            //        int countSta = 0;
            //        int totalCountsta = staQCList.Count;
            //        foreach (var geoStat in staQCList)
            //        {
            //            if (Cancel)
            //            {
            //                richTextBoxStatus.AppendText($"Pressed Cancel\r\n");
            //                return;
            //            }


            //            countSta += 1;
            //            lblStatus2.Text = countSta + " of " + totalCountsta + " ... CreateRunsQC for " + subsector;
            //            Application.DoEvents();

            //            Application.DoEvents();

            //            bool IsActive = true;
            //            if (geoStat.status != null)
            //            {
            //                IsActive = (geoStat.status.Substring(0, 1) == "i" ? false : true);
            //            }
            //            string MWQMSiteTVText = "0000".Substring(0, 4 - geoStat.station.ToString().Length) + geoStat.station.ToString();

            //            TVItemModel tvItemMWQMSiteExist = (from c in tvItemMWQMSiteAll
            //                                               where c.ParentID == tvItemModelSubsector.TVItemID
            //                                               && c.TVText == MWQMSiteTVText
            //                                               && c.TVType == TVTypeEnum.MWQMSite
            //                                               select c).FirstOrDefault();

            //            if (tvItemMWQMSiteExist == null)
            //            {
            //                richTextBoxStatus.AppendText($"could not find MWQMSite [{MWQMSiteTVText} under subsector {subsector}]\r\n");
            //                return;
            //            }

            //            var dbMesureList = (from m in dbMesureListAll
            //                                where m.id_geo_station_p == geoStat.id_geo_station_p
            //                                select m).ToList();


            //            foreach (var dbm in dbMesureList)
            //            {

            //                Application.DoEvents();

            //                // getting Runs
            //                PCCSM.db_tournee dbt = (from t in dbtAll
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

            //                DateTime DateRun = new DateTime(((DateTime)SampDateTime).Year, ((DateTime)SampDateTime).Month, ((DateTime)SampDateTime).Day);

            //                MWQMRunModel mwqmRunModelNew = new MWQMRunModel()
            //                {
            //                    SubsectorTVItemID = tvItemModelSubsector.TVItemID,
            //                    DateTime_Local = DateRun,
            //                    RunSampleType = SampleTypeEnum.Routine,
            //                    RunNumber = 1,
            //                };

            //                MWQMRunModel wqmRunModelExist = (from c in mwqmRunModelAll
            //                                                 where c.DateTime_Local == DateRun
            //                                                 && c.RunSampleType == SampleTypeEnum.Routine
            //                                                 && c.RunNumber == 1
            //                                                 select c).FirstOrDefault();

            //                if (wqmRunModelExist == null)
            //                {
            //                    string TVTextRun = DateRun.Year.ToString()
            //                        + " " + (DateRun.Month > 9 ? DateRun.Month.ToString() : "0" + DateRun.Month.ToString())
            //                        + " " + (DateRun.Day > 9 ? DateRun.Day.ToString() : "0" + DateRun.Day.ToString());

            //                    TVItemModel tvItemModelRunRet = (from c in tvItemMWQMRunAll
            //                                                     where c.ParentID == tvItemModelSubsector.TVItemID
            //                                                     && c.TVText == TVTextRun
            //                                                     select c).FirstOrDefault();

            //                    if (tvItemModelRunRet == null)
            //                    {
            //                        // richTextBoxStatus.AppendText($"{tvItemModelSubsector.TVText} --- { TVTextRun } adding TVText\r\n");
            //                        tvItemModelRunRet = tvItemService.PostAddChildTVItemDB(tvItemModelSubsector.TVItemID, TVTextRun, TVTypeEnum.MWQMRun);
            //                        if (!string.IsNullOrWhiteSpace(tvItemModelRunRet.Error))
            //                        {
            //                            richTextBoxStatus.AppendText($"could not add TVItem for Runs [{TVTextRun} under subsector {subsector}]\r\n");
            //                            return;
            //                        }
            //                        tvItemMWQMRunAll.Add(tvItemModelRunRet);
            //                    }

            //                    // add the run in the DB
            //                    mwqmRunModelNew.MWQMRunTVItemID = tvItemModelRunRet.TVItemID;
            //                    mwqmRunModelNew.AnalyzeMethod = null;
            //                    mwqmRunModelNew.SampleCrewInitials = null;
            //                    mwqmRunModelNew.DateTime_Local = DateRun;
            //                    if (SampEndDateTime == null)
            //                    {
            //                        mwqmRunModelNew.EndDateTime_Local = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.EndDateTime_Local = (DateTime)SampEndDateTime;
            //                    }
            //                    if (SampStartDateTime == null)
            //                    {
            //                        mwqmRunModelNew.StartDateTime_Local = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.StartDateTime_Local = (DateTime)SampStartDateTime;
            //                    }
            //                    if (dbt.analyse_datetime == null)
            //                    {
            //                        mwqmRunModelNew.LabAnalyzeBath1IncubationStartDateTime_Local = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.LabAnalyzeBath1IncubationStartDateTime_Local = dbt.analyse_datetime;
            //                    }
            //                    if (dbt.hre_recep_lab == null)
            //                    {
            //                        mwqmRunModelNew.LabReceivedDateTime_Local = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.LabReceivedDateTime_Local = dbt.hre_recep_lab;
            //                    }
            //                    mwqmRunModelNew.Laboratory = null;
            //                    mwqmRunModelNew.SampleMatrix = null;
            //                    if (dbt.mer_etat_fin == null)
            //                    {
            //                        mwqmRunModelNew.SeaStateAtEnd_BeaufortScale = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.SeaStateAtEnd_BeaufortScale = (BeaufortScaleEnum)dbt.mer_etat_fin;
            //                    }
            //                    if (dbt.mer_etat == null)
            //                    {
            //                        mwqmRunModelNew.SeaStateAtStart_BeaufortScale = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.SeaStateAtStart_BeaufortScale = (BeaufortScaleEnum)dbt.mer_etat;
            //                    }
            //                    mwqmRunModelNew.SampleStatus = null;
            //                    if (dbt.temp_glace_recep == null)
            //                    {
            //                        mwqmRunModelNew.TemperatureControl1_C = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.TemperatureControl1_C = (float)dbt.temp_glace_recep;
            //                    }
            //                    mwqmRunModelNew.SubsectorTVItemID = tvItemModelSubsector.TVItemID;
            //                    if (dbt.validation_datetime == null)
            //                    {
            //                        mwqmRunModelNew.LabRunSampleApprovalDateTime_Local = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.LabRunSampleApprovalDateTime_Local = dbt.validation_datetime;
            //                    }
            //                    if (dbt.validation == null)
            //                    {
            //                        mwqmRunModelNew.LabSampleApprovalContactTVItemID = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.LabSampleApprovalContactTVItemID = null; // 1; // this will be changed in the future to reflect the actuall UserInfoID
            //                    }
            //                    if (dbt.niveau_eau == null)
            //                    {
            //                        mwqmRunModelNew.WaterLevelAtBrook_m = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.WaterLevelAtBrook_m = (double)dbt.niveau_eau;
            //                    }
            //                    if (dbt.vague_hauteur_fin == null)
            //                    {
            //                        mwqmRunModelNew.WaveHightAtEnd_m = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.WaveHightAtEnd_m = dbt.vague_hauteur_fin;
            //                    }
            //                    if (dbt.vague_hauteur == null)
            //                    {
            //                        mwqmRunModelNew.WaveHightAtStart_m = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.WaveHightAtStart_m = dbt.vague_hauteur;
            //                    }

            //                    string TextEN = "--";
            //                    if (!string.IsNullOrWhiteSpace(dbt.commentaire))
            //                    {
            //                        TextEN = dbt.commentaire.Trim();
            //                    }

            //                    if (dbt.precipit == null)
            //                    {
            //                        mwqmRunModelNew.RainDay1_mm = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.RainDay1_mm = dbt.precipit;
            //                    }
            //                    if (dbt.precipit_3jant == null)
            //                    {
            //                        mwqmRunModelNew.RainDay3_mm = null;
            //                    }
            //                    else
            //                    {
            //                        mwqmRunModelNew.RainDay3_mm = dbt.precipit_3jant;
            //                    }


            //                    mwqmRunModelNew.Tide_Start = TideTextEnum.MidTide;

            //                    if (dbt.maree_principale != null)
            //                    {
            //                        if (dbt.maree_principale == 594)
            //                        {
            //                            mwqmRunModelNew.Tide_Start = TideTextEnum.LowTide;
            //                        }
            //                        else if (dbt.maree_principale == 593)
            //                        {
            //                            mwqmRunModelNew.Tide_Start = TideTextEnum.HighTide;
            //                        }
            //                        else
            //                        {
            //                        }
            //                    }

            //                    mwqmRunModelNew.Tide_End = mwqmRunModelNew.Tide_Start;

            //                    mwqmRunModelNew.AnalyzeMethod = null;
            //                    if (dbt.cf_methode_analytique != null)
            //                    {
            //                        switch (dbt.cf_methode_analytique.Value.ToString())
            //                        {
            //                            case "0":
            //                                {
            //                                    mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.ZZ_0Q;
            //                                }
            //                                break;
            //                            case "509":
            //                                {
            //                                    mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.ZZ_509Q;
            //                                }
            //                                break;
            //                            case "510":
            //                                {
            //                                    mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.ZZ_510Q;
            //                                }
            //                                break;
            //                            case "525":
            //                                {
            //                                    mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.ZZ_525Q;
            //                                }
            //                                break;
            //                            default:
            //                                break;
            //                        }
            //                    }

            //                    mwqmRunModelNew.SampleMatrix = SampleMatrixEnum.MPNQ;

            //                    mwqmRunModelNew.Laboratory = null;

            //                    if (dbt.laboratoire_operateur_id != null)
            //                    {
            //                        switch (dbt.laboratoire_operateur_id.ToString())
            //                        {
            //                            case "1":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_0;
            //                                }
            //                                break;
            //                            case "2":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_1;
            //                                }
            //                                break;
            //                            case "3":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_2;
            //                                }
            //                                break;
            //                            case "4":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_3;
            //                                }
            //                                break;
            //                            case "5":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_4;
            //                                }
            //                                break;
            //                            case "6":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_1Q;
            //                                }
            //                                break;
            //                            case "7":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_2Q;
            //                                }
            //                                break;
            //                            case "8":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_3Q;
            //                                }
            //                                break;
            //                            case "9":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_4Q;
            //                                }
            //                                break;
            //                            case "10":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_5Q;
            //                                }
            //                                break;
            //                            case "11":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_11BC;
            //                                }
            //                                break;
            //                            case "12":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_12BC;
            //                                }
            //                                break;
            //                            case "14":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_14BC;
            //                                }
            //                                break;
            //                            case "15":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_15BC;
            //                                }
            //                                break;
            //                            case "16":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_16BC;
            //                                }
            //                                break;
            //                            case "17":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_17BC;
            //                                }
            //                                break;
            //                            case "18":
            //                                {
            //                                    mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_18BC;
            //                                }
            //                                break;
            //                            default:
            //                                break;
            //                        }
            //                    }

            //                    // Doing Status
            //                    mwqmRunModelNew.SampleStatus = null;
            //                    if (dbt.status != null)
            //                    {
            //                        if (dbt.status == 11)
            //                        {
            //                            mwqmRunModelNew.SampleStatus = SampleStatusEnum.Active;
            //                        }
            //                        else if (dbt.status == 606)
            //                        {
            //                            mwqmRunModelNew.SampleStatus = SampleStatusEnum.Archived;
            //                        }
            //                        else
            //                        {

            //                        }
            //                    }

            //                    if (mwqmRunModelNew.StartDateTime_Local > mwqmRunModelNew.EndDateTime_Local)
            //                    {
            //                        mwqmRunModelNew.EndDateTime_Local = mwqmRunModelNew.StartDateTime_Local;
            //                    }

            //                    MWQMRunModel mwqmRunModel = (from c in mwqmRunModelAll
            //                                                 where c.SubsectorTVItemID == tvItemModelSubsector.TVItemID
            //                                                 && c.MWQMRunTVText == TVTextRun
            //                                                 && c.RunSampleType == mwqmRunModelNew.RunSampleType
            //                                                 select c).FirstOrDefault();

            //                    if (mwqmRunModel == null)
            //                    {
            //                        //richTextBoxStatus.AppendText($"{tvItemModelSubsector.TVText} --- { TVTextRun } adding MWQMRun\r\n");
            //                        MWQMRunModel mwqmRunModelRet = mwqmRunService.PostAddMWQMRunDB(mwqmRunModelNew);
            //                        if (!string.IsNullOrWhiteSpace(mwqmRunModelRet.Error))
            //                        {
            //                            richTextBoxStatus.AppendText($"could not add Runs [{TVTextRun} under subsector {subsector}]\r\n");
            //                            return;
            //                        }

            //                        mwqmRunModelAll.Add(mwqmRunModelRet);
            //                    }
            //                }
            //            }
            //        }
            //    }

            //}
            #endregion Add all new Runs that are in QC DB and not in CSSPDB

            #region remove all MWQMSites with a _ within the TVText
            ///// -----------------------------------------------------------------
            ///// ------ remove all MWQMSites with a _ within the TVText --
            ///// -----------------------------------------------------------------
            //using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            //{
            //    List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);

            //    foreach (TVItemModel tvItemModel in tvItemModelSubsectorList)
            //    {
            //        if (tvItemModel.TVText.StartsWith("MS"))
            //        {
            //            continue;
            //        }

            //        lblStatus.Text = tvItemModel.TVText;
            //        lblStatus.Refresh();
            //        Application.DoEvents();

            //        List<TVItemModel> tvItemModelSiteList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModel.TVItemID, TVTypeEnum.MWQMSite);

            //        foreach (TVItemModel tvItemModelSite in tvItemModelSiteList)
            //        {
            //            lblStatus.Text = tvItemModel.TVText.Substring(0, tvItemModel.TVText.IndexOf(" ")) + " -- " + tvItemModelSite.TVText;
            //            lblStatus.Refresh();
            //            Application.DoEvents();

            //            if (tvItemModelSite.TVText.Contains("_"))
            //            {
            //                using (CSSPDBEntities db2 = new CSSPDBEntities())
            //                {
            //                    List<TVItemStat> tvItemStatList = (from c in db2.TVItemStats
            //                                                       where c.TVItemID == tvItemModelSite.TVItemID
            //                                                       select c).ToList();

            //                    if (tvItemStatList.Count > 0)
            //                    {
            //                        db2.TVItemStats.RemoveRange(tvItemStatList);

            //                        try
            //                        {
            //                            db2.SaveChanges();
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            richTextBoxStatus.AppendText($"Could not delete TVItemStats of MWQMSites");
            //                        }
            //                    }

            //                    List<MWQMSample> mwqmSampleList = (from c in db2.MWQMSamples
            //                                                       where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
            //                                                       select c).ToList();

            //                    if (mwqmSampleList.Count > 0)
            //                    {
            //                        db2.MWQMSamples.RemoveRange(mwqmSampleList);

            //                        try
            //                        {
            //                            db2.SaveChanges();
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            richTextBoxStatus.AppendText($"Could not delete Sample");
            //                        }
            //                    }

            //                    List<MapInfo> MapInfoList = (from c in db2.MapInfos
            //                                                 where c.TVItemID == tvItemModelSite.TVItemID
            //                                                 select c).ToList();


            //                    if (MapInfoList.Count > 0)
            //                    {
            //                        db2.MapInfos.RemoveRange(MapInfoList);

            //                        try
            //                        {
            //                            db2.SaveChanges();
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            richTextBoxStatus.AppendText($"Could not delete MapInfos for MWQMSites");
            //                        }
            //                    }

            //                    List<MWQMSite> mwqmSiteList = (from c in db2.MWQMSites
            //                                                   where c.MWQMSiteTVItemID == tvItemModelSite.TVItemID
            //                                                   select c).ToList();

            //                    if (mwqmSiteList.Count > 0)
            //                    {
            //                        db2.MWQMSites.RemoveRange(mwqmSiteList);

            //                        try
            //                        {
            //                            db2.SaveChanges();
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
            //                        }
            //                    }

            //                    List<TVItem> TVItemList = (from c in db2.TVItems
            //                                               where c.TVItemID == tvItemModelSite.TVItemID
            //                                               select c).ToList();

            //                    if (TVItemList.Count > 0)
            //                    {
            //                        db2.TVItems.RemoveRange(TVItemList);

            //                        try
            //                        {
            //                            db2.SaveChanges();
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            richTextBoxStatus.AppendText($"Could not delete TVItems for MWQMSites");
            //                        }
            //                    }

            //                }
            //            }
            //        }
            //    }
            //    lblStatus.Text = "done...";
            //}
            #endregion remove all sectors and under info that are not in QC DB

            #region Add all new MWQMSample that are in QC DB and not in CSSPDB
            /// -----------------------------------------------------------------
            /// ------ Add all new MWQMSample that are in QC DB and not in CSSPDB --
            /// -----------------------------------------------------------------

            List<TVItemModel> tvItemModelSubsectorList = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.Subsector);
            if (tvItemModelSubsectorList.Count == 0)
            {
                richTextBoxStatus.AppendText("Error: could not find TVItem Subsector for " + tvItemModelQC.TVText + "\r\n");
                return;
            }

            using (PCCSM.pccsmEntities dbQC = new PCCSM.pccsmEntities())
            {

                var staQCListAll = (from c in dbQC.geo_stations_p
                                    where (c.x != null && c.y != null)
                                    && c.secteur != null
                                    orderby c.secteur
                                    select new { c.id_geo_station_p, c.status, c.station, c.secteur, c.x, c.y }).ToList();

                var subsectorList = (from s in dbQC.geo_stations_p
                                     where s.secteur != null
                                     orderby s.secteur
                                     select s.secteur).Distinct().ToList();

                List<TVItemModel> tvItemMWQMSiteAll = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.MWQMSite);

                List<TVItemModel> tvItemMWQMRunAll = tvItemService.GetChildrenTVItemModelListWithTVItemIDAndTVTypeDB(tvItemModelQC.TVItemID, TVTypeEnum.MWQMRun);

                var dbMesureListAll = (from m in dbQC.db_mesure
                                       select new { m.id_geo_station_p, m.id_tournee, m.hre_echantillonnage, m.prof, m.sal, m.temp, m.ph,m.diffusable, m.commentaire, m.cf }).ToList();

                List<PCCSM.db_tournee> dbtAll = (from t in dbQC.db_tournee
                                                 select t).ToList();

                foreach (string subsector in subsectorList)
                {
                    if (subsector == "S")
                    {
                        continue;
                    }

                    if (subsector.StartsWith("MS"))
                    {
                        continue;
                    }

                    if (Cancel)
                    {
                        richTextBoxStatus.AppendText($"Pressed Cancel\r\n");
                        return;
                    }

                    lblStatus.Text = subsector;
                    lblStatus.Refresh();
                    Application.DoEvents();

                    TVItemModel tvItemModelSubsector = (from c in tvItemModelSubsectorList
                                                        where c.TVText.StartsWith(subsector)
                                                        select c).FirstOrDefault();

                    if (tvItemModelSubsector == null)
                    {
                        richTextBoxStatus.AppendText($"could not find tvItemmodelSubsector [{subsector}]\r\n");
                        return;
                    }

                    List<MWQMRunModel> mwqmRunModelAll = mwqmRunService.GetMWQMRunModelListWithSubsectorTVItemIDDB(tvItemModelSubsector.TVItemID);

                    var staQCList = (from c in staQCListAll
                                     where c.secteur == subsector
                                     select c).ToList();

                    foreach (var geoStat in staQCList)
                    {
                        if (Cancel)
                        {
                            richTextBoxStatus.AppendText($"Pressed Cancel\r\n");
                            return;
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
                            return;
                        }

                        var dbMesureList = (from m in dbMesureListAll
                                            where m.id_geo_station_p == geoStat.id_geo_station_p
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

                        foreach (var dbm in dbMesureList)
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
                                richTextBoxStatus.AppendText($"could not find Run [{DateRun} under subsector {subsector}]\r\n");
                                return;

                                //string TVTextRun = DateRun.Year.ToString()
                                //    + " " + (DateRun.Month > 9 ? DateRun.Month.ToString() : "0" + DateRun.Month.ToString())
                                //    + " " + (DateRun.Day > 9 ? DateRun.Day.ToString() : "0" + DateRun.Day.ToString());

                                //TVItemModel tvItemModelRunRet = (from c in tvItemMWQMRunAll
                                //                                 where c.ParentID == tvItemModelSubsector.TVItemID
                                //                                 && c.TVText == TVTextRun
                                //                                 select c).FirstOrDefault();

                                //if (tvItemModelRunRet == null)
                                //{
                                //    richTextBoxStatus.AppendText($"{tvItemModelSubsector.TVText} --- { TVTextRun } adding TVText\r\n");
                                //    tvItemModelRunRet = tvItemService.PostAddChildTVItemDB(tvItemModelSubsector.TVItemID, TVTextRun, TVTypeEnum.MWQMRun);
                                //    if (!CheckModelOK<TVItemModel>(tvItemModelRunRet)) return;

                                //    tvItemMWQMRunAll.Add(tvItemModelRunRet);
                                //}

                                //// add the run in the DB
                                //mwqmRunModelNew.MWQMRunTVItemID = tvItemModelRunRet.TVItemID;
                                //mwqmRunModelNew.AnalyzeMethod = null;
                                //mwqmRunModelNew.SampleCrewInitials = null;
                                //mwqmRunModelNew.DateTime_Local = DateRun;
                                //if (SampEndDateTime == null)
                                //{
                                //    mwqmRunModelNew.EndDateTime_Local = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.EndDateTime_Local = (DateTime)SampEndDateTime;
                                //}
                                //if (SampStartDateTime == null)
                                //{
                                //    mwqmRunModelNew.StartDateTime_Local = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.StartDateTime_Local = (DateTime)SampStartDateTime;
                                //}
                                //if (dbt.analyse_datetime == null)
                                //{
                                //    mwqmRunModelNew.LabAnalyzeBath1IncubationStartDateTime_Local = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.LabAnalyzeBath1IncubationStartDateTime_Local = dbt.analyse_datetime;
                                //}
                                //if (dbt.hre_recep_lab == null)
                                //{
                                //    mwqmRunModelNew.LabReceivedDateTime_Local = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.LabReceivedDateTime_Local = dbt.hre_recep_lab;
                                //}
                                //mwqmRunModelNew.Laboratory = null;
                                //mwqmRunModelNew.SampleMatrix = null;
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
                                //mwqmRunModelNew.SampleStatus = null;
                                //if (dbt.temp_glace_recep == null)
                                //{
                                //    mwqmRunModelNew.TemperatureControl1_C = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.TemperatureControl1_C = (float)dbt.temp_glace_recep;
                                //}
                                //mwqmRunModelNew.SubsectorTVItemID = tvItemModelSubsector.TVItemID;
                                //if (dbt.validation_datetime == null)
                                //{
                                //    mwqmRunModelNew.LabRunSampleApprovalDateTime_Local = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.LabRunSampleApprovalDateTime_Local = dbt.validation_datetime;
                                //}
                                //if (dbt.validation == null)
                                //{
                                //    mwqmRunModelNew.LabSampleApprovalContactTVItemID = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.LabSampleApprovalContactTVItemID = null; // 1; // this will be changed in the future to reflect the actuall UserInfoID
                                //}
                                //if (dbt.niveau_eau == null)
                                //{
                                //    mwqmRunModelNew.WaterLevelAtBrook_m = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.WaterLevelAtBrook_m = (double)dbt.niveau_eau;
                                //}
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

                                //string TextEN = "--";
                                //if (!string.IsNullOrWhiteSpace(dbt.commentaire))
                                //{
                                //    TextEN = dbt.commentaire.Trim();
                                //}

                                //if (dbt.precipit == null)
                                //{
                                //    mwqmRunModelNew.RainDay1_mm = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.RainDay1_mm = dbt.precipit;
                                //}
                                //if (dbt.precipit_3jant == null)
                                //{
                                //    mwqmRunModelNew.RainDay3_mm = null;
                                //}
                                //else
                                //{
                                //    mwqmRunModelNew.RainDay3_mm = dbt.precipit_3jant;
                                //}


                                //mwqmRunModelNew.Tide_Start = TideTextEnum.MidTide;

                                //if (dbt.maree_principale != null)
                                //{
                                //    if (dbt.maree_principale == 594)
                                //    {
                                //        mwqmRunModelNew.Tide_Start = TideTextEnum.LowTide;
                                //    }
                                //    else if (dbt.maree_principale == 593)
                                //    {
                                //        mwqmRunModelNew.Tide_Start = TideTextEnum.HighTide;
                                //    }
                                //    else
                                //    {
                                //    }
                                //}

                                //mwqmRunModelNew.Tide_End = mwqmRunModelNew.Tide_Start;


                                //mwqmRunModelNew.AnalyzeMethod = null;
                                //if (dbt.cf_methode_analytique != null)
                                //{
                                //    switch (dbt.cf_methode_analytique.Value.ToString())
                                //    {
                                //        case "0":
                                //            {
                                //                mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.ZZ_0Q;
                                //            }
                                //            break;
                                //        case "509":
                                //            {
                                //                mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.ZZ_509Q;
                                //            }
                                //            break;
                                //        case "510":
                                //            {
                                //                mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.ZZ_510Q;
                                //            }
                                //            break;
                                //        case "525":
                                //            {
                                //                mwqmRunModelNew.AnalyzeMethod = AnalyzeMethodEnum.ZZ_525Q;
                                //            }
                                //            break;
                                //        default:
                                //            break;
                                //    }
                                //}

                                //mwqmRunModelNew.SampleMatrix = SampleMatrixEnum.MPNQ;

                                //mwqmRunModelNew.Laboratory = null;

                                //if (dbt.laboratoire_operateur_id != null)
                                //{
                                //    switch (dbt.laboratoire_operateur_id.ToString())
                                //    {
                                //        case "1":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_0;
                                //            }
                                //            break;
                                //        case "2":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_1;
                                //            }
                                //            break;
                                //        case "3":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_2;
                                //            }
                                //            break;
                                //        case "4":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_3;
                                //            }
                                //            break;
                                //        case "5":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_4;
                                //            }
                                //            break;
                                //        case "6":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_1Q;
                                //            }
                                //            break;
                                //        case "7":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_2Q;
                                //            }
                                //            break;
                                //        case "8":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_3Q;
                                //            }
                                //            break;
                                //        case "9":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_4Q;
                                //            }
                                //            break;
                                //        case "10":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_5Q;
                                //            }
                                //            break;
                                //        case "11":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_11BC;
                                //            }
                                //            break;
                                //        case "12":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_12BC;
                                //            }
                                //            break;
                                //        case "14":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_14BC;
                                //            }
                                //            break;
                                //        case "15":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_15BC;
                                //            }
                                //            break;
                                //        case "16":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_16BC;
                                //            }
                                //            break;
                                //        case "17":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_17BC;
                                //            }
                                //            break;
                                //        case "18":
                                //            {
                                //                mwqmRunModelNew.Laboratory = LaboratoryEnum.ZZ_18BC;
                                //            }
                                //            break;
                                //        default:
                                //            break;
                                //    }
                                //}

                                //// Doing Status
                                //mwqmRunModelNew.SampleStatus = null;
                                //if (dbt.status != null)
                                //{
                                //    if (dbt.status == 11)
                                //    {
                                //        mwqmRunModelNew.SampleStatus = SampleStatusEnum.Active;
                                //    }
                                //    else if (dbt.status == 606)
                                //    {
                                //        mwqmRunModelNew.SampleStatus = SampleStatusEnum.Archived;
                                //    }
                                //    else
                                //    {

                                //    }
                                //}

                                //if (mwqmRunModelNew.StartDateTime_Local > mwqmRunModelNew.EndDateTime_Local)
                                //{
                                //    mwqmRunModelNew.EndDateTime_Local = mwqmRunModelNew.StartDateTime_Local;
                                //}

                                //MWQMRunModel mwqmRunModel = (from c in mwqmRunModelAll
                                //                             where c.SubsectorTVItemID == tvItemModelSubsector.TVItemID
                                //                             && c.MWQMRunTVText == TVTextRun
                                //                             && c.RunSampleType == mwqmRunModelNew.RunSampleType
                                //                             select c).FirstOrDefault();

                                //if (mwqmRunModel == null)
                                //{
                                //    richTextBoxStatus.AppendText($"{tvItemModelSubsector.TVText} --- { TVTextRun } adding MWQMRun\r\n");
                                //    MWQMRunModel mwqmRunModelRet = mwqmRunService.PostAddMWQMRunDB(mwqmRunModelNew);
                                //    if (!CheckModelOK<MWQMRunModel>(mwqmRunModelRet)) return false;

                                //    mwqmRunModelAll.Add(mwqmRunModelRet);
                                //}
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
                                return;
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
                                return;
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
                                    return;

                                }
                            }


                        }
                    }
                }

            }
            #endregion Add all new MWQMSample that are in QC DB and not in CSSPDB


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

    public class CoCoRaHSSite
    {
        public string id { get; set; }
        public string st_num { get; set; }
        public string st_name { get; set; }
        public DateTime obs_date { get; set; }
        public string obs_time { get; set; }
        public float lat { get; set; }
        public float lng { get; set; }
        public float prcp { get; set; }
        public string snowfall { get; set; }
        public string snowfallswe { get; set; }
        public string snowdepth { get; set; }
        public string snowdepthswe { get; set; }
    }
}
