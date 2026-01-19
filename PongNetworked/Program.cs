using Pong.Networking;

namespace Pong
{
	public static class Program
	{
		private static void Main(string[] args)
		{
			bool isServer = args[0] == "server";
			
			Thread networkThread = new(NetworkThread);
			
			networkThread.Start();
			return;

			async void NetworkThread()
			{
				if (isServer)
				{
					Network.CreateServer();
				}
				else
				{
					Network.CreateClient();
				}

				if (Network.Instance == null)
				{
					throw new NullReferenceException("Network instance is null");
				}
				
				try
				{
					while (true)
					{
						try
						{
							await Network.Instance.Poll();
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							throw;
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}
	}
}