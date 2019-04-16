// (C) Copyright 2010 by Autodesk, Inc. 
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to 
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

using System;
using System.IO;
using System.Xml;

namespace KojtoCAD.Plotter
{
    class XmlUtils
    {
        /// <summary>
        ///  Element names for our XML publishing status file
        /// </summary>
        const string batchPublishName = "BatchPublish";
        const string userDataName = "UserData";
        const string dwgDirName = "DwgDirectory";
        const string dwfDirName = "DwfDirectory";
        const string pdfDirName = "PdfDirectory";
        const string pubDwfName = "PublishDwf";
        const string pubPdfName = "PublishPdf";
        const string dwfLogName = "DwfLogFile";
        const string pdfLogName = "PdfLogFile";
        const string fdName = "FileDia";
        const string bpName = "BgPublish";
        const string rdName = "RecoverDia";
        const string timeoutName = "Timeout";
        const string retryName = "Retry";
        const string modOnlyName = "ModifiedOnly";

        const string trueValue = "True";
        const string falseValue = "False";

        const string pubInfoName = "PublishInfo";
        const string pubStatusName = "Published";
        const string idxName = "Index";
        const string dwgFileName = "DrawingName";
        const string failedStatusName = "Failed";
        const string canRetryName = "CanRetry";
        const string skipDwgName = "SkipDWG";
        const string layInfoName = "LayoutInfo";
        const string layNameName = "Name";
        const string layPubName = "Publish";

        const string dwfTimeName = "DwfTime";
        const string pdfTimeName = "PdfTime";

        const string xmlFile = "PublishInfo.xml";

        static private string _xmlName = "";
        static private string _xmlCopy = "";

        internal static string FileName
        {
            set { _xmlName = value; }
            get { return _xmlName; }
        }

        internal static string FileCopy
        {
            set { _xmlCopy = value; }
            get { return _xmlCopy; }
        }

        /// <summary>
        /// Build and save an XML file containing publishing information for our various files
        /// </summary>
        /// <param name="dwgDir"></param>
        /// <param name="dwfDir"></param>
        /// <param name="pdfDir"></param>
        /// <param name="pubDwf"></param>
        /// <param name="pubPdf"></param>
        /// <param name="retry"></param>
        /// <param name="dwfLog"></param>
        /// <param name="pdfLog"></param>
        /// <param name="timeout"></param>
        /// <param name="modOnly"></param>
        /// <param name="fd"></param>
        /// <param name="bp"></param>
        /// <param name="rd"></param>
        internal static void BuildXml(
          string dwgDir,
          string dwfDir,
          string pdfDir,
          bool pubDwf,
          bool pubPdf,
          bool retry,
          string dwfLog,
          string pdfLog,
          string timeout,
          bool modOnly,
          object fd,
          object bp,
          object rd
        )
        {
            XmlDocument xd = new XmlDocument();

            // Create the root element

            XmlElement root = xd.CreateElement(batchPublishName);
            xd.AppendChild(root);

            // Put userdata

            XmlElement ud = xd.CreateElement(userDataName);
            root.AppendChild(ud);

            // Start storing user inputs

            XmlElement srcDirElem = xd.CreateElement(dwgDirName);
            XmlText text = xd.CreateTextNode(dwgDir);
            srcDirElem.AppendChild(text);
            ud.AppendChild(srcDirElem);

            XmlElement dwfOutElem = xd.CreateElement(dwfDirName);
            text = xd.CreateTextNode(dwfDir);
            dwfOutElem.AppendChild(text);
            ud.AppendChild(dwfOutElem);

            XmlElement pdfOutElem = xd.CreateElement(pdfDirName);
            text = xd.CreateTextNode(pdfDir);
            pdfOutElem.AppendChild(text);
            ud.AppendChild(pdfOutElem);

            XmlElement pubDwfElem = xd.CreateElement(pubDwfName);
            text = xd.CreateTextNode(GetValue(pubDwf));

            pubDwfElem.AppendChild(text);
            ud.AppendChild(pubDwfElem);

            XmlElement pubPdfElem = xd.CreateElement(pubPdfName);
            text = xd.CreateTextNode(GetValue(pubPdf));
            pubPdfElem.AppendChild(text);
            ud.AppendChild(pubPdfElem);

            XmlElement dwfLogElem = xd.CreateElement(dwfLogName);
            text = xd.CreateTextNode(dwfLog);
            dwfLogElem.AppendChild(text);
            ud.AppendChild(dwfLogElem);

            XmlElement pdfLogElem = xd.CreateElement(pdfLogName);
            text = xd.CreateTextNode(pdfLog);
            pdfLogElem.AppendChild(text);
            ud.AppendChild(pdfLogElem);

            XmlElement timeoutElem = xd.CreateElement(timeoutName);
            text = xd.CreateTextNode(timeout);
            timeoutElem.AppendChild(text);
            ud.AppendChild(timeoutElem);

            XmlElement fdElem = xd.CreateElement(fdName);
            text = xd.CreateTextNode(fd.ToString());
            fdElem.AppendChild(text);
            ud.AppendChild(fdElem);

            XmlElement bpElem = xd.CreateElement(bpName);
            text = xd.CreateTextNode(bp.ToString());
            bpElem.AppendChild(text);
            ud.AppendChild(bpElem);

            if (rd != null)
            {
                XmlElement rdElem = xd.CreateElement(rdName);
                text = xd.CreateTextNode(rd.ToString());
                rdElem.AppendChild(text);
                ud.AppendChild(rdElem);
            }

            XmlElement retryElem = xd.CreateElement(retryName);
            text = xd.CreateTextNode(GetValue(retry));
            retryElem.AppendChild(text);
            ud.AppendChild(retryElem);

            XmlElement mdElem = xd.CreateElement(modOnlyName);
            text = xd.CreateTextNode(GetValue(modOnly));
            mdElem.AppendChild(text);
            ud.AppendChild(mdElem);

            // Write the publishInfoList

            foreach (PublishInfo info in KojtoCAD.Plotter.Plotter.PubInfos)
            {
                AddPublishInfoToXml(info, xd);
            }

            _xmlName =
              (pubDwf ? dwfDir : pdfDir) + "\\" + xmlFile;

            xd.Save(_xmlName);
        }

