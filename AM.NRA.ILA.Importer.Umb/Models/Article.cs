using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.NRA.ILA.Importer.Umb.Models
{
    public class Article
    {
        private string _documentType = "Article";
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
        public string Footnotes { get; set; }
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
        public string OgDescription { get; set; }
        public string OgImage { get; set; }
        public int ParentId { get; set; }
        public bool PinToTheTop { get; set; }
        public DateTime PublicationDate { get; set; }
        public string SummaryContent { get; set; }
        public string Title { get; set; }
        public string TopicTags { get; set; }
        public string Categories { get; set; }
        public string TwitterMessage { get; set; }
        public bool UmbracoNaviHide { get; set; }
        public string UrlList { get; set; }
        public string Author { get; set; }
        public string Image { get; set; }
        public string ByLine { get; set; }
        public string Pdf { get; set; }
        public string Video { get; set; }
        public string VideoFlv { get; set; }
        public string VideoMp4 { get; set; }
        public string UmbracoUrlAlias { get; set; }

        public Article(int parentId, string nodeName)
        {
            _nodeName = nodeName;
            this.BodyContent = string.Empty;
            this.ContentBuckets = string.Empty;
            this.Footnotes = string.Empty;
            this.GeographicTags = string.Empty;
            this.MetaDescription = string.Empty;
            this.MetaKeywords = string.Empty;
            this.OgDescription = string.Empty;
            this.OgImage = string.Empty;
            this.ParentId = parentId;
            this.PinToTheTop = false;
            this.PublicationDate = DateTime.Now;
            this.SummaryContent = string.Empty;
            this.Title = string.Empty;
            this.TopicTags = string.Empty;
            this.Categories = string.Empty;
            this.TwitterMessage = string.Empty;
            this.UmbracoNaviHide = false;
            this.UrlList = string.Empty;
            this.Author = string.Empty;
            this.Image = string.Empty;
            this.ByLine = string.Empty;
            this.Pdf = string.Empty;
            this.Video = string.Empty;
            this.VideoFlv = string.Empty;
            this.VideoMp4 = string.Empty;
            this.UmbracoUrlAlias = string.Empty;

        }
    }
}