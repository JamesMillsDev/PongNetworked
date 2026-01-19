using System.Net;
using System.Net.Sockets;
using Pong.Networking.Packets;

namespace Pong.Networking
{
	public class NetworkServer : Network
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

		public override async Task Poll()
		{
			
		}
	}
}