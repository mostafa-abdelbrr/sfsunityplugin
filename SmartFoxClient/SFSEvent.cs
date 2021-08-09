using System.Collections;

namespace SmartFoxClientAPI
{
	public class SFSEvent
	{
		private string type;

		private Hashtable parameters;

		public SFSEvent(string type, Hashtable parameters)
		{
			this.type = type;
			this.parameters = parameters;
		}

		public new string GetType()
		{
			return type;
		}

		public object GetParameter(string key)
		{
			return parameters[key];
		}
	}
}
