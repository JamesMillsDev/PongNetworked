using System.Net;
using System.Net.Sockets;
using System.Text;
using Pong.Networking.Packets;

namespace Pong.Networking
{
	/// <summary>
	/// Abstract base class for network implementations (server and client).
	/// Provides shared functionality for packet serialization, deserialization, and transmission.
	/// </summary>
	public abstract class Network
	{
		/// <summary>
		/// Gets the singleton instance of the current network implementation (either server or client).
		/// </summary>
		public static Network? Instance { get; private set; }

		/// <summary> Creates and initializes a new network server instance. </summary>
		public static void CreateServer() => Instance = new NetworkServer();

		/// <summary> Creates and initializes a new network client instance. </summary>
		public static void CreateClient() => Instance = new NetworkClient();

		/// <summary>
		/// Parses a packet buffer to extract the packet ID and payload data.
		/// Expected buffer format: [idLength (4 bytes)][id (variable)][payload (remaining)]
		/// </summary>
		/// <param name="buffer">The buffer containing the packet data.</param>
		/// <param name="id">Output parameter that receives the packet ID string.</param>
		/// <param name="payload">Output parameter that receives the packet payload bytes.</param>
		/// <exception cref="InvalidOperationException">Thrown when the buffer stream cannot be read.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the ID length is negative.</exception>
		private static void ReadPacketHeader(byte[] buffer, out string id, out byte[] payload)
		{
			using (MemoryStream memoryStream = new(buffer))
			{
				if (!memoryStream.CanRead)
				{
					throw new InvalidOperationException("Cannot read packet header");
				}

				// Read the length of the packet ID string
				byte[] idLengthBytes = new byte[sizeof(int)];
				memoryStream.ReadExactly(idLengthBytes);

				int idLength = BitConverter.ToInt32(idLengthBytes);
				ArgumentOutOfRangeException.ThrowIfNegative(idLength);

				// Handle empty ID case (ID length is 0)
				if (idLength == 0)
				{
					id = string.Empty;
					// Everything remaining in buffer is payload
					int payloadSize = (int)(memoryStream.Length - memoryStream.Position);
					payload = new byte[payloadSize];
					memoryStream.ReadExactly(payload);

					return;
				}

				// Read the packet ID string
				byte[] idBytes = new byte[idLength];
				memoryStream.ReadExactly(idBytes);
				id = Encoding.UTF8.GetString(idBytes);

				// Read the remaining bytes as payload
				int remainingPayloadSize = (int)(memoryStream.Length - memoryStream.Position);
				payload = new byte[remainingPayloadSize];
				memoryStream.ReadExactly(payload);
			}
		}

		/// <summary>
		/// Reads a complete packet from the specified socket with timeout protection.
		/// Uses a two-stage process: first reads the length prefix, then reads the packet body.
		/// Each receive operation has a 1-second timeout to prevent indefinite blocking.
		/// </summary>
		/// <param name="target">The socket to read from.</param>
		/// <returns>A tuple containing the packet ID and payload data.</returns>
		/// <exception cref="TimeoutException">Thrown when the packet is not received within the timeout period (1 second per read operation).</exception>
		/// <exception cref="Exception">Thrown when the client disconnects during the read operation.</exception>
		protected static async Task<Tuple<string, byte[]>> ReadPacket(Socket target)
		{
			// Stage 1: Read the 4-byte length prefix
			// This tells us how many bytes to expect in the packet body
			byte[] lengthBuffer = new byte[sizeof(int)];
			int totalRead = 0;
			while (totalRead < sizeof(int))
			{
				ValueTask<int> receiveValueTask = target.ReceiveAsync(lengthBuffer.AsMemory(totalRead));
				Task<int> receiveTask = receiveValueTask.AsTask();
				Task completedTask = await Task.WhenAny(receiveTask, Task.Delay(1000));

				// Check if the receive operation timed out
				if (completedTask != receiveTask)
				{
					throw new TimeoutException("Failed to read packet in time!");
				}

				int bytesRead = receiveTask.Result;

				// Check if the client disconnected (0 bytes read indicates disconnection)
				if (bytesRead == 0)
				{
					throw new Exception("Client disconnected");
				}

				totalRead += bytesRead;
			}

			int packetLength = BitConverter.ToInt32(lengthBuffer);

			// Stage 2: Read the packet body using the length we just received
			// The packet body contains [idLength][id][payload]
			byte[] buffer = new byte[packetLength];
			totalRead = 0;
			while (totalRead < packetLength)
			{
				ValueTask<int> receiveValueTask = target.ReceiveAsync(buffer.AsMemory(totalRead));
				Task<int> receiveTask = receiveValueTask.AsTask();
				Task completedTask = await Task.WhenAny(receiveTask, Task.Delay(1000));

				// Check if the receive operation timed out
				if (completedTask != receiveTask)
				{
					throw new TimeoutException("Failed to read packet in time!");
				}

				int bytesRead = receiveTask.Result;

				// Check if the client disconnected (0 bytes read indicates disconnection)
				if (bytesRead == 0)
				{
					throw new Exception("Client disconnected");
				}

				totalRead += bytesRead;
			}

			// Parse the packet header to extract the ID and payload
			ReadPacketHeader(buffer, out string id, out byte[] payload);
			return new Tuple<string, byte[]>(id, payload);
		}

