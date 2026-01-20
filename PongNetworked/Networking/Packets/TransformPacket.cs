using System.Numerics;
using Pong.Gameplay;

namespace Pong.Networking.Packets
{
	public class TransformPacket() : Packet("transform")
	{
		private string actorName = "";
		private Vector2 position = Vector2.Zero;
		private Vector2 size = Vector2.Zero;

		public TransformPacket(string actorName, Vector2 position, Vector2 size) : this()
		{
			this.actorName = actorName;
			this.position = position;
			this.size = size;
		}

		public override void Serialize(PacketWriter writer)
		{
			writer.Write(this.actorName);
			writer.Write(this.position.X);
			writer.Write(this.position.Y);
			writer.Write(this.size.X);
			writer.Write(this.size.Y);
		}

		public override void Deserialize(PacketReader reader)
		{
			this.actorName = reader.ReadString();
			this.position.X = reader.ReadFloat();
			this.position.Y = reader.ReadFloat();
			this.size.X = reader.ReadFloat();
			this.size.Y = reader.ReadFloat();
		}

		public override Task Process()
		{
			if (Network.Instance == null)
			{
				// This should never happen
				return Task.CompletedTask;
			}

			if (Network.Instance.HasAuthority)
			{
				((NetworkServer)Network.Instance).BroadcastPacket(
					new TransformPacket(this.actorName, this.position, this.size)
				);
			}
			
			Actor? actor = Application.actors.FirstOrDefault(a => a?.Name == this.actorName);
			if (actor == null)
			{
				return Task.CompletedTask;
			}

			actor.Transform.Position = this.position;
			actor.Transform.Size = this.size;

			return Task.CompletedTask;
		}
	}
}