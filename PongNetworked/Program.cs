using Pong.Client;
using Pong.Gameplay;
using Pong.Networking;

namespace Pong
{
	public static class Program
	{
		private static async Task Main(string[] args)
		{
			bool isServer = args[0] == "server";
			Application app = isServer ? new ServerApplication() : new ClientApplication();

			Task networkTask = Task.Run(async () =>
			{
				try
				{
					await NetworkLoop(isServer, !isServer ? args[1] : "", app);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			});
			
			Task runTask = Task.Run(async () => await app.Run());

			await Task.WhenAll(runTask, networkTask);
		}

		private static async Task NetworkLoop(bool isServer, string endpoint, Application app)
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
				while (!app.IsClosing)
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