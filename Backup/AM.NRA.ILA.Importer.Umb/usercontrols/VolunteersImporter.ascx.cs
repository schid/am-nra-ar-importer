using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace AM.NRA.ILA.Importer.Umb.usercontrols
{
    public partial class VolunteersImporter : System.Web.UI.UserControl
    {
        private int startingNode = 1184;
        private IContentService umbContentService = ApplicationContext.Current.Services.ContentService;
        
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SubmitButton_Clicked(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Server.MapPath("/App_Data/umbracoconfig.xml"));
            XPathNavigator navigator = doc.CreateNavigator();

            XPathNodeIterator volunteerFoldersIterator = navigator.Select("//evcList/folder");
            while (volunteerFoldersIterator.MoveNext())
            {
                // create VolunteerFolder node under starting node
                // set nodeName attribute
                // capture node ID
                string folderName = volunteerFoldersIterator.Current.GetAttribute("nodeName", "");

                if (volunteerFoldersIterator.Current.HasChildren)
                {
                    XPathNavigator volunteersNavigator = volunteerFoldersIterator.Current;
                    XPathNodeIterator volunteersIterator = volunteersNavigator.SelectChildren("evcItem", "");
                    while (volunteersIterator.MoveNext())
                    {
                        string x = volunteersIterator.Current.LocalName;
                        // create ElectionVolunteerCoordinator node under VolunteerFolder node
                        // set nodeName attribute
                        // set federalDistrictCode
                        // set name
                        // set streetAddress
                        // set city
                        // set state (convert value)
                        // set zipCode
                        // set phoneNumber
                        // set emailAddress
                        // set categoryTags
                        // set stateTags
                    }
                }
            }
        }
    }
}