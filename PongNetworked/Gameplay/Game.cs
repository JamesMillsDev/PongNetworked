using Pong.Networking;
using Raylib_cs;

namespace Pong.Gameplay
{
	public class Game
	{
		public void Run()
		{
			Raylib.InitWindow(800, 600, "Pong");

			

			while (!Raylib.WindowShouldClose())
			{
				if (Raylib.IsKeyPressed(KeyboardKey.Space))
				{
					Network.Instance!.SendPacket(new MessagePacket("Banana"));
				}

				Raylib.BeginDrawing();
				Raylib.ClearBackground(Color.RayWhite);

				Raylib.EndDrawing();
			}

			Raylib.CloseWindow();
		}
	}
}