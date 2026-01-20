using System.Net.Sockets;
using Pong.Networking.Packets;

namespace Pong.Networking
{
	public delegate void ClientConnectionDelegate(byte id);

	// TODO: Work out client's disconnecting
	public class NetworkServer : Network
	{
		private static byte nextId;
		
		public event ClientConnectionDelegate? OnClientConnected;
		public event ClientConnectionDelegate? OnClientDisconnected;

		private readonly List<Socket> connections = [];
		private bool isClosing;

		private readonly Queue<byte> returnedIds = [];

		public NetworkServer(string host, int port = 25565) : base(host, port)
		{
			this.HasAuthority = true;

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

		public void BroadcastPacket(Packet packet)
		{
			lock (connections)
			{
				foreach (Socket connection in connections)
				{
					SendPacket(packet, connection);
				}
			}
		}

		protected override async Task Poll()
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

		protected override void Open(int backlog = 10)
		{
			this.socket = new Socket(this.ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			this.socket.Bind(this.localEndPoint);
			this.socket.Listen(backlog);
		}

		protected override void Close()
		{
			lock (this.connections)
			{
				foreach (Socket connection in this.connections)
				{
					connection.Shutdown(SocketShutdown.Both);
					connection.Close();
				}

				this.connections.Clear();
			}

			this.isClosing = true;
		}

		private async Task AwaitConnections()
		{
			await Tasks.While(() => this.socket == null);

			while (!this.isClosing)
			{
				Task<Socket> clientSocket = this.socket!.AcceptAsync();
				await Task.WhenAny(clientSocket, Tasks.While(() => !this.isClosing));

				if (!clientSocket.IsCompleted)
				{
					continue;
				}

				if (!this.returnedIds.TryDequeue(out byte id))
				{
					id = nextId++;
				}

				lock (this.connections)
				{
					this.connections.Add(clientSocket.Result);
				}
				
				OnClientConnected?.Invoke(id);
				Console.WriteLine("Client: {0} connected - Assigning id: {1}", clientSocket.Result.RemoteEndPoint, id);
			}
		}
	}
}