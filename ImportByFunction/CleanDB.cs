using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSSPModelsDLL.Models;
using CSSPDBDLL.Services;
using CSSPDBDLL;
using CSSPEnumsDLL.Enums;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        private bool CleanDB()
        {
            TestDBService testDBService = new TestDBService(LanguageEnum.en, user, "", "");
            List<DBTable> AllTablesToDelete = new List<DBTable>();

            lblStatus.Text = "Checking DB";
            lblStatus.Refresh();
            Application.DoEvents();

            if (!CSSPDBisOK())
                return false;

            testDBService.FillTableNameToDeleteList(AllTablesToDelete);
            if (AllTablesToDelete.Count == 0)
            {
                richTextBoxStatus.AppendText("Count of AllTablesToDelete should not be 0\r\n");
                return false;
            }

            using (CSSPDBEntities dbMVC = new CSSPDBEntities())
            {
                int CurrentTVLevel = 20;
                foreach (DBTable dbTable in AllTablesToDelete)
                {

                    lblStatus.Text = "Deleting Table  [" + dbTable.TableName + dbTable.Plurial + "]";
                    lblStatus.Refresh();
                    Application.DoEvents();

                    richTextBoxStatus.AppendText("Deleting Table [" + dbTable.TableName + dbTable.Plurial + "]\r\n");

                    int HasMore = 1;
                    int Count = 0;
                    while (HasMore > 0)
                    {
                        Count += 1;
                        lblStatus.Text = "Deleting Table  [" + dbTable.TableName + dbTable.Plurial + "] [" + (Count * 10000) + "]";
                        lblStatus.Refresh();
                        Application.DoEvents();

                        if ((dbTable.TableName + dbTable.Plurial) == "TVItems")
                        {
                            HasMore = dbMVC.Database.ExecuteSqlCommand("DELETE TOP (10000) FROM " + dbTable.TableName + dbTable.Plurial + " WHERE TVLevel = " + CurrentTVLevel);
                            if (HasMore == 0)
                            {
                                if (CurrentTVLevel > 0)
                                {
                                    CurrentTVLevel -= 1;
                                    HasMore = 1;
                                }
                            }
                        }
                        else
                        {
                            HasMore = dbMVC.Database.ExecuteSqlCommand("DELETE TOP (10000) FROM " + dbTable.TableName + dbTable.Plurial);
                        }
                    }
                    richTextBoxStatus.AppendText("Deleted Table [" + dbTable.TableName + dbTable.Plurial + "]\r\n");

                    lblStatus.Text = "Deleted Table [" + dbTable.TableName + dbTable.Plurial + "]";
                    lblStatus.Refresh();
                    Application.DoEvents();

                    try
                    {
                        dbMVC.Database.ExecuteSqlCommand("DBCC CHECKIDENT('" + dbTable.TableName + dbTable.Plurial + "', RESEED, 0)");
                    }
                    catch (Exception)
                    {
                        // some table don't have identity
                    }
                }

            }
            lblStatus.Text = "Done ...  deleting tables";

            return true;
        }      
    }
}
