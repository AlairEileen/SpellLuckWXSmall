using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpellLuckWXSmall.Models;
using Microsoft.AspNetCore.Mvc;
using Tools.DB;
using MongoDB.Driver;

namespace SpellLuckWXSmall.Pages
{
    public class GoodsListShowModel : PageModel
    {
    

        public List<GoodsModel> GoodsModelList { get; set; }
        private int pageSize=10;
        public int PageIndex { get; set; }
        public int PageSize { get => pageSize; }

        public void OnGet()
        {
            var filter = Builders<GoodsModel>.Filter.Empty ;
            GoodsModelList= new MongoDBTool().GetMongoCollection<GoodsModel>().Find(filter).Skip(PageIndex).Limit(PageSize).ToList();
        }

        public IActionResult OnGetNextPage()
        {
            PageIndex++;
            var filter = Builders<GoodsModel>.Filter.Empty;
            GoodsModelList = new MongoDBTool().GetMongoCollection<GoodsModel>().Find(filter).Skip(PageIndex).Limit(PageSize).ToList();
            return Page();
        }
    }
}
