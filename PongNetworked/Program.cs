using Pong.Networking;

namespace Pong
{
	public static class Program
	{
		private static void Main(string[] args)
		{
			bool isServer = args[0] == "server";
			_ = Task.Run(async () =>
			{
				try
				{
					await NetworkLoop(isServer);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
			});
		}

		private static async Task NetworkLoop(bool isServer)
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