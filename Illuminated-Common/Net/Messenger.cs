using System.Collections.Generic;

using Lidgren.Network;

namespace Illuminated.Net
{
	public class MessageRecipient
	{
		public NetConnection Connection;

		public MessageRecipient(NetConnection connection)
		{
			this.Connection = connection;
		}
	}

	public class Messenger
	{
		private NetPeer peer;

		public Messenger(NetPeer peer)
		{
			this.peer = peer;
		}

		public Messenger SendSingle(
			MessageRecipient recipient,
			Message message
		) {
			var outgoing = this.peer.CreateMessage();
			message.Write(outgoing);

			this.peer.SendMessage(
				outgoing,
				recipient.Connection,
				NetDeliveryMethod.ReliableOrdered);

			return this;
		}

		public Messenger SendMany(
			IEnumerable<MessageRecipient> recipients,
			Message message
		) {
			foreach (var recipient in recipients)
			{
				this.SendSingle(recipient, message);
			}

			return this;
		}
	}
}
