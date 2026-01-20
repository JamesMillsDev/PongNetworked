using System.Net;
using System.Net.Sockets;
using Pong.Networking.Packets;

namespace Pong.Networking
{
	public class NetworkServer : Network
	{
		private readonly List<Socket> connections = [];

		public NetworkServer(string host, int port = 25565) : base(host, port)
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
				packet!.Deserialize(reader);
				await packet.Process();
			}

			await Task.Delay(this.pollRate);
		}

		public override void Open(int backlog = 10)
		{
			this.socket = new Socket(this.ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			this.socket.Bind(this.localEndPoint);
			this.socket.Listen(backlog);
		}

		private async Task AwaitConnections()
		{
			await Tasks.While(() => this.socket == null);
			
			while (true)
			{
				Task<Socket> clientSocket = this.socket!.AcceptAsync();
				await clientSocket;
				
				Console.WriteLine("Client: {0} connected", clientSocket.Result.RemoteEndPoint);

				lock (this.connections)
				{
					this.connections.Add(clientSocket.Result);
				}
			}
		}
	}
}