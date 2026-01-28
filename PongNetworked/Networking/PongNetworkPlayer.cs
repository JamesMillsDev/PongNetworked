using System.Numerics;
using DaisNET.Networking;
using DaisNET.Networking.Gameplay;
using Pong.Gameplay;
using Raylib_cs;

namespace Pong.Networking
{
	public class PongNetworkPlayer(NetworkConnection connection, bool isLocalPlayer)
		: NetworkPlayer(connection, isLocalPlayer)
	{
		private Paddle? paddle;
		
		public override void OnClientConnected()
		{
			if (Network.Instance == null)
			{
				throw new NullReferenceException("Network instance is null");
			}

			if (!this.IsLocalPlayer)
			{
				return;
			}

			Paddle newPaddle = Network.Instance.World.Spawn<Paddle>(this.Connection.ID);
			newPaddle.color = this.Connection.ID == 0 ? Color.Red : Color.Green;
			newPaddle.Transform.Position = GetStartPos(this.Connection.ID);
		}

		public override void OnClientDisconnected()
		{
		}

		public override void OnOwnedActorSpawned(Actor actor)
		{
			if (actor is Paddle paddleActor)
			{
				this.paddle = paddleActor;
			}
		}

		private Vector3 GetStartPos(uint id)
		{
			return id switch
			{
				0 => new Vector3(25, 25, 1),
				1 => new Vector3(Raylib.GetScreenWidth() - 50, 25, 1),
				_ => throw new IndexOutOfRangeException()
			};
		}
	}
}