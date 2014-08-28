using AM.NRA.ILA.Importer.Umb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace AM.NRA.ILA.Importer.Umb.Models.Repository
{
    public class Repository
    {
        private IContentService _contentService = ApplicationContext.Current.Services.ContentService;

        private int? ContentExists(int parentId, string name)
        {
            var files = _contentService.GetChildren(parentId);
            foreach (var f in files)
            {
                if (f.Name.ToLower() == name.ToLower())
                {
                    return f.Id;
                }
            }
            return null;
        }

        private int? ContentExists(int parentId, string name, int bodyCount)
        {
            var files = _contentService.GetChildren(parentId);
            foreach (var f in files)
            {
                if (f.Name.ToLower() == name.ToLower())
                {
                    if (f.GetValue("bodyContent").ToString().Length == bodyCount)
                    {
                        return f.Id;
                    }
                }
            }
            return null;
        }

        public bool ArticleExists(Article article)
        {
            int? result = ContentExists(article.ParentId, article.NodeName, article.BodyContent.Length);
            if (result != null)
            {
                article.ID = (int)result;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AudioExists(Audio audio)
        {
            int? result = ContentExists(audio.ParentId, audio.NodeName);
            if (result != null)
            {
                audio.ID = (int)result;
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public bool FolderExists(Folder folder)
        {
            int? result = ContentExists(folder.ParentId, folder.NodeName);
            if (result != null)
            {
                folder.ID = (int)result;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool PDFExists(Pdf file)
        {
            int? result = ContentExists(file.ParentId, file.NodeName);
            if (result != null)
            {
                file.ID = (int)result;
                return true;
            }
            else
            {
                return false;
            }
        }

        public int SaveArticle(Article article, int userId)
        {
            var newContent = _contentService.CreateContent(article.NodeName, article.ParentId, "Article", userId);
            newContent.SetValue("title", article.Title);
            newContent.SetValue("publicationDate", article.PublicationDate.ToString("s"));
            //newContent.SetValue("pinToTheTop", article.PinToTheTop);
            newContent.SetValue("summaryContent", article.SummaryContent);
            newContent.SetValue("bodyContent", article.BodyContent);
            //newContent.SetValue("geographicTags", article.GeographicTags);
            newContent.SetValue("topicTags", article.TopicTags);
            newContent.SetValue("categories", article.Categories);
            newContent.SetValue("author", article.Author);
            newContent.SetValue("image", article.Image);
            newContent.SetValue("byLine", article.ByLine);
            newContent.SetValue("pdf", article.Pdf);
            newContent.SetValue("video", article.Video);
            newContent.SetValue("videoFlv", article.VideoFlv);
            newContent.SetValue("videoMp4", article.VideoMp4);
            newContent.SetValue("umbracoUrlAlias", article.UmbracoUrlAlias);

            //newContent.SetValue("contentBuckets", article.ContentBuckets);
            //newContent.SetValue("urlList", article.UrlList);
            //newContent.SetValue("umbracoNaviHide", article.UmbracoNaviHide);
            //newContent.SetValue("metaDescription", article.MetaDescription);
            //newContent.SetValue("metaKeywords", article.MetaKeywords);
            //newContent.SetValue("ogImage", article.OgImage);
            //newContent.SetValue("ogDescription", article.OgDescription);
            //newContent.SetValue("twitterMessage", article.TwitterMessage);
            //newContent.SetValue("footNotes", article.Footnotes);
            _contentService.SaveAndPublishWithStatus(newContent);
            return newContent.Id;
        }

        public int SaveAudio(Audio audio, int userId)
        {
            var newContent = _contentService.CreateContent(audio.NodeName, audio.ParentId, "Audio", userId);
            newContent.SetValue("title", audio.Title);
            newContent.SetValue("publicationDate", audio.PublicationDate.ToString("s"));
            newContent.SetValue("summaryContent", audio.SummaryContent);
            newContent.SetValue("bodyContent", audio.BodyContent);
            newContent.SetValue("audioFile", audio.AudioFile);
            newContent.SetValue("thumbnailImage", audio.ThumbnailImage);
            newContent.SetValue("contentBuckets", audio.ContentBuckets);
            newContent.SetValue("geographicTags", audio.GeographicTags);
            newContent.SetValue("topicTags", audio.TopicTags);
            newContent.SetValue("urlList", audio.UrlList);
            newContent.SetValue("umbracoNaviHide", audio.UmbracoNaviHide);
            newContent.SetValue("metaDescription", audio.MetaDescription);
            newContent.SetValue("metaKeywords", audio.MetaKeywords);
            _contentService.SaveAndPublishWithStatus(newContent);
            return newContent.Id;
        }

        public int SaveEVC(EVC evc, int userId)
        {
            var newContent = _contentService.CreateContent(evc.NodeName, evc.ParentId, "ElectionVolunteerCoordinator", userId);
            newContent.SetValue("federalDistrictCode", evc.FederalDistrictCode);
            newContent.SetValue("name", evc.Name);
            newContent.SetValue("streetAddress", evc.StreetAddress);
            newContent.SetValue("city", evc.City);
            newContent.SetValue("state", evc.State);
            newContent.SetValue("zipCode", evc.ZipCode);
            newContent.SetValue("phoneNumber", evc.PhoneNumber);
            newContent.SetValue("emailAddress", evc.EmailAddress);
            newContent.SetValue("categoryTags", evc.CategoryTags);
            newContent.SetValue("stateTags", evc.StateTags);
            _contentService.SaveAndPublishWithStatus(newContent);
            return newContent.Id;
        }

        public int SaveFolder(Folder folder, int userId)
        {


            System.Diagnostics.Debug.WriteLine("*** SaveFolder ***");
            System.Diagnostics.Debug.WriteLine("*** folder.NodeName: " + folder.NodeName);
            System.Diagnostics.Debug.WriteLine("*** folder.ParentId: " + folder.ParentId);
            System.Diagnostics.Debug.WriteLine("*** folder.DocumentType: " + folder.DocumentType);
            System.Diagnostics.Debug.WriteLine("*** userId: " + userId);
            System.Diagnostics.Debug.WriteLine("**************************");


            var newContent = _contentService.CreateContent(folder.NodeName, folder.ParentId, folder.DocumentType, userId);
            _contentService.SaveAndPublishWithStatus(newContent);
            return newContent.Id;

        }

        public int SavePDF(Pdf file, int userId)
        {
            var newContent = _contentService.CreateContent(file.NodeName, file.ParentId, "PDF", userId);
            newContent.SetValue("title", file.Title);
            newContent.SetValue("publicationDate", file.PublicationDate.ToString("s")); //sortable datetime format ("yyyy'-'MM'-'dd'T'HH':'mm':'ss"): 2008-04-10T06:30:00
            newContent.SetValue("summaryContent", file.SummaryContent);
            newContent.SetValue("bodyContent", file.BodyContent);
            newContent.SetValue("pdfFile", file.PdfFile);
            newContent.SetValue("thumbnailImage", file.ThumbnailImage);
            newContent.SetValue("contentBuckets", file.ContentBuckets);
            newContent.SetValue("geographicTags", file.GeographicTags);
            newContent.SetValue("topicTags", file.TopicTags);
            newContent.SetValue("urlList", file.UrlList);
            newContent.SetValue("umbracoNaviHide", file.UmbracoNaviHide);
            newContent.SetValue("metaDescription", file.MetaDescription);
            newContent.SetValue("metaKeywords", file.MetaKeywords);
            _contentService.SaveAndPublishWithStatus(newContent);
            return newContent.Id;
        }

        public int SaveTag(Tag tag, int userId)
        {
            var newContent = _contentService.CreateContent(tag.NodeName, tag.ParentId, tag.DocumentType, userId);
            newContent.SetValue("title", tag.Title);
            _contentService.SaveAndPublishWithStatus(newContent);
            return newContent.Id;
        }

        public int SaveVideo(Video video, int userId)
        {
            var newContent = _contentService.CreateContent(video.NodeName, video.ParentId, "Video", userId);
            newContent.SetValue("title", video.Title);
            newContent.SetValue("publicationDate", video.PublicationDate.ToString("s"));
            newContent.SetValue("thumbnailImage", video.Thumbnail);
            newContent.SetValue("posterImage", video.Poster);
            newContent.SetValue("legacyVideoStream", video.LegacyVideoStream);
            newContent.SetValue("legacyVideoFlashStream", video.LegacyVideoFlashStream);
            newContent.SetValue("contentBuckets", video.ContentBuckets);
            newContent.SetValue("geographicTags", video.GeographicTags);
            newContent.SetValue("topicTags", video.TopicTags);
            newContent.SetValue("urlList", video.UrlList);
            newContent.SetValue("umbracoNaviHide", video.UmbracoNaviHide);
            newContent.SetValue("metaDescription", video.MetaDescription);
            newContent.SetValue("metaKeywords", video.MetaKeywords);
            _contentService.SaveAndPublishWithStatus(newContent);
            return newContent.Id;
        }

        public bool TagExists(Tag tag)
        {
            var items = _contentService.GetChildren(tag.ParentId);
            foreach (var item in items)
            {
                if (item.Name.ToLower() == tag.NodeName.ToLower())
                {
                    tag.ID = item.Id;
                    return true;
                }
            }
            return false;
        }

        public bool VideoExists(Video video)
        {
            int? result = ContentExists(video.ParentId, video.NodeName);
            if (result != null)
            {
                video.ID = (int)result;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}