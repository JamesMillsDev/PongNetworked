namespace Pong
{
	public static class MemoryStreamExtensions
	{
		public static byte[] ReadBytes(this MemoryStream stream, int count)
		{
			byte[] buffer = new byte[count];
			stream.ReadExactly(buffer, 0, count);
			return buffer;
		}
	}
}