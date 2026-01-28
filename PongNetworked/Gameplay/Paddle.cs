using System.Numerics;
using DaisNET.Networking;
using DaisNET.Networking.Gameplay;
using Raylib_cs;

namespace Pong.Gameplay
{
	public class Paddle : Actor
	{
		public Color color;

		public Paddle(NetworkPlayer player, Guid id)
			: base(player, id)
		{
			this.Transform.Scale = new Vector3(25, 150, 1);
			this.color = Color.RayWhite;
		}

		/*public override void Tick(float dt)
		{
			if (!this.GetNetworkPlayer<PongNetworkPlayer>().IsLocalPlayer)
			{
				return;
			}
		}*/

		public override void Render()
		{
			Vector2 position = new(this.Transform.Position.X, this.Transform.Position.Y);
			Vector2 size = new(this.Transform.Scale.X, this.Transform.Scale.Y);

			Raylib.DrawRectangleV(position, size, this.color);
		}
	}
}