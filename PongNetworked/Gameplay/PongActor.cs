using DaisNET.Gameplay;
using Pong.Networking;

namespace Pong.Gameplay
{
	public class PongActor(string name, PongNetworkPlayer player) : Actor<PongNetworkPlayer>(name, player)
	{
		
	}
}