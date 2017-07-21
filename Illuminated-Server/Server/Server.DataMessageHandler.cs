using System.Linq;

using Lidgren.Network;

using Microsoft.Xna.Framework;

namespace Illuminated.Net
{
	public partial class Server
	{
		private class DataMessageHandler : MessageHandler.ISubHandler
		{
			public string Key => "data";
			public MessageHandler.HandlerDelegate Handler => this.Handle;
			public MessageQueue MessageQueue { get; private set; } = new MessageQueue();

			private Model model;

			public DataMessageHandler(Model model)
			{
				this.model = model;
			}

			public void Handle(NetIncomingMessage incoming)
			{
				var mt = MessageHandler.ReadType(incoming);
				var received = MessageHandler.ReceivedMessage.Receive(mt, incoming);

				var client = this.model.Clients[received.Connection] as Client;
				var message = received.Message;

				switch (received.MessageType)
				{
					case Net.Message.MessageType.Run:
						client.Player.Position += new Vector2(
							message.GetField<float>("dx"),
							message.GetField<float>("dy"));

						// Update other clients.
						this.MessageQueue.SendExcept(
							client.Recipient,
							this.model.Clients.All.Select(c => (c as Client).Recipient),
							Message.Create(Message.MessageType.PlayerPosition)
								.SetField("player-id", client.ID)
								.SetField("x", client.Player.Position.X)
								.SetField("y", client.Player.Position.Y));

						break;
				}
			}
		}
	}
}
