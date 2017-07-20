using System.Collections.Generic;
using System.Linq;

namespace Illuminated.Net
{
	public class MessageQueue
	{
		public Queue<(MessageRecipient r, Message m)> Queue { get; private set; }

		public MessageQueue()
		{
			this.Queue = new Queue<(MessageRecipient r, Message m)>();
		}

		public MessageQueue SendSingle(
			MessageRecipient recipient,
			Message message
		) {
			this.Queue.Enqueue((r: recipient, m: message));

			return this;
		}

		public MessageQueue SendMany(
			List<MessageRecipient> recipients,
			Message message
		) {
			recipients.Select(r => this.SendSingle(r, message));

			return this;
		}

		public MessageQueue SendExcept(
			MessageRecipient excluded,
			List<MessageRecipient> recipients,
			Message message
		) {
			recipients.Exclude(excluded).Select(r => this.SendSingle(r, message));

			return this;
		}

		public MessageQueue SendExcept(
			MessageRecipient excluded,
			IEnumerable<MessageRecipient> recipients,
			Message message
		) {
			recipients.Exclude(excluded).Select(r => this.SendSingle(r, message));

			return this;
		}
	}
}
