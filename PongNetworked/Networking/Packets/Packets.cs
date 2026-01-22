using DaisNET.Networking;

namespace Pong.Networking.Packets
{
	public static class Packets
	{
		public static void Register(Network network)
		{
			network.RegisterPacket(TransformPacket.ID_NAME, typeof(TransformPacket));
		}
	}
}