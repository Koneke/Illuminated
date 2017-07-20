namespace Illuminated.Net
{
	public partial class Message
	{
		public enum MessageType
		{
			// generic
			Vector,

			// from server
			Spawn,
			OtherSpawn,
			OtherDisconnected,
			PlayerPosition,

				// security
				SafeHandshake,

			// from client
			Login,
			Run,

				// security
				RequestSafeConversation,
			
			// from both
				// security
				SafeMessage,

			// special
			Terminal,
		}

		public static readonly MessageType[] SecureTypes = {
			// from server
			MessageType.SafeHandshake,

			// from client
			MessageType.RequestSafeConversation,

			// from both
			MessageType.SafeMessage
		};
	}
}
