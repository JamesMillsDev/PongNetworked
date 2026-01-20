using System.Net.Sockets;
using Pong.Networking.Packets;

namespace Pong.Networking
{
	public class NetworkClient(string endpoint, int port) : Network(endpoint, port)
	{
		private bool connected;

		protected override async Task Poll()
		{
			if (!this.connected)
			{
				return;
			}

			if (this.socket == null)
			{
				throw new NullReferenceException("Socket not connected somehow!");
			}

			Task<Tuple<string, byte[]>> reading = ReadPacket(this.socket);
			await reading;
			
			string id = reading.Result.Item1;
			
			if (TryMakePacketFor(id, out Packet? packet))
			{
				byte[] payload = reading.Result.Item2;
				PacketReader reader = new(payload);
				
				packet!.Deserialize(reader);
				await packet.Process();
			}
			
			await Task.Delay(this.pollRate);
		}

		protected override void Open(int backlog = 10)
		{
			try
			{
				this.socket = new Socket(this.ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				
				_ = Task.Run(async () =>
				{
					try
					{
						Task<bool> task = AwaitServerConnection();
						await task;
					
						this.connected = task.Result;
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
						throw;
					}
				});
			}
			catch (ArgumentNullException ane)
			{
				Console.WriteLine("ArgumentNullException : {0}", ane);
				throw;
			}
			catch (SocketException se)
			{
				Console.WriteLine("SocketException : {0}", se);
				throw;
			}
			catch (Exception e)
			{
				Console.WriteLine("Unexpected exception : {0}", e);
				throw;
			}
		}

		private async Task<bool> AwaitServerConnection()
		{
			await Tasks.While(() => this.socket == null);

			Task connectionTask = this.socket!.ConnectAsync(this.localEndPoint);
			Task completedTask = await Task.WhenAny(connectionTask, Task.Delay(5000));
			
			return completedTask == connectionTask;
		}
	}
}