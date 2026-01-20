using System.Numerics;
using System.Runtime.InteropServices;
using Pong.Networking.Packets;

namespace Pong.Gameplay
{
	public class Transform : IPacketSerializable
	{
		public Vector2 Position { get; set; }
		public Vector2 Size { get; set; }
		
		public byte[] Serialize()
		{
			List<byte> bytes = [];
			if (bytes == null)
			{
				throw new ArgumentNullException(nameof(bytes));
			}

			bytes.AddRange(BitConverter.GetBytes(Position.X));
			bytes.AddRange(BitConverter.GetBytes(Position.Y));
			bytes.AddRange(BitConverter.GetBytes(Size.X));
			bytes.AddRange(BitConverter.GetBytes(Size.Y));

			return bytes.ToArray();
		}

		public void Deserialize(byte[] data)
		{
			using (MemoryStream stream = new(data))
			{
				Position = new Vector2
				{
					X = BitConverter.ToSingle(stream.ReadBytes(sizeof(float)), 0),
					Y = BitConverter.ToSingle(stream.ReadBytes(sizeof(float)), 0)
				};
			
				Size = new Vector2
				{
					X = BitConverter.ToSingle(stream.ReadBytes(sizeof(float)), 0),
					Y = BitConverter.ToSingle(stream.ReadBytes(sizeof(float)), 0)
				};
			}
		}

		public int GetSize() => Marshal.SizeOf<Vector2>() * 2;
	}
}