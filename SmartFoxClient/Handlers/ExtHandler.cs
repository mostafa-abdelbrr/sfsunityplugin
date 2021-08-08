using System.Collections;
using System.Xml;
using LitJson;
using SmartFoxClientAPI.Util;
using SmartFoxClientAPI.Data;

namespace SmartFoxClientAPI.Handlers {
    /**
     * <summary>Extension handler class.</summary>
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
    public class ExtHandler : IMessageHandler
	{
		SmartFoxClient sfs;

        /**
         * <summary>
         * ExtHandler constructor.
         * </summary>
         * 
         * <param name="sfs">the smart fox client</param>
         */
        public ExtHandler(SmartFoxClient sfs)
		{
			this.sfs = sfs;
		}

        /**
         * <summary>
         * Handle messages
         * </summary>
         * 
         * <param name="msgObj">the message object to handle</param>
         * <param name="type">type of message</param>
         */
        public void HandleMessage(object msgObj, string type)
		{
			Hashtable parameters;
			SFSEvent evt;
			
			if (type == SmartFoxClient.XTMSG_TYPE_XML)
			{
                XmlDocument xmlDoc = (XmlDocument)msgObj;
                XmlNode xml = XmlUtil.GetSingleNode(xmlDoc, "/msg/.");
                string action = XmlUtil.GetString(xml, "body/@action");

				if (action == "xtRes")
				{
                    string xmlStr = XmlUtil.GetString(xml, "body/node()");
					SFSObject asObj = SFSObjectSerializer.GetInstance().Deserialize(xmlStr);

					// Fire event!
					parameters = Hashtable.Synchronized(new Hashtable());
					parameters.Add("dataObj", asObj);
					parameters.Add("type", type);
					
					evt = new SFSEvent(SFSEvent.onExtensionResponseEvent, parameters);
					sfs.DispatchEvent(evt);
				}
			}

			else if (type == SmartFoxClient.XTMSG_TYPE_JSON)
			{
				// Fire event!
				parameters = Hashtable.Synchronized(new Hashtable());
				parameters.Add("dataObj", ((JsonData)msgObj)["o"]);
				parameters.Add("type", type);

				evt = new SFSEvent(SFSEvent.onExtensionResponseEvent, parameters);
				sfs.DispatchEvent(evt);
			}
			
			else if (type == SmartFoxClient.XTMSG_TYPE_STR)
			{
				// Fire event!
				parameters = Hashtable.Synchronized(new Hashtable());
				parameters.Add("dataObj", msgObj);
				parameters.Add("type", type);

				evt = new SFSEvent(SFSEvent.onExtensionResponseEvent, parameters);
                sfs.DispatchEvent(evt);
			}
		}
	}
}