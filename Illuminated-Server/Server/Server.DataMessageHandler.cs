using System.Linq;
using Microsoft.Xna.Framework;

namespace Illuminated.Net
{
	public partial class Server
	{
		private class DataMessageHandler
		{
			public MessageQueue MessageQueue;

			private Model model;

			public DataMessageHandler(Model model)
			{
				this.model = model;
				this.MessageQueue = new MessageQueue();
			}

			public void HandleData(MessageHandler.ReceivedMessage received)
			{
				var client = received.Client;
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
							this.model.Clients.All.Select(c => c.Recipient),
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
