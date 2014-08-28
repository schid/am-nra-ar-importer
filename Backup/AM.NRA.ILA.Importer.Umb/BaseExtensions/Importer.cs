using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml;

namespace AM.NRA.ILA.Importer.Umb.BaseExtensions
{
    public class Importer
    {
        public static string Hello()
        {
            return "Hello World";
        }

        public static string StartImport(string sourceFileName, string importType, string startDate, string endDate)
        {
            //XmlDocument doc = new XmlDocument();
            //XmlElement elemData = doc.CreateElement("data");

            //XmlElement elemSourceFileName = doc.CreateElement("sourceFile");
            //elemSourceFileName.InnerText = sourceFileName;
            //elemData.AppendChild(elemSourceFileName);

            //XmlElement elemImportType = doc.CreateElement("importType");
            //elemImportType.InnerText = importType;
            //elemData.AppendChild(elemImportType);

            //XmlElement elemStartDate = doc.CreateElement("startDate");
            //elemStartDate.InnerText = startDate;
            //elemData.AppendChild(elemStartDate);

            //XmlElement elemEndDate = doc.CreateElement("endDate");
            //elemEndDate.InnerText = endDate;
            //elemData.AppendChild(elemEndDate);

            //doc.AppendChild(elemData);

            if (!string.IsNullOrEmpty(sourceFileName))
            {
                string sourcePath = HttpContext.Current.Server.MapPath("/App_Data/" + sourceFileName);

                if (!string.IsNullOrEmpty(startDate))
                {
                    string[] startDateParts = startDate.Split('-');
                    DateTime startDateTime = new DateTime(int.Parse(startDateParts[0]), int.Parse(startDateParts[1]), int.Parse(startDateParts[2]));

                    if (!string.IsNullOrEmpty(endDate))
                    {
                        string[] endDateParts = endDate.Split('-');
                        DateTime endDateTime = new DateTime(int.Parse(endDateParts[0]), int.Parse(endDateParts[1]), int.Parse(endDateParts[2]));

                        if (!string.IsNullOrEmpty(importType))
                        {
                            Import.Importer oImporter = Import.Importer.Instance;
                            oImporter.FilePath = sourcePath;
                            oImporter.ImportType = importType;
                            oImporter.StartDate = startDateTime;
                            oImporter.EndDate = endDateTime;

                            Thread oThread = new Thread(new ThreadStart(oImporter.Start));
                            oThread.Start();
                            while (!oThread.IsAlive) ;
                            
                            return "success";
                        }
                        else
                        {
                            return "failure: import type";
                        }
                    }
                    else
                    {
                        return "failure: end date";
                    }
                }
                else
                {
                    return "failure: start date";
                }
            }
            else
            {
                return "failure: no source file found";
            }
        }
    }
}