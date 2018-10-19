using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSSPDBDLL;
using CSSPModelsDLL.Models;
using CSSPDBDLL.Services;
using CSSPEnumsDLL.Enums;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        public bool CreateWQMLookupMPNs()
        {
            lblStatus.Text = "Starting ... LoadWQMLookupMPNs";
            Application.DoEvents();

            string DirName = @"C:\ASGAD Data\NB\"; // only need to do it once so not doin NS, NL or PE
            string connectionString = @"Provider=Microsoft.Jet.OleDb.4.0; Data Source=" + DirName + "; Extended Properties=DBASE III;";

            OleDbConnection conn = new OleDbConnection(connectionString);
            conn.Open();

            OleDbCommand comm = new OleDbCommand("SELECT TUBE5_10, TUBE5_1, TUBE5_01, MPN FROM [MPNTABLE.dbf$]"); //"Select distinct Type  from [" + "NB obs" + "];");

            OleDbDataReader reader;
            comm.Connection = conn;
            reader = comm.ExecuteReader();

            int Count = 0;
            while (reader.Read())
            {
                if (Cancel)
                {
                    return false;
                }

                Count += 1;

                string TUBE5_10 = "";
                string TUBE5_1 = "";
                string TUBE5_01 = "";
                string MPN = "";
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    switch (reader.GetName(i).ToUpper())
                    {
                        case "TUBE5_10":
                            {
                                if (reader.GetValue(i).GetType() != typeof(DBNull))
                                {
                                    TUBE5_10 = ((string)reader.GetValue(i)).Trim();
                                    if (TUBE5_10 == "")
                                    {
                                        richTextBoxStatus.AppendText("Error: could not parse the TUBE5_10 in sample.dbf\r\n");
                                        return false;
                                    }
                                }
                            }
                            break;
                        case "TUBE5_1":
                            {
                                if (reader.GetValue(i).GetType() != typeof(DBNull))
                                {
                                    TUBE5_1 = ((string)reader.GetValue(i)).Trim();
                                    if (TUBE5_1 == "")
                                    {
                                        richTextBoxStatus.AppendText("Error: could not parse the TUBE5_1 in sample.dbf\r\n");
                                        return false;
                                    }
                                }
                            }
                            break;
                        case "TUBE5_01":
                            {
                                if (reader.GetValue(i).GetType() != typeof(DBNull))
                                {
                                    TUBE5_01 = ((string)reader.GetValue(i)).Trim();
                                    if (TUBE5_01 == "")
                                    {
                                        richTextBoxStatus.AppendText("Error: could not parse the TUBE5_01 in sample.dbf\r\n");
                                        return false;
                                    }
                                }
                            }
                            break;
                        case "MPN":
                            {
                                if (reader.GetValue(i).GetType() != typeof(DBNull))
                                {
                                    MPN = ((string)reader.GetValue(i)).Trim();
                                    if (MPN == "")
                                    {
                                        richTextBoxStatus.AppendText("Error: could not parse the MPN in sample.dbf\r\n");
                                        return false;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                int IntTUBE5_10 = int.Parse(TUBE5_10);
                int IntTUBE5_1 = int.Parse(TUBE5_1);
                int IntTUBE5_01 = int.Parse(TUBE5_01);
                if (MPN == "1.9")
                {
                    MPN = "1";
                }
                int IntMPN = int.Parse(MPN);

                MWQMLookupMPNService mwqmLookupMPNService = new MWQMLookupMPNService(LanguageEnum.en, user); 

                MWQMLookupMPN wqmLookupMPNExist = mwqmLookupMPNService.GetMWQMLookupMPNExistDB(IntTUBE5_10, IntTUBE5_1, IntTUBE5_01, IntMPN);

                if (wqmLookupMPNExist == null)
                {
                    MWQMLookupMPNModel mwqmLookupMPNModel = new MWQMLookupMPNModel();
                    mwqmLookupMPNModel.Tubes10 = int.Parse(TUBE5_10);
                    mwqmLookupMPNModel.Tubes1 = int.Parse(TUBE5_1);
                    mwqmLookupMPNModel.Tubes01 = int.Parse(TUBE5_01);
                    mwqmLookupMPNModel.MPN_100ml = int.Parse(MPN);

                    MWQMLookupMPNModel mwqmLookupMPNModelRet = mwqmLookupMPNService.PostAddMWQMLookupMPNDB(mwqmLookupMPNModel);
                    if (!CheckModelOK<MWQMLookupMPNModel>(mwqmLookupMPNModelRet)) return false;
                }
            }

            return true;
        }
    }
}
