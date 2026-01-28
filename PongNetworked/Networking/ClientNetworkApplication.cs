using DaisNET.Networking;
using DaisNET.Networking.Gameplay;
using Pong.Client;
using Raylib_cs;

namespace Pong.Networking
{
	public class ClientNetworkApplication() : NetworkApplicationBase(false)
	{
		protected override void RegisterPackets(Network network)
		{
			
		}

		protected override bool ShouldClose() => Raylib.WindowShouldClose();

		protected override void Initialise(Network network)
		{
			PongTraceLog.SetupLog();

			Raylib.InitWindow(800, 600, "Pong");
		}

		protected override void Tick(Network network)
		{
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.RayWhite);

			foreach ((Guid _, Actor actor) in network.World)
			{
				actor.Render();
			}

			Raylib.EndDrawing();
		}

		protected override unsafe void Shutdown(Network network)
		{
			Raylib.CloseWindow();

			Raylib.SetTraceLogCallback(&Logging.LogConsole);
		}

		protected override float GetFrameTime() => Raylib.GetFrameTime();
	}
}