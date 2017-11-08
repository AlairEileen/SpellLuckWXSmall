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
        private int pageSize = 5;
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        private int maxPageShow=10;
        public int MaxPageShow { get => maxPageShow; }
        public int PageSize { get => pageSize; }

        public void OnGet()
        {

            PageIndex = 0;
            var filter = Builders<GoodsModel>.Filter.Empty;
            var find = new MongoDBTool().GetMongoCollection<GoodsModel>().Find(filter);
            GoodsModelList = find.Skip(PageIndex * pageSize).Limit(PageSize).ToList();
            PageCount = (int)find.Count() / pageSize;
        }
        public IActionResult OnGetGoPage(int pageIndex)
        {
            if (PageIndex == -1)
            {
                return Page();
            }
            PageIndex = pageIndex;
            var filter = Builders<GoodsModel>.Filter.Empty;
            GoodsModelList = new MongoDBTool().GetMongoCollection<GoodsModel>().Find(filter).Skip(PageIndex * pageSize).Limit(PageSize).ToList();
            return Page();
        }
    }
}
