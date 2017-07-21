using System;

namespace Illuminated.Net
{
	public class SafeConversation
	{
		public Guid Guid;

		public bool Open;
		public bool Pending => !this.hasSalt;

		private Messenger messenger;
		private MessageRecipient recipient;
		private bool hasSalt;
		private byte[] salt;

		public SafeConversation(
			Messenger messenger,
			MessageRecipient recipient
		) {
			this.Open = true;
			this.messenger = messenger;
			this.recipient = recipient;
		}

		public SafeConversation SendSingle(Message message)
		{
			if (!this.hasSalt)
			{
				this.messenger.SendSingle(
					this.recipient,
					Message.Create(Message.MessageType.RequestSafeConversation));
			}
			else
			{
				this.messenger.SendSingle(
					this.recipient,
					Message.Create(Message.MessageType.SafeMessage)
						.SetField("guid", this.Guid.ToString())
						.SetField("data", ""));
			}

			return this;
		}

		public void Close()
		{
			this.Open = false;
		}
	}
}
