using System;
using System.Linq;

using Lidgren.Network;

namespace Illuminated.Net
{
	public partial class Server
	{
		private class ConnectionMessageHandler : MessageHandler.ISubHandler
		{
			public string Key => "connection";
			public MessageHandler.HandlerDelegate Handler => this.Handle;
			public MessageQueue MessageQueue { get; private set; } = new MessageQueue();

			private Model model;

			public ConnectionMessageHandler(Model model)
			{
				this.model = model;
			}

			public void Handle(NetIncomingMessage incoming)
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
								this.model.Clients.All.Select(c => (c as Client).Recipient),
								Message.Create(Message.MessageType.OtherSpawn)
									.SetField("player-id", client.ID)
									.SetField("x", client.Player.Position.X)
									.SetField("y", client.Player.Position.Y));
						}

						break;

					case NetConnectionStatus.Disconnected:
						Console.WriteLine("Client left.");

						client = this.model.Clients[incoming.SenderConnection] as Client;

						this.MessageQueue.SendExcept(
							client.Recipient,
							this.model.Clients.All.Select(c => (c as Client).Recipient),
							Message.Create(Message.MessageType.OtherDisconnected)
								.SetField("player-id", client.ID));

						this.model.Clients.RemoveClient(client);

						break;
				}
			}
		}
	}
}
