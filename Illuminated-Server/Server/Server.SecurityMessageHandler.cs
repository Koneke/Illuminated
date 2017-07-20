using System;
using System.Security.Cryptography;

using Lidgren.Network;

namespace Illuminated.Net
{
	public partial class Server
	{
		private class SecurityMessageHandler
		{
			public MessageQueue MessageQueue =
				new MessageQueue();

			private ClientCollection clients;

			private const int saltSize = 32;

			public SecurityMessageHandler(ClientCollection clients)
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

			public void HandleSecurity(MessageHandler.ReceivedMessage received)
			{
				var mt = received.IncomingMessage.ReadString();

				//NetIncomingMessage incoming = null;
				//var rec = MessageHandler.ReceivedMessage.Receive(, incoming);

				var client = this.clients[received.Connection];
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
