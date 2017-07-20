using System;

using Lidgren.Network;

namespace Illuminated.Net
{
	public partial class Server
	{
		private class MessageHandler
		{
			public class ReceivedMessage
			{
				public NetIncomingMessage IncomingMessage;
				public NetConnection Connection;
				public Client Client;
				public Message.MessageType MessageType;
				public Message Message;
			}

			public delegate void DataDelegate(ReceivedMessage message);
			public DataDelegate DataHandler;

			public delegate void ConnectionDelegate(NetIncomingMessage incoming);
			public ConnectionDelegate ConnectionHandler;

			private ClientCollection clients;

			public MessageHandler(
				ClientCollection clients,
				DataDelegate dataHandler,
				ConnectionDelegate connectionHandler
			) {
				this.clients = clients;

				this.DataHandler = dataHandler;
				this.ConnectionHandler = connectionHandler;
			}

			public void Handle(NetIncomingMessage incoming)
			{
				switch (incoming.MessageType)
				{
					case NetIncomingMessageType.Data:
						var messageType = (Message.MessageType) Enum.Parse(
							typeof(Message.MessageType),
							incoming.ReadString());

						var connection = incoming.SenderConnection;

						var received = new ReceivedMessage
						{
							IncomingMessage = incoming,
							Connection = connection,
							Client = this.clients.ByConnection[connection],
							MessageType = messageType,
							Message = Message.Create(messageType).Read(incoming)
						};

						this.DataHandler(received);
						break;

					case NetIncomingMessageType.StatusChanged:
						this.ConnectionHandler(incoming);
						break;

					default:
						Console.WriteLine($"Unknown message type: {incoming.MessageType}.");
						break;
				}
			}
		}
	}
}
