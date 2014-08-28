using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.NRA.ILA.Importer.Umb.Models
{
    public class Pdf
    {
        private string _documentType = "PDF";
        private string _nodeName;

        public string BodyContent { get; set; }
        public string ContentBuckets { get; set; }
        public string DocumentType
        {
            get
            {
                return _documentType;
            }
        }
        public string GeographicTags { get; set; }
        public int ID { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string NodeName
        {
            get
            {
                return _nodeName;
            }
        }
        public int ParentId { get; set; }
        public string PdfFile { get; set; }
        public DateTime PublicationDate { get; set; }
        public string SummaryContent { get; set; }
        public string ThumbnailImage { get; set; }
        public string Title { get; set; }
        public string TopicTags { get; set; }
        public bool UmbracoNaviHide { get; set; }
        public string UrlList { get; set; }

        public Pdf(int parentId, string name)
        {
            this.ParentId = parentId;
            _nodeName = name;
            this.BodyContent = string.Empty;
            this.ContentBuckets = string.Empty;
            this.GeographicTags = string.Empty;
            this.MetaDescription = string.Empty;
            this.MetaKeywords = string.Empty;
            this.PdfFile = string.Empty;
            this.PublicationDate = DateTime.Now;
            this.SummaryContent = string.Empty;
            this.ThumbnailImage = string.Empty;
            this.Title = string.Empty;
            this.TopicTags = string.Empty;
            this.UmbracoNaviHide = false;
            this.UrlList = string.Empty;
        }
    }
}