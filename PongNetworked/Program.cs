using Pong.Gameplay;
using Pong.Networking;

namespace Pong
{
	public static class Program
	{
		private static async Task Main(string[] args)
		{
			bool isServer = args[0] == "server";
			Task networkTask = Task.Run(async () =>
			{
				try
				{
					await NetworkLoop(isServer, !isServer ? args[1] : "");
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			});

			await Tasks.While(() => Network.Instance == null);

			Network.Instance!.RegisterPacket("message", typeof(MessagePacket));
			
			if (!isServer)
			{
				Game game = new();
				game.Run();
			}

			await networkTask;
		}

		private static async Task NetworkLoop(bool isServer, string endpoint)
		{
			if (isServer)
			{
				Network.CreateServer();
			}
			else
			{
				Network.CreateClient(endpoint);
			}

			if (Network.Instance == null)
			{
				throw new NullReferenceException("Network instance is null");
			}

			Network.Instance.Open();

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
				
				Network.Instance.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}