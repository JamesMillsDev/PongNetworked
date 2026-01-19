using System.Net;
using System.Net.Sockets;
using Pong.Networking.Packets;

namespace Pong.Networking
{
	public class NetworkServer : Network
	{
		private readonly List<Socket> connections = [];

		public NetworkServer() : base(Dns.GetHostName())
		{
			_ = Task.Run(async () =>
			{
				try
				{
					await AwaitConnections();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			});
		}

		public override async Task Poll()
		{
			List<Socket> connected = [];
			lock (this.connections)
			{
				connected.AddRange(this.connections);
			}

			List<Task<Tuple<string, byte[]>>> reading = new(connected.Select(ReadPacket));
			await Task.WhenAll(reading.ToArray());

			foreach ((string id, byte[] payload) in reading.Select(task => task.Result))
			{
				if (!TryMakePacketFor(id, out Packet? packet))
				{
					continue;
				}

				PacketReader reader = new(payload);
				packet?.Deserialize(reader);
				packet?.Process();
			}

			await Task.Delay(20);
		}

		private async Task AwaitConnections()
		{
			if (this.socket == null)
			{
				throw new InvalidOperationException("Socket not connected!");
			}
			
			while (true)
			{
				Task<Socket> clientSocket = this.socket.AcceptAsync();
				await clientSocket;

				lock (this.connections)
				{
					this.connections.Add(clientSocket.Result);
				}
			}
		}
	}
}