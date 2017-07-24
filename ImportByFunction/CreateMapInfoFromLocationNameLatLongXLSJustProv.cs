using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        private bool CreateMapInfoFromLocationNameLatLongXLSJustProv()
        {
            lblStatus.Text = "Starting ... CreateMapInfoFromLocationNameLatLongXLSJustProv";
            Application.DoEvents();

            string Prov = "";
            string FuncName = "CreateMapInfoFromLocationNameLatLongXLSJustProv";

            if (!CreateMapInfoFromLocationNameLatLongXLSAll(Prov))
            {
                richTextBoxStatus.AppendText("Error in LoadTVItemsAll with FuncName [" + FuncName + "] Prov [" + Prov + "]\r\n");
                return false;
            }

            return true;
        }
    }
}
