using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.NRA.ILA.Importer.Umb.Models
{
    public class EVC
    {
        private int _parentId;
        private string _nodeName;

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

        public string FederalDistrictCode { get; set; }
        public string Name { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string CategoryTags { get; set; }
        public string StateTags { get; set; }

        public EVC(int ParentId, string NodeName)
        {
            _parentId = ParentId;
            _nodeName = NodeName;
        }
    }
}