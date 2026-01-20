using Pong.Networking;

namespace Pong
{
	public abstract class Application
	{
		private static void RegisterPackets(Network network)
		{
			network.RegisterPacket("message", typeof(MessagePacket));
		}
		
		public bool IsClosing { get; protected set; }

		public async Task Run()
		{
			await Tasks.While(() => Network.Instance == null);

			if (Network.Instance == null)
			{
				// this should literally be impossible
				return;
			}
			
			RegisterPackets(Network.Instance);
			
			Initialise(Network.Instance);

			while (!ShouldClose())
			{
				Tick(Network.Instance);
			}
			
			Shutdown(Network.Instance);
		}

		protected abstract bool ShouldClose();
		
		protected abstract void Initialise(Network network);
		protected abstract void Tick(Network network);
		protected abstract void Shutdown(Network network);
	}
}