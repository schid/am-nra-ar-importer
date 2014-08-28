using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.NRA.ILA.Importer.Umb.Models.Repository
{
    public sealed class JobsRepository
    {
        private static volatile JobsRepository _instance;
        private List<Job> _jobs = new List<Job>();
        private static object _syncRoot = new object();

        public static JobsRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new JobsRepository();
                        }
                    }
                }

                return _instance;
            }
        }

        public List<Job> Jobs
        {
            get
            {
                return _jobs;
            }
        }

        public JobsRepository()
        {

        }

        public void Cleanup()
        {
            _jobs.RemoveAll(HasExpired);
        }

        public void AddJob(Job j)
        {
            if (j != null)
            {
                _jobs.Add(j);
                this.Cleanup();
            }
        }

        private static bool HasExpired(Job job)
        {
            DateTime n = DateTime.Now.AddMinutes(-30);
            if (job.ItemCount <= 0 && job.Created < n)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}