namespace Pong.Networking
{
	// TODO: This
	public class NetworkClient : INetwork
	{
		public Task Poll()
		{
			return Task.CompletedTask;
		}
	}
}