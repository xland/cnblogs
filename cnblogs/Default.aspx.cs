using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace cnblogs
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = base.Request.Form["Action"];
            if (!string.IsNullOrEmpty(action))
            {
                Response.Clear();
                if (action == "Save")
                {
                    this.Save();
                }
                else if (action == "GetLast")
                {
                    GetLast();
                }
                else
                {
                    this.Login();
                }
                Response.End();
            }
        }
        private void GetLast()
        {
            string CnBlogsUserName = base.Request.Form["CnBlogsUserName"];
            string CnBlogsPassWord = base.Request.Form["CnBlogsPassWord"];
            string ApiAddress = Request.Form["ApiAddress"];
            CnBlogsUserName = Decrypt(CnBlogsUserName);
            CnBlogsPassWord = Decrypt(CnBlogsPassWord);
            ApiAddress = Decrypt(ApiAddress);
            IgetCatList categories = (IgetCatList)XmlRpcProxyGen.Create(typeof(IgetCatList));
            ((XmlRpcClientProtocol)categories).Url = ApiAddress;
            try
            {
                var obj = categories.getRecentPosts(CnBlogsUserName, CnBlogsUserName, CnBlogsPassWord, 1);
                if (obj.Length < 1)
                {
                    Response.Write("没有获取到博客");
                    return;
                }
                string sRet = new JavaScriptSerializer().Serialize(obj[0]);
                Response.Write(sRet);
            }
            catch
            {
                Response.Write("获取最近一篇博客出现异常");
            }
        }
        private void Save()
        {
            int imgErr = 0;
            string CnBlogsUserName = Request.Form["CnBlogsUserName"];
            string CnBlogsPassWord = Request.Form["CnBlogsPassWord"];
            string ApiAddress = Request.Form["ApiAddress"];
            CnBlogsUserName = Decrypt(CnBlogsUserName);
            CnBlogsPassWord = Decrypt(CnBlogsPassWord);
            ApiAddress = Decrypt(ApiAddress);
            string[] picRes = Request.Form["PicRes"].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            IgetCatList categories = (IgetCatList)XmlRpcProxyGen.Create(typeof(IgetCatList));
            ((XmlRpcClientProtocol)categories).Url = ApiAddress;
            BlogEntity BlogInfo = default(BlogEntity);
            BlogInfo.title = Request.Form["BlogTitle"];
            BlogInfo.description = Request.Form["BlogBody"];
            BlogInfo.dateCreated = DateTime.Now;
            BlogInfo.postid = Request.Form["BlogId"];
            for (int i = 0; i < picRes.Length; i++)
            {
                FileData fd = default(FileData);
                string picPath = base.Server.MapPath(picRes[i]);
                fd.bits = File.ReadAllBytes(picPath);
                fd.name = Path.GetExtension(picPath);
                fd.type = string.Format("image/{0}", fd.name.Substring(1));
                try
                {
                    FileData obj = categories.newMediaObject(CnBlogsUserName, CnBlogsUserName, CnBlogsPassWord, fd);
                    BlogInfo.description = BlogInfo.description.Replace(picRes[i], obj.url);
                }
                catch
                {
                    imgErr++;
                }
            }
            try
            {
                if (string.IsNullOrWhiteSpace(BlogInfo.postid))
                {
                    categories.newPost(string.Empty, CnBlogsUserName, CnBlogsPassWord, BlogInfo, false);
                }
                else
                {

                    categories.editPost(BlogInfo.postid, CnBlogsUserName, CnBlogsPassWord, BlogInfo, false);
                }
            }
            catch (Exception)
            {
                Response.Write("博客发送失败");
                return;
            }
            if (imgErr < 1)
            {
                Response.Write("博客发送成功");
                return;
            }
            Response.Write(string.Format("博客虽然发送成功了，但是有{0}张图片发送失败了", imgErr));
        }
        private void Login()
        {
            string CnBlogsUserName = Request.Form["UserName"];
            string CnBlogsPassWord = Request.Form["PassWord"];
            string ApiAddress = Request.Form["ApiAddress"];
            var result = new
            {
                CnBlogsUserName = Encrypt(CnBlogsUserName),
                CnBlogsPassWord = Encrypt(CnBlogsPassWord),
                ApiAddress = Encrypt(ApiAddress)
            };
            string sRet = new JavaScriptSerializer().Serialize(result);
            Response.Write(sRet);
        }
        private string Encrypt(string src)
        {
            string key = ConfigurationManager.AppSettings["DES_KEY"];
            string iv = ConfigurationManager.AppSettings["DES_IV"];
            byte[] btKey = Encoding.UTF8.GetBytes(key);
            byte[] btIV = Encoding.UTF8.GetBytes(iv);
            string result = "";
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] inData = Encoding.UTF8.GetBytes(src);
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(btKey, btIV), CryptoStreamMode.Write))
                {
                    cs.Write(inData, 0, inData.Length);
                    cs.FlushFinalBlock();
                }
                result = Convert.ToBase64String(ms.ToArray());
            }
            return result;
        }
        private string Decrypt(string src)
        {
            string key = ConfigurationManager.AppSettings["DES_KEY"];
            string iv = ConfigurationManager.AppSettings["DES_IV"];
            byte[] btKey = Encoding.UTF8.GetBytes(key);
            byte[] btIV = Encoding.UTF8.GetBytes(iv);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            string result = "";
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] inData = Convert.FromBase64String(src);
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(btKey, btIV), CryptoStreamMode.Write))
                {
                    cs.Write(inData, 0, inData.Length);
                    cs.FlushFinalBlock();
                }
                result = Encoding.UTF8.GetString(ms.ToArray());
            }
            return result;
        }
    }
}