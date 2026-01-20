using Pong.Networking;
using Raylib_cs;

namespace Pong.Gameplay
{
	public class ClientApplication : Application
	{
		protected override bool ShouldClose() => Raylib.WindowShouldClose();

		protected override void Initialise(Network network)
		{
			Raylib.InitWindow(800, 600, "Pong");
		}

		protected override void Tick(Network network)
		{
			if (Raylib.IsKeyPressed(KeyboardKey.Space))
			{
				network.SendPacket(new MessagePacket("Banana"));
			}

			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.RayWhite);

			Raylib.EndDrawing();
		}

		protected override void Shutdown(Network network)
		{
			Raylib.CloseWindow();
		}
	}
}