		/// <summary>
		/// Serializes and sends a packet to the specified socket.
		/// The packet is sent with a 4-byte length prefix followed by the serialized packet data.
		/// Packet format: [totalLength (4 bytes)][idLength (4 bytes)][id (variable)][payload (variable)]
		/// </summary>
		/// <param name="packet">The packet to serialize and send.</param>
		/// <param name="target">The socket to send the packet to.</param>
		protected static void SendPacket(Packet packet, Socket target)
		{
			// Serialize the packet (ID + payload data)
			PacketWriter writer = new(packet.ID);
			packet.Serialize(writer);

			byte[] serialized = writer.GetBytes();

			// Prepend the total length as a 4-byte prefix
			byte[] data = new byte[serialized.Length + sizeof(int)];
			Array.Copy(BitConverter.GetBytes(serialized.Length), data, sizeof(int));
			Array.Copy(serialized, 0, data, sizeof(int), serialized.Length);

			target.Send(data);
		}

		/// <summary>
		/// The underlying socket used for network communication.
		/// Null until <see cref="Open"/> is called.
		/// </summary>
		protected Socket? socket;

		/// <summary> The IP address resolved from the hostname provided in the constructor. </summary>
		private readonly IPAddress ipAddr;
		
		/// <summary> The local endpoint combining the IP address and port number. </summary>
		private readonly IPEndPoint localEndPoint;
		
		/// <summary>
		/// The list of valid packets that can be sent / received on the network.
		/// </summary>
		private readonly Dictionary<string, Type> registeredPackets = new();

		/// <summary>
		/// Initializes a new instance of the Network class with the specified hostname and port.
		/// Resolves the hostname to an IP address and creates an endpoint.
		/// </summary>
		/// <param name="hostName">The hostname to resolve (e.g., "localhost" or a domain name).</param>
		/// <param name="port">The port number to use for the connection. Defaults to 25565 (Minecraft's default port).</param>
		protected Network(string hostName, int port = 25565)
		{
			IPHostEntry ipHost = Dns.GetHostEntry(hostName);
			this.ipAddr = ipHost.AddressList[0];
			this.localEndPoint = new IPEndPoint(ipAddr, port);

			this.socket = null;
		}

		/// <summary>
		/// Polls the network for incoming data and processes packets.
		/// Must be implemented by derived classes (NetworkServer and NetworkClient).
		/// </summary>
		/// <returns>A task representing the asynchronous poll operation.</returns>
		public abstract Task Poll();

		/// <summary>
		/// Opens the socket, binds it to the local endpoint, and starts listening for connections.
		/// This is typically called by server implementations.
		/// </summary>
		/// <param name="backlog">The maximum length of the pending connections queue. Defaults to 10.</param>
		public void Open(int backlog = 10)
		{
			this.socket = new Socket(this.ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			this.socket.Bind(this.localEndPoint);
			this.socket.Listen(backlog);
		}

		/// <summary>
		/// Gracefully shuts down and closes the socket connection.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when attempting to close a socket that is not connected.</exception>
		public void Close()
		{
			if (this.socket == null)
			{
				throw new InvalidOperationException("Socket not connected");
			}

			this.socket.Shutdown(SocketShutdown.Both);
			this.socket.Close();
		}

		/// <summary>
		/// Registers a packet type to the internal tracker.
		/// </summary>
		/// <param name="id">The ID for the packet. Must match the variable <see cref="Packet.ID"/>.</param>
		/// <param name="type">The type of the packet.</param>
		/// <returns>Whether the packet was successfully added.</returns>
		public bool RegisterPacket(string id, Type type) => this.registeredPackets.TryAdd(id, type);
		
		/// <summary>
		/// Attempts to create and get a packet for the passed id.
		/// </summary>
		/// <param name="id">The ID for the packet we are attempting to make</param>
		/// <param name="packet">The created packet. Will be null if <see cref="id"/> is an invalid id.</param>
		/// <returns>True if a packet was successfully created, false if it wasn't.</returns>
		protected bool TryMakePacketFor(string id, out Packet? packet)
		{
			// Attempt to get the packet type from the included dictionary
			if (!this.registeredPackets.TryGetValue(id, out Type? type))
			{
				Console.WriteLine($"No packet with id {id} found.");
				packet = null;
				return false;
			}
			
			// Create and return the packet
			packet = (Packet)Activator.CreateInstance(type, id)!;
			return true;
		}
	}
}