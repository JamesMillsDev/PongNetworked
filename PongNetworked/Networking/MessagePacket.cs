using Pong.Networking.Packets;

namespace Pong.Networking
{
	public class MessagePacket(string message) : Packet("message")
	{
		private string message = message;

		public override void Serialize(PacketWriter writer)
		{
			writer.Write(message);
		}

		public override void Deserialize(PacketReader reader)
		{
			this.message = reader.ReadString();
		}

		public override Task Process()
		{
			Console.WriteLine(this.message);

			if (Network.Instance!.IsHost)
			{
				((NetworkServer)Network.Instance).BroadcastPacket(new MessagePacket(this.message));
			}
			
			return Task.CompletedTask;
		}
	}
}