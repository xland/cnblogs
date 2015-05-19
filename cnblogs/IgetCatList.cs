using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cnblogs
{
    public interface IgetCatList
    {
        [XmlRpcMethod("metaWeblog.newPost")]
        string newPost(string blogid, string username, string password, BlogEntity post, bool publish);
        [XmlRpcMethod("metaWeblog.newMediaObject")]
        FileData newMediaObject(string blogid, string username, string password, FileData file);
    }
}