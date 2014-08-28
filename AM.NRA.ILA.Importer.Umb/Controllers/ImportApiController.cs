using AM.NRA.ILA.Importer.Umb.Models;
using AM.NRA.ILA.Importer.Umb.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace AM.NRA.ILA.Importer.Umb.Controllers
{
    /// <summary>
    /// Web API Controller for running content imports.
    /// Example route URL:
    /// http://localhost/umbraco/AMImporter/ImportApi/Test
    /// </summary>
    [PluginController("AMImporter")]
    public class ImportApiController : UmbracoAuthorizedApiController
    {
        private JobsRepository _jobrepo = JobsRepository.Instance;

        private int GetCurrentUserId()
        {
            var userService = ApplicationContext.Current.Services.UserService;
            return userService.GetByUsername(HttpContext.Current.User.Identity.Name).Id;
        }

        public int GetItemCount()
        {
            int result = 0;

            if (_jobrepo.Jobs != null && _jobrepo.Jobs.Count >= 1)
            {
                result = _jobrepo.Jobs[0].ItemCount;
            }

            return result;
        }

        public List<Job> GetJobs()
        {
            _jobrepo.Cleanup();
            return _jobrepo.Jobs;
        }

        public string GetTest()
        {
            string result = "hello clarice";
            return result;
        }

        public bool PostStartImport(Job job)
        {
            if (!string.IsNullOrEmpty(job.SourceFile))
            {
                string pathToSourceXmlFile = HttpContext.Current.Server.MapPath("/App_Data/" + job.SourceFile);

                if (!string.IsNullOrEmpty(job.StartDate))
                {
                    string[] startDateParts = job.StartDate.Split('-');
                    DateTime startDateTime = new DateTime(int.Parse(startDateParts[0]), int.Parse(startDateParts[1]), int.Parse(startDateParts[2]));

                    if (!string.IsNullOrEmpty(job.EndDate))
                    {
                        string[] endDateParts = job.EndDate.Split('-');
                        DateTime endDateTime = new DateTime(int.Parse(endDateParts[0]), int.Parse(endDateParts[1]), int.Parse(endDateParts[2]));

                        if (job.JobContentType != Job.ContentDocumentType.None)
                        {
                            // kickoff import in new thread
                            Import.Importer oImporter = Import.Importer.Instance;
                            oImporter.FilePath = pathToSourceXmlFile;
                            oImporter.ImportType = job.JobContentType;
                            oImporter.StartDate = startDateTime;
                            oImporter.EndDate = endDateTime;
                            oImporter.CurrentJob = job;
                            oImporter.CurrentContext = HttpContext.Current;
                            oImporter.CurrentUserId = GetCurrentUserId();

                            Thread oThread = new Thread(new ThreadStart(oImporter.Start));
                            oThread.Start();
                            while (!oThread.IsAlive);

                            _jobrepo.AddJob(job);

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}