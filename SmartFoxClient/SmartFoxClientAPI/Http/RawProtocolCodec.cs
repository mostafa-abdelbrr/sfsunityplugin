namespace SmartFoxClientAPI.Http
{
	public class RawProtocolCodec : IHttpProtocolCodec
	{
		private const int SESSION_ID_LEN = 32;

		public string Encode(string sessionId, string message)
		{
			return ((sessionId == null) ? "" : sessionId) + message;
		}

		public string Decode(string message)
		{
			string result = "";
			if (message.Substring(0, 1) == "#")
			{
				result = message.Substring(1, 32);
			}
			return result;
		}
	}
}
