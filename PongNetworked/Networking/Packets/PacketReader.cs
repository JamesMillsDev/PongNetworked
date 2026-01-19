using System.Text;

namespace Pong.Networking.Packets
{
	public class PacketReader(byte[] buffer)
	{
		private readonly MemoryStream stream = new(buffer);

		public bool ReadBoolean()
		{
			return this.stream.CanRead
				? BitConverter.ToBoolean(ReadBytes(sizeof(bool)))
				: throw new Exception("Stream is not readable");
		}

		public char ReadChar()
		{
			return this.stream.CanRead
				? BitConverter.ToChar(ReadBytes(sizeof(char)))
				: throw new Exception("Stream is not readable");
		}

		public short ReadInt16()
		{
			return this.stream.CanRead
				? BitConverter.ToInt16(ReadBytes(sizeof(short)))
				: throw new Exception("Stream is not readable");
		}

		public int ReadInt32()
		{
			return this.stream.CanRead
				? BitConverter.ToInt32(ReadBytes(sizeof(int)))
				: throw new Exception("Stream is not readable");
		}

		public long ReadInt64()
		{
			return this.stream.CanRead
				? BitConverter.ToInt64(ReadBytes(sizeof(long)))
				: throw new Exception("Stream is not readable");
		}

		public float ReadFloat()
		{
			return this.stream.CanRead
				? BitConverter.ToSingle(ReadBytes(sizeof(float)))
				: throw new Exception("Stream is not readable");
		}

		public double ReadDouble()
		{
			return this.stream.CanRead
				? BitConverter.ToDouble(ReadBytes(sizeof(double)))
				: throw new Exception("Stream is not readable");
		}

		public ushort ReadUInt16()
		{
			return this.stream.CanRead
				? BitConverter.ToUInt16(ReadBytes(sizeof(ushort)))
				: throw new Exception("Stream is not readable");
		}

		public uint ReadUInt32()
		{
			return this.stream.CanRead
				? BitConverter.ToUInt32(ReadBytes(sizeof(uint)))
				: throw new Exception("Stream is not readable");
		}

		public ulong ReadULong()
		{
			return this.stream.CanRead
				? BitConverter.ToUInt64(ReadBytes(sizeof(ulong)))
				: throw new Exception("Stream is not readable");
		}

		public string ReadString()
		{
			if (!this.stream.CanRead)
			{
				throw new Exception("Stream is not readable");
			}
			
			int length = ReadInt32();
			
			if (length < 0)
			{
				throw new Exception("Invalid string length");
			}
			
			return length == 0 
				? string.Empty 
				: Encoding.UTF8.GetString(ReadBytes(length));
		}

		public T ReadPacketSerializable<T>() where T : IPacketSerializable, new()
		{
			if (!this.stream.CanRead)
			{
				throw new Exception("Stream is not readable");
			}
			
			T packetSerializable = new();
			packetSerializable.Deserialize(ReadBytes(packetSerializable.GetSize()));

			return packetSerializable;
		}

		private byte[] ReadBytes(int count)
		{
			byte[] buffer = new byte[count];
			this.stream.ReadExactly(buffer, 0, count);
			return buffer;
		}
	}
}