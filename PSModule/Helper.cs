﻿using PSModule.Models;
using PSModule.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using System.Xml;

namespace PSModule
{
    class Helper
    {
        public static List<ReportMetaData> readReportFromXMLFile(string reportPath)
        {
            List<ReportMetaData> listReport = new List<ReportMetaData>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(reportPath);

            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
            {
                foreach (XmlNode currentNode in node)
                {
                    ReportMetaData reportmetadata = new ReportMetaData();
                    XmlAttributeCollection attributesList = currentNode.Attributes;
                    foreach (XmlAttribute attribute in attributesList)
                    {
                        switch (attribute.Name)
                        {
                            case "name": reportmetadata.setDisplayName(attribute.Value); break;
                            case "status": reportmetadata.setStatus(attribute.Value); break;
                            default: break;
                        }
                    }

                    XmlNodeList nodes = currentNode.ChildNodes;
                    foreach (XmlNode xmlNode in nodes)
                    {
                        if (xmlNode.Name.Equals("system-out"))
                        {
                            reportmetadata.setDateTime(xmlNode.InnerText.Substring(0, 19));
                        }
                    }
                    listReport.Add(reportmetadata);
                }
            }

            return listReport;
        }

        public static int getErrorCode(List<ReportMetaData> listReport)
        {
            int errorCode = 0;
            int passedTests = 0;
            int failedTests = 0;

            foreach (ReportMetaData report in listReport)
            {
                if (report.getStatus().Equals("pass"))
                {
                    passedTests++;
                }
                if (report.getStatus().Equals("fail"))
                {
                    failedTests++;
                }
            }

            if (passedTests > 0 && failedTests > 0)
            {
                errorCode = -2;//job unstable
            }
            if (passedTests == 0 && failedTests > 0)
            {
                errorCode = -1;//job failed
            }

            return errorCode;

        }



        public static void createSummaryReport(string uftWorkingFolder, ref List<ReportMetaData> reportList,
                                               string uploadArtifact, string artifactType,
                                               string storageAccount, string container, 
                                               string reportName, string archiveName, string buildNumber, string runType)
        {
            HtmlTable table = new HtmlTable();
            HtmlTableRow header = new HtmlTableRow();
            HtmlTableCell h1 = new HtmlTableCell();
            h1.InnerText = "Test name";
            h1.Width = "100";
            h1.Align = "center";
            header.Cells.Add(h1);

            HtmlTableCell h2 = new HtmlTableCell();
            h2.InnerText = "Timestamp";
            h2.Width = "150";
            h2.Align = "center";
            header.Cells.Add(h2);

            HtmlTableCell h3 = new HtmlTableCell();
            h3.InnerText = "Status";
            h3.Width = "50";
            h3.Align = "center";
            header.Cells.Add(h3);

            if (runType.Equals(RunType.FileSystem.ToString()))
            {
                if (uploadArtifact.Equals("yes") && (artifactType.Equals(ArtifactType.onlyReport.ToString()) || artifactType.Equals(ArtifactType.bothReportArchive.ToString())))
                {
                    HtmlTableCell h4 = new HtmlTableCell();
                    h4.InnerText = "UFT report";
                    h4.Width = "100";
                    h4.Align = "center";
                    header.Cells.Add(h4);

                    if (artifactType.Equals(ArtifactType.bothReportArchive.ToString()))
                    {
                        HtmlTableCell h5 = new HtmlTableCell();
                        h5.InnerText = "UFT report archive";
                        h5.Width = "150";
                        h5.Align = "center";
                        header.Cells.Add(h5);
                    }
                }

                if (uploadArtifact.Equals("yes") && (artifactType.Equals(ArtifactType.onlyArchive.ToString())))
                {
                    HtmlTableCell h4 = new HtmlTableCell();
                    h4.InnerText = "UFT report archive";
                    h4.Width = "150";
                    h4.Align = "center";
                    header.Cells.Add(h4);
                }
            }

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);