        /// <summary>
        /// Read the publishing information for our various files from the XML on disk
        /// </summary>
        /// <param name="dwgDir"></param>
        /// <param name="dwfDir"></param>
        /// <param name="pdfDir"></param>
        /// <param name="pubDWF"></param>
        /// <param name="pubPDF"></param>
        /// <param name="retry"></param>
        /// <param name="dwfLog"></param>
        /// <param name="pdfLog"></param>
        /// <param name="timeout"></param>
        /// <param name="modOnly"></param>
        /// <param name="fd"></param>
        /// <param name="bp"></param>
        /// <param name="rd"></param>
        internal static void ReadUserInputFromXml(
          ref string dwgDir,
          ref string dwfDir,
          ref string pdfDir,
          ref bool pubDWF,
          ref bool pubPDF,
          ref bool retry,
          ref string dwfLog,
          ref string pdfLog,
          ref string timeout,
          ref bool modOnly,
          ref object fd,
          ref object bp,
          ref object rd
        )
        {
            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(_xmlName);

                XmlNodeList udList =
                  xd.GetElementsByTagName(userDataName);

                // Should contain a single node...

                XmlNode ud = udList[0];

                foreach (XmlNode node in ud.ChildNodes)
                {
                    if (Same(node.Name, dwgDirName))
                    {
                        dwgDir = node.InnerText;
                    }
                    else if (Same(node.Name, dwfDirName))
                    {
                        dwfDir = node.InnerText;
                    }
                    else if (Same(node.Name, pdfDirName))
                    {
                        pdfDir = node.InnerText;
                    }
                    else if (Same(node.Name, pubDwfName))
                    {
                        pubDWF = Same(node.InnerText, trueValue);
                    }
                    else if (Same(node.Name, pubPdfName))
                    {
                        pubPDF = Same(node.InnerText, trueValue);
                    }
                    else if (Same(node.Name, dwfLogName))
                    {
                        dwfLog = node.InnerText;
                    }
                    else if (Same(node.Name, pdfLogName))
                    {
                        pdfLog = node.InnerText;
                    }
                    else if (Same(node.Name, fdName))
                    {
                        fd = Convert.ToInt16(node.InnerText);
                    }
                    else if (Same(node.Name, bpName))
                    {
                        bp = Convert.ToInt16(node.InnerText);
                    }
                    else if (Same(node.Name, rdName))
                    {
                        rd = Convert.ToInt16(node.InnerText);
                    }
                    else if (Same(node.Name, timeoutName))
                    {
                        timeout = node.InnerText;
                    }
                    else if (Same(node.Name, retryName))
                    {
                        retry = Same(node.InnerText, trueValue);
                    }
                    else if (Same(node.Name, modOnlyName))
                    {
                        modOnly = Same(node.InnerText, trueValue);
                    }
                }
                xd.Save(_xmlName);
            }
            catch { }
        }

        // Open the copy of our XML file, update the Timeout value
        // and save it back

