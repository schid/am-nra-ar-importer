using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace AM.NRA.ILA.Importer.Umb.Import
{
    public sealed class Importer
    {
        private static volatile Importer instance;
        private static object syncRoot = new object();
        private XmlDocument doc = new XmlDocument();

        public string FilePath { get; set; }
        public string ImportType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Importer()
        {

        }

        public static Importer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new Importer();
                        }
                    }
                }

                return instance;
            }
        }

        public void Start()
        {
            if (!string.IsNullOrEmpty(this.FilePath)
                && !string.IsNullOrEmpty(this.ImportType)
                && this.StartDate != null
                && this.EndDate != null)
            {
                doc.Load(this.FilePath);
                string x = "hello";
            }
        }
    }
}