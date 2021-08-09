using System;
using System.Xml;
using System.Xml.XPath;

namespace SmartFoxClientAPI.Util
{
	public class XmlUtil
	{
		private static bool DEBUG = false;

		public static string GetString(XmlNode node, string path)
		{
			try
			{
				XmlNode xmlNode = node.SelectSingleNode(path);
				if (xmlNode != null)
				{
					return node.SelectSingleNode(path).Value;
				}
				return "";
			}
			catch (XPathException ex)
			{
				if (DEBUG)
				{
					Console.WriteLine("XmlUtil string converter got XPathException exception accessing path: " + path + "\n\r" + ex.ToString());
				}
				return "";
			}
		}

		public static int GetInt(XmlNode node, string path)
		{
			try
			{
				XmlNode xmlNode = node.SelectSingleNode(path);
				if (xmlNode != null)
				{
					return int.Parse(node.SelectSingleNode(path).Value);
				}
				return -1;
			}
			catch (XPathException ex)
			{
				if (DEBUG)
				{
					Console.WriteLine("XmlUtil int converter got exception accessing path: " + path + "\n\r" + ex.ToString());
				}
				return -1;
			}
		}

		public static bool GetBool(XmlNode node, string path)
		{
			try
			{
				XmlNode xmlNode = node.SelectSingleNode(path);
				if (xmlNode != null)
				{
					return node.SelectSingleNode(path).Value == "1" || node.SelectSingleNode(path).Value.ToLower() == "true";
				}
				return false;
			}
			catch (XPathException ex)
			{
				if (DEBUG)
				{
					Console.WriteLine("XmlUtil boolean converter got exception accessing path: " + path + "\n\r" + ex.ToString());
				}
				return false;
			}
		}

		public static XmlNode GetSingleNode(XmlNode node, string path)
		{
			try
			{
				return node.SelectSingleNode(path);
			}
			catch (XPathException ex)
			{
				if (DEBUG)
				{
					Console.WriteLine("XmlUtil single node converter got exception accessing path: " + path + "\n\r" + ex.ToString());
				}
				return null;
			}
		}

		public static XmlNodeList GetNodeList(XmlNode node, string path)
		{
			try
			{
				return node.SelectNodes(path);
			}
			catch (XPathException ex)
			{
				if (DEBUG)
				{
					Console.WriteLine("XmlUtil node list converter got exception accessing path: " + path + "\n\r" + ex.ToString());
				}
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml("");
				return xmlDocument.ChildNodes;
			}
		}

		public static void Dump(XmlNode xnod, int level)
		{
			string text = new string(' ', level * 2);
			Console.WriteLine(text + xnod.Name + "(" + xnod.NodeType.ToString() + ": " + xnod.Value + ")");
			if (xnod.NodeType == XmlNodeType.Element)
			{
				XmlNamedNodeMap attributes = xnod.Attributes;
				for (int i = 0; i < attributes.Count; i++)
				{
					Console.WriteLine(text + " " + attributes.Item(i).Name + " = " + attributes.Item(i).Value);
				}
			}
			if (xnod.HasChildNodes)
			{
				for (XmlNode xmlNode = xnod.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
				{
					Dump(xmlNode, level + 1);
				}
			}
		}
	}
}
