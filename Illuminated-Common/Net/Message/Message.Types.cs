namespace Illuminated.Net
{
	public partial class Message
	{
		public class Types
		{
			// generic
			public const string Vector = "Vector";

			// from server
			public const string Spawn = "Spawn";
			public const string OtherSpawn = "OtherSpawn";
			public const string OtherDisconnected = "OtherDisconnected";
			public const string PlayerPosition = "PlayerPosition";

			// from client
			public const string Run = "Run";
		}

		public enum MessageType
		{
			Vector,
			Spawn,
			OtherSpawn,
			OtherDisconnected,
			PlayerPosition,
			Run
		}
	}
}