            //create table content
            int index = 1;
            foreach (ReportMetaData report in reportList)
            {
                HtmlTableRow row = new HtmlTableRow();

                HtmlTableCell cell1 = new HtmlTableCell();
                cell1.InnerText = getTestName(report.getDisplayName());
                cell1.Align = "center";
                row.Cells.Add(cell1);

                HtmlTableCell cell2 = new HtmlTableCell();
                cell2.InnerText = report.getDateTime();
                cell2.Align = "center";
                row.Cells.Add(cell2);

                HtmlTableCell cell3 = new HtmlTableCell();
                HtmlImage statusImage = new HtmlImage();

                if (report.getStatus().Equals("pass"))
                {
                    statusImage.Src = "https://extensionado.blob.core.windows.net/uft-extension-images/passed.png";
                }
                else
                {
                    statusImage.Src = "https://extensionado.blob.core.windows.net/uft-extension-images/failed.png";
                }

                cell3.Align = "center";
                cell3.Controls.Add(statusImage);
                row.Cells.Add(cell3);

                if (runType.Equals(RunType.FileSystem.ToString()))
                {
                    if (uploadArtifact.Equals("yes") && (artifactType.Equals(ArtifactType.onlyReport.ToString()) || artifactType.Equals(ArtifactType.bothReportArchive.ToString())))
                    {

                        HtmlTableCell cell4 = new HtmlTableCell();
                        HtmlAnchor reportLink = new HtmlAnchor();

                        reportLink.HRef = "https://" + storageAccount + ".blob.core.windows.net/" + container + "/" + reportName + "_" + index + ".html";
                        reportLink.InnerText = "View report";

                        cell4.Controls.Add(reportLink);
                        cell4.Align = "center";
                        row.Cells.Add(cell4);


                        if (artifactType.Equals(ArtifactType.bothReportArchive.ToString()))
                        {
                            HtmlTableCell cell5 = new HtmlTableCell();
                            HtmlAnchor archiveLink = new HtmlAnchor();

                            archiveLink.HRef = "https://" + storageAccount + ".blob.core.windows.net/" + container + "/" + archiveName + "_" + index + ".zip";
                            archiveLink.InnerText = "Download";

                            cell5.Controls.Add(archiveLink);
                            cell5.Align = "center";
                            row.Cells.Add(cell5);
                        }
                    }

                    if (uploadArtifact.Equals("yes") && artifactType.Equals(ArtifactType.onlyArchive.ToString()))
                    {
                        HtmlTableCell cell4 = new HtmlTableCell();
                        HtmlAnchor archiveLink = new HtmlAnchor();

                        archiveLink.HRef = "https://" + storageAccount + ".blob.core.windows.net/" + container + "/" + archiveName + "_" + index + ".zip";
                        archiveLink.InnerText = "Download";

                        cell4.Controls.Add(archiveLink);
                        cell4.Align = "center";
                        row.Cells.Add(cell4);
                    }
                }

                table.Rows.Add(row);
                index++;
            }

            //add table to file
            string html;
            var reportMessage = new System.Text.StringBuilder();

            using (var sw = new StringWriter())
            {
                table.RenderControl(new System.Web.UI.HtmlTextWriter(sw));
                html = sw.ToString();
            }

            reportMessage.AppendFormat(html);

            System.IO.File.WriteAllText(uftWorkingFolder + @"\\res\\Report_" + buildNumber + "\\UFT Report", reportMessage.ToString());
            
        }

        private static string getTestName(string testPath)
        {
            int pos = testPath.LastIndexOf("\\", StringComparison.Ordinal) + 1;
            return testPath.Substring(pos, testPath.Length - pos);
        }

        private static string ImageToBase64(System.Drawing.Image _imagePath)
        {
            byte[] imageBytes = ImageToByteArray(_imagePath);
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }

        private static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            return ms.ToArray();
        }
    }
}
