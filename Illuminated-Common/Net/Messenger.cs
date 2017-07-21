using System;
using System.Collections.Generic;

using Lidgren.Network;

namespace Illuminated.Net
{
	public class Messenger
	{
		private NetPeer peer;
		private List<MessageQueue> pollList =
			new List<MessageQueue>();

		private List<SafeConversation> safeConversations =
			new List<SafeConversation>();
		private List<SafeConversation> pendingConversations =
			new List<SafeConversation>();

		public MessageHandler MessageHandler;

		public Messenger(NetPeer peer, ClientCollection<IIllClient> clients)
		{
			this.peer = peer;
			this.MessageHandler = new MessageHandler(clients);
		}

		public Messenger AddMessageHandler(MessageHandler.ISubHandler handler)
		{
			this.MessageHandler.AddSubHandler(handler);
			this.Poll(handler.MessageQueue);

			return this;
		}

		public Messenger SendSingle(
			MessageRecipient recipient,
			Message message
		) {
			Console.WriteLine($"-> {message.Type}");

			var outgoing = this.peer.CreateMessage();
			message.Write(outgoing);

			if (message.IsSecure)
			{
				var conversation = new SafeConversation(this, recipient);
			}
			else
			{
				this.peer.SendMessage(
					outgoing,
					recipient.Connection,
					NetDeliveryMethod.ReliableOrdered);
			}

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

		public Messenger SendExcept(
			MessageRecipient except,
			IEnumerable<MessageRecipient> recipients,
			Message message
		) {
			this.SendMany(recipients.Exclude(except), message);

			return this;
		}

		public Messenger ConsumeQueue(MessageQueue queue)
		{
			foreach (var message in queue.Queue)
			{
				this.SendSingle(message.r, message.m);
			}

			queue.Queue.Clear();

			return this;
		}
		
		public Messenger Poll(MessageQueue queue)
		{
			this.pollList.Add(queue);

			return this;
		}

		public Messenger Flush()
		{
			foreach (var queue in this.pollList)
			{
				this.ConsumeQueue(queue);
			}

			return this;
		}
	}
}
