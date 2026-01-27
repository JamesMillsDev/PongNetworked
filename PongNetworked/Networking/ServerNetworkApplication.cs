using DaisNET.Networking;

namespace Pong.Networking
{
	public class ServerNetworkApplication() : NetworkApplicationBase<PongNetworkPlayer>(true)
	{
		private bool shouldClose;

		protected override void RegisterPackets(Network<PongNetworkPlayer> network)
		{
		}

		protected override bool ShouldClose() => shouldClose;

		protected override void Initialise(Network<PongNetworkPlayer> network) => this.shouldClose = false;

		protected override void Tick(Network<PongNetworkPlayer> network)
		{
			string? line = Console.ReadLine();
			if (line == null)
			{
				return;
			}

			shouldClose = line == "close";
		}

		protected override void Shutdown(Network<PongNetworkPlayer> network)
		{
		}
	}
}