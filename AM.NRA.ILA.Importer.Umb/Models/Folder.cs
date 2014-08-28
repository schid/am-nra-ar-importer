using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.NRA.ILA.Importer.Umb.Models
{
    public class Folder
    {
        private int _parentId;
        private string _nodeName;
        private string _documentType;

        public int ParentId
        {
            get
            {
                return _parentId;
            }
        }

        public string NodeName
        {
            get
            {
                return _nodeName;
            }
        }

        public string DocumentType
        {
            get
            {
                return _documentType;
            }
        }

        public int ID { get; set; }

        public Folder(int ParentId, string NodeName, string DocumentType)
        {
            _parentId = ParentId;
            _nodeName = NodeName;
            _documentType = DocumentType;
        }
    }
}