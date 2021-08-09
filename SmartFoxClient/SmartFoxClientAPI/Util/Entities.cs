using System.Collections;
using System.Globalization;

namespace SmartFoxClientAPI.Util
{
	public class Entities
	{
		private static readonly Hashtable ascTab;

		private static readonly Hashtable ascTabRev;

		static Entities()
		{
			ascTab = new Hashtable();
			ascTabRev = new Hashtable();
			ascTab.Add(">", "&gt;");
			ascTab.Add("<", "&lt;");
			ascTab.Add("&", "&amp;");
			ascTab.Add("'", "&apos;");
			ascTab.Add("\"", "&quot;");
			ascTabRev.Add("&gt;", ">");
			ascTabRev.Add("&lt;", "<");
			ascTabRev.Add("&amp;", "&");
			ascTabRev.Add("&apos;", "'");
			ascTabRev.Add("&quot;", "\"");
		}

		private Entities()
		{
		}

		public static string EncodeEntities(string st)
		{
			string text = "";
			for (int i = 0; i < st.Length; i++)
			{
				char c = st.Substring(i, 1).ToCharArray()[0];
				int num = c;
				text = ((num != 9 && num != 10 && num != 13) ? ((num < 32 || num > 126) ? (text + c) : ((ascTab[c] == null) ? (text + c) : (text + ascTab[c]))) : (text + c));
			}
			return text;
		}

		public static string DecodeEntities(string st)
		{
			int i = 0;
			string text = "";
			for (; i < st.Length; i++)
			{
				string text2 = st.Substring(i, 1);
				if (text2 == "&")
				{
					string text3 = text2;
					string text4;
					do
					{
						i++;
						text4 = st.Substring(i, 1);
						text3 += text4;
					}
					while (text4 != ";" && i < st.Length);
					string text5 = (string)ascTabRev[text3];
					text = ((text5 == null) ? (text + (char)GetCharCode(text3)) : (text + text5));
				}
				else
				{
					text += text2;
				}
			}
			return text;
		}

		private static int GetCharCode(string ent)
		{
			string text = ent.Substring(3, ent.Length);
			text = text.Substring(0, text.Length - 1);
			return int.Parse(text, NumberStyles.AllowHexSpecifier);
		}
	}
}
