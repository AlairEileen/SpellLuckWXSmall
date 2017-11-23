using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpellLuckWXSmall.Models;
using Microsoft.AspNetCore.Mvc;
using Tools.DB;
using MongoDB.Driver;
using MongoDB.Bson;

namespace SpellLuckWXSmall.Pages
{
    public class GoodsListShowModel : PageModel
    {
        public List<GoodsModel> GoodsModelList { get; set; }
        private int pageSize = 5;
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        private int maxPageShow = 10;
        public int MaxPageShow { get => maxPageShow; }
        public int PageSize { get => pageSize; }

        public GoodsModel SelectedGoods { get; set; }
        public void OnGet()
        {
           SetPage(0);
        }
        public async Task<IActionResult> OnGetGoPageAsync(int pageIndex)
        {
            if (pageIndex == -1 || pageIndex == PageCount)
            {
                return RedirectToPage();
            }
            await Task.Run(()=> {
                SetPage(pageIndex);
            });

            return Page();
        }

        private void SetPage(int pageIndex)
        {
      
               PageIndex = pageIndex;
               var filter = Builders<GoodsModel>.Filter.Empty;
               var find = new MongoDBTool().GetMongoCollection<GoodsModel>().Find(filter);
               var count = find.Count();
               PageCount = ((int)count / pageSize) + (count % pageSize == 0 ? 0 : 1);
               GoodsModelList = find.Skip(PageIndex * pageSize).Limit(PageSize).ToList();
         
        }

        public async Task<IActionResult> OnGetDelGoods(string goodsId)
        {
           await Task.Run(()=> {

                var filter = Builders<GoodsModel>.Filter.Eq(x => x.GoodsID, new ObjectId(goodsId));
                new MongoDBTool().GetMongoCollection<GoodsModel>().DeleteOne(filter);
            });
         
            return RedirectToPage();
        }
    }
}
