using Pong.Networking;

namespace Pong
{
	public abstract class ApplicationBase
	{
		public bool IsClosing { get; private set; }

		public async Task Run()
		{
			await Tasks.While(() => Network.Instance == null);
			
			if (Network.Instance == null)
			{
				// this should literally be impossible
				return;
			}

			RegisterPackets(Network.Instance);

			this.IsClosing = false;
			Initialise(Network.Instance);

			while (!ShouldClose())
			{
				Tick(Network.Instance);
			}

			Shutdown(Network.Instance);
			IsClosing = true;
		}
		
		protected abstract void RegisterPackets(Network network);
		protected abstract bool ShouldClose();
		protected abstract void Initialise(Network network);
		protected abstract void Tick(Network network);
		protected abstract void Shutdown(Network network);
	}
}