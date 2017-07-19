using Microsoft.Xna.Framework;

namespace Illuminated.Client
{
	public partial class Model
	{
		public class IllLocalPlayer : IllPlayer
		{
			public Vector2 UncommittedMovement = Vector2.Zero;

			public override Vector2 Position => base.Position + this.UncommittedMovement;

			public IllLocalPlayer(int id)
				: base(id)
			{
			}

			public void MoveLocal(Vector2 delta)
			{
				this.UncommittedMovement += delta * PlayerSpeed;
			}

			public void CommitMovement(Vector2 committedDelta)
			{
				this.UncommittedMovement = this.UncommittedMovement - committedDelta;
				this.SetPosition(base.Position + committedDelta);
				// this.UncommittedMovement = Vector2.Zero;
			}
		}
	}
}
