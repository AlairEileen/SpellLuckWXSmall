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
            setPage(0);
        }
        public IActionResult OnGetGoPage(int pageIndex)
        {
            if (pageIndex == -1)
            {
                return Page();
            }
            setPage(pageIndex);
            
            return Page();
        }

        private void setPage(int pageIndex)
        {
            PageIndex = pageIndex;
            var filter = Builders<GoodsModel>.Filter.Empty;
            var find = new MongoDBTool().GetMongoCollection<GoodsModel>().Find(filter);
            var count = find.Count();
            PageCount = ((int)count / pageSize) + (count % pageSize == 0 ? 0 : 1);
            GoodsModelList = find.Skip(PageIndex * pageSize).Limit(PageSize).ToList();
        }
    }
}
