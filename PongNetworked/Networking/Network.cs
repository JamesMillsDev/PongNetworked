namespace Pong.Networking
{
	public static class Network
	{
		public static INetwork? Instance
		{
			get;
			private set;
		}

		public static void CreateServer()
		{
			Instance = new NetworkServer();
		}

		public static void CreateClient()
		{
			
		}
	}
}