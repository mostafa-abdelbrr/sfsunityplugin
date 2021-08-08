using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.IO;
using System.Globalization;
using SmartFoxClientAPI.Data;

namespace SmartFoxClientAPI.Util
{
    /** 
     * <summary>SFS Object Serializer and Deserializer Class.</summary>
     * 
     * <remarks>
     * <para><b>Version:</b><br/>
     * 1.0.1</para>
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
    public class SFSObjectSerializer
    {
        private static SFSObjectSerializer _instance;
        private static Hashtable asciiTable_e;	// ascii code table for encoding	
        
        private SFSObjectSerializer()
        {
            asciiTable_e = new Hashtable();

            asciiTable_e.Add('>', "&gt;");
            asciiTable_e.Add('<', "&lt;");
			asciiTable_e.Add('&', "&amp;");
            asciiTable_e.Add('\'', "&apos;");
            asciiTable_e.Add('"', "&quot;");
        }

        /**
         * <summary>
         * Get instance of this serializer
         * </summary>
         * 
         * <returns>Singleton instance of this serializer</returns>
         */
        public static SFSObjectSerializer GetInstance()
        {
            if (_instance == null)
                _instance = new SFSObjectSerializer();

            return _instance;
        }

        /**
         * <summary>
         * Serialize given object
         * </summary>
         * 
         * <param name="ao">Object to serialize</param>
         * 
         * <returns>Serialized object</returns>
         */
        public string Serialize(SFSObject ao)
        {
            StringBuilder xmlData = new StringBuilder();
            Obj2xml(ao, 0, "", xmlData);

            return xmlData.ToString();
        }

        /**
         * <summary>
         * Deserialize given string to object
         * </summary>
         * 
         * <param name="xmlData">String to deserialize</param>
         * 
         * <returns>Deserialized object</returns>
         */
        public SFSObject Deserialize(string xmlData)
        {
            SFSObject ao = new SFSObject();

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(new StringReader(xmlData));

                XmlNode root = xmlDoc.FirstChild;

                Xml2obj(root, ao, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Problems parsing XML: " + e);
            }
            return ao;
        }

        private void Xml2obj(XmlNode xmlNode, SFSObject ao, int depth)
        {
            XmlNodeList subNodes = xmlNode.ChildNodes;

            foreach (XmlNode subNode in subNodes)
            {
                switch (subNode.Name)
                {
                    case "obj":
                        string objName = subNode.Attributes["o"].Value;

                        SFSObject subASObj = new SFSObject();
                        ao.Put(objName, subASObj);
                        Xml2obj(subNode, subASObj, depth + 1);

                        break;

                    case "var":
                        string name = subNode.Attributes["n"].Value;
                        string type = subNode.Attributes["t"].Value;
						string val;
						try {
							val = type != "x" ? subNode.FirstChild.Value : null;
						} catch {
							val = "";
						} 

                        object varValue = null;

                        //--- bool ----------------------------------------------------------------
                        if (type == "b")
                            varValue = val == "1";

                        //--- string ------------------------------------------------------------------
                        else if (type == "s")
                            varValue = val;

                        //--- Number ------------------------------------------------------------------
                        else if (type == "n") {
                            // This is a workaround for a mono bug where mono doesnt automatically changes
                            // , and . as separator depending on running on OSX or Windows.
                            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
                       	    varValue = double.Parse(val, nfi);
                        }

                        // Add as string key
                        ao.Put(name, varValue);

                        break;

                }
            }
        }

