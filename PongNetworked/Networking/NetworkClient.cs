namespace Pong.Networking
{
	// TODO: This
	public class NetworkClient : Network
	{
		public override Task Poll()
		{
			return Task.CompletedTask;
		}
	}
}