        internal static void UpdateTimeoutInXml(string timeout)
        {
            try
            {
                // Load our backup copy of the XML to work on

                XmlDocument xd = new XmlDocument();
                xd.Load(_xmlCopy);

                // Look for the failed documents to retry

                XmlNode root = xd.DocumentElement;
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Contains(pubInfoName))
                    {
                        // Check whether it has failed

                        XmlNode pin = node.ChildNodes[2];
                        if (Same(pin.InnerText, trueValue))
                        {
                            // Now check whether we can retry

                            XmlNode retry = node.ChildNodes[3];
                            if (Same(retry.InnerText, trueValue))
                            {
                                // Now we mark the "published" flag as false

                                foreach (XmlNode publish in node.ChildNodes)
                                {
                                    if (Same(publish.Name, pubStatusName))
                                    {
                                        publish.InnerText = falseValue;
                                    }
                                }
                            }
                        }
                    }
                }

                // Get our UserData section of the XML file

                XmlNodeList nodeList = xd.GetElementsByTagName(userDataName);
                XmlNode infoNode = nodeList[0];
                foreach (XmlNode node in infoNode.ChildNodes)
                {
                    // We want to set "retry" to true and update the "timeout"

                    if (Same(node.Name, retryName))
                    {
                        node.InnerText = trueValue;
                    }

                    if (Same(node.Name, timeoutName))
                    {
                        node.InnerText = timeout;
                    }
                }

                // And we save to the main XML location

