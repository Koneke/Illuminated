using System;
using System.Linq;

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
				// public Client Client;
				public Message.MessageType MessageType;
				public Message Message;

				public static ReceivedMessage Receive(
					Net.Message.MessageType messageType,
					NetIncomingMessage incoming
				) {
					var connection = incoming.SenderConnection;

					var received = new ReceivedMessage
					{
						IncomingMessage = incoming,
						Connection = connection,
						// Client = this.clients.ByConnection[connection],
						MessageType = messageType,
						Message = Message.Create(messageType).Read(incoming)
					};

					return received;
				}
			}

			public delegate void DataDelegate(NetIncomingMessage incoming);
			public DataDelegate DataHandler;

			public delegate void ConnectionDelegate(NetIncomingMessage incoming);
			public ConnectionDelegate ConnectionHandler;
			
			public delegate void TerminalDelegate(NetIncomingMessage incoming);
			public TerminalDelegate TerminalHandler;

			public delegate void SecurityDelegate(NetIncomingMessage incoming);
			public SecurityDelegate SecurityHandler;

			private ClientCollection clients;

			public MessageHandler(
				ClientCollection clients,
				DataDelegate dataHandler,
				ConnectionDelegate connectionHandler,
				TerminalDelegate terminalHandler,
				SecurityDelegate securityHandler
			) {
				this.clients = clients;

				this.DataHandler = dataHandler;
				this.ConnectionHandler = connectionHandler;
				this.TerminalHandler = terminalHandler;
				this.SecurityHandler = securityHandler;
			}

			private static Message.MessageType readType(NetIncomingMessage incoming, Func<string> source) => (Message.MessageType)
				Enum.Parse(
					typeof(Message.MessageType),
					source());

			public static Message.MessageType PeekType(NetIncomingMessage incoming) =>
				readType(incoming, incoming.PeekString);

			public static Message.MessageType ReadType(NetIncomingMessage incoming) =>
				readType(incoming, incoming.ReadString);

			public void Handle(NetIncomingMessage incoming)
			{
				switch (incoming.MessageType)
				{
					case NetIncomingMessageType.Data:
						switch (PeekType(incoming))
						{
							case Message.MessageType.Terminal:
								this.TerminalHandler(incoming);
								return;
								
							case Message.MessageType.RequestSafeConversation:
							case Message.MessageType.SafeHandshake:
							case Message.MessageType.SafeMessage:
								this.SecurityHandler(incoming);
								return;

							default:
								this.DataHandler(incoming);
								return;
						}

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
