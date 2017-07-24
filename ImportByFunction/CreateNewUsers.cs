using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSSPWebToolsDBDLL;
using System.Security.Principal;
using CSSPEnumsDLL.Enums;
using CSSPModelsDLL.Models;
using CSSPWebToolsDBDLL.Services; 

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        private bool CreateNewUsers()
        {

            lblStatus.Text = "Creating new users...";
            lblStatus.Refresh();
            Application.DoEvents();

            UserList = new List<RegisterModel>()
            { 
                new RegisterModel() { FirstName = "Christopher", Initial = "", LastName = "Roberts", WebName = "Christopher", LoginEmail = "Christopher.Roberts@ec.gc.ca", Password = "abcde2!", ConfirmPassword = "abcde2!", IsAdmin = false, EmailValidated = true, Disabled = false },
                new RegisterModel() { FirstName = "Martin", Initial = "", LastName = "Rodrigue", WebName = "Martin", LoginEmail = "Martin.Rodrigue@ec.gc.ca", Password = "abcde2!", ConfirmPassword = "abcde2!", IsAdmin = false, EmailValidated = false, Disabled = false },
            };

            for (int i = 0; i < UserList.Count() - 2; i++)
            {
                if (UserList[i].FirstName == UserList[i + 1].FirstName && UserList[i].Initial == UserList[i + 1].Initial && UserList[i].LastName == UserList[i + 1].LastName)
                {
                    richTextBoxStatus.AppendText("2 individual have the same full name.");
                    return false;
                }

            }

            foreach (RegisterModel rm in UserList)
            {
                if (rm.Password.Length < 6)
                {
                    richTextBoxStatus.AppendText(string.Format("Password length should be > 5 characters. FirstName [{0}] Initial [{1}] LastName [{2}]", rm.FirstName, rm.Initial, rm.LastName));
                    return false;
                }

                if (rm.Password != rm.ConfirmPassword)
                {
                    richTextBoxStatus.AppendText("Password and ConfirmPassword not equal");
                    return false;
                }
            }

            foreach (RegisterModel rm in UserList)
            {
                AspNetUserModel aspNetUserModelNew = new AspNetUserModel();
                aspNetUserModelNew.LoginEmail = rm.LoginEmail;
                aspNetUserModelNew.Password = rm.Password;

                AspNetUserService aspNetUserService = new AspNetUserService(LanguageEnum.en, user);
                AspNetUserModel aspNetUserModelRet = aspNetUserService.PostAddAspNetUserDB(aspNetUserModelNew, true);
                if (!CheckModelOK<AspNetUserModel>(aspNetUserModelRet)) return false;

                user = new GenericPrincipal(new GenericIdentity("Charles.LeBlanc2@canada.ca", "Forms"), null);

                TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
                TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
                if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

                ContactModel contactModelNew = new ContactModel();
                contactModelNew.Id = aspNetUserModelRet.Id;
                contactModelNew.ContactTVItemID = tvItemModelRoot.TVItemID;
                contactModelNew.LoginEmail = rm.LoginEmail;
                contactModelNew.FirstName = rm.FirstName;
                contactModelNew.LastName = rm.LastName;
                contactModelNew.Initial = rm.Initial;
                contactModelNew.WebName = rm.WebName;
                contactModelNew.IsAdmin = rm.IsAdmin;
                contactModelNew.EmailValidated = rm.EmailValidated;
                contactModelNew.Disabled = rm.Disabled;

                ContactService contactService = new ContactService(LanguageEnum.en, user);

                ContactModel contactModelAdmin = contactService.GetContactModelWithLoginEmailDB("Charles.LeBlanc2@canada.ca");
                if (!CheckModelOK<ContactModel>(contactModelAdmin)) return false;

                if (!contactModelAdmin.IsAdmin)
                    return false;

                string TVText = contactService.CreateTVText(contactModelNew);
                if (string.IsNullOrWhiteSpace(TVText)) return false;

                TVItemModel tvItemModelContact = tvItemService.PostAddChildTVItemDB(tvItemModelRoot.TVItemID, TVText, TVTypeEnum.Contact);
                if (!CheckModelOK<TVItemModel>(tvItemModelContact)) return false;

                contactModelNew.ContactTVItemID = tvItemModelContact.TVItemID;
                ContactModel contactModelRet = contactService.PostAddContactDB(contactModelNew);
                if (!CheckModelOK<ContactModel>(contactModelRet)) return false;

            }

            return true;
        }
    }
}