                xd.Save(_xmlName);
            }
            catch { }
        }

        internal static void UpdatePublishStatus(PublishInfo info, string dateTime, string text)
        {
            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(_xmlName);

                string xpath = "descendant::PublishInfo[DrawingName=" +
                        "'" + info.DwgName + "']";

                XmlNodeList listNode = xd.SelectNodes(xpath);

                if (listNode.Count == 0)
                {
                    xd.Save(_xmlName);
                    return;
                }

                XmlNode main = listNode[0];

                if (main == null)
                {
                    xd.Save(_xmlName);
                    return;
                }

                foreach (XmlNode node in main.ChildNodes)
                {
                    if (Same(text, pubStatusName))
                    {
                        if (Same(node.Name, pubStatusName))
                        {
                            node.InnerText = trueValue;
                        }
                        else if (Same(node.Name, idxName))
                        {
                            node.InnerText = info.Index.ToString();
                        }
                        else if (Same(node.Name, dwfTimeName))
                        {
                            node.InnerText = dateTime;
                        }
                        else if (Same(node.Name, pdfTimeName))
                        {
                            node.InnerText = dateTime;
                        }
                    }
                    else if (Same(text, failedStatusName))
                    {
                        if (Same(node.Name, failedStatusName))
                        {
                            node.InnerText = GetValue(info.Failed);
                        }
                    }
                }
                xd.Save(_xmlName);
            }
            catch { }
        }

        /// <summary>
        /// Add information regarding the publishing to our XML
        /// </summary>
        /// <param name="info"></param>
        /// <param name="xd"></param>
        static void AddPublishInfoToXml(PublishInfo info, XmlDocument xd)
        {
            try
            {
                XmlElement root = xd.DocumentElement;

                XmlElement elem =
                  xd.CreateElement(
                    pubInfoName
                  );
                root.AppendChild(elem);

                XmlElement idx = xd.CreateElement(idxName);
                XmlText text = xd.CreateTextNode(info.Index.ToString());
                idx.AppendChild(text);
                elem.AppendChild(idx);

                XmlElement dwgName = xd.CreateElement(dwgFileName);
                text = xd.CreateTextNode(info.DwgName);
                dwgName.AppendChild(text);
                elem.AppendChild(dwgName);

                XmlElement dwgFailed = xd.CreateElement(failedStatusName);
                text = xd.CreateTextNode(GetValue(info.Failed));
                dwgFailed.AppendChild(text);
                elem.AppendChild(dwgFailed);

                XmlElement retry = xd.CreateElement(canRetryName);
                text = xd.CreateTextNode(GetValue(info.CanRetry));
                retry.AppendChild(text);
                elem.AppendChild(retry);

                XmlElement dwgSkip = xd.CreateElement(skipDwgName);
                text = xd.CreateTextNode(GetValue(info.SkipDwg));
                dwgSkip.AppendChild(text);
                elem.AppendChild(dwgSkip);

                foreach (LayoutInfo LayoutInfo in info.LayoutInfos)
                {
                    // Add each layer info

                    XmlElement layInfo = xd.CreateElement(layInfoName);
                    elem.AppendChild(layInfo);

                    // Add the data

                    XmlElement layName = xd.CreateElement(layNameName);
                    text = xd.CreateTextNode(LayoutInfo.Layout);
                    layName.AppendChild(text);
                    layInfo.AppendChild(layName);

                    XmlElement layPub = xd.CreateElement(layPubName);
                    text = xd.CreateTextNode(GetValue(LayoutInfo.Publish));
                    layPub.AppendChild(text);

                    layInfo.AppendChild(layPub);
                }

                // Add the publish status...

                XmlElement pubStat = xd.CreateElement(pubStatusName);
                text = xd.CreateTextNode(GetValue(info.Published));
                pubStat.AppendChild(text);
                elem.AppendChild(pubStat);

                // Add the time if published...

                XmlElement dwfTime = xd.CreateElement(dwfTimeName);
                text = xd.CreateTextNode("");
                dwfTime.AppendChild(text);
                elem.AppendChild(dwfTime);

                XmlElement pdfTime = xd.CreateElement(pdfTimeName);
                text = xd.CreateTextNode("");
                pdfTime.AppendChild(text);
                elem.AppendChild(pdfTime);
            }
            catch { }
        }

        /// <summary>
        /// Pull out publishing information from our XML
        /// </summary>
        /// <param name="retry"></param>
        internal static void UpdatePublishInfoListFromXml(bool retry)
        {
            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(_xmlName);

                bool checkForFailed = true;
                PublishInfo prvInfo = null;

                XmlNode root = xd.DocumentElement;
                XmlNode prvNode = null;

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Contains(pubInfoName))
                    {
                        PublishInfo pi = new PublishInfo();
                        KojtoCAD.Plotter.Plotter.PubInfos.Add(pi);

                        foreach (XmlNode pin in node.ChildNodes)
                        {
                            if (Same(pin.Name, idxName))
                            {
                                pi.Index = Convert.ToInt32(pin.InnerText);
                            }
                            else if (Same(pin.Name, dwgFileName))
                            {
                                pi.DwgName = pin.InnerText;
                            }
                            else if (Same(pin.Name, failedStatusName))
                            {
                                pi.Failed =
                                  Same(pin.InnerText, trueValue);
                            }
                            else if (Same(pin.Name, skipDwgName))
                            {
                                pi.SkipDwg =
                                  Same(pin.InnerText, trueValue);
                            }
                            else if (Same(pin.Name, canRetryName))
                            {
                                pi.CanRetry =
                                  Same(pin.InnerText, trueValue);
                            }
                            else if (Same(pin.Name, layInfoName))
                            {
                                LayoutInfo li = new LayoutInfo();
                                pi.LayoutInfos.Add(li);
                                foreach (XmlNode lin in pin.ChildNodes)
                                {
                                    if (Same(lin.Name, layNameName))
                                    {
                                        li.Layout = lin.InnerText;
                                    }
                                    else if (Same(lin.Name, layPubName))
                                    {
                                        li.Publish =
                      Same(lin.InnerText, trueValue);
                                    }
                                }
                            }
                            else if (Same(pin.Name, pubStatusName))
                            {
                                pi.Published =
                                  Same(pin.InnerText, trueValue);

                                if (!retry)
                                {
                                    if (!pi.Published)
                                    {
                                        if (checkForFailed == true && prvInfo != null)
                                        {
                                            prvInfo.Failed = true;
                                            checkForFailed = false;

                                            // Failed status is stored as 3rd node...

                                            XmlNode failedNode = prvNode.ChildNodes.Item(2);
                                            failedNode.InnerText = trueValue;
                                        }
                                    }
                                }
                            }
                            else if (Same(pin.Name, dwfTimeName))
                            {
                                pi.DwfPubTime = pin.InnerText;
                            }
                            else if (Same(pin.Name, pdfTimeName))
                            {
                                pi.PdfPubTime = pin.InnerText;
                            }
                        }
                        prvInfo = pi;
                        prvNode = node;
                    }
                }
                xd.Save(_xmlName);
            }
            catch { }
        }

        /// <summary>
        /// Return the appropriate string for a Boolean
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        internal static string GetValue(bool test)
        {
            return (test ? trueValue : falseValue);
        }

        /// <summary>
        /// Make a backup copy of our main XML file
        /// </summary>
        internal static void BackupXml()
        {
            DeleteFile(FileCopy);
            File.Copy(FileName, FileCopy);
        }

        /// <summary>
        /// Delete our main XML file
        /// </summary>
        internal static void DeleteXml()
        {
            DeleteFile(XmlUtils.FileName);
        }

        /// <summary>
        /// Delete our backup copy
        /// </summary>
        internal static void DeleteCopy()
        {
            DeleteFile(XmlUtils.FileCopy);
        }

        internal static void DeleteFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        internal static bool Same(string first, string second)
        {
            return (string.Compare(first, second, true) == 0);
        }
    }
}
