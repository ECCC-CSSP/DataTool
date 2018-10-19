using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;
using CSSPEnumsDLL.Enums;
using CSSPModelsDLL.Models;
using CSSPDBDLL;
using CSSPDBDLL.Services;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        private bool CreateFirstNewUser()
        {
            if (Cancel) return false;

            lblStatus.Text = "Creating First new users...";
            lblStatus.Refresh();
            Application.DoEvents();

            RegisterModel registerModelFirstUser = new RegisterModel() { FirstName = "Charles", Initial = "G", LastName = "LeBlanc", WebName = "Charles", LoginEmail = "Charles.LeBlanc2@canada.ca", Password = "abcde2!", ConfirmPassword = "abcde2!", IsAdmin = true, EmailValidated = true, Disabled = false };

            if (registerModelFirstUser.Password.Length < 6)
            {
                richTextBoxStatus.AppendText(string.Format("Password length should be > 5 characters. FirstName [{0}] Initial [{1}] LastName [{2}]", registerModelFirstUser.FirstName, registerModelFirstUser.Initial, registerModelFirstUser.LastName));
                return false;
            }

            if (registerModelFirstUser.Password != registerModelFirstUser.ConfirmPassword)
            {
                richTextBoxStatus.AppendText("Password and ConfirmPassword not equal");
                return false;
            }

            AspNetUserModel aspNetUserModelNew = new AspNetUserModel();
            aspNetUserModelNew.LoginEmail = registerModelFirstUser.LoginEmail;
            aspNetUserModelNew.Password = registerModelFirstUser.Password;

            AspNetUserService aspNetUserService = new AspNetUserService(LanguageEnum.en, user);
            AspNetUserModel aspNetUserModel = aspNetUserService.PostAddFirstAspNetUserDB(aspNetUserModelNew);
            if (!CheckModelOK<AspNetUserModel>(aspNetUserModel)) return false;

            user = new GenericPrincipal(new GenericIdentity("Charles.LeBlanc2@canada.ca", "Forms"), null);

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            ContactModel contactModelNew = new ContactModel();
            contactModelNew.Id = aspNetUserModel.Id;
            contactModelNew.ContactTVItemID = tvItemModelRoot.TVItemID;
            contactModelNew.LoginEmail = registerModelFirstUser.LoginEmail;
            contactModelNew.FirstName = registerModelFirstUser.FirstName;
            contactModelNew.LastName = registerModelFirstUser.LastName;
            contactModelNew.Initial = registerModelFirstUser.Initial;
            contactModelNew.WebName = registerModelFirstUser.WebName;
            contactModelNew.IsAdmin = registerModelFirstUser.IsAdmin;
            contactModelNew.EmailValidated = registerModelFirstUser.EmailValidated;
            contactModelNew.Disabled = registerModelFirstUser.Disabled;

            ContactService contactService = new ContactService(LanguageEnum.en, user);

            string TVText = contactService.CreateTVText(contactModelNew);
            if (string.IsNullOrWhiteSpace(TVText)) return false;

            TVItemModel tvItemModelContact = tvItemService.PostAddChildContactTVItemDB(tvItemModelRoot.TVItemID, TVText, TVTypeEnum.Contact);
            if (!CheckModelOK<TVItemModel>(tvItemModelContact)) return false;

            contactModelNew.ContactTVItemID = tvItemModelContact.TVItemID;
            ContactModel contactModelRet = contactService.PostAddFirstContactDB(contactModelNew);
            if (!CheckModelOK<ContactModel>(contactModelRet)) return false;

            return true;
        }
    }
}
