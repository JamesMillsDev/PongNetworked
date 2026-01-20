namespace Pong.Gameplay
{
	public class Actor(string name)
	{
		public Transform Transform { get; set; } = new();
		public string Name { get; } = name;

		public virtual void Render()
		{
		}

		public virtual void Tick(float dt)
		{
		}
	}
}