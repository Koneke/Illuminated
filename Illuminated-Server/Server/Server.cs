using System;

using Lidgren.Network;
using Microsoft.Xna.Framework;

using Illuminated.Common;

namespace Illuminated.Net
{
	public partial class Server
	{
		private bool shutdown;
		private NetServer server;

		private Messenger messenger;

		private Model model;

		public Server()
		{
			this.server = new NetServer(
				new NetPeerConfiguration("Illuminated a-0.1") {
					Port = 3333
				});

			this.messenger = new Messenger(this.server);

			this.Run();
		}

		public void SendMessage(Client client, Message message)
		{
			var outgoing = this.server.CreateMessage();
			message.Write(outgoing);

			this.server.SendMessage(
				outgoing,
				client.Connection,
				NetDeliveryMethod.ReliableOrdered);
		}

		public void SendEveryoneMessage(Client excludedClient, Message message)
		{
			foreach (var client in this.model.Clients)
			{
				var outgoing = this.server.CreateMessage();
				message.Write(outgoing);

				this.SendMessage(client, message);
			}
		}

		public void SendInvertedMessage(Client excludedClient, Message message)
		{
			foreach (var client in this.model.Clients)
			{
				if (client == excludedClient)
				{
					continue;
				}

				var outgoing = this.server.CreateMessage();
				message.Write(outgoing);

				this.SendMessage(client, message);
			}
		}

		private void Run()
		{
			this.model = new Model();

			this.server.Start();

			Async.Do(() => {
				Console.Read();
				Console.WriteLine("Shutting down.");
				shutdown = true;
			});

			while (!shutdown)
			{
				this.Receive();
			}
		}

		private Client Spawn(NetConnection connection)
		{
			var player = new Player {
				Position = new Vector2(100.0f, 100.0f)
			};

			var client = new Client(
				this,
				connection,
				player);

			this.model.AddClient(client.Connection, client);

			this.messenger.SendSingle(
				new MessageRecipient(client.Connection),
				Message.Create(Message.MessageType.Spawn)
					.SetField("player-id", client.ID)
					.SetField("x", player.Position.X)
					.SetField("y", player.Position.Y));

			client.CatchUp();

			return client;
		}

		private Client ClientFromMessage(NetIncomingMessage message) =>
			this.model.ClientsByConnection[message.SenderConnection];

		private void Receive()
		{
			NetIncomingMessage incoming;
			Client client;

			while ((incoming = server.ReadMessage()) != null)
			{
				switch (incoming.MessageType)
				{
					case NetIncomingMessageType.Data:
						var messageType = (Net.Message.MessageType) Enum.Parse(
							typeof(Message.MessageType),
							incoming.ReadString());

						var message = Message
							.Create(messageType)
							.Read(incoming);

						client = this.ClientFromMessage(incoming);

						switch (message.Type)
						{
							case Net.Message.MessageType.Run:
								client.Player.Position += new Vector2(
									message.GetField<float>("dx"),
									message.GetField<float>("dy"));

								this.SendInvertedMessage(
									client,
									Message.Create(Message.MessageType.PlayerPosition)
										.SetField("player-id", client.ID)
										.SetField("x", client.Player.Position.X)
										.SetField("y", client.Player.Position.Y));

								break;
						}
						break;

					case NetIncomingMessageType.StatusChanged:
						switch (incoming.SenderConnection.Status)
						{
							case NetConnectionStatus.Connected:
								Console.WriteLine("New client.");

								client = this.Spawn(incoming.SenderConnection);

								this.SendInvertedMessage(
									client,
									Message.Create(Message.MessageType.OtherSpawn)
										.SetField("player-id", client.ID)
										.SetField("x", client.Player.Position.X)
										.SetField("y", client.Player.Position.Y));

								break;

							case NetConnectionStatus.Disconnected:
								Console.WriteLine("Client left.");

								client = this.model.ClientsByConnection
									[incoming.SenderConnection];

								this.SendInvertedMessage(
									client,
									Message.Create(Message.MessageType.OtherDisconnected)
										.SetField("player-id", client.ID));

								this.model.DisconnectClient(client);

								break;
						}
						break;

					default:
						Console.WriteLine($"Unknown message type: {incoming.MessageType}.");
						break;
				}
			}
		}
	}
}
