using AM.NRA.ILA.Importer.Umb.Models;
using AM.NRA.ILA.Importer.Umb.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;
using Umbraco.Core;

namespace AM.NRA.ILA.Importer.Umb.usercontrols
{
    public partial class TopicTagsImporter : System.Web.UI.UserControl
    {
        private int _startingNode = 1113;
        private Repository _repo = new Repository();
        
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Server.MapPath("/App_Data/ILA_Tags_20140212_to-be-imported.xml"));
            XPathNavigator navigator = doc.CreateNavigator();

            XPathNodeIterator dataNodes = navigator.Select("/Workbook/Data");

            while (dataNodes.MoveNext())
            {
                string tagText = dataNodes.Current.Value.ToString();

                // get first character of tagText
                string firstChar = tagText.Remove(1).ToUpper();

                // check if there is a folder matching the first character ... set that node id as the parent
                int folderId = 0;
                Folder f = new Folder(_startingNode, firstChar, "TagFolder");
                if (_repo.FolderExists(f))
                {
                    folderId = f.ID;
                }
                else
                {
                    folderId = _repo.SaveFolder(f, GetCurrentUserId());
                }

                // create tag content
                if (folderId > 0)
                {
                    Tag t = new Tag(folderId, tagText, "TopicTag");
                    t.Title = tagText;
                    if (!_repo.TagExists(t))
                    {
                        _repo.SaveTag(t, GetCurrentUserId());
                    }
                }
            }
        }

        private int GetCurrentUserId()
        {
            var userService = ApplicationContext.Current.Services.UserService;
            return userService.GetByUsername(HttpContext.Current.User.Identity.Name).Id;
        }
    }
}