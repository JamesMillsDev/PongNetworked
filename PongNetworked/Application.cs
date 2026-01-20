using System.Numerics;
using Pong.Gameplay;
using Pong.Networking;
using Pong.Networking.Packets;
using Raylib_cs;

namespace Pong
{
	public abstract class Application
	{
		public static readonly List<Actor?> actors =
		[
			new Paddle("player1", new Vector2(10, 10), Color.Red),
			new Paddle("player2", new Vector2(765, 10), Color.Blue)
		];

		private static void RegisterPackets(Network network)
		{
			network.RegisterPacket("transform", typeof(TransformPacket));
		}

		public bool IsClosing { get; protected set; }

		public async Task Run()
		{
			await Tasks.While(() => Network.Instance == null);

			if (Network.Instance == null)
			{
				// this should literally be impossible
				return;
			}

			RegisterPackets(Network.Instance);

			this.IsClosing = false;
			Initialise(Network.Instance);

			while (!ShouldClose())
			{
				Tick(Network.Instance);
			}

			Shutdown(Network.Instance);
			IsClosing = true;
		}

		protected abstract bool ShouldClose();
		protected abstract void Initialise(Network network);
		protected abstract void Tick(Network network);
		protected abstract void Shutdown(Network network);
	}
}