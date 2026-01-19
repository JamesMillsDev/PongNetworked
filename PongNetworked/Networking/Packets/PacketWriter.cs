using System.Net.Sockets;
using System.Text;

namespace Pong.Networking.Packets
{
	public class PacketWriter : IDisposable, IAsyncDisposable
	{
		private static byte[] Serialize(object? data)
		{
			ArgumentNullException.ThrowIfNull(data);

			Type type = data.GetType();

			if (type == typeof(int))
			{
				return BitConverter.GetBytes((int)data);
			}

			if (type == typeof(double))
			{
				return BitConverter.GetBytes((double)data);
			}

			if (type == typeof(bool))
			{
				return BitConverter.GetBytes((bool)data);
			}

			if (type == typeof(char))
			{
				return BitConverter.GetBytes((char)data);
			}

			if (type == typeof(short))
			{
				return BitConverter.GetBytes((short)data);
			}

			if (type == typeof(long))
			{
				return BitConverter.GetBytes((long)data);
			}

			if (type == typeof(float))
			{
				return BitConverter.GetBytes((float)data);
			}

			if (type == typeof(ushort))
			{
				return BitConverter.GetBytes((ushort)data);
			}

			if (type == typeof(uint))
			{
				return BitConverter.GetBytes((uint)data);
			}

			if (type == typeof(ulong))
			{
				return BitConverter.GetBytes((ulong)data);
			}

			if (type == typeof(string))
			{
				byte[] bytes = Encoding.UTF8.GetBytes((string)data);
				byte[] serialized = new byte[bytes.Length + 1];
				bytes[0] = (byte)bytes.Length;
				Array.Copy(bytes, 0, serialized, 1, bytes.Length);

				return bytes;
			}

			if (type == typeof(IPacketSerializable))
			{
				return ((IPacketSerializable)data).Serialize();
			}

			throw new NotSupportedException($"Type {type} is not supported.");
		}
		
		private readonly byte[] serialized;
		private readonly MemoryStream stream;

		public PacketWriter(int maxDataSize)
		{
			this.serialized = new byte[maxDataSize];
			this.stream = new MemoryStream(this.serialized);
		}
		
		public void Write(object data)
		{
			if (!this.stream.CanWrite)
			{
				return;
			}

			this.stream.Write(Serialize(data));
		}

		public void Send(Socket sender) => sender.Send(this.serialized);

		public void Dispose()
		{
			this.stream.Dispose();
			
			GC.SuppressFinalize(this);
		}

		public async ValueTask DisposeAsync()
		{
			await this.stream.DisposeAsync();
			
			GC.SuppressFinalize(this);
		}
	}
}