namespace Pong.Networking
{
	public class ServerApplication : Application
	{
		private bool shouldClose;
		
		protected override bool ShouldClose() => shouldClose;

		protected override void Initialise(Network network)
		{
		}

		protected override void Tick(Network network)
		{
			string? line = Console.ReadLine();
			if (line == null)
			{
				return;
			}
			
			shouldClose = line == "close";
			if (this.shouldClose)
			{
				IsClosing = true;
			}
		}

		protected override void Shutdown(Network network)
		{
		}
	}
}