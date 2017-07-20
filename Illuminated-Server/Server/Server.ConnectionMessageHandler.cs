using System;
using System.Linq;

using Lidgren.Network;

namespace Illuminated.Net
{
	public partial class Server
	{
		private class ConnectionMessageHandler
		{
			public MessageQueue MessageQueue = new MessageQueue();

			private Model model;
			private ClientCollection clients => model.Clients;

			public ConnectionMessageHandler(Model model)
			{
				this.model = model;
			}

			public void HandleConnection(NetIncomingMessage incoming)
			{
				Client client;

				switch (incoming.SenderConnection.Status)
				{
					case NetConnectionStatus.Connected:
						Console.WriteLine("New client.");

						client = this.model.Spawn(incoming.SenderConnection);

						lock (client.Player)
						{
							this.MessageQueue.SendExcept(
								client.Recipient,
								this.clients.All.Select(c => c.Recipient),
								Message.Create(Message.MessageType.OtherSpawn)
									.SetField("player-id", client.ID)
									.SetField("x", client.Player.Position.X)
									.SetField("y", client.Player.Position.Y));
						}

						break;

					case NetConnectionStatus.Disconnected:
						Console.WriteLine("Client left.");

						client = this.clients[incoming.SenderConnection];

						this.MessageQueue.SendExcept(
							client.Recipient,
							this.clients.All.Select(c => c.Recipient),
							Message.Create(Message.MessageType.OtherDisconnected)
								.SetField("player-id", client.ID));

						this.clients.RemoveClient(client);

						break;
				}
			}
		}
	}
}
