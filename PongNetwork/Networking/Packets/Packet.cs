namespace Pong.Networking.Packets
{
	public abstract class Packet(string id)
	{
		public string ID { get; } = id;

		public abstract void Serialize(PacketWriter writer);

		public abstract void Deserialize(PacketReader reader);

		public abstract void Process();
	}
}