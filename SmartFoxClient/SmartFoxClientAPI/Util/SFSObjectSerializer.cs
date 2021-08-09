using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using SmartFoxClientAPI.Data;

namespace SmartFoxClientAPI.Util
{
	public class SFSObjectSerializer
	{
		private static SFSObjectSerializer _instance;

		private static Hashtable asciiTable_e;

		private SFSObjectSerializer()
		{
			asciiTable_e = new Hashtable();
			asciiTable_e.Add('>', "&gt;");
			asciiTable_e.Add('<', "&lt;");
			asciiTable_e.Add('&', "&amp;");
			asciiTable_e.Add('\'', "&apos;");
			asciiTable_e.Add('"', "&quot;");
		}

		public static SFSObjectSerializer GetInstance()
		{
			if (_instance == null)
			{
				_instance = new SFSObjectSerializer();
			}
			return _instance;
		}

		public string Serialize(SFSObject ao)
		{
			StringBuilder stringBuilder = new StringBuilder();
			Obj2xml(ao, 0, "", stringBuilder);
			return stringBuilder.ToString();
		}

		public SFSObject Deserialize(string xmlData)
		{
			SFSObject sFSObject = new SFSObject();
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(new StringReader(xmlData));
				XmlNode firstChild = xmlDocument.FirstChild;
				Xml2obj(firstChild, sFSObject, 0);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Problems parsing XML: " + ex);
			}
			return sFSObject;
		}

		private void Xml2obj(XmlNode xmlNode, SFSObject ao, int depth)
		{
			XmlNodeList childNodes = xmlNode.ChildNodes;
			foreach (XmlNode item in childNodes)
			{
				switch (item.Name)
				{
				case "obj":
				{
					string value4 = item.Attributes["o"].Value;
					SFSObject sFSObject = new SFSObject();
					ao.Put(value4, sFSObject);
					Xml2obj(item, sFSObject, depth + 1);
					break;
				}
				case "var":
				{
					string value = item.Attributes["n"].Value;
					string value2 = item.Attributes["t"].Value;
					string text;
					try
					{
						text = ((value2 != "x") ? item.FirstChild.Value : null);
					}
					catch
					{
						text = "";
					}
					object value3 = null;
					switch (value2)
					{
					case "b":
						value3 = text == "1";
						break;
					case "s":
						value3 = text;
						break;
					case "n":
					{
						NumberFormatInfo numberFormat = new CultureInfo("en-US", useUserOverride: false).NumberFormat;
						value3 = double.Parse(text, numberFormat);
						break;
					}
					}
					ao.Put(value, value3);
					break;
				}
				}
			}
		}

		private void Obj2xml(SFSObject ao, int depth, string nodeName, StringBuilder xmlData)
		{
			if (depth == 0)
			{
				xmlData.Append("<dataObj>");
			}
			else
			{
				xmlData.Append("<obj o='").Append(nodeName).Append("' t='a'>");
			}
			ICollection collection = ao.Keys();
			foreach (object item in collection)
			{
				string text = item.ToString();
				object obj = ao.Get(text);
				if (obj == null)
				{
					xmlData.Append("<var n='").Append(text).Append("' t='x' />");
				}
				else if (obj is SFSObject)
				{
					Obj2xml((SFSObject)obj, depth + 1, text, xmlData);
					xmlData.Append("</obj>");
				}
				else if (obj is bool)
				{
					bool flag = (bool)obj;
					xmlData.Append("<var n='").Append(text).Append("' t='b'>")
						.Append(flag ? "1" : "0")
						.Append("</var>");
				}
				else if (obj is float)
				{
					float num = (float)obj;
					xmlData.Append("<var n='").Append(text).Append("' t='n'>")
						.Append(num.ToString(CultureInfo.CreateSpecificCulture("en-US")))
						.Append("</var>");
				}
				else if (obj is double)
				{
					double num2 = (double)obj;
					xmlData.Append("<var n='").Append(text).Append("' t='n'>")
						.Append(num2.ToString(CultureInfo.CreateSpecificCulture("en-US")))
						.Append("</var>");
				}
				else if (obj is int)
				{
					xmlData.Append("<var n='").Append(text).Append("' t='n'>")
						.Append(obj.ToString())
						.Append("</var>");
				}
				else if (obj is string)
				{
					xmlData.Append("<var n='").Append(text).Append("' t='s'>")
						.Append(EncodeEntities((string)obj))
						.Append("</var>");
				}
			}
			if (depth == 0)
			{
				xmlData.Append("</dataObj>");
			}
		}

		private static string EncodeEntities(string in_str)
		{
			char[] array = in_str.ToCharArray();
			string text = "";
			foreach (char c in array)
			{
				text = ((!asciiTable_e.ContainsKey(c)) ? (text + c) : (text + asciiTable_e[c]));
			}
			return text;
		}
	}
}
