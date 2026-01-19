using System.Net;
using System.Net.Sockets;
using Pong.Networking.Packets;

namespace Pong.Networking
{
	public class NetworkServer : Network
	{
		private readonly Dictionary<string, Type> registeredPackets = new();

		private readonly List<Socket> clients = [];

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

		public bool RegisterPacket(Packet packet) => this.registeredPackets.TryAdd(packet.ID, packet.GetType());

		public override async Task Poll()
		{
			List<Socket> connected = [];
			lock (this.clients)
			{
				connected.AddRange(this.clients);
			}

			List<Task<Tuple<string, byte[]>>> reading = [];
			reading.AddRange(connected.Select(ReadPacket));

			// ReSharper disable once CoVariantArrayConversion
			await Task.WhenAll(reading.ToArray());

			foreach ((string id, byte[] payload) in reading.Select(task => task.Result))
			{
				if (!this.registeredPackets.TryGetValue(id, out Type? packetType))
				{
					Console.WriteLine($"No packet with id {id} found.");
					continue;
				}

				PacketReader reader = new(payload);
				Packet packet = (Packet)Activator.CreateInstance(packetType, id)!;

				packet.Deserialize(reader);
				packet.Process();
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

				lock (this.clients)
				{
					this.clients.Add(clientSocket.Result);
				}
			}
		}
	}
}