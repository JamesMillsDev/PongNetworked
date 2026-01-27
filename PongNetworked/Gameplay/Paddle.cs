using System.Numerics;
using Pong.Networking;
using Raylib_cs;

namespace Pong.Gameplay
{
	public class Paddle : PongActor
	{
		private readonly Color color;

		public Paddle(string name, Color color, PongNetworkPlayer player) : base(name, player)
		{
			this.Transform.Size = new Vector2(25, 150);
			this.color = color;
		}

		public override void Tick(float dt)
		{
			if (!this.Player.IsLocalPlayer)
			{
				return;
			}
			
			if (Raylib.IsKeyDown(KeyboardKey.W))
			{
				this.Transform.Velocity -= Vector2.UnitY * 100 * dt;
			}

			if (Raylib.IsKeyDown(KeyboardKey.S))
			{
				this.Transform.Velocity += Vector2.UnitY * 100 * dt;
			}
		}

		public override void Render()
		{
			Raylib.DrawRectangleV(this.Transform.Position, this.Transform.Size, this.color);
		}
	}
}