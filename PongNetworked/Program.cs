using DaisNET.Networking;
using DaisNET.Networking.Networking;
using Pong.Client;
using Pong.Networking;

namespace Pong
{
	public static class Program
	{
		private static async Task Main(string[] args)
		{
			bool isServer = args[0] == "server";
			ApplicationBase app = isServer ? new ServerApplication() : new ClientApplication();

			Task networkTask = Task.Run(async () => await Network.RunNetworkLoop(args, app));
			Task runTask = Task.Run(async () => await app.Run());

			await Task.WhenAll(runTask, networkTask);
		}
	}
}