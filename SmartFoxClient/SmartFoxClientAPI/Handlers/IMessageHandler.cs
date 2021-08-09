namespace SmartFoxClientAPI.Handlers
{
	public interface IMessageHandler
	{
		void HandleMessage(object msgObj, string type);
	}
}
