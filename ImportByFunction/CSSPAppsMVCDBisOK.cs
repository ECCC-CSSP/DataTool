using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSSPDBDLL.Services;
using CSSPEnumsDLL.Enums;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        private bool CSSPDBisOK()
        {
            if (Cancel) return false;

            TestDBService testDBService = new TestDBService(LanguageEnum.en, user, "", "");

            lblStatus.Text = "Checking DB";
            lblStatus.Refresh();
            Application.DoEvents();

            string retStr = testDBService.CSSPDBIsOK();
            if (!string.IsNullOrWhiteSpace(retStr))
            {
                richTextBoxStatus.AppendText(retStr + "\r\n");
                richTextBoxStatus.AppendText("Database not OK\r\n");
                return false;
            }

            return true;
        }
    }
}
