using Pong.Gameplay;
using Pong.Networking;

namespace Pong
{
	public static class Program
	{
		private static async Task Main(string[] args)
		{
			bool isServer = args[0] == "server";
			Task networkTask = Task.Run(async () => await NetworkTaskFnc());

			if (!isServer)
			{
				Game game = new Game();
				game.Run();
			}

			await networkTask;

			return;

			async Task NetworkTaskFnc()
			{
				try
				{
					await NetworkLoop(isServer, !isServer ? args[1] : "");
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
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
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}