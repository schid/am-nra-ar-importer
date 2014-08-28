using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.NRA.ILA.Importer.Umb.Models
{
    public class Video
    {
        private string _documentType = "Video";
        private string _nodeName;

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
        public string LegacyVideoStream { get; set; }
        public string LegacyVideoFlashStream { get; set; }
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
        public string Poster { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public string TopicTags { get; set; }
        public bool UmbracoNaviHide { get; set; }
        public string UrlList { get; set; }
        public string VideoData { get; set; }

        public Video(int parentId, string nodeName)
        {
            _nodeName = nodeName;
            this.ContentBuckets = string.Empty;
            this.GeographicTags = string.Empty;
            this.MetaDescription = string.Empty;
            this.MetaKeywords = string.Empty;
            this.ParentId = parentId;
            this.Poster = string.Empty;
            this.PublicationDate = DateTime.Now;
            this.Thumbnail = string.Empty;
            this.Title = string.Empty;
            this.TopicTags = string.Empty;
            this.UmbracoNaviHide = false;
            this.UrlList = string.Empty;
            this.VideoData = string.Empty;
        }
    }
}