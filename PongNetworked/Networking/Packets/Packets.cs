using DaisNET.Networking;

namespace Pong.Networking.Packets
{
	public static class Packets
	{
		public static void Register<T>(Network<T> network)
			where T : NetworkPlayer, new()
		{
			network.RegisterPacket(TransformPacket.ID_NAME, typeof(TransformPacket));
		}
	}
}