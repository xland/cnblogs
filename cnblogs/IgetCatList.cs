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
        [XmlRpcMethod("metaWeblog.getRecentPosts")]
        BlogEntity[] getRecentPosts(string blogid, string username, string password, int numberOfPosts);
        [XmlRpcMethod("metaWeblog.editPost")]
        object editPost(string postid, string username, string password, object post, bool publish);
    }
}