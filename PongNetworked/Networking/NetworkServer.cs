using System.Net;
using System.Net.Sockets;
using Pong.Networking.Packets;

namespace Pong.Networking
{
	public class NetworkServer : INetwork
	{
		private readonly Dictionary<string, Type> registeredPackets = new();

		private readonly IPHostEntry ipHost;
		private readonly IPAddress ipAddr;
		private readonly IPEndPoint localEndPoint;

		private readonly Socket listener;
		private readonly List<Socket> clients = [];

		public NetworkServer()
		{
			this.ipHost = Dns.GetHostEntry(Dns.GetHostName());
			this.ipAddr = this.ipHost.AddressList[0];
			this.localEndPoint = new IPEndPoint(ipAddr, 25565);

			this.listener = new Socket(this.ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			this.listener.Bind(this.localEndPoint);
			this.listener.Listen(10);
		}

		public bool RegisterPacket(Packet packet) => this.registeredPackets.TryAdd(packet.ID, packet.GetType());

		public async Task AwaitClientConnection()
		{
			Task<Socket> clientSocket = this.listener.AcceptAsync();
			await clientSocket;

			this.clients.Add(clientSocket.Result);
		}

		public async Task Poll()
		{
			List<Tuple<Task, byte[]>> polled = [];
			polled.AddRange(this.clients.Select(clientSocket => PollClient(clientSocket)));

			foreach ((Task task, byte[] data) in polled)
			{
				await task;
				
				PacketReader reader = new(data);
				string id = reader.ReadString();
				if (!this.registeredPackets.TryGetValue(id, out Type? packetType))
				{
					Console.WriteLine($"Received packed with unknown id '{id}'. Ignoring");
					continue;
				}
				
				Packet packet = (Packet)Activator.CreateInstance(packetType, id)!;
				packet.Deserialize(reader);
				
				packet.Process();
			}
		}

		private Tuple<Task, byte[]> PollClient(Socket client, int bufferSize = 1024)
		{
			byte[] buffer = new byte[bufferSize];
			Task<int> readTask = client.ReceiveAsync(buffer);
			
			return new Tuple<Task, byte[]>(readTask, buffer);
		}
	}
}