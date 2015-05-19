using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cnblogs
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct FileData
    {
        public byte[] bits;
        public string name;
        public string type;
        public bool overwrite;
        public int gallery;
        public int image_id;
        public string id;
        public string file;
        public string url;
    }
}
