namespace Pong.Networking.Packets
{
	public interface IPacketSerializable
	{
		public byte[] Serialize();
		public void Deserialize(byte[] data);
		public int GetSize();
	}
}