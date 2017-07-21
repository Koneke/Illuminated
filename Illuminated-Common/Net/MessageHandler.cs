using System;
using System.Collections.Generic;
using System.Linq;

using Lidgren.Network;

namespace Illuminated.Net
{
	public class MessageHandler
	{
		public interface ISubHandler
		{
			string Key { get; }
			HandlerDelegate Handler { get; }
			MessageQueue MessageQueue { get; }
		}

		public class ReceivedMessage
		{
			public NetIncomingMessage IncomingMessage;
			public NetConnection Connection;
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
					MessageType = messageType,
					Message = Message.Create(messageType).Read(incoming)
				};

				return received;
			}
		}

		public delegate void HandlerDelegate(NetIncomingMessage incoming);

		public Dictionary<string, ISubHandler> Handlers =
			new Dictionary<string, ISubHandler>();

		private ClientCollection<IIllClient> clients;

		public MessageHandler(
			ClientCollection<IIllClient> clients
		) {
			this.clients = clients;
		}

		public MessageHandler AddSubHandler(ISubHandler handler)
		{
			this.Handlers.Add(handler.Key, handler);

			return this;
		}

		private static Message.MessageType readType(NetIncomingMessage incoming, Func<string> source) => (Message.MessageType)
			Enum.Parse(
				typeof(Message.MessageType),
				source());

		// Move to Message (static)?
		public static Message.MessageType PeekType(NetIncomingMessage incoming) =>
			readType(incoming, incoming.PeekString);

		public static Message.MessageType ReadType(NetIncomingMessage incoming) =>
			readType(incoming, incoming.ReadString);

		private void Pass(string key, NetIncomingMessage incoming)
		{
			this.Handlers[key].Handler(incoming);
		}

		public void Handle(NetIncomingMessage incoming)
		{
			switch (incoming.MessageType)
			{
				case NetIncomingMessageType.Data:
					switch (PeekType(incoming))
					{
						case Message.MessageType.Terminal:
							this.Pass("terminal", incoming);
							return;
								
						case Message.MessageType.RequestSafeConversation:
						case Message.MessageType.SafeHandshake:
						case Message.MessageType.SafeMessage:
							this.Pass("security", incoming);
							return;

						default:
							this.Pass("data", incoming);
							return;
					}

				case NetIncomingMessageType.StatusChanged:
					this.Pass("connection", incoming);
					break;

				default:
					Console.WriteLine($"Unknown message type: {incoming.MessageType}.");
					break;
			}
		}
	}
}
