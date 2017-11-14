using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpellLuckWXSmall.Models;
using Tools.DB;
using MongoDB.Driver;

namespace SpellLuckWXSmall.Pages
{
    public class SettingsModel : PageModel
    {
        public void OnGet()
        {
            Company = new MongoDBTool().GetMongoCollection<CompanyModel>().Find(Builders<CompanyModel>.Filter.Empty).FirstOrDefault();

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
                collection.ReplaceOne(x=>x.CompanyID.Equals(company.CompanyID),Company);
            }

            return Page();
        }
    }
}