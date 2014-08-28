using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.NRA.ILA.Importer.Umb.Models
{
    public class Job
    {
        public enum ContentDocumentType
        {
            None,
            Article,
            Video,
            Audio,
            PDF
        };

        private DateTime _created;

        public string ContentType { get; set; }
        public DateTime Created
        {
            get
            {
                return _created;
            }
        }
        public string EndDate { get; set; }
        public int ItemCount { get; set; }
        public int ItemTotal { get; set; }
        public ContentDocumentType JobContentType
        {
            get
            {
                if (this.ContentType != null)
                {
                    switch (this.ContentType.ToLower())
                    {
                        case "article":
                            return ContentDocumentType.Article;
                        case "video":
                            return ContentDocumentType.Video;
                        case "audio":
                            return ContentDocumentType.Audio;
                        case "pdf":
                            return ContentDocumentType.PDF;
                        default:
                            return ContentDocumentType.None;
                    }
                }
                else
                {
                    return ContentDocumentType.None;
                }
            }
        }
        public string SourceFile { get; set; }
        public string StartDate { get; set; }
        public int User { get; set; }

        public Job()
        {
            _created = DateTime.Now;
        }
    }
}