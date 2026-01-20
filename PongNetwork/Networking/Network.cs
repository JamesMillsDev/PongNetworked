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

		/// <summary>
		/// Initializes and runs the network loop based on command-line arguments.
		/// Creates either a server or client instance and continuously polls for network events.
		/// </summary>
		/// <param name="args">Command-line arguments. First argument should be "server" or anything else for client.
		/// For client mode, second argument should be the server endpoint address.</param>
		/// <param name="app">The application instance used to check when to stop the network loop.</param>
		public static async Task RunNetworkLoop(string[] args, ApplicationBase app)
		{
			try
			{
				bool isServer = args[0] == "server";
				await InitializeAndPoll(isServer, !isServer ? args[1] : "", app);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		/// <summary>
		/// Reads a complete packet from the specified socket with timeout protection.
		/// Uses a two-stage process: first reads the length prefix, then reads the packet body.
		/// Each receive operation has a 1-second timeout to prevent indefinite blocking.
		/// </summary>
		/// <param name="target">The socket to read from.</param>
		/// <returns>A tuple containing the packet ID and payload data. Returns ("NULL", empty array) if no data is available.</returns>
		/// <exception cref="TimeoutException">Thrown when the packet is not received within the timeout period (1 second per read operation).</exception>
		/// <exception cref="Exception">Thrown when the client disconnects during the read operation.</exception>
		protected static async Task<Tuple<string, byte[]>> ReadPacket(Socket target)
		{
			// Check if any data is available before attempting to read
			if (target.Available == 0)
			{
				return new Tuple<string, byte[]>("NULL", []);
			}
			
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
		/// Creates and initializes a new network server instance.
		/// </summary>
		/// <param name="endpoint">The hostname or IP address to bind to. Defaults to "localhost".</param>
		/// <param name="port">The port number to listen on. Defaults to 25565.</param>
		private static void CreateServer(string endpoint = "localhost", int port = 25565) =>
			Instance = new NetworkServer(endpoint, port);

		/// <summary>
		/// Creates and initializes a new network client instance.
		/// </summary>
		/// <param name="endpoint">The hostname or IP address of the server to connect to.</param>
		/// <param name="port">The port number to connect to. Defaults to 25565.</param>
		private static void CreateClient(string endpoint, int port = 25565) =>
			Instance = new NetworkClient(endpoint, port);
		
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
		/// Internal method that initializes the network (server or client) and runs the polling loop.
		/// Continues polling until the application signals it is closing.
		/// </summary>
		/// <param name="isServer">True to create a server instance, false to create a client instance.</param>
		/// <param name="endpoint">The server endpoint address (only used for client mode).</param>
		/// <param name="app">The application instance to monitor for shutdown.</param>
		private static async Task InitializeAndPoll(bool isServer, string endpoint, ApplicationBase app)
		{
			if (isServer)
			{
				CreateServer();
			}
			else
			{
				CreateClient(endpoint);
			}

			if (Instance == null)
			{
				throw new NullReferenceException("Network instance is null");
			}

			Instance.Open();
			
			try
			{
				// Continue polling until the application signals it's closing
				while (!app.IsClosing)
				{
					try
					{
						await Instance.Poll();
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
						throw;
					}
				}

				Instance.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
		
		/// <summary>
		/// Gets whether this network instance has authority (true for server, false for client).
		/// The authoritative instance is responsible for game state and validation.
		/// </summary>
		public bool HasAuthority { get; protected init; }

		/// <summary>
		/// The underlying socket used for network communication.
		/// Null until <see cref="Open"/> is called.
		/// </summary>
		protected Socket? socket;

		/// <summary>
		/// The IP address resolved from the hostname provided in the constructor.
		/// </summary>
		protected readonly IPAddress ipAddr;

		/// <summary>
		/// The local endpoint combining the IP address and port number.
		/// </summary>
		protected readonly IPEndPoint localEndPoint;

		/// <summary>
		/// The rate (in milliseconds) at which the <see cref="Poll"/> function will run.
		/// </summary>
		protected readonly int pollRate;

		/// <summary>
		/// Dictionary mapping packet IDs to their corresponding packet types.
		/// Used to instantiate the correct packet class when receiving data.
		/// </summary>
		private readonly Dictionary<string, Type> registeredPackets = new();

		/// <summary>
		/// Initializes a new instance of the Network class with the specified hostname and port.
		/// Resolves the hostname to an IP address and creates an endpoint.
		/// </summary>
		/// <param name="hostName">The hostname to resolve (e.g., "localhost" or a domain name).</param>
		/// <param name="port">The port number to use for the connection. Defaults to 25565.</param>
		/// <param name="pollRate">How often (in milliseconds) the network should poll for changes. Defaults to 20ms.</param>
		protected Network(string hostName, int port = 25565, int pollRate = 20)
		{
			IPHostEntry ipHost = Dns.GetHostEntry(hostName);
			this.ipAddr = ipHost.AddressList[0];
			this.localEndPoint = new IPEndPoint(ipAddr, port);

			this.socket = null;
			this.pollRate = pollRate;
		}

		/// <summary>
		/// Sends a packet through this network instance's socket.
		/// No-op if the socket is not initialized.
		/// </summary>
		/// <param name="packet">The packet to serialize and send.</param>
		public void SendPacket(Packet packet)
		{
			if (this.socket == null)
			{
				return;
			}
			
			SendPacket(packet, this.socket);
		}

		/// <summary>
		/// Registers a packet type so it can be instantiated when received over the network.
		/// </summary>
		/// <param name="id">The unique ID for the packet. Must match <see cref="Packet.ID"/>.</param>
		/// <param name="type">The Type of the packet class.</param>
		/// <returns>True if the packet was successfully registered, false if a packet with this ID already exists.</returns>
		public bool RegisterPacket(string id, Type type) => this.registeredPackets.TryAdd(id, type);

		/// <summary>
		/// Polls the network for incoming data and processes packets.
		/// Must be implemented by derived classes (NetworkServer and NetworkClient).
		/// </summary>
		/// <returns>A task representing the asynchronous poll operation.</returns>
		protected abstract Task Poll();

		/// <summary>
		/// Opens the socket and prepares it for network communication.
		/// For servers: binds to the local endpoint and starts listening.
		/// For clients: creates the socket and initiates connection to the server.
		/// </summary>
		/// <param name="backlog">The maximum length of the pending connections queue (server only). Defaults to 10.</param>
		protected abstract void Open(int backlog = 10);

		/// <summary>
		/// Gracefully shuts down and closes the socket connection.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when attempting to close a socket that is not connected.</exception>
		protected virtual void Close()
		{
			if (this.socket is not { Connected: true })
			{
				throw new InvalidOperationException("Socket not connected");
			}

			this.socket.Shutdown(SocketShutdown.Both);
			this.socket.Close();
		}

		/// <summary>
		/// Attempts to create a packet instance for the given packet ID.
		/// </summary>
		/// <param name="id">The ID of the packet to create.</param>
		/// <param name="packet">Output parameter that receives the created packet instance, or null if creation failed.</param>
		/// <returns>True if a packet was successfully created, false if the ID is invalid or not registered.</returns>
		protected bool TryMakePacketFor(string id, out Packet? packet)
		{
			// Handle special "NULL" ID returned when no data is available
			if (id == "NULL")
			{
				packet = null;
				return false;
			}
			
			// Attempt to get the packet type from the registered packets dictionary
			if (!this.registeredPackets.TryGetValue(id, out Type? type))
			{
				Console.WriteLine($"No packet with id {id} found.");
				packet = null;
				return false;
			}

			// Create and return the packet instance
			packet = (Packet)Activator.CreateInstance(type)!;
			return true;
		}
	}
}