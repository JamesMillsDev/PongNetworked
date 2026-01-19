using System.Net;

namespace Pong.Networking
{
	public class NetworkClient() : Network(Dns.GetHostName())
	{
		public override Task Poll()
		{
			return Task.CompletedTask;
		}
	}
}