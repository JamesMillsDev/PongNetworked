using System.Net.Sockets;
using System.Text;
using Pong.Networking.Packets;

namespace Pong.Networking
{
	public abstract class Network
	{
		public static Network? Instance { get; private set; }

		public static void CreateServer()
		{
			Instance = new NetworkServer();
		}

		public static void CreateClient()
		{
			Instance = new NetworkClient();
		}
		
		private static void ReadPacketHeader(byte[] buffer, out string id, out byte[] payload)
		{
			using (MemoryStream memoryStream = new(buffer))
			{
				if (!memoryStream.CanRead)
				{
					throw new InvalidOperationException("Cannot read packet header");
				}

				byte[] idLengthBytes = new byte[sizeof(int)];
				memoryStream.ReadExactly(idLengthBytes);

				int idLength = BitConverter.ToInt32(idLengthBytes);
				ArgumentOutOfRangeException.ThrowIfNegative(idLength);

				if (idLength == 0)
				{
					id = string.Empty;
					// Everything remaining in buffer is payload
					int payloadSize = (int)(memoryStream.Length - memoryStream.Position);
					payload = new byte[payloadSize];
					memoryStream.ReadExactly(payload);

					return;
				}

				byte[] idBytes = new byte[idLength];
				memoryStream.ReadExactly(idBytes);
				id = Encoding.UTF8.GetString(idBytes);

				// Everything remaining in buffer is payload
				int remainingPayloadSize = (int)(memoryStream.Length - memoryStream.Position);
				payload = new byte[remainingPayloadSize];
				memoryStream.ReadExactly(payload);
			}
		}

		protected static async Task<Tuple<string, byte[]>> ReadPacket(Socket target)
		{
			// Stage 1: Read length prefix (keep receiving until we have 4 bytes)
			byte[] lengthBuffer = new byte[sizeof(int)];
			int totalRead = 0;
			while (totalRead < sizeof(int))
			{
				int bytesRead = await target.ReceiveAsync(lengthBuffer.AsMemory(totalRead));
				totalRead += bytesRead;
			}
			int packetLength = BitConverter.ToInt32(lengthBuffer);

			// Stage 2: Read packet body (keep receiving until we have packetLength bytes)
			byte[] buffer = new byte[packetLength];
			totalRead = 0;
			while (totalRead < packetLength)
			{
				int bytesRead = await target.ReceiveAsync(buffer.AsMemory(totalRead));
				totalRead += bytesRead;
			}

			ReadPacketHeader(buffer, out string id, out byte[] payload);
			return new Tuple<string, byte[]>(id, payload);
		}

		protected static void SendPacket(Packet packet, Socket target)
		{
			PacketWriter writer = new(packet.ID);
			packet.Serialize(writer);

			byte[] serialized = writer.GetBytes();
			byte[] data = new byte[serialized.Length + sizeof(int)];

			Array.Copy(BitConverter.GetBytes(serialized.Length), data, sizeof(int));
			Array.Copy(serialized, 0, data, sizeof(int), serialized.Length);

			target.Send(data);
		}

		public abstract Task Poll();
	}
}