using System;
using Microsoft.Xna.Framework;

namespace Illuminated.Net
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("-- Starting Illuminated (Server) a-0.1 --");
			var server = new Server();
		}
	}

	public class Player
	{
		public Vector2 Position;
	}
}
