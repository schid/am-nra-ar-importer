using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using log4net;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace AM.NRA.ILA.Importer.Umb.usercontrols
{
    public partial class FixTags : System.Web.UI.UserControl
    {
        private IContentService _contentService = ApplicationContext.Current.Services.ContentService;
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int FixThisTag(string tagTypeAlias, string tagText)
        {
            int result = 0;
            int nodeId = 0;
            switch (tagTypeAlias)
            {
                case "geographicTag":
                    nodeId = 1524;
                    break;
                case "topicTag":
                    nodeId = 1113;
                    break;
            }

            foreach (var item in _contentService.GetDescendants(nodeId))
            {
                if (item.ContentType.Alias.ToLower() == tagTypeAlias.ToLower())
                {
                    if (item.GetValue("title").ToString().ToLower() == tagText.ToLower())
                    {
                        result = item.Id;
                        break;
                    }
                }
            }

            return result;
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            this.literalMessage.Text = "<p>We're fixing it ...</p>";
            foreach (var node in _contentService.GetDescendants(int.Parse(this.startNode.Text)))
            {
                if (node.ContentType.Alias == "Article")
                {
                    var doc = _contentService.GetById(node.Id);
                    bool doPublish = false;

                    if (!string.IsNullOrEmpty(doc.GetValue("geographicTags").ToString()))
                    {
                        List<TagToFix> geoTags = JsonConvert.DeserializeObject<List<TagToFix>>(doc.GetValue("geographicTags").ToString());
                        foreach (TagToFix t in geoTags)
                        {
                            if (t.id == 0)
                            {
                                doPublish = true;
                                t.id = FixThisTag("geographicTag", t.tag);
                            }
                        }
                        doc.SetValue("geographicTags", JsonConvert.SerializeObject(geoTags));
                    }

                    if (!string.IsNullOrEmpty(doc.GetValue("topicTags").ToString()))
                    {
                        List<TagToFix> topTags = JsonConvert.DeserializeObject<List<TagToFix>>(doc.GetValue("topicTags").ToString());
                        foreach (TagToFix t in topTags)
                        {
                            if (t.id == 0)
                            {
                                doPublish = true;
                                t.id = FixThisTag("topicTag", t.tag);
                            }
                        }
                        doc.SetValue("topicTags", JsonConvert.SerializeObject(topTags));
                    }

                    if (doPublish)
                    {
                        _contentService.SaveAndPublishWithStatus(doc);
                        _log.Info("Article " + node.Id.ToString() + " was updated with new tag json.");
                    }
                    doc.DisposeIfDisposable();
                }
            }
            this.literalMessage.Text = "<p>Our work here is done.</p>";
        }
    }

    class TagToFix
    {
        public string tag { get; set; }
        public int id { get; set; }
    }
}