
using Lidgren.Network;

namespace Illuminated.Net
{
	public class MessageRecipient
	{
		public static MessageRecipient Create(NetConnection c) =>
			new MessageRecipient(c);

		public NetConnection Connection;

		public MessageRecipient(NetConnection connection)
		{
			this.Connection = connection;
		}
	}
}
