using AM.NRA.ILA.Importer.Umb.Models;
using AM.NRA.ILA.Importer.Umb.Models.Repository;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace AM.NRA.ILA.Importer.Umb.Import
{
    public sealed class Importer
    {








        private IContentService _contentService = ApplicationContext.Current.Services.ContentService;
        private static volatile Importer _instance;
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IMediaService _mediaService = ApplicationContext.Current.Services.MediaService;
        private Repository _repo = new Repository();
        private static object _syncRoot = new object();
        private IUserService _userService = ApplicationContext.Current.Services.UserService;

        public HttpContext CurrentContext { get; set; }
        public Job CurrentJob { get; set; }
        public int CurrentUserId { get; set; }
        public string FilePath { get; set; }
        public Job.ContentDocumentType ImportType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Importer()
        {
        }

        public static Importer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new Importer();
                        }
                    }
                }

                return _instance;
            }
        }












        private string CreateParagraphTags(string postbody)
        {
            /*
            string replaceWith = "<br />";
            StringBuilder sb = new StringBuilder();
            sb.Append("<p>");
            //sb.Append(postbody.Replace("\n\n", "</p><p>"));
            //sb.Append(postbody.Replace(/(?:\r\n|\r|\n)/g, '<br />'));
            //sb.Append(postbody.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith));
            //sb.Append(postbody.Replace("\r\n", replaceWith));
            sb.Append("</p>");
            return sb.ToString();
            */

            /*
            string result = "<p>" + postbody
                .Replace(Environment.NewLine + Environment.NewLine, "</p><p>")
                .Replace(Environment.NewLine, "<br />")
                .Replace("</p><p>", "</p>" + Environment.NewLine + "<p>") + "</p>";
            return result;
            */

            /*
            string result = "<p>" + postbody
                .Replace(Environment.NewLine + Environment.NewLine, "</p><p>")
                .Replace(Environment.NewLine, "<br />")
                .Replace("</p><p>", "</p>" + Environment.NewLine + "<p>")
                .Replace("<p></p>", "") + "</p>";
            return result;
            */

            /*
            string result = "<p>" + postbody
                .Replace(Environment.NewLine + Environment.NewLine, "</p><p>")
                .Replace("</p><p>", "</p>" + Environment.NewLine + "<p>")
                .Replace("<p></p>", "")
                .Replace("<p>&nbsp;</p>", "") 
                + "</p>";
            return result;  
            */

            string result = "<p>" + postbody
                .Replace("&nbsp;", "")
                .Replace(Environment.NewLine + Environment.NewLine, "</p><p>")
                .Replace("</p><p>", "</p>" + Environment.NewLine + "<p>")
                + "</p>";
            result = result.Replace("<p></p>", "");
            result = Regex.Replace(result, @"\t|\n|\r", "");
            return result;

        }


        











        private string ConstructSummary(string sourceText)
        {
            string tempBody = StripHtml(sourceText);
            int bodyWordCount = tempBody.Split(' ').Length;

            if (bodyWordCount > 100)
            {
                int tempBodyLength = tempBody.Length > 300 ? 300 : tempBody.Length;
                int stringEnd = tempBody.LastIndexOfAny(new char[] { '.', '!', '?' }, tempBodyLength);
                return tempBody.Remove(stringEnd + 1);
            }
            else
            {
                return tempBody;
            }
        }
        
        private string CreateUrl(XmlNode rootNode, string path, bool includeLastItem)
        {
            StringBuilder result = new StringBuilder();
            result.Append(GetHostName(path));

            string[] pathArray = path.Split(',');
            int itemCount = includeLastItem ? pathArray.Count() : pathArray.Count() - 1;
            for (int i = 2; i < itemCount; i++)
            {
                if (pathArray[i] != "-1")
                {
                    result.Append("/" + GetUrlName(rootNode, pathArray[i]));
                }
            }

            return result.ToString();
        }

        private string FixJsonQuotes(string sourceString)
        {
            string result = sourceString.Replace("\"", "\\\"");
            return result;
        }
        
        private string GetAudioId(string fileName, int mediaParentId)
        {
            string result = string.Empty;
            bool exists = false;

            if (_mediaService.HasChildren(mediaParentId))
            {
                foreach (IMedia item in _mediaService.GetChildren(mediaParentId))
                {
                    if (item.Name.ToLower() == fileName.ToLower())
                    {
                        result = item.Id.ToString();
                        exists = true;
                        break;
                    }
                }
            }

            if (!exists)
            {
                var newMedia = _mediaService.CreateMedia(fileName, mediaParentId, "File", CurrentUserId);
                FileStream fs = System.IO.File.OpenRead(CurrentContext.Server.MapPath("/legacy/audio/" + fileName));
                newMedia.SetValue("umbracoFile", fileName, fs);
                _mediaService.Save(newMedia);
                result = newMedia.Id.ToString(); 
            }

            return result;
        }

        private int GetFolderId(int parentId, string name, string contentType)
        {
            Folder f = new Folder(parentId, name, contentType);
            if (_repo.FolderExists(f))
            {
                return f.ID;
            }
            else
            {
                return _repo.SaveFolder(f, CurrentUserId);
            }
        }

        private string GetHostName(string path)
        {
            /*
            string result = string.Empty;
            if (path.StartsWith("-1,3812"))
            {
                result = "nrapvf.org";
            }
            else if (path.StartsWith("-1,42579"))
            {
                result = "nraila.org";
            }
            else if (path.StartsWith("-1,227931"))
            {
                result = "gunbanobama.com";
            }
            else if (path.StartsWith("-1,251424"))
            {
                result = "gunbanfacts.com";
            }
            */

            string result = "www.americanrifleman.org";

            return result;
        }

        private string GetMediaId(string xmlFilePath, string xpathQuery, string originalId, int mediaParentId, string attrName)
        {
            string result = string.Empty;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(CurrentContext.Server.MapPath(xmlFilePath));
                XmlNode root = doc.DocumentElement;
                XmlNodeList nodes = root.SelectNodes(xpathQuery);
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes.GetNamedItem("id").Value == originalId)
                    {
                        int start = node.Attributes.GetNamedItem(attrName).Value.ToString().LastIndexOf("/") + 1;
                        string fileName = node.Attributes.GetNamedItem(attrName).Value.ToString().Substring(start);

                        IMedia m = _mediaService.GetById(int.Parse(originalId));
                        if (m != null && m.Name.ToLower() == fileName.ToLower())
                        {
                            result = originalId;
                            break;
                        }

                        if (_mediaService.HasChildren(mediaParentId))
                        {
                            bool breakOut = false;
                            foreach (IMedia item in _mediaService.GetChildren(mediaParentId))
                            {
                                if (item.Name.ToLower() == fileName.ToLower())
                                {
                                    result = item.Id.ToString();
                                    breakOut = true;
                                    break;
                                }
                            }

                            if (breakOut)
                            {
                                break;
                            }
                        }

                        var newMedia = _mediaService.CreateMedia(fileName, mediaParentId, "Image", CurrentUserId);
                        FileStream fs = System.IO.File.OpenRead(CurrentContext.Server.MapPath("/legacy" + node.Attributes.GetNamedItem("thumbnail").Value.ToString()));
                        newMedia.SetValue("umbracoFile", fileName, fs);
                        _mediaService.Save(newMedia);
                        result = newMedia.Id.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Content Importer Oops!: ", ex);
            }

            return result;
        }

        private string GetMediaPath(string xmlFilePath, string xpathQuery, string originalId, string attrName)
        {
            string result = string.Empty;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(CurrentContext.Server.MapPath(xmlFilePath));
                XmlNode root = doc.DocumentElement;
                XmlNodeList nodes = root.SelectNodes(xpathQuery);
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes.GetNamedItem("id").Value == originalId)
                    {
                        result = node.Attributes.GetNamedItem(attrName).Value;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Content Importer Oops!: " + ex);
            }

            return result;
        }

        private int GetNodeIdByNodeName(int parentNodeId, string nodeName, string nodeTypeAlias)
        {
            int result = 0;

            foreach (var item in _contentService.GetDescendants(parentNodeId))
            {
                if (item.ContentType.Alias.ToLower() == nodeTypeAlias.ToLower() && item.Name.ToLower() == nodeName.ToLower())
                {
                    result = item.Id;
                }
            }

            return result;
        }

        private int GetNodeIdByPropertyValue(int parentNodeId, string propName, string propValue, string nodeTypeAlias)
        {
            int result = 0;

            foreach (var item in _contentService.GetDescendants(parentNodeId))
            {
                if (item.ContentType.Alias.ToLower() == nodeTypeAlias.ToLower())
                {
                    if (item.GetValue(propName).ToString().ToLower() == propValue.ToLower())
                    {
                        result = item.Id;
                    }
                }
            }

            return result;
        }
        
        private string GetUrlName(XmlNode rootNode, string nodeId)
        {
            string result = string.Empty;

            XmlNode node = rootNode.SelectSingleNode("//*[@id=" + nodeId + "]");
            if (node != null)
            {
                result = node.Attributes.GetNamedItem("urlName").Value.ToString();
            }

            return result;
        }















        private List<Article> ImportArticleContent(XmlNode rootNode, List<Article> articles, int parentNodeId)
        {

            System.Diagnostics.Debug.WriteLine("*** ImportArticleContent " + rootNode);
            System.Diagnostics.Debug.WriteLine("*** LOADED XML FILE: " + rootNode);

            //XmlNodeList nodes = rootNode.SelectNodes("//ilaArticle[@isDoc]");
            //XmlNodeList nodes = rootNode.SelectNodes("//rss");
            //XmlNodeList nodes = rootNode.SelectNodes("//item");
            XmlNodeList nodes = rootNode.SelectNodes("//channel/item");
            
            int numberNodes = nodes.Count;
            string status = "";
            System.Diagnostics.Debug.WriteLine("*** Number Nodes: " + numberNodes.ToString());

            System.Diagnostics.Debug.WriteLine("*** TRY TO LOOP OVER NODES ");


            foreach (XmlNode node in nodes)
            {
                //if (node.SelectSingleNode("wp:post_date") != null && node.SelectSingleNode("wp:post_date").FirstChild != null)


                System.Diagnostics.Debug.WriteLine("************************************");
                System.Diagnostics.Debug.WriteLine("*** FOUND A NODE ");
                if (node.SelectSingleNode("post_id") != null && node.SelectSingleNode("post_id").FirstChild != null)
                {
                    System.Diagnostics.Debug.WriteLine("*** post_id:  " + node.SelectSingleNode("post_id").FirstChild.Value);
                }
                if (node.SelectSingleNode("title") != null && node.SelectSingleNode("title").FirstChild != null)
                {
                    System.Diagnostics.Debug.WriteLine("*** title:  " + node.SelectSingleNode("title").FirstChild.Value);
                }
                if (node.SelectSingleNode("post_date") != null && node.SelectSingleNode("post_date").FirstChild != null)
                {
                    System.Diagnostics.Debug.WriteLine("*** post_date:  " + node.SelectSingleNode("post_date").FirstChild.Value);
                }
                if (node.SelectSingleNode("content_encoded") != null && node.SelectSingleNode("content_encoded").FirstChild != null)
                {
                    System.Diagnostics.Debug.WriteLine("*** content_encoded:  " + node.SelectSingleNode("content_encoded").FirstChild.Value);
                }
                if (node.SelectSingleNode("excerpt_encoded") != null && node.SelectSingleNode("excerpt_encoded").FirstChild != null)
                {
                    System.Diagnostics.Debug.WriteLine("*** excerpt_encoded:  " + node.SelectSingleNode("excerpt_encoded").FirstChild.Value);
                }
                if (node.SelectSingleNode("status") != null && node.SelectSingleNode("status").FirstChild != null)
                {
                    System.Diagnostics.Debug.WriteLine("*** status:  " + node.SelectSingleNode("status").FirstChild.Value);
                    status = node.SelectSingleNode("status").FirstChild.Value;
                }


















                //System.Diagnostics.Debug.WriteLine("*** node: " + node.ToString());
                //System.Diagnostics.Debug.WriteLine("*** node: " + node.SelectSingleNode("pubDate").FirstChild.Value);



                if (node.SelectSingleNode("post_date") != null && node.SelectSingleNode("post_date").FirstChild != null && status == "publish")
                {


                    System.Diagnostics.Debug.WriteLine("*** post_date is NOT NULL *** ");
                    System.Diagnostics.Debug.WriteLine("*** post_date: " + node.SelectSingleNode("post_date").FirstChild.Value);
                    System.Diagnostics.Debug.WriteLine("*** StartDate: " + this.StartDate);
                    System.Diagnostics.Debug.WriteLine("*** EndDate: " + this.EndDate);
                    System.Diagnostics.Debug.WriteLine("*** TRY TO PARSE post_date ");





                    DateTime pubDate;
                    if (DateTime.TryParse(node.SelectSingleNode("post_date").FirstChild.Value, out pubDate))
                    {
                        if (pubDate >= this.StartDate && pubDate <= this.EndDate)
                        {

                            System.Diagnostics.Debug.WriteLine("*** PUBLICATION DATE IS WITHIN THE RANGE SELECTED ");
                            System.Diagnostics.Debug.WriteLine("*** CREATE NEW ARTICLE ");





                            //Article a = new Article(parentNodeId, node.Attributes.GetNamedItem("post_id").Value);
                            Article a = new Article(parentNodeId, node.SelectSingleNode("title").FirstChild.Value);




                            System.Diagnostics.Debug.WriteLine("*** SET TITLE: " + node.SelectSingleNode("title").FirstChild.Value);
                            // title
                            if (node.SelectSingleNode("title") != null && node.SelectSingleNode("title").FirstChild != null)
                            {
                                if (!string.IsNullOrEmpty(node.SelectSingleNode("title").FirstChild.Value))
                                {
                                    a.Title = node.SelectSingleNode("title").FirstChild.Value;
                                }
                                else
                                {
                                    a.Title = "*** NO TITLE FOUND ***";
                                }
                            }


                            System.Diagnostics.Debug.WriteLine("*** SET PUBDATE: " + node.SelectSingleNode("pubDate").FirstChild.Value);
                            // publicationDate
                            a.PublicationDate = pubDate;

                            /* pinToTheTop
                            if (node.SelectSingleNode("elevatedStatus") != null && node.SelectSingleNode("elevatedStatus").FirstChild != null)
                            {
                                a.PinToTheTop = node.SelectSingleNode("elevatedStatus").FirstChild.Value == "1" ? true : false;
                            }
                            */






                            // summaryContent
                            System.Diagnostics.Debug.WriteLine("*** SET summaryContent: " + node.SelectSingleNode("excerpt_encoded").FirstChild.Value);
                            if (node.SelectSingleNode("excerpt_encoded") != null && node.SelectSingleNode("excerpt_encoded").FirstChild != null)
                            {
                                a.SummaryContent = node.SelectSingleNode("excerpt_encoded").FirstChild.Value;
                            }
                            


                            
                            // bodyContent
                            System.Diagnostics.Debug.WriteLine("*** SET BODYCONTENT: " + node.SelectSingleNode("content_encoded").FirstChild.Value);
                            if (node.SelectSingleNode("content_encoded") != null && node.SelectSingleNode("content_encoded").FirstChild != null)
                            {
                                //a.BodyContent = node.SelectSingleNode("bodyText").FirstChild.Value.Replace("src=\"/media/", "src=\"/legacy/media/");
                                //a.BodyContent = node.SelectSingleNode("content_encoded").FirstChild.Value;
                                
                                
                                a.BodyContent = CreateParagraphTags(node.SelectSingleNode("content_encoded").FirstChild.Value);


                                /* if content does NOT have any html code, add paragraph tags.
                                if (a.BodyContent != HttpUtility.HtmlEncode(a.BodyContent))
                                {
                                    a.BodyContent = CreateParagraphTags(node.SelectSingleNode("content_encoded").FirstChild.Value);
                                }
                                else
                                {
                                    a.BodyContent = node.SelectSingleNode("content_encoded").FirstChild.Value;
                                }
                                */



                                /*
                                if (string.IsNullOrEmpty(a.SummaryContent) && !string.IsNullOrEmpty(a.BodyContent))
                                {
                                    a.SummaryContent = ConstructSummary(a.BodyContent);
                                }
                                */

                            }


                            // author
                            System.Diagnostics.Debug.WriteLine("*** SET AUTHOR: " + node.SelectSingleNode("creator").FirstChild.Value);
                            if (node.SelectSingleNode("creator") != null && node.SelectSingleNode("creator").FirstChild != null)
                            {
                                a.Author = node.SelectSingleNode("creator").FirstChild.Value;
                            }














                            // UmbracoUrlAlias
                            System.Diagnostics.Debug.WriteLine("*** SET UmbracoUrlAlias: " + node.SelectSingleNode("post_id").FirstChild.Value);
                            string strUrls = "";
                            if (node.SelectSingleNode("post_id") != null && node.SelectSingleNode("post_id").FirstChild != null)
                            {
                                string contentEncoded = node.SelectSingleNode("content_encoded").FirstChild.Value;

                                System.Diagnostics.Debug.WriteLine("*** contentEncoded: " + contentEncoded);

                                if (contentEncoded  == "[gallery]"){
                                    System.Diagnostics.Debug.WriteLine("*** THIS IS A GALLERY RECORD! ***");
                                    if (node.SelectSingleNode("post_name") != null && node.SelectSingleNode("post_name").FirstChild != null)
                                    {
                                        strUrls = "/" + node.SelectSingleNode("post_name").FirstChild.Value;
                                        strUrls += ",/lid" + node.SelectSingleNode("post_id").FirstChild.Value;
                                        strUrls += ",/galleries/" + node.SelectSingleNode("post_name").FirstChild.Value;
                                    }
                                }
                                else {
                                    System.Diagnostics.Debug.WriteLine("*** THIS IS AN ARTICLE, BLOG, OR VIDEO RECORD! ***");
                                    if (node.SelectSingleNode("post_name") != null && node.SelectSingleNode("post_name").FirstChild != null)
                                    {
                                        strUrls = "/" + node.SelectSingleNode("post_name").FirstChild.Value;
                                        strUrls += ",/lid" + node.SelectSingleNode("post_id").FirstChild.Value;
                                        strUrls += ",/articles/" + node.SelectSingleNode("post_name").FirstChild.Value;
                                        strUrls += ",/blogs/" + node.SelectSingleNode("post_name").FirstChild.Value;
                                        strUrls += ",/videos/" + node.SelectSingleNode("post_name").FirstChild.Value;
                                    }
                                }


                                a.UmbracoUrlAlias = strUrls;


                            }












                            
                            // get meta data (image/pdf/video/friendly)
                            System.Diagnostics.Debug.WriteLine("*** GET IMAGE: ");
                            if (node.SelectSingleNode("postmeta") != null && node.SelectSingleNode("postmeta").FirstChild != null)
                            {

                                System.Diagnostics.Debug.WriteLine("*** postmeta count:  " + node.SelectNodes("postmeta").Count);

                                string meta_key = "";
                                string meta_value = "";
                                string AR_by_line = "";
                                string AR_image_front = "";
                                string AR_pdf = "";
                                string Video = "";
                                string AR_video_flv = "";
                                string AR_video_mp4 = "";

                                
                                foreach (XmlNode postmeta in node.SelectNodes("postmeta"))
                                {

                                    if (postmeta.SelectSingleNode("meta_key").FirstChild.Value != null){meta_key = postmeta.SelectSingleNode("meta_key").FirstChild.Value;}
                                    if (postmeta.SelectSingleNode("meta_value").FirstChild.Value != null){meta_value = postmeta.SelectSingleNode("meta_value").FirstChild.Value;}
                                    
                                    System.Diagnostics.Debug.WriteLine("*** postmeta meta_key:  " + meta_key);
                                    System.Diagnostics.Debug.WriteLine("*** postmeta meta_value:  " + meta_value);

                                    if ((meta_key == "AR_by_line") || (meta_key == "AH_by_line")) { 
                                        AR_by_line = meta_value;
                                        a.ByLine = AR_by_line;
                                    }
                                    if ((meta_key == "AR_image_front") || (meta_key == "AH_image_front")) { 
                                        AR_image_front = meta_value;
                                        a.Image = AR_image_front;
                                    }
                                    if (((meta_key == "AR_pdf") || (meta_key == "AH_pdf")) && meta_value != "none") { 
                                        AR_pdf = meta_value;
                                        a.Pdf = AR_pdf;
                                    }
                                    if (meta_key == "video") { 
                                        Video = meta_value;
                                        a.Video = Video;
                                    }
                                    if ((meta_key == "AR_video_flv") || (meta_key == "AH_video_flv")) { 
                                        AR_video_flv = meta_value;
                                        a.VideoFlv = AR_video_flv;
                                    }
                                    if ((meta_key == "AR_video_mp4") || (meta_key == "AH_video_mp4")) { 
                                        AR_video_mp4 = meta_value;
                                        a.VideoMp4 = AR_video_mp4;
                                    }

                                    // if friendly url is entered, add it to the legacy url list (strUrls)
                                    if (meta_key == "friendly")
                                    {
                                        strUrls += ",/" + meta_value;
                                        strUrls += ",/articles/" + meta_value;
                                        strUrls += ",/blogs/" + meta_value;
                                        strUrls += ",/videos/" + meta_value;
                                        a.UmbracoUrlAlias = strUrls;
                                    }
                                    
                                }
                                
                                System.Diagnostics.Debug.WriteLine("*** AR_by_line:  " + AR_by_line);
                                System.Diagnostics.Debug.WriteLine("*** AR_image_front:  " + AR_image_front);
                                System.Diagnostics.Debug.WriteLine("*** AR_pdf:  " + AR_pdf);
                                System.Diagnostics.Debug.WriteLine("*** Video:  " + Video);
                                System.Diagnostics.Debug.WriteLine("*** AR_video_flv:  " + AR_video_flv);
                                System.Diagnostics.Debug.WriteLine("*** AR_video_mp4:  " + AR_video_mp4);

                            }
                           















                            // topicTags
                            if (node.SelectSingleNode("category") != null && node.SelectSingleNode("category").FirstChild != null)
                            {
                                System.Diagnostics.Debug.WriteLine("*** category:  " + node.SelectSingleNode("category").FirstChild.Value);
                                System.Diagnostics.Debug.WriteLine("*** category count:  " + node.SelectNodes("category").Count);

                                string categoryList = "";
                                string tagList = "";
                                foreach (XmlNode category in node.SelectNodes("category"))
                                {

                                    System.Diagnostics.Debug.WriteLine("*** category domain:  " + category.Attributes.GetNamedItem("domain").Value);
                                    System.Diagnostics.Debug.WriteLine("*** category nicename:  " + category.Attributes.GetNamedItem("nicename").Value);
                                    System.Diagnostics.Debug.WriteLine("*** category value:  " + category.FirstChild.Value);

                                    if (category.Attributes.GetNamedItem("domain").Value != null)
                                    {
                                        string categoryDomain = category.Attributes.GetNamedItem("domain").Value;
                                        string categoryValue = category.FirstChild.Value;

                                        if (categoryDomain.Equals("category"))
                                        {
                                            categoryList += categoryValue + ",";
                                        }
                                        else if (categoryDomain.Equals("post_tag"))
                                        {
                                            tagList += categoryValue + ",";
                                        }
                                    }
                                }


                                System.Diagnostics.Debug.WriteLine("*** categoryList:  " + categoryList);
                                System.Diagnostics.Debug.WriteLine("*** tagList:  " + tagList);

                                // add topicTags
                                System.Diagnostics.Debug.WriteLine("*** Add topicTags");
                                if (!string.IsNullOrEmpty(tagList))
                                {
                                    a.TopicTags = tagList;
                                }

                                // add categories
                                System.Diagnostics.Debug.WriteLine("*** Add categores");
                                if (!string.IsNullOrEmpty(tagList))
                                {
                                    a.Categories = categoryList;
                                }


                            }





                            /* footNotes
                            if (node.SelectSingleNode("Footnotes") != null)
                            {
                                XmlNode f = node.SelectSingleNode("Footnotes/footNotes");
                                if (f.FirstChild != null)
                                {
                                    a.Footnotes = f.FirstChild.Value;
                                }
                            }
                            */

                            /* contentBuckets
                            foreach (contentBucket cb in ContentBuckets.buckets)
                            {
                                if (node.Attributes.GetNamedItem("path").Value.StartsWith(cb.path))
                                {
                                    if (!string.IsNullOrEmpty(a.ContentBuckets))
                                    {
                                        a.ContentBuckets += ",";
                                    }

                                    a.ContentBuckets += cb.id.ToString();
                                }
                            }
                            */

                            /*
                            if (string.IsNullOrEmpty(a.ContentBuckets))
                            {
                                if (node.Attributes.GetNamedItem("path").Value.StartsWith("-1,3812")) //pvf
                                {
                                    a.ContentBuckets = "8167";
                                }
                                else if (node.Attributes.GetNamedItem("path").Value.StartsWith("-1,42579")) //ila
                                {
                                    a.ContentBuckets = "8166";
                                }
                            }
                            */

                            /* geographicTags
                            if (node.SelectSingleNode("articleState") != null && node.SelectSingleNode("articleState").FirstChild != null)
                            {
                                if (!string.IsNullOrEmpty(node.SelectSingleNode("articleState").FirstChild.Value))
                                {
                                    int id;
                                    if (int.TryParse(node.SelectSingleNode("articleState").FirstChild.Value, out id))
                                    {
                                        string statename = USStates.states.Where(s => s.id == id).First().name;
                                        int stateNodeId = GetNodeIdByNodeName(1524, statename, "GeographicTag");
                                        a.GeographicTags = "[{\"tag\": \"" + statename + "\", \"id\":\"" + stateNodeId.ToString() + "\"}]";
                                    }
                                    else
                                    {
                                        _log.Error("Assigning geo tag for node id " + node.Attributes.GetNamedItem("id").Value + " failed.");
                                    }
                                    
                                }
                            }
                            */


                            /* topicTags
                            if (node.SelectSingleNode("articleTags") != null && node.SelectSingleNode("articleTags").FirstChild != null)
                            {
                                if (!string.IsNullOrEmpty(node.SelectSingleNode("articleTags").FirstChild.Value))
                                {
                                    StringBuilder tTags = new StringBuilder();
                                    tTags.Append("[");

                                    string[] oldTags = node.SelectSingleNode("articleTags").FirstChild.Value.Split(',');
                                    for (int i = 0; i < oldTags.Length; i++)
                                    {
                                        if (i > 0)
                                        {
                                            tTags.Append(",");
                                        }
                                        int nodeId = GetNodeIdByPropertyValue(1113, "title", oldTags[i], "TopicTag");
                                        tTags.Append("{\"tag\": \"" + FixJsonQuotes(oldTags[i]) + "\", \"id\":\"" + nodeId.ToString() + "\"}");
                                    }

                                    tTags.Append("]");
                                    a.TopicTags = tTags.ToString();
                                }
                            }
                            */ 

                            /* metaDescription
                            if (node.SelectSingleNode("metaDescription") != null && node.SelectSingleNode("metaDescription").FirstChild != null)
                            {
                                a.MetaDescription = string.IsNullOrEmpty(node.SelectSingleNode("metaDescription").FirstChild.Value) ? string.Empty : node.SelectSingleNode("metaDescription").FirstChild.Value;
                            }
                            */

                            /* metaKeywords
                            if (node.SelectSingleNode("metaKeywords") != null && node.SelectSingleNode("metaKeywords").FirstChild != null)
                            {
                                a.MetaKeywords = string.IsNullOrEmpty(node.SelectSingleNode("metaKeywords").FirstChild.Value) ? string.Empty : node.SelectSingleNode("metaKeywords").FirstChild.Value;
                            }
                            */

                            // ogImage ...none exist

                            /* ogDescription
                            if (node.SelectSingleNode("ogDescription") != null && node.SelectSingleNode("ogDescription").FirstChild != null)
                            {
                                a.OgDescription = node.SelectSingleNode("ogDescription").FirstChild.Value;
                            }
                            */

                            /* twitterMessage
                            if (node.SelectSingleNode("twitterMessage") != null && node.SelectSingleNode("twitterMessage").FirstChild != null)
                            {
                                a.TwitterMessage = node.SelectSingleNode("twitterMessage").FirstChild.Value;
                            }
                            */

                            /* umbracoNaviHide
                            if (node.SelectSingleNode("umbracoNaviHide") != null && node.SelectSingleNode("umbracoNaviHide").FirstChild != null)
                            {
                                a.UmbracoNaviHide = node.SelectSingleNode("umbracoNaviHide").FirstChild.Value == "1" ? true : false;
                            }
                            */

                            /* urlList ... original urls
                            List<legacyUrl> uList = new List<legacyUrl>(); //object to be serialized to json
                            string originalUrl = CreateUrl(rootNode, node.Attributes.GetNamedItem("path").Value, true);
                            uList.Add(new legacyUrl(originalUrl + "/"));
                            uList.Add(new legacyUrl(originalUrl + ".aspx"));
                            */

                            /* ... umbracoUrlAlias
                            if (node.SelectSingleNode("umbracoUrlAlias") != null && node.SelectSingleNode("umbracoUrlAlias").FirstChild != null && !string.IsNullOrEmpty(node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value))
                            {
                                string host = GetHostName(node.Attributes.GetNamedItem("path").Value);
                                if (node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value.Contains(','))
                                {
                                    foreach (string item in node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value.Split(','))
                                    {
                                        uList.Add(new legacyUrl(host + "/" + item + "/"));
                                        uList.Add(new legacyUrl(host + "/" + item + ".aspx"));
                                    }
                                }
                                else
                                {
                                    uList.Add(new legacyUrl(host + "/" + node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value + "/"));
                                    uList.Add(new legacyUrl(host + "/" + node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value + ".aspx"));
                                }
                            }
                            */

                            /* ... umbracoUrlName
                            if (node.SelectSingleNode("umbracoUrlName") != null && node.SelectSingleNode("umbracoUrlName").FirstChild != null && !string.IsNullOrEmpty(node.SelectSingleNode("umbracoUrlName").FirstChild.Value))
                            {
                                uList.Add(new legacyUrl(CreateUrl(rootNode, node.Attributes.GetNamedItem("path").Value, false) + node.SelectSingleNode("umbracoUrlName").FirstChild.Value + "/"));
                                uList.Add(new legacyUrl(CreateUrl(rootNode, node.Attributes.GetNamedItem("path").Value, false) + node.SelectSingleNode("umbracoUrlName").FirstChild.Value + ".aspx"));
                            }
                            */

                            /* ... legacyURL
                            if (node.SelectSingleNode("legacyURL") != null && node.SelectSingleNode("legacyURL").FirstChild != null && !string.IsNullOrEmpty(node.SelectSingleNode("legacyURL").FirstChild.Value))
                            {
                                uList.Add(new legacyUrl(GetHostName(node.Attributes.GetNamedItem("path").Value) + node.SelectSingleNode("legacyURL").FirstChild.Value));
                            }
                            */

                            //a.UrlList = JsonConvert.SerializeObject(uList);

                            System.Diagnostics.Debug.WriteLine("*** TRY TO ADD ARTICLE TO CMS");
                            articles.Add(a);
                        }
                    }
                }
                else
                {
                    _log.Debug("Content Importer node missing publicationDate. post_id: " + node.SelectSingleNode("post_id").FirstChild.Value);
                }
            }

            return articles;
        }

       

        private string StripHtml(string text)
        {
            string pattern = @"<(.|\n)*?>";
            return Regex.Replace(text, pattern, string.Empty);
        }
        
        public void ImportArticleContent(XmlNode rootNode)
        {
            List<Article> articlesToImport = new List<Article>();










            /* ******************* ROOT ARTICLE FOLDER ID *************  */
            //int parentNodeId = 2404;     // Staging AH
            int parentNodeId = 1090;     // local AR
            /* ******************* ROOT ARTICLE FOLDER ID *************  */









            articlesToImport = ImportArticleContent(rootNode, articlesToImport, parentNodeId);

            System.Diagnostics.Debug.WriteLine("***********************************");
            System.Diagnostics.Debug.WriteLine("*** Articles to Import:  " + articlesToImport.Count);


            if (articlesToImport.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("*** START IMPORT INTO CMS");
                this.CurrentJob.ItemCount = articlesToImport.Count;
                this.CurrentJob.ItemTotal = articlesToImport.Count;

                foreach (Article item in articlesToImport)
                {
                    string year = item.PublicationDate.Year.ToString();
                    string month = item.PublicationDate.Month.ToString();
                    string day = item.PublicationDate.Day.ToString();



                    System.Diagnostics.Debug.WriteLine("*** year: " + year);
                    System.Diagnostics.Debug.WriteLine("*** month: " + month);
                    System.Diagnostics.Debug.WriteLine("*** day: " + day);


                    item.ParentId = GetFolderId(item.ParentId, year, "ArticleFolder");
                    item.ParentId = GetFolderId(item.ParentId, month, "ArticleFolder");
                    item.ParentId = GetFolderId(item.ParentId, day, "ArticleFolder");



                    System.Diagnostics.Debug.WriteLine("*** ParentId: " + item.ParentId);



                    if (!_repo.ArticleExists(item))
                    {
                        int newArticleContentId = _repo.SaveArticle(item, CurrentUserId);
                        System.Diagnostics.Debug.WriteLine("*** Saved item to DB!: " + newArticleContentId.ToString());
                        _log.Info("Imported Article content " + newArticleContentId.ToString());
                    }
                    this.CurrentJob.ItemCount--;
                }
            }
            else
            {
                this.CurrentJob.ItemCount = 0;
                this.CurrentJob.ItemTotal = 0;
            }
        }

        public void ImportAudioContent(XmlNode rootNode)
        {
            List<Audio> audioToImport = new List<Audio>();
            XmlNodeList nodes = rootNode.SelectNodes("//mediaCenterAudio[@isDoc]");
            foreach (XmlNode node in nodes)
            {
                if (node.SelectSingleNode("publicationDate") != null && node.SelectSingleNode("publicationDate").FirstChild != null)
                {
                    DateTime pubDate;
                    if (DateTime.TryParse(node.SelectSingleNode("publicationDate").FirstChild.Value, out pubDate))
                    {
                        if (pubDate >= this.StartDate && pubDate <= this.EndDate)
                        {
                            Audio a = new Audio(3325, node.Attributes.GetNamedItem("nodeName").Value);

                            // Title
                            if (node.SelectSingleNode("mcTitle") != null && node.SelectSingleNode("mcTitle").FirstChild != null)
                            {
                                if (!string.IsNullOrEmpty(node.SelectSingleNode("mcTitle").FirstChild.Value))
                                {
                                    a.Title = node.SelectSingleNode("mcTitle").FirstChild.Value;
                                }
                                else
                                {
                                    a.Title = node.Attributes.GetNamedItem("nodeName").Value;
                                }
                            }

                            // Publication Date
                            a.PublicationDate = pubDate;

                            // Summary Content
                            if (node.SelectSingleNode("bodyText") != null && node.SelectSingleNode("bodyText").FirstChild != null)
                            {
                                a.SummaryContent = node.SelectSingleNode("bodyText").FirstChild.Value;
                            }

                            // Body Content
                            if (node.SelectSingleNode("mcDescription") != null && node.SelectSingleNode("mcDescription").FirstChild != null)
                            {
                                a.BodyContent = node.SelectSingleNode("mcDescription").FirstChild.Value;
                            }

                            // Audio File
                            if (node.SelectSingleNode("mcItemUrl") != null && node.SelectSingleNode("mcItemUrl").FirstChild != null)
                            {
                                a.AudioFile = "/legacy/audio/" + node.SelectSingleNode("mcItemUrl").FirstChild.Value;
                            }

                            // Thumbnail Image
                            if (node.SelectSingleNode("mcThumbnail") != null && node.SelectSingleNode("mcThumbnail").FirstChild != null)
                            {
                                a.ThumbnailImage = "/legacy" + GetMediaPath("/App_Data/audio_media_list.xml", "//audio", node.Attributes.GetNamedItem("id").Value.ToString(), "thumbnail");
                            }

                            // Content Buckets
                            if (node.Attributes.GetNamedItem("path").Value.StartsWith("-1,3812,"))
                            {
                                a.ContentBuckets = "8167";
                            }
                            else if (node.Attributes.GetNamedItem("path").Value.StartsWith("-1,42579,"))
                            {
                                a.ContentBuckets = "8166";
                            }

                            // Geographic Tags ... none exist

                            // Topic Tags ... none exist

                            // Alternate & Legacy URLs
                            List<legacyUrl> uList = new List<legacyUrl>(); //object to be serialized to json

                            // - original urls
                            string legacyUrl = CreateUrl(rootNode, node.Attributes.GetNamedItem("path").Value, true);
                            uList.Add(new legacyUrl(legacyUrl + "/"));
                            uList.Add(new legacyUrl(legacyUrl + ".aspx"));

                            // - umbracoUrlAlias
                            if (node.SelectSingleNode("umbracoUrlAlias") != null && node.SelectSingleNode("umbracoUrlAlias").FirstChild != null && !string.IsNullOrEmpty(node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value))
                            {
                                string host = GetHostName(node.Attributes.GetNamedItem("path").Value);
                                if (node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value.Contains(','))
                                {
                                    foreach (string item in node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value.Split(','))
                                    {
                                        uList.Add(new legacyUrl(host + "/" + item + "/"));
                                        uList.Add(new legacyUrl(host + "/" + item + ".aspx"));
                                    }
                                }
                                else
                                {
                                    uList.Add(new legacyUrl(host + "/" + node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value + "/"));
                                    uList.Add(new legacyUrl(host + "/" + node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value + ".aspx"));
                                }
                            }

                            // - umbracoUrlName
                            if (node.SelectSingleNode("umbracoUrlName") != null && node.SelectSingleNode("umbracoUrlName").FirstChild != null && !string.IsNullOrEmpty(node.SelectSingleNode("umbracoUrlName").FirstChild.Value))
                            {
                                uList.Add(new legacyUrl(CreateUrl(rootNode, node.Attributes.GetNamedItem("path").Value, false) + node.SelectSingleNode("umbracoUrlName").FirstChild.Value + "/"));
                                uList.Add(new legacyUrl(CreateUrl(rootNode, node.Attributes.GetNamedItem("path").Value, false) + node.SelectSingleNode("umbracoUrlName").FirstChild.Value + ".aspx"));
                            }

                            // - legacy urls
                            if (node.SelectSingleNode("legacyURL") != null && node.SelectSingleNode("legacyURL").FirstChild != null && !string.IsNullOrEmpty(node.SelectSingleNode("legacyURL").FirstChild.Value))
                            {
                                uList.Add(new legacyUrl(GetHostName(node.Attributes.GetNamedItem("path").Value) + node.SelectSingleNode("legacyURL").FirstChild.Value));
                            }

                            a.UrlList = JsonConvert.SerializeObject(uList);

                            // UmbracoNaviHide
                            if (node.SelectSingleNode("umbracoNaviHide") != null && node.SelectSingleNode("umbracoNaviHide").FirstChild != null)
                            {
                                a.UmbracoNaviHide = node.SelectSingleNode("umbracoNaviHide").FirstChild.Value == "1" ? true : false;
                            }

                            // Meta Description
                            if (node.SelectSingleNode("metaDescription") != null && node.SelectSingleNode("metaDescription").FirstChild != null)
                            {
                                a.MetaDescription = string.IsNullOrEmpty(node.SelectSingleNode("metaDescription").FirstChild.Value) ? string.Empty : node.SelectSingleNode("metaDescription").FirstChild.Value;
                            }

                            // Meta Keywords
                            if (node.SelectSingleNode("metaKeywords") != null && node.SelectSingleNode("metaKeywords").FirstChild != null)
                            {
                                a.MetaKeywords = string.IsNullOrEmpty(node.SelectSingleNode("metaKeywords").FirstChild.Value) ? string.Empty : node.SelectSingleNode("metaKeywords").FirstChild.Value;
                            }

                            audioToImport.Add(a);
                        }
                    }
                }
                else
                {
                    _log.Debug("Content Importer node missing publicationDate. Node ID: " + node.Attributes.GetNamedItem("id").Value);
                }
            }

            if (audioToImport.Count > 0)
            {
                this.CurrentJob.ItemCount = audioToImport.Count;
                this.CurrentJob.ItemTotal = audioToImport.Count;

                foreach (Audio item in audioToImport)
                {
                    string year = item.PublicationDate.Year.ToString();
                    string month = item.PublicationDate.Month.ToString();
                    string day = item.PublicationDate.Day.ToString();

                    Folder yf = new Folder(item.ParentId, year, "AudioFolder");
                    if (_repo.FolderExists(yf))
                    {
                        item.ParentId = yf.ID;
                    }
                    else
                    {
                        item.ParentId = _repo.SaveFolder(yf, CurrentUserId);
                    }

                    Folder mf = new Folder(item.ParentId, month, "AudioFolder");
                    if (_repo.FolderExists(mf))
                    {
                        item.ParentId = mf.ID;
                    }
                    else
                    {
                        item.ParentId = _repo.SaveFolder(mf, CurrentUserId);
                    }

                    Folder df = new Folder(item.ParentId, day, "AudioFolder");
                    if (_repo.FolderExists(df))
                    {
                        item.ParentId = df.ID;
                    }
                    else
                    {
                        item.ParentId = _repo.SaveFolder(df, CurrentUserId);
                    }

                    if (!_repo.AudioExists(item))
                    {
                        int newPdfContentId = _repo.SaveAudio(item, CurrentUserId);
                        _log.Info("Imported Audio content " + newPdfContentId.ToString());
                    }
                    this.CurrentJob.ItemCount--;
                }
            }
            else
            {
                this.CurrentJob.ItemCount = 0;
                this.CurrentJob.ItemTotal = 0;
            }
        }

        public void ImportPdfContent(XmlNode rootNode)
        {
            List<Pdf> contentToImport = new List<Pdf>();
            XmlNodeList nodes = rootNode.SelectNodes("//mediaCenterPDF[@isDoc]");
            foreach (XmlNode node in nodes)
            {
                //string x = node.Attributes.GetNamedItem("nodeName").Value;
                //string d = string.Empty;

                if (node.SelectSingleNode("publicationDate") != null && node.SelectSingleNode("publicationDate").FirstChild != null)
                {
                    DateTime pubDate;
                    if (DateTime.TryParse(node.SelectSingleNode("publicationDate").FirstChild.Value, out pubDate))
                    {
                        if (pubDate >= this.StartDate && pubDate <= this.EndDate)
                        {
                            Pdf p = new Pdf(3326, node.Attributes.GetNamedItem("nodeName").Value);

                            // title
                            if (node.SelectSingleNode("mcTitle") != null && node.SelectSingleNode("mcTitle").FirstChild != null)
                            {
                                if (!string.IsNullOrEmpty(node.SelectSingleNode("mcTitle").FirstChild.Value))
                                {
                                    p.Title = node.SelectSingleNode("mcTitle").FirstChild.Value;
                                }
                                else
                                {
                                    p.Title = node.Attributes.GetNamedItem("nodeName").Value;
                                }
                            }

                            // publicationDate
                            p.PublicationDate = pubDate;

                            // summaryContent
                            if (node.SelectSingleNode("bodyText") != null && node.SelectSingleNode("bodyText").FirstChild != null)
                            {
                                p.SummaryContent = node.SelectSingleNode("bodyText").FirstChild.Value;
                            }

                            // bodyContent
                            if (node.SelectSingleNode("mcDescription") != null && node.SelectSingleNode("mcDescription").FirstChild != null)
                            {
                                p.BodyContent = node.SelectSingleNode("mcDescription").FirstChild.Value;
                            }
                            
                            // pdfFile
                            if (node.SelectSingleNode("mcItemFile") != null && node.SelectSingleNode("mcItemFile").FirstChild != null)
                            {
                                p.PdfFile = node.SelectSingleNode("mcItemFile").FirstChild.Value.ToString().Replace("/media/", "/legacy/media/");
                            }

                            // thumbnailImage
                            if (node.SelectSingleNode("mcThumbnail") != null && node.SelectSingleNode("mcThumbnail").FirstChild != null)
                            {
                                p.ThumbnailImage = "/legacy" + GetMediaPath("/App_Data/pdf_media_list.xml", "//pdf", node.Attributes.GetNamedItem("id").Value.ToString(), "thumbnail");
                            }

                            // metaDescription
                            if (node.SelectSingleNode("metaDescription") != null && node.SelectSingleNode("metaDescription").FirstChild != null)
                            {
                                p.MetaDescription = string.IsNullOrEmpty(node.SelectSingleNode("metaDescription").FirstChild.Value) ? string.Empty : node.SelectSingleNode("metaDescription").FirstChild.Value;
                            }

                            // metaKeywords
                            if (node.SelectSingleNode("metaKeywords") != null && node.SelectSingleNode("metaKeywords").FirstChild != null)
                            {
                                p.MetaKeywords = string.IsNullOrEmpty(node.SelectSingleNode("metaKeywords").FirstChild.Value) ? string.Empty : node.SelectSingleNode("metaKeywords").FirstChild.Value;
                            }

                            // umbracoNaviHide
                            if (node.SelectSingleNode("umbracoNaviHide") != null && node.SelectSingleNode("umbracoNaviHide").FirstChild != null)
                            {
                                p.UmbracoNaviHide = node.SelectSingleNode("umbracoNaviHide").FirstChild.Value == "1" ? true : false;
                            }

                            // content buckets
                            if (node.Attributes.GetNamedItem("path").Value.StartsWith("-1,3812,"))
                            {
                                p.ContentBuckets = "8167";
                            }
                            else if (node.Attributes.GetNamedItem("path").Value.StartsWith("-1,42579,"))
                            {
                                p.ContentBuckets = "8166";
                            }

                            // original urls
                            List<legacyUrl> uList = new List<legacyUrl>(); //object to be serialized to json

                            string legacyUrl = CreateUrl(rootNode, node.Attributes.GetNamedItem("path").Value, true);
                            uList.Add(new legacyUrl(legacyUrl + "/"));
                            uList.Add(new legacyUrl(legacyUrl + ".aspx"));

                            // umbracoUrlAlias
                            if (node.SelectSingleNode("umbracoUrlAlias") != null && node.SelectSingleNode("umbracoUrlAlias").FirstChild != null && !string.IsNullOrEmpty(node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value))
                            {
                                string host = GetHostName(node.Attributes.GetNamedItem("path").Value);
                                if (node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value.Contains(','))
                                {
                                    foreach (string item in node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value.Split(','))
                                    {
                                        uList.Add(new legacyUrl(host + "/" + item + "/"));
                                        uList.Add(new legacyUrl(host + "/" + item + ".aspx"));
                                    }
                                }
                                else
                                {
                                    uList.Add(new legacyUrl(host + "/" + node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value + "/"));
                                    uList.Add(new legacyUrl(host + "/" + node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value + ".aspx"));
                                }
                            }

                            // umbracoUrlName
                            if (node.SelectSingleNode("umbracoUrlName") != null && node.SelectSingleNode("umbracoUrlName").FirstChild != null && !string.IsNullOrEmpty(node.SelectSingleNode("umbracoUrlName").FirstChild.Value))
                            {
                                uList.Add(new legacyUrl(CreateUrl(rootNode, node.Attributes.GetNamedItem("path").Value, false) + node.SelectSingleNode("umbracoUrlName").FirstChild.Value + "/"));
                                uList.Add(new legacyUrl(CreateUrl(rootNode, node.Attributes.GetNamedItem("path").Value, false) + node.SelectSingleNode("umbracoUrlName").FirstChild.Value + ".aspx"));
                            }

                            // legacy urls
                            if (node.SelectSingleNode("legacyURL") != null && node.SelectSingleNode("legacyURL").FirstChild != null && !string.IsNullOrEmpty(node.SelectSingleNode("legacyURL").FirstChild.Value))
                            {
                                uList.Add(new legacyUrl(GetHostName(node.Attributes.GetNamedItem("path").Value) + node.SelectSingleNode("legacyURL").FirstChild.Value));
                            }

                            p.UrlList = JsonConvert.SerializeObject(uList);

                            // and add it to the list
                            contentToImport.Add(p);
                        }
                    }
                }
                else
                {
                    _log.Debug("Content Importer node missing publicationDate. Node ID: " + node.Attributes.GetNamedItem("id").Value);
                }
            }

            if (contentToImport.Count > 0)
            {
                this.CurrentJob.ItemCount = contentToImport.Count;
                this.CurrentJob.ItemTotal = contentToImport.Count;

                foreach (Pdf item in contentToImport)
                {
                    string year = item.PublicationDate.Year.ToString();
                    string month = item.PublicationDate.Month.ToString();
                    string day = item.PublicationDate.Day.ToString();

                    Folder yf = new Folder(item.ParentId, year, "PDFFolder");
                    if (_repo.FolderExists(yf))
                    {
                        item.ParentId = yf.ID;
                    }
                    else
                    {
                        item.ParentId = _repo.SaveFolder(yf, CurrentUserId);
                    }

                    Folder mf = new Folder(item.ParentId, month, "PDFFolder");
                    if (_repo.FolderExists(mf))
                    {
                        item.ParentId = mf.ID;
                    }
                    else
                    {
                        item.ParentId = _repo.SaveFolder(mf, CurrentUserId);
                    }

                    Folder df = new Folder(item.ParentId, day, "PDFFolder");
                    if (_repo.FolderExists(df))
                    {
                        item.ParentId = df.ID;
                    }
                    else
                    {
                        item.ParentId = _repo.SaveFolder(df, CurrentUserId);
                    }

                    if (!_repo.PDFExists(item))
                    {
                        int newPdfContentId = _repo.SavePDF(item, CurrentUserId);
                        _log.Info("Imported PDF content " + newPdfContentId.ToString());
                    }
                    this.CurrentJob.ItemCount--;
                }
            }
            else
            {
                this.CurrentJob.ItemCount = 0;
                this.CurrentJob.ItemTotal = 0;

            }
        }

        public void ImportVideoContent(XmlNode rootNode)
        {
            List<Video> videoToImport = new List<Video>();
            XmlNodeList nodes = rootNode.SelectNodes("//mediaCenterVideo[@isDoc]");
            foreach (XmlNode node in nodes)
            {
                if (node.SelectSingleNode("publicationDate") != null && node.SelectSingleNode("publicationDate").FirstChild != null)
                {
                    DateTime pubDate;
                    if (DateTime.TryParse(node.SelectSingleNode("publicationDate").FirstChild.Value, out pubDate))
                    {
                        if (pubDate >= this.StartDate && pubDate <= this.EndDate)
                        {
                            Video v = new Video(3318, node.Attributes.GetNamedItem("nodeName").Value);
                            
                            // Title
                            if (node.SelectSingleNode("mcTitle") != null && node.SelectSingleNode("mcTitle").FirstChild != null)
                            {
                                if (!string.IsNullOrEmpty(node.SelectSingleNode("mcTitle").FirstChild.Value))
                                {
                                    v.Title = node.SelectSingleNode("mcTitle").FirstChild.Value;
                                }
                                else
                                {
                                    v.Title = node.Attributes.GetNamedItem("nodeName").Value;
                                }
                            }

                            // Publication Date
                            v.PublicationDate = pubDate;

                            // Thumbnail Image
                            if (node.SelectSingleNode("mcThumbnail") != null && node.SelectSingleNode("mcThumbnail").FirstChild != null)
                            {
                                if (!string.IsNullOrEmpty(node.SelectSingleNode("mcThumbnail").FirstChild.Value))
                                {
                                    v.Thumbnail = node.SelectSingleNode("mcThumbnail").FirstChild.Value.Replace("/media/", "/legacy/media/");
                                }
                            }

                            if (node.SelectSingleNode("mcNRANewsThumb") != null && node.SelectSingleNode("mcNRANewsThumb").FirstChild != null)
                            {
                                if (!string.IsNullOrEmpty(node.SelectSingleNode("mcNRANewsThumb").FirstChild.Value))
                                {
                                    v.Thumbnail = "/legacy/nra-news-thumbs/" + node.SelectSingleNode("mcNRANewsThumb").FirstChild.Value;
                                }
                            }

                            // Poster Image
                            if (node.SelectSingleNode("mcSplash") != null && node.SelectSingleNode("mcSplash").FirstChild != null)
                            {
                                v.Poster = "/legacy" + GetMediaPath("/App_Data/video_media_list.xml", "//video", node.Attributes.GetNamedItem("id").Value.ToString(), "poster");
                            }

                            // Video Data
                            if (node.SelectSingleNode("mcHosting") != null && node.SelectSingleNode("mcHosting").FirstChild != null && node.SelectSingleNode("mcItemUrl") != null && node.SelectSingleNode("mcItemUrl").FirstChild != null)
                            {
                                if (!string.IsNullOrEmpty(node.SelectSingleNode("mcHosting").FirstChild.Value))
                                {
                                    switch (node.SelectSingleNode("mcHosting").FirstChild.Value)
                                    {
                                        //fms host
                                        case "173":
                                            v.LegacyVideoStream = "http://fms1.nranews.com/nra_ila/" + node.SelectSingleNode("mcItemUrl").FirstChild.Value;
                                            v.LegacyVideoFlashStream = "rtmp://fms1.nranews.com/nra_ila/mp4:" + node.SelectSingleNode("mcItemUrl").FirstChild;
                                            break;

                                        // NRA.org
                                        case "266":
                                            v.LegacyVideoStream = "http://d28ncmsnoi76d4.cloudfront.net/uploads/production/videofiles/base/" + node.SelectSingleNode("mcItemUrl").FirstChild.Value;
                                            v.LegacyVideoFlashStream = "rtmp://s15184nhr5m0oj.cloudfront.net/cfx/st/_definst_/mp4:uploads/production/videofiles/base/" + node.SelectSingleNode("mcItemUrl").FirstChild.Value;
                                            break;

                                        // NRA.news
                                        case "267":
                                            v.LegacyVideoStream = "http://d2x1oedzzjp4du.cloudfront.net/uploads/production/videofiles/base/" + node.SelectSingleNode("mcItemUrl").FirstChild.Value;
                                            v.LegacyVideoFlashStream = "rtmp://s20fryxrsuzx1o.cloudfront.net/cfx/st/_definst_/mp4:uploads/production/videofiles/base/" + node.SelectSingleNode("mcItemUrl").FirstChild.Value;
                                            break;
                                    }
                                }
                            }

                            // Content Buckets
                            if (node.Attributes.GetNamedItem("path").Value.StartsWith("-1,3812,"))
                            {
                                v.ContentBuckets = "8167";
                            }
                            else if (node.Attributes.GetNamedItem("path").Value.StartsWith("-1,42579,"))
                            {
                                v.ContentBuckets = "8166";
                            }

                            // Geographic Tags ... none exist

                            // Topic Tags ... none exist

                            // Alternate & Legacy URLs
                            List<legacyUrl> uList = new List<legacyUrl>(); //object to be serialized to json

                            // - original urls
                            string legacyUrl = CreateUrl(rootNode, node.Attributes.GetNamedItem("path").Value, true);
                            uList.Add(new legacyUrl(legacyUrl + "/"));
                            uList.Add(new legacyUrl(legacyUrl + ".aspx"));

                            // - umbracoUrlAlias
                            if (node.SelectSingleNode("umbracoUrlAlias") != null && node.SelectSingleNode("umbracoUrlAlias").FirstChild != null && !string.IsNullOrEmpty(node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value))
                            {
                                string host = GetHostName(node.Attributes.GetNamedItem("path").Value);
                                if (node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value.Contains(','))
                                {
                                    foreach (string item in node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value.Split(','))
                                    {
                                        uList.Add(new legacyUrl(host + "/" + item + "/"));
                                        uList.Add(new legacyUrl(host + "/" + item + ".aspx"));
                                    }
                                }
                                else
                                {
                                    uList.Add(new legacyUrl(host + "/" + node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value + "/"));
                                    uList.Add(new legacyUrl(host + "/" + node.SelectSingleNode("umbracoUrlAlias").FirstChild.Value + ".aspx"));
                                }
                            }

                            // - umbracoUrlName
                            if (node.SelectSingleNode("umbracoUrlName") != null && node.SelectSingleNode("umbracoUrlName").FirstChild != null && !string.IsNullOrEmpty(node.SelectSingleNode("umbracoUrlName").FirstChild.Value))
                            {
                                uList.Add(new legacyUrl(CreateUrl(rootNode, node.Attributes.GetNamedItem("path").Value, false) + node.SelectSingleNode("umbracoUrlName").FirstChild.Value + "/"));
                                uList.Add(new legacyUrl(CreateUrl(rootNode, node.Attributes.GetNamedItem("path").Value, false) + node.SelectSingleNode("umbracoUrlName").FirstChild.Value + ".aspx"));
                            }

                            // - legacy urls
                            if (node.SelectSingleNode("legacyURL") != null && node.SelectSingleNode("legacyURL").FirstChild != null && !string.IsNullOrEmpty(node.SelectSingleNode("legacyURL").FirstChild.Value))
                            {
                                uList.Add(new legacyUrl(GetHostName(node.Attributes.GetNamedItem("path").Value) + node.SelectSingleNode("legacyURL").FirstChild.Value));
                            }

                            v.UrlList = JsonConvert.SerializeObject(uList);

                            // UmbracoNaviHide
                            if (node.SelectSingleNode("umbracoNaviHide") != null && node.SelectSingleNode("umbracoNaviHide").FirstChild != null)
                            {
                                v.UmbracoNaviHide = node.SelectSingleNode("umbracoNaviHide").FirstChild.Value == "1" ? true : false;
                            }

                            // Meta Description
                            if (node.SelectSingleNode("metaDescription") != null && node.SelectSingleNode("metaDescription").FirstChild != null)
                            {
                                v.MetaDescription = string.IsNullOrEmpty(node.SelectSingleNode("metaDescription").FirstChild.Value) ? string.Empty : node.SelectSingleNode("metaDescription").FirstChild.Value;
                            }

                            // Meta Keywords
                            if (node.SelectSingleNode("metaKeywords") != null && node.SelectSingleNode("metaKeywords").FirstChild != null)
                            {
                                v.MetaKeywords = string.IsNullOrEmpty(node.SelectSingleNode("metaKeywords").FirstChild.Value) ? string.Empty : node.SelectSingleNode("metaKeywords").FirstChild.Value;
                            }

                            videoToImport.Add(v);
                        }
                    }
                }
                else
                {
                    _log.Debug("Content Importer node missing publicationDate. Node ID: " + node.Attributes.GetNamedItem("id").Value);
                }
            }

            if (videoToImport.Count > 0)
            {
                this.CurrentJob.ItemCount = videoToImport.Count;
                this.CurrentJob.ItemTotal = videoToImport.Count;

                foreach (Video item in videoToImport)
                {
                    string year = item.PublicationDate.Year.ToString();
                    string month = item.PublicationDate.Month.ToString();
                    string day = item.PublicationDate.Day.ToString();

                    item.ParentId = GetFolderId(item.ParentId, year, "VideoFolder");
                    item.ParentId = GetFolderId(item.ParentId, month, "VideoFolder");
                    item.ParentId = GetFolderId(item.ParentId, day, "VideoFolder");

                    if (!_repo.VideoExists(item))
                    {
                        int newVideoContentId = _repo.SaveVideo(item, CurrentUserId);
                        _log.Info("Imported Video content " + newVideoContentId.ToString());
                    }
                    this.CurrentJob.ItemCount--;
                }
            }
            else
            {
                this.CurrentJob.ItemCount = 0;
                this.CurrentJob.ItemTotal = 0;
            }
        }

        public void Start()
        {



            _log.Debug("*** IMPORTER Start ***");
            _log.Debug("*** IMPORTER Start ***");
            _log.Debug("*** IMPORTER Start ***");
            _log.Debug("*** IMPORTER Start ***");
            _log.Debug("*** IMPORTER Start ***");
            _log.Debug("*** IMPORTER Start ***");
            _log.Debug("*** IMPORTER Start ***");
            _log.Debug("*** IMPORTER Start ***");
            _log.Debug("*** IMPORTER Start ***");
            _log.Debug("*** IMPORTER Start ***");
            _log.Debug("*** IMPORTER Start ***");


            Console.WriteLine("*** IMPORTER ***");
            Console.WriteLine("*** IMPORTER ***");
            Console.WriteLine("*** IMPORTER ***");
            Console.WriteLine("*** IMPORTER ***"); 
            Console.WriteLine("*** IMPORTER ***");
            Console.WriteLine("*** IMPORTER ***");
            Console.WriteLine("*** IMPORTER ***");
            Console.WriteLine("*** IMPORTER ***");

            System.Diagnostics.Debug.WriteLine("*** IMPORTER 2 ***");
            System.Diagnostics.Debug.WriteLine("*** IMPORTER 2 ***");
            System.Diagnostics.Debug.WriteLine("*** IMPORTER 2 ***");
            System.Diagnostics.Debug.WriteLine("*** IMPORTER 2 ***");
            System.Diagnostics.Debug.WriteLine("*** IMPORTER 2 ***");
            System.Diagnostics.Debug.WriteLine("*** IMPORTER 2 ***");
            System.Diagnostics.Debug.WriteLine("*** IMPORTER 2 ***");




            if (!string.IsNullOrEmpty(this.FilePath)
                && ImportType != Job.ContentDocumentType.None
                && this.StartDate != null
                && this.EndDate != null)
            {
                try
                {


                    System.Diagnostics.Debug.WriteLine("this.FilePath: " + this.FilePath);


                    XmlDocument doc = new XmlDocument();
                    doc.Load(this.FilePath);

                    XmlNode root = doc.DocumentElement;

                    switch (ImportType)
                    {
                        case Job.ContentDocumentType.None:
                            break;
                        case Job.ContentDocumentType.Article:
                            ImportArticleContent(root);
                            break;
                        case Job.ContentDocumentType.Video:
                            ImportVideoContent(root);
                            break;
                        case Job.ContentDocumentType.Audio:
                            ImportAudioContent(root);
                            break;
                        case Job.ContentDocumentType.PDF:
                            ImportPdfContent(root);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("Content Importer Oops!", ex);
                }
            }
        }
    }

    class contentBucket
    {
        public int id { get; set; }
        public string nodeName { get; set; }
        public string path { get; set; }
        public contentBucket(int id, string nodeName, string path)
        {
            this.id = id;
            this.nodeName = nodeName;
            this.path = path;
        }
    }
    
    class ContentBuckets
    {
        public static readonly List<contentBucket> buckets = new List<contentBucket>();
        static ContentBuckets()
        {
            //buckets.Add(new contentBucket(8165, "GunBanFacts", ""));
            buckets.Add(new contentBucket(8168, "GBF: Assault Weapons", "-1,42579,251344,251345"));
            buckets.Add(new contentBucket(8169, "GBF: FAQ", "-1,42579,251344,251373"));
            buckets.Add(new contentBucket(8170, "GBF: History", "-1,42579,251344,251388"));
            buckets.Add(new contentBucket(8171, "GBF: Large Magazines", "-1,42579,251344,251351"));
            buckets.Add(new contentBucket(8172, "GBF: Myths vs. Facts", "-1,42579,251344,251358"));
            //buckets.Add(new contentBucket(8173, "GBF: News", ""));
            //buckets.Add(new contentBucket(8174, "GBF: Resources", ""));
            //buckets.Add(new contentBucket(8166, "NRA-ILA", "-1,42579"));
            buckets.Add(new contentBucket(8176, "ILA: Armed Citizen", "-1,42579,42605,42673"));
            buckets.Add(new contentBucket(8177, "ILA: Federal Legislation", "-1,42579,42606,42675"));
            buckets.Add(new contentBucket(8178, "ILA: From The Director", "-1,42579,42610,42711"));
            buckets.Add(new contentBucket(8195, "ILA: Get Involved Locally, Articles", "-1,42579,42607,253247"));
            buckets.Add(new contentBucket(8179, "ILA: Grassroots Alert", "-1,42579,42607,42680,86402"));
            buckets.Add(new contentBucket(8180, "ILA: Gun Laws", "-1,42579,42605,42672"));
            buckets.Add(new contentBucket(8188, "ILA: Hunting: Articles", "-1,42579,42608,42685"));
            buckets.Add(new contentBucket(8190, "ILA: Hunting: Fact Sheets", "-1,42579,42608,42687"));
            buckets.Add(new contentBucket(8189, "ILA: Hunting: Issues and Alerts", "-1,42579,42608,42686"));
            buckets.Add(new contentBucket(8181, "ILA: Hunting: News", "-1,42579,42608,42684"));
            buckets.Add(new contentBucket(8183, "ILA: Legal", "-1,42579,88824,88837"));
            buckets.Add(new contentBucket(8193, "ILA: Legal: Legal Update", "-1,42579,88824,89643"));
            buckets.Add(new contentBucket(8185, "ILA: News and Issues: Articles", "-1,42579,42604,42670"));
            buckets.Add(new contentBucket(8191, "ILA: News and Issues: Fact Sheets", "-1,42579,42604,42668"));
            buckets.Add(new contentBucket(8182, "ILA: News and Issues: In The News", "-1,42579,42604,42666"));
            buckets.Add(new contentBucket(8186, "ILA: News and Issues: News From NRA-ILA", "-1,42579,42604,42667"));
            buckets.Add(new contentBucket(8192, "ILA: News and Issues: Speeches", "-1,42579,42604,88670"));
            buckets.Add(new contentBucket(8194, "ILA: Second Amendment", "-1,42579,42609,42922"));
            buckets.Add(new contentBucket(8187, "ILA: State Legislation", "-1,42579,42606,42676"));
            //buckets.Add(new contentBucket(8167, "NRA-PVF", "-1,3812"));
            buckets.Add(new contentBucket(8175, "PVF: News and Alerts", "-1,3812,3830"));
        }
    }

    class legacyUrl
    {
        public string item { get; set; }

        public legacyUrl(string i)
        {
            this.item = i;
        }
    }

    class Tag
    {
        public string tag { get; set; }

        public Tag(string val)
        {
            this.tag = val;
        }
    }

    class usState
    {
        private string _abbreviation;
        private int _id;
        private string _name;
        private int _prevalue;

        public string Abbreviation
        {
            get
            {
                return _abbreviation;
            }
        }
        public int id
        {
            get
            {
                return _id;
            }
        }
        public string name
        {
            get
            {
                return _name;
            }
        }
        public int prevalue
        {
            get
            {
                return _prevalue;
            }
        }

        public usState(int id, string abbr, string name, int preval)
        {
            _id = id;
            _abbreviation = abbr;
            _name = name;
            _prevalue = preval;
        }
    }

    class USStates
    {
        public static readonly List<usState> states = new List<usState>();
        static USStates()
        {
            states.Add(new usState(10465, "AL", "Alabama", 1056));
            states.Add(new usState(10466, "AK", "Alaska", 1057));
            states.Add(new usState(10467, "AZ", "Arizona", 1058));
            states.Add(new usState(10468, "AR", "Arkansas", 1059));
            states.Add(new usState(10469, "CA", "California", 1060));
            states.Add(new usState(10470, "CO", "Colorado", 1061));
            states.Add(new usState(10471, "CT", "Connecticut", 1062));
            states.Add(new usState(10472, "DE", "Delaware", 1063));
            states.Add(new usState(107131, "DC", "District of Columbia", 1106));
            states.Add(new usState(10473, "FL", "Florida", 1064));
            states.Add(new usState(10474, "GA", "Georgia", 1065));
            states.Add(new usState(10475, "HI", "Hawaii", 1066));
            states.Add(new usState(10476, "ID", "Idaho", 1067));
            states.Add(new usState(10477, "IL", "Illinois", 1068));
            states.Add(new usState(10478, "IN", "Indiana", 1069));
            states.Add(new usState(10479, "IA", "Iowa", 1070));
            states.Add(new usState(10480, "KS", "Kansas", 1071));
            states.Add(new usState(10481, "KY", "Kentucky", 1072));
            states.Add(new usState(10482, "LA", "Louisiana", 1073));
            states.Add(new usState(10483, "ME", "Maine", 1074));
            states.Add(new usState(10484, "MD", "Maryland", 1075));
            states.Add(new usState(10485, "MA", "Massachusetts", 1076));
            states.Add(new usState(10486, "MI", "Michigan", 1077));
            states.Add(new usState(10487, "MN", "Minnesota", 1078));
            states.Add(new usState(10488, "MS", "Mississippi", 1079));
            states.Add(new usState(10489, "MO", "Missouri", 1080));
            states.Add(new usState(10490, "MT", "Montana", 1081));
            states.Add(new usState(10491, "NE", "Nebraska", 1082));
            states.Add(new usState(10492, "NV", "Nevada", 1083));
            states.Add(new usState(10493, "NH", "New Hampshire", 1084));
            states.Add(new usState(10494, "NJ", "New Jersey", 1085));
            states.Add(new usState(10495, "NM", "New Mexico", 1086));
            states.Add(new usState(10496, "NY", "New York", 1087));
            states.Add(new usState(10497, "NC", "North Carolina", 1088));
            states.Add(new usState(10498, "ND", "North Dakota", 1089));
            states.Add(new usState(10499, "OH", "Ohio", 1090));
            states.Add(new usState(10500, "OK", "Oklahoma", 1091));
            states.Add(new usState(10501, "OR", "Oregon", 1092));
            states.Add(new usState(10502, "PA", "Pennsylvania", 1093));
            states.Add(new usState(10503, "RI", "Rhode Island", 1094));
            states.Add(new usState(10504, "SC", "South Carolina", 1095));
            states.Add(new usState(10505, "SD", "South Dakota", 1096));
            states.Add(new usState(10506, "TN", "Tennessee", 1097));
            states.Add(new usState(10507, "TX", "Texas", 1098));
            states.Add(new usState(10508, "UT", "Utah", 1099));
            states.Add(new usState(10509, "VT", "Vermont", 1100));
            states.Add(new usState(10510, "VA", "Virginia", 1101));
            states.Add(new usState(10511, "WA", "Washington", 1102));
            states.Add(new usState(10512, "WV", "West Virginia", 1103));
            states.Add(new usState(10513, "WI", "Wisconsin", 1104));
            states.Add(new usState(10514, "WY", "Wyoming", 1105));
        }
    }
}