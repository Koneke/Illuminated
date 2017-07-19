using Microsoft.Xna.Framework;

namespace Illuminated.Client
{
	public partial class Model
	{
		public class IllPlayer
		{
			public const float PlayerSpeed = 150.0f;

			public int ID;

			public virtual Vector2 Position { get; private set; }

			public CurveSystem Curves = new CurveSystem();

			public IllPlayer(int id)
			{
				this.ID = id;
			}

			public IllPlayer SetPosition(Vector2 position)
			{
				this.Position = position;

				return this;
			}
		}
	}
}
