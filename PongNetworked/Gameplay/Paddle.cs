using System.Numerics;
using DaisNET.Networking.Networking;
using Pong.Networking.Packets;
using Raylib_cs;

namespace Pong.Gameplay
{
	public class Paddle : Actor
	{
		private readonly Color color;

		public Paddle(string name, Vector2 initialPosition, Color color) : base(name)
		{
			this.Transform.Position = initialPosition;

			this.Transform.Size = new Vector2(25, 150);
			this.color = color;
		}

		public override void Tick(float dt)
		{
			if (Raylib.IsKeyPressed(KeyboardKey.Up) && this.Name == "player1")
			{
				Network.Instance?.SendPacket(
					new TransformPacket(
						this.Name,
						this.Transform with
						{
							Position = this.Transform.Position + new  Vector2(0, this.Transform.Size.Y)
						}
					)
				);
			}
		}

		public override void Render()
		{
			Raylib.DrawRectangleV(this.Transform.Position, this.Transform.Size, this.color);
		}
	}
}