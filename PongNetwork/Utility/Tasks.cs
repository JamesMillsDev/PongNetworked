namespace Pong.Utility
{
	public static class Tasks
	{
		public static async Task While(Func<bool> condition)
		{
			while (true)
			{
				if (!condition())
				{
					return;
				}
				
				await Task.Delay(100);
			}
		}
	}
}