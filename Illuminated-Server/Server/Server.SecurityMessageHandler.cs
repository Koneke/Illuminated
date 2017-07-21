using System;
using System.Security.Cryptography;

using Lidgren.Network;

namespace Illuminated.Net
{
	public partial class Server
	{
		private class SecurityMessageHandler : MessageHandler.ISubHandler
		{
			public string Key => "security";
			public MessageHandler.HandlerDelegate Handler => this.Handle;
			public MessageQueue MessageQueue { get; private set; } = new MessageQueue();

			private ClientCollection<IIllClient> clients;

			private const int saltSize = 32;

			public SecurityMessageHandler(ClientCollection<IIllClient> clients)
			{
				this.clients = clients;
			}

			private byte[] generateSalt()
			{
				var salt = new byte[saltSize];

				using (var random = new RNGCryptoServiceProvider())
				{
					random.GetNonZeroBytes(salt);
				}

				return salt;
			}

			public void Handle(NetIncomingMessage incoming)
			{
				var mt = MessageHandler.ReadType(incoming);
				var received = MessageHandler.ReceivedMessage.Receive(mt, incoming);

				//NetIncomingMessage incoming = null;
				//var rec = MessageHandler.ReceivedMessage.Receive(, incoming);

				var client = this.clients[received.Connection] as Client;
				var message = received.Message;

				switch (received.MessageType)
				{
					case Message.MessageType.RequestSafeConversation:
						var salt = this.generateSalt();
						var b64 = System.Convert.ToBase64String(salt);

						this.MessageQueue.SendSingle(
							client.Recipient,
							Message.Create(Message.MessageType.SafeHandshake)
								.SetField("guid", Guid.NewGuid().ToString())
								.SetField("salt", b64));
						break;

					case Message.MessageType.SafeMessage:
						break;
				}
			}
		}
	}
}
