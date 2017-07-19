using System;

using Microsoft.Xna.Framework;

namespace Illuminated
{
	public static class Vector2Extensions
	{
		public static Vector2 Round(this Vector2 v) =>
			new Vector2(
				(float)Math.Round(v.X),
				(float)Math.Round(v.Y));
	}
}
