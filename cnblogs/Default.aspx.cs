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
                base.Response.Clear();
                if (!(action == "Login"))
                {
                    if (action == "Save")
                    {
                        this.Save();
                    }
                }
                else
                {
                    this.Login();
                }
                base.Response.End();
            }
        }
        private void Save()
        {
            int imgErr = 0;
            string CnBlogsUserName = base.Request.Form["CnBlogsUserName"];
            string CnBlogsPassWord = base.Request.Form["CnBlogsPassWord"];
            CnBlogsUserName = this.Decrypt(CnBlogsUserName);
            CnBlogsPassWord = this.Decrypt(CnBlogsPassWord);
            string[] picRes = base.Request.Form["PicRes"].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            IgetCatList categories = (IgetCatList)XmlRpcProxyGen.Create(typeof(IgetCatList));
            ((XmlRpcClientProtocol)categories).Url = string.Format("http://rpc.cnblogs.com/metaweblog/{0}", CnBlogsUserName);
            BlogEntity BlogInfo = default(BlogEntity);
            BlogInfo.title = base.Request.Form["BlogTitle"];
            BlogInfo.description = base.Request.Form["BlogBody"];
            BlogInfo.dateCreated = DateTime.Now;
            for (int i = 0; i < picRes.Length; i++)
            {
                FileData fd = default(FileData);
                string picPath = base.Server.MapPath(picRes[i]);
                fd.bits = File.ReadAllBytes(picPath);
                fd.name = Path.GetExtension(picPath);
                fd.type = string.Format("image/{0}", fd.name.Substring(1));
                try
                {
                    IgetCatList arg_13F_0 = categories;
                    string expr_13B = CnBlogsUserName;
                    FileData obj = arg_13F_0.newMediaObject(expr_13B, expr_13B, CnBlogsPassWord, fd);
                    BlogInfo.description = BlogInfo.description.Replace(picRes[i], obj.url);
                }
                catch
                {
                    imgErr++;
                }
            }
            try
            {
                categories.newPost(string.Empty, CnBlogsUserName, CnBlogsPassWord, BlogInfo, false);
            }
            catch (Exception)
            {
                base.Response.Write("博客发送失败");
                return;
            }
            if (imgErr < 1)
            {
                base.Response.Write("博客发送成功");
                return;
            }
            base.Response.Write(string.Format("博客虽然发送成功了，但是有{0}张图片发送失败了", imgErr));
        }
        private void Login()
        {
            string CnBlogsUserName = base.Request.Form["UserName"];
            string CnBlogsPassWord = base.Request.Form["PassWord"];
            var result = new
            {
                CnBlogsUserName = this.Encrypt(CnBlogsUserName),
                CnBlogsPassWord = this.Encrypt(CnBlogsPassWord)
            };
            string sRet = new JavaScriptSerializer().Serialize(result);
            base.Response.Write(sRet);
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