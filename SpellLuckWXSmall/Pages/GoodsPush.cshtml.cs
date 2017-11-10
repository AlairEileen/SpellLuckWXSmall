using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using SpellLuckWXSmall.Models;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Tools;
using System.IO;
using MongoDB.Bson;
using Tools.ResponseModels;
using Microsoft.AspNetCore.Http;
using System.Threading;
using Tools.Models;
using Tools.DB;
using System.ComponentModel.DataAnnotations;

namespace SpellLuckWXSmall.Pages
{
    public class GoodsPushModel : PageModel
    {
        public string Message { get; set; }

        private IHostingEnvironment hostingEnvironment;
        public GoodsPushModel(IHostingEnvironment environment)
        {
            this.hostingEnvironment = environment;
        }
        public void OnGet()
        {
            Message = "Your application description page.";
        }
        [BindProperty]
        public GoodsModel GoodsModel { get; set; }


        [Required]
        [BindProperty]
        public List<IFormFile> MainImages { get; set; }

        [Required]
        [BindProperty]
        public List<IFormFile> OtherImages { get; set; }
        [Required]
        [BindProperty]
        public string GoodsColor { get; set; }
        [Required]
        [BindProperty]
        public string GoodsRule { get; set; }
        [BindProperty]
        [Required]
        public IFormFile ListImage { get; set; }
        public async Task<IActionResult> OnPostSubmitGoodsAsync()
        {

            if (CheckHasNull())
            {
                return RedirectToPage("/ErrorNull");
            }
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();
            if (MainImages.Count > 5)
            {
                MainImages.RemoveRange(5, MainImages.Count - 5);
            }
            if (GoodsModel.GoodsMainImages == null)
            {
                GoodsModel.GoodsMainImages = new List<FileModel<string[]>>();
            }
            if (GoodsModel.GoodsOtherImages == null)
            {
                GoodsModel.GoodsOtherImages = new List<FileModel<string[]>>();
            }
            await SaveImage(ListImage);
            await SaveImages(MainImages, GoodsModel.GoodsMainImages);
            await SaveImages(OtherImages, GoodsModel.GoodsOtherImages);
            saveColor();
            saveRule();
            if (GoodsModel.GoodsPayType == 2)
            {
                GoodsModel.GoodsPrice = new decimal(0.01);
            }
            try
            {
                new MongoDBTool().GetMongoCollection<GoodsModel>().InsertOne(GoodsModel);
                responseModel.StatusCode = (int)ActionParams.code_ok;
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
                throw;
            }

            return RedirectToPage("/GoodsPushFinish");
        }

        private bool CheckHasNull()
        {
            bool hasNull = false;
            if (
                OtherImages == null || OtherImages.Count == 0
               || MainImages == null || MainImages.Count == 0
               || ListImage == null
                || string.IsNullOrEmpty(GoodsColor)
                || string.IsNullOrEmpty(GoodsRule)
                || string.IsNullOrEmpty(GoodsModel.GoodsTitle)
                || string.IsNullOrEmpty(GoodsModel.GoodsDetail)
                || GoodsModel.GoodsOldPrice == 0
                )
            {
                hasNull = true;
            }
            return hasNull;
        }

        private void saveRule()
        {
            var rules = GoodsRule.Split("\r\n");
            if (GoodsModel.GoodsRule == null)
            {
                GoodsModel.GoodsRule = new List<string>();
            }
            for (int i = 0; i < rules.Length; i++)
            {
                GoodsModel.GoodsRule.Add(rules[i]);
            }
        }

        private void saveColor()
        {
            var colors = GoodsColor.Split("\r\n");
            if (GoodsModel.GoodsColor == null)
            {
                GoodsModel.GoodsColor = new List<string>();
            }
            for (int i = 0; i < colors.Length; i++)
            {
                GoodsModel.GoodsColor.Add(colors[i]);
            }
        }

        private async Task<long> SaveImage(IFormFile listImage)
        {
            return await Task.Run(() =>
            {

                long size = 0;
                if (listImage != null)
                {
                    var filename = ContentDispositionHeaderValue
                                    .Parse(listImage.ContentDisposition)
                                    .FileName
                                    .Trim('"');
                    string saveDir = $@"{ConstantProperty.BaseDir}{ConstantProperty.GoodsImagesDir}";
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }
                    filename = filename.Substring(filename.LastIndexOf("."));
                    string saveName = ConstantProperty.GoodsImagesDir + ObjectId.GenerateNewId().ToString() + $@"{filename}";
                    filename = ConstantProperty.BaseDir + saveName;
                    size += listImage.Length;
                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        listImage.CopyTo(fs);
                        fs.Flush();

                    }
                    ParamsCreate3Img params3Img = new ParamsCreate3Img() { FileName = filename, FileDir = ConstantProperty.GoodsImagesDir };
                    params3Img.OnFinish += fileModel =>
                    {
                        if (fileModel.FileID == ObjectId.Empty)
                        {
                            fileModel.FileID = ObjectId.GenerateNewId();
                        }
                        GoodsModel.GoodsListImage = fileModel;
                    };
                    ImageTool.Create3Img(params3Img);
                }
                return size;
            });
        }

        private async Task<long> SaveImages(List<IFormFile> sourceImages, List<FileModel<string[]>> container)
        {
            return await Task.Run(() =>
            {

                long size = 0;
                foreach (var file in sourceImages)
                {
                    var filename = ContentDispositionHeaderValue
                                    .Parse(file.ContentDisposition)
                                    .FileName
                                    .Trim('"');
                    string saveDir = $@"{ConstantProperty.BaseDir}{ConstantProperty.GoodsImagesDir}";
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }
                    filename = filename.Substring(filename.LastIndexOf("."));
                    string saveName = ConstantProperty.GoodsImagesDir + ObjectId.GenerateNewId().ToString() + $@"{filename}";
                    filename = ConstantProperty.BaseDir + saveName;
                    size += file.Length;
                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        file.CopyTo(fs);
                        fs.Flush();

                    }
                    ParamsCreate3Img params3Img = new ParamsCreate3Img() { FileName = filename, FileDir = ConstantProperty.GoodsImagesDir };
                    params3Img.OnFinish += fileModel =>
                    {
                        if (fileModel.FileID == ObjectId.Empty)
                        {
                            fileModel.FileID = ObjectId.GenerateNewId();
                        }
                        container.Add(fileModel);
                    };
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(ImageTool.Create3Img), params3Img);
                    //Thread thread = new Thread(new ParameterizedThreadStart(ImageTool.Create3Img));
                    //thread.IsBackground = false;
                    //thread.Start(params3Img);
                    ImageTool.Create3Img(params3Img);
                }
                return size;
            });



        }
    }

}

