using System.Numerics;
using DaisNET.Networking;
using Pong.Gameplay;
using Raylib_cs;

namespace Pong.Networking
{
	public class PongNetworkPlayer : NetworkPlayer
	{
		private Paddle? paddle;
		
		public override void OnClientConnected()
		{
			this.paddle = new Paddle($"player{Connection.ID}", Color.Red, this);
		}

		public override void OnClientDisconnected()
		{
			if (this.paddle == null)
			{
				return;
			}
		}
	}
}