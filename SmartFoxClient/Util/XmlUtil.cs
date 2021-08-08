using System;
using System.Xml;
using System.Xml.XPath;

namespace SmartFoxClientAPI.Util
{
    /**
     * <summary>XML utility class</summary>
     * 
     * <remarks>
     * <para><b>Version:</b><br/>
     * 1.0.0</para>
     * 
     * <para><b>Author:</b><br/>
     * Thomas Hentschel Lund<br/>
     * 			<a href="http://www.fullcontrol.dk">http://www.fullcontrol.dk</a><br/>
     * 			<a href="mailto:sfs-api@fullcontrol.dk">sfs-api@fullcontrol.dk</a><p/>
     * (c) 2008 gotoAndPlay()<br/>
     *          <a href="http://www.smartfoxserver.com">http://www.smartfoxserver.com</a><br/>
     * 			<a href="http://www.gotoandplay.it">http://www.gotoandplay.it</a><br/>
     * </para>
     * </remarks>
     */
    public class XmlUtil
    {

        private static Boolean DEBUG = false; // TODO - remove after beta period

        /**
         * <summary>
         * Convert given node and path to string
         * </summary>
         *
         * <param name="node">Node to convert</param>
         * <param name="path">Path to node</param>
         * 
         * <returns>Converted string</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public static string GetString(XmlNode node, string path)
        {
            try
            {
                XmlNode nodeAtPath = node.SelectSingleNode(path);
                if (nodeAtPath != null)
                {
                    return node.SelectSingleNode(path).Value;
                }
                else {
                    return "";
                }

            }
            catch (XPathException e)
            {
                if (DEBUG) {
                    Console.WriteLine("XmlUtil string converter got XPathException exception accessing path: " + path + "\n\r" + e.ToString());
                } 
                return "";
            }
        }

        /**
         * <summary>
         * Convert given node and path to int
         * </summary>
         *
         * <param name="node">Node to convert</param>
         * <param name="path">Path to node</param>
         * 
         * <returns>Converted int</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public static int GetInt(XmlNode node, string path)
        {
            try
            {
                XmlNode nodeAtPath = node.SelectSingleNode(path);
                if (nodeAtPath != null)
                {
                    return int.Parse(node.SelectSingleNode(path).Value);
                }
                else
                {
                    return -1;
                }
            }
            catch (XPathException e)
            {
                if (DEBUG)
                {
                    Console.WriteLine("XmlUtil int converter got exception accessing path: " + path + "\n\r" + e.ToString());
                }
                return -1;
            }
        }

        /**
         * <summary>
         * Convert given node and path to bool
         * </summary>
         *
         * <param name="node">Node to convert</param>
         * <param name="path">Path to node</param>
         * 
         * <returns>Converted string</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public static bool GetBool(XmlNode node, string path)
        {
            try
            {
                XmlNode nodeAtPath = node.SelectSingleNode(path);
                if (nodeAtPath != null)
                {
                    return (node.SelectSingleNode(path).Value == "1") || (node.SelectSingleNode(path).Value.ToLower() == "true");
                }
                else
                {
                    return false;
                }
            }
            catch (XPathException e)
            {
                if (DEBUG)
                {
                    Console.WriteLine("XmlUtil boolean converter got exception accessing path: " + path + "\n\r" + e.ToString());
                }
                return false;
            }
        }

        /**
         * <summary>
         * Return single node at given node and path
         * </summary>
         *
         * <param name="node">Node</param>
         * <param name="path">Path to node</param>
         * 
         * <returns>Node</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public static XmlNode GetSingleNode(XmlNode node, string path)
        {
            try
            {
                return node.SelectSingleNode(path);
            }
            catch (XPathException e)
            {
                if (DEBUG)
                {
                    Console.WriteLine("XmlUtil single node converter got exception accessing path: " + path + "\n\r" + e.ToString());
                }
                return null;
            }
        }

        /**
         * <summary>
         * Return list of nodes at given node and path
         * </summary>
         *
         * <param name="node">Node</param>
         * <param name="path">Path to node</param>
         * 
         * <returns>Node list</returns>
         * 
         * <remarks>
         * <b>Version:</b><br/>
         * SmartFoxServer Basic / Pro
         * </remarks>
         */
        public static XmlNodeList GetNodeList(XmlNode node, string path)
        {
            try
            {
                return node.SelectNodes(path);
            }
            catch (XPathException e)
            {
                if (DEBUG)
                {
                    Console.WriteLine("XmlUtil node list converter got exception accessing path: " + path + "\n\r" + e.ToString());
                }
                // Create empty nodelist
                XmlDocument emptyDoc = new XmlDocument();
                emptyDoc.LoadXml("");
                return emptyDoc.ChildNodes;
            }
        }

        // Debug methods

        /**
         * <summary>
         * Dump node and children
         * </summary>
         */
        public static void Dump(XmlNode xnod, int level)
        {
            XmlNode xnodWorking;
            string pad = new string(' ', level * 2);

            Console.WriteLine(pad + xnod.Name + "(" + xnod.NodeType.ToString() + ": " + xnod.Value + ")");

            if (xnod.NodeType == XmlNodeType.Element)
            {
                XmlNamedNodeMap mapAttributes = xnod.Attributes;
                for (int i = 0; i < mapAttributes.Count; i++)
                {
                    Console.WriteLine(pad + " " + mapAttributes.Item(i).Name + " = " + mapAttributes.Item(i).Value);
                }
            }

            if (xnod.HasChildNodes)
            {
                xnodWorking = xnod.FirstChild;
                while (xnodWorking != null)
                {
                    Dump(xnodWorking, level + 1);
                    xnodWorking = xnodWorking.NextSibling;
                }
            }
        }
    }

}
