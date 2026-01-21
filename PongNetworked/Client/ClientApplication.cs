using DaisNET.Networking;
using DaisNET.Networking.Networking;
using Pong.Networking.Packets;
using Raylib_cs;

namespace Pong.Client
{
	public class ClientApplication : ApplicationBase
	{
		protected override void RegisterPackets(Network network)
		{
			Packets.Register(network);
		}
		
		protected override bool ShouldClose() => Raylib.WindowShouldClose();

		protected override void Initialise(Network network)
		{
			Raylib.InitWindow(800, 600, "Pong");
		}

		protected override void Tick(Network network)
		{
			/*foreach (Actor? actor in actors)
			{
				actor?.Tick(Raylib.GetFrameTime());
			}*/
			
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.RayWhite);

			/*foreach (Actor? actor in actors)
			{
				actor?.Render();
			}*/

			Raylib.EndDrawing();
		}

		protected override void Shutdown(Network network)
		{
			Raylib.CloseWindow();
		}
	}
}