using System.Collections;
using System.Xml;
using LitJson;
using SmartFoxClientAPI.Data;
using SmartFoxClientAPI.Util;

namespace SmartFoxClientAPI.Handlers
{
	public class ExtHandler : IMessageHandler
	{
		private SmartFoxClient sfs;

		public ExtHandler(SmartFoxClient sfs)
		{
			this.sfs = sfs;
		}

		public void HandleMessage(object msgObj, string type)
		{
			switch (type)
			{
			case "xml":
			{
				XmlDocument node = (XmlDocument)msgObj;
				XmlNode singleNode = XmlUtil.GetSingleNode(node, "/msg/.");
				string @string = XmlUtil.GetString(singleNode, "body/@action");
				if (@string == "xtRes")
				{
					string string2 = XmlUtil.GetString(singleNode, "body/node()");
					SFSObject value = SFSObjectSerializer.GetInstance().Deserialize(string2);
					Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
					hashtable.Add("dataObj", value);
					hashtable.Add("type", type);
					SFSEvent evt = new SFSEvent("OnExtensionResponse", hashtable);
					sfs.DispatchEvent(evt);
				}
				break;
			}
			case "json":
			{
				Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
				hashtable.Add("dataObj", ((JsonData)msgObj)["o"]);
				hashtable.Add("type", type);
				SFSEvent evt = new SFSEvent("OnExtensionResponse", hashtable);
				sfs.DispatchEvent(evt);
				break;
			}
			case "str":
			{
				Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
				hashtable.Add("dataObj", msgObj);
				hashtable.Add("type", type);
				SFSEvent evt = new SFSEvent("OnExtensionResponse", hashtable);
				sfs.DispatchEvent(evt);
				break;
			}
			}
		}
	}
}
