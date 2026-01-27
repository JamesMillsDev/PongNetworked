using DaisNET.Networking;
using Pong.Client;
using Raylib_cs;

namespace Pong.Networking
{
	public class ClientNetworkApplication() : NetworkApplicationBase<PongNetworkPlayer>(false)
	{
		protected override void RegisterPackets(Network<PongNetworkPlayer> network)
		{
			
		}

		protected override bool ShouldClose() => Raylib.WindowShouldClose();

		protected override void Initialise(Network<PongNetworkPlayer> network)
		{
			PongTraceLog.SetupLog();

			Raylib.InitWindow(800, 600, "Pong");
		}

		protected override void Tick(Network<PongNetworkPlayer> network)
		{
			/*foreach (Actor<PongNetworkPlayer> actor in network.Actors)
			{
				actor.Tick(Raylib.GetFrameTime());
			}*/

			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.RayWhite);

			/*foreach (Actor<PongNetworkPlayer> actor in network.Actors)
			{
				actor.Render();
			}*/

			Raylib.EndDrawing();
		}

		protected override unsafe void Shutdown(Network<PongNetworkPlayer> network)
		{
			Raylib.CloseWindow();

			Raylib.SetTraceLogCallback(&Logging.LogConsole);
		}
	}
}