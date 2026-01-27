using DaisNET.Networking;

namespace Pong.Networking
{
	public class PongNetworkPlayer(NetworkConnection connection, bool isLocalPlayer)
		: NetworkPlayer(connection, isLocalPlayer)
	{
		public override void OnClientConnected()
		{
		}

		public override void OnClientDisconnected()
		{
		}
	}
}