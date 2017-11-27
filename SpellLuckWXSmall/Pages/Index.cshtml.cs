using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpellLuckWXSmall.Models;
using Tools.DB;
using MongoDB.Driver;
using SpellLuckWXSmall.Interceptors;
using System.Text;

namespace SpellLuckWXSmall.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public CompanyAccountModel CAM { get; set; }
        MongoDBTool mongo = new MongoDBTool();

        public CompanyAccountModel CAMOut { get; set; }
        private CompanyModel company;
        public bool ErrorAccount { get; set; }
        public bool ErrorVerify { get; set; }
        public void OnGet()
        {
            company = GetCompany();

            if (company != null && company.CompanyAccountList != null && company.CompanyAccountList.Count != 0)
            {
                CAMOut = company.CompanyAccountList.FirstOrDefault();
            }
            else
            {
                CAMOut = null;
            }
        }

        private CompanyModel GetCompany()
        {
            return mongo.GetMongoCollection<CompanyModel>().Find(Builders<CompanyModel>.Filter.Empty).FirstOrDefault();

        }

        public async Task<IActionResult> OnPostLogin()
        {

            bool isSuccess;
            CAMOut = GetCAM();
            if (CAMOut == null)
            {
                if (!CAM.CompanyAccountPassword.Equals(CAM.CompanyAccountVerifyPassword))
                {
                    ErrorVerify = true;
                    OnGet();
                    return Page();
                }
                isSuccess = await DoSetAdmin();
            }
            else
            {
                isSuccess = await DoLogin();
            }
            if (isSuccess)
            {
                HttpContext.Session.Set("CompanyAccountName", Encoding.UTF8.GetBytes(CAM.CompanyAccountName.ToString()));
                return RedirectToPage("/OrderListShow");
            }
            else
            {
                OnGet();
                ErrorAccount = true;
                return Page();
            }
        }

        private async Task<bool> DoLogin()
        {
            return await Task.Run(() =>
            {
                CAMOut = GetCAM();
                if (CAM.CompanyAccountPassword.Equals(CAMOut.CompanyAccountPassword) && CAM.CompanyAccountName.Equals(CAMOut.CompanyAccountName))
                {
                    return true;
                }
                return false;
            });

        }

        private CompanyAccountModel GetCAM()
        {
            var company = GetCompany();
            if (company.CompanyAccountList == null || company.CompanyAccountList.Count == 0)
            {
                return null;
            }
            return company.CompanyAccountList.FirstOrDefault();
        }

        private async Task<bool> DoSetAdmin()
        {
            return await Task.Run(
                () =>
                {
                    List<CompanyAccountModel> list = new List<CompanyAccountModel>() { CAM };
                    company = GetCompany();
                    if (company != null && company.CompanyAccountList == null)
                    {
                        mongo.GetMongoCollection<CompanyModel>().UpdateOne(x => x.CompanyID.Equals(company.CompanyID), Builders<CompanyModel>.Update.Set(x => x.CompanyAccountList, list));
                    }
                    else if (company == null)
                    {
                        mongo.GetMongoCollection<CompanyModel>().InsertOne(new CompanyModel() { CompanyAccountList = list });
                    }
                    return true;
                }
                );
        }
    }
}
