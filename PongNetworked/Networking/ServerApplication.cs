using DaisNET.Networking;
using DaisNET.Networking.Networking;

namespace Pong.Networking
{
	public class ServerApplication : ApplicationBase
	{
		private bool shouldClose;

		protected override void RegisterPackets(Network network) => Packets.Packets.Register(network);

		protected override bool ShouldClose() => shouldClose;

		protected override void Initialise(Network network) => this.shouldClose = false;

		protected override void Tick(Network network)
		{
			string? line = Console.ReadLine();
			if (line == null)
			{
				return;
			}

			shouldClose = line == "close";
		}

		protected override void Shutdown(Network network)
		{
		}
	}
}