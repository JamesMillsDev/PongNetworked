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
				Raylib.BeginDrawing();
				Raylib.ClearBackground(Color.RayWhite);
				
				Raylib.EndDrawing();
			}
			
			Raylib.CloseWindow();
		}
	}
}