        private void Obj2xml(SFSObject ao, int depth, string nodeName, StringBuilder xmlData)
        {
            if (depth == 0)
                xmlData.Append("<dataObj>");
            else
                xmlData.Append("<obj o='").Append(nodeName).Append("' t='a'>");

            ICollection keys = ao.Keys();

            foreach (object k in keys)
            {
                string key = k.ToString();
                object o = ao.Get(key);

                //--- Handle Nulls -----------------------------------------
                if (o == null)
                    xmlData.Append("<var n='").Append(key).Append("' t='x' />");

                else if (o is SFSObject)
                {
                    // Scan the object recursively			
                    Obj2xml((SFSObject)o, depth + 1, key, xmlData);

                    // When you get back to this level, close the 
                    xmlData.Append("</obj>");
                }

                else if (o is bool)
                {
                    bool b = (bool)o;
                    xmlData.Append("<var n='").Append(key).Append("' t='b'>").Append((b ? "1" : "0")).Append("</var>");

                }

				//--- Handle strings and Numbers ---------------------------------------
				else if ( o is float ) {
					float val = (float)o;
					xmlData.Append("<var n='").Append(key).Append("' t='n'>").Append(val.ToString(CultureInfo.CreateSpecificCulture("en-US"))).Append("</var>");
				} else if ( o is double ) {
					double val = (double)o;
					xmlData.Append("<var n='").Append(key).Append("' t='n'>").Append(val.ToString(CultureInfo.CreateSpecificCulture("en-US"))).Append("</var>");
				} else if ( o is int ) {
					xmlData.Append("<var n='").Append(key).Append("' t='n'>").Append(o.ToString()).Append("</var>");
				}



                //--- Handle strings ---------------------------------------
                else if (o is string)
                {
                    xmlData.Append("<var n='").Append(key).Append("' t='s'>").Append(EncodeEntities((string)o)).Append("</var>");
                }
            }

            // If we're back to root node then close it!
            if (depth == 0)
                xmlData.Append("</dataObj>");
        }

        private static string EncodeEntities(string in_str)
        {
			char[] in_chars = in_str.ToCharArray();
            string out_str = "";
            int i = 0;
           
            while (i < in_chars.Length)
            {
                char c = in_chars[i];

				if (asciiTable_e.ContainsKey(c)) {
					out_str+=asciiTable_e[c];
				}
				else {
					out_str+=c;
				}
                i++;
            }

            return out_str;
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        // Debugging 
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

/*        private static string Echo(string s, int n)
        {
            return EchoTabs(n) + s;
        }

        private static string EchoTabs(int n)
        {
            string s = "";

            for (int i = 0; i < n; i++)
                s += "\t";

            return s;
        }

        private static void DumpASObject(SFSObject ao)
        {
            LinkedList<string> lines = new LinkedList<string>();
            Dump(ao, lines, 0);

            foreach (string line in lines)
                Console.WriteLine(line);
        }

        private static void Dump(SFSObject ao, LinkedList<string> lines, int index)
        {
            lines.AddFirst(Echo("", 0));
            ICollection keys = ao.Keys();
            string output = null;

            foreach (object key in keys)
            {
                object item = ao.Get(key);

                if (item == null)
                    output = "Null";

                else if (item is SFSObject)
                {
                    output = "Object: ";
                    Dump((SFSObject)item, lines, index + 1);
                }

                else if (item is double || item is float || item is int)
                    output = "Number: " + item;

                else if (item is string)
                    output = "string: " + item;

                else if (item is bool)
                    output = "Bool: " + item;

                else
                    output = "Unknown!";

                lines.AddFirst(Echo("[ " + key + " ] > " + output, index));
            }
        }


        private string PrettyPrint(string XML)
        {
            string Result = "";

            MemoryStream MS = new MemoryStream();
            XmlTextWriter W = new XmlTextWriter(MS, Encoding.Unicode);
            XmlDocument D = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                D.LoadXml(XML);

                W.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                D.WriteContentTo(W);
                W.Flush();
                MS.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                MS.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader SR = new StreamReader(MS);

                // Extract the text from the StreamReader.
                string FormattedXML = SR.ReadToEnd();

                Result = FormattedXML;
            }
            catch (XmlException)
            {
                // should show some feedback...
            }

            MS.Close();
            W.Close();

            return Result;
        }
*/
	}
}
