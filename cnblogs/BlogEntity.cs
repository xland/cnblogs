using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cnblogs
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct BlogEntity
    {
        public DateTime dateCreated;
        public string description;
        public string title;
        public string postid;
        public string link;
    }
}
