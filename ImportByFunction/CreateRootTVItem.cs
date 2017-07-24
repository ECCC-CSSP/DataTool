using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSSPModelsDLL.Models;
using CSSPWebToolsDBDLL.Services;
using CSSPEnumsDLL.Enums;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        private bool CreateRootTVItem()
        {
            if (Cancel) return false;

            lblStatus.Text = "CreateRootTVItem ... ";
            lblStatus.Refresh();
            Application.DoEvents();

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.PostAddRootTVItemDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            return true;
        }
    }
}
