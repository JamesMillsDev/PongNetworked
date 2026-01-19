using System.Text;

namespace Pong.Networking.Packets
{
	public class PacketReader(byte[] buffer)
	{
		private readonly MemoryStream stream = new(buffer);

		public double ReadDouble()
		{
			return this.stream.CanRead
				? Convert.ToDouble(ReadBytes(sizeof(double)))
				: throw new Exception("Stream is not readable");
		}

		public bool ReadBoolean()
		{
			return this.stream.CanRead
				? Convert.ToBoolean(ReadBytes(sizeof(bool)))
				: throw new Exception("Stream is not readable");
		}

		public char ReadChar()
		{
			return this.stream.CanRead
				? Convert.ToChar(ReadBytes(sizeof(char)))
				: throw new Exception("Stream is not readable");
		}

		public short ReadInt16()
		{
			return this.stream.CanRead
				? Convert.ToInt16(ReadBytes(sizeof(short)))
				: throw new Exception("Stream is not readable");
		}

		public int ReadInt32()
		{
			return this.stream.CanRead
				? Convert.ToInt32(ReadBytes(sizeof(int)))
				: throw new Exception("Stream is not readable");
		}

		public long ReadInt64()
		{
			return this.stream.CanRead
				? Convert.ToInt64(ReadBytes(sizeof(long)))
				: throw new Exception("Stream is not readable");
		}

		public float ReadFloat()
		{
			return this.stream.CanRead
				? Convert.ToSingle(ReadBytes(sizeof(float)))
				: throw new Exception("Stream is not readable");
		}

		public ushort ReadUInt16()
		{
			return this.stream.CanRead
				? Convert.ToUInt16(ReadBytes(sizeof(ushort)))
				: throw new Exception("Stream is not readable");
		}

		public uint ReadUInt32()
		{
			return this.stream.CanRead
				? Convert.ToUInt32(ReadBytes(sizeof(uint)))
				: throw new Exception("Stream is not readable");
		}

		public ulong ReadULong()
		{
			return this.stream.CanRead
				? Convert.ToUInt64(ReadBytes(sizeof(ulong)))
				: throw new Exception("Stream is not readable");
		}

		public string ReadString()
		{
			int length = ReadInt32();
			
			return this.stream.CanRead
				? Encoding.UTF8.GetString(ReadBytes(sizeof(char) * length))
				: throw new Exception("Stream is not readable");
		}

		public T ReadPacketSerializable<T>() where T : IPacketSerializable, new()
		{
			T packetSerializable = new();
			packetSerializable.Deserialize(ReadBytes(packetSerializable.GetSize()));
			
			return this.stream.CanRead
				? packetSerializable
				: throw new Exception("Stream is not readable");
		}

		private byte[] ReadBytes(int count)
		{
			byte[] buffer = new byte[count];
			this.stream.ReadExactly(buffer, 0, count);
			return buffer;
		}
	}
}