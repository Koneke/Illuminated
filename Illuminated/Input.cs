using Microsoft.Xna.Framework.Input;

namespace Illuminated.Client
{
	public class Input
	{
		private KeyboardState oks;
		private KeyboardState ks;

		public bool Pressed(Keys key) => oks.IsKeyUp(key) && ks.IsKeyDown(key);
		public bool Released(Keys key) => oks.IsKeyDown(key) && ks.IsKeyUp(key);
		public bool Down(Keys key) => ks.IsKeyDown(key);
		public bool Up(Keys key) => ks.IsKeyUp(key);

		public void Update()
		{
			oks = ks;
			ks = Keyboard.GetState();
		}
	}
}
