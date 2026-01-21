using DaisNET.Networking;
using DaisNET.Networking.Packets;
using Pong.Gameplay;

namespace Pong.Networking.Packets
{
	public class TransformPacket() : Packet("transform")
	{
		private string actorName = "";
		private Transform transform = new();

		public TransformPacket(string actorName, Transform transform) : this()
		{
			this.actorName = actorName;
			this.transform = transform;
		}

		public override void Serialize(PacketWriter writer)
		{
			writer.Write(this.actorName);
			writer.Write(this.transform);
		}

		public override void Deserialize(PacketReader reader)
		{
			this.actorName = reader.ReadString();
			this.transform = reader.ReadPacketSerializable<Transform>();
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
					new TransformPacket(this.actorName, this.transform)
				);
			}
			
			/*Actor? actor = Application.actors.FirstOrDefault(a => a?.Name == this.actorName);
			if (actor == null)
			{
				return Task.CompletedTask;
			}

			actor.Transform = this.transform;*/

			return Task.CompletedTask;
		}
	}
}