using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpellLuckWXSmall.Models;
using Tools.DB;
using MongoDB.Driver;
using System.Text;

namespace SpellLuckWXSmall.Pages
{
    public class SettingsModel : PageModel
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
            Company = new MongoDBTool().GetMongoCollection<CompanyModel>().Find(Builders<CompanyModel>.Filter.Empty).FirstOrDefault();
            company = Company;

            if (company != null && company.CompanyAccountList != null && company.CompanyAccountList.Count != 0)
            {
                CAMOut = company.CompanyAccountList.FirstOrDefault();
            }
            else
            {
                CAMOut = null;
            }
        }
        [BindProperty]
        public CompanyModel Company { get; set; }
        public IActionResult OnPostSubmitCompany()
        {
            if (string.IsNullOrEmpty(Company.CompanyName) && string.IsNullOrEmpty(Company.ServicePhone))
            {
                return Page();
            }
            var filter = Builders<CompanyModel>.Filter.Empty;
            var collection = new MongoDBTool().GetMongoCollection<CompanyModel>();
            var company = collection.Find(filter).FirstOrDefault();
            if (company == null)
            {
                company = Company;
                collection.InsertOne(company);
            }
            else
            {
                Company.CompanyID = company.CompanyID;
                collection.UpdateOne(x => x.CompanyID.Equals(company.CompanyID), Builders<CompanyModel>.Update.Set(x => x.CompanyName, Company.CompanyName).Set(x => x.ServicePhone, Company.ServicePhone));
            }
            OnGet();
            return Page();
        }

        public IActionResult OnPostSubmitCompanyTime()
        {
            var filter = Builders<CompanyModel>.Filter.Empty;
            var collection = new MongoDBTool().GetMongoCollection<CompanyModel>();
            var company = collection.Find(filter).FirstOrDefault();
            if (company == null)
            {
                company = Company;
                collection.InsertOne(company);
            }
            else
            {
                Company.CompanyID = company.CompanyID;
                collection.UpdateOne(x => x.CompanyID.Equals(company.CompanyID), Builders<CompanyModel>.Update.Set(x => x.TimeOpenJack,
                    new TimeOpenJack()
                    {
                        JackPotTimerHour = Company.TimeOpenJack.JackPotTimerHour,
                        JackPotTimerMinute = Company.TimeOpenJack.JackPotTimerMinute
                    }
                    ));
            }
            OnGet();
            return Page();
        }
        private CompanyModel GetCompany()
        {
            return mongo.GetMongoCollection<CompanyModel>().Find(Builders<CompanyModel>.Filter.Empty).FirstOrDefault();

        }

        public async Task<IActionResult> OnPostLogin()
        {
            CAMOut = GetCAM();
            if (!CAM.CompanyAccountPassword.Equals(CAM.CompanyAccountVerifyPassword))
            {
                ErrorVerify = true;
                OnGet();
                return Page();
            }
            if (!CAM.CompanyAccountOlderPassword.Equals(CAMOut.CompanyAccountPassword))
            {
                ErrorAccount = true;
                OnGet();
                return Page();
            }
            await DoSetAdmin();
            HttpContext.Session.Set("CompanyAccountName", Encoding.UTF8.GetBytes(CAM.CompanyAccountName.ToString()));
            OnGet();
            return Page();
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
                    if (company != null && company.CompanyAccountList != null)
                    {
                        mongo.GetMongoCollection<CompanyModel>().UpdateOne(x => x.CompanyID.Equals(company.CompanyID), Builders<CompanyModel>.Update.Set(x => x.CompanyAccountList, list));
                    }
                  
                    return true;
                }
                );
        }
    }
}