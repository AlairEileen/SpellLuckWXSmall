using System;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Tools;
using System.IO;
using MongoDB.Driver;
using SpellLuckWXSmall.Models;
using Microsoft.AspNetCore.Hosting;
using Tools.ResponseModels;
using Newtonsoft.Json;
using System.Threading;
using MongoDB.Bson;
using System.Collections.Generic;
using Tools.DB;
using Tools.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SpellLuckWXSmall.Controllers
{
    public class FileController : Controller
    {

        private IHostingEnvironment hostingEnvironment;
        public FileController(IHostingEnvironment environment)
        {
            this.hostingEnvironment = environment;
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="fileId"></param>
        [HttpGet]
        public IActionResult FileDownload(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return null;
            }
            fileUrl = ConstantProperty.BaseDir + fileUrl;
            var stream = System.IO.File.OpenRead(fileUrl);
            return File(stream, "application/vnd.android.package-archive", Path.GetFileName(fileUrl));
        }

        /// <summary>
        /// 头像上传
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        public string UploadAvatar(ObjectId accountID)
        {
            BaseResponseModel<String> responseModel = new BaseResponseModel<String>();
            if (accountID == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                responseModel.JsonData = $@"参数：openId:{accountID}";
                return JsonConvert.SerializeObject(responseModel);
            }
            long size = 0;
            var files = Request.Form.Files;


            try
            {
                foreach (var file in files)
                {
                    var filename = ContentDispositionHeaderValue
                                    .Parse(file.ContentDisposition)
                                    .FileName
                                    .Trim('"');
                    string saveDir = $@"{ConstantProperty.BaseDir}{ConstantProperty.AvatarDir}";
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }
                    filename = filename.Substring(filename.LastIndexOf("."));
                    string saveName = ConstantProperty.AvatarDir + $@"{accountID}{filename}";
                    filename = ConstantProperty.BaseDir + saveName;
                    size += file.Length;
                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                        var filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, accountID);
                        var update = Builders<AccountModel>.Update.Set(x => x.AccountAvatar, saveName);
                        var dbTool = new MongoDBTool();
                        dbTool.GetMongoCollection<AccountModel>().UpdateOne(filter, update);
                    }

                }
                responseModel.StatusCode = (int)ActionParams.code_ok;
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
                throw;
            }
            return JsonConvert.SerializeObject(responseModel);
        }
       
       
        /// <summary>
        /// 上传单张图片
        /// </summary>
        /// <returns></returns>
        public string UploadImage(string openId)
        {
            long size = 0;
            var files = Request.Form.Files;
            string resultFileId = null;
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();

            try
            {
                foreach (var file in files)
                {
                    var filename = ContentDispositionHeaderValue
                                    .Parse(file.ContentDisposition)
                                    .FileName
                                    .Trim('"');
                    string saveDir = $@"{ConstantProperty.BaseDir}{ConstantProperty.AlbumDir}";
                    string dbSaveDir = $@"{ConstantProperty.AlbumDir}";
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }
                    string exString = filename.Substring(filename.LastIndexOf("."));
                    string saveName = Guid.NewGuid().ToString("N");
                    filename = $@"{saveDir}{saveName}{exString}";

                    size += file.Length;
                    FileModel<string[]> fileCard = new FileModel<string[]>();
                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                        string[] fileUrls = new string[] { $@"{dbSaveDir}{saveName}{exString}" };
                    }
                    ParamsCreate3Img params3Img = new ParamsCreate3Img() { FileName = filename ,FileDir=ConstantProperty.AlbumDir};
                    params3Img.OnFinish += fileModel => {
                        new MongoDBTool().GetMongoCollection<FileModel<string[]>>("FileModel").InsertOne(fileModel);
                        resultFileId = fileModel.FileID.ToString();
                    };
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(ImageTool.Create3Img), params3Img);
                    new Thread(new ParameterizedThreadStart(ImageTool.Create3Img)).Start(params3Img);

                }
                responseModel.StatusCode = (int)ActionParams.code_ok;
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
            }
            responseModel.JsonData = resultFileId;
            //return JsonConvert.SerializeObject(responseModel);
            return resultFileId;
        }

      
    }
}
