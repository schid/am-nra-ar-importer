using AM.NRA.ILA.Importer.Umb.Models;
using AM.NRA.ILA.Importer.Umb.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;
using Umbraco.Core;

namespace AM.NRA.ILA.Importer.Umb.usercontrols
{
    public partial class VolunteersImporter : System.Web.UI.UserControl
    {
        private int startingNode = 1184;
        private Repository _repo = new Repository();
        
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
                Folder stateFolder = new Folder(startingNode, volunteerFoldersIterator.Current.GetAttribute("nodeName", ""), "VolunteerFolder");
                int stateFolderId = _repo.SaveFolder(stateFolder, GetCurrentUserId());

                if (volunteerFoldersIterator.Current.HasChildren)
                {
                    XPathNavigator volunteersNavigator = volunteerFoldersIterator.Current;
                    XPathNodeIterator volunteersIterator = volunteersNavigator.SelectChildren("evcItem", "");
                    while (volunteersIterator.MoveNext())
                    {
                        // create ElectionVolunteerCoordinator node under VolunteerFolder node
                        EVC volunteer = new EVC(stateFolderId, volunteersIterator.Current.GetAttribute("nodeName", ""));
                        volunteer.FederalDistrictCode = volunteersIterator.Current.SelectSingleNode("federalDistrict").Value.ToString();
                        volunteer.Name = volunteersIterator.Current.SelectSingleNode("evcName").Value.ToString();
                        volunteer.StreetAddress = volunteersIterator.Current.SelectSingleNode("evcAddress").Value.ToString();
                        volunteer.City = volunteersIterator.Current.SelectSingleNode("evcCity").Value.ToString();
                        volunteer.State = ConvertState(volunteersIterator.Current.SelectSingleNode("evcState").Value.ToString());
                        volunteer.ZipCode = volunteersIterator.Current.SelectSingleNode("evcZip").Value.ToString();
                        volunteer.PhoneNumber = volunteersIterator.Current.SelectSingleNode("evcPhone").Value.ToString();
                        volunteer.EmailAddress = volunteersIterator.Current.SelectSingleNode("evcEmail").Value.ToString();
                        volunteer.CategoryTags = "1114,1115";
                        volunteer.StateTags = ConvertState(volunteersIterator.Current.SelectSingleNode("evcState").Value.ToString());
                        _repo.SaveEVC(volunteer, GetCurrentUserId());
                    }
                }
            }
            this.ResultsMessage.Text = "<p>Import is complete.</p>";
        }

        private string ConvertState(string s)
        {
            string result = string.Empty;
            foreach (AM.NRA.ILA.Importer.Umb.Import.usState state in AM.NRA.ILA.Importer.Umb.Import.USStates.states)
            {
                if (state.id.ToString() == s || state.Abbreviation == s)
                {
                    result = state.prevalue.ToString();
                    break;
                }
            }
            return result;
        }

        private int GetCurrentUserId()
        {
            var userService = ApplicationContext.Current.Services.UserService;
            return userService.GetByUsername(HttpContext.Current.User.Identity.Name).Id;
        }
    }
}