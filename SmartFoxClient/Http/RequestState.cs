using System.Net;

namespace SmartFoxClientAPI.Http
{
	public class RequestState
	{
		private WebRequest request;

		public RequestState()
		{
			request = null;
		}

		public WebRequest GetRequest()
		{
			return request;
		}

		public void SetRequest(WebRequest request)
		{
			this.request = request;
		}
	}
}
