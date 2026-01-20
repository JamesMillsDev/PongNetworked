namespace Pong.Networking
{
	public class ServerApplication : Application
	{
		private bool shouldClose;

		protected override bool ShouldClose() => shouldClose;

		protected override void Initialise(Network network)
		{
			this.shouldClose = false;
		}

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