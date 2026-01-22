using DaisNET.Networking;
using Pong.Client;
using Pong.Networking;

namespace Pong
{
	public static class Program
	{
		private static async Task Main(string[] args)
		{
			bool isServer = args[0] == "server";
			NetworkApplicationBase app = isServer ? new ServerNetworkApplication() : new ClientNetworkApplication();

			Task networkTask = Task.Run(async () => await Network.RunNetworkLoop(app, app.IsServer ? "" : args[1]));
			Task runTask = Task.Run(async () => await app.Run());

			await Task.WhenAll(runTask, networkTask);
		}
	